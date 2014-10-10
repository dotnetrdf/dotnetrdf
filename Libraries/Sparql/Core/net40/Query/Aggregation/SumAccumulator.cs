using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Operators.Numeric;

namespace VDS.RDF.Query.Aggregation
{
    public class SumAccumulator
        : BaseExpressionAccumulator
    {
        private readonly AdditionOperator _adder = new AdditionOperator();
        private readonly IValuedNode[] _args = new IValuedNode[2];

        public SumAccumulator(IExpression expr)
            : base(expr, new LongNode(0))
        {
            this._args[0] = this.AccumulatedResult;
        }

        public override bool Equals(IAccumulator other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is SumAccumulator)) return false;

            SumAccumulator sum = (SumAccumulator) other;
            return this.Expression.Equals(sum.Expression);
        }

        protected override void Accumulate(IValuedNode value)
        {
            if (value == null) return;

            // Check that we can add this to the previous argument
            this._args[1] = value;
            if (!this._adder.IsApplicable(this._args)) return;

            // If so go ahead and accumulate it
            // Put the total back into the first entry in our arguments array for next time
            this.AccumulatedResult = this._adder.Apply(this._args);
            this._args[0] = this.AccumulatedResult;
        }
    }
}
