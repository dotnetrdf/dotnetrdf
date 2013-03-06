/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
