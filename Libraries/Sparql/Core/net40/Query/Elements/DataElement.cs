using System;
using System.Collections.Generic;
using VDS.RDF.Query.Results;

namespace VDS.RDF.Query.Elements
{
    public class DataElement
        : IElement
    {
        public DataElement(IMutableTabularResults data)
        {
            if (data == null) throw new ArgumentNullException("data");
            this.Data = data;
        }

        public IMutableTabularResults Data { get; private set; }

        public bool Equals(IElement other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is DataElement)) return false;

            DataElement data = (DataElement) other;
            return this.Data.Equals(data.Data);
        }

        public void Accept(IElementVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<string> Variables
        {
            get { return this.Data.Variables; }
        }

        public IEnumerable<string> ProjectedVariables
        {
            get { return this.Data.Variables; }
        }
    }
}
