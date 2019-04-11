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
    using System.Diagnostics;
    using System.Linq;
    using VDS.RDF.Nodes;
    using VDS.RDF.Query;
    using VDS.RDF.Query.Expressions.Primary;
    using VDS.RDF.Query.Patterns;

    internal class ShaclSparqlAskConstraint : ShaclSparqlConstraint
    {
        [DebuggerStepThrough]
        internal ShaclSparqlAskConstraint(ShaclShape shape, INode value, IEnumerable<KeyValuePair<string, INode>> parameters)
            : base(shape, value, parameters)
        {
        }

        protected override string Query => Shacl.Ask.ObjectsOf(this).Single().AsValuedNode().AsString();

        protected override bool ValidateInternal(INode focusNode, IEnumerable<INode> valueNodes, ShaclValidationReport report, SparqlQuery query)
        {
            IEnumerable<INode> execute()
            {
                foreach (var valueNode in valueNodes)
                {
                    var q = query.Copy();
                    q.RootGraphPattern.TriplePatterns.Insert(0, new BindPattern("value", new ConstantTerm(valueNode)));

                    if (!((SparqlResultSet)focusNode.Graph.ExecuteQuery(q)).Result)
                    {
                        yield return valueNode;
                    }
                }
            }

            return ReportValueNodes(focusNode, execute(), report);
        }
    }
}
