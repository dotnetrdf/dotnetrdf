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
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Shacl.Paths;
using VDS.RDF.Shacl.Validation;

namespace VDS.RDF.Shacl.Constraints;

internal class Select : Sparql
{
    [DebuggerStepThrough]
    internal Select(Shape shape, INode value)
        : base(shape, value)
    {
    }

    [DebuggerStepThrough]
    internal Select(Shape shape, INode value, IEnumerable<KeyValuePair<string, INode>> parameters)
        : base(shape, value, parameters)
    {
    }

    protected override string DefaultMessage => "SPARQL SELECT query must not return any result bindings.";

    protected override string Query
    {
        get
        {
            return Vocabulary.Select.ObjectsOf(this).Single().AsValuedNode().AsString();
        }
    }

    protected override bool ValidateInternal(IGraph dataGraph, INode focusNode, IEnumerable<INode> valueNodes, Report report, SparqlQuery query)
    {
        var propertyShape = Shape as Shapes.Property;

        if (propertyShape != null)
        {
            BindPath(query.RootGraphPattern, propertyShape.Path.SparqlPath);
        }

        var solutions = (SparqlResultSet)dataGraph.ExecuteQuery(query);

        if (solutions.IsEmpty)
        {
            return true;
        }

        if (report is null)
        {
            return false;
        }

        foreach (SparqlResult solution in solutions)
        {
            var result = Result.Create(report.Graph);
            result.FocusNode = solution["this"];

            if (solution.TryGetBoundValue("path", out INode pathValue) && pathValue.NodeType == NodeType.Uri)
            {
                result.ResultPath = new Predicate(pathValue, result.Graph);
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
                result.Message = (ILiteralNode)solution["message"];
            }
            else
            {
                result.Message = Message;
            }

            result.SourceConstraintComponent = ConstraintComponent;
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
            if (pattern.TriplePatterns[i] is not TriplePattern triplePattern) { continue; }
            if (triplePattern.Predicate is not VariablePattern variablePattern) { continue; }
            if (variablePattern.VariableName != "PATH") { continue; }

            pattern.TriplePatterns.RemoveAt(i);
            pattern.TriplePatterns.Insert(i, new PropertyPathPattern(triplePattern.Subject, path, triplePattern.Object));
        }

        foreach (GraphPattern subPattern in pattern.ChildGraphPatterns)
        {
            BindPath(subPattern, path);
        }

        foreach (SubQueryPattern subQueryPattern in pattern.TriplePatterns.OfType<SubQueryPattern>())
        {
            BindPath(subQueryPattern.SubQuery.RootGraphPattern, path);
        }
    }
}