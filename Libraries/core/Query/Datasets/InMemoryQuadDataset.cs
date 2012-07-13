/*

Copyright Robert Vesse 2009-12
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
#if !NO_RWLOCK
        , IThreadSafeDataset
#endif
    {
        private IInMemoryQueryableStore _store;
#if !NO_RWLOCK
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
#endif

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
            this._store = store;

            if (!this._store.HasGraph(null))
            {
                this._store.Add(new Graph());
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
            this._store = store;

            if (!this._store.HasGraph(defaultGraphUri))
            {
                Graph g = new Graph();
                g.BaseUri = defaultGraphUri;
                this._store.Add(g);
            }
        }

#if !NO_RWLOCK
        /// <summary>
        /// Gets the Lock used to ensure MRSW concurrency on the dataset when available
        /// </summary>
        public ReaderWriterLockSlim Lock
        {
            get
            {
                return this._lock;
            }
        }
#endif

        #region Graph Existence and Retrieval

        /// <summary>
        /// Adds a Graph to the Dataset merging it with any existing Graph with the same URI
        /// </summary>
        /// <param name="g">Graph</param>
        protected override void AddGraphInternal(IGraph g)
        {
            this._store.Add(g, true);
        }

        /// <summary>
        /// Removes a Graph from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        protected override void RemoveGraphInternal(Uri graphUri)
        {
            if (graphUri == null)
            {
                if (this._store.HasGraph(null))
                {
                    this._store.Graphs[null].Clear();
                }
            }
            else
            {
                this._store.Remove(graphUri);
            }
        }

        /// <summary>
        /// Gets whether a Graph with the given URI is the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected override bool HasGraphInternal(Uri graphUri)
        {
            return this._store.HasGraph(graphUri);
        }

        /// <summary>
        /// Gets all the Graphs in the Dataset
        /// </summary>
        public override IEnumerable<IGraph> Graphs
        {
            get
            {
                return this._store.Graphs;
            }
        }

        /// <summary>
        /// Gets all the URIs of Graphs in the Dataset
        /// </summary>
        public override IEnumerable<Uri> GraphUris
        {
            get
            {
                return (from g in this._store.Graphs
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
        /// For In-Memory datasets the Graph returned from this property is no different from the Graph returned by the <see cref="InMemoryDataset.GetModifiableGraph">GetModifiableGraph()</see> method
        /// </para>
        /// </remarks>
        protected override IGraph GetGraphInternal(Uri graphUri)
        {
            return this._store.Graph(graphUri);
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
        protected internal override void AddQuad(Uri graphUri, Triple t)
        {
            if (!this._store.HasGraph(graphUri))
            {
                Graph g = new Graph();
                g.BaseUri = graphUri;
                this._store.Add(g);
            }
            this._store.Graph(graphUri).Assert(t);
        }

        /// <summary>
        /// Gets whether the dataset contains a given Quad
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="t">Triple</param>
        protected internal override bool ContainsQuad(Uri graphUri, Triple t)
        {
            if (this._store.HasGraph(graphUri))
            {
                return this._store.Graph(graphUri).ContainsTriple(t);
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
            if (this._store.HasGraph(graphUri))
            {
                return this._store.Graph(graphUri).Triples;
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
            if (this._store.HasGraph(graphUri))
            {
                return this._store.Graph(graphUri).GetTriplesWithObject(obj);
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
            if (this._store.HasGraph(graphUri))
            {
                return this._store.Graph(graphUri).GetTriplesWithPredicate(pred);
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
            if (this._store.HasGraph(graphUri))
            {
                return this._store.Graph(graphUri).GetTriplesWithPredicateObject(pred, obj);
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
            if (this._store.HasGraph(graphUri))
            {
                return this._store.Graph(graphUri).GetTriplesWithSubject(subj);
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
            if (this._store.HasGraph(graphUri))
            {
                return this._store.Graph(graphUri).GetTriplesWithSubjectObject(subj, obj);
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
            if (this._store.HasGraph(graphUri))
            {
                return this._store.Graph(graphUri).GetTriplesWithSubjectPredicate(subj, pred);
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
        protected internal override void RemoveQuad(Uri graphUri, Triple t)
        {
            if (this._store.HasGraph(graphUri))
            {
                this._store.Graph(graphUri).Retract(t);
            }
        }

        #endregion

        /// <summary>
        /// Flushes any changes to the store
        /// </summary>
        protected override void FlushInternal()
        {
            if (this._store is ITransactionalStore)
            {
                ((ITransactionalStore)this._store).Flush();
            }
        }
    }
}
