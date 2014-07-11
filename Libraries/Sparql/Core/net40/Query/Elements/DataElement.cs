using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Elements
{
    public class DataElement
        : IElement
    {
        public bool Equals(IElement other)
        {
            throw new NotImplementedException();
        }

        public void Accept(IElementVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }
}
