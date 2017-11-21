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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Parser Context for RDFa Parsers
    /// </summary>
    public class RdfAParserContext<THtmlDocument> : BaseParserContext
    {
        private THtmlDocument _document;
        private RdfASyntax _syntax = RdfASyntax.RDFa_1_1;
        private bool _allowXmlBase = true;
        private IRdfAVocabulary _defaultVocabularly;

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="document">XML Document</param>
        public RdfAParserContext(IGraph g, THtmlDocument document)
            : this(g, document, false) { }

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="document">HTML Document</param>
        /// <param name="traceParsing">Whether to Trace Parsing</param>
        public RdfAParserContext(IGraph g, THtmlDocument document, bool traceParsing)
            : base(g, traceParsing) 
        {
            _document = document;
        }

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="document">HTML Document</param>
        /// <param name="traceParsing">Whether to Trace Parsing</param>
        public RdfAParserContext(IRdfHandler handler, THtmlDocument document, bool traceParsing)
            : base(handler, traceParsing)
        {
            _document = document;
        }

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="document">HTML Document</param>
        public RdfAParserContext(IRdfHandler handler, THtmlDocument document)
            : this(handler, document, false) { }

        /// <summary>
        /// Gets the HTML Document
        /// </summary>
        public THtmlDocument Document
        {
            get
            {
                return _document;
            }
        }

        /// <summary>
        /// Gets/Sets whether xml:base is allowed in the embedded RDF
        /// </summary>
        public bool XmlBaseAllowed
        {
            get
            {
                return _allowXmlBase;
            }
            set
            {
                _allowXmlBase = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Default Vocabularly
        /// </summary>
        public IRdfAVocabulary DefaultVocabulary
        {
            get
            {
                return _defaultVocabularly;
            }
            set
            {
                _defaultVocabularly = value;
            }
        }

        /// <summary>
        /// Gets/Sets the RDFa syntax in use
        /// </summary>
        public RdfASyntax Syntax
        {
            get
            {
                return _syntax;
            }
            set
            {
                _syntax = value;
            }
        }
    }
}
