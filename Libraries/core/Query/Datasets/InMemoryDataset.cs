/*

Copyright Robert Vesse 2009-10
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
using System.Text;
using System.Threading;

namespace VDS.RDF.Query.Datasets
{
    /// <summary>
    /// Represents an in-memory dataset (i.e. a <see cref="IInMemoryQueryableStore">InMemoryQueryableStore</see>) for querying and updating using SPARQL
    /// </summary>
    public class InMemoryDataset : BaseTransactionalDataset
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
        public InMemoryDataset()
            : this(new TripleStore()) { }

        /// <summary>
        /// Creates a new in-memory dataset using the default in-memory <see cref="TripleStore">TripleStore</see> as the underlying storage
        /// </summary>
        /// <param name="unionDefaultGraph">Whether the Default Graph when no Active/Default Graph is explicitly set should be the union of all Graphs in the Dataset</param>
        public InMemoryDataset(bool unionDefaultGraph)
            : this(new TripleStore(), unionDefaultGraph) { }

        /// <summary>
        /// Creates a new In-Memory dataset
        /// </summary>
        /// <param name="store">In-Memory queryable store</param>
        public InMemoryDataset(IInMemoryQueryableStore store)
            : this(store, true) { }

        /// <summary>
        /// Creates a new In-Memory dataset
        /// </summary>
        /// <param name="store">In-Memory queryable store</param>
        /// <param name="unionDefaultGraph">Whether the Default Graph when no Active/Default Graph is explicitly set should be the union of all Graphs in the Dataset</param>
        public InMemoryDataset(IInMemoryQueryableStore store, bool unionDefaultGraph)
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
        public InMemoryDataset(IInMemoryQueryableStore store, Uri defaultGraphUri)
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
            if (graphUri == null || graphUri.ToSafeString().Equals(GraphCollection.DefaultGraphUri))
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

        #region Triple Existence and Retrieval

        /// <summary>
        /// Gets whether the Dataset contains a specific Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected override bool ContainsTripleInternal(Triple t)
        {
            return this._store.Contains(t);
        }

        /// <summary>
        /// Gets all the Triples in the underlying in-memory store
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<Triple> GetAllTriples()
        {
            return this._store.Triples;
        }

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        protected override IEnumerable<Triple> GetTriplesWithSubjectInternal(INode subj)
        {
            return (from g in this.Graphs
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
            return (from g in this.Graphs
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
            return (from g in this.Graphs
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
            return (from g in this.Graphs
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
            return (from g in this.Graphs
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
            return (from g in this.Graphs
                    from t in g.GetTriplesWithPredicateObject(pred, obj)
                    select t);
        }

        #endregion       

        /// <summary>
        /// If there have been changes made to the Dataset and the underlying in-memory store is a <see cref="IFlushableStore">IFlushableStore</see> ensures the underlying store is notified to flush those changes
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
