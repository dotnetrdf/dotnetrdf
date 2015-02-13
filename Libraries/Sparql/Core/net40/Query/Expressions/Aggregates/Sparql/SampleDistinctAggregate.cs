using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Query.Grouping;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Aggregates.Sparql
{
    public class SampleDistinctAggregate
        : BaseDistinctAggregate
    {

        public SampleDistinctAggregate(IExpression arg)
            : base(arg.AsEnumerable()) { }

        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordSample; }
        }

        public override IAccumulator CreateAccumulator()
        {
            // Note that unlike other DISTINCT aggregates distinctness is completely irrelevant for SAMPLE
            // since duplicates are irrelevant for sampling a value
            // Therefore we intentionally DO NOT wrap this with a DistinctAccumulator
            return new SampleAccumulator(this.Arguments.First());
        }

        public override IExpression Copy(IEnumerable<IExpression> arguments)
        {
            return new SampleDistinctAggregate(arguments.FirstOrDefault());
        }
    }
}
