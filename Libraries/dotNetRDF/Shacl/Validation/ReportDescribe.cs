/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF.Shacl.Validation
{
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF.Parsing;
    using VDS.RDF.Query;
    using VDS.RDF.Query.Describe;

    internal class ReportDescribeAlgorithm : BaseDescribeAlgorithm
    {
        protected override void DescribeInternal(IRdfHandler handler, SparqlEvaluationContext context, IEnumerable<INode> nodes)
        {
            var bnodeMapping = new Dictionary<string, INode>();
            var map = new Dictionary<INode, INode>();
            var outstanding = new Queue<INode>();
            var done = new HashSet<INode>();

            void process(INode originalSubject, INode mappedSubject = null)
            {
                foreach (var t in context.Data.GetTriplesWithSubject(originalSubject))
                {
                    var @object = t.Object;
                    if (@object.NodeType == NodeType.Blank && !done.Contains(@object))
                    {
                        if (Vocabulary.PredicatesToExpandInReport.Contains(t.Predicate))
                        {
                            @object = @object.Graph.CreateBlankNode();
                            map.Add(@object, t.Object);
                            outstanding.Enqueue(@object);
                        }
                    }

                    if (!handler.HandleTriple(RewriteDescribeBNodes(new Triple(mappedSubject ?? originalSubject, t.Predicate, @object, t.Graph), bnodeMapping, handler)))
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
    }
}
