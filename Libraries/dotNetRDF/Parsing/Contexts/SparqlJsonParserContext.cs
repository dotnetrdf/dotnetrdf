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
using Newtonsoft.Json;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// SPARQL JSON Parser Context
    /// </summary>
    public class SparqlJsonParserContext : BaseResultsParserContext
    {
        private JsonTextReader _reader;

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="reader">JSON Text Reader</param>
        /// <param name="handler">Results Handler</param>
        public SparqlJsonParserContext(JsonTextReader reader, ISparqlResultsHandler handler)
            : base(handler)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            _reader = reader;
        }

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="reader">JSON Text Reader</param>
        /// <param name="results">SPARQL Result Set</param>
        public SparqlJsonParserContext(JsonTextReader reader, SparqlResultSet results)
            : this(reader, new ResultSetHandler(results)) { }

        /// <summary>
        /// Gets the JSON Text Reader
        /// </summary>
        public JsonTextReader Input
        {
            get
            {
                return _reader;
            }
        }
    }
}
