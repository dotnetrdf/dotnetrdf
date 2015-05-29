/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Elements
{
    public class OptionalElement
        : IElement
    {
        public OptionalElement(IElement element)
            : this(element, null) { }

        public OptionalElement(IElement element, IEnumerable<IExpression> expressions)
        {
            if (element == null) throw new ArgumentNullException("element");
            this.Element = element;
            this.Expressions = expressions != null ? expressions.ToList() : new List<IExpression>();
        }

        public IElement Element { get; set; }

        public IList<IExpression> Expressions { get; set; }

        public bool Equals(IElement other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is OptionalElement)) return false;

            OptionalElement optional = (OptionalElement) other;
            if (!this.Element.Equals(optional.Element)) return false;
            if (this.Expressions.Count != optional.Expressions.Count) return false;

            for (int i = 0; i < this.Expressions.Count; i++)
            {
                if (!this.Expressions[i].Equals(optional.Expressions[i])) return false;
            }
            return true;
        }

        public void Accept(IElementVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<string> Variables
        {
            get { return this.Element.Variables; }
        }

        public IEnumerable<string> ProjectedVariables
        {
            get { return Enumerable.Empty<string>(); }
        }
    }
}