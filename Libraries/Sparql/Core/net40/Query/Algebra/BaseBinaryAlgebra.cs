using System;
using System.Collections.Generic;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Algebra
{
    public abstract class BaseBinaryAlgebra
        : IBinaryAlgebra
    {
        protected BaseBinaryAlgebra(IAlgebra lhs, IAlgebra rhs)
        {
            if (lhs == null) throw new ArgumentNullException("lhs");
            if (rhs == null) throw new ArgumentNullException("rhs");

            this.Lhs = lhs;
            this.Rhs = rhs;
        }

        public IAlgebra Lhs { get; private set; }

        public IAlgebra Rhs { get; private set; }

        public abstract IEnumerable<string> ProjectedVariables { get; }

        public abstract IEnumerable<string> FixedVariables { get; }

        public abstract IEnumerable<string> FloatingVariables { get; }

        public abstract void Accept(IAlgebraVisitor visitor);

        public abstract IEnumerable<ISolution> Execute(IAlgebraExecutor executor, IExecutionContext context);

        public abstract bool Equals(IAlgebra other);

        public override string ToString()
        {
            return ToString(new AlgebraFormatter());
        }

        public abstract string ToString(IAlgebraFormatter formatter);
    }
}