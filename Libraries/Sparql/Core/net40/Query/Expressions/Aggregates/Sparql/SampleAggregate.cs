using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Query.Grouping;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Aggregates.Sparql
{
    public class SampleAggregate
        : BaseAggregate
    {

        public SampleAggregate(IExpression arg)
            : base(arg.AsEnumerable()) { }

        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordSample; }
        }

        public override IAccumulator CreateAccumulator()
        {
            return new SampleAccumulator(this.Arguments.First());
        }

        public override IExpression Copy(IEnumerable<IExpression> arguments)
        {
            return new SampleAggregate(arguments.FirstOrDefault());
        }
    }
}
