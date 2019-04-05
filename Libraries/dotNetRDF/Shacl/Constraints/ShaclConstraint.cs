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
    using System.Linq;

    internal abstract class ShaclConstraint : WrapperNode
    {
        protected ShaclConstraint(ShaclShape shape, INode value)
            : base(value)
        {
            this.Shape = shape;
        }

        internal abstract INode Component { get; }

        protected ShaclShape Shape { get; private set; }

        public abstract bool Validate(INode focusNode, IEnumerable<INode> valueNodes, ShaclValidationReport report);

        protected bool ReportValueNodes(INode focusNode, IEnumerable<INode> invalidValues, ShaclValidationReport report = null)
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
                var result = ShaclValidationResult.Create(report.Graph);
                result.SourceConstraintComponent = Component;
                result.Severity = Shape.Severity;
                result.Message = Shape.Message;
                result.SourceShape = Shape;
                result.FocusNode = focusNode;

                if (Shape is ShaclPropertyShape propertyShape)
                {
                    result.ResultPath = propertyShape.Path;
                    report.Graph.Assert(propertyShape.Path.AsTriples().Select(t => t.CopyTriple(report.Graph)));
                }

                result.ResultValue = invalidValue;

                report.Results.Add(result);
            }

            return false;
        }

        protected bool ReportValueNodes(IEnumerable<Triple> invalidValues, ShaclValidationReport report)
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
                var result = ShaclValidationResult.Create(report.Graph);
                result.SourceConstraintComponent = Component;
                result.Severity = Shape.Severity;
                result.Message = Shape.Message;
                result.SourceShape = Shape;
                result.FocusNode = invalidValue.Subject;
                result.ResultPath = ShaclPath.Parse(invalidValue.Predicate);
                report.Graph.Assert(result.ResultPath.AsTriples().Select(t => t.CopyTriple(report.Graph)));
                result.ResultValue = invalidValue.Object;

                report.Results.Add(result);
            }

            return false;
        }

        protected bool ReportFocusNode(INode focusNode, IEnumerable<INode> invalidValues, ShaclValidationReport report)
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

            var result = ShaclValidationResult.Create(report.Graph);
            result.SourceConstraintComponent = Component;
            result.Severity = Shape.Severity;
            result.Message = Shape.Message;
            result.SourceShape = Shape;
            result.FocusNode = focusNode;

            if (Shape is ShaclPropertyShape propertyShape)
            {
                result.ResultPath = propertyShape.Path;
                report.Graph.Assert(propertyShape.Path.AsTriples().Select(t => t.CopyTriple(report.Graph)));
            }

            report.Results.Add(result);

            return false;
        }

        protected bool ReportFocusNodes(INode focusNode, IEnumerable<INode> invalidValues, ShaclValidationReport report)
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
                var result = ShaclValidationResult.Create(report.Graph);
                result.SourceConstraintComponent = Component;
                result.Severity = Shape.Severity;
                result.Message = Shape.Message;
                result.SourceShape = Shape;
                result.FocusNode = focusNode;

                if (Shape is ShaclPropertyShape propertyShape)
                {
                    result.ResultPath = propertyShape.Path;
                    report.Graph.Assert(propertyShape.Path.AsTriples().Select(t => t.CopyTriple(report.Graph)));
                }

                report.Results.Add(result);
            }

            return false;
        }

        internal static ShaclConstraint Parse(ShaclShape shape, INode type, INode value)
        {
            var constraints = new Dictionary<INode, Func<INode, ShaclConstraint>>()
            {
                { Shacl.Class, n => new ShaclClassConstraint(shape, n) },
                { Shacl.Node, n => new ShaclNodeConstraint(shape, n) },
                { Shacl.Property, n => new ShaclPropertyConstraint(shape, n) },
                { Shacl.Datatype, n => new ShaclDatatypeConstraint(shape, n) },
                { Shacl.And, n => new ShaclAndConstraint(shape, n) },
                { Shacl.Or, n => new ShaclOrConstraint(shape, n) },
                { Shacl.Not, n => new ShaclNotConstraint(shape, n) },
                { Shacl.Xone, n => new ShaclXoneConstraint(shape, n) },
                { Shacl.NodeKind, n => new ShaclNodeKindConstraint(shape, n) },
                { Shacl.MinLength, n => new ShaclMinLengthConstraint(shape, n) },
                { Shacl.MaxLength, n => new ShaclMaxLengthConstraint(shape, n) },
                { Shacl.LanguageIn, n => new ShaclLanguageInConstraint(shape, n) },
                { Shacl.In, n => new ShaclInConstraint(shape, n) },
                { Shacl.MinCount, n => new ShaclMinCountConstraint(shape, n) },
                { Shacl.MaxCount, n => new ShaclMaxCountConstraint(shape, n) },
                { Shacl.UniqueLang, n => new ShaclUniqueLangConstraint(shape, n) },
                { Shacl.HasValue, n => new ShaclHasValueConstraint(shape, n) },
                { Shacl.Pattern, n => new ShaclPatternConstraint(shape, n) },
                { Shacl.Equals, n => new ShaclEqualsConstraint(shape, n) },
                { Shacl.Disjoint, n => new ShaclDisjointConstraint(shape, n) },
                { Shacl.LessThan, n => new ShaclLessThanConstraint(shape, n) },
                { Shacl.LessThanOrEquals, n => new ShaclLessThanOrEqualsConstraint(shape, n) },
                { Shacl.MinExclusive, n => new ShaclMinExclusiveConstraint(shape, n) },
                { Shacl.MinInclusive, n => new ShaclMinInclusiveConstraint(shape, n) },
                { Shacl.MaxExclusive, n => new ShaclMaxExclusiveConstraint(shape, n) },
                { Shacl.MaxInclusive, n => new ShaclMaxInclusiveConstraint(shape, n) },
                { Shacl.QualifiedMinCount, n => new ShaclQualifiedMinCountConstraint(shape, n) },
                { Shacl.QualifiedMaxCount, n => new ShaclQualifiedMaxCountConstraint(shape, n) },
                { Shacl.Closed, n => new ShaclClosedConstraint(shape, n) },
                { Shacl.Sparql, n => new ShaclSparqlSelectConstraint(shape, n) },
           };

            return constraints[type](value);
        }
    }
}