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
using System.Diagnostics;
using System.Linq;
using VDS.RDF.Query;
using VDS.RDF.Shacl.Validation;

namespace VDS.RDF.Shacl.Constraints;

internal class Pattern : Constraint
{
    [DebuggerStepThrough]
    internal Pattern(Shape shape, INode node)
        : base(shape, node)
    {
    }

    protected override string DefaultMessage =>
        $"Value must match the expected regular expression \"{this}\" with flags \"{Flags}\".";

    internal override INode ConstraintComponent
    {
        get
        {
            return Vocabulary.PatternConstraintComponent;
        }
    }

    private INode Flags
    {
        get
        {
            return Vocabulary.Flags.ObjectsOf(Shape).SingleOrDefault();
        }
    }

    internal override bool Validate(IGraph dataGraph, INode focusNode, IEnumerable<INode> valueNodes, Report report)
    {
        var query = new SparqlParameterizedString(@"
ASK {
    BIND($flags AS ?f)
    FILTER(IF(BOUND(?f), REGEX(STR($value), $pattern, ?f), REGEX(STR($value), $pattern)))
}");
        query.SetVariable("pattern", this);
        query.SetVariable("flags", Flags);

        bool isInvalid(INode node)
        {
            if (node.NodeType == NodeType.Blank)
            {
                return true;
            }

            query.SetVariable("value", node);
            return !((SparqlResultSet)dataGraph.ExecuteQuery(query)).Result;
        }

        IEnumerable<INode> invalidValues = valueNodes.Where(isInvalid);

        return ReportValueNodes(focusNode, invalidValues, report);
    }
}