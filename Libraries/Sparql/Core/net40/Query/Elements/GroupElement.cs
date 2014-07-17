using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF.Query.Elements
{
    public class GroupElement
        : IElement
    {
        public GroupElement(IEnumerable<IElement> elements)
        {
            this.Elements = elements != null ? new List<IElement>(elements) : new List<IElement>();
        }

        public IList<IElement> Elements { get; private set; }

        public bool Equals(IElement other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is GroupElement)) return false;

            GroupElement otherGroup = (GroupElement) other;
            if (this.Elements.Count != otherGroup.Elements.Count) return false;

            for (int i = 0; i < this.Elements.Count; i++)
            {
                if (!this.Elements[i].Equals(otherGroup.Elements[i])) return false;
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