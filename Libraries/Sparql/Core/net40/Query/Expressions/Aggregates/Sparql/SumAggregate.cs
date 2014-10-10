using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Query.Grouping;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Aggregates.Sparql
{
    public class SumAggregate
        : BaseAggregate
    {
        public SumAggregate(IExpression arg)
            : base(arg.AsEnumerable()) { }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            return new SumAggregate(args.First());
        }

        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordSum; }
        }

        public override IAccumulator CreateAccumulator()
        {
            return new SumAccumulator(this.Arguments.First());
        }
    }
}
