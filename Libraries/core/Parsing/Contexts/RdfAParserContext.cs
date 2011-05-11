/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

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
            this._nsmapper.AddNamespace(String.Empty, new Uri(RdfAParser.XHtmlVocabNamespace));
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
