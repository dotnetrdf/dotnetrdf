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
    /// An in-memory dataset that operates in terms of quads, underlying storage is identical to a <see cref="InMemoryDataset">InMemoryDataset</see> though this dataset should be more performant for queries that access named graphs frequently
    /// </summary>
    public class InMemoryQuadDataset
        : BaseTransactionalQuadDataset
        , IThreadSafeDataset
    {
        private IInMemoryQueryableStore _store;
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Creates a new in-memory dataset using the default in-memory <see cref="TripleStore">TripleStore</see> as the underlying storage
        /// </summary>
        public InMemoryQuadDataset()
            : this(new TripleStore()) { }

        /// <summary>
        /// Creates a new in-memory dataset using the default in-memory <see cref="TripleStore">TripleStore</see> as the underlying storage
        /// </summary>
        /// <param name="unionDefaultGraph">Whether the Default Graph when no Active/Default Graph is explicitly set should be the union of all Graphs in the Dataset</param>
        public InMemoryQuadDataset(bool unionDefaultGraph)
            : this(new TripleStore(), unionDefaultGraph) { }

        /// <summary>
        /// Creates a new in-memory dataset containing initially just the given graph and treating the given graph as the default graph of the dataset
        /// </summary>
        /// <param name="g">Graph</param>
        public InMemoryQuadDataset(IGraph g)
            : this(g.AsTripleStore(), g.BaseUri) { }

        /// <summary>
        /// Creates a new In-Memory dataset
        /// </summary>
        /// <param name="store">In-Memory queryable store</param>
        public InMemoryQuadDataset(IInMemoryQueryableStore store)
            : this(store, false) { }

        /// <summary>
        /// Creates a new In-Memory dataset
        /// </summary>
        /// <param name="store">In-Memory queryable store</param>
        /// <param name="unionDefaultGraph">Whether the Default Graph when no Active/Default Graph is explicitly set should be the union of all Graphs in the Dataset</param>
        public InMemoryQuadDataset(IInMemoryQueryableStore store, bool unionDefaultGraph)
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
        public InMemoryQuadDataset(IInMemoryQueryableStore store, Uri defaultGraphUri)
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

        #region Quad Existence and Retrieval

        /// <summary>
        /// Adds a quad to the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="t">Triple</param>
        protected internal override bool AddQuad(Uri graphUri, Triple t)
        {
            if (!_store.HasGraph(graphUri))
            {
                Graph g = new Graph();
                g.BaseUri = graphUri;
                _store.Add(g);
            }
            return _store[graphUri].Assert(t);
        }

        /// <summary>
        /// Gets whether the dataset contains a given Quad
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="t">Triple</param>
        protected internal override bool ContainsQuad(Uri graphUri, Triple t)
        {
            if (_store.HasGraph(graphUri))
            {
                return _store[graphUri].ContainsTriple(t);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets all quads for a given graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected internal override IEnumerable<Triple> GetQuads(Uri graphUri)
        {
            if (_store.HasGraph(graphUri))
            {
                return _store[graphUri].Triples;
            }
            else
            {
                return Enumerable.Empty<Triple>();
            }
        }

        /// <summary>
        /// Gets all Quads with a given object
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected internal override IEnumerable<Triple> GetQuadsWithObject(Uri graphUri, INode obj)
        {
            if (_store.HasGraph(graphUri))
            {
                return _store[graphUri].GetTriplesWithObject(obj);
            }
            else
            {
                return Enumerable.Empty<Triple>();
            }
        }

        /// <summary>
        /// Gets all Quads with a given predicate
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        protected internal override IEnumerable<Triple> GetQuadsWithPredicate(Uri graphUri, INode pred)
        {
            if (_store.HasGraph(graphUri))
            {
                return _store[graphUri].GetTriplesWithPredicate(pred);
            }
            else
            {
                return Enumerable.Empty<Triple>();
            }
        }

        /// <summary>
        /// Gets all Quads with a given predicate and object
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected internal override IEnumerable<Triple> GetQuadsWithPredicateObject(Uri graphUri, INode pred, INode obj)
        {
            if (_store.HasGraph(graphUri))
            {
                return _store[graphUri].GetTriplesWithPredicateObject(pred, obj);
            }
            else
            {
                return Enumerable.Empty<Triple>();
            }
        }

        /// <summary>
        /// Gets all Quads with a given subject
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        protected internal override IEnumerable<Triple> GetQuadsWithSubject(Uri graphUri, INode subj)
        {
            if (_store.HasGraph(graphUri))
            {
                return _store[graphUri].GetTriplesWithSubject(subj);
            }
            else
            {
                return Enumerable.Empty<Triple>();
            }
        }

        /// <summary>
        /// Gets all Quads with a given subject and object
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected internal override IEnumerable<Triple> GetQuadsWithSubjectObject(Uri graphUri, INode subj, INode obj)
        {
            if (_store.HasGraph(graphUri))
            {
                return _store[graphUri].GetTriplesWithSubjectObject(subj, obj);
            }
            else
            {
                return Enumerable.Empty<Triple>();
            }
        }

        /// <summary>
        /// Gets all Quads with a given subject and predicate
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        protected internal override IEnumerable<Triple> GetQuadsWithSubjectPredicate(Uri graphUri, INode subj, INode pred)
        {
            if (_store.HasGraph(graphUri))
            {
                return _store[graphUri].GetTriplesWithSubjectPredicate(subj, pred);
            }
            else
            {
                return Enumerable.Empty<Triple>();
            }
        }

        /// <summary>
        /// Removes a quad from the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="t">Triple</param>
        protected internal override bool RemoveQuad(Uri graphUri, Triple t)
        {
            if (_store.HasGraph(graphUri))
            {
                return _store[graphUri].Retract(t);
            }
            return false;
        }

        #endregion

        /// <summary>
        /// Flushes any changes to the store
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
