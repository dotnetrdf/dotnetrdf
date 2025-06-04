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

using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF;

/// <summary>
/// Provides an implementation of the RDF-Star unstar operation that can be shared by both <see cref="BaseGraph"/> and <see cref="GraphPersistenceWrapper"/>.
/// </summary>
internal class RdfStarHelper
{
    /// <summary>
    /// Converts an graph containing quoted triples into to a graph with no quoted triples by
    /// applying the unstar operation described in https://w3c.github.io/rdf-star/cg-spec/2021-12-17.html#mapping.
    /// </summary>
    /// <remarks>The unstar operation modifies the graph in-place by calls to <see cref="IGraph.Assert(VDS.RDF.Triple)"/> an <see cref="IGraph.Retract(VDS.RDF.Triple)"/>.</remarks>
    public static void Unstar(IGraph g)
    {
        UpdateUnstarNodes(g);
        ReplaceQuotedTriples(g);
    }

    /// <summary>
    /// Replace any existing IRIs in the namespace `https://w3c.github.io/rdf-star/unstar#` with the IRI with an underscore appended.
    /// </summary>
    private static void UpdateUnstarNodes(IGraph g)
    {
        foreach (Triple t in g.Triples.Asserted.Union(g.Triples.Quoted).Where(t => t.Nodes.Any(IsUnstarNode)).ToList())
        {
            g.Retract(t);
            g.Assert(new Triple(UpdateUnstarNodes(g, t.Subject), UpdateUnstarNodes(g, t.Predicate), UpdateUnstarNodes(g, t.Object), t.Context));
        }
    }

    /// <summary>
    /// Determine if a node is a URI node in the `https://w3c.github.io/rdf-star/unstar#` namespace.
    /// </summary>
    /// <param name="n">The node to be tested.</param>
    /// <returns></returns>
    private static bool IsUnstarNode(INode n)
    {
        return n is IUriNode un && un.Uri.AbsoluteUri.StartsWith("https://w3c.github.io/rdf-star/unstar#");
    }

    private static INode UpdateUnstarNodes(INodeFactory g, INode n)
    {
        if (IsUnstarNode(n))
        {
            return g.CreateUriNode(UriFactory.Create((n as IUriNode)?.Uri.AbsoluteUri + "_")) ?? n;
        }

        return n;
    }

    /// <summary>
    /// Replace each triple node in the graph with a blank node with properties in the `https://w3c.github.io/rdf-star/unstar#` namespace.
    /// </summary>
    private static void ReplaceQuotedTriples(IGraph g)
    {
        var mappings = new Dictionary<ITripleNode, IBlankNode>(new FastNodeComparer());
        IUriNode unstarSubject = g.CreateUriNode(UriFactory.Create("https://w3c.github.io/rdf-star/unstar#subject"));
        IUriNode unstarPredicate = g.CreateUriNode(UriFactory.Create("https://w3c.github.io/rdf-star/unstar#predicate"));
        IUriNode unstarObject = g.CreateUriNode(UriFactory.Create("https://w3c.github.io/rdf-star/unstar#object"));
        IUriNode unstarSubjectLexical = g.CreateUriNode(UriFactory.Create("https://w3c.github.io/rdf-star/unstar#subjectLexical"));
        IUriNode unstarPredicateLexical = g.CreateUriNode(UriFactory.Create("https://w3c.github.io/rdf-star/unstar#predicateLexical"));
        IUriNode unstarObjectLexical = g.CreateUriNode(UriFactory.Create("https://w3c.github.io/rdf-star/unstar#objectLexical"));
        var lexicalFormatter = new NTriples11Formatter();

        // First assign a new blank node to each distinct triple node in the graph.
        var tripleNodes = g.Nodes.OfType<ITripleNode>().Union(g.QuotedNodes.OfType<ITripleNode>()).ToList();
        foreach (ITripleNode tn in tripleNodes)
        {
            if (!mappings.ContainsKey(tn))
            {
                IBlankNode b = g.CreateBlankNode();
                mappings[tn] = b;
            }
        }

        // Now record the unstar triples for each triple node, taking into account that
        // the subject or object of a triple node may itself be a triple node that is mapped to a blank node.
        foreach (ITripleNode tn in tripleNodes)
        {
            IBlankNode b = mappings[tn];
            IBlankNode mappedSubject = null, mappedObject = null;
            if (tn.Triple.Subject is ITripleNode stn)
            {
                mappings.TryGetValue(stn, out mappedSubject);
            }

            if (tn.Triple.Object is ITripleNode otn)
            {
                mappings.TryGetValue(otn, out mappedObject);
            }

            g.Assert(new[]
            {
                new Triple(b, unstarSubject, mappedSubject ?? tn.Triple.Subject),
                new Triple(b, unstarPredicate, tn.Triple.Predicate),
                new Triple(b, unstarObject, mappedObject ?? tn.Triple.Object),
            });

            if (mappedSubject == null && tn.Triple.Subject.NodeType != NodeType.Blank)
            {
                g.Assert(new Triple(b, unstarSubjectLexical,
                    new LiteralNode(lexicalFormatter.Format(tn.Triple.Subject), false)));
            }

            g.Assert(new Triple(b, unstarPredicateLexical,
                new LiteralNode(lexicalFormatter.Format(tn.Triple.Predicate), false)));

            if (mappedObject == null && tn.Triple.Object.NodeType != NodeType.Blank)
            {
                g.Assert(new Triple(b, unstarObjectLexical,
                    new LiteralNode(lexicalFormatter.Format(tn.Triple.Object), false)));
            }

        }

        // Now replace triple nodes with their mapped blank nodes in all asserted triples where they occur.
        var toReplace = g.Triples
            .Where(t =>
                (t.Subject is ITripleNode stn && mappings.ContainsKey(stn)) ||
                (t.Object is ITripleNode otn && mappings.ContainsKey(otn)))
            .ToList();
        foreach (Triple t in toReplace)
        {
            IBlankNode subjectReplacement = null, objectReplacement = null;
            var replaceSubject = t.Subject is ITripleNode stn && mappings.TryGetValue(stn, out subjectReplacement);
            var replaceObject = t.Object is ITripleNode otn && mappings.TryGetValue(otn, out objectReplacement);
            if (replaceSubject || replaceObject)
            {
                g.Retract(t);
                g.Assert(new Triple(subjectReplacement ?? t.Subject, t.Predicate, objectReplacement ?? t.Object,
                    t.Context));
            }
        }
    }
}
