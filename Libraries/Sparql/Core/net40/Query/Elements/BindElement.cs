using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Elements
{
    public class BindElement
        : IElement
    {
        public BindElement(IEnumerable<KeyValuePair<String, IExpression>> assignments)
        {
            if (assignments == null) throw new ArgumentNullException("assignments");
            this.Assignments = assignments.ToList();
        }

        public IList<KeyValuePair<String, IExpression>> Assignments { get; private set; }

        public bool Equals(IElement other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is BindElement)) return false;

            BindElement bind = (BindElement) other;
            if (this.Assignments.Count != bind.Assignments.Count) return false;
            for (int i = 0; i < this.Assignments.Count; i++)
            {
                if (!this.Assignments[i].Equals(bind.Assignments[i])) return false;
            }
            return true;
        }

        public void Accept(IElementVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<string> Variables
        {
            get { return this.Assignments.Select(kvp => kvp.Key).Concat(this.Assignments.SelectMany(kvp => kvp.Value.Variables)).Distinct(); }
        }

        public IEnumerable<string> ProjectedVariables
        {
            get { return this.Assignments.Select(kvp => kvp.Key); }
        }
    }
}
