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

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Key for Objects that are cached by the Configuration Loader
    /// </summary>
    class CachedObjectKey : IEquatable<CachedObjectKey>
    {
        private INode _n;
        private IGraph _g;

        /// <summary>
        /// Creates a new Cached Object Key
        /// </summary>
        /// <param name="objNode">Object Node</param>
        /// <param name="g">Configuration Graph</param>
        public CachedObjectKey(INode objNode, IGraph g)
        {
            this._n = objNode;
            this._g = g;
        }

        /// <summary>
        /// Gets the Hash Code for the Key
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Tools.CombineHashCodes(_g, _n);
        }

        /// <summary>
        /// Gets whether this Key is equal to the given Object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is CachedObjectKey)
            {
                return this.Equals((CachedObjectKey)obj);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets whether this Key is equal to the given Key
        /// </summary>
        /// <param name="other">Key</param>
        /// <returns></returns>
        public bool Equals(CachedObjectKey other)
        {
            return ReferenceEquals(this._g, other._g) && this._n.Equals(other._n);
        }
    }
}
