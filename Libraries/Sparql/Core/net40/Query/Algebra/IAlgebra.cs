using System;
using System.Collections;
using System.Collections.Generic;
using VDS.RDF.Query.Engine;

namespace VDS.RDF.Query.Algebra
{
    public interface IAlgebra
        : IEquatable<IAlgebra>
    {
        void Accept(IAlgebraVisitor visitor);

        IEnumerable<ISet> Execute(IAlgebraExecutor executor, IExecutionContext context);
    }

    public interface IUnaryAlgebra
        : IAlgebra
    {
        IAlgebra InnerAlgebra { get; }
    }

    public interface IBinaryAlgebra
        : IAlgebra
    {
        IAlgebra Lhs { get; }

        IAlgebra Rhs { get; }
    }
}
