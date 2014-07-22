using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Elements
{
    public class FilterElement
        : IElement
    {
        public FilterElement(IEnumerable<IExpression> expressions)
        {
            if (expressions == null) throw new ArgumentNullException("expressions");
            this.Expressions = expressions.ToList();
        }

        public IList<IExpression> Expressions { get; private set; }

        public bool Equals(IElement other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is FilterElement)) return false;

            FilterElement f = (FilterElement) other;
            if (this.Expressions.Count != f.Expressions.Count) return false;
            for (int i = 0; i < this.Expressions.Count; i++)
            {
                if (!this.Expressions[i].Equals(f.Expressions[i])) return false;
            }
            return true;
        }

        public void Accept(IElementVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<string> Variables
        {
            get { return this.Expressions.SelectMany(e => e.Variables).Distinct(); }
        }

        public IEnumerable<string> ProjectedVariables
        {
            get { return Enumerable.Empty<string>(); }
        }
    }
}