using System.Collections.Generic;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Aggregation
{
    /// <summary>
    /// A decorator around other accumulators which only accumulates distinct values
    /// </summary>
    public class DistinctShortCircuitAccumulator
        : BaseShortCircuitExpressionAccumulator
    {
        private readonly ISet<IValuedNode> _values = new HashSet<IValuedNode>();

        public DistinctShortCircuitAccumulator(BaseShortCircuitExpressionAccumulator accumulator)
            : this(accumulator, null) { }

        public DistinctShortCircuitAccumulator(BaseShortCircuitExpressionAccumulator accumulator, IValuedNode initialValue)
            : base(accumulator.Expression, initialValue)
        {
            this.InnerAccumulator = accumulator;
        }

        public BaseShortCircuitExpressionAccumulator InnerAccumulator { get; private set; }

        public override bool Equals(IAccumulator other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is DistinctShortCircuitAccumulator)) return false;

            DistinctShortCircuitAccumulator distinct = (DistinctShortCircuitAccumulator) other;
            return this.InnerAccumulator.Equals(distinct.InnerAccumulator);
        }

        protected internal override void Accumulate(IValuedNode value)
        {
            if (this.InnerAccumulator.ShortCircuit) return;

            // Check if we've already seen this item
            if (!this._values.Add(value)) return;

            this.InnerAccumulator.Accumulate(value);
        }
    }
}
