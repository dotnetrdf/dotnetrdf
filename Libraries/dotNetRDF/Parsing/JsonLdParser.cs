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
using Newtonsoft.Json;
using VDS.RDF.Parsing.Handlers;
using Newtonsoft.Json.Linq;
using VDS.RDF.JsonLd;

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Parser for JSON-LD 1.0/1.1
    /// </summary>
    public class JsonLdParser : IStoreReader
    {
        /// <inheritdoc/>
        public event StoreReaderWarning Warning;

        /// <summary>
        /// Get the current parser options
        /// </summary>
        public JsonLdProcessorOptions ParserOptions { get; private set; }

        /// <summary>
        /// Create an instance of the parser configured to parser JSON-LD 1.1 with no pre-defined context
        /// </summary>
        public JsonLdParser() : this(new JsonLdProcessorOptions { Syntax = JsonLdSyntax.JsonLd11 }) { }

        /// <summary>
        /// Create an instace of the parser configured with the provided parser options
        /// </summary>
        /// <param name="parserOptions"></param>
        public JsonLdParser(JsonLdProcessorOptions parserOptions)
        {
            ParserOptions = parserOptions;
        }

        /// <summary>
        /// Read JSON-LD from the specified file and add the RDF quads found in the JSON-LD to the specified store
        /// </summary>
        /// <param name="store">The store to add the parsed RDF quads to</param>
        /// <param name="filename">The path to the JSON file to be parsed</param>
        public void Load(ITripleStore store, string filename)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (filename == null) throw new ArgumentNullException(nameof(filename));
            using (var reader = File.OpenText(filename))
            {
                Load(new StoreHandler(store), reader);
            }
        }

        /// <inheritdoc/>
        public void Load(ITripleStore store, TextReader input)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (input == null) throw new ArgumentNullException(nameof(input));
            Load(new StoreHandler(store), input);
        }

        /// <inheritdoc/>
        public void Load(IRdfHandler handler, string filename)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            if (filename == null) throw new ArgumentNullException(nameof(filename));
            using (var reader = File.OpenText(filename))
            {
                Load(handler, reader);
            }
        }

        /// <inheritdoc/>
        public void Load(IRdfHandler handler, TextReader input)
        {
            throw new NotImplementedException();
        }
    }
}