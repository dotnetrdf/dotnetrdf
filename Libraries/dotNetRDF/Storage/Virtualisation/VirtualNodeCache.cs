/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
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
            _keyGenerator = keyGenerator;
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
                if (_mapping.TryGetValue(_keyGenerator(id), out temp))
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
                TKey key = _keyGenerator(id);
                if (_mapping.ContainsKey(key))
                {
                    _mapping[key] = value;
                }
                else
                {
                    _mapping.Add(key, value);
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
