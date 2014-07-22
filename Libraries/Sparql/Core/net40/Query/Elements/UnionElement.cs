using System;
using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF.Query.Elements
{
    public class UnionElement
        : IElement
    {
        public UnionElement(IEnumerable<IElement> elements)
        {
            if (elements == null) throw new ArgumentNullException("elements");
            this.Elements = elements.ToList();
        }

        public IList<IElement> Elements { get; private set; }

        public bool Equals(IElement other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is UnionElement)) return false;

            UnionElement union = (UnionElement) other;
            if (this.Elements.Count != union.Elements.Count) return false;
            for (int i = 0; i < this.Elements.Count; i++)
            {
                if (!this.Elements[i].Equals(union.Elements[i])) return false;
            }
            return true;
        }

        public void Accept(IElementVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<string> Variables
        {
            get { return this.Elements.SelectMany(e => e.Variables).Distinct(); }
        }

        public IEnumerable<string> ProjectedVariables
        {
            get { return this.Elements.SelectMany(e => e.ProjectedVariables).Distinct(); }
        }
    }
}