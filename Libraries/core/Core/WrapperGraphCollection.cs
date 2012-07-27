/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
    /// Abstract decorator for Graph Collections to make it easier to add new functionality to existing implementations
    /// </summary>
    public abstract class WrapperGraphCollection
        : BaseGraphCollection
    {
        /// <summary>
        /// Underlying Graph Collection
        /// </summary>
        protected readonly BaseGraphCollection _graphs;

        /// <summary>
        /// Creates a decorator around a default <see cref="GraphCollection"/> instance
        /// </summary>
        public WrapperGraphCollection()
            : this(new GraphCollection()) { }

        /// <summary>
        /// Creates a decorator around the given graph collection
        /// </summary>
        /// <param name="graphCollection">Graph Collection</param>
        public WrapperGraphCollection(BaseGraphCollection graphCollection)
        {
            if (graphCollection == null) throw new ArgumentNullException("graphCollection");
            this._graphs = graphCollection;
            this._graphs.GraphAdded += this.HandleGraphAdded;
            this._graphs.GraphRemoved += this.HandleGraphRemoved;
        }

        private void HandleGraphAdded(Object sender, GraphEventArgs args)
        {
            this.RaiseGraphAdded(args.Graph);
        }

        private void HandleGraphRemoved(Object sender, GraphEventArgs args)
        {
            this.RaiseGraphRemoved(args.Graph);
        }

        /// <summary>
        /// Adds a Graph to the collection
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="mergeIfExists">Whether to merge into an existing Graph with the same URI</param>
        /// <returns></returns>
        protected internal override bool Add(IGraph g, bool mergeIfExists)
        {
            return this._graphs.Add(g, mergeIfExists);
        }

        /// <summary>
        /// Gets whether the collection contains the given Graph
        /// </summary>
        /// <param name="graphUri"></param>
        /// <returns></returns>
        public override bool Contains(Uri graphUri)
        {
            return this._graphs.Contains(graphUri);
        }

        /// <summary>
        /// Gets the number of Graphs in the collection
        /// </summary>
        public override int Count
        {
            get
            {
                return this._graphs.Count;
            }
        }

        /// <summary>
        /// Disposes of the collection
        /// </summary>
        public override void Dispose()
        {
            this._graphs.Dispose();
        }

        /// <summary>
        /// Gets the enumerator for the collection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<IGraph> GetEnumerator()
        {
            return this._graphs.GetEnumerator();
        }

        /// <summary>
        /// Gets the URIs of the Graphs in the collection
        /// </summary>
        public override IEnumerable<Uri> GraphUris
        {
            get 
            {
                return this._graphs.GraphUris;
            }
        }

        /// <summary>
        /// Removes a Graph from the collection
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected internal override bool Remove(Uri graphUri)
        {
            return this._graphs.Remove(graphUri);
        }

        /// <summary>
        /// Gets a Graph from the collection
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public override IGraph this[Uri graphUri]
        {
            get
            {
                return this._graphs[graphUri];
            }
        }
    }
}
