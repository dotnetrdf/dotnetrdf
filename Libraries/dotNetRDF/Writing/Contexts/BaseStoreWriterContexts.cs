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
using System.IO;

namespace VDS.RDF.Writing.Contexts
{
    /// <summary>
    /// Interface for Store Writer Contexts
    /// </summary>
    public interface IStoreWriterContext
    {
        /// <summary>
        /// Gets the Store being written
        /// </summary>
        ITripleStore Store
        {
            get;
        }
    }

    /// <summary>
    /// Base Class for Store Writer Context Objects
    /// </summary>
    public class BaseStoreWriterContext : IStoreWriterContext
    {
        private ITripleStore _store;
        private TextWriter _output;
        /// <summary>
        /// Pretty Print Mode setting
        /// </summary>
        protected bool _prettyPrint = true;
        /// <summary>
        /// High Speed Mode setting
        /// </summary>
        protected bool _hiSpeedAllowed = true;

        /// <summary>
        /// Creates a new Base Store Writer Context with default settings
        /// </summary>
        /// <param name="store">Store to write</param>
        /// <param name="output">TextWriter being written to</param>
        public BaseStoreWriterContext(ITripleStore store, TextWriter output)
        {
            _store = store;
            _output = output;
        }

        /// <summary>
        /// Creates a new Base Store Writer Context with custom settings
        /// </summary>
        /// <param name="store">Store to write</param>
        /// <param name="output">TextWriter being written to</param>
        /// <param name="prettyPrint">Pretty Print Mode</param>
        /// <param name="hiSpeedAllowed">High Speed Mode</param>
        public BaseStoreWriterContext(ITripleStore store, TextWriter output, bool prettyPrint, bool hiSpeedAllowed)
            : this(store, output)
        {
            _prettyPrint = prettyPrint;
            _hiSpeedAllowed = hiSpeedAllowed;
        }

        /// <summary>
        /// Gets/Sets the Pretty Printing Mode used
        /// </summary>
        public bool PrettyPrint
        {
            get
            {
                return _prettyPrint;
            }
            set
            {
                _prettyPrint = value;
            }
        }

        /// <summary>
        /// Gets/Sets the High Speed Mode used
        /// </summary>
        public bool HighSpeedModePermitted
        {
            get
            {
                return _hiSpeedAllowed;
            }
            set
            {
                _hiSpeedAllowed = value;
            }
        }

        /// <summary>
        /// Gets the Store being written
        /// </summary>
        public ITripleStore Store
        {
            get
            {
                return _store;
            }
        }

        /// <summary>
        /// Gets the TextWriter being written to
        /// </summary>
        public TextWriter Output
        {
            get
            {
                return _output;
            }
        }

        /// <summary>
        /// Formats a URI as a String for full Output
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        public virtual String FormatUri(String u)
        {
            String uri = Uri.EscapeUriString(u);
            uri = uri.Replace(">", "\\>");
            return uri;
        }

        /// <summary>
        /// Formats a URI as a String for full Output
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        public virtual String FormatUri(Uri u)
        {
            return FormatUri(u.AbsoluteUri);
        }
    }
}
