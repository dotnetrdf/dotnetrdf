using System;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Grouping
{
    public abstract class BaseExpressionAccumulator
        : IAccumulator
    {
        protected BaseExpressionAccumulator(IExpression expr)
        {
            if (expr == null) throw new ArgumentNullException("expr");
            this.Expression = expr;
        }

        protected BaseExpressionAccumulator(IExpression expr, IValuedNode initialValue)
            : this(expr)
        {
            this.AccumulatedResult = initialValue;
        }

        public IExpression Expression { get; private set; }

        public abstract bool Equals(IAccumulator other);

        public void Accumulate(ISolution solution, IExpressionContext context)
        {
            try
            {
                Accumulate(this.Expression.Evaluate(solution, context));
            }
            catch (RdfQueryException)
            {
                Accumulate(null);
            }
        }

        protected abstract void Accumulate(IValuedNode value);

        public virtual IValuedNode AccumulatedResult { get; protected set; }
    }
}
