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
using VDS.RDF.Query.Builder;
using VDS.RDF.Update.Commands;

namespace VDS.RDF
{
    /// <summary>
    /// Provides useful Extension Methods for working with <see cref="GraphDiffReport">graph diff reports</see>
    /// </summary>
    public static class GraphDiffReportExtensions
    {
        private static NodeFactory Factory { get; } = new NodeFactory();

        /// <summary>
        /// Converts a <see cref="GraphDiffReport">diff</see> to an equivalent <see cref="ModifyCommand">SPARQL Update INSERT/DELETE command</see>
        /// </summary>
        /// <param name="diff">The <see cref="GraphDiffReport">diff</see> to convert</param>
        /// <param name="graphUri">Optional <see cref="Uri">URI</see> of the affected graph</param>
        /// <param name="prefixes">Optional <see cref="INamespaceMapper">mapper</see> used to resolve prefixes</param>
        /// <returns>A <see cref="ModifyCommand">SPARQL Update INSERT/DELETE command</see> that represents the <see cref="GraphDiffReport">diff</see></returns>
        public static ModifyCommand AsUpdate(this GraphDiffReport diff, Uri graphUri = null, INamespaceMapper prefixes = null)
        {
            var delete = new GraphPatternBuilder();
            var insert = new GraphPatternBuilder();
            var where = new GraphPatternBuilder();

            // Removed ground triples are added as is to both delete and where clauses
            foreach (var t in diff.RemovedTriples)
            {
                delete.AddTriplePattern(t);
                where.AddTriplePattern(t);
            }

            foreach (var g in diff.RemovedMSGs)
            {
                // Blank nodes in removed non-ground triples are converted to variables and added to both delete and where clauses
                foreach (var t in g.Triples)
                {
                    delete.AddVariablePattern(t);
                    where.AddVariablePattern(t);
                }

                // An ISBLANK filter is added for each blank node in removed non-ground triples
                foreach (var n in g.BlankNodes())
                {
                    where.AddBlankNodeFilter(n);
                }
            }

            // Added triples (ground or not) are added as is to the insert clause
            foreach (var t in diff.AllAddedTriples())
            {
                insert.AddTriplePattern(t);
            }

            return new ModifyCommand(
                delete.BuildGraphPattern(prefixes),
                insert.BuildGraphPattern(prefixes),
                where.BuildGraphPattern(prefixes),
                graphUri);
        }

        private static void AddTriplePattern(this IGraphPatternBuilder builder, Triple t)
        {
            builder
                .Where(tripleBuilder =>
                    tripleBuilder
                        .Subject(t.Subject)
                        .PredicateUri(t.Predicate as IUriNode)
                        .Object(t.Object));
        }

        private static void AddVariablePattern(this IGraphPatternBuilder builder, Triple t)
        {
            builder
                .Where(tripleBuilder =>
                    tripleBuilder
                        .Subject(t.Subject.AsVariable())
                        .PredicateUri(t.Predicate as IUriNode)
                        .Object(t.Object.AsVariable()));
        }

        private static void AddBlankNodeFilter(this IGraphPatternBuilder builder, IBlankNode n)
        {
            builder.Filter(node =>
                node.IsBlank(n.InternalID));
        }

        private static INode AsVariable(this INode n)
        {
            return n is IBlankNode blank ? Factory.CreateVariableNode(blank.InternalID) : n;
        }

        private static IEnumerable<IBlankNode> BlankNodes(this IGraph g)
        {
            return g.Nodes.BlankNodes();
        }

        private static IEnumerable<Triple> AllAddedTriples(this GraphDiffReport diff)
        {
            return diff.AddedMSGs.SelectMany(msg => msg.Triples).Union(diff.AddedTriples);
        }
    }
}
