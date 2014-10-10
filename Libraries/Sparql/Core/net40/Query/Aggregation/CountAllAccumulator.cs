using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Aggregation
{
    public class CountAllAccumulator
        : IAccumulator
    {
        private long _count = 0;

        public bool Equals(IAccumulator other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            return other is CountAllAccumulator;
        }

        public void Accumulate(ISolution solution, IExpressionContext context)
        {
            this._count++;
        }

        public IValuedNode AccumulatedResult
        {
            get { return new LongNode(_count); }
        }
    }
}
