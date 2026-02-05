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

// dotNetRDF is free and open source software licensed under the MIT License
//
// -----------------------------------------------------------------------------
//
// Copyright (c) [InvalidReference] dotNetRDF Project
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VDS.RDF.JsonLd;
using VDS.RDF.JsonLd.Processors;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF;

/// <summary>
/// Implementation of the RDFC-1.0 algorithm. Transforms a RDF graph into its canonical form.
/// </summary>
/// <param name="hashAlgorithm"></param>
public class RdfCanonicalizer(string hashAlgorithm = "SHA256")
{
    private const string CanonicalIdentifier = "c14n";
    private readonly BlankNodeGenerator _canonicalIssuer = new(CanonicalIdentifier);
    private readonly MultiValueDictionary<string, Quad> _blankNodeToQuadsMap = new();
    private readonly MultiValueDictionary<string, string> _hashToBlankNodesMap = new();
    private readonly HashAlgorithmName _hashAlgorithm = new(hashAlgorithm);
    private int _nquadsRecursionLimit = 1000;

    /// <summary>
    /// This implements RDFC-1.0 (https://www.w3.org/TR/rdf-canon).
    /// </summary>
    /// <param name="inputDataset"></param>
    /// <returns></returns>
    public CanonicalizedRdfDataset Canonicalize(ITripleStore inputDataset)
    {
        // 1) Create the canonicalization state (done in constructor).
        // Note: Since our input will always be a dataset, we do not need to parse n-quads syntax.

        var outputDataset = new TripleStore();
        var inputDatasetEnum = inputDataset.Graphs.ToList();

        // 2) For every quad (generated here using LINQ) in the input dataset
        // 2.1) For every blank node of the quad, add an entry to _blankNodeToQuadsMap linking them
        inputDatasetEnum.SelectMany(graph =>
                graph.Triples.SelectMany(triple =>
                    triple.Nodes.Concat([graph.Name]).OfType<BlankNode>().Select(node => (graph, triple, node))))
            .ToList().ForEach(item =>
                _blankNodeToQuadsMap.Add(item.node.InternalID, new Quad(item.triple, item.graph.Name)));

        // 3) For each key in _blankNodeToQuadsMap
        foreach (var identifier in _blankNodeToQuadsMap.Keys.ToList())
        {
            // 3.1) Create a hash using HashFirstDegreeQuads
            var hash = HashFirstDegreeQuads(identifier);
            // 3.2) Add the an entry to _hashToBlankNodesMap
            _hashToBlankNodesMap.Add(hash, identifier);
        }

        // 4) For each entry of _hashToBlankNodesMap, code point ordered by hash
        // 4.1) Skipping entries that have more than one value
        foreach (KeyValuePair<string, List<string>> entry in _hashToBlankNodesMap
                     .OrderBy(p => p.Key, StringComparer.Ordinal).Where(pair => pair.Value.Count <= 1))
        {
            // 4.2) Issue a canonical identifier for the blank node
            _canonicalIssuer.GenerateBlankNodeIdentifier(entry.Value[0]);
            // 4.3) Remove the entry from _hashToBlankNodesMap
            _hashToBlankNodesMap.Remove(entry.Value[0]);
        }

        // 5) For each entry of _hashToBlankNodesMap, code point ordered by hash 
        foreach (KeyValuePair<string, List<string>> entry in _hashToBlankNodesMap.OrderBy(p => p.Key,
                     StringComparer.Ordinal))
        {
            // 5.1) Create hashPathList where each item will be a result of running HashNDegreeQuads
            var hashPathList = new List<(string hash, IBlankNodeGenerator issuer)>();

            // 5.2) For each blank node identifier in the map entry 
            foreach (var blankNodeIdentifier in entry.Value)
            {
                // 5.2.1) If a canonical identifier has already been issued, continue to the next blank node identifier
                if (blankNodeIdentifier.StartsWith($"_:{CanonicalIdentifier}")) continue;
                // 5.2.2) Create temporary issuer, an identifier issuer initialized with the prefix 'b' (the default)
                var tempIssuer = new BlankNodeGenerator();
                // 5.2.3) Generate a temporary blank node identifier using tempIssuer
                tempIssuer.GenerateBlankNodeIdentifier(blankNodeIdentifier);
                // 5.2.4) Run HashNDegreeQuads for the blank node identifier, passing the other variables we just set up
                hashPathList.Add((
                    HashNDegreeQuads(blankNodeIdentifier, tempIssuer, out IBlankNodeGenerator resultIssuer),
                    resultIssuer));
            }

            // 5.3) For each entry of hashPathList, code point ordered by the hash
            // 5.3.1a) For each blank node identifier issued by the temporary identifier (aggregated here using LINQ)
            foreach (var identifier in hashPathList.OrderBy(p => p.hash, StringComparer.Ordinal)
                         .SelectMany(result => result.issuer.GetMappedIdentifiers()))
            {
                // 5.3.1b) generate a canonical identifier
                _canonicalIssuer.GenerateBlankNodeIdentifier(identifier);
            }
        }

        // Generate the canonicalized dataset
        foreach (IGraph graph in inputDatasetEnum)
        {
            IRefNode graphName = graph.Name is BlankNode graphBlankNode
                ? new BlankNode(_canonicalIssuer.GenerateBlankNodeIdentifier(graphBlankNode.InternalID).Substring(2))
                : graph.Name;

            var nGraph = new Graph(graphName);
            foreach (Triple triple in graph.Triples)
            {
                INode subj = triple.Subject is BlankNode subjBlankNode
                    ? new BlankNode(_canonicalIssuer.GenerateBlankNodeIdentifier(subjBlankNode.InternalID).Substring(2))
                    : triple.Subject;

                INode pred = triple.Predicate is BlankNode predBlankNode
                    ? new BlankNode(_canonicalIssuer.GenerateBlankNodeIdentifier(predBlankNode.InternalID).Substring(2))
                    : triple.Predicate;

                INode obj = triple.Object is BlankNode objBlankNode
                    ? new BlankNode(_canonicalIssuer.GenerateBlankNodeIdentifier(objBlankNode.InternalID).Substring(2))
                    : triple.Object;

                var nTriple = new Triple(subj, pred, obj);
                nGraph.Triples.Add(nTriple);
            }

            outputDataset.Add(nGraph, true);
        }

        return new CanonicalizedRdfDataset(inputDataset, outputDataset, _canonicalIssuer.GetDictionary());
    }

    private string HashFirstDegreeQuads(string identifier)
    {
        var formatter = new NQuads11Formatter();

        IOrderedEnumerable<string> nquads = _blankNodeToQuadsMap[identifier]
            .Select(quad => PrepareQuadForHash(quad, identifier))
            .Select(quad => formatter.Format(quad.Triple, quad.Graph) + '\n')
            .OrderBy(p => p, StringComparer.Ordinal);

        return MultiHash(nquads);
    }

    private string HashNDegreeQuads(string identifier, IBlankNodeGenerator issuer, out IBlankNodeGenerator issuerResult)
    {
        if (_nquadsRecursionLimit-- <= 0) throw new Exception("Recursion limit reached");

        var relatedHashToBlankNodesMap = new MultiValueDictionary<string, string>();
        foreach (Quad quad in _blankNodeToQuadsMap[identifier])
        {
            ProcessRelatedComponent(quad.Subject, "s", identifier, quad, issuer, ref relatedHashToBlankNodesMap);
            ProcessRelatedComponent(quad.Object, "o", identifier, quad, issuer, ref relatedHashToBlankNodesMap);
            ProcessRelatedComponent(quad.Graph, "g", identifier, quad, issuer, ref relatedHashToBlankNodesMap);
        }

        List<string> dataToHash = [];

        foreach (KeyValuePair<string, List<string>> mapping in relatedHashToBlankNodesMap.OrderBy(p => p.Key,
                     StringComparer.Ordinal))
        {
            dataToHash.Add(mapping.Key);
            var chosenPath = string.Empty;
            IBlankNodeGenerator chosenIssuer = null;

            foreach (IList<string> permutation in Permute(mapping.Value))
            {
                IBlankNodeGenerator issuerCopy = issuer.Clone();
                var path = string.Empty;
                var recursionList = new List<string>();

                foreach (var related in permutation)
                {
                    var canonicalId = _canonicalIssuer.GetMappedIdentifier(related);
                    if (canonicalId != null)
                    {
                        path += canonicalId;
                    }
                    else
                    {
                        if (issuerCopy.GetMappedIdentifier(related) == null)
                            recursionList.Add(related);
                        path += issuerCopy.GenerateBlankNodeIdentifier(related);
                    }

                    if (!string.IsNullOrEmpty(chosenPath) && path.Length >= chosenPath.Length &&
                        StringGreaterThan(path, chosenPath))
                    {
                        goto nextPerm;
                    }
                }

                foreach (var related in recursionList)
                {
                    var result = HashNDegreeQuads(related, issuerCopy, out IBlankNodeGenerator issuerCopyRecurse);
                    path += issuerCopy.GenerateBlankNodeIdentifier(related);
                    path += $"<{result}>";
                    issuerCopy = issuerCopyRecurse;
                    if (!string.IsNullOrEmpty(chosenPath) && path.Length >= chosenPath.Length &&
                        StringGreaterThan(path, chosenPath))
                    {
                        goto nextPerm;
                    }
                }

                if (!string.IsNullOrEmpty(chosenPath) && !StringLessThan(path, chosenPath))
                    continue;

                chosenPath = path;
                chosenIssuer = issuerCopy;

                nextPerm: ;
            }

            dataToHash.Add(chosenPath);
            issuer = chosenIssuer;
        }

        issuerResult = issuer;
        return MultiHash(dataToHash);
    }

    private static bool StringGreaterThan(string a, string b) => StringCompare(a, b) > 0;
    private static bool StringLessThan(string a, string b) => StringCompare(a, b) < 0;

    private static int StringCompare(string a, string b) => StringComparer.Ordinal.Compare(a, b);

    private void ProcessRelatedComponent(INode node, string position, string identifier, Quad quad,
        IBlankNodeGenerator issuer,
        ref MultiValueDictionary<string, string> hashToRelatedBlankNodesMap)
    {
        if (node is not IBlankNode blankNode) return;
        if (blankNode.InternalID == identifier) return;

        var hash = HashRelatedBlankNode(blankNode.InternalID, quad, issuer, position);
        hashToRelatedBlankNodesMap.Add(hash, blankNode.InternalID);
    }

    private string HashRelatedBlankNode(string relatedBlankNodeIdentifier, Quad quad,
        IBlankNodeGenerator issuer, string position)
    {
        var identifier = _canonicalIssuer.GetMappedIdentifier(relatedBlankNodeIdentifier) ??
                         issuer.GetMappedIdentifier(relatedBlankNodeIdentifier) ??
                         HashFirstDegreeQuads(relatedBlankNodeIdentifier);

        var input = position;
        if (position != "g") input += $"<{quad.Predicate}>";
        input += identifier;

        return MultiHash([input]);
    }

    private string MultiHash(IEnumerable<string> inputs)
    {
        var hash = IncrementalHash.CreateHash(_hashAlgorithm);

        foreach (var input in inputs)
        {
            hash.AppendData(Encoding.UTF8.GetBytes(input));
        }

        var hashBuilder = new StringBuilder();
        foreach (var b in hash.GetHashAndReset())
            hashBuilder.Append(b.ToString("x2"));

        return hashBuilder.ToString();
    }

    private static Quad PrepareQuadForHash(Quad quad, string referenceId)
    {
        IRefNode graph = quad.Graph is BlankNode blankRefNode
            ? new BlankNode(blankRefNode.InternalID == referenceId ? "a" : "z")
            : quad.Graph;

        INode subj = quad.Subject is BlankNode blankSubjectNode
            ? new BlankNode(blankSubjectNode.InternalID == referenceId ? "a" : "z")
            : quad.Subject;

        INode pred = quad.Predicate is BlankNode blankPredicateNode
            ? new BlankNode(blankPredicateNode.InternalID == referenceId ? "a" : "z")
            : quad.Predicate;

        INode obj = quad.Object is BlankNode blankObjectNode
            ? new BlankNode(blankObjectNode.InternalID == referenceId ? "a" : "z")
            : quad.Object;

        return new Quad(new Triple(subj, pred, obj), graph);
    }

    private class MultiValueDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, List<TValue>>>
        where TKey : notnull
    {
        private readonly Dictionary<TKey, List<TValue>> _dictionary = [];

        public void Add(TKey key, TValue value)
        {
            if (_dictionary.TryGetValue(key, out List<TValue> list))
            {
                list.Add(value);
            }
            else
            {
                _dictionary.Add(key, [value]);
            }
        }

        public IEnumerable<TValue> this[TKey key] => _dictionary[key];
        public IEnumerable<TKey> Keys => _dictionary.Keys;
        public IEnumerable<TValue> Values => _dictionary.Values.SelectMany(p => p);

        IEnumerator<KeyValuePair<TKey, List<TValue>>> IEnumerable<KeyValuePair<TKey, List<TValue>>>.GetEnumerator() =>
            _dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

        public void Clear() => _dictionary.Clear();
        public void Remove(TKey key) => _dictionary.Remove(key);
    }

    private class Quad(Triple triple, IRefNode graph)
    {
        internal readonly INode Subject = triple.Subject;
        internal readonly INode Predicate = triple.Predicate;
        internal readonly INode Object = triple.Object;
        internal readonly IRefNode Graph = graph;
        internal Triple Triple => new(Subject, Predicate, Object);
    }

    private static void RotateRight<T>(IList<T> sequence, int count)
    {
        T tmp = sequence[count - 1];
        sequence.RemoveAt(count - 1);
        sequence.Insert(0, tmp);
    }

    private static IEnumerable<IList<T>> Permute<T>(IList<T> sequence) => Permute(sequence, sequence.Count);

    private static IEnumerable<IList<T>> Permute<T>(IList<T> sequence, int count)
    {
        if (count == 1) yield return sequence;
        else
        {
            for (var i = 0; i < count; i++)
            {
                foreach (IList<T> perm in Permute(sequence, count - 1))
                    yield return perm;
                RotateRight(sequence, count);
            }
        }
    }

    /// <summary>
    /// A canonicalized RDF dataset, as per https://www.w3.org/TR/rdf-canon/#dfn-canonicalized-dataset.
    /// It does not contain the input blank node identifier map, as n-quads inputs are not currently supported.
    /// </summary>
    /// <remarks>
    /// For convenience, this also contains the serialized n-quads format, as well as the output dataset.
    /// </remarks>
    /// <param name="inputDataset"></param>
    /// <param name="outputDataset"></param>
    /// <param name="issuedIdentifiersMap"></param>
    public class CanonicalizedRdfDataset(
        ITripleStore inputDataset,
        ITripleStore outputDataset,
        IDictionary<string, string> issuedIdentifiersMap)
    {
        /// <summary>
        /// The dataset, serialized in canonical n-quads form.
        /// </summary>
        public string SerializedNQuads
        {
            get
            {
                var sb = new StringBuilder();
                var formatter = new NQuads11Formatter();
                OutputDataset.Graphs
                    .SelectMany(graph => graph.Triples.Select(triple => formatter.Format(triple, graph.Name)))
                    .OrderBy(p => p, StringComparer.Ordinal).ToList().ForEach(s =>
                    {
                        sb.Append(s);
                        sb.Append("\n");
                    });
                return sb.ToString();
            }
        }

        /// <summary>
        /// The input dataset.
        /// </summary>
        public readonly ITripleStore InputDataset = inputDataset;

        /// <summary>
        /// The canonicalized output dataset.
        /// </summary>
        public readonly ITripleStore OutputDataset = outputDataset;

        /// <summary>
        /// The mapping between original blank node identifiers and their canonical counterparts.
        /// </summary>
        public readonly IDictionary<string, string> IssuedIdentifiersMap = issuedIdentifiersMap;
    }
    
}