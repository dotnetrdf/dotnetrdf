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

namespace VDS.RDF.Shacl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF.Query;
    using VDS.RDF.Query.Builder;
    using VDS.RDF.Query.Paths;
    using VDS.RDF.Query.Patterns;

    internal abstract class ShaclPath : WrapperNode
    {
        protected ShaclPath(INode node)
            : base(node)
        {
        }

        internal abstract ISparqlPath SparqlPath { get; }

        internal IEnumerable<INode> SelectFocusNodes(INode node)
        {
            var a = QueryBuilder.Select("x").Distinct().Where(new PropertyPathPattern(new NodeMatchPattern(node), this.SparqlPath, new VariablePattern("x"))).BuildQuery();
            var b = node.Graph.ExecuteQuery(a);

            return ((SparqlResultSet)b).Select(x => x["x"]);
        }

        internal static ShaclPath Parse(INode node)
        {
            if (node.IsListRoot(node.Graph))
            {
                return new ShaclSequencePath(node);
            }

            if (node is IBlankNode)
            {
                var predicate = node.Graph.GetTriplesWithSubject(node).Single().Predicate;

                var paths = new Dictionary<INode, Func<INode, ShaclPath>>()
                {
                    { Shacl.AlternativePath, n => new ShaclAlternativePath(n) },
                    { Shacl.InversePath, n => new ShaclInversePath(n) },
                    { Shacl.ZeroOrMorePath, n => new ShaclZeroOrMorePath(n) },
                    { Shacl.OneOrMorePath, n => new ShaclOneOrMorePath(n) },
                    { Shacl.ZeroOrOnePath, n => new ShaclZeroOrOnePath(n) },
                };

                return paths[predicate](node);
            }

            return new ShaclPredicatePath(node);
        }
    }
}