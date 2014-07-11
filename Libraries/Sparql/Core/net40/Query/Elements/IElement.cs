using System;

namespace VDS.RDF.Query.Elements
{
    public interface IElement
        : IEquatable<IElement>
    {
        void Accept(IElementVisitor visitor);
    }
}
