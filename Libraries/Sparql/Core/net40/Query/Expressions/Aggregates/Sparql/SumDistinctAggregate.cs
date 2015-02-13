using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Query.Aggregation.Sparql;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Aggregates.Sparql
{
    public class SumDistinctAggregate
        : BaseDistinctAggregate
    {
        public SumDistinctAggregate(IExpression arg)
            : base(arg.AsEnumerable()) { }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            return new SumDistinctAggregate(args.First());
        }

        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordSum; }
        }

        public override IAccumulator CreateAccumulator()
        {
            return new DistinctAccumulator(new SumAccumulator(this.Arguments.First()));
        }
    }
}
