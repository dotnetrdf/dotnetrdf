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
    using VDS.RDF.Nodes;

    internal abstract class ShaclQualifiedConstraint : ShaclConstraint
    {
        private readonly INode shapeNode;

        public ShaclQualifiedConstraint(INode shapeNode, INode node)
            : base(node)
        {
            this.shapeNode = shapeNode;
        }

        protected int ThereShouldBe => (int)this.AsValuedNode().AsInteger();

        public IEnumerable<INode> DisjointConformingValueNodes(INode focusNode, IEnumerable<INode> valueNodes)
        {
            var currentShape = ShaclShape.Parse(shapeNode);

            IEnumerable<ShaclShape> selectSiblingShapes()
            {
                return
                    from parent in Shacl.Property.SubjectsOf(currentShape)
                    from property in Shacl.Property.ObjectsOf(parent)
                    from qulifiedShape in Shacl.QualifiedValueShape.ObjectsOf(property)
                    where qulifiedShape != currentShape
                    select ShaclShape.Parse(qulifiedShape);
            }

            var isDisjoint = (
                from disjoint in Shacl.QualifiedValueShapesDisjoint.ObjectsOf(currentShape)
                where disjoint.AsValuedNode().AsBoolean()
                select disjoint)
                .Any();

            var siblingShapes = isDisjoint ? selectSiblingShapes() : Enumerable.Empty<ShaclShape>();

            return
                from qualified in Shacl.QualifiedValueShape.ObjectsOf(shapeNode)
                let qualifiedShape = ShaclShape.Parse(qualified)
                from valueNode in valueNodes
                let v = valueNode.AsEnumerable()
                let conformsToQualifiedShape = qualifiedShape.Validate(focusNode, v)
                let doesNotConformToSiblingShapes = !siblingShapes.Any(siblingShape => siblingShape.Validate(focusNode, v))
                where conformsToQualifiedShape && doesNotConformToSiblingShapes
                select valueNode;
        }
    }
}