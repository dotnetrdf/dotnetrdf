using System;
using System.Collections.Generic;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;

namespace VDS.RDF.Query.Algebra
{
    public interface IAlgebra
        : IEquatable<IAlgebra>
    {
        IEnumerable<String> ProjectedVariables { get;  }

        IEnumerable<String> FixedVariables { get;  }

        IEnumerable<String> FloatingVariables { get; }

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
