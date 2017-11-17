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

namespace VDS.RDF.Parsing.Contexts
{
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
            _baseUri = baseUri;
            _nsmapper.AddNamespace(String.Empty, UriFactory.Create(RdfAParser.XHtmlVocabNamespace));
        }

        /// <summary>
        /// Creates a new RDFa Evaluation Context
        /// </summary>
        /// <param name="baseUri">Base URI</param>
        /// <param name="nsmap">Namepace Map</param>
        public RdfAEvaluationContext(Uri baseUri, NamespaceMapper nsmap)
            : this(baseUri)
        {
            _nsmapper = nsmap;
        }

        /// <summary>
        /// Gets/Sets the Base URI
        /// </summary>
        public Uri BaseUri
        {
            get
            {
                return _baseUri;
            }
            set
            {
                _baseUri = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Parent Subject
        /// </summary>
        public INode ParentSubject
        {
            get
            {
                return _parentSubj;
            }
            set
            {
                _parentSubj = value;
            }
        }

        /// <summary>
        /// Gets/Sets the Parent Object
        /// </summary>
        public INode ParentObject
        {
            get
            {
                return _parentObj;
            }
            set
            {
                _parentObj = value;
            }
        }

        /// <summary>
        /// Gets the Namespace Map
        /// </summary>
        public NamespaceMapper NamespaceMap
        {
            get
            {
                return _nsmapper;
            }
        }

        /// <summary>
        /// Gets/Sets the Language
        /// </summary>
        public String Language
        {
            get
            {
                return _lang;
            }
            set
            {
                _lang = value;
            }
        }

        /// <summary>
        /// Gets the list of incomplete Triples
        /// </summary>
        public List<IncompleteTriple> IncompleteTriples
        {
            get
            {
                return _incompleteTriples;
            }
        }

        /// <summary>
        /// Gets/Sets the Local Vocabulary
        /// </summary>
        public IRdfAVocabulary LocalVocabulary
        {
            get
            {
                return _localVocabularly;
            }
            set
            {
                _localVocabularly = value;
            }
        }
    }
}
