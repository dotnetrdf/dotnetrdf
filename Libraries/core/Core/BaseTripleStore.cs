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
using VDS.RDF.Parsing;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract Base Class for a Triple Store
    /// </summary>
    public abstract class BaseTripleStore : ITripleStore
    {
        /// <summary>
        /// Collection of Graphs that comprise the Triple Store
        /// </summary>
        protected BaseGraphCollection _graphs;

        /// <summary>
        /// Event Handler definitions
        /// </summary>
        private GraphEventHandler GraphAddedHandler, GraphRemovedHandler, GraphChangedHandler, GraphMergedHandler, GraphClearedHandler;

        /// <summary>
        /// Creates a new Base Triple Store
        /// </summary>
        /// <param name="graphCollection">Graph Collection to use</param>
        protected BaseTripleStore(BaseGraphCollection graphCollection)
        {
            if (graphCollection == null) throw new ArgumentNullException("graphCollection", "Graph Collection must be an non-null instance of a class which derives from BaseGraphCollection");
            this._graphs = graphCollection;

            this.GraphAddedHandler = new GraphEventHandler(this.OnGraphAdded);
            this.GraphRemovedHandler = new GraphEventHandler(this.OnGraphRemoved);
            this.GraphChangedHandler = new GraphEventHandler(this.OnGraphChanged);
            this.GraphMergedHandler = new GraphEventHandler(this.OnGraphMerged);
            this.GraphClearedHandler = new GraphEventHandler(this.OnGraphCleared);

            //Attach Handlers to the Graph Collection
            this._graphs.GraphAdded += this.GraphAddedHandler;
            this._graphs.GraphRemoved += this.GraphRemovedHandler;
        }

        #region Properties

        /// <summary>
        /// Gets whether the Triple Store is empty
        /// </summary>
        public virtual bool IsEmpty
        {
            get
            {
                //Empty if there are no Graphs in the Store
                return (this._graphs.Count == 0);
            }
        }

        /// <summary>
        /// Gets the Collection of Graphs that comprise this Triple Store
        /// </summary>
        public BaseGraphCollection Graphs
        {
            get
            {
                return this._graphs;
            }
        }

        /// <summary>
        /// Gets all the Triples in the Triple Store
        /// </summary>
        public IEnumerable<Triple> Triples
        {
            get
            {
                return (from g in this._graphs
                        from t in g.Triples
                        select t);
            }
        }

        #endregion

        #region Loading & Unloading

        /// <summary>
        /// Adds a Graph into the Triple Store
        /// </summary>
        /// <param name="g">Graph to add</param>
        public virtual void Add(IGraph g)
        {
            this._graphs.Add(g, false);
        }

        /// <summary>
        /// Adds a Graph into the Triple Store using the chosen Merging Behaviour
        /// </summary>
        /// <param name="g">Graph to add</param>
        /// <param name="mergeIfExists">Whether the Graph should be merged with an existing Graph with the same Base Uri</param>
        public virtual void Add(IGraph g, bool mergeIfExists)
        {
            this._graphs.Add(g, mergeIfExists);
        }

        /// <summary>
        /// Adds a Graph into the Triple Store which is retrieved from the given Uri
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to load</param>
        public virtual void AddFromUri(Uri graphUri)
        {
            this.AddFromUri(graphUri, false);
        }

        /// <summary>
        /// Adds a Graph into the Triple Store which is retrieved from the given Uri using the chosen Merging Behaviour
        /// </summary>
        /// <param name="graphUri">Graph to add</param>
        /// <param name="mergeIfExists">Whether the Graph should be merged with an existing Graph with the same Base Uri</param>
        /// <remarks>
        /// <strong>Important:</strong> Under Silverlight/Windows Phone 7 this will happen asynchronously so the Graph may not be immediatedly available in the store
        /// </remarks>
        public virtual void AddFromUri(Uri graphUri, bool mergeIfExists)
        {
            Graph g = new Graph();
#if !SILVERLIGHT
            UriLoader.Load(g, graphUri);
            this._graphs.Add(g, mergeIfExists);
#else
            UriLoader.Load(g, graphUri, (gr, _) => { this._graphs.Add(gr, mergeIfExists); }, null);
#endif
        }

        /// <summary>
        /// Removes a Graph from the Triple Store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to Remove</param>
        public virtual void Remove(Uri graphUri)
        {
            this._graphs.Remove(graphUri);
        }

        #endregion

        #region Graph Retrieval

        /// <summary>
        /// Checks whether a Graph with the given Base Uri exists in the Triple Store
        /// </summary>
        /// <param name="graphUri">Graph Uri</param>
        /// <returns>True if the Graph exists in the Triple Store</returns>
        public bool HasGraph(Uri graphUri)
        {
            return this._graphs.Contains(graphUri);
        }

        /// <summary>
        /// Gets the Graph with the given Uri
        /// </summary>
        /// <param name="graphUri">Graph Uri</param>
        /// <returns></returns>
        public IGraph Graph(Uri graphUri)
        {
            return this._graphs[graphUri];
        }

        #endregion

        #region Events

        /// <summary>
        /// Event which is raised when a Graph is added
        /// </summary>
        public event TripleStoreEventHandler GraphAdded;

        /// <summary>
        /// Event which is raised when a Graph is removed
        /// </summary>
        public event TripleStoreEventHandler GraphRemoved;

        /// <summary>
        /// Event which is raised when a Graphs contents changes
        /// </summary>
        public event TripleStoreEventHandler GraphChanged;

        /// <summary>
        /// Event which is raised when a Graph is cleared
        /// </summary>
        public event TripleStoreEventHandler GraphCleared;

        /// <summary>
        /// Event which is raised when a Graph has a merge operation performed on it
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
            this.AttachEventHandlers(g);
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
            this.AttachEventHandlers(args.Graph);
        }

        /// <summary>
        /// Event Handler which handles the <see cref="BaseGraphCollection.GraphAdded">Graph Added</see> event from the underlying Graph Collection and raises the Triple Store's <see cref="GraphAdded">Graph Added</see> event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Graph Event Arguments</param>
        /// <remarks>Override this method if your Triple Store implementation wishes to take additional actions when a Graph is added to the Store</remarks>
        protected virtual void OnGraphAdded(Object sender, GraphEventArgs args)
        {
            this.RaiseGraphAdded(args);
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
            this.DetachEventHandlers(g);
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
            this.DetachEventHandlers(args.Graph);
        }

        /// <summary>
        /// Event Handler which handles the <see cref="BaseGraphCollection.GraphRemoved">Graph Removed</see> event from the underlying Graph Collection and raises the Triple Stores's <see cref="GraphRemoved">Graph Removed</see> event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Graph Event Arguments</param>
        protected virtual void OnGraphRemoved(Object sender, GraphEventArgs args)
        {
            this.RaiseGraphRemoved(args);
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
        protected virtual void OnGraphChanged(Object sender, GraphEventArgs args)
        {
            this.RaiseGraphChanged(args);
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
        protected virtual void OnGraphCleared(Object sender, GraphEventArgs args)
        {
            this.RaiseGraphCleared(args);
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
        protected virtual void OnGraphMerged(Object sender, GraphEventArgs args)
        {
            this.RaiseGraphMerged(args);
        }

        /// <summary>
        /// Helper method which attaches the Triple Store's Event Handlers to the relevant events of a Graph
        /// </summary>
        /// <param name="g">Graph</param>
        protected void AttachEventHandlers(IGraph g)
        {
            g.Changed += this.GraphChangedHandler;
            g.Cleared += this.GraphClearedHandler;
            g.Merged += this.GraphMergedHandler;
        }

        /// <summary>
        /// Helper method which detaches the Triple Store's Event Handlers from the relevant events of a Graph
        /// </summary>
        /// <param name="g"></param>
        protected void DetachEventHandlers(IGraph g)
        {
            g.Changed -= this.GraphChangedHandler;
            g.Cleared -= this.GraphClearedHandler;
            g.Merged -= this.GraphMergedHandler;
        }

        #endregion

        /// <summary>
        /// Disposes of the Triple Store
        /// </summary>
        /// <remarks>Derived classes must override this to implement required disposal actions</remarks>
        public abstract void Dispose();
    }
}
