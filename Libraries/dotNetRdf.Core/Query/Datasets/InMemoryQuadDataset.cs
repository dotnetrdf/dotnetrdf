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
using System.Threading;

namespace VDS.RDF.Query.Datasets;

/// <summary>
/// An in-memory dataset that operates in terms of quads, underlying storage is identical to a <see cref="InMemoryDataset">InMemoryDataset</see> though this dataset should be more performant for queries that access named graphs frequently.
/// </summary>
public class InMemoryQuadDataset
    : BaseTransactionalQuadDataset
    , IThreadSafeDataset
{
    private readonly IInMemoryQueryableStore _store;
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    /// <summary>
    /// Creates a new in-memory dataset using the default in-memory <see cref="TripleStore">TripleStore</see> as the underlying storage.
    /// </summary>
    public InMemoryQuadDataset()
        : this(new TripleStore()) { }

    /// <summary>
    /// Creates a new in-memory dataset using the default in-memory <see cref="TripleStore">TripleStore</see> as the underlying storage.
    /// </summary>
    /// <param name="unionDefaultGraph">Whether the Default Graph when no Active/Default Graph is explicitly set should be the union of all Graphs in the Dataset.</param>
    public InMemoryQuadDataset(bool unionDefaultGraph)
        : this(new TripleStore(), unionDefaultGraph) { }

    /// <summary>
    /// Creates a new in-memory dataset containing initially just the given graph and treating the given graph as the default graph of the dataset.
    /// </summary>
    /// <param name="g">Graph.</param>
    public InMemoryQuadDataset(IGraph g)
        : this(g.AsTripleStore(), g.Name) { }

    /// <summary>
    /// Creates a new In-Memory dataset.
    /// </summary>
    /// <param name="store">In-Memory queryable store.</param>
    public InMemoryQuadDataset(IInMemoryQueryableStore store)
        : this(store, false) { }

    /// <summary>
    /// Creates a new In-Memory dataset.
    /// </summary>
    /// <param name="store">In-Memory queryable store.</param>
    /// <param name="unionDefaultGraph">Whether the Default Graph when no Active/Default Graph is explicitly set should be the union of all Graphs in the Dataset.</param>
    public InMemoryQuadDataset(IInMemoryQueryableStore store, bool unionDefaultGraph)
        : base(unionDefaultGraph)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));

        if (!_store.HasGraph((IRefNode)null))
        {
            _store.Add(new Graph());
        }
    }

    /// <summary>
    /// Creates a new In-Memory dataset.
    /// </summary>
    /// <param name="store">In-Memory queryable store.</param>
    /// <param name="defaultGraphUri">Default Graph URI.</param>
    public InMemoryQuadDataset(IInMemoryQueryableStore store, Uri defaultGraphUri)
        : this(store, new UriNode(defaultGraphUri))
    {
    }

    /// <summary>
    /// Creates a new In-Memory dataset.
    /// </summary>
    /// <param name="store">In-Memory queryable store.</param>
    /// <param name="defaultGraphName">Default Graph name.</param>
    public InMemoryQuadDataset(IInMemoryQueryableStore store, IRefNode defaultGraphName) : base(defaultGraphName)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        if (!_store.HasGraph(defaultGraphName))
        {
            var g = new Graph(defaultGraphName);
            _store.Add(g);
        }
    }

    /// <summary>
    /// Gets the Lock used to ensure MRSW concurrency on the dataset when available.
    /// </summary>
    public ReaderWriterLockSlim Lock
    {
        get
        {
            return _lock;
        }
    }

    #region Graph Existence and Retrieval

    /// <summary>
    /// Adds a Graph to the Dataset merging it with any existing Graph with the same URI.
    /// </summary>
    /// <param name="g">Graph.</param>
    protected override bool AddGraphInternal(IGraph g)
    {
        return _store.Add(g, true);
    }

    /// <summary>
    /// Removes a graph from the dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <returns></returns>
    protected override bool RemoveGraphInternal(IRefNode graphName)
    {
        if (graphName == null)
        {
            if (_store.HasGraph((IRefNode)null))
            {
                _store.Graphs[(IRefNode)null].Clear();
                return true;
            }
            return false;
        }
        else
        {
            return _store.Remove(graphName);
        }
    }

    /// <summary>
    /// Gets whether a Graph with the given URI is the Dataset.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    protected override bool HasGraphInternal(IRefNode graphUri)
    {
        return _store.HasGraph(graphUri);
    }

    /// <summary>
    /// Gets all the Graphs in the Dataset.
    /// </summary>
    public override IEnumerable<IGraph> Graphs
    {
        get
        {
            return _store.Graphs;
        }
    }

    /// <summary>
    /// Gets all the URIs of Graphs in the Dataset.
    /// </summary>
    [Obsolete("Replaced by GraphNames")]
    public override IEnumerable<Uri> GraphUris
    {
        get
        {
            return _store.Graphs.Select(g => g.Name).OfType<IUriNode>().Select(n => n.Uri);
            // return (from g in _store.Graphs select g.BaseUri);
        }
    }

    /// <summary>
    /// Gets an enumeration of the names of all graphs in the dataset.
    /// </summary>
    public override IEnumerable<IRefNode> GraphNames
    {
        get
        {
            return _store.Graphs.Select(g => g.Name);
        }
    }

    /// <summary>
    /// Gets the Graph with the given URI from the Dataset.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// For In-Memory datasets the Graph returned from this property is no different from the Graph returned by the <see cref="InMemoryDataset.GetModifiableGraphInternal(IRefNode)">GetModifiableGraphInternal()</see> method.
    /// </para>
    /// </remarks>
    protected override IGraph GetGraphInternal(IRefNode graphUri)
    {
        return _store[graphUri];
    }

    /// <summary>
    /// Gets a Modifiable wrapper around a Graph in the Dataset.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    protected override ITransactionalGraph GetModifiableGraphInternal(IRefNode graphUri)
    {
        return new GraphPersistenceWrapper(this[graphUri]);
    }

    #endregion

    #region Quad Existence and Retrieval

    /// <summary>
    /// Adds a quad to the dataset.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <param name="t">Triple.</param>
    [Obsolete("Replaced by AddQuad(IRefNode, Triple)")]
    public override bool AddQuad(Uri graphUri, Triple t)
    {
        return AddQuad(new UriNode(graphUri), t);
    }

    /// <summary>
    /// Adds a Quad to the Dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <param name="t">Triple.</param>
    public override bool AddQuad(IRefNode graphName, Triple t)
    {
        if (!_store.HasGraph(graphName))
        {
            _store.Add(new Graph(graphName));
        }

        return _store[graphName].Assert(t);
    }


    /// <inheritdoc />
    public override bool ContainsQuad(IRefNode graphName, Triple t)
    {
        return _store.HasGraph(graphName) && _store[graphName].ContainsTriple(t);
    }

    /// <inheritdoc />
    public override bool ContainsQuoted(IRefNode graphName, Triple t)
    {
        return _store.HasGraph(graphName) && _store[graphName].ContainsQuotedTriple(t);
    }

    /// <summary>
    /// Gets all asserted triples for a given graph.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> GetQuads(IRefNode graphName)
    {
        return _store.HasGraph(graphName) ? _store[graphName].Triples : Enumerable.Empty<Triple>();
    }

    /// <summary>
    /// Gets all quoted triples for a given graph.
    /// </summary>
    /// <param name="graphName"></param>
    /// <returns></returns>
    public override IEnumerable<Triple> GetQuoted(IRefNode graphName)
    {
        return _store.HasGraph(graphName) ? _store[graphName].Triples.Quoted : Enumerable.Empty<Triple>();
    }

    /// <summary>
    /// Gets all Quads with a given object.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> GetQuadsWithObject(IRefNode graphName, INode obj)
    {
        if (_store.HasGraph(graphName))
        {
            return _store[graphName].GetTriplesWithObject(obj);
        }
        else
        {
            return Enumerable.Empty<Triple>();
        }
    }

    /// <summary>
    /// Get the quoted triples with a given object in the specified graph.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> GetQuotedWithObject(IRefNode graphName, INode obj)
    {
        return _store.HasGraph(graphName)
            ? _store[graphName].GetQuotedWithObject(obj)
            : Enumerable.Empty<Triple>();
    }

    /// <summary>
    /// Gets all Quads with a given predicate.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <param name="pred">Predicate.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> GetQuadsWithPredicate(IRefNode graphName, INode pred)
    {
        if (_store.HasGraph(graphName))
        {
            return _store[graphName].GetTriplesWithPredicate(pred);
        }
        else
        {
            return Enumerable.Empty<Triple>();
        }
    }

    /// <summary>
    /// Gets all the quoted triples in the specified graph that have the specified predicate.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <param name="pred">Predicate.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> GetQuotedWithPredicate(IRefNode graphName, INode pred)
    {
        return _store.HasGraph(graphName)
            ? _store[graphName].GetQuotedWithPredicate(pred)
            : Enumerable.Empty<Triple>();
    }

    /// <summary>
    /// Gets all Quads with a given predicate and object.
    /// </summary>
    /// <param name="graphName">Graph URI.</param>
    /// <param name="pred">Predicate.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> GetQuadsWithPredicateObject(IRefNode graphName, INode pred, INode obj)
    {
        if (_store.HasGraph(graphName))
        {
            return _store[graphName].GetTriplesWithPredicateObject(pred, obj);
        }
        else
        {
            return Enumerable.Empty<Triple>();
        }
    }

    /// <summary>
    /// Get all the quoted triples in the specified graph that have the specified predicate and object.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <param name="pred">Predicate.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> GetQuotedWithPredicateObject(IRefNode graphName, INode pred, INode obj)
    {
        return _store.HasGraph(graphName)
            ? _store[graphName].GetQuotedWithPredicateObject(pred, obj)
            : Enumerable.Empty<Triple>();
    }

    /// <summary>
    /// Gets all Quads with a given subject.
    /// </summary>
    /// <param name="graphName">Graph URI.</param>
    /// <param name="subj">Subject.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> GetQuadsWithSubject(IRefNode graphName, INode subj)
    {
        if (_store.HasGraph(graphName))
        {
            return _store[graphName].GetTriplesWithSubject(subj);
        }
        else
        {
            return Enumerable.Empty<Triple>();
        }
    }

    /// <summary>
    /// Get all the quoted triples in the specified graph that have the specified subject.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <param name="subj">Subject.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> GetQuotedWithSubject(IRefNode graphName, INode subj)
    {
        return _store.HasGraph(graphName)
            ? _store[graphName].GetQuotedWithSubject(subj)
            : Enumerable.Empty<Triple>();
    }

    /// <summary>
    /// Gets all Quads with a given subject and object.
    /// </summary>
    /// <param name="graphName">Graph URI.</param>
    /// <param name="subj">Subject.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> GetQuadsWithSubjectObject(IRefNode graphName, INode subj, INode obj)
    {
        if (_store.HasGraph(graphName))
        {
            return _store[graphName].GetTriplesWithSubjectObject(subj, obj);
        }
        else
        {
            return Enumerable.Empty<Triple>();
        }
    }

    /// <summary>
    /// Get all the quoted triples in the specified graph that have the specified subject and object.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <param name="subj">Subject.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> GetQuotedWithSubjectObject(IRefNode graphName, INode subj, INode obj)
    {
        return _store.HasGraph(graphName)
            ? _store[graphName].GetQuotedWithSubjectObject(subj, obj)
            : Enumerable.Empty<Triple>();
    }

    /// <summary>
    /// Gets all Quads with a given subject and predicate.
    /// </summary>
    /// <param name="graphName">Graph URI.</param>
    /// <param name="subj">Subject.</param>
    /// <param name="pred">Predicate.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> GetQuadsWithSubjectPredicate(IRefNode graphName, INode subj, INode pred)
    {
        return _store.HasGraph(graphName) ? _store[graphName].GetTriplesWithSubjectPredicate(subj, pred) : Enumerable.Empty<Triple>();
    }

    /// <summary>
    /// Get all the quoted triples in the specified graph that have the specified subject and predicate.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <param name="subj">Subject.</param>
    /// <param name="pred">Predicate.</param>
    /// <returns></returns>
    public override IEnumerable<Triple> GetQuotedWithSubjectPredicate(IRefNode graphName, INode subj, INode pred)
    {
        return _store.HasGraph(graphName)
            ? _store[graphName].GetQuotedWithSubjectPredicate(subj, pred)
            : Enumerable.Empty<Triple>();
    }

    /// <summary>
    /// Removes a quad from the dataset.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <param name="t">Triple.</param>
    [Obsolete("Replaced by RemoveQuad(IRefNode, Triple)")]
    public override bool RemoveQuad(Uri graphUri, Triple t)
    {
        if (_store.HasGraph(graphUri))
        {
            return _store[graphUri].Retract(t);
        }
        return false;
    }

    /// <summary>
    /// Removes a quad from the dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <param name="t">Triple to remove.</param>
    /// <returns></returns>
    public override bool RemoveQuad(IRefNode graphName, Triple t)
    {
        if (_store.HasGraph(graphName))
        {
            return _store[graphName].Retract(t);
        }

        return false;
    }

    #endregion

    /// <summary>
    /// Flushes any changes to the store.
    /// </summary>
    protected override void FlushInternal()
    {
        if (_store is ITransactionalStore store)
        {
            store.Flush();
        }
    }
}
