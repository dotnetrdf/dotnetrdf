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