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
        protected BaseGraphCollection _graphs = new GraphCollection();

        /// <summary>
        /// Event Handler definitions
        /// </summary>
        private GraphEventHandler GraphChangedHandler, GraphMergedHandler, GraphClearedHandler;

        /// <summary>
        /// Creates a new Base Triple Store
        /// </summary>
        public BaseTripleStore()
        {
            this.GraphChangedHandler = new GraphEventHandler(this.OnGraphChanged);
            this.GraphMergedHandler = new GraphEventHandler(this.OnGraphMerged);
            this.GraphClearedHandler = new GraphEventHandler(this.OnGraphCleared);
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
            this.OnGraphAdded(g);
        }

        /// <summary>
        /// Adds a Graph into the Triple Store using the chosen Merging Behaviour
        /// </summary>
        /// <param name="g">Graph to add</param>
        /// <param name="mergeIfExists">Whether the Graph should be merged with an existing Graph with the same Base Uri</param>
        public virtual void Add(IGraph g, bool mergeIfExists)
        {
            bool didExist = this._graphs.Contains(g);
            this._graphs.Add(g, mergeIfExists);

            //We only raise the event if the Graph didn't exist prior to the addition call
            //If it already existed then a Merged event will have been raised instead
            if (!didExist) this.OnGraphAdded(g);
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
        public virtual void AddFromUri(Uri graphUri, bool mergeIfExists)
        {
            bool didExist = this._graphs.Contains(graphUri);
            Graph g = new Graph();
            UriLoader.Load(g, graphUri);
            this._graphs.Add(g, mergeIfExists);

            //We only raise the event if the Graph didn't exist prior to the addition call
            //If it already existed then a Merged event will have been raised instead
            if (!didExist) this.OnGraphAdded(g);
        }

        /// <summary>
        /// Removes a Graph from the Triple Store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to Remove</param>
        public virtual void Remove(Uri graphUri)
        {
            IGraph temp = this._graphs[graphUri];
            this._graphs.Remove(graphUri);
            this.OnGraphRemoved(temp);
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
        public event TripleStoreEventHandler GraphsMerged;

        /// <summary>
        /// Helper method for raising the <see cref="GraphAdded">Graph Added</see> event
        /// </summary>
        /// <param name="g"></param>
        protected void OnGraphAdded(IGraph g)
        {
            TripleStoreEventHandler d = this.GraphAdded;
            if (d != null)
            {
                d(this, new TripleStoreEventArgs(this, g));
            }
            this.AttachEventHandlers(g);
        }

        protected void OnGraphRemoved(IGraph g)
        {
            TripleStoreEventHandler d = this.GraphRemoved;
            if (d != null)
            {
                d(this, new TripleStoreEventArgs(this, g));
            }
            this.DetachEventHandlers(g);
        }

        protected void OnGraphChanged(GraphEventArgs args)
        {
            TripleStoreEventHandler d = this.GraphChanged;
            if (d != null)
            {
                d(this, new TripleStoreEventArgs(this, args));
            }
        }

        private void OnGraphChanged(Object sender, GraphEventArgs args)
        {
            this.OnGraphChanged(args);
        }

        protected void OnGraphChanged(IGraph g)
        {
            TripleStoreEventHandler d = this.GraphChanged;
            if (d != null)
            {
                d(this, new TripleStoreEventArgs(this, g));
            }
        }

        protected void OnGraphCleared(GraphEventArgs args)
        {
            TripleStoreEventHandler d = this.GraphCleared;
            if (d != null)
            {
                d(this, new TripleStoreEventArgs(this, args));
            }
        }

        private void OnGraphCleared(Object sender, GraphEventArgs args)
        {
            this.OnGraphCleared(args);
        }

        protected void OnGraphMerged(GraphEventArgs args)
        {
            TripleStoreEventHandler d = this.GraphsMerged;
            if (d != null)
            {
                d(this, new TripleStoreEventArgs(this, args));
            }
        }

        private void OnGraphMerged(Object sender, GraphEventArgs args)
        {
            this.OnGraphMerged(args);
        }

        protected void AttachEventHandlers(IGraph g)
        {
            g.Changed += this.GraphChangedHandler;
            g.Cleared += this.GraphClearedHandler;
            g.Merged += this.GraphMergedHandler;
        }

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
