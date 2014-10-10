using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Grouping;

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
            return other is CountAccumulator;
        }

        protected override void Accumulate(IValuedNode value)
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