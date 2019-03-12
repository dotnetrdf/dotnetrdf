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

    internal abstract class ShaclConstraint : WrapperNode
    {
        protected ShaclConstraint(INode node)
            : base(node)
        {
        }

        public abstract bool Validate(INode node);

        internal static ShaclConstraint Parse(INode type, INode value)
        {
            var constraints = new Dictionary<INode, Func<INode, ShaclConstraint>>()
            {
                { Shacl.Class, n => new ShaclClassConstraint(n) },
                { Shacl.Node, n => new ShaclNodeConstraint(n) },
                { Shacl.Property, n => new ShaclPropertyConstraint(n) },
                { Shacl.Datatype, n => new ShaclDatatypeConstraint(n) },
                { Shacl.And, n => new ShaclAndConstraint(n) },
                { Shacl.Or, n => new ShaclOrConstraint(n) },
                { Shacl.Not, n => new ShaclNotConstraint(n) },
                { Shacl.Xone, n => new ShaclXoneConstraint(n) },
                { Shacl.NodeKind, n => new ShaclNodeKindConstraint(n) },
            };

            return constraints[type](value);
        }
    }
}