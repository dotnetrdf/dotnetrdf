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

        protected internal override bool Add(IGraph g, bool mergeIfExists)
        {
            return this._graphs.Add(g, mergeIfExists);
        }

        public override bool Contains(Uri graphUri)
        {
            return this._graphs.Contains(graphUri);
        }

        public override int Count
        {
            get
            {
                return this._graphs.Count;
            }
        }

        public override void Dispose()
        {
            this._graphs.Dispose();
        }

        public override IEnumerator<IGraph> GetEnumerator()
        {
            return this._graphs.GetEnumerator();
        }

        public override IEnumerable<Uri> GraphUris
        {
            get 
            {
                return this._graphs.GraphUris;
            }
        }

        protected internal override bool Remove(Uri graphUri)
        {
            return this._graphs.Remove(graphUri);
        }

        public override IGraph this[Uri graphUri]
        {
            get
            {
                return this._graphs[graphUri];
            }
        }
    }
}
