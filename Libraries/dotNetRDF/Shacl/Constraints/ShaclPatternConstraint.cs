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
    using VDS.RDF.Query;

    internal class ShaclPatternConstraint : ShaclConstraint
    {
        private readonly INode shape;

        public ShaclPatternConstraint(INode shape, INode node)
            : base(node)
        {
            this.shape = shape;
        }

        public INode Flags => this.Graph.GetTriplesWithSubjectPredicate(this.shape, Shacl.Flags).Select(t => t.Object).SingleOrDefault();

        public override bool Validate(INode focusNode, IEnumerable<INode> valueNodes)
        {
            var query = new SparqlParameterizedString(@"
ASK {
    BIND($flags AS ?f)
    FILTER(IF(BOUND(?f), REGEX(STR($value), $pattern, ?f), REGEX(STR($value), $pattern)))
}");
            query.SetVariable("pattern", this);
            query.SetVariable("flags", Flags);

            bool matches(INode node)
            {
                if (node.NodeType == NodeType.Blank)
                {
                    return false;
                }

                query.SetVariable("value", node);

                return ((SparqlResultSet)node.Graph.ExecuteQuery(query)).Result;
            }

            return valueNodes.All(matches);
        }
    }
}