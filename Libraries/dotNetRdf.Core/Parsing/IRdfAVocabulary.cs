/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2023 dotNetRDF Project (http://dotnetrdf.org/)
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
        [Obsolete("Use the NamespaceMapper property to access the namespace map")]
        IEnumerable<KeyValuePair<string, string>> Namespaces
        {
            get;
        }

        /// <summary>
        /// Gets the namespace mappings.
        /// </summary>
        INamespaceMapper NamespaceMap { get; }

        /// <summary>
        /// Resolve a CURIE using the namespaces defined in this vocabulary.
        /// </summary>
        /// <param name="curie">The CURIE string to resolve.</param>
        /// <param name="baseUri"></param>
        /// <returns></returns>
        string ResolveCurie(string curie, Uri baseUri);

    }

    /// <summary>
    /// Base implementation of the <see cref="IRdfAVocabulary"/> interface.
    /// </summary>
    public class TermMappings: IRdfAVocabulary
    {
        private readonly Dictionary<string, string> _termMap;
        private readonly NamespaceMapper _namespaceMapper;

        /// <inheritdoc />
        public string VocabularyUri { get; set; } = string.Empty;

        /// <summary>
        /// Create a new empty vocabulary.
        /// </summary>
        public TermMappings()
        {
            _termMap = new Dictionary<string, string>();
            _namespaceMapper = new NamespaceMapper(true);
        }

        /// <summary>
        /// Create a new vocabulary with the specified URI, terms and namespace prefixes.
        /// </summary>
        /// <param name="vocabularyUri">The base URI for the vocabulary. Used to resolve terms to IRIs.</param>
        /// <param name="terms">The list of terms defined by the vocabulary.</param>
        /// <param name="prefixMappings">The namespace prefixes defined by the vocabulary.</param>
        public TermMappings(Uri vocabularyUri, IEnumerable<string> terms = null,
            IEnumerable<KeyValuePair<string, Uri>> prefixMappings = null)
        {
            _termMap = terms?.ToDictionary(t=>t.ToLowerInvariant(), t=>vocabularyUri + t) ?? new Dictionary<string, string>();
            _namespaceMapper = new NamespaceMapper(true);
            if (prefixMappings != null)
            {
                foreach (KeyValuePair<string, Uri> entry in prefixMappings)
                {
                    _namespaceMapper.AddNamespace(entry.Key, entry.Value);
                }
            }
        }

        /// <summary>
        /// Create a new vocabulary from an existing vocabulary.
        /// </summary>
        /// <param name="vocabulary">The base vocabulary to copy.</param>
        public TermMappings(IRdfAVocabulary vocabulary)
        {
            VocabularyUri = vocabulary.VocabularyUri;
            _namespaceMapper = new NamespaceMapper(vocabulary.NamespaceMap);
            if (vocabulary is TermMappings mappings)
            {
                _termMap = new Dictionary<string, string>(mappings._termMap);
            }
            else
            {
                _termMap = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> entry in vocabulary.Mappings)
                {
                    _termMap[entry.Key.ToLowerInvariant()] = entry.Value;
                }
            }
        }

        /// <inheritdoc />
        public bool HasTerm(string term)
        {
            return _termMap.ContainsKey(term.ToLowerInvariant());
        }

        /// <inheritdoc />
        public string ResolveTerm(string term)
        {
            var lcTerm = term.ToLowerInvariant();
            if (_termMap.TryGetValue(lcTerm, out var mapping))
            {
                return mapping;
            }

            if (!string.IsNullOrEmpty(VocabularyUri))
            {
                return VocabularyUri + lcTerm;
            }
            return null;
        }

        /// <inheritdoc />
        public void AddTerm(string term, string uri)
        {
            _termMap[term.ToLowerInvariant()] = uri;
        }

        /// <inheritdoc />
        public void AddNamespace(string prefix, string nsUri)
        {
            _namespaceMapper.AddNamespace(prefix, new Uri(nsUri));
        }

        /// <inheritdoc />
        public void Merge(IRdfAVocabulary vocab)
        {
            foreach (KeyValuePair<string, string> mapping in vocab.Mappings)
            {
                _termMap[mapping.Key] = mapping.Value;
            }

            foreach (var importPrefix in vocab.NamespaceMap.Prefixes)
            {
                _namespaceMapper.AddNamespace(importPrefix, vocab.NamespaceMap.GetNamespaceUri(importPrefix));
            }
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, string>> Mappings => _termMap;

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, string>> Namespaces => _namespaceMapper.Prefixes.Select(p=> new KeyValuePair<string, string>(p, _namespaceMapper.GetNamespaceUri(p).AbsoluteUri));

        /// <inheritdoc />
        public INamespaceMapper NamespaceMap => _namespaceMapper;
        /// <inheritdoc />
        public string ResolveCurie(string curie, Uri baseUri)
        {
            return Tools.ResolveQName(curie, _namespaceMapper, baseUri);
        }
    }

    /// <summary>
    /// Vocabulary for XHTML+RDFa (and HTML+RDFa).
    /// </summary>
    public class XHtmlRdfAVocabulary : TermMappings
    {
        /// <summary>
        /// Construct a new XHTML+RDFa vocabulary definition.
        /// </summary>
        public XHtmlRdfAVocabulary():base(new Uri(RdfAParser.XHtmlVocabNamespace), Terms, 
            VocabNamespaces) {}
        private static readonly string[] Terms = new[]
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

        private static readonly Dictionary<string, Uri> VocabNamespaces = new()
        {
            { "dcat", new Uri("http://www.w3.org/ns/dcat#") },
            { "grddl", new Uri("http://www.w3.org/2003/g/data-view#") },
            { "as", new Uri("https://www.w3.org/ns/activitystreams#") },
            { "duv", new Uri("https://www.w3.org/ns/duv#") },
            { "csvw", new Uri("http://www.w3.org/ns/csvw#") },
            { "odrl", new Uri("http://www.w3.org/ns/odrl/2/") },
            { "oa", new Uri("http://www.w3.org/ns/oa#") },
            { "ma", new Uri("http://www.w3.org/ns/ma-ont#") },
            { "dqv", new Uri("http://www.w3.org/ns/dqv#") },
            { "org", new Uri("http://www.w3.org/ns/org#") },
            { "prov", new Uri("http://www.w3.org/ns/prov#") },
            { "ldp", new Uri("http://www.w3.org/ns/ldp#") },
            { "qb", new Uri("http://purl.org/linked-data/cube#") },
            { "rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#") },
            { "owl", new Uri("http://www.w3.org/2002/07/owl#") },
            { "rdfa", new Uri("http://www.w3.org/ns/rdfa#") },
            { "sd", new Uri("http://www.w3.org/ns/sparql-service-description#") },
            { "jsonld", new Uri("http://www.w3.org/ns/json-ld#") },
            { "skosxl", new Uri("http://www.w3.org/2008/05/skos-xl#") },
            { "time", new Uri("http://www.w3.org/2006/time#") },
            { "sosa", new Uri("http://www.w3.org/ns/sosa/") },
            { "ssn", new Uri("http://www.w3.org/ns/ssn/") },
            { "void", new Uri("http://rdfs.org/ns/void#") },
            { "wdr", new Uri("http://www.w3.org/2007/05/powder#") },
            { "wdrs", new Uri("http://www.w3.org/2007/05/powder-s#") },
            { "xml", new Uri("http://www.w3.org/XML/1998/namespace") },
            { "xsd", new Uri("http://www.w3.org/2001/XMLSchema#") },
            { "xhv", new Uri("http://www.w3.org/1999/xhtml/vocab#") },
            { "rr", new Uri("http://www.w3.org/ns/r2rml#") },
            { "rdfs", new Uri("http://www.w3.org/2000/01/rdf-schema#") },
            { "dc", new Uri("http://purl.org/dc/terms/") },
            { "cc", new Uri("http://creativecommons.org/ns#") },
            { "ctag", new Uri("http://commontag.org/ns#") },
            { "dcterms", new Uri("http://purl.org/dc/terms/") },
            { "dc11", new Uri("http://purl.org/dc/elements/1.1/") },
            { "gr", new Uri("http://purl.org/goodrelations/v1#") },
            { "rev", new Uri("http://purl.org/stuff/rev#") },
            { "foaf", new Uri("http://xmlns.com/foaf/0.1/") },
            { "v", new Uri("http://rdf.data-vocabulary.org/#") },
            { "og", new Uri("http://ogp.me/ns#") },
            { "sioc", new Uri("http://rdfs.org/sioc/ns#") },
            { "schema", new Uri("http://schema.org/") },
            { "ical", new Uri("http://www.w3.org/2002/12/cal/icaltzd#") },
            { "vcard", new Uri("http://www.w3.org/2006/vcard/ns#") },
            { "skos", new Uri("http://www.w3.org/2004/02/skos/core#") },
            { "rif", new Uri("http://www.w3.org/2007/rif#") },
        };
    }

    /*/// <summary>
    /// Represents a dynamic vocabulary for RDFa.
    /// </summary>
    public class TermMappings : BaseRdfAVocabulary
    {
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
        public TermMappings(string vocabUri) : base(new Uri(vocabUri), Array.Empty<string>(), Array.Empty<KeyValuePair<string, Uri>>())
        {
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
                throw new RdfParseException("The Term '" + term + "' cannot be resolved to a valid URI as it is not a term in this vocabulary nor is there a vocabulary URI defined");
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
    }*/
}
