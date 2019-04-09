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
    using System.Text.RegularExpressions;
    using VDS.RDF.Nodes;
    using VDS.RDF.Query;
    using VDS.RDF.Query.Paths;
    using VDS.RDF.Query.Patterns;

    internal class ShaclSparqlSelectConstraint : ShaclSparqlConstraint
    {
        public ShaclSparqlSelectConstraint(ShaclShape shape, INode value)
            : base(shape, value)
        {
        }

        public ShaclSparqlSelectConstraint(ShaclShape shape, INode value, IEnumerable<KeyValuePair<string, INode>> parameters)
            : base(shape, value, parameters)
        {
        }

        protected override string Query
        {
            get
            {
                var query = Shacl.Select.ObjectsOf(this).Single().AsValuedNode().AsString();

                // TODO: Ths is a workaround for https://github.com/dotnetrdf/dotnetrdf/issues/237
                query = Regex.Replace(query, @"(FILTER.+)\.(\s*)", "$1$2");

                return query;
            }
        }

        protected override bool ValidateInternal(INode focusNode, IEnumerable<INode> valueNodes, ShaclValidationReport report, SparqlQuery query)
        {
            var propertyShape = Shape as ShaclPropertyShape;

            if (propertyShape != null)
            {
                BindPath(query.RootGraphPattern, propertyShape.Path.SparqlPath);
            }

            var solutions = (SparqlResultSet)focusNode.Graph.ExecuteQuery(query);

            if (solutions.IsEmpty)
            {
                return true;
            }

            if (report is null)
            {
                return false;
            }

            foreach (var solution in solutions)
            {
                var result = ShaclValidationResult.Create(report.Graph);
                result.FocusNode = solution["this"];

                if (solution.TryGetBoundValue("path", out var pathValue) && pathValue.NodeType == NodeType.Uri)
                {
                    result.ResultPath = new ShaclPredicatePath(pathValue);
                }
                else
                {
                    if (propertyShape != null)
                    {
                        result.ResultPath = propertyShape.Path;
                    }
                }

                if (solution.HasValue("value"))
                {
                    result.ResultValue = solution["value"];
                }
                else
                {
                    result.ResultValue = focusNode;
                }

                if (solution.HasValue("message"))
                {
                    result.Message = solution["message"];
                }
                else
                {
                    result.Message = Message;
                }

                result.SourceConstraintComponent = Component;
                result.SourceShape = Shape;
                result.Severity = Shape.Severity;
                result.SourceConstraint = this;

                report.Results.Add(result);
            }

            return false;
        }

        private static void BindPath(GraphPattern pattern, ISparqlPath path)
        {
            for (var i = 0; i < pattern.TriplePatterns.Count(); i++)
            {
                if (pattern.TriplePatterns[i] is TriplePattern triplePattern && triplePattern.Predicate.VariableName == "PATH")
                {
                    pattern.TriplePatterns.RemoveAt(i);
                    pattern.TriplePatterns.Insert(i, new PropertyPathPattern(triplePattern.Subject, path, triplePattern.Object));
                }
            }

            foreach (var subPattern in pattern.ChildGraphPatterns)
            {
                BindPath(subPattern, path);
            }

            foreach (var subQueryPattern in pattern.TriplePatterns.OfType<SubQueryPattern>())
            {
                BindPath(subQueryPattern.SubQuery.RootGraphPattern, path);
            }
        }
    }
}