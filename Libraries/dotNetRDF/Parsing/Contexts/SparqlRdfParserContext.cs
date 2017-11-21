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
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Parser Context for SPARQL RDF Parser
    /// </summary>
    public class SparqlRdfParserContext : BaseResultsParserContext
    {
        private IGraph _g;

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="g">Graph to parse from</param>
        /// <param name="handler">Results Handler</param>
        public SparqlRdfParserContext(IGraph g, ISparqlResultsHandler handler)
            : base(handler)
        {
            if (g == null) throw new ArgumentNullException("g");
            _g = g;
        }

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="g">Graph to parse from</param>
        /// <param name="results">Results Handler</param>
        public SparqlRdfParserContext(IGraph g, SparqlResultSet results)
            : this(g, new ResultSetHandler(results)) { }

        /// <summary>
        /// Gets the Graph being parsed from
        /// </summary>
        public IGraph Graph
        {
            get
            {
                return _g;
            }
        }

    }
}
