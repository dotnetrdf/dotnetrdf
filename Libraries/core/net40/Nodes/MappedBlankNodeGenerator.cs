/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// A blank node generator that keeps a mapping of string IDs to blank nodes
    /// </summary>
    /// <remarks>
    /// As this implementation keeps a mapping its memory usage will grow over time and for data with large amounts of blank nodes this may exhaust memory
    /// </remarks>
    public class MappedBlankNodeGenerator
        : IBlankNodeGenerator
    {
        private IDictionary<String, Guid> _ids = new Dictionary<string, Guid>();

        /// <summary>
        /// Create a new blank node
        /// </summary>
        /// <param name="id">String ID</param>
        /// <returns>Blank Node</returns>
        public Guid GetGuid(string id)
        {
            Guid guid;
            if (!this._ids.TryGetValue(id, out guid))
            {
                guid = Guid.NewGuid();
                this._ids.Add(id, guid);
            }
            return guid;
        }
    }
}
