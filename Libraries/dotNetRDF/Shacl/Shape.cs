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
    using VDS.RDF.Shacl.Shapes;
    using VDS.RDF.Shacl.Validation;

    internal abstract class Shape : WrapperNode
    {
        [DebuggerStepThrough]
        protected Shape(INode node)
            : base(node)
        {
        }

        internal INode Severity =>
            Vocabulary.Severity.ObjectsOf(this).SingleOrDefault() ?? Vocabulary.Violation;

        internal INode Message =>
            Vocabulary.Message.ObjectsOf(this).SingleOrDefault();

        private bool Deactivated =>
            Vocabulary.Deactivated.ObjectsOf(this).SingleOrDefault()?.AsValuedNode().AsBoolean() ?? false;

        private new ShapesGraph Graph => new ShapesGraph(base.Graph);

        private IEnumerable<Constraint> Constraints =>
            Vocabulary.Constraints.SelectMany(constraint =>
                from o in constraint.ObjectsOf(this)
                select Constraint.Parse(this, constraint, o));

        private IEnumerable<Target> Targets
        {
            get
            {
                var rdfs_Class = Graph.CreateUriNode("rdfs:Class");

                bool isShape(INode node)
                {
                    return Vocabulary.Shapes.Any(shapeClass => shapeClass.IsInstance(node));
                }

                bool isClass(INode node)
                {
                    return rdfs_Class.IsInstance(node);
                }

                IEnumerable<Target> selectTargets(INode type) =>
                    from target in type.ObjectsOf(this)
                    select Target.Parse(type, target);

                var targets = Vocabulary.Targets.SelectMany(selectTargets);

                var implicitClassTargets =
                    from shape in this.AsEnumerable()
                    where isClass(shape) && isShape(shape)
                    select Target.Parse(Vocabulary.TargetClass, shape);

                return targets.Union(implicitClassTargets);
            }
        }

        internal static Shape Parse(INode node)
        {
            if (Vocabulary.Path.ObjectsOf(node).Any())
            {
                return new Property(node);
            }
            else
            {
                return new Node(node);
            }
        }

        internal bool Validate(IGraph dataGragh, Report report)
        {
            return SelectFocusNodes(dataGragh)
                .Select(focusNode => Validate(focusNode, focusNode.AsEnumerable(), report))
                .Aggregate(true, (a, b) => a && b);
        }

        internal bool Validate(INode focusNode, Report report = null)
        {
            return Validate(focusNode, focusNode.AsEnumerable(), report);
        }

        internal bool Validate(INode focusNode, IEnumerable<INode> valueNodes, Report report = null)
        {
            if (Deactivated)
            {
                return true;
            }

            return ValidateInternal(focusNode, valueNodes, report);
        }

        protected virtual bool ValidateInternal(INode focusNode, IEnumerable<INode> valueNodes, Report report)
        {
            var components = Graph.ConstraintComponents.Where(component => component.Matches(this)).SelectMany(c => c.Constraints(this));

            return Constraints.Concat(components)
                .Select(constraint => constraint.Validate(focusNode, valueNodes, report))
                .Aggregate(true, (a, b) => a && b);
        }

        private IEnumerable<INode> SelectFocusNodes(IGraph dataGragh)
        {
            return Targets.SelectMany(target => target.SelectFocusNodes(dataGragh)).Distinct();
        }
    }
}