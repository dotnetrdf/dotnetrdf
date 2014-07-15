using System;
using System.Collections.Generic;

namespace VDS.RDF.Query.Elements
{
    public class SubQueryElement
        : IElement
    {
        public SubQueryElement(IQuery query)
        {
            if (query == null) throw new ArgumentNullException("query");
            this.SubQuery = query;
        }

        public IQuery SubQuery { get; set; }

        public bool Equals(IElement other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is SubQueryElement)) return false;

            return this.SubQuery.Equals(((SubQueryElement) other).SubQuery);
        }

        public void Accept(IElementVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<string> Variables {
            get
            {
                throw new NotImplementedException();
            } 
        }

        public IEnumerable<string> ProjectedVariables { get { return this.SubQuery.ResultVariables; } }
    }
}