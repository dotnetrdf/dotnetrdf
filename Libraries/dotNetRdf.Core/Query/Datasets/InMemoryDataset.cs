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
/// Represents an in-memory dataset (i.e. a <see cref="IInMemoryQueryableStore">InMemoryQueryableStore</see>) for querying and updating using SPARQL.
/// </summary>
public class InMemoryDataset
    : BaseTransactionalDataset
    , IThreadSafeDataset
{
    private IInMemoryQueryableStore _store;

    /// <summary>
    /// Creates a new in-memory dataset using the default in-memory <see cref="TripleStore">TripleStore</see> as the underlying storage.
    /// </summary>
    public InMemoryDataset()
        : this(new TripleStore()) { }

    /// <summary>
    /// Creates a new in-memory dataset using the default in-memory <see cref="TripleStore">TripleStore</see> as the underlying storage.
    /// </summary>
    /// <param name="unionDefaultGraph">Whether the Default Graph when no Active/Default Graph is explicitly set should be the union of all Graphs in the Dataset.</param>
    public InMemoryDataset(bool unionDefaultGraph)
        : this(new TripleStore(), unionDefaultGraph) { }

    /// <summary>
    /// Creates a new in-memory dataset containing initially just the given graph and treating the given graph as the default graph of the dataset.
    /// </summary>
    /// <param name="g">Graph.</param>
    public InMemoryDataset(IGraph g)
        : this(g.AsTripleStore(), g.Name) { }

    /// <summary>
    /// Creates a new In-Memory dataset.
    /// </summary>
    /// <param name="store">In-Memory queryable store.</param>
    public InMemoryDataset(IInMemoryQueryableStore store)
        : this(store, false) { }

    /// <summary>
    /// Creates a new In-Memory dataset.
    /// </summary>
    /// <param name="store">In-Memory queryable store.</param>
    /// <param name="unionDefaultGraph">Whether the Default Graph when no Active/Default Graph is explicitly set should be the union of all Graphs in the Dataset.</param>
    public InMemoryDataset(IInMemoryQueryableStore store, bool unionDefaultGraph)
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
    /// <param name="defaultGraphName">Default Graph URI.</param>
    public InMemoryDataset(IInMemoryQueryableStore store, IRefNode defaultGraphName)
        : base(defaultGraphName)
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
    public ReaderWriterLockSlim Lock { get; } = new ReaderWriterLockSlim();

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
    /// Removes a Graph from the Dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
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

        return _store.Remove(graphName);
    }

    /// <summary>
    /// Gets whether a Graph with the given URI is the Dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <returns></returns>
    protected override bool HasGraphInternal(IRefNode graphName)
    {
        return _store.HasGraph(graphName);
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
            foreach (IRefNode n in GraphNames)
            {
                switch (n)
                {
                    case null: 
                        yield return null;
                        break;
                    case IUriNode node: 
                        yield return node.Uri;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Gets an enumeration of the names of all graphs in the dataset.
    /// </summary>
    public override IEnumerable<IRefNode> GraphNames => _store.Graphs.GraphNames;

    /// <summary>
    /// Gets the Graph with the given URI from the Dataset.
    /// </summary>
    /// <param name="graphName">Graph URI.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// For In-Memory datasets the Graph returned from this property is no different from the Graph returned by the <see cref="InMemoryDataset.GetModifiableGraphInternal(IRefNode)">GetModifiableGraphInternal()</see> method.
    /// </para>
    /// </remarks>
    protected override IGraph GetGraphInternal(IRefNode graphName)
    {
        return _store[graphName];
    }

    /// <summary>
    /// Gets a Modifiable wrapper around a Graph in the Dataset.
    /// </summary>
    /// <param name="graphName">Graph URI.</param>
    /// <returns></returns>
    protected override ITransactionalGraph GetModifiableGraphInternal(IRefNode graphName)
    {
        return new GraphPersistenceWrapper(this[graphName]);
    }

    #endregion

    #region Triple Existence and Retrieval

    /// <summary>
    /// Gets whether the Dataset contains a specific Triple.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <returns></returns>
    protected override bool ContainsTripleInternal(Triple t)
    {
        return _store.Contains(t);
    }

    /// <summary>
    /// Gets whether the specified triple is quoted in any graph in the dataset.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    protected override bool ContainsQuotedTripleInternal(Triple t)
    {
        return Graphs.Any(g => g.ContainsQuotedTriple(t));
    }

    /// <summary>
    /// Gets all the Triples in the underlying in-memory store.
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<Triple> GetAllTriples()
    {
        return _store.Triples;
    }

    /// <summary>
    /// Gets all the quoted triples in the underlying in-memory store.
    /// </summary>
    /// <returns></returns>
    protected override IEnumerable<Triple> GetAllQuotedTriples()
    {
        return Graphs.SelectMany(g => g.QuotedTriples).Distinct();
    }

    /// <summary>
    /// Gets all the Triples in the Dataset with the given Subject.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <returns></returns>
    protected override IEnumerable<Triple> GetTriplesWithSubjectInternal(INode subj)
    {
        return from g in Graphs
            from t in g.GetTriplesWithSubject(subj)
            select t;
    }

    /// <summary>
    /// Get all the quoted triples in the dataset with the given subject.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <returns></returns>
    protected override IEnumerable<Triple> GetQuotedWithSubjectInternal(INode subj)
    {
        return Graphs.SelectMany(g => g.GetQuotedWithSubject(subj));
    }

    /// <summary>
    /// Gets all the Triples in the Dataset with the given Predicate.
    /// </summary>
    /// <param name="predicate">Predicate.</param>
    /// <returns></returns>
    protected override IEnumerable<Triple> GetTriplesWithPredicateInternal(INode predicate)
    {
        return from g in Graphs
            from t in g.GetTriplesWithPredicate(predicate)
            select t;
    }

    /// <summary>
    /// Get all the quoted triples in the dataset with the given predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    protected override IEnumerable<Triple> GetQuotedWithPredicateInternal(INode predicate)
    {
        return Graphs.SelectMany(g => g.GetQuotedWithPredicate(predicate));
    }

    /// <summary>
    /// Gets all the Triples in the Dataset with the given Object.
    /// </summary>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    protected override IEnumerable<Triple> GetTriplesWithObjectInternal(INode obj)
    {
        return (from g in Graphs
                from t in g.GetTriplesWithObject(obj)
                select t);
    }

    /// <summary>
    /// Gets all the quoted triples in the dataset with the given object.
    /// </summary>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    protected override IEnumerable<Triple> GetQuotedWithObjectInternal(INode obj)
    {
        return Graphs.SelectMany(g => g.GetQuotedWithObject(obj));
    }

    /// <summary>
    /// Gets all the Triples in the Dataset with the given Subject and Predicate.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="predicate">Predicate.</param>
    /// <returns></returns>
    protected override IEnumerable<Triple> GetTriplesWithSubjectPredicateInternal(INode subj, INode predicate)
    {
        return (from g in Graphs
                from t in g.GetTriplesWithSubjectPredicate(subj, predicate)
                select t);
    }

    /// <summary>
    /// Gets all the quoted triples in the dataset with the given subject and predicate.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="predicate">Predicate.</param>
    /// <returns></returns>
    protected override IEnumerable<Triple> GetQuotedWithSubjectPredicateInternal(INode subj, INode predicate)
    {
        return Graphs.SelectMany(g => g.GetQuotedWithSubjectPredicate(subj, predicate));
    }

    /// <summary>
    /// Gets all the Triples in the Dataset with the given Subject and Object.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    protected override IEnumerable<Triple> GetTriplesWithSubjectObjectInternal(INode subj, INode obj)
    {
        return (from g in Graphs
                from t in g.GetTriplesWithSubjectObject(subj, obj)
                select t);
    }

    /// <summary>
    /// Gets all the quoted triples in the dataset with the given subject and object.
    /// </summary>
    /// <param name="subj">Subject.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    protected override IEnumerable<Triple> GetQuotedWithSubjectObjectInternal(INode subj, INode obj)
    {
        return Graphs.SelectMany(g => g.GetQuotedWithSubjectObject(subj, obj));
    }

    /// <summary>
    /// Gets all the Triples in the Dataset with the given Predicate and Object.
    /// </summary>
    /// <param name="pred">Predicate.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    protected override IEnumerable<Triple> GetTriplesWithPredicateObjectInternal(INode pred, INode obj)
    {
        return (from g in Graphs
                from t in g.GetTriplesWithPredicateObject(pred, obj)
                select t);
    }

    /// <summary>
    /// Gets all the quoted triples in the dataset with the given predicate and object.
    /// </summary>
    /// <param name="pred">Predicate.</param>
    /// <param name="obj">Object.</param>
    /// <returns></returns>
    protected override IEnumerable<Triple> GetQuotedWithPredicateObjectInternal(INode pred, INode obj)
    {
        return Graphs.SelectMany(g => g.GetQuotedWithPredicateObject(pred, obj));
    }

    #endregion       

    /// <summary>
    /// If there have been changes made to the Dataset and the underlying in-memory store is a <see cref="ITransactionalStore">ITransactionalStore</see> ensures the underlying store is notified to flush those changes.
    /// </summary>
    protected override void FlushInternal()
    {
        if (_store is ITransactionalStore transactionalStore)
        {
            transactionalStore.Flush();
        }
    }
}
