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
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF.Parsing;
    using VDS.RDF.Query;
    using VDS.RDF.Query.Describe;

    public class ShaclValidationReportDescribe : BaseDescribeAlgorithm
    {
        private static readonly NodeFactory factory = new NodeFactory();
        private static readonly INode rdf_first = factory.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListFirst));
        private static readonly INode rdf_rest = factory.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfListRest));

        protected override void DescribeInternal(IRdfHandler handler, SparqlEvaluationContext context, IEnumerable<INode> nodes)
        {
            var bnodeMapping = new Dictionary<string, INode>();
            var map = new Dictionary<INode, INode>();
            var outstanding = new Queue<INode>();
            var done = new HashSet<INode>();

            void process(INode originalSubject, INode mappedSubject = null)
            {
                foreach (var original in context.Data.GetTriplesWithSubject(originalSubject))
                {
                    var @object = original.Object;
                    if (@object.NodeType == NodeType.Blank && !done.Contains(@object))
                    {
                        if (PredicatesToExpand.Contains(original.Predicate))
                        {
                            @object = @object.Graph.CreateBlankNode();
                            map.Add(@object, original.Object);
                            outstanding.Enqueue(@object);
                        }
                    }

                    if (!handler.HandleTriple((RewriteDescribeBNodes(new Triple(mappedSubject ?? originalSubject, original.Predicate, @object, original.Graph), bnodeMapping, handler))))
                    {
                        ParserHelper.Stop();
                    }
                }
            }

            foreach (var node in nodes)
            {
                process(node);

                while (outstanding.Any())
                {
                    var mappedSubject = outstanding.Dequeue();

                    if (done.Add(mappedSubject))
                    {
                        if (map.TryGetValue(mappedSubject, out var originalSubject))
                        {
                            process(originalSubject, mappedSubject);
                        }
                        else
                        {
                            process(mappedSubject);
                        }
                    }
                }
            }
        }

        private static IEnumerable<INode> PredicatesToExpand
        {
            get
            {
                yield return Shacl.Result;
                yield return Shacl.ResultPath;
                yield return Shacl.ResultMessage;
                yield return Shacl.ResultSeverity;

                foreach (var item in Shacl.Paths)
                {
                    yield return item;
                }

                yield return rdf_first;
                yield return rdf_rest;
            }
        }
    }
}
