using System;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Aggregation
{
    public class SampleAccumulator
        : BaseExpressionAccumulator
    {
        private readonly Random _random = new Random();

        public SampleAccumulator(IExpression expr) 
            : base(expr) {}

        public override bool Equals(IAccumulator other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is SampleAccumulator)) return false;

            SampleAccumulator sample = (SampleAccumulator) other;
            return this.Expression.Equals(sample.Expression);
        }

        protected internal override void Accumulate(IValuedNode value)
        {
            // Ignore null values
            if (value == null) return;

            // If the current value is null take the new value
            if (this.AccumulatedResult == null)
            {
                this.AccumulatedResult = value;
            }

            // Otherwise randomly decide whether to take the new value
            // 50/50 chance we take the new value
            if (this._random.NextDouble() >= 0.5)
            {
                this.AccumulatedResult = value;
            }
        }
    }
}
