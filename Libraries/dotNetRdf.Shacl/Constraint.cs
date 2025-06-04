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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VDS.RDF.Shacl.Constraints;
using VDS.RDF.Shacl.Validation;

namespace VDS.RDF.Shacl;

internal abstract class Constraint : GraphWrapperNode
{
    [DebuggerStepThrough]
    protected Constraint(Shape shape, INode value)
        : base(value, shape.Graph)
    {
        Shape = shape;
    }

    internal abstract INode ConstraintComponent { get; }

    protected Shape Shape { get; private set; }

    protected virtual string DefaultMessage { get; }

    // TODO: Spec says this is a collection
    private ILiteralNode Message
    {
        get
        {
            if (Shape.Message is ILiteralNode message)
            {
                return message;
            }

            if (DefaultMessage is null)
            {
                return null;
            }

            return Graph.CreateLiteralNode(DefaultMessage);
        }
    }

    internal static Constraint Parse(Shape shape, INode type, INode value)
    {
        switch (type)
        {
            case INode t when t.Equals(Vocabulary.Property): return new Property(shape, value);
            case INode t when t.Equals(Vocabulary.MaxCount): return new MaxCount(shape, value);
            case INode t when t.Equals(Vocabulary.NodeKind): return new NodeKind(shape, value);
            case INode t when t.Equals(Vocabulary.MinCount): return new MinCount(shape, value);
            case INode t when t.Equals(Vocabulary.Node): return new Node(shape, value);
            case INode t when t.Equals(Vocabulary.Datatype): return new Datatype(shape, value);
            case INode t when t.Equals(Vocabulary.Closed): return new Closed(shape, value);
            case INode t when t.Equals(Vocabulary.HasValue): return new HasValue(shape, value);
            case INode t when t.Equals(Vocabulary.Or): return new Or(shape, value);
            case INode t when t.Equals(Vocabulary.Class): return new Class(shape, value);
            case INode t when t.Equals(Vocabulary.Not): return new Not(shape, value);
            case INode t when t.Equals(Vocabulary.Xone): return new Xone(shape, value);
            case INode t when t.Equals(Vocabulary.In): return new In(shape, value);
            case INode t when t.Equals(Vocabulary.Sparql): return new Select(shape, value);
            case INode t when t.Equals(Vocabulary.Pattern): return new Pattern(shape, value);
            case INode t when t.Equals(Vocabulary.MinInclusive): return new MinInclusive(shape, value);
            case INode t when t.Equals(Vocabulary.MinExclusive): return new MinExclusive(shape, value);
            case INode t when t.Equals(Vocabulary.MaxExclusive): return new MaxExclusive(shape, value);
            case INode t when t.Equals(Vocabulary.MinLength): return new MinLength(shape, value);
            case INode t when t.Equals(Vocabulary.MaxInclusive): return new MaxInclusive(shape, value);
            case INode t when t.Equals(Vocabulary.And): return new And(shape, value);
            case INode t when t.Equals(Vocabulary.QualifiedMinCount): return new QualifiedMinCount(shape, value);
            case INode t when t.Equals(Vocabulary.QualifiedMaxCount): return new QualifiedMaxCount(shape, value);
            case INode t when t.Equals(Vocabulary.EqualsNode): return new Equals(shape, value);
            case INode t when t.Equals(Vocabulary.LanguageIn): return new LanguageIn(shape, value);
            case INode t when t.Equals(Vocabulary.LessThan): return new LessThan(shape, value);
            case INode t when t.Equals(Vocabulary.Disjoint): return new Disjoint(shape, value);
            case INode t when t.Equals(Vocabulary.LessThanOrEquals): return new LessThanOrEquals(shape, value);
            case INode t when t.Equals(Vocabulary.UniqueLang): return new UniqueLang(shape, value);
            case INode t when t.Equals(Vocabulary.MaxLength): return new MaxLength(shape, value);

            default: throw new Exception();
        }
    }

    internal abstract bool Validate(IGraph dataGraph, INode focusNode, IEnumerable<INode> valueNodes, Report report);

    protected bool ReportValueNodes(INode focusNode, IEnumerable<INode> invalidValues, Report report)
    {
        var allValid = !invalidValues.Any();

        if (report is null)
        {
            return allValid;
        }

        if (allValid)
        {
            return true;
        }

        foreach (INode invalidValue in invalidValues)
        {
            var result = Result.Create(report.Graph);
            result.SourceConstraintComponent = ConstraintComponent;
            result.Severity = Shape.Severity;
            result.Message = Message;
            result.SourceShape = Shape;
            result.FocusNode = focusNode;

            if (Shape is Shapes.Property propertyShape)
            {
                result.ResultPath = propertyShape.Path;
                report.Graph.Assert(propertyShape.Path.AsTriples);
            }

            result.ResultValue = invalidValue;

            report.Results.Add(result);
        }

        return false;
    }

    protected bool ReportValueNodes(IEnumerable<Triple> invalidValues, Report report)
    {
        var allValid = !invalidValues.Any();

        if (report is null)
        {
            return allValid;
        }

        if (allValid)
        {
            return true;
        }

        foreach (Triple invalidValue in invalidValues)
        {
            var result = Result.Create(report.Graph);
            result.SourceConstraintComponent = ConstraintComponent;
            result.Severity = Shape.Severity;
            result.Message = Message;
            result.SourceShape = Shape;
            result.FocusNode = invalidValue.Subject;
            result.ResultPath = Path.Parse(invalidValue.Predicate, Graph);
            report.Graph.Assert(result.ResultPath.AsTriples);
            result.ResultValue = invalidValue.Object;

            report.Results.Add(result);
        }

        return false;
    }

    protected bool ReportFocusNode(INode focusNode, IEnumerable<INode> invalidValues, Report report)
    {
        var allValid = !invalidValues.Any();

        if (report is null)
        {
            return allValid;
        }

        if (allValid)
        {
            return true;
        }

        var result = Result.Create(report.Graph);
        result.SourceConstraintComponent = ConstraintComponent;
        result.Severity = Shape.Severity;
        result.Message = Message;
        result.SourceShape = Shape;
        result.FocusNode = focusNode;

        if (Shape is Shapes.Property propertyShape)
        {
            result.ResultPath = propertyShape.Path;
            report.Graph.Assert(propertyShape.Path.AsTriples);
        }

        report.Results.Add(result);

        return false;
    }

    protected bool ReportFocusNodes(INode focusNode, IEnumerable<INode> invalidValues, Report report)
    {
        var allValid = !invalidValues.Any();

        if (report is null)
        {
            return allValid;
        }

        if (allValid)
        {
            return true;
        }

        foreach (INode invalidValue in invalidValues)
        {
            var result = Result.Create(report.Graph);
            result.SourceConstraintComponent = ConstraintComponent;
            result.Severity = Shape.Severity;
            result.Message = Message;
            result.SourceShape = Shape;
            result.FocusNode = focusNode;

            if (Shape is Shapes.Property propertyShape)
            {
                result.ResultPath = propertyShape.Path;
                report.Graph.Assert(propertyShape.Path.AsTriples);
            }

            report.Results.Add(result);
        }

        return false;
    }
}