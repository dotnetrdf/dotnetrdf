using System;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Aggregation
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
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            this.AccumulatedResult = initialValue;
        }

        public IExpression Expression { get; private set; }

        public abstract bool Equals(IAccumulator other);

        public virtual void Accumulate(ISolution solution, IExpressionContext context)
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

        /// <summary>
        /// Accumulates the actual value of evaluating the expression
        /// </summary>
        /// <param name="value">Value, will be null if the expression evaluated to an error</param>
        protected internal abstract void Accumulate(IValuedNode value);

        /// <summary>
        /// Gets/Sets the accumulated result
        /// </summary>
        public virtual IValuedNode AccumulatedResult { get; protected internal set; }
    }
}