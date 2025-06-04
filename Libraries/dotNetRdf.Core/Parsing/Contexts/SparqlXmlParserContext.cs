/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Xml;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing.Contexts;

/// <summary>
/// Parser Context for SPARQL XML Results parsers.
/// </summary>
public class SparqlXmlParserContext : BaseResultsParserContext
{
    /// <summary>
    /// Creates a new Parser Context.
    /// </summary>
    /// <param name="reader">XML Reader.</param>
    /// <param name="handler">Results Handler.</param>
    /// <param name="uriFactory">URI Factory to use.</param>
    public SparqlXmlParserContext(XmlReader reader, ISparqlResultsHandler handler, IUriFactory uriFactory = null)
        : base(handler, uriFactory)
    {
        Input = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    /// <summary>
    /// Creates a new Parser Context.
    /// </summary>
    /// <param name="reader">XML Reader.</param>
    /// <param name="results">Results Set to load into.</param>
    /// <param name="uriFactory">URI Factory to use.</param>
    public SparqlXmlParserContext(XmlReader reader, SparqlResultSet results, IUriFactory uriFactory = null)
        : this(reader, new ResultSetHandler(results), uriFactory) { }

    /// <summary>
    /// Gets the XML Reader.
    /// </summary>
    public XmlReader Input { get; }
}
