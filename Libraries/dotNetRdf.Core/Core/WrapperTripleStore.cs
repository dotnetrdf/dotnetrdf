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
using VDS.RDF.Parsing;

namespace VDS.RDF;

/// <summary>
/// Abstract decorator for Triple Stores to make it easier to add new functionality on top of existing implementations.
/// </summary>
public class WrapperTripleStore
    : ITripleStore
{
    /// <summary>
    /// Underlying store.
    /// </summary>
    protected readonly ITripleStore _store;

    /// <summary>
    /// Event Handler definitions.
    /// </summary>
    private TripleStoreEventHandler GraphAddedHandler, GraphRemovedHandler, GraphChangedHandler, GraphMergedHandler, GraphClearedHandler;

    /// <summary>
    /// Creates a new triple store decorator around the given <see cref="ITripleStore"/> instance.
    /// </summary>
    /// <param name="store">Triple Store.</param>
    public WrapperTripleStore(ITripleStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));

        GraphAddedHandler = OnGraphAdded;
        GraphRemovedHandler = OnGraphRemoved;
        GraphChangedHandler = OnGraphChanged;
        GraphMergedHandler = OnGraphMerged;
        GraphClearedHandler = OnGraphCleared;

        _store.GraphAdded += GraphAddedHandler;
        _store.GraphChanged += GraphChangedHandler;
        _store.GraphCleared += GraphClearedHandler;
        _store.GraphMerged += GraphMergedHandler;
        _store.GraphRemoved += GraphRemovedHandler;
    }

    /// <summary>
    /// Gets whether the store is empty.
    /// </summary>
    public virtual bool IsEmpty
    {
        get
        {
            return _store.IsEmpty;
        }
    }

    /// <summary>
    /// Gets the Graphs of the store.
    /// </summary>
    public virtual BaseGraphCollection Graphs
    {
        get 
        {
            return _store.Graphs;
        }
    }

    /// <summary>
    /// Gets the triples of the store.
    /// </summary>
    public virtual IEnumerable<Triple> Triples
    {
        get 
        {
            return _store.Triples;
        }
    }

    /// <inheritdoc />
    public virtual IEnumerable<Quad> Quads
    {
        get
        {
            return _store.Quads;
        }
    }

    /// <summary>
    /// Get the preferred URI factory to use when creating URIs in this store.
    /// </summary>
    public virtual IUriFactory UriFactory => _store.UriFactory;

    /// <inheritdoc />
    public virtual void Assert(Quad quad)
    {
        _store.Assert(quad);
    }

    /// <inheritdoc />
    public virtual void Retract(Quad quad)
    {
        _store.Retract(quad);
    }

    /// <inheritdoc />
    public virtual bool Add(IRefNode? graphName)
    {
        return _store.Add(graphName);
    }

    /// <summary>
    /// Adds a Graph to the store.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <returns></returns>
    public virtual bool Add(IGraph g)
    {
        return Add(g, false);
    }

    /// <summary>
    /// Adds a Graph to the store.
    /// </summary>
    /// <param name="g">Graph.</param>
    /// <param name="mergeIfExists">Whether to merge with an existing graph with the same URI.</param>
    /// <returns></returns>
    public virtual bool Add(IGraph g, bool mergeIfExists)
    {
        return _store.Add(g, mergeIfExists);
    }

    /// <summary>
    /// Adds a Graph to the store from a URI.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    public virtual bool AddFromUri(Uri graphUri)
    {
        return AddFromUri(graphUri, false, new Loader());
    }

    /// <summary>
    /// Adds a Graph to the store from a URI.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <param name="mergeIfExists">Whether to merge with an existing graph with the same URI.</param>
    /// <returns></returns>
    public virtual bool AddFromUri(Uri graphUri, bool mergeIfExists)
    {
        return AddFromUri(graphUri, mergeIfExists, new Loader());
    }

    /// <summary>
    /// Adds a Graph to the store from a URI.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <param name="mergeIfExists">Whether to merge with an existing graph with the same URI.</param>
    /// <param name="loader">The loader to use for retrieving and parsing the RDF data.</param>
    /// <returns></returns>
    public virtual bool AddFromUri(Uri graphUri, bool mergeIfExists, Loader loader)
    {
        return _store.AddFromUri(graphUri, mergeIfExists, loader);
    }

    /// <summary>
    /// Removes a Graph from the store.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    [Obsolete("Replaced by Remove(IRefNode)")]
    public virtual bool Remove(Uri? graphUri)
    {
        return _store.Remove(graphUri);
    }

    /// <summary>
    /// Removes a graph from the triple store.
    /// </summary>
    /// <param name="graphName">The name of the graph to remove.</param>
    /// <returns>True if the operation removed a graph, false if no matching graph was found to remove.</returns>
    public virtual bool Remove(IRefNode? graphName)
    {
        return _store.Remove(graphName);
    }

    /// <summary>
    /// Gets whether a Graph exists in the store.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    [Obsolete("Replaced by HasGraph(IRefNode)")]
    public virtual bool HasGraph(Uri graphUri)
    {
        return _store.HasGraph(graphUri);
    }

    /// <summary>
    /// Checks whether the graph with the given name is in this triple store.
    /// </summary>
    /// <param name="graphName">The name of the graph to check for.</param>
    /// <returns>True if this store contains a graph with the specified name, false otherwise.</returns>
    /// <remarks>Pass null for<paramref name="graphName"/> to check for the default (unnamed) graph.</remarks>
    public virtual bool HasGraph(IRefNode? graphName)
    {
        return _store.HasGraph(graphName);
    }

    /// <summary>
    /// Gets a Graph from the store.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    [Obsolete("Replaced by this[IRefNode]")]
    public virtual IGraph this[Uri graphUri]
    {
        get
        {
            return _store[graphUri];
        }
    }

    /// <summary>
    /// Gets a graph from the triple store.
    /// </summary>
    /// <param name="graphName">The name of the graph to be retrieved. May be null to retrieve the default (unnamed) graph.</param>
    /// <returns></returns>
    public virtual IGraph this[IRefNode? graphName] => _store[graphName];

    /// <inheritdoc />
    public IEnumerable<Quad> GetQuads(INode? s = null, INode? p = null, INode? o = null, IRefNode? g = null, bool allGraphs = true)
    {
        return _store.GetQuads(s, p, o, g, allGraphs);
    }

    /// <summary>
    /// Event which is raised when a graph is added
    /// </summary>
    public event TripleStoreEventHandler? GraphAdded;

    /// <summary>
    /// Events which is raised when a graph is removed
    /// </summary>
    public event TripleStoreEventHandler? GraphRemoved;

    /// <summary>
    /// Event which is raised when a graph is changed
    /// </summary>
    public event TripleStoreEventHandler? GraphChanged;

    /// <summary>
    /// Event which is raised when a graph is cleared
    /// </summary>
    public event TripleStoreEventHandler? GraphCleared;

    /// <summary>
    /// Event which is raised when a graph is merged
    /// </summary>
    public event TripleStoreEventHandler? GraphMerged;

    /// <summary>
    /// Helper method for raising the <see cref="GraphAdded">Graph Added</see> event manually.
    /// </summary>
    /// <param name="g">Graph.</param>
    protected void RaiseGraphAdded(IGraph g)
    {
        GraphAdded?.Invoke(this, new TripleStoreEventArgs(this, g));
    }

    /// <summary>
    /// Helper method for raising the <see cref="GraphAdded">Graph Added</see> event manually.
    /// </summary>
    /// <param name="args">Graph Event Arguments.</param>
    protected void RaiseGraphAdded(GraphEventArgs args)
    {
        GraphAdded?.Invoke(this, new TripleStoreEventArgs(this, args));
    }

    /// <summary>
    /// Event Handler which handles the <see cref="BaseGraphCollection.GraphAdded">Graph Added</see> event from the underlying Graph Collection and raises the Triple Store's <see cref="GraphAdded">Graph Added</see> event.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Graph Event Arguments.</param>
    /// <remarks>Override this method if your Triple Store implementation wishes to take additional actions when a Graph is added to the Store.</remarks>
    protected virtual void OnGraphAdded(object sender, TripleStoreEventArgs args)
    {
        RaiseGraphAdded(args.GraphEvent);
    }

    /// <summary>
    /// Helper method for raising the <see cref="GraphRemoved">Graph Removed</see> event manually.
    /// </summary>
    /// <param name="g">Graph.</param>
    protected void RaiseGraphRemoved(IGraph g)
    {
        GraphRemoved?.Invoke(this, new TripleStoreEventArgs(this, g));
    }

    /// <summary>
    /// Helper method for raising the <see cref="GraphRemoved">Graph Removed</see> event manually.
    /// </summary>
    /// <param name="args">Graph Event Arguments.</param>
    protected void RaiseGraphRemoved(GraphEventArgs args)
    {
        GraphRemoved?.Invoke(this, new TripleStoreEventArgs(this, args));
    }

    /// <summary>
    /// Event Handler which handles the <see cref="BaseGraphCollection.GraphRemoved">Graph Removed</see> event from the underlying Graph Collection and raises the Triple Stores's <see cref="GraphRemoved">Graph Removed</see> event.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Graph Event Arguments.</param>
    protected virtual void OnGraphRemoved(object sender, TripleStoreEventArgs args)
    {
        RaiseGraphRemoved(args.GraphEvent);
    }

    /// <summary>
    /// Helper method for raising the <see cref="GraphChanged">Graph Changed</see> event manually.
    /// </summary>
    /// <param name="args">Graph Event Arguments.</param>
    protected void RaiseGraphChanged(GraphEventArgs args)
    {
        GraphChanged?.Invoke(this, new TripleStoreEventArgs(this, args));
    }

    /// <summary>
    /// Event Handler which handles the <see cref="IGraph.Changed">Changed</see> event of the contained Graphs by raising the Triple Store's <see cref="GraphChanged">Graph Changed</see> event.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Graph Event Arguments.</param>
    protected virtual void OnGraphChanged(object sender, TripleStoreEventArgs args)
    {
        RaiseGraphChanged(args.GraphEvent);
    }

    /// <summary>
    /// Helper method for raising the <see cref="GraphChanged">Graph Changed</see> event manually.
    /// </summary>
    /// <param name="g">Graph.</param>
    protected void RaiseGraphChanged(IGraph g)
    {
        GraphChanged?.Invoke(this, new TripleStoreEventArgs(this, g));
    }

    /// <summary>
    /// Helper method for raising the <see cref="GraphCleared">Graph Cleared</see> event manually.
    /// </summary>
    /// <param name="args">Graph Event Arguments.</param>
    protected void RaiseGraphCleared(GraphEventArgs args)
    {
        GraphCleared?.Invoke(this, new TripleStoreEventArgs(this, args));
    }

    /// <summary>
    /// Event Handler which handles the <see cref="IGraph.Cleared">Cleared</see> event of the contained Graphs by raising the Triple Stores's <see cref="GraphCleared">Graph Cleared</see> event.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Graph Event Arguments.</param>
    protected virtual void OnGraphCleared(object sender, TripleStoreEventArgs args)
    {
        RaiseGraphCleared(args.GraphEvent);
    }

    /// <summary>
    /// Helper method for raising the <see cref="GraphMerged">Graph Merged</see> event manually.
    /// </summary>
    /// <param name="args">Graph Event Arguments.</param>
    protected void RaiseGraphMerged(GraphEventArgs args)
    {
        GraphMerged?.Invoke(this, new TripleStoreEventArgs(this, args));
    }

    /// <summary>
    /// Event Handler which handles the <see cref="IGraph.Merged">Merged</see> event of the contained Graphs by raising the Triple Store's <see cref="GraphMerged">Graph Merged</see> event.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="args">Graph Event Arguments.</param>
    protected virtual void OnGraphMerged(object sender, TripleStoreEventArgs args)
    {
        RaiseGraphMerged(args.GraphEvent);
    }

    /// <summary>
    /// Disposes of the Triple Store.
    /// </summary>
    public virtual void Dispose()
    {
        _store.Dispose();
    }
}
