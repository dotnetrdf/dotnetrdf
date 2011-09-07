/*

Copyright Robert Vesse 2009-11
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

namespace VDS.RDF.Storage.Virtualisation
{
    /// <summary>
    /// A Cache that maps from Virtual IDs to Materialised Values
    /// </summary>
    public class VirtualNodeCache<TNodeID, TKey>
    {
        private Dictionary<TKey, INode> _mapping = new Dictionary<TKey, INode>();
        private Func<TNodeID, TKey> _keyGenerator;

        /// <summary>
        /// Creates a new Virtual ID cache
        /// </summary>
        /// <param name="keyGenerator">Function that maps Node IDs to dictionary keys</param>
        public VirtualNodeCache(Func<TNodeID, TKey> keyGenerator)
        {
            this._keyGenerator = keyGenerator;
        }

        /// <summary>
        /// Gets/Sets the materialised value for a particular Virtual ID
        /// </summary>
        /// <param name="id">Virtual ID</param>
        /// <returns></returns>
        public INode this[TNodeID id]
        {
            get
            {
                INode temp;
                if (this._mapping.TryGetValue(this._keyGenerator(id), out temp))
                {
                    return temp;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                TKey key = this._keyGenerator(id);
                if (this._mapping.ContainsKey(key))
                {
                    this._mapping[key] = value;
                }
                else
                {
                    this._mapping.Add(key, value);
                }
            }
        }
    }

    /// <summary>
    /// A Cache that maps from Virtual IDs to Materialised Values where the IDs map directly to dictionary keys
    /// </summary>
    /// <typeparam name="TNodeID">Node ID Type</typeparam>
    public class SimpleVirtualNodeCache<TNodeID>
        : VirtualNodeCache<TNodeID, TNodeID>
    {
        /// <summary>
        /// Creates a new Simple Virtual Node Cache
        /// </summary>
        public SimpleVirtualNodeCache()
            : base(id => id) { }
    }
}
