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

using System.IO;

namespace VDS.RDF.Writing.Contexts
{
    /// <summary>
    /// Writer Context for TriG
    /// </summary>
    public class TriGWriterContext : ThreadedStoreWriterContext
    {
        private int _compressionLevel = WriterCompressionLevel.Default;
        private bool _n3compatability = false;

        /// <summary>
        /// Creates a new TriG Writer context
        /// </summary>
        /// <param name="store">Triple Store to save</param>
        /// <param name="output">TextWriter to output to</param>
        /// <param name="prettyPrint">Whether to use pretty printing</param>
        /// <param name="hiSpeedAllowed">Whether high speed mode is permitted</param>
        /// <param name="compressionLevel">Compression Level to use</param>
        /// <param name="n3compatability">Whether to enable N3 compatability mode</param>
        public TriGWriterContext(ITripleStore store, TextWriter output, bool prettyPrint, bool hiSpeedAllowed, int compressionLevel, bool n3compatability)
            : base(store, output, prettyPrint, hiSpeedAllowed)
        {
            _compressionLevel = compressionLevel;
        }

        /// <summary>
        /// Gets/Sets the Compression Level
        /// </summary>
        public int CompressionLevel
        {
            get
            {
                return _compressionLevel;
            }
            set
            {
                _compressionLevel = value;
            }
        }

        /// <summary>
        /// Gets/Sets N3 Compatability Mode
        /// </summary>
        public bool N3CompatabilityMode
        {
            get
            {
                return _n3compatability;
            }
            set
            {
                _n3compatability = value;
            }
        }
    }
}
