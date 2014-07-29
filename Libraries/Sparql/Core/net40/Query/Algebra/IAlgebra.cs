using System;
using System.Collections.Generic;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Algebra
{
    public interface IAlgebra
        : IEquatable<IAlgebra>
    {
        IEnumerable<String> ProjectedVariables { get;  }

        IEnumerable<String> FixedVariables { get;  }

        IEnumerable<String> FloatingVariables { get; }

        void Accept(IAlgebraVisitor visitor);

        IEnumerable<ISolution> Execute(IAlgebraExecutor executor, IExecutionContext context);

        String ToString();

        string ToString(IAlgebraFormatter formatter);
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
