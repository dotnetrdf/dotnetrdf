using System;

namespace VDS.RDF.Query.Elements
{
    public class FilterElement
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