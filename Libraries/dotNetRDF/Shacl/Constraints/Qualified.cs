/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF.Shacl.Constraints
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using VDS.RDF.Nodes;
    using VDS.RDF.Shacl.Validation;

    internal abstract class Qualified : Numeric
    {
        [DebuggerStepThrough]
        internal Qualified(Shape shape, INode node)
            : base(shape, node)
        {
        }

        protected Shape QualifiedValueShape
        {
            get
            {
                return (
                    from shape in Vocabulary.QualifiedValueShape.ObjectsOf(Shape)
                    select Shape.Parse(shape))
                    .SingleOrDefault();
            }
        }

        internal override bool Validate(INode focusNode, IEnumerable<INode> valueNodes, Report report)
        {
            if (QualifiedValueShape is null)
            {
                return true;
            }

            return ValidateInternal(focusNode, valueNodes, report);
        }

        protected IEnumerable<INode> QualifiedValueNodes(INode focusNode, IEnumerable<INode> valueNodes)
        {
            var currentShape = Shape;

            IEnumerable<Shape> selectSiblingShapes()
            {
                return
                    from parent in Vocabulary.Property.SubjectsOf(currentShape)
                    from property in Vocabulary.Property.ObjectsOf(parent)
                    from qulifiedShape in Vocabulary.QualifiedValueShape.ObjectsOf(property)
                    where !QualifiedValueShape.Equals(qulifiedShape)
                    select Shape.Parse(qulifiedShape);
            }

            var isDisjoint = (
                from disjoint in Vocabulary.QualifiedValueShapesDisjoint.ObjectsOf(currentShape)
                where disjoint.AsValuedNode().AsBoolean()
                select disjoint)
                .Any();

            var siblingShapes = isDisjoint ? selectSiblingShapes() : Enumerable.Empty<Shape>();

            return
                from qualified in Vocabulary.QualifiedValueShape.ObjectsOf(Shape)
                let qualifiedShape = Shape.Parse(qualified)
                from valueNode in valueNodes
                let v = valueNode.AsEnumerable()
                let conformsToQualifiedShape = qualifiedShape.Validate(focusNode, v)
                let doesNotConformToSiblingShapes = !siblingShapes.Any(siblingShape => siblingShape.Validate(focusNode, v))
                where conformsToQualifiedShape && doesNotConformToSiblingShapes
                select valueNode;
        }

        protected abstract bool ValidateInternal(INode focusNode, IEnumerable<INode> valueNodes, Report report);
    }
}