using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Aggregates.Sparql
{
    public class CountDistinctAggregate
        : BaseDistinctAggregate
    {

        public CountDistinctAggregate(IExpression arg)
            : base(arg.AsEnumerable()) { }

        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordCount; }
        }

        public override IAccumulator CreateAccumulator()
        {
            return new DistinctAccumulator(new CountAccumulator(this.Arguments[0]));
        }

        public override IExpression Copy(IEnumerable<IExpression> arguments)
        {
            return new CountDistinctAggregate(arguments.FirstOrDefault());
        }
    }
}