/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2023 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Diagnostics;
using System.Linq;
using VDS.RDF.Shacl.Validation;

namespace VDS.RDF.Shacl.Constraints
{
    internal class UniqueLang : Boolean
    {
        [DebuggerStepThrough]
        internal UniqueLang(Shape shape, INode node)
            : base(shape, node)
        {
        }

        protected override string DefaultMessage => "Value nodes must not use the same language tag.";

        internal override INode ConstraintComponent
        {
            get
            {
                return Vocabulary.UniqueLangConstraintComponent;
            }
        }

        internal override bool Validate(IGraph dataGraph, INode focusNode, IEnumerable<INode> valueNodes, Report report)
        {
            if (!BooleanValue)
            {
                return true;
            }

            IEnumerable<ILiteralNode> invalidValues =
                from valueNode in valueNodes
                where valueNode.NodeType == NodeType.Literal
                let literal = (ILiteralNode)valueNode
                where !string.IsNullOrEmpty(literal.Language)
                group valueNode by literal.Language into langNodes
                where langNodes.Skip(1).Any()
                select Graph.CreateLiteralNode(langNodes.Key);

            return ReportFocusNodes(focusNode, invalidValues, report);
        }
    }
}