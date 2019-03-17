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
        private static readonly INode rdf_type = new NodeFactory().CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));

        private ShaclValidationResult(INode node)
            : base(node)
        {
        }

        internal INode FocusNode
        {
            get
            {
                return Shacl.FocusNode.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                Graph.Retract(this, Shacl.FocusNode, FocusNode ?? Graph.CreateVariableNode("FocusNode"));
                Graph.Assert(this, Shacl.FocusNode, value);
            }
        }

        internal INode Value
        {
            get
            {
                return Shacl.Value.ObjectsOf(this).SingleOrDefault();
            }

            set
            {
                Graph.Retract(this, Shacl.Value, Value ?? Graph.CreateVariableNode("Value"));
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
                Graph.Retract(this, Shacl.SourceShape, SourceShape ?? Graph.CreateVariableNode("SourceShape"));
                Graph.Assert(this, Shacl.SourceShape, value);
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
                Graph.Retract(this, Shacl.SourceConstraintComponent, SourceConstraintComponent ?? Graph.CreateVariableNode("SourceConstraintComponent"));
                Graph.Assert(this, Shacl.SourceConstraintComponent, value);
            }
        }

        internal static ShaclValidationResult Create(IGraph g)
        {
            var report = new ShaclValidationResult(g.CreateBlankNode());
            g.Assert(report, rdf_type, Shacl.ValidationResult);
            return report;
        }

        internal static ShaclValidationResult Parse(INode node)
        {
            return new ShaclValidationResult(node);
        }
    }
}