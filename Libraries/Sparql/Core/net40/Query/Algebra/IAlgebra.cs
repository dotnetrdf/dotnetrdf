using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace VDS.RDF.Query.Algebra
{
    public interface IAlgebra
        : IEquatable<IAlgebra>
    {
        void Accept(IAlgebraVisitor visitor);
    }

    public interface IUnaryAlgebra
        : IAlgebra
    {
        IAlgebra InnerAlgebra { get; }
    }
}
