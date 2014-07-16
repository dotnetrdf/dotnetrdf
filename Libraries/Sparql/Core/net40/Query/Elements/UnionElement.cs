using System;
using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF.Query.Elements
{
    public class UnionElement
        : IElement
    {
        public UnionElement(IElement lhs, IElement rhs)
        {
            if (lhs == null) throw new ArgumentNullException("lhs");
            if (rhs == null) throw new ArgumentNullException("rhs");

            this.Lhs = lhs;
            this.Rhs = rhs;
        }

        public IElement Lhs { get; private set; }

        public IElement Rhs { get; private set; }

        public bool Equals(IElement other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is UnionElement)) return false;

            UnionElement union = (UnionElement) other;
            return this.Lhs.Equals(union.Lhs) && this.Rhs.Equals(union.Rhs);
        }

        public void Accept(IElementVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<string> Variables
        {
            get { return this.Lhs.Variables.Concat(this.Rhs.Variables).Distinct(); }
        }

        public IEnumerable<string> ProjectedVariables
        {
            get { return this.Lhs.ProjectedVariables.Concat(this.Rhs.ProjectedVariables).Distinct(); }
        }
    }
}