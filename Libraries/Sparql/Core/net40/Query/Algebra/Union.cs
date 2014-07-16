using System;
using System.Collections.Generic;
using VDS.RDF.Query.Engine;

namespace VDS.RDF.Query.Algebra
{
    public class Union
        : IBinaryAlgebra
    {
        public Union(IAlgebra lhs, IAlgebra rhs)
        {
            if (lhs == null) throw new ArgumentNullException("lhs");
            if (rhs == null) throw new ArgumentNullException("rhs");

            this.Lhs = lhs;
            this.Rhs = rhs;
        }

        public bool Equals(IAlgebra other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is Union)) return false;

            Union union = (Union) other;
            return this.Lhs.Equals(union.Lhs) && this.Rhs.Equals(union.Rhs);
        }

        public void Accept(IAlgebraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<ISet> Execute(IAlgebraExecutor executor, IExecutionContext context)
        {
            return executor.Execute(this, context);
        }

        public IAlgebra Lhs { get; private set; }

        public IAlgebra Rhs { get; private set; }
    }
}
