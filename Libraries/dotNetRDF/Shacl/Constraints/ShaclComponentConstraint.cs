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

    internal class ShaclComponentConstraint : ShaclConstraint
    {
        private readonly IEnumerable<KeyValuePair<string, INode>> parameters;

        // value has to be the ConstraintComponent, not the object of the constraint predicate statement
        // in which case parameter values have to be passed into constructor
        [DebuggerStepThrough]
        internal ShaclComponentConstraint(ShaclShape shape, INode value, IEnumerable<KeyValuePair<string, INode>> parameters)
            : base(shape, value)
        {
            this.parameters = parameters;
        }

        internal override INode Component => this;

        private ShaclConstraint Validator
        {
            get
            {
                if (Shape is ShaclNodeShape)
                {
                    var nodeValidator = Shacl.NodeValidator.ObjectsOf(this).SingleOrDefault();

                    if (nodeValidator != null)
                    {
                        return new ShaclSparqlSelectConstraint(Shape, nodeValidator, parameters);
                    }
                }

                if (Shape is ShaclPropertyShape)
                {
                    var propertyValidator = Shacl.PropertyValidator.ObjectsOf(this).SingleOrDefault();

                    if (propertyValidator != null)
                    {
                        return new ShaclSparqlSelectConstraint(Shape, propertyValidator, parameters);
                    }
                }

                return Shacl.Validator.ObjectsOf(this).Select(v => new ShaclSparqlAskConstraint(Shape, v, parameters)).SingleOrDefault();
            }
        }

        internal override bool Validate(INode focusNode, IEnumerable<INode> valueNodes, ShaclValidationReport report)
        {
            var constraint = Validator;

            var invalidValues =
                from valueNode in valueNodes
                where !constraint.Validate(focusNode, valueNode.AsEnumerable(), null)
                select valueNode;

            return ReportValueNodes(focusNode, invalidValues, report);
        }
    }
}
