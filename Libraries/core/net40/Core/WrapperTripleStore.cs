/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract decorator for Triple Stores to make it easier to add new functionality on top of existing implementations
    /// </summary>
    public class WrapperTripleStore
        : ITripleStore
    {
        /// <summary>
        /// Underlying store
        /// </summary>
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

        /// <summary>
        /// Gets whether the store is empty
        /// </summary>
        public virtual bool IsEmpty
        {
            get
            {
                return this._store.IsEmpty;
            }
        }

        /// <summary>
        /// Gets the Graphs of the store
        /// </summary>
        public virtual BaseGraphCollection Graphs
        {
            get 
            {
                return this._store.Graphs;
            }
        }

        /// <summary>
        /// Gets the triples of the store
        /// </summary>
        public virtual IEnumerable<Triple> Triples
        {
            get 
            {
                return this._store.Triples;
            }
        }

        /// <summary>
        /// Adds a Graph to the store
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public virtual bool Add(IGraph g)
        {
            return this.Add(g, false);
        }

        /// <summary>
        /// Adds a Graph to the store
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="mergeIfExists">Whether to merge with an existing graph with the same URI</param>
        /// <returns></returns>
        public virtual bool Add(IGraph g, bool mergeIfExists)
        {
            return this._store.Add(g, mergeIfExists);
        }

        /// <summary>
        /// Adds a Graph to the store from a URI
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public virtual bool AddFromUri(Uri graphUri)
        {
            return this.AddFromUri(graphUri, false);
        }

        /// <summary>
        /// Adds a Graph to the store from a URI
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="mergeIfExists">Whether to merge with an existing graph with the same URI</param>
        /// <returns></returns>
        public virtual bool AddFromUri(Uri graphUri, bool mergeIfExists)
        {
            return this._store.AddFromUri(graphUri, mergeIfExists);
        }

        /// <summary>
        /// Removes a Graph from the store
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public virtual bool Remove(Uri graphUri)
        {
            return this._store.Remove(graphUri);
        }

        /// <summary>
        /// Gets whether a Graph exists in the store
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public virtual bool HasGraph(Uri graphUri)
        {
            return this._store.HasGraph(graphUri);
        }

        /// <summary>
        /// Gets a Graph from the store
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        [Obsolete("This method is obsolete, use the indexer (this[graphUri]) to access Graphs instead", false)]
        public virtual IGraph Graph(Uri graphUri)
        {
            return this._store.Graph(graphUri);
        }

        /// <summary>
        /// Gets a Graph from the store
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public virtual IGraph this[Uri graphUri]
        {
            get
            {
                return this._store[graphUri];
            }
        }

        /// <summary>
        /// Event which is raised when a graph is added
        /// </summary>
        public event TripleStoreEventHandler GraphAdded;

        /// <summary>
        /// Events which is raised when a graph is removed
        /// </summary>
        public event TripleStoreEventHandler GraphRemoved;

        /// <summary>
        /// Event which is raised when a graph is changed
        /// </summary>
        public event TripleStoreEventHandler GraphChanged;

        /// <summary>
        /// Event which is raised when a graph is cleared
        /// </summary>
        public event TripleStoreEventHandler GraphCleared;

        /// <summary>
        /// Event which is raised when a graph is merged
        /// </summary>
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

        /// <summary>
        /// Disposes of the Triple Store
        /// </summary>
        public virtual void Dispose()
        {
            this._store.Dispose();
        }
    }
}
