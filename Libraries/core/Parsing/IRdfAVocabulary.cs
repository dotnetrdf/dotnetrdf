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

namespace VDS.RDF.Parsing
{
    /// <summary>
    /// Interface for RDFa Vocabularies
    /// </summary>
    public interface IRdfAVocabulary
    {
        /// <summary>
        /// Gets whether a Vocabulary contains a Term
        /// </summary>
        /// <param name="term">Term</param>
        /// <returns></returns>
        bool HasTerm(String term);

        /// <summary>
        /// Resolves a Term in the Vocabulary
        /// </summary>
        /// <param name="term">Term</param>
        /// <returns></returns>
        String ResolveTerm(String term);

        /// <summary>
        /// Adds a Term to the Vocabulary
        /// </summary>
        /// <param name="term">Term</param>
        /// <param name="uri">URI</param>
        void AddTerm(String term, String uri);

        /// <summary>
        /// Adds a Namespace to the Vocabulary
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <param name="nsUri">Namespace URI</param>
        void AddNamespace(String prefix, String nsUri);

        /// <summary>
        /// Merges another Vocabulary into this one
        /// </summary>
        /// <param name="vocab">Vocabulary</param>
        void Merge(IRdfAVocabulary vocab);

        /// <summary>
        /// Gets/Sets the Vocabulary URI
        /// </summary>
        String VocabularyUri
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the Term Mappings
        /// </summary>
        IEnumerable<KeyValuePair<String, String>> Mappings
        {
            get;
        }

        /// <summary>
        /// Gets the Namespace Mappings
        /// </summary>
        IEnumerable<KeyValuePair<String, String>> Namespaces
        {
            get;
        }
    }

    /// <summary>
    /// Vocabulary for XHTML+RDFa (and HTML+RDFa)
    /// </summary>
    public class XHtmlRdfAVocabulary : IRdfAVocabulary
    {
        private String[] _terms = new String[]
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
            "up"
        };

        /// <summary>
        /// Gets whether the Vocabulary contains a Term
        /// </summary>
        /// <param name="term">Term</param>
        /// <returns></returns>
        public bool HasTerm(String term)
        {
            return this._terms.Contains(term);
        }

        /// <summary>
        /// Resolves a Term in the Vocabulary
        /// </summary>
        /// <param name="term">Term</param>
        /// <returns></returns>
        public string ResolveTerm(string term)
        {
            return RdfAParser.XHtmlVocabNamespace + term;
        }

        /// <summary>
        /// Adds a Term to the Vocabulary
        /// </summary>
        /// <param name="term">Term</param>
        /// <param name="uri">URI</param>
        /// <exception cref="NotSupportedException">Thrown since this vocabulary is fixed and cannot be changed</exception>
        public void AddTerm(String term, String uri)
        {
            throw new NotSupportedException("Cannot add a term to a fixed vocabulary");
        }

        /// <summary>
        /// Adds a Namespace to the Vocabulary
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <param name="nsUri">Namespace URI</param>
        /// <exception cref="NotSupportedException">Thrown since this vocabulary is fixed and cannot be changed</exception>
        public void AddNamespace(String prefix, String nsUri)
        {
            throw new NotSupportedException("Cannot add a namespace to a fixed vocabulary");
        }

        /// <summary>
        /// Merges another Vocabulary into this one
        /// </summary>
        /// <param name="vocab">Vocabulary</param>
        /// <exception cref="NotSupportedException">Thrown since this vocabulary is fixed and cannot be changed</exception>
        public void Merge(IRdfAVocabulary vocab)
        {
            throw new NotSupportedException("Cannot merge a vocabulary into a fixed vocabulary");
        }

        /// <summary>
        /// Gets the Term Mappings
        /// </summary>
        public IEnumerable<KeyValuePair<String, String>> Mappings
        {
            get
            {
                return (from t in this._terms
                        select new KeyValuePair<String,String>(t, RdfAParser.XHtmlVocabNamespace + t));
            }
        }

        /// <summary>
        /// Gets the Namespace Mappings
        /// </summary>
        public IEnumerable<KeyValuePair<String, String>> Namespaces
        {
            get
            {
                return Enumerable.Empty<KeyValuePair<String, String>>();
            }
        }

        /// <summary>
        /// Gets/Sets the Vocabulary URI
        /// </summary>
        /// <exception cref="NotSupportedException">Set throws this since this vocabulary is fixed and cannot be changed</exception>
        public String VocabularyUri
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
    /// Represents a dynamic vocabulary for RDFa
    /// </summary>
    public class TermMappings : IRdfAVocabulary
    {
        private Dictionary<String, String> _terms = new Dictionary<string, string>();
        private Dictionary<String, String> _namespaces = new Dictionary<string, string>();
        private String _vocabUri = String.Empty;

        /// <summary>
        /// Creates a new set of Term Mappings
        /// </summary>
        public TermMappings()
        {
        }

        /// <summary>
        /// Creates a new set of Term Mappings with the given Vocabulary URI
        /// </summary>
        /// <param name="vocabUri">Vocabulary URI</param>
        public TermMappings(String vocabUri)
        {
            this._vocabUri = vocabUri;
        }

        /// <summary>
        /// Creates a new set of Term Mappings from the given Vocabulary
        /// </summary>
        /// <param name="vocab">Vocabulary</param>
        public TermMappings(IRdfAVocabulary vocab)
        {
            foreach (KeyValuePair<String,String> term in vocab.Mappings)
            {
                this.AddTerm(term.Key, term.Value);
            }
            this._vocabUri = vocab.VocabularyUri;
        }

        /// <summary>
        /// Merges another Vocabulary into this one
        /// </summary>
        /// <param name="vocab">Vocabulary</param>
        public void Merge(IRdfAVocabulary vocab)
        {
            foreach (KeyValuePair<String, String> term in vocab.Mappings)
            {
                this.AddTerm(term.Key, term.Value);
            }
            foreach (KeyValuePair<String, String> ns in vocab.Namespaces)
            {
                this.AddNamespace(ns.Key, ns.Value);
            }
            this._vocabUri = vocab.VocabularyUri;
        }

        /// <summary>
        /// Gets whether the Vocabulary contains a Term
        /// </summary>
        /// <param name="term">Term</param>
        /// <returns></returns>
        public bool HasTerm(String term)
        {
            return this._terms.ContainsKey(term);
        }

        /// <summary>
        /// Resolves a Term in the Vocabulary
        /// </summary>
        /// <param name="term">Term</param>
        /// <returns></returns>
        public string ResolveTerm(String term)
        {
            if (this._terms.ContainsKey(term))
            {
                return this._terms[term];
            }
            else if (!this._vocabUri.Equals(String.Empty))
            {
                return this._vocabUri + term;
            }
            else
            {
                throw new RdfParseException("The Term '" + term + "' cannot be resolved to a valid URI as it is not a term in this vocabularly nor is there a vocabulary URI defined");
            }

        }

        /// <summary>
        /// Adds a Namespace to the Vocabulary
        /// </summary>
        /// <param name="prefix">Prefix</param>
        /// <param name="nsUri">Namespace URI</param>
        public void AddNamespace(String prefix, String nsUri)
        {
            if (this._namespaces.ContainsKey(prefix))
            {
                this._namespaces[prefix] = nsUri;
            }
            else
            {
                this._namespaces.Add(prefix, nsUri);
            }
        }

        /// <summary>
        /// Adds a Term to the Vocabulary
        /// </summary>
        /// <param name="term">Term</param>
        /// <param name="uri">URI</param>
        public void AddTerm(String term, String uri)
        {
            if (this._terms.ContainsKey(term))
            {
                this._terms[term] = uri;
            }
            else
            {
                this._terms.Add(term, uri);
            }
        }

        /// <summary>
        /// Gets the Term Mappings
        /// </summary>
        public IEnumerable<KeyValuePair<String, String>> Mappings
        {
            get
            {
                return this._terms;
            }
        }

        /// <summary>
        /// Gets the Namespace Mappings
        /// </summary>
        public IEnumerable<KeyValuePair<String, String>> Namespaces
        {
            get
            {
                return this._namespaces;
            }
        }

        /// <summary>
        /// Gets/Sets the Vocabulary URI
        /// </summary>
        public String VocabularyUri
        {
            get
            {
                return this._vocabUri;
            }
            set
            {
                this._vocabUri = value;
            }
        }
    }
}
