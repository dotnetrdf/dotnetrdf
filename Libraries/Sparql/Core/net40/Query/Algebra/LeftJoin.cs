using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Algebra
{
    public class LeftJoin
        : BaseBinaryAlgebra
    {
        public LeftJoin(IAlgebra lhs, IAlgebra rhs)
            : this(lhs, rhs, Enumerable.Empty<IExpression>()) { }

        public LeftJoin(IAlgebra lhs, IAlgebra rhs, IEnumerable<IExpression> expressions)
            : base(lhs, rhs)
        {
            if (expressions == null) throw new ArgumentNullException("expressions");
            this.Expressions = expressions.ToList().AsReadOnly();
        }

        public IList<IExpression> Expressions { get; private set; }

        public override IEnumerable<string> ProjectedVariables
        {
            get { return this.Lhs.ProjectedVariables.Concat(this.Rhs.ProjectedVariables).Distinct(); }
        }

        public override IEnumerable<string> FixedVariables
        {
            get { return this.Lhs.ProjectedVariables; }
        }

        public override IEnumerable<string> FloatingVariables
        {
            get { return this.Lhs.FloatingVariables.Concat(this.Rhs.ProjectedVariables).Except(this.Lhs.FixedVariables); }
        }

        public override void Accept(IAlgebraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override IEnumerable<ISolution> Execute(IAlgebraExecutor executor, IExecutionContext context)
        {
            return executor.Execute(this, context);
        }

        public override bool Equals(IAlgebra other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is LeftJoin)) return false;

            LeftJoin lj = (LeftJoin) other;
            if (!this.Lhs.Equals(lj.Lhs) || !this.Rhs.Equals(lj.Rhs)) return false;
            for (int i = 0; i < this.Expressions.Count; i++)
            {
                if (!this.Expressions[i].Equals(lj.Expressions[i])) return false;
            }
            return true;
        }
    }
}
