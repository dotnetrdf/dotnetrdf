/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2020 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Interface for RDFa Vocabularies.
    /// </summary>
    public interface IRdfAVocabulary
    {
        /// <summary>
        /// Gets whether a Vocabulary contains a Term.
        /// </summary>
        /// <param name="term">Term.</param>
        /// <returns></returns>
        bool HasTerm(string term);

        /// <summary>
        /// Resolves a Term in the Vocabulary.
        /// </summary>
        /// <param name="term">Term.</param>
        /// <returns></returns>
        string ResolveTerm(string term);

        /// <summary>
        /// Adds a Term to the Vocabulary.
        /// </summary>
        /// <param name="term">Term.</param>
        /// <param name="uri">URI.</param>
        void AddTerm(string term, string uri);

        /// <summary>
        /// Adds a Namespace to the Vocabulary.
        /// </summary>
        /// <param name="prefix">Prefix.</param>
        /// <param name="nsUri">Namespace URI.</param>
        void AddNamespace(string prefix, string nsUri);

        /// <summary>
        /// Merges another Vocabulary into this one.
        /// </summary>
        /// <param name="vocab">Vocabulary.</param>
        void Merge(IRdfAVocabulary vocab);

        /// <summary>
        /// Gets/Sets the Vocabulary URI.
        /// </summary>
        string VocabularyUri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the Term Mappings.
        /// </summary>
        IEnumerable<KeyValuePair<string, string>> Mappings
        {
            get;
        }

        /// <summary>
        /// Gets the Namespace Mappings.
        /// </summary>
        IEnumerable<KeyValuePair<string, string>> Namespaces
        {
            get;
        }
    }

    /// <summary>
    /// Vocabulary for XHTML+RDFa (and HTML+RDFa).
    /// </summary>
    public class XHtmlRdfAVocabulary : IRdfAVocabulary
    {
        private string[] _terms = new string[]
        {
            "alternate",
            "appendix",
            "bookmark",
            "cite",
            "chapter",
            "contents",
            "copyright",
            "first",
            "glossary",
            "help",
            "icon",
            "index",
            "last",
            "license",
            "meta",
            "next",
            "p3pv1",
            "prev",
            "role",
            "section",
            "stylesheet",
            "subsection",
            "start",
            "top",
            "up",
        };

        /// <summary>
        /// Gets whether the Vocabulary contains a Term.
        /// </summary>
        /// <param name="term">Term.</param>
        /// <returns></returns>
        public bool HasTerm(string term)
        {
            return _terms.Contains(term);
        }

        /// <summary>
        /// Resolves a Term in the Vocabulary.
        /// </summary>
        /// <param name="term">Term.</param>
        /// <returns></returns>
        public string ResolveTerm(string term)
        {
            return RdfAParser.XHtmlVocabNamespace + term;
        }

        /// <summary>
        /// Adds a Term to the Vocabulary.
        /// </summary>
        /// <param name="term">Term.</param>
        /// <param name="uri">URI.</param>
        /// <exception cref="NotSupportedException">Thrown since this vocabulary is fixed and cannot be changed.</exception>
        public void AddTerm(string term, string uri)
        {
            throw new NotSupportedException("Cannot add a term to a fixed vocabulary");
        }

        /// <summary>
        /// Adds a Namespace to the Vocabulary.
        /// </summary>
        /// <param name="prefix">Prefix.</param>
        /// <param name="nsUri">Namespace URI.</param>
        /// <exception cref="NotSupportedException">Thrown since this vocabulary is fixed and cannot be changed.</exception>
        public void AddNamespace(string prefix, string nsUri)
        {
            throw new NotSupportedException("Cannot add a namespace to a fixed vocabulary");
        }

        /// <summary>
        /// Merges another Vocabulary into this one.
        /// </summary>
        /// <param name="vocab">Vocabulary.</param>
        /// <exception cref="NotSupportedException">Thrown since this vocabulary is fixed and cannot be changed.</exception>
        public void Merge(IRdfAVocabulary vocab)
        {
            throw new NotSupportedException("Cannot merge a vocabulary into a fixed vocabulary");
        }

        /// <summary>
        /// Gets the Term Mappings.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Mappings
        {
            get
            {
                return (from t in _terms
                        select new KeyValuePair<string, string>(t, RdfAParser.XHtmlVocabNamespace + t));
            }
        }

        /// <summary>
        /// Gets the Namespace Mappings.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Namespaces
        {
            get
            {
                return Enumerable.Empty<KeyValuePair<string, string>>();
            }
        }

        /// <summary>
        /// Gets/Sets the Vocabulary URI.
        /// </summary>
        /// <exception cref="NotSupportedException">Set throws this since this vocabulary is fixed and cannot be changed.</exception>
        public string VocabularyUri
        {
            get
            {
                return RdfAParser.XHtmlVocabNamespace;
            }
            set
            {
                throw new NotSupportedException("Cannot change the Vocabulary URI of a fixed vocabulary");
            }
        }
    }

    /// <summary>
    /// Represents a dynamic vocabulary for RDFa.
    /// </summary>
    public class TermMappings : IRdfAVocabulary
    {
        private Dictionary<string, string> _terms = new Dictionary<string, string>();
        private Dictionary<string, string> _namespaces = new Dictionary<string, string>();
        private string _vocabUri = string.Empty;

        /// <summary>
        /// Creates a new set of Term Mappings.
        /// </summary>
        public TermMappings()
        {
        }

        /// <summary>
        /// Creates a new set of Term Mappings with the given Vocabulary URI.
        /// </summary>
        /// <param name="vocabUri">Vocabulary URI.</param>
        public TermMappings(string vocabUri)
        {
            _vocabUri = vocabUri;
        }

        /// <summary>
        /// Creates a new set of Term Mappings from the given Vocabulary.
        /// </summary>
        /// <param name="vocab">Vocabulary.</param>
        public TermMappings(IRdfAVocabulary vocab)
        {
            foreach (KeyValuePair<string, string> term in vocab.Mappings)
            {
                AddTerm(term.Key, term.Value);
            }
            _vocabUri = vocab.VocabularyUri;
        }

        /// <summary>
        /// Merges another Vocabulary into this one.
        /// </summary>
        /// <param name="vocab">Vocabulary.</param>
        public void Merge(IRdfAVocabulary vocab)
        {
            foreach (KeyValuePair<string, string> term in vocab.Mappings)
            {
                AddTerm(term.Key, term.Value);
            }
            foreach (KeyValuePair<string, string> ns in vocab.Namespaces)
            {
                AddNamespace(ns.Key, ns.Value);
            }
            _vocabUri = vocab.VocabularyUri;
        }

        /// <summary>
        /// Gets whether the Vocabulary contains a Term.
        /// </summary>
        /// <param name="term">Term.</param>
        /// <returns></returns>
        public bool HasTerm(string term)
        {
            return _terms.ContainsKey(term);
        }

        /// <summary>
        /// Resolves a Term in the Vocabulary.
        /// </summary>
        /// <param name="term">Term.</param>
        /// <returns></returns>
        public string ResolveTerm(string term)
        {
            if (_terms.ContainsKey(term))
            {
                return _terms[term];
            }
            else if (!_vocabUri.Equals(string.Empty))
            {
                return _vocabUri + term;
            }
            else
            {
                throw new RdfParseException("The Term '" + term + "' cannot be resolved to a valid URI as it is not a term in this vocabularly nor is there a vocabulary URI defined");
            }

        }

        /// <summary>
        /// Adds a Namespace to the Vocabulary.
        /// </summary>
        /// <param name="prefix">Prefix.</param>
        /// <param name="nsUri">Namespace URI.</param>
        public void AddNamespace(string prefix, string nsUri)
        {
            if (_namespaces.ContainsKey(prefix))
            {
                _namespaces[prefix] = nsUri;
            }
            else
            {
                _namespaces.Add(prefix, nsUri);
            }
        }

        /// <summary>
        /// Adds a Term to the Vocabulary.
        /// </summary>
        /// <param name="term">Term.</param>
        /// <param name="uri">URI.</param>
        public void AddTerm(string term, string uri)
        {
            if (_terms.ContainsKey(term))
            {
                _terms[term] = uri;
            }
            else
            {
                _terms.Add(term, uri);
            }
        }

        /// <summary>
        /// Gets the Term Mappings.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Mappings
        {
            get
            {
                return _terms;
            }
        }

        /// <summary>
        /// Gets the Namespace Mappings.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Namespaces
        {
            get
            {
                return _namespaces;
            }
        }

        /// <summary>
        /// Gets/Sets the Vocabulary URI.
        /// </summary>
        public string VocabularyUri
        {
            get
            {
                return _vocabUri;
            }
            set
            {
                _vocabUri = value;
            }
        }
    }
}
