/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF.Query.Datasets
{
    /// <summary>
    /// Represents an in-memory dataset (i.e. a <see cref="IInMemoryQueryableStore">InMemoryQueryableStore</see>) for querying and updating using SPARQL
    /// </summary>
    public class InMemoryDataset
        : BaseTransactionalDataset
        , IThreadSafeDataset
    {
        private IInMemoryQueryableStore _store;
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Creates a new in-memory dataset using the default in-memory <see cref="TripleStore">TripleStore</see> as the underlying storage
        /// </summary>
        public InMemoryDataset()
            : this(new TripleStore()) { }

        /// <summary>
        /// Creates a new in-memory dataset using the default in-memory <see cref="TripleStore">TripleStore</see> as the underlying storage
        /// </summary>
        /// <param name="unionDefaultGraph">Whether the Default Graph when no Active/Default Graph is explicitly set should be the union of all Graphs in the Dataset</param>
        public InMemoryDataset(bool unionDefaultGraph)
            : this(new TripleStore(), unionDefaultGraph) { }

        /// <summary>
        /// Creates a new in-memory dataset containing initially just the given graph and treating the given graph as the default graph of the dataset
        /// </summary>
        /// <param name="g">Graph</param>
        public InMemoryDataset(IGraph g)
            : this(g.AsTripleStore(), g.BaseUri) { }

        /// <summary>
        /// Creates a new In-Memory dataset
        /// </summary>
        /// <param name="store">In-Memory queryable store</param>
        public InMemoryDataset(IInMemoryQueryableStore store)
            : this(store, false) { }

        /// <summary>
        /// Creates a new In-Memory dataset
        /// </summary>
        /// <param name="store">In-Memory queryable store</param>
        /// <param name="unionDefaultGraph">Whether the Default Graph when no Active/Default Graph is explicitly set should be the union of all Graphs in the Dataset</param>
        public InMemoryDataset(IInMemoryQueryableStore store, bool unionDefaultGraph)
            : base(unionDefaultGraph)
        {
            if (store == null) throw new ArgumentNullException("store");
            _store = store;

            if (!_store.HasGraph(null))
            {
                _store.Add(new Graph());
            }
        }

        /// <summary>
        /// Creates a new In-Memory dataset
        /// </summary>
        /// <param name="store">In-Memory queryable store</param>
        /// <param name="defaultGraphUri">Default Graph URI</param>
        public InMemoryDataset(IInMemoryQueryableStore store, Uri defaultGraphUri)
            : base(defaultGraphUri)
        {
            if (store == null) throw new ArgumentNullException("store");
            _store = store;

            if (!_store.HasGraph(defaultGraphUri))
            {
                Graph g = new Graph();
                g.BaseUri = defaultGraphUri;
                _store.Add(g);
            }
        }

        /// <summary>
        /// Gets the Lock used to ensure MRSW concurrency on the dataset when available
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
        /// Adds a Graph to the Dataset merging it with any existing Graph with the same URI
        /// </summary>
        /// <param name="g">Graph</param>
        protected override bool AddGraphInternal(IGraph g)
        {
            return _store.Add(g, true);
        }

        /// <summary>
        /// Removes a Graph from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        protected override bool RemoveGraphInternal(Uri graphUri)
        {
            if (graphUri == null)
            {
                if (_store.HasGraph(null))
                {
                    _store.Graphs[null].Clear();
                    return true;
                }
                return false;
            }
            else
            {
                return _store.Remove(graphUri);
            }
        }

        /// <summary>
        /// Gets whether a Graph with the given URI is the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected override bool HasGraphInternal(Uri graphUri)
        {
            return _store.HasGraph(graphUri);
        }

        /// <summary>
        /// Gets all the Graphs in the Dataset
        /// </summary>
        public override IEnumerable<IGraph> Graphs
        {
            get 
            {
                return _store.Graphs; 
            }
        }

        /// <summary>
        /// Gets all the URIs of Graphs in the Dataset
        /// </summary>
        public override IEnumerable<Uri> GraphUris
        {
            get 
            {
                return (from g in _store.Graphs
                        select g.BaseUri);
            }
        }

        /// <summary>
        /// Gets the Graph with the given URI from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// For In-Memory datasets the Graph returned from this property is no different from the Graph returned by the <see cref="InMemoryDataset.GetModifiableGraphInternal(Uri)">GetModifiableGraphInternal()</see> method
        /// </para>
        /// </remarks>
        protected override IGraph GetGraphInternal(Uri graphUri)
        {
            return _store[graphUri];
        }

        /// <summary>
        /// Gets a Modifiable wrapper around a Graph in the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected override ITransactionalGraph GetModifiableGraphInternal(Uri graphUri)
        {
            return new GraphPersistenceWrapper(this[graphUri]);
        }

        #endregion

        #region Triple Existence and Retrieval

        /// <summary>
        /// Gets whether the Dataset contains a specific Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected override bool ContainsTripleInternal(Triple t)
        {
            return _store.Contains(t);
        }

        /// <summary>
        /// Gets all the Triples in the underlying in-memory store
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<Triple> GetAllTriples()
        {
            return _store.Triples;
        }

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        protected override IEnumerable<Triple> GetTriplesWithSubjectInternal(INode subj)
        {
            return (from g in Graphs
                    from t in g.GetTriplesWithSubject(subj)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Predicate
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        protected override IEnumerable<Triple> GetTriplesWithPredicateInternal(INode pred)
        {
            return (from g in Graphs
                    from t in g.GetTriplesWithPredicate(pred)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected override IEnumerable<Triple> GetTriplesWithObjectInternal(INode obj)
        {
            return (from g in Graphs
                    from t in g.GetTriplesWithObject(obj)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Subject and Predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        protected override IEnumerable<Triple> GetTriplesWithSubjectPredicateInternal(INode subj, INode pred)
        {
            return (from g in Graphs
                    from t in g.GetTriplesWithSubjectPredicate(subj, pred)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Subject and Object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected override IEnumerable<Triple> GetTriplesWithSubjectObjectInternal(INode subj, INode obj)
        {
            return (from g in Graphs
                    from t in g.GetTriplesWithSubjectObject(subj, obj)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Predicate and Object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected override IEnumerable<Triple> GetTriplesWithPredicateObjectInternal(INode pred, INode obj)
        {
            return (from g in Graphs
                    from t in g.GetTriplesWithPredicateObject(pred, obj)
                    select t);
        }

        #endregion       

        /// <summary>
        /// If there have been changes made to the Dataset and the underlying in-memory store is a <see cref="ITransactionalStore">ITransactionalStore</see> ensures the underlying store is notified to flush those changes
        /// </summary>
        protected override void FlushInternal()
        {
            if (_store is ITransactionalStore)
            {
                ((ITransactionalStore)_store).Flush();
            }
        }
    }
}
