using System.Collections.Generic;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Aggregation.Sparql
{
    public class CountAllDistinctAccumulator
        : IAccumulator
    {
        private readonly ISet<ISolution> _solutions = new HashSet<ISolution>();
        private long _count = 0;

        public bool Equals(IAccumulator other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            return other is CountAllDistinctAccumulator;
        }

        public void Accumulate(ISolution solution, IExpressionContext context)
        {
            if (this._solutions.Add(solution))
            {
                this._count++;
            }
        }

        public IValuedNode AccumulatedResult
        {
            get { return new LongNode(_count); }
        }
    }
}