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

    public abstract class ShaclShape : WrapperNode
    {
        protected ShaclShape(INode node)
            : base(node)
        {
        }

        private IEnumerable<ShaclConstraint> Constraints
        {
            get
            {
                IEnumerable<ShaclConstraint> selectConstraints(INode parameter) =>
                    from t in this.Graph.GetTriplesWithSubjectPredicate(this, parameter)
                    select ShaclConstraint.Parse(t);

                return Shacl.Constraints.SelectMany(selectConstraints);
            }
        }

        private IEnumerable<ShaclTarget> Targets
        {
            get
            {
                var rdf_type = this.Graph.CreateUriNode("rdf:type");
                var rdfs_Class = this.Graph.CreateUriNode("rdfs:Class");

                bool hasType(INode node, INode type)
                {
                    return node.Graph.GetTriplesWithSubjectPredicate(this, rdf_type).WithObject(type).Any();
                }

                bool isShape(INode node)
                {
                    return Shacl.Shapes.Any(shape => hasType(node, shape));
                }

                bool isClass(INode node)
                {
                    return hasType(node, rdfs_Class);
                }

                IEnumerable<ShaclTarget> selectTargets(INode type) =>
                    from t in this.Graph.GetTriplesWithSubjectPredicate(this, type)
                    select ShaclTarget.Parse(type, t.Object);

                var targets = Shacl.Targets.SelectMany(selectTargets);

                var implicitClassTargets =
                    from shape in this.AsEnumerable()
                    where isClass(shape) && isShape(shape)
                    select ShaclTarget.Parse(Shacl.TargetClass, shape);

                return targets.Union(implicitClassTargets);
            }
        }

        internal static ShaclShape Parse(INode node)
        {
            if (node.Graph.GetTriplesWithSubjectPredicate(node, Shacl.Path).Any())
            {
                return new ShaclPropertyShape(node);
            }
            else
            {
                return new ShaclNodeShape(node);
            }
        }

        internal bool Validate(IGraph dataGragh)
        {
            return SelectFocusNodes(dataGragh).All(focusNode => this.Validate(focusNode, focusNode.AsEnumerable()));
        }

        internal IEnumerable<INode> SelectFocusNodes(IGraph dataGragh)
        {
            return this.Targets.SelectMany(target => target.SelectFocusNodes(dataGragh));
        }

        internal virtual bool Validate(INode focusNode, IEnumerable<INode> valueNodes)
        {
            return this.Constraints.All(constraint => constraint.Validate(focusNode, valueNodes));
        }
    }
}