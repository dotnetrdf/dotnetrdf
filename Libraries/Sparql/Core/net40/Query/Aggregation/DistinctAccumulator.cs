using System.Collections.Generic;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Aggregation
{
    /// <summary>
    /// A decorator around other accumulators which only accumulates distinct values
    /// </summary>
    public class DistinctAccumulator
        : BaseExpressionAccumulator
    {
        private readonly ISet<IValuedNode> _values = new HashSet<IValuedNode>();

        public DistinctAccumulator(BaseExpressionAccumulator accumulator)
            : this(accumulator, null) { }

        public DistinctAccumulator(BaseExpressionAccumulator accumulator, IValuedNode initialValue)
            : base(accumulator.Expression, initialValue)
        {
            this.InnerAccumulator = accumulator;
        }

        public BaseExpressionAccumulator InnerAccumulator { get; private set; }

        public override bool Equals(IAccumulator other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is DistinctAccumulator)) return false;

            DistinctAccumulator distinct = (DistinctAccumulator) other;
            return this.InnerAccumulator.Equals(distinct.InnerAccumulator);
        }

        protected internal override void Accumulate(IValuedNode value)
        {
            // Check if we've already seen this item
            if (!this._values.Add(value)) return;

            this.InnerAccumulator.Accumulate(value);
        }

        public override IValuedNode AccumulatedResult
        {
            get { return this.InnerAccumulator.AccumulatedResult; }
            protected internal set {this.InnerAccumulator.AccumulatedResult = value; }
        }
    }
}
