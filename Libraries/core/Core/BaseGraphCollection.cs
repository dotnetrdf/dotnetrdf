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

namespace VDS.RDF
{
    /// <summary>
    /// Abstract Base Class for Graph Collections
    /// </summary>
    /// <remarks>Designed to allow the underlying storage of a Graph Collection to be changed at a later date without affecting classes that use it</remarks>
    public abstract class BaseGraphCollection 
        : IEnumerable<IGraph>, IDisposable
    {
        /// <summary>
        /// Checks whether the Graph with the given Uri exists in this Graph Collection
        /// </summary>
        /// <param name="graphUri">Graph Uri to test</param>
        /// <returns></returns>
        /// <remarks>
        /// The null URI is used to reference the Default Graph
        /// </remarks>
        public abstract bool Contains(Uri graphUri);

        /// <summary>
        /// Adds a Graph to the Collection
        /// </summary>
        /// <param name="g">Graph to add</param>
        /// <param name="mergeIfExists">Sets whether the Graph should be merged with an existing Graph of the same Uri if present</param>
        protected abstract internal void Add(IGraph g, bool mergeIfExists);

        /// <summary>
        /// Removes a Graph from the Collection
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to remove</param>
        /// <remarks>
        /// The null URI is used to reference the Default Graph
        /// </remarks>
        protected abstract internal void Remove(Uri graphUri);

        /// <summary>
        /// Gets the number of Graphs in the Collection
        /// </summary>
        public abstract int Count
        {
            get;
        }

        /// <summary>
        /// Provides access to the Graph URIs of Graphs in the Collection
        /// </summary>
        public abstract IEnumerable<Uri> GraphUris
        {
            get;
        }

        /// <summary>
        /// Gets a Graph from the Collection
        /// </summary>
        /// <param name="graphUri">Graph Uri</param>
        /// <returns></returns>
        /// <remarks>
        /// The null URI is used to reference the Default Graph
        /// </remarks>
        public abstract IGraph this[Uri graphUri] 
        {
            get;
        }

        /// <summary>
        /// Disposes of the Graph Collection
        /// </summary>
        /// <remarks>Invokes the <see cref="IGraph.Dipose">Dispose()</see> method of all Graphs contained in the Collection</remarks>
        public abstract void Dispose();

        /// <summary>
        /// Gets the Enumerator for the Collection
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator<IGraph> GetEnumerator();

        /// <summary>
        /// Gets the Enumerator for this Collection
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Enumerable.Empty<IGraph>().GetEnumerator();
        }

        /// <summary>
        /// Event which is raised when a Graph is added to the Collection
        /// </summary>
        public event GraphEventHandler GraphAdded;

        /// <summary>
        /// Event which is raised when a Graph is removed from the Collection
        /// </summary>
        public event GraphEventHandler GraphRemoved;

        /// <summary>
        /// Helper method which raises the <see cref="GraphAdded">Graph Added</see> event manually
        /// </summary>
        /// <param name="g">Graph</param>
        protected virtual void RaiseGraphAdded(IGraph g)
        {
            GraphEventHandler d = this.GraphAdded;
            if (d != null)
            {
                d(this, new GraphEventArgs(g));
            }
        }

        /// <summary>
        /// Helper method which raises the <see cref="GraphRemoved">Graph Removed</see> event manually
        /// </summary>
        /// <param name="g">Graph</param>
        protected virtual void RaiseGraphRemoved(IGraph g)
        {
            GraphEventHandler d = this.GraphRemoved;
            if (d != null)
            {
                d(this, new GraphEventArgs(g));
            }
        }
    }
}
