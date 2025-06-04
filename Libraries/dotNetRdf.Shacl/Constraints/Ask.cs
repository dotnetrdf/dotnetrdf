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
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Shacl.Validation;

namespace VDS.RDF.Shacl.Constraints;

internal class Ask : Sparql
{
    [DebuggerStepThrough]
    internal Ask(Shape shape, INode value, IEnumerable<KeyValuePair<string, INode>> parameters)
        : base(shape, value, parameters)
    {
    }

    protected override string DefaultMessage => "SPARQL ASK must return true for all value nodes.";

    protected override string Query
    {
        get
        {
            return Vocabulary.Ask.ObjectsOf(this).Single().AsValuedNode().AsString();
        }
    }

    protected override bool ValidateInternal(IGraph dataGraph, INode focusNode, IEnumerable<INode> valueNodes, Report report, SparqlQuery query)
    {
        IEnumerable<INode> invalidValues =
            from valueNode in valueNodes
            let q = BindValue(query, valueNode)
            let result = ((SparqlResultSet)dataGraph.ExecuteQuery(q)).Result
            where !result
            select valueNode;

        return ReportValueNodes(focusNode, invalidValues, report);
    }

    private static SparqlQuery BindValue(SparqlQuery query, INode valueNode)
    {
        SparqlQuery q = query.Copy();
        q.RootGraphPattern.TriplePatterns.Insert(0, new BindPattern("value", new ConstantTerm(valueNode)));

        return q;
    }
}
