/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing;

namespace VDS.RDF;

/// <summary>
/// Abstract Base Class for a Triple Store.
/// </summary>
public abstract class BaseTripleStore 
    : ITripleStore
{
    /// <summary>
    /// Collection of Graphs that comprise the Triple Store.
    /// </summary>
    protected BaseGraphCollection _graphs;

    /// <summary>
    /// Event Handler definitions.
    /// </summary>
    private GraphEventHandler GraphAddedHandler, GraphRemovedHandler, GraphChangedHandler, GraphMergedHandler, GraphClearedHandler;

    /// <summary>
    /// Creates a new Base Triple Store.
    /// </summary>
    /// <param name="graphCollection">Graph Collection to use.</param>
    protected BaseTripleStore(BaseGraphCollection graphCollection)
    {
        _graphs = graphCollection ?? throw new ArgumentNullException(nameof(graphCollection), "Graph Collection must be an non-null instance of a class which derives from BaseGraphCollection");

        GraphAddedHandler = OnGraphAdded;
        GraphRemovedHandler = OnGraphRemoved;
        GraphChangedHandler = OnGraphChanged;
        GraphMergedHandler = OnGraphMerged;
        GraphClearedHandler = OnGraphCleared;

        // Attach Handlers to the Graph Collection
        _graphs.GraphAdded += GraphAddedHandler;
        _graphs.GraphRemoved += GraphRemovedHandler;
    }

    #region Properties

    /// <summary>
    /// Gets whether the Triple Store is empty.
    /// </summary>
    public virtual bool IsEmpty
    {
        get
        {
            // Empty if there are no Graphs in the Store
            return (_graphs.Count == 0);
        }
    }

    /// <summary>
    /// Gets the Collection of Graphs that comprise this Triple Store.
    /// </summary>
    public BaseGraphCollection Graphs
    {
        get
        {
            return _graphs;
        }
    }

    /// <summary>
    /// Gets all the Triples in the Triple Store.
    /// </summary>
    public IEnumerable<Triple> Triples
    {
        get
        {
            return (from g in _graphs
                    from t in g.Triples
                    select t);
        }
    }

    /// <inheritdoc />
    public IEnumerable<Quad> Quads
    {
        get
        {
            return from g in _graphs
                from t in g.Triples
                select new Quad(t, g.Name);
        }
    }

    /// <summary>
    /// Get the preferred URI factory to use when creating URIs in this store.
    /// </summary>
    public abstract IUriFactory UriFactory { get; }
    #endregion

    #region Assert/Retract Quads

    /// <inheritdoc />
    public void Assert(Quad quad)
    {
        if (!HasGraph(quad.Graph))
        {
            Add(quad.Graph);
        }
        Graphs[quad.Graph].Assert(quad.AsTriple());
    }

    /// <inheritdoc />
    public void Retract(Quad quad)
    {
        if (HasGraph(quad.Graph))
        {
            Graphs[quad.Graph].Retract(quad.AsTriple());
        }
    }
    #endregion

    #region Loading & Unloading

    /// <summary>
    /// Add a new empty graph with the specified name to the triple store
    /// </summary>
    /// <param name="graphName">The name of the graph to add.</param>
    /// <returns>True if the execution resulted in a new graph being added to the triplestore, false otherwise.</returns>
    public virtual bool Add(IRefNode graphName)
    {
        return !HasGraph(graphName) && Graphs.Add(new Graph(graphName), false);
    }

    /// <summary>
    /// Adds a Graph into the Triple Store.
    /// </summary>
    /// <param name="g">Graph to add.</param>
    public virtual bool Add(IGraph g)
    {
        return _graphs.Add(g, false);
    }

    /// <summary>
    /// Adds a Graph into the Triple Store using the chosen Merging Behaviour.
    /// </summary>
    /// <param name="g">Graph to add.</param>
    /// <param name="mergeIfExists">Whether the Graph should be merged with an existing Graph with the same Base Uri.</param>
    public virtual bool Add(IGraph g, bool mergeIfExists)
    {
        return _graphs.Add(g, mergeIfExists);
    }

    /// <summary>
    /// Adds a Graph into the Triple Store which is retrieved from the given Uri.
    /// </summary>
    /// <param name="graphUri">Uri of the Graph to load.</param>
    public virtual bool AddFromUri(Uri graphUri)
    {
        return AddFromUri(graphUri, false);
    }

    /// <summary>
    /// Adds a Graph into the Triple Store which is retrieved from the given Uri using the chosen Merging Behaviour.
    /// </summary>
    /// <param name="graphUri">Graph to add.</param>
    /// <param name="mergeIfExists">Whether the Graph should be merged with an existing Graph with the same Base Uri.</param>
    public virtual bool AddFromUri(Uri graphUri, bool mergeIfExists)
    {
        return AddFromUri(graphUri, mergeIfExists, new Loader());
    }

    /// <summary>
    /// Adds a Graph into the Triple Store which is retrieved from the given Uri using the chosen Merging Behaviour.
    /// </summary>
    /// <param name="graphUri">Graph to add.</param>
    /// <param name="mergeIfExists">Whether the Graph should be merged with an existing Graph with the same Base Uri.</param>
    /// <param name="loader">The loader to use to retrieve and parse remote data.</param>
    public virtual bool AddFromUri(Uri graphUri, bool mergeIfExists, Loader loader)
    {
        if (loader == null) throw new ArgumentNullException(nameof(loader));
        var g = new Graph();
        loader.LoadGraph(g, graphUri);
        return _graphs.Add(g, mergeIfExists);
    }

    /// <summary>
    /// Removes a Graph from the Triple Store.
    /// </summary>
    /// <param name="graphUri">Uri of the Graph to Remove.</param>
    [Obsolete("Replaced by Remove(IRefNode)")]
    public virtual bool Remove(Uri graphUri)
    {
        return _graphs.Remove(graphUri == null ? null : new UriNode(graphUri));
    }


    /// <summary>
    /// Removes a graph from the triple store.
    /// </summary>
    /// <param name="graphName">The name of the graph to remove.</param>
    /// <returns>True if the operation removed a graph, false if no matching graph was found to remove.</returns>
    public virtual bool Remove(IRefNode graphName)
    {
        return _graphs.Remove(graphName);
    }
    #endregion

    #region Graph Retrieval

    /// <summary>
    /// Checks whether a Graph with the given Base Uri exists in the Triple Store.
    /// </summary>
    /// <param name="graphUri">Graph Uri.</param>
    /// <returns>True if the Graph exists in the Triple Store.</returns>
    [Obsolete("Replaced by HasGraph(IRefNode)")]
    public bool HasGraph(Uri graphUri)
    {
        return _graphs.Contains(graphUri == null ? null : new UriNode(graphUri));
    }

    /// <summary>
    /// Checks whether the graph with the given name is in this triple store.
    /// </summary>
    /// <param name="graphName">The name of the graph to check for.</param>
    /// <returns>True if this store contains a graph with the specified name, false otherwise.</returns>
    /// <remarks>Pass null for<paramref name="graphName"/> to check for the default (unnamed) graph.</remarks>
    public bool HasGraph(IRefNode? graphName)
    {
        return _graphs.Contains(graphName);
    }

    /// <summary>
    /// Gets the Graph with the given URI.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    [Obsolete("Replaced by this[IRefNode]")]
    public IGraph this[Uri? graphUri]
    {
        get
        {
            return _graphs[graphUri == null ? null : new UriNode(graphUri)];
        }
    }

    /// <summary>
    /// Gets a graph from the triple store.
    /// </summary>
    /// <param name="graphName">The name of the graph to be retrieved. May be null to retrieve the default (unnamed) graph.</param>
    /// <returns></returns>
    public IGraph this[IRefNode? graphName] => _graphs[graphName];

    #endregion

    #region Events

    /// <inheritdoc />
    public IEnumerable<Quad> GetQuads(INode? s = null, INode? p = null, INode? o = null, IRefNode? g = null, bool allGraphs = true)
    {
        if (g == null && allGraphs)
        {
            return Graphs.SelectMany(g => GetTriplesFromGraph(g, s, p, o).Select(t => new Quad(t, g.Name)));
        }
        return !HasGraph(g) ? Enumerable.Empty<Quad>() : GetTriplesFromGraph(Graphs[g], s, p, o).Select(t => new Quad(t, g));
    }

    /// <summary>
    /// Returns an enumeration of the Triples in the specified graph that match the specified node filters.
    /// </summary>
    /// <param name="graph">The graph to return triples from.</param>
    /// <param name="s">The subject node to filter on, or null to match all subject nodes.</param>
    /// <param name="p">The predicate node to filter on, or null to match all predicate nodes.</param>
    /// <param name="o">The object node to filter on, or null to match all object nodes.</param>
    /// <returns>An enumeration of the triples that match the specified filter.</returns>
    protected virtual IEnumerable<Triple> GetTriplesFromGraph(IGraph graph, INode? s, INode? p, INode? o)
    {
        if (s == null)
        {
            if (p == null)
            {
                return o == null ? graph.Triples : graph.GetTriplesWithObject(o);
            }
            return o == null ? graph.GetTriplesWithPredicate(p) : graph.GetTriplesWithPredicateObject(p, o);
        }

        if (p == null)
        {
            return o == null ? graph.GetTriplesWithSubject(s) : graph.GetTriplesWithSubjectObject(s, o);
        }

        if (o == null)
        {
            return graph.GetTriplesWithSubjectPredicate(s, p);
        }

        var t = new Triple(s, p, o);
        return graph.ContainsTriple(t) ? t.AsEnumerable() : Enumerable.Empty<Triple>();
    }

    /// <summary>
    /// Event which is raised when a Graph is added
    /// </summary>
    public event TripleStoreEventHandler GraphAdded;

    /// <summary>
    /// Event which is raised when a Graph is removed
    /// </summary>
    public event TripleStoreEventHandler GraphRemoved;

    /// <summary>
    /// Event which is raised when a Graphs contents changes
    /// </summary>
    public event TripleStoreEventHandler GraphChanged;

    /// <summary>
    /// Event which is raised when a Graph is cleared
    /// </summary>
    public event TripleStoreEventHandler GraphCleared;

    /// <summary>
    /// Event which is raised when a Graph has a merge operation performed on it
    /// </summary>
    public event TripleStoreEventHandler GraphMerged;

    /// <summary>
    /// Helper method for raising the <see cref="GraphAdded">Graph Added</see> event manually.
    /// </summary>
    /// <param name="g">Graph.</param>
    protected void RaiseGraphAdded(IGraph g)
    {
        TripleStoreEventHandler d = GraphAdded;
        if (d != null)
        {
            d(this, new TripleStoreEventArgs(this, g));
        }
        AttachEventHandlers(g);
    }

    /// <summary>
    /// Helper method for raising the <see cref="GraphAdded">Graph Added</see> event manually.
    /// </summary>
    /// <param name="args">Graph Event Arguments.</param>
    protected void RaiseGraphAdded(GraphEventArgs args)
    {
        TripleStoreEventHandler d = GraphAdded;
        if (d != null)
        {
            d(this, new TripleStoreEventArgs(this, args));
        }
        AttachEventHandlers(args.Graph);
    }

    /// <summary>
    /// Event Handler which handles the <see cref="BaseGraphCollection.GraphAdded">Graph Added</see> event from the underlying Graph Collection and raises the Triple Store's <see cref="GraphAdded">Graph Added</see> event.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Graph Event Arguments.</param>
    /// <remarks>Override this method if your Triple Store implementation wishes to take additional actions when a Graph is added to the Store.</remarks>
    protected virtual void OnGraphAdded(object sender, GraphEventArgs args)
    {
        RaiseGraphAdded(args);
    }

    /// <summary>
    /// Helper method for raising the <see cref="GraphRemoved">Graph Removed</see> event manually.
    /// </summary>
    /// <param name="g">Graph.</param>
    protected void RaiseGraphRemoved(IGraph g)
    {
        TripleStoreEventHandler d = GraphRemoved;
        if (d != null)
        {
            d(this, new TripleStoreEventArgs(this, g));
        }
        DetachEventHandlers(g);
    }

    /// <summary>
    /// Helper method for raising the <see cref="GraphRemoved">Graph Removed</see> event manually.
    /// </summary>
    /// <param name="args">Graph Event Arguments.</param>
    protected void RaiseGraphRemoved(GraphEventArgs args)
    {
        TripleStoreEventHandler d = GraphRemoved;
        if (d != null)
        {
            d(this, new TripleStoreEventArgs(this, args));
        }
        DetachEventHandlers(args.Graph);
    }

    /// <summary>
    /// Event Handler which handles the <see cref="BaseGraphCollection.GraphRemoved">Graph Removed</see> event from the underlying Graph Collection and raises the Triple Stores's <see cref="GraphRemoved">Graph Removed</see> event.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Graph Event Arguments.</param>
    protected virtual void OnGraphRemoved(object sender, GraphEventArgs args)
    {
        RaiseGraphRemoved(args);
    }

    /// <summary>
    /// Helper method for raising the <see cref="GraphChanged">Graph Changed</see> event manually.
    /// </summary>
    /// <param name="args">Graph Event Arguments.</param>
    protected void RaiseGraphChanged(GraphEventArgs args)
    {
        TripleStoreEventHandler d = GraphChanged;
        if (d != null)
        {
            d(this, new TripleStoreEventArgs(this, args));
        }
    }

    /// <summary>
    /// Event Handler which handles the <see cref="IGraph.Changed">Changed</see> event of the contained Graphs by raising the Triple Store's <see cref="GraphChanged">Graph Changed</see> event.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Graph Event Arguments.</param>
    protected virtual void OnGraphChanged(object sender, GraphEventArgs args)
    {
        RaiseGraphChanged(args);
    }

    /// <summary>
    /// Helper method for raising the <see cref="GraphChanged">Graph Changed</see> event manually.
    /// </summary>
    /// <param name="g">Graph.</param>
    protected void RaiseGraphChanged(IGraph g)
    {
        TripleStoreEventHandler d = GraphChanged;
        if (d != null)
        {
            d(this, new TripleStoreEventArgs(this, g));
        }
    }

    /// <summary>
    /// Helper method for raising the <see cref="GraphCleared">Graph Cleared</see> event manually.
    /// </summary>
    /// <param name="args">Graph Event Arguments.</param>
    protected void RaiseGraphCleared(GraphEventArgs args)
    {
        TripleStoreEventHandler d = GraphCleared;
        if (d != null)
        {
            d(this, new TripleStoreEventArgs(this, args));
        }
    }

    /// <summary>
    /// Event Handler which handles the <see cref="IGraph.Cleared">Cleared</see> event of the contained Graphs by raising the Triple Stores's <see cref="GraphCleared">Graph Cleared</see> event.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Graph Event Arguments.</param>
    protected virtual void OnGraphCleared(object sender, GraphEventArgs args)
    {
        RaiseGraphCleared(args);
    }

    /// <summary>
    /// Helper method for raising the <see cref="GraphMerged">Graph Merged</see> event manually.
    /// </summary>
    /// <param name="args">Graph Event Arguments.</param>
    protected void RaiseGraphMerged(GraphEventArgs args)
    {
        TripleStoreEventHandler d = GraphMerged;
        if (d != null)
        {
            d(this, new TripleStoreEventArgs(this, args));
        }
    }

    /// <summary>
    /// Event Handler which handles the <see cref="IGraph.Merged">Merged</see> event of the contained Graphs by raising the Triple Store's <see cref="GraphMerged">Graph Merged</see> event.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Graph Event Arguments.</param>
    protected virtual void OnGraphMerged(object sender, GraphEventArgs args)
    {
        RaiseGraphMerged(args);
    }

    /// <summary>
    /// Helper method which attaches the Triple Store's Event Handlers to the relevant events of a Graph.
    /// </summary>
    /// <param name="g">Graph.</param>
    protected void AttachEventHandlers(IGraph g)
    {
        g.Changed += GraphChangedHandler;
        g.Cleared += GraphClearedHandler;
        g.Merged += GraphMergedHandler;
    }

    /// <summary>
    /// Helper method which detaches the Triple Store's Event Handlers from the relevant events of a Graph.
    /// </summary>
    /// <param name="g"></param>
    protected void DetachEventHandlers(IGraph g)
    {
        g.Changed -= GraphChangedHandler;
        g.Cleared -= GraphClearedHandler;
        g.Merged -= GraphMergedHandler;
    }

    #endregion

    /// <summary>
    /// Disposes of the Triple Store.
    /// </summary>
    /// <remarks>Derived classes must override this to implement required disposal actions.</remarks>
    public abstract void Dispose();
}
