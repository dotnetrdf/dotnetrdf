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
using System.Linq;
using System.Text;
using System.Threading;
using VDS.Common.Collections;

namespace VDS.RDF.Writing
{
    /// <summary>
    /// Mapper class which remaps Blank Node GUIDs into string IDs for output
    /// </summary>
    public class BlankNodeOutputMapper
    {
        /// <summary>
        /// Default prefix prepended to the numeric IDs produced by this mapper
        /// </summary>
        public const String DefaultOutputPrefix = "b";

        private readonly String _outputPrefix = DefaultOutputPrefix;
        private readonly IDictionary<Guid, String> _mappings = new MultiDictionary<Guid, String>();
        private long _nextid = 0;

        /// <summary>
        /// Creates a new mapper using the default settings
        /// </summary>
        public BlankNodeOutputMapper()
            : this(DefaultOutputPrefix) { }

        /// <summary>
        /// Creates a new mapper using a custom blank node prefix
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <remarks>
        /// <para>
        /// It is up to the user to ensure that the the prefix given will result in valid blank node identifiers for any usage the generated string IDs are put to
        /// </para>
        /// </remarks>
        public BlankNodeOutputMapper(String prefix)
        {
            this._outputPrefix = prefix.ToSafeString();
        }

        /// <summary>
        /// Takes a GUID and generates a valid output ID
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns></returns>
        public String GetOutputId(Guid id)
        {
            lock (this._mappings)
            {
                String outputId;
                if (!this._mappings.TryGetValue(id, out outputId))
                {
                    outputId = this.GetNextId();
                    this._mappings.Add(id, outputId);
                }
                return outputId;
            }
        }

        /// <summary>
        /// Internal Helper function which generates the new IDs
        /// </summary>
        /// <returns></returns>
        private String GetNextId()
        {
            String nextID = this._outputPrefix + Interlocked.Increment(ref this._nextid);
            return nextID;
        }
    }
}
