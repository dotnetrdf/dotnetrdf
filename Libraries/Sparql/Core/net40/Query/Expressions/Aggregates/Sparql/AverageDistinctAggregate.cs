using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Aggregates.Sparql
{
    public class AverageDistinctAggregate
        : BaseDistinctAggregate
    {
        public AverageDistinctAggregate(IExpression arg)
            : base(arg.AsEnumerable()) { }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            return new AverageDistinctAggregate(args.First());
        }

        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordAvg; }
        }

        public override IAccumulator CreateAccumulator()
        {
            return new DistinctAccumulator(new AverageAccumulator(this.Arguments[0]));
        }
    }
}
