using System;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Operators.Numeric;

namespace VDS.RDF.Query.Aggregation.Sparql
{
    public class AverageAccumulator
        : BaseExpressionAccumulator
    {
        private readonly AdditionOperator _adder = new AdditionOperator();
        private readonly DivisionOperator _divisor = new DivisionOperator();
        private readonly IValuedNode[] _args = new IValuedNode[2];
        private IValuedNode _sum = new LongNode(0);
        private long _count = 0;

        public AverageAccumulator(IExpression expr)
            : base(expr)
        {
            this._args[0] = this._sum;
        }

        public override bool Equals(IAccumulator other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is AverageAccumulator)) return false;

            AverageAccumulator sum = (AverageAccumulator) other;
            return this.Expression.Equals(sum.Expression);
        }

        protected internal override void Accumulate(IValuedNode value)
        {
            if (value == null) return;

            // Check that we can add this to the previous argument
            this._args[1] = value;
            if (!this._adder.IsApplicable(this._args)) return;

            // If so go ahead and accumulate it
            // Put the total back into the first entry in our arguments array for next time
            this._sum = this._adder.Apply(this._args);
            this._args[0] = this._sum;
            this._count++;
        }

        public override IValuedNode AccumulatedResult
        {
            get { return this._divisor.Apply(this._sum, new LongNode(this._count)); }
            protected internal set { base.AccumulatedResult = value; }
        }
    }
}
