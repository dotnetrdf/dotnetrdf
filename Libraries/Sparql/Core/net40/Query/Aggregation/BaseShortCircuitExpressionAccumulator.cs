using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Aggregation
{
    /// <summary>
    /// An abstract expression based accumulator that supports short circuiting
    /// </summary>
    public abstract class BaseShortCircuitExpressionAccumulator 
        : BaseExpressionAccumulator {

        protected BaseShortCircuitExpressionAccumulator(IExpression expr, IValuedNode initialValue) 
            : base(expr, initialValue) {}

        /// <summary>
        /// Gets/Sets the short circuited result assuming evaluation was short circuited
        /// </summary>
        protected virtual IValuedNode ShortCircuitResult { get; set; }

        /// <summary>
        /// Gets/Sets whether to short circuit evaluation
        /// </summary>
        /// <remarks>
        /// This can be used by derived implementations to indicate that they have already determined the result and accumulating further solutions will not change that result.  This allows evaluation to be sped up considerably in some cases
        /// </remarks>
        protected internal bool ShortCircuit { get; set; }

        /// <summary>
        /// Gets/Sets the actual accumulated result assuming evaluation was not short circuited
        /// </summary>
        protected virtual IValuedNode ActualResult { get; set; }

        public sealed override IValuedNode AccumulatedResult
        {
            get
            {
                return this.ShortCircuit ? this.ShortCircuitResult : this.ActualResult;
            }
            protected internal set { base.AccumulatedResult = value; }
        }

        public sealed override void Accumulate(ISolution solution, IExpressionContext context)
        {
            // Can skip evaluation if short circuiting as the final result is already known
            if (this.ShortCircuit) return;

            // Otherwise evaluate expression and try and accumulate
            base.Accumulate(solution, context);
        }
    }
}