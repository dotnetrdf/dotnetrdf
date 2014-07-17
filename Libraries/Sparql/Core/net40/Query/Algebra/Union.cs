using System;
using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<string> ProjectedVariables
        {
            get { return this.Lhs.ProjectedVariables.Concat(this.Rhs.ProjectedVariables).Distinct(); }
        }

        public IEnumerable<string> FixedVariables
        {
            get { return this.Lhs.FixedVariables.Intersect(this.Rhs.FixedVariables).Except(this.FloatingVariables); }
        }

        public IEnumerable<string> FloatingVariables
        {
            get { return this.Lhs.FloatingVariables.Concat(this.Rhs.FloatingVariables).Distinct(); }
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