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

#if !NO_HTMLAGILITYPACK
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace VDS.RDF.Parsing.Contexts
{
    /// <summary>
    /// Parser Context for RDFa Parsers
    /// </summary>
    public class RdfAParserContext : BaseParserContext
    {
        private HtmlDocument _document;
        private RdfASyntax _syntax = RdfASyntax.RDFa_1_1;
        private bool _allowXmlBase = true;
        private IRdfAVocabulary _defaultVocabularly;

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="document">XML Document</param>
        public RdfAParserContext(IGraph g, HtmlDocument document)
            : this(g, document, false) { }

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="document">HTML Document</param>
        /// <param name="traceParsing">Whether to Trace Parsing</param>
        public RdfAParserContext(IGraph g, HtmlDocument document, bool traceParsing)
            : base(g, traceParsing) 
        {
            this._document = document;
        }

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="document">HTML Document</param>
        /// <param name="traceParsing">Whether to Trace Parsing</param>
        public RdfAParserContext(IRdfHandler handler, HtmlDocument document, bool traceParsing)
            : base(handler, traceParsing)
        {
            this._document = document;
        }

        /// <summary>
        /// Creates a new Parser Context
        /// </summary>
        /// <param name="handler">RDF Handler to use</param>
        /// <param name="document">HTML Document</param>
        public RdfAParserContext(IRdfHandler handler, HtmlDocument document)
            : this(handler, document, false) { }

        /// <summary>
        /// Gets the HTML Document
        /// </summary>
        public HtmlDocument Document
        {
            get
            {
                return this._document;
            }
        }

        /// <summary>
        /// Gets/Sets whether xml:base is allowed in the embedded RDF
        /// </summary>
        public bool XmlBaseAllowed
        {
            get
            {
                return this._allowXmlBase;
            }
            set
            {
                this._allowXmlBase = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Default Vocabularly
        /// </summary>
        public IRdfAVocabulary DefaultVocabulary
        {
            get
            {
                return this._defaultVocabularly;
            }
            set
            {
                this._defaultVocabularly = value;
            }
        }

        /// <summary>
        /// Gets/Sets the RDFa syntax in use
        /// </summary>
        public RdfASyntax Syntax
        {
            get
            {
                return this._syntax;
            }
            set
            {
                this._syntax = value;
            }
        }
    }

    /// <summary>
    /// Evaluation Context for RDFa Parsers
    /// </summary>
    public class RdfAEvaluationContext
    {
        private Uri _baseUri;
        private INode _parentSubj, _parentObj;
        private NamespaceMapper _nsmapper = new NamespaceMapper(true);
        private List<IncompleteTriple> _incompleteTriples = new List<IncompleteTriple>();
        private String _lang = String.Empty;
        private IRdfAVocabulary _localVocabularly;

        /// <summary>
        /// Creates a new RDFa Evaluation Context
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        public RdfAEvaluationContext(Uri baseUri)
        {
            this._baseUri = baseUri;
            this._nsmapper.AddNamespace(String.Empty, UriFactory.Create(RdfAParser.XHtmlVocabNamespace));
        }

        /// <summary>
        /// Creates a new RDFa Evaluation Context
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <param name="nsmap">Namepace Map</param>
        public RdfAEvaluationContext(Uri baseUri, NamespaceMapper nsmap)
            : this(baseUri)
        {
            this._nsmapper = nsmap;
        }

        /// <summary>
        /// Gets/Sets the Base URI
        /// </summary>
        public Uri BaseUri
        {
            get
            {
                return this._baseUri;
            }
            set
            {
                this._baseUri = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Parent Subject
        /// </summary>
        public INode ParentSubject
        {
            get
            {
                return this._parentSubj;
            }
            set
            {
                this._parentSubj = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Parent Object
        /// </summary>
        public INode ParentObject
        {
            get
            {
                return this._parentObj;
            }
            set
            {
                this._parentObj = value;
            }
        }

        /// <summary>
        /// Gets the Namespace Map
        /// </summary>
        public NamespaceMapper NamespaceMap
        {
            get
            {
                return this._nsmapper;
            }
        }

        /// <summary>
        /// Gets/Sets the Language
        /// </summary>
        public String Language
        {
            get
            {
                return this._lang;
            }
            set
            {
                this._lang = value;
            }
        }

        /// <summary>
        /// Gets the list of incomplete Triples
        /// </summary>
        public List<IncompleteTriple> IncompleteTriples
        {
            get
            {
                return this._incompleteTriples;
            }
        }

        /// <summary>
        /// Gets/Sets the Local Vocabulary
        /// </summary>
        public IRdfAVocabulary LocalVocabulary
        {
            get
            {
                return this._localVocabularly;
            }
            set
            {
                this._localVocabularly = value;
            }
        }
    }

    /// <summary>
    /// Represents an incomplete Triple as part of the RDFa parsing process
    /// </summary>
    public class IncompleteTriple
    {
        private INode _pred;
        private IncompleteTripleDirection _dir;

        /// <summary>
        /// Creates a new Incomplete Triple
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="direction">Direction</param>
        public IncompleteTriple(INode pred, IncompleteTripleDirection direction)
        {
            this._pred = pred;
            this._dir = direction;
        }

        /// <summary>
        /// Gets the Predicate of the Incomplete Triple
        /// </summary>
        public INode Predicate
        {
            get
            {
                return this._pred;
            }
        }

        /// <summary>
        /// Gets the Direction of the Incomplete Triple
        /// </summary>
        public IncompleteTripleDirection Direction
        {
            get
            {
                return this._dir;
            }
        }
    }

    /// <summary>
    /// Possible Directions for Incomplete Triples
    /// </summary>
    public enum IncompleteTripleDirection
    {
        /// <summary>
        /// Forward
        /// </summary>
        Forward,
        /// <summary>
        /// Reverse
        /// </summary>
        Reverse
    }
}
#endif