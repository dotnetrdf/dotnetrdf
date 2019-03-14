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

        public abstract bool Validate(INode focusNode, IEnumerable<INode> valueNodes);

        internal static ShaclConstraint Parse(Triple triple)
        {
            var constraints = new Dictionary<INode, Func<Triple, ShaclConstraint>>()
            {
                { Shacl.Class, t => new ShaclClassConstraint(t.Object) },
                { Shacl.Node, t => new ShaclNodeConstraint(t.Object) },
                { Shacl.Property, t => new ShaclPropertyConstraint(t.Object) },
                { Shacl.Datatype, t => new ShaclDatatypeConstraint(t.Object) },
                { Shacl.And, t => new ShaclAndConstraint(t.Object) },
                { Shacl.Or, t => new ShaclOrConstraint(t.Object) },
                { Shacl.Not, t => new ShaclNotConstraint(t.Object) },
                { Shacl.Xone, t => new ShaclXoneConstraint(t.Object) },
                { Shacl.NodeKind, t => new ShaclNodeKindConstraint(t.Object) },
                { Shacl.MinLength, t => new ShaclMinLengthConstraint(t.Object) },
                { Shacl.MaxLength, t => new ShaclMaxLengthConstraint(t.Object) },
                { Shacl.LanguageIn, t => new ShaclLanguageInConstraint(t.Object) },
                { Shacl.In, t => new ShaclInConstraint(t.Object) },
                { Shacl.MinCount, t => new ShaclMinCountConstraint(t.Object) },
                { Shacl.MaxCount, t => new ShaclMaxCountConstraint(t.Object) },
                { Shacl.UniqueLang, t => new ShaclUniqueLangConstraint(t.Object) },
                { Shacl.HasValue, t => new ShaclHasValueConstraint(t.Object) },
                { Shacl.Pattern, t => new ShaclPatternConstraint(t.Subject, t.Object) },
           };

            return constraints[triple.Predicate](triple);
        }
    }
}