using System;
using System.Collections.Generic;
using VDS.Common.Collections;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Algebra
{
    public class Filter
        : BaseUnaryAlgebra
    {
        public Filter(IAlgebra innerAlgebra, IEnumerable<IExpression> expressions)
            : base(innerAlgebra)
        {
            if (expressions == null) throw new ArgumentNullException("expressions");
            this.Expressions = new MaterializedImmutableView<IExpression>(expressions);
        }

        public ICollection<IExpression> Expressions { get; private set; }

        public override void Accept(IAlgebraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override IEnumerable<ISet> Execute(IAlgebraExecutor executor, IExecutionContext context)
        {
            return executor.Execute(this, context);
        }

        public override bool Equals(IAlgebra other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is Filter)) return false;

            Filter f = (Filter) other;
            if (this.Expressions.Count != f.Expressions.Count) return false;

            IEnumerator<IExpression> exprs = this.Expressions.GetEnumerator();
            IEnumerator<IExpression> otherExprs = f.Expressions.GetEnumerator();
            while (exprs.MoveNext())
            {
                if (!otherExprs.MoveNext()) return false;
                if (exprs.Current.Equals(otherExprs.Current)) return false;
            }
            return true;
        }
    }
}
