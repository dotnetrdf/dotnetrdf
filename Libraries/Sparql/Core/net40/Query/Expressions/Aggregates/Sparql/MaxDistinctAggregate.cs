using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Query.Sorting;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Aggregates.Sparql
{
    public class MaxDistinctAggregate
        : BaseDistinctAggregate
    {
        public MaxDistinctAggregate(IExpression expr)
            : base(expr.AsEnumerable()) {}

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            return new MaxDistinctAggregate(args.First());
        }

        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordMax; }
        }

        public override IAccumulator CreateAccumulator()
        {
            // Note that unlike other DISTINCT aggregates distinctness is completely irrelevant for MAX
            // since duplicates are irrelevant for determining the maximum value
            // Therefore we intentionally DO NOT wrap this with a DistinctAccumulator
            return new SortingAccumulator(this.Arguments[0], new SparqlOrderingComparer());
        }
    }
}