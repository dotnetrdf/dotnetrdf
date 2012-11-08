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
using VDS.Common;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Construct
{
    /// <summary>
    /// Context used for Constructing Triples in SPARQL Query/Update
    /// </summary>
    public class ConstructContext
    {
        private ISet _s;
        private Dictionary<String, INode> _bnodeMap;

        public ConstructContext() { }

        /// <summary>
        /// Creates a new Construct Context
        /// </summary>
        /// <param name="s">Set to construct from</param>
        /// <remarks>
        /// </remarks>
        public ConstructContext(ISet s)
        {
            if (s == null) throw new ArgumentNullException("s");
            this._s = s;
        }

        /// <summary>
        /// Gets the Set that this Context pertains to
        /// </summary>
        public ISet Set
        {
            get
            {
                return this._s;
            }
        }

        /// <summary>
        /// Creates a new Blank Node for this Context
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// If the same Blank Node ID is used multiple times in this Context you will always get the same Blank Node for that ID
        /// </para>
        /// </remarks>
        public INode CreateBlankNode(String id)
        {
            if (this._bnodeMap == null) this._bnodeMap = new Dictionary<string, INode>();

            if (this._bnodeMap.ContainsKey(id)) return this._bnodeMap[id];

            INode temp = new BlankNode(Guid.NewGuid());
            this._bnodeMap.Add(id, temp);
            return temp;
        }
    }
}
