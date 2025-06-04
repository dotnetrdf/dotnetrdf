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

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.Common.References;

namespace VDS.RDF.Query.Datasets;

/// <summary>
/// Abstract Base Class for Datasets which provides implementation of Active and Default Graph management.
/// </summary>
public abstract class BaseDataset
    : ISparqlDataset
{
    /// <summary>
    /// Reference to the Active Graph being used for executing a SPARQL Query.
    /// </summary>
    private readonly ThreadIsolatedReference<IGraph> _activeGraph;
    /// <summary>
    /// Default Graph for executing SPARQL Queries against.
    /// </summary>
    private readonly ThreadIsolatedReference<IGraph> _defaultGraph;
    /// <summary>
    /// Stack of Default Graph References used for executing a SPARQL Query when a Query may choose to change the Default Graph from the Dataset defined one.
    /// </summary>
    private readonly ThreadIsolatedReference<Stack<IGraph>> _defaultGraphs;
    /// <summary>
    /// Stack of Active Graph References used for executing a SPARQL Query when there are nested GRAPH Clauses.
    /// </summary>
    private readonly ThreadIsolatedReference<Stack<IGraph>> _activeGraphs;

    private readonly ThreadIsolatedReference<Stack<IEnumerable<IRefNode>>> _defaultGraphUris;

    private readonly ThreadIsolatedReference<Stack<IEnumerable<IRefNode>>> _activeGraphUris;

    private readonly bool _unionDefaultGraph = true;
    //private readonly Uri _defaultGraphUri;
    private readonly IRefNode _defaultGraphName;

    /// <summary>
    /// Creates a new Dataset.
    /// </summary>
    public BaseDataset()
    {
        _activeGraph = new ThreadIsolatedReference<IGraph>();
        _defaultGraph = new ThreadIsolatedReference<IGraph>(InitDefaultGraph);
        _defaultGraphs = new ThreadIsolatedReference<Stack<IGraph>>(InitGraphStack);
        _activeGraphs = new ThreadIsolatedReference<Stack<IGraph>>(InitGraphStack);
        _defaultGraphUris = new ThreadIsolatedReference<Stack<IEnumerable<IRefNode>>>(InitDefaultGraphUriStack);
        _activeGraphUris = new ThreadIsolatedReference<Stack<IEnumerable<IRefNode>>>(InitGraphUriStack);
    }

    /// <summary>
    /// Creates a new Dataset with the given Union Default Graph setting.
    /// </summary>
    /// <param name="unionDefaultGraph">Whether to use a Union Default Graph.</param>
    public BaseDataset(bool unionDefaultGraph)
        : this()
    {
        _unionDefaultGraph = unionDefaultGraph;
    }

    /// <summary>
    /// Creates a new Dataset with a fixed Default Graph and without a Union Default Graph.
    /// </summary>
    /// <param name="defaultGraphUri"></param>
    [Obsolete("Replaced by BaseDataset(IRefNode)")]
    public BaseDataset(Uri defaultGraphUri)
        : this()
    {
        _unionDefaultGraph = false;
        //_defaultGraphUri = defaultGraphUri;
        _defaultGraphName = new UriNode(defaultGraphUri);
    }

    /// <summary>
    /// Creates a new dataset with a fixed default graph and without a union default graph.
    /// </summary>
    /// <param name="defaultGraphName">Name to assign to the default graph.</param>
    public BaseDataset(IRefNode defaultGraphName)
        :this()
    {
        _unionDefaultGraph = false;
        _defaultGraphName = defaultGraphName;
    }

    private IGraph InitDefaultGraph()
    {
        if (_unionDefaultGraph)
        {
            return null;
        }
        else
        {
            return GetGraphInternal(_defaultGraphName);
        }
    }

    private Stack<IGraph> InitGraphStack()
    {
        return new Stack<IGraph>();
    }

    private Stack<IEnumerable<IRefNode>> InitDefaultGraphUriStack()
    {
        var s = new Stack<IEnumerable<IRefNode>>();
        if (!_unionDefaultGraph)
        {
            s.Push(new[] { _defaultGraphName });
        }
        return s;
    }

    private Stack<IEnumerable<IRefNode>> InitGraphUriStack()
    {
        return new Stack<IEnumerable<IRefNode>>();
    }

    #region Active and Default Graph Management

    /// <summary>
    /// Gets a reference to the actual <see cref="IGraph">IGraph</see> that is currently treated as the default graph.
    /// </summary>
    protected IGraph InternalDefaultGraph
    {
        get
        {
            return _defaultGraph.Value;
        }
    }

    /// <summary>
    /// Sets the Default Graph for the SPARQL Query.
    /// </summary>
    /// <param name="g"></param>
    private void SetDefaultGraphInternal(IGraph g)
    {
        _defaultGraphs.Value.Push(_defaultGraph.Value);
        _defaultGraph.Value = g;
    }

    /// <summary>
    /// Sets the Default Graph.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    [Obsolete("Replaced by SetDefaultGraph(IRefNode)")]
    public void SetDefaultGraph(Uri graphUri)
    {
        SetDefaultGraph(new UriNode(graphUri));
    }

    /// <summary>
    /// Sets the default graph to be the graph with the given name.
    /// </summary>
    /// <param name="graphName"></param>
    public void SetDefaultGraph(IRefNode graphName)
    {
        if (HasGraph(graphName))
        {
            SetDefaultGraphInternal(this[graphName]);
            _defaultGraphUris.Value.Push(new [] { graphName });
        }
        else
        {
            SetDefaultGraphInternal(new Graph());
            _defaultGraphUris.Value.Push(Enumerable.Empty<IRefNode>());
        }
    }

    /// <summary>
    /// Sets the Default Graph.
    /// </summary>
    /// <param name="graphUris">Graph URIs.</param>
    [Obsolete("Replaced by SetDefaultGraph(IEnumerable<IRefNode>)")]
    public void SetDefaultGraph(IEnumerable<Uri> graphUris)
    {
        SetDefaultGraph(graphUris.Select(x=>x == null ? null : new UriNode(x) as IRefNode).ToList());
    }

    /// <summary>
    /// Sets the default graph to be the union of the graphs with the given names.
    /// </summary>
    /// <param name="graphNames">Graph names.</param>
    public void SetDefaultGraph(IList<IRefNode> graphNames)
    {
        if (!graphNames.Any())
        {
            SetDefaultGraphInternal(new Graph());
            _defaultGraphUris.Value.Push(Enumerable.Empty<IRefNode>());
        }
        else if (graphNames.Count == 1)
        {
            SetDefaultGraph(graphNames.First());
        }
        else
        {
            // Multiple Graph URIs
            // Build a merged Graph of all the Graph URIs
            var g = new Graph();
            foreach (IRefNode graphName in graphNames)
            {
                if (HasGraph(graphName))
                {
                    g.Merge(this[graphName], true);
                }
            }
            SetDefaultGraphInternal(g);
            _defaultGraphUris.Value.Push(graphNames.ToList());
        }
    }

    /// <summary>
    /// Sets the Active Graph for the SPARQL Query.
    /// </summary>
    /// <param name="g">Active Graph.</param>
    private void SetActiveGraphInternal(IGraph g)
    {
        _activeGraphs.Value.Push(_activeGraph.Value);
        _activeGraph.Value = g;
    }

    /// <summary>
    /// Sets the Active Graph for the SPARQL query.
    /// </summary>
    /// <param name="graphUri">Uri of the Active Graph.</param>
    /// <remarks>
    /// Helper function used primarily in the execution of GRAPH Clauses.
    /// </remarks>
    [Obsolete("Replaced by SetActiveGraph(IRefNode)")]
    public void SetActiveGraph(Uri graphUri)
    {
        SetActiveGraph(graphUri == null ? null : new UriNode(graphUri));
    }

    /// <summary>
    /// Sets the active graph to be the graph with the given name.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    public void SetActiveGraph(IRefNode graphName)
    {
        if (graphName == null)
        {
            // Change the Active Graph so that the query operates over the default graph
            // If the default graph is null then it operates over the entire dataset
            _activeGraphs.Value.Push(_activeGraph.Value);
            _activeGraph.Value = _defaultGraph.Value;
            _activeGraphUris.Value.Push(_defaultGraphUris.Value.Count > 0 ? _defaultGraphUris.Value.Peek() : Enumerable.Empty<IRefNode>());
        }
        else if (HasGraph(graphName))
        {
            SetActiveGraphInternal(this[graphName]);
            _activeGraphUris.Value.Push(new[] { graphName });
        }
        else
        {
            // Active Graph is an empty Graph in the case where the Graph is not present in the Dataset
            SetActiveGraphInternal(new Graph());
            _activeGraphUris.Value.Push(Enumerable.Empty<IRefNode>());
        }
    }


    /// <summary>
    /// Sets the Active Graph for the SPARQL query.
    /// </summary>
    /// <param name="graphUris">URIs of the Graphs which form the Active Graph.</param>
    /// <remarks>Helper function used primarily in the execution of GRAPH Clauses.</remarks>
    [Obsolete("Replaced by SetActiveGraph(IList<IRefNode>)")]
    public void SetActiveGraph(IEnumerable<Uri> graphUris)
    {
        SetActiveGraph(graphUris.Select(x=>x == null ? null : new UriNode(x) as IRefNode).ToList());
    }

    /// <summary>
    /// Sets the active graph to be the union of the graphs with the given names.
    /// </summary>
    /// <param name="graphNames"></param>
    public void SetActiveGraph(IList<IRefNode> graphNames)
    {
        if (!graphNames.Any())
        {
            SetActiveGraph((UriNode)null);
        }
        else if (graphNames.Count == 1)
        {
            // If only 1 Graph Uri call the simpler SetActiveGraph method which will be quicker
            SetActiveGraph(graphNames.First());
        }
        else
        {
            // Multiple Graph URIs
            // Build a merged Graph of all the Graph URIs
            var g = new Graph();
            foreach (IRefNode graphName in graphNames)
            {
                if (HasGraph(graphName))
                {
                    g.Merge(this[graphName], true);
                }
            }
            SetActiveGraphInternal(g);
            _activeGraphUris.Value.Push(graphNames.ToList());
        }
    }



    /// <summary>
    /// Sets the Active Graph for the SPARQL query to be the previous Active Graph.
    /// </summary>
    public void ResetActiveGraph()
    {
        if (_activeGraphs.Value.Count > 0)
        {
            _activeGraph.Value = _activeGraphs.Value.Pop();
            _activeGraphUris.Value.Pop();
        }
        else
        {
            throw new RdfQueryException("Unable to reset the Active Graph since no previous Active Graphs exist");
        }
    }

    /// <summary>
    /// Sets the Default Graph for the SPARQL Query to be the previous Default Graph.
    /// </summary>
    public void ResetDefaultGraph()
    {
        if (_defaultGraphs.Value.Count > 0)
        {
            _defaultGraph.Value = _defaultGraphs.Value.Pop();
            _defaultGraphUris.Value.Pop();
        }
        else
        {
            throw new RdfQueryException("Unable to reset the Default Graph since no previous Default Graphs exist");
        }
    }

    /// <summary>
    /// Gets the Default Graph URIs.
    /// </summary>
    [Obsolete("Replaced by DefaultGraphNames. This property does not return the names of graphs that are named with a blank node.")]
    public IEnumerable<Uri> DefaultGraphUris
    {
        get
        {
            foreach (IRefNode refNode in DefaultGraphNames)
            {
                if (refNode is null) yield return null;
                if (refNode is IUriNode uriNode) yield return uriNode.Uri;
            }
        }
    }

    /// <summary>
    /// Gets the enumeration of the names of the graphs that currently make up the default graph.
    /// </summary>
    public IEnumerable<IRefNode> DefaultGraphNames
    {
        get
        {
            return _defaultGraphUris.Value.Count > 0
                ? _defaultGraphUris.Value.Peek()
                : Enumerable.Empty<IRefNode>();
        }
    }

    /// <summary>
    /// Gets the Active Graph URIs.
    /// </summary>
    [Obsolete("Replaced by ActiveGraphNames. This property does not return the names of any graphs named with a blank node.")]
    public IEnumerable<Uri> ActiveGraphUris
    {
        get
        {
            foreach (IRefNode n in ActiveGraphNames)
            {
                if (n == null) yield return null;
                if (n is IUriNode uriNode) yield return uriNode.Uri;
            }
        }
    }

    /// <summary>
    /// Gets the enumeration of the names of the graphs that currently make up the active graph.
    /// </summary>
    public IEnumerable<IRefNode> ActiveGraphNames
    {
        get
        {
            return _activeGraphUris.Value.Count > 0
                ? _activeGraphUris.Value.Peek()
                : Enumerable.Empty<IRefNode>();
        }
    }

    /// <summary>
    /// Gets whether the Default Graph is treated as being the union of all Graphs in the dataset when no Default Graph is otherwise set.
    /// </summary>
    public bool UsesUnionDefaultGraph
    {
        get
        {
            return _unionDefaultGraph;
        }
    }

    #endregion

    #region General Implementation

    /// <summary>
    /// Adds a Graph to the Dataset.
    /// </summary>
    /// <param name="g">Graph.</param>
    public abstract bool AddGraph(IGraph g);

    /// <summary>
    /// Removes a Graph from the Dataset.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    [Obsolete("Replaced by RemoveGraph(IRefNode)")]
    public virtual bool RemoveGraph(Uri graphUri)
    {
        return RemoveGraph(graphUri == null ? null : new UriNode(graphUri));
    }

    /// <summary>
    /// Removes a Graph from the Dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <exception cref="NotSupportedException">May be thrown if the Dataset is immutable i.e. Updates not supported.</exception>
    public virtual bool RemoveGraph(IRefNode graphName)
    {
        if (graphName == null)
        {
            if (_defaultGraph != null)
            {
                _defaultGraph.Value.Clear();
                return true;
            }

            if (HasGraph((IRefNode) null))
            {
                return RemoveGraphInternal(null);
            }
        }
        else if (HasGraph(graphName))
        {
            return RemoveGraphInternal(graphName);
        }
        return false;
    }

    /// <summary>
    /// Removes a Graph from the Dataset.
    /// </summary>
    /// <param name="graphName">Graph URI.</param>
    protected abstract bool RemoveGraphInternal(IRefNode graphName);

    /// <summary>
    /// Gets whether a Graph with the given URI is the Dataset.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    [Obsolete("Replaced by HasGraph(IRefNode)")]
    public bool HasGraph(Uri graphUri)
    {
        return HasGraph(graphUri == null ? null : new UriNode(graphUri));
    }

    /// <summary>
    /// Gets whether a Graph with the given name is the Dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <returns></returns>
    public bool HasGraph(IRefNode graphName)
    {
        if (graphName == null)
        {
            return _defaultGraph != null || HasGraphInternal(null);
        }

        return HasGraphInternal(graphName);
    }

    /// <summary>
    /// Determines whether a given Graph exists in the Dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <returns></returns>
    protected abstract bool HasGraphInternal(IRefNode graphName);

    /// <summary>
    /// Gets all the Graphs in the Dataset.
    /// </summary>
    public abstract IEnumerable<IGraph> Graphs
    {
        get;
    }

    /// <summary>
    /// Gets all the URIs of Graphs in the Dataset.
    /// </summary>
    [Obsolete("Replaced by GraphNames")]
    public virtual IEnumerable<Uri> GraphUris
    {
        get
        {
            foreach (IRefNode graphName in GraphNames)
            {
                switch (graphName)
                {
                    case null:
                        yield return null;
                        break;
                    case IUriNode uriNode:
                        yield return uriNode.Uri;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Gets an enumeration of the names of all graphs in the dataset.
    /// </summary>
    public abstract IEnumerable<IRefNode> GraphNames { get; }

    /// <summary>
    /// Gets the Graph with the given URI from the Dataset.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// This property need only return a read-only view of the Graph, code which wishes to modify Graphs should use the <see cref="ISparqlDataset.GetModifiableGraph(IRefNode)">GetModifiableGraph()</see> method to guarantee a Graph they can modify and will be persisted to the underlying storage.
    /// </para>
    /// </remarks>
    [Obsolete("Replaced by this[IRefNode]")]
    public virtual IGraph this[Uri graphUri]
    {
        get
        {
            if (graphUri == null)
            {
                return _defaultGraph != null ? _defaultGraph.Value : GetGraphInternal(null);
            }

            return GetGraphInternal(new UriNode(graphUri));
        }
    }

    /// <summary>
    /// Gets the graph with the given name from the dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// This property need only return a read-only view of the Graph, code which wishes to modify Graphs should use the <see cref="ISparqlDataset.GetModifiableGraph(IRefNode)">GetModifiableGraph()</see> method to guarantee a Graph they can modify and will be persisted to the underlying storage.
    /// </para>
    /// </remarks>
    public virtual IGraph this[IRefNode graphName]
    {
        get
        {
            if (graphName == null)
            {
                return _defaultGraph != null ? _defaultGraph.Value : GetGraphInternal(null);
            }

            return GetGraphInternal(graphName);
        }
    }

    /// <summary>
    /// Gets the given Graph from the Dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <returns></returns>
    protected abstract IGraph GetGraphInternal(IRefNode graphName);

    /// <summary>
    /// Gets the Graph with the given URI from the Dataset.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">May be thrown if the Dataset is immutable i.e. Updates not supported.</exception>        /// <exception cref="NotSupportedException">May be thrown if the Dataset is immutable.</exception>
    /// <remarks>
    /// <para>
    /// Graphs returned from this method must be modifiable and the Dataset must guarantee that when it is Flushed or Disposed of that any changes to the Graph are persisted.
    /// </para>
    /// </remarks>
    [Obsolete("Replaced by GetModifiableGraph(IRefNode)")]
    public IGraph GetModifiableGraph(Uri graphUri)
    {
        return GetModifiableGraph(graphUri == null ? null : new UriNode(graphUri));
    }

    /// <summary>
    /// Gets the Graph with the given URI from the Dataset.
    /// </summary>
    /// <param name="graphName">Graph URI.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// Graphs returned from this method must be modifiable and the Dataset must guarantee that when it is Flushed or Disposed of that any changes to the Graph are persisted.
    /// </para>
    /// </remarks>
    public abstract IGraph GetModifiableGraph(IRefNode graphName);

    /// <summary>
    /// Gets whether the Dataset has any Triples.
    /// </summary>
    public virtual bool HasTriples
    {
        get 
        { 
            return Triples.Any(); 
        }
    }

    /// <summary>
    /// Gets whether the Dataset contains a specific Triple.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <returns></returns>
    public bool ContainsTriple(Triple t)
    {
        if (_activeGraph.Value == null)
        {
            return _defaultGraph.Value?.ContainsTriple(t) ?? ContainsTripleInternal(t);
        }

        return _activeGraph.Value.ContainsTriple(t);
    }

    /// <summary>
    /// Determines whether the Dataset contains a specific Triple.
    /// </summary>
    /// <param name="t">Triple to search for.</param>
    /// <returns></returns>
    protected abstract bool ContainsTripleInternal(Triple t);


    /// <summary>
    /// Gets whether the dataset contains a specific quoted triple.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <returns>True if the dataset contains <paramref name="t"/> as a quoted triple, false otherwise.</returns>
    public bool ContainsQuotedTriple(Triple t)
    {
        if (_activeGraph.Value == null)
        {
            return _defaultGraph.Value?.ContainsQuotedTriple(t) ?? ContainsQuotedTripleInternal(t);
        }

        return _activeGraph.Value.ContainsQuotedTriple(t);
    }

    /// <summary>
    /// Determines whether the Dataset contains a specific quoted triple.
    /// </summary>
    /// <param name="t">Triple to search for.</param>
    /// <returns></returns>
    protected abstract bool ContainsQuotedTripleInternal(Triple t);

    /// <summary>
    /// Gets all the Triples in the Dataset.
    /// </summary>
    public IEnumerable<Triple> Triples
    {
        get
        {
            if (_activeGraph.Value == null)
            {
                if (_defaultGraph.Value == null)
                {
                    // No specific Active Graph which implies that the Default Graph is the entire Triple Store
                    return GetAllTriples();
                }
                else
                {
                    // Specific Default Graph so return that
                    return _defaultGraph.Value.Triples;
                }
            }
            else
            {
                // Active Graph is used (which may happen to be the Default Graph)
                return _activeGraph.Value.Triples;
            }
        }
    }

    /// <inheritdoc />
    public IEnumerable<Triple> QuotedTriples
    {
        get
        {
            if (_activeGraph.Value != null)
            {
                return _activeGraph.Value.QuotedTriples;
            }

            return _defaultGraph.Value == null ? GetAllQuotedTriples() : _defaultGraph.Value.QuotedTriples;
        }
    }

    private IEnumerable<Triple> GetTriples(Func<IGraph, IEnumerable<Triple>> graphFunc,
        Func<IEnumerable<Triple>> fallback)
    {
        if (_activeGraph.Value != null) return graphFunc(_activeGraph.Value);
        if (_defaultGraph.Value != null) return graphFunc(_defaultGraph.Value);
        return fallback();
    }
    /// <summary>
    /// Abstract method that concrete implementations must implement to return an enumerable of all the Triples in the Dataset.
    /// </summary>
    /// <returns></returns>
    protected abstract IEnumerable<Triple> GetAllTriples();

    /// <summary>
    /// Abstract method that concrete implementations must implement to return an enumerable of all the quoted triples in the Dataset.
    /// </summary>
    /// <returns></returns>
    protected abstract IEnumerable<Triple> GetAllQuotedTriples();

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriplesWithPredicate(Uri u)
    {
        return GetTriplesWithPredicate(new UriNode(u));
    }

    /// <summary>
    /// Gets all the Triples in the Dataset with the given Subject.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <returns></returns>
    public IEnumerable<Triple> GetTriplesWithSubject(INode subj)
    {
        return GetTriples(g => g.GetTriplesWithSubject(subj), () => GetTriplesWithSubjectInternal(subj));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriplesWithSubject(Uri u)
    {
        return GetTriplesWithSubject(new UriNode(u));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuotedWithSubject(Uri u)
    {
        return GetQuotedWithSubject(new UriNode(u));
    }

    /// <inheritdoc/>
    public IEnumerable<Triple> GetQuotedWithSubject(INode subj)
    {
        return GetTriples(g => g.GetQuotedWithSubject(subj), ()=>GetQuotedWithSubjectInternal(subj));
    }

    /// <summary>
    /// Gets all the asserted triples in the Dataset with the given Subject.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <returns></returns>
    protected abstract IEnumerable<Triple> GetTriplesWithSubjectInternal(INode subj);

    /// <summary>
    /// Gets all the quoted triples in the dataset with the given subject.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <returns></returns>
    protected abstract IEnumerable<Triple> GetQuotedWithSubjectInternal(INode subj);

    /// <summary>
    /// Gets all the Triples in the Dataset with the given Predicate.
    /// </summary>
    /// <param name="pred">Predicate.</param>
    /// <returns></returns>
    public IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
    {
        return GetTriples(g => g.GetTriplesWithPredicate(pred), ()=>GetTriplesWithPredicateInternal(pred));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuotedWithPredicate(Uri u)
    {
        return GetQuotedWithPredicate(new UriNode(u));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuotedWithPredicate(INode pred)
    {
        return GetTriples(g=> g.GetQuotedWithPredicate(pred), ()=>GetQuotedWithPredicateInternal(pred));
    }

    /// <summary>
    /// Gets all the asserted triples in the Dataset with the given Predicate.
    /// </summary>
    /// <param name="predicate">Predicate.</param>
    /// <returns></returns>
    protected abstract IEnumerable<Triple> GetTriplesWithPredicateInternal(INode predicate);

    /// <summary>
    /// Gets all the quoted triples in the dataset with the given predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    protected abstract IEnumerable<Triple> GetQuotedWithPredicateInternal(INode predicate);
    /// <inheritdoc />
    public IEnumerable<Triple> GetTriples(Uri uri)
    {
        return GetTriples(new UriNode(uri));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriples(INode n)
    {
        return GetTriplesWithSubject(n).Union(GetTriplesWithPredicate(n).Where(t => !t.Subject.Equals(n)))
            .Union(GetTriplesWithObject(n).Where(t => !(t.Subject.Equals(n) || t.Predicate.Equals(n))));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuoted(Uri u)
    {
        return GetQuoted(new UriNode(u));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuoted(INode n)
    {
        return GetQuotedWithSubject(n).Union(GetQuotedWithPredicate(n).Where(t => !t.Subject.Equals(n)))
            .Union(GetQuotedWithObject(n).Where(t => !(t.Subject.Equals(n) || t.Predicate.Equals(n))));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriplesWithObject(Uri u)
    {
        return GetTriplesWithObject(new UriNode(u));
    }

    /// <summary>
    /// Gets all the Triples in the Dataset with the given Object.
    /// </summary>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public IEnumerable<Triple> GetTriplesWithObject(INode obj)
    {
        return GetTriples(g => g.GetTriplesWithObject(obj), () => GetTriplesWithObjectInternal(obj));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuotedWithObject(Uri u)
    {
        return GetQuotedWithObject(new UriNode(u));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuotedWithObject(INode obj)
    {
        return GetTriples(g => g.GetQuotedWithObject(obj), () => GetQuotedWithObjectInternal(obj));
    }

    /// <summary>
    /// Gets all the asserted triples in the dataset with the given object.
    /// </summary>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    protected abstract IEnumerable<Triple> GetTriplesWithObjectInternal(INode obj);

    /// <summary>
    /// Gets all the quoted triples in the dataset with the given object.
    /// </summary>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    protected abstract IEnumerable<Triple> GetQuotedWithObjectInternal(INode obj);
    /// <summary>
    /// Gets all the Triples in the Dataset with the given Subject and Predicate.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="pred">Predicate.</param>
    /// <returns></returns>
    public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
    {
        return GetTriples(g => g.GetTriplesWithSubjectPredicate(subj, pred),
            () => GetTriplesWithSubjectPredicateInternal(subj, pred));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuotedWithSubjectPredicate(INode subj, INode pred)
    {
        return GetTriples(g => g.GetQuotedWithSubjectPredicate(subj, pred),
            () => GetQuotedWithSubjectPredicateInternal(subj, pred));
    }
    /// <summary>
    /// Gets all the asserted triples in the dataset with the given subject and predicate.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="predicate">Predicate.</param>
    /// <returns></returns>
    protected abstract IEnumerable<Triple> GetTriplesWithSubjectPredicateInternal(INode subj, INode predicate);

    /// <summary>
    /// Gets all the quoted triples in the dataset with the given subject and predicate.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="predicate">Predicate.</param>
    /// <returns></returns>
    protected abstract IEnumerable<Triple> GetQuotedWithSubjectPredicateInternal(INode subj, INode predicate);

    /// <inheritdoc />
    public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
    {
        return GetTriples(g => g.GetTriplesWithSubjectObject(subj, obj),
            () => GetTriplesWithSubjectObjectInternal(subj, obj));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuotedWithSubjectObject(INode subj, INode obj)
    {
        return GetTriples(g => g.GetQuotedWithSubjectObject(subj, obj),
            () => GetQuotedWithSubjectObjectInternal(subj, obj));
    }
    /// <summary>
    /// Gets all the asserted triples in the dataset with the given subject and object.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    protected abstract IEnumerable<Triple> GetTriplesWithSubjectObjectInternal(INode subj, INode obj);

    /// <summary>
    /// Gets all the quoted triples in the dataset with the given subject and object.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    protected abstract IEnumerable<Triple> GetQuotedWithSubjectObjectInternal(INode subj, INode obj);


    /// <inheritdoc />
    public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
    {
        return GetTriples(g => g.GetTriplesWithPredicateObject(pred, obj),
            () => GetTriplesWithPredicateObjectInternal(pred, obj));
    }

    /// <inheritdoc />
    public IEnumerable<Triple> GetQuotedWithPredicateObject(INode pred, INode obj)
    {
        return GetTriples(g => g.GetQuotedWithPredicateObject(pred, obj),
            () => GetQuotedWithPredicateObjectInternal(pred, obj));
    }

    /// <summary>
    /// Gets all the asserted triples in the dataset with the given predicate and object.
    /// </summary>
    /// <param name="pred">Predicate.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    protected abstract IEnumerable<Triple> GetTriplesWithPredicateObjectInternal(INode pred, INode obj);

    /// <summary>
    /// Gets all the quoted triples in the dataset with the given predicate and object.
    /// </summary>
    /// <param name="pred">Predicate.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    protected abstract IEnumerable<Triple> GetQuotedWithPredicateObjectInternal(INode pred, INode obj);

    #endregion

    /// <summary>
    /// Ensures that any changes to the Dataset (if any) are flushed to the underlying Storage.
    /// </summary>
    public abstract void Flush();

    /// <summary>
    /// Ensures that any changes to the Dataset (if any) are discarded.
    /// </summary>
    public abstract void Discard();
}
