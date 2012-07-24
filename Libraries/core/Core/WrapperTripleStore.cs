using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Core
{
    /// <summary>
    /// Abstract decorator for Triple Stores to make it easier to add new functionality on top of existing implementations
    /// </summary>
    public class WrapperTripleStore
        : ITripleStore
    {
        protected readonly ITripleStore _store;

        /// <summary>
        /// Event Handler definitions
        /// </summary>
        private TripleStoreEventHandler GraphAddedHandler, GraphRemovedHandler, GraphChangedHandler, GraphMergedHandler, GraphClearedHandler;

        /// <summary>
        /// Creates a new triple store decorator that uses a default in-memory <see cref="TripleStore"/>
        /// </summary>
        public WrapperTripleStore()
            : this(new TripleStore()) { }

        /// <summary>
        /// Creates a new triple store decorator around the given <see cref="ITripleStore"/> instance
        /// </summary>
        /// <param name="store">Triple Store</param>
        public WrapperTripleStore(ITripleStore store)
        {
            if (store == null) throw new ArgumentNullException("store");
            this._store = store;

            this.GraphAddedHandler = new TripleStoreEventHandler(this.OnGraphAdded);
            this.GraphRemovedHandler = new TripleStoreEventHandler(this.OnGraphRemoved);
            this.GraphChangedHandler = new TripleStoreEventHandler(this.OnGraphChanged);
            this.GraphMergedHandler = new TripleStoreEventHandler(this.OnGraphMerged);
            this.GraphClearedHandler = new TripleStoreEventHandler(this.OnGraphCleared);

            this._store.GraphAdded += this.GraphAddedHandler;
            this._store.GraphChanged += this.GraphChangedHandler;
            this._store.GraphCleared += this.GraphClearedHandler;
            this._store.GraphMerged += this.GraphMergedHandler;
            this._store.GraphRemoved += this.GraphRemovedHandler;
        }

        public virtual bool IsEmpty
        {
            get
            {
                return this._store.IsEmpty;
            }
        }

        public BaseGraphCollection Graphs
        {
            get 
            {
                return this._store.Graphs;
            }
        }

        public IEnumerable<Triple> Triples
        {
            get 
            {
                return this._store.Triples;
            }
        }

        public bool Add(IGraph g)
        {
            return this.Add(g, false);
        }

        public bool Add(IGraph g, bool mergeIfExists)
        {
            return this._store.Add(g, mergeIfExists);
        }

        public bool AddFromUri(Uri graphUri)
        {
            return this.AddFromUri(graphUri, false);
        }

        public bool AddFromUri(Uri graphUri, bool mergeIfExists)
        {
            return this._store.AddFromUri(graphUri, mergeIfExists);
        }

        public bool Remove(Uri graphUri)
        {
            return this._store.Remove(graphUri);
        }

        public bool HasGraph(Uri graphUri)
        {
            return this._store.HasGraph(graphUri);
        }

        public IGraph Graph(Uri graphUri)
        {
            return this._store.Graph(graphUri);
        }

        public event TripleStoreEventHandler GraphAdded;

        public event TripleStoreEventHandler GraphRemoved;

        public event TripleStoreEventHandler GraphChanged;

        public event TripleStoreEventHandler GraphCleared;

        public event TripleStoreEventHandler GraphMerged;

        /// <summary>
        /// Helper method for raising the <see cref="GraphAdded">Graph Added</see> event manually
        /// </summary>
        /// <param name="g">Graph</param>
        protected void RaiseGraphAdded(IGraph g)
        {
            TripleStoreEventHandler d = this.GraphAdded;
            if (d != null)
            {
                d(this, new TripleStoreEventArgs(this, g));
            }
        }

        /// <summary>
        /// Helper method for raising the <see cref="GraphAdded">Graph Added</see> event manually
        /// </summary>
        /// <param name="args">Graph Event Arguments</param>
        protected void RaiseGraphAdded(GraphEventArgs args)
        {
            TripleStoreEventHandler d = this.GraphAdded;
            if (d != null)
            {
                d(this, new TripleStoreEventArgs(this, args));
            }
        }

        /// <summary>
        /// Event Handler which handles the <see cref="BaseGraphCollection.GraphAdded">Graph Added</see> event from the underlying Graph Collection and raises the Triple Store's <see cref="GraphAdded">Graph Added</see> event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Graph Event Arguments</param>
        /// <remarks>Override this method if your Triple Store implementation wishes to take additional actions when a Graph is added to the Store</remarks>
        protected virtual void OnGraphAdded(Object sender, TripleStoreEventArgs args)
        {
            this.RaiseGraphAdded(args.GraphEvent);
        }

        /// <summary>
        /// Helper method for raising the <see cref="GraphRemoved">Graph Removed</see> event manually
        /// </summary>
        /// <param name="g">Graph</param>
        protected void RaiseGraphRemoved(IGraph g)
        {
            TripleStoreEventHandler d = this.GraphRemoved;
            if (d != null)
            {
                d(this, new TripleStoreEventArgs(this, g));
            }
        }

        /// <summary>
        /// Helper method for raising the <see cref="GraphRemoved">Graph Removed</see> event manually
        /// </summary>
        /// <param name="args">Graph Event Arguments</param>
        protected void RaiseGraphRemoved(GraphEventArgs args)
        {
            TripleStoreEventHandler d = this.GraphRemoved;
            if (d != null)
            {
                d(this, new TripleStoreEventArgs(this, args));
            }
        }

        /// <summary>
        /// Event Handler which handles the <see cref="BaseGraphCollection.GraphRemoved">Graph Removed</see> event from the underlying Graph Collection and raises the Triple Stores's <see cref="GraphRemoved">Graph Removed</see> event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Graph Event Arguments</param>
        protected virtual void OnGraphRemoved(Object sender, TripleStoreEventArgs args)
        {
            this.RaiseGraphRemoved(args.GraphEvent);
        }

        /// <summary>
        /// Helper method for raising the <see cref="GraphChanged">Graph Changed</see> event manually
        /// </summary>
        /// <param name="args">Graph Event Arguments</param>
        protected void RaiseGraphChanged(GraphEventArgs args)
        {
            TripleStoreEventHandler d = this.GraphChanged;
            if (d != null)
            {
                d(this, new TripleStoreEventArgs(this, args));
            }
        }

        /// <summary>
        /// Event Handler which handles the <see cref="IGraph.Changed">Changed</see> event of the contained Graphs by raising the Triple Store's <see cref="GraphChanged">Graph Changed</see> event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Graph Event Arguments</param>
        protected virtual void OnGraphChanged(Object sender, TripleStoreEventArgs args)
        {
            this.RaiseGraphChanged(args.GraphEvent);
        }

        /// <summary>
        /// Helper method for raising the <see cref="GraphChanged">Graph Changed</see> event manually
        /// </summary>
        /// <param name="g">Graph</param>
        protected void RaiseGraphChanged(IGraph g)
        {
            TripleStoreEventHandler d = this.GraphChanged;
            if (d != null)
            {
                d(this, new TripleStoreEventArgs(this, g));
            }
        }

        /// <summary>
        /// Helper method for raising the <see cref="GraphCleared">Graph Cleared</see> event manually
        /// </summary>
        /// <param name="args">Graph Event Arguments</param>
        protected void RaiseGraphCleared(GraphEventArgs args)
        {
            TripleStoreEventHandler d = this.GraphCleared;
            if (d != null)
            {
                d(this, new TripleStoreEventArgs(this, args));
            }
        }

        /// <summary>
        /// Event Handler which handles the <see cref="IGraph.Cleared">Cleared</see> event of the contained Graphs by raising the Triple Stores's <see cref="GraphCleared">Graph Cleared</see> event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Graph Event Arguments</param>
        protected virtual void OnGraphCleared(Object sender, TripleStoreEventArgs args)
        {
            this.RaiseGraphCleared(args.GraphEvent);
        }

        /// <summary>
        /// Helper method for raising the <see cref="GraphMerged">Graph Merged</see> event manually
        /// </summary>
        /// <param name="args">Graph Event Arguments</param>
        protected void RaiseGraphMerged(GraphEventArgs args)
        {
            TripleStoreEventHandler d = this.GraphMerged;
            if (d != null)
            {
                d(this, new TripleStoreEventArgs(this, args));
            }
        }

        /// <summary>
        /// Event Handler which handles the <see cref="IGraph.Merged">Merged</see> event of the contained Graphs by raising the Triple Store's <see cref="GraphMerged">Graph Merged</see> event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Graph Event Arguments</param>
        protected virtual void OnGraphMerged(Object sender, TripleStoreEventArgs args)
        {
            this.RaiseGraphMerged(args.GraphEvent);
        }

        public void Dispose()
        {
            this._store.Dispose();
        }
    }
}
