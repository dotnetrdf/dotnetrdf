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
        public virtual void AddFromUri(Uri graphUri, bool mergeIfExists)
        {
            Graph g = new Graph();
            UriLoader.Load(g, graphUri);
            this._graphs.Add(g, mergeIfExists);
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

        /// <summary>
        /// Disposes of the Triple Store
        /// </summary>
        /// <remarks>Derived classes must override this to implement required disposal actions</remarks>
        public abstract void Dispose();
    }
}
