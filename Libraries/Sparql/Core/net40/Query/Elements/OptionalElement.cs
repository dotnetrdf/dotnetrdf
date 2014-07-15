using System;
using System.Collections.Generic;

namespace VDS.RDF.Query.Elements
{
    public class OptionalElement
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

        public IEnumerable<string> Variables
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<string> ProjectedVariables
        {
            get { throw new NotImplementedException(); }
        }
    }
}