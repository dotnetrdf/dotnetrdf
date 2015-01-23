using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Aggregation
{
    public class CountAccumulator
        : BaseExpressionAccumulator
    {
        private long _count = 0;

        public CountAccumulator(IExpression expr) 
            : base(expr, new LongNode(0)) {}

        public override bool Equals(IAccumulator other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is CountAccumulator)) return false;

            CountAccumulator count = (CountAccumulator) other;
            return this.Expression.Equals(count.Expression);
        }

        protected internal override void Accumulate(IValuedNode value)
        {
            if (value == null) return;
            this._count++;
        }

        public override IValuedNode AccumulatedResult
        {
            get { return new LongNode(_count); }
        }
    }
}