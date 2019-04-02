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
    using System.Linq;
    using VDS.RDF.Parsing;

    internal class ShaclValidationResult : WrapperNode
    {
        private ShaclValidationResult(INode node)
            : base(node)
        {
        }

        internal INode Type
        {
            get
            {
                return rdf_type.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var type in rdf_type.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, rdf_type, type);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, rdf_type, value);
            }
        }

        internal INode Severity
        {
            get
            {
                return Shacl.ResultSeverity.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var severity in Shacl.ResultSeverity.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Shacl.ResultSeverity, severity);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Shacl.ResultSeverity, value);
            }
        }

        internal INode FocusNode
        {
            get
            {
                return Shacl.FocusNode.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var focusNode in Shacl.FocusNode.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Shacl.FocusNode, focusNode);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Shacl.FocusNode, value);
            }
        }

        internal INode ResultValue
        {
            get
            {
                return Shacl.Value.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var valueNode in Shacl.Value.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Shacl.Value, valueNode);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Shacl.Value, value);
            }
        }

        internal INode SourceShape
        {
            get
            {
                return Shacl.SourceShape.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var sourceShape in Shacl.SourceShape.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Shacl.SourceShape, sourceShape);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Shacl.SourceShape, value);
            }
        }

        internal INode Message
        {
            get
            {
                return Shacl.ResultMessage.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var sourceShape in Shacl.ResultMessage.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Shacl.ResultMessage, sourceShape);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Shacl.ResultMessage, value);
            }
        }

        internal INode SourceConstraintComponent
        {
            get
            {
                return Shacl.SourceConstraintComponent.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var sourceConstraintComponent in Shacl.SourceConstraintComponent.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Shacl.SourceConstraintComponent, sourceConstraintComponent);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Shacl.SourceConstraintComponent, value);
            }
        }

        internal ShaclPath ResultPath
        {
            get
            {
                return Shacl.ResultPath.ObjectsOf(this).Select(ShaclPath.Parse).SingleOrDefault();
            }

            set
            {
                foreach (var sourceConstraintComponent in Shacl.ResultPath.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Shacl.ResultPath, sourceConstraintComponent);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Shacl.ResultPath, value);
            }
        }

        internal INode SourceConstraint
        {
            get
            {
                return Shacl.SourceConstraint.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                foreach (var sourceConstraint in Shacl.SourceConstraint.ObjectsOf(this).ToList())
                {
                    Graph.Retract(this, Shacl.SourceConstraint, sourceConstraint);
                }

                if (value is null)
                {
                    return;
                }

                Graph.Assert(this, Shacl.SourceConstraint, value);
            }
        }

        private INode rdf_type => Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));

        internal static ShaclValidationResult Create(IGraph g)
        {
            var report = new ShaclValidationResult(g.CreateBlankNode());
            report.Type = Shacl.ValidationResult;

            return report;
        }

        internal static ShaclValidationResult Parse(INode node)
        {
            return new ShaclValidationResult(node);
        }
    }
}