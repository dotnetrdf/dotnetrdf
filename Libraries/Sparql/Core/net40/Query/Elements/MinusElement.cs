using System;
using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF.Query.Elements
{
    public class MinusElement
        : IElement
    {
        public MinusElement(IElement element)
        {
            if (element == null) throw new ArgumentNullException("element");
            this.Element = element;
        }

        public IElement Element { get; set; }

        public bool Equals(IElement other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is MinusElement)) return false;

            return this.Element.Equals(((MinusElement) other).Element);
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