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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using VDS.RDF.Shacl.Constraints;
    using VDS.RDF.Shacl.Validation;

    internal abstract class Constraint : WrapperNode
    {
        [DebuggerStepThrough]
        protected Constraint(Shape shape, INode value)
            : base(value)
        {
            this.Shape = shape;
        }

        internal abstract INode ConstraintComponent { get; }

        protected Shape Shape { get; private set; }

        internal static Constraint Parse(Shape shape, INode type, INode value)
        {
            var constraints = new Dictionary<INode, Func<INode, Constraint>>()
            {
                { Vocabulary.Class, n => new Class(shape, n) },
                { Vocabulary.Node, n => new Node(shape, n) },
                { Vocabulary.Property, n => new Property(shape, n) },
                { Vocabulary.Datatype, n => new Datatype(shape, n) },
                { Vocabulary.And, n => new And(shape, n) },
                { Vocabulary.Or, n => new Or(shape, n) },
                { Vocabulary.Not, n => new Not(shape, n) },
                { Vocabulary.Xone, n => new Xone(shape, n) },
                { Vocabulary.NodeKind, n => new NodeKind(shape, n) },
                { Vocabulary.MinLength, n => new MinLength(shape, n) },
                { Vocabulary.MaxLength, n => new MaxLength(shape, n) },
                { Vocabulary.LanguageIn, n => new LanguageIn(shape, n) },
                { Vocabulary.In, n => new In(shape, n) },
                { Vocabulary.MinCount, n => new MinCount(shape, n) },
                { Vocabulary.MaxCount, n => new MaxCount(shape, n) },
                { Vocabulary.UniqueLang, n => new UniqueLang(shape, n) },
                { Vocabulary.HasValue, n => new HasValue(shape, n) },
                { Vocabulary.Pattern, n => new Pattern(shape, n) },
                { Vocabulary.EqualsNode, n => new Equals(shape, n) },
                { Vocabulary.Disjoint, n => new Disjoint(shape, n) },
                { Vocabulary.LessThan, n => new LessThan(shape, n) },
                { Vocabulary.LessThanOrEquals, n => new LessThanOrEquals(shape, n) },
                { Vocabulary.MinExclusive, n => new MinExclusive(shape, n) },
                { Vocabulary.MinInclusive, n => new MinInclusive(shape, n) },
                { Vocabulary.MaxExclusive, n => new MaxExclusive(shape, n) },
                { Vocabulary.MaxInclusive, n => new MaxInclusive(shape, n) },
                { Vocabulary.QualifiedMinCount, n => new QualifiedMinCount(shape, n) },
                { Vocabulary.QualifiedMaxCount, n => new QualifiedMaxCount(shape, n) },
                { Vocabulary.Closed, n => new Closed(shape, n) },
                { Vocabulary.Sparql, n => new Select(shape, n) },
           };

            return constraints[type](value);
        }

        internal abstract bool Validate(INode focusNode, IEnumerable<INode> valueNodes, Report report);

        protected bool ReportValueNodes(INode focusNode, IEnumerable<INode> invalidValues, Report report = null)
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

            foreach (var invalidValue in invalidValues)
            {
                var result = Result.Create(report.Graph);
                result.SourceConstraintComponent = ConstraintComponent;
                result.Severity = Shape.Severity;
                result.Message = Shape.Message;
                result.SourceShape = Shape;
                result.FocusNode = focusNode;

                if (Shape is Shapes.Property propertyShape)
                {
                    result.ResultPath = propertyShape.Path;
                    report.Graph.Assert(propertyShape.Path.AsTriples().Select(t => t.CopyTriple(report.Graph)));
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

            foreach (var invalidValue in invalidValues)
            {
                var result = Result.Create(report.Graph);
                result.SourceConstraintComponent = ConstraintComponent;
                result.Severity = Shape.Severity;
                result.Message = Shape.Message;
                result.SourceShape = Shape;
                result.FocusNode = invalidValue.Subject;
                result.ResultPath = Path.Parse(invalidValue.Predicate);
                report.Graph.Assert(result.ResultPath.AsTriples().Select(t => t.CopyTriple(report.Graph)));
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
            result.Message = Shape.Message;
            result.SourceShape = Shape;
            result.FocusNode = focusNode;

            if (Shape is Shapes.Property propertyShape)
            {
                result.ResultPath = propertyShape.Path;
                report.Graph.Assert(propertyShape.Path.AsTriples().Select(t => t.CopyTriple(report.Graph)));
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

            foreach (var invalidValue in invalidValues)
            {
                var result = Result.Create(report.Graph);
                result.SourceConstraintComponent = ConstraintComponent;
                result.Severity = Shape.Severity;
                result.Message = Shape.Message;
                result.SourceShape = Shape;
                result.FocusNode = focusNode;

                if (Shape is Shapes.Property propertyShape)
                {
                    result.ResultPath = propertyShape.Path;
                    report.Graph.Assert(propertyShape.Path.AsTriples().Select(t => t.CopyTriple(report.Graph)));
                }

                report.Results.Add(result);
            }

            return false;
        }
    }
}