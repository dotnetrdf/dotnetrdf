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
    /// Abstract Base Class for Node Collections
    /// </summary>
    /// <remarks>Designed to allow the underlying storage of a Node Collection to be changed at a later date without affecting classes that use it</remarks>
    public abstract class BaseNodeCollection : IEnumerable<INode>, IDisposable
    {
        /// <summary>
        /// Adds a new Node to the Collection
        /// </summary>
        /// <param name="n">Node to add</param>
        protected abstract internal void Add(INode n);

        /// <summary>
        /// Gets all the Blank Nodes in the Collection
        /// </summary>
        public abstract IEnumerable<IBlankNode> BlankNodes
        { 
            get;
        }

        /// <summary>
        /// Checks whether the given Node is in the Node Collection
        /// </summary>
        /// <param name="n">The Node to test</param>
        /// <returns>Returns True if the Node is already in the collection</returns>
        public abstract bool Contains(INode n);

        /// <summary>
        /// Gets the Number of Nodes in the Collection
        /// </summary>
        public abstract int Count 
        {
            get;
        }

        /// <summary>
        /// Gets all the Graph Literal Nodes in the Collection
        /// </summary>
        public abstract IEnumerable<IGraphLiteralNode> GraphLiteralNodes
        { 
            get;
        }

        /// <summary>
        /// Gets all the Literal Nodes in the Collection
        /// </summary>
        public abstract IEnumerable<ILiteralNode> LiteralNodes 
        { 
            get; 
        }

        /// <summary>
        /// Gets all the Uri Nodes in the Collection
        /// </summary>
        public abstract IEnumerable<IUriNode> UriNodes 
        { 
            get; 
        }

        /// <summary>
        /// Disposes of a Node Collection
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Gets the Typed Enumerator for the Node Collection
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator<INode> GetEnumerator();

        /// <summary>
        /// Gets the non-generic Enumerator for the Node Collection
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Enumerable.Empty<INode>().GetEnumerator();
        }
    }
}
