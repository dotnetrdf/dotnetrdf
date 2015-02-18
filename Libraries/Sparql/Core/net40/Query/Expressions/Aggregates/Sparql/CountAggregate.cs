using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Query.Aggregation.Sparql;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Aggregates.Sparql
{
    public class CountAggregate
        : BaseAggregate
    {
        public CountAggregate(IExpression arg)
            : base(arg.AsEnumerable()) { }

        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordCount; }
        }

        public override IAccumulator CreateAccumulator()
        {
            return new CountAccumulator(this.Arguments.First());
        }

        public override IExpression Copy(IEnumerable<IExpression> arguments)
        {
            return new CountAggregate(arguments.FirstOrDefault());
        }
    }
}
