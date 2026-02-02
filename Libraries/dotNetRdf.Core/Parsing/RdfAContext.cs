/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Parsing.Contexts;

namespace VDS.RDF.Parsing;

/// <summary>
/// Base implementation of the <see cref="IRdfAContext"/> interface.
/// </summary>
public class RdfAContext: IRdfAContext
{
    private readonly Dictionary<string, string> _termMap;
    private readonly NamespaceMapper _namespaceMapper;

    /// <inheritdoc />
    public string VocabularyUri { get; set; } = string.Empty;

    /// <summary>
    /// Create a new empty vocabulary.
    /// </summary>
    public RdfAContext()
    {
        _termMap = [];
        _namespaceMapper = new NamespaceMapper(true);
    }

    /// <summary>
    /// Create a new RDFa context with the specified default vocabulary URI, term and prefix mappings.
    /// </summary>
    /// <param name="vocabularyUri">The default vocabulary URI. If null, the context will be defined with no default vocabulary URI.</param>
    /// <param name="terms">An enumeration of key value pairs mapping each term to the URI that the term maps to. May be null.</param>
    /// <param name="prefixes">An enumeration of key value pairs mapping each prefix to the URI that the prefix maps to. May be null.</param>
    public RdfAContext(string vocabularyUri, IEnumerable<KeyValuePair<string, string>> terms,
        IEnumerable<KeyValuePair<string, string>> prefixes)
    {
        VocabularyUri = vocabularyUri;
        _termMap = terms?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? [];
        _namespaceMapper = new NamespaceMapper(true);
        if (prefixes != null)
        {
            foreach (KeyValuePair<string, string> prefixMapping in prefixes)
            {
                _namespaceMapper.AddNamespace(prefixMapping.Key, new Uri(prefixMapping.Value));
            }
        }
    }

    /// <summary>
    /// Create a new vocabulary from an existing vocabulary.
    /// </summary>
    /// <param name="context">The base vocabulary to copy.</param>
    public RdfAContext(IRdfAContext context)
    {
        VocabularyUri = context.VocabularyUri;
        _namespaceMapper = new NamespaceMapper(context.NamespaceMap);
        if (context is RdfAContext mappings)
        {
            _termMap = new Dictionary<string, string>(mappings._termMap);
        }
        else
        {
            _termMap = [];
            foreach (KeyValuePair<string, string> entry in context.Mappings)
            {
                _termMap[entry.Key.ToLowerInvariant()] = entry.Value;
            }
        }
    }

    /// <summary>
    /// Create a new vocabulary that extends an existing vocabulary and add terms and prefix mappings to it.
    /// </summary>
    /// <param name="baseContext"></param>
    /// <param name="vocabularyUri"></param>
    /// <param name="terms"></param>
    /// <param name="prefixMappings"></param>
    public RdfAContext(IRdfAContext baseContext, string vocabularyUri, IEnumerable<KeyValuePair<string, string>> terms = null,
        IEnumerable<KeyValuePair<string, Uri>> prefixMappings = null) : this(baseContext)
    {
        VocabularyUri = vocabularyUri;
        if (terms != null)
        {
            foreach (KeyValuePair<string, string> entry in terms)
            {
                AddTerm(entry.Key, entry.Value);
            }
        }

        if (prefixMappings != null)
        {
            foreach (KeyValuePair<string, Uri> mapping in prefixMappings)
            {
                if (_namespaceMapper.HasNamespace(mapping.Key))
                {
                    _namespaceMapper.RemoveNamespace(mapping.Key);
                }
                _namespaceMapper.AddNamespace(mapping.Key, mapping.Value);
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
            return VocabularyUri + term;
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
    public void Merge(IRdfAContext vocab)
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

    /// <summary>
    /// Create a new RDFa context by processing the triples in the specified graph.
    /// </summary>
    /// <param name="g">The graph to process.</param>
    /// <returns>A new <see cref="IRdfAContext"/> instance containing the vocabulary declaration, term and prefix mappings defined in the graph.</returns>
    public static IRdfAContext Load(IGraph g)
    {
        IEnumerable<KeyValuePair<string, string>> prefixMappings = GetPrefixMappings(g);
        IEnumerable<KeyValuePair<string, string>> termMappings = GetTermMappings(g);
        var vocabularyUri = GetVocabularyUri(g);
        return new RdfAContext(vocabularyUri, termMappings, prefixMappings);
    }

    private static IEnumerable<KeyValuePair<string, string>> GetPrefixMappings(IGraph g)
    {
        IUriNode prefix = g.GetUriNode(g.UriFactory.Create(RdfASpecsHelper.RdfAPrefix));
        IUriNode uri = g.GetUriNode(g.UriFactory.Create(RdfASpecsHelper.RdfAUri));
        if (prefix == null || uri == null)
        {
            // Graph contains no mappings
            return [];
        }
        return GetMappings(prefix, uri, g);
    }

    private static IEnumerable<KeyValuePair<string, string>> GetTermMappings(IGraph g)
    {
        IUriNode term = g.GetUriNode(g.UriFactory.Create(RdfASpecsHelper.RdfATerm));
        IUriNode uri = g.GetUriNode(g.UriFactory.Create(RdfASpecsHelper.RdfAUri));
        if (term == null || uri == null)
        {
            // Graph contains no mappings
            return [];
        }
        return GetMappings(term, uri, g);
    }

    private static IEnumerable<KeyValuePair<string, string>> GetMappings(INode keyPredicateNode, INode valuePredicateNode,
        IGraph graph)
    {
        IDictionary<INode, INode> prefixByMapNode = graph.GetTriplesWithPredicate(keyPredicateNode)
            .GroupBy(t => t.Subject)
            .Where(grouping => grouping.Count() == 1)
            .ToDictionary(grouping => grouping.Key, grouping => grouping.First().Object);

        IDictionary<INode, INode> uriByMapNode = graph.GetTriplesWithPredicate(valuePredicateNode)
            .GroupBy(t => t.Subject)
            .Where(g => g.Count() == 1)
            .ToDictionary(g => g.Key, g => g.First().Object);

        foreach (INode mappingNode in prefixByMapNode.Keys.Intersect(uriByMapNode.Keys))
        {
            INode prefixNode = prefixByMapNode[mappingNode];
            INode nsNode = uriByMapNode[mappingNode];
            if (prefixNode is ILiteralNode prefixLiteralNode &&
                nsNode is ILiteralNode nsListLiteralNode)
            {
                yield return new KeyValuePair<string, string>(
                    prefixLiteralNode.Value.ToLower(),
                    nsListLiteralNode.Value);
            }
        }
    }

    private static string GetVocabularyUri(IGraph g)
    {
        IUriNode vocabulary = g.GetUriNode(g.UriFactory.Create(RdfASpecsHelper.RdfAVocabulary));
        return g.GetTriplesWithPredicate(vocabulary)
            .Select(t => t.Predicate)
            .OfType<IUriNode>()
            .Select(node => node.Uri.AbsoluteUri)
            .FirstOrDefault();
    }
}