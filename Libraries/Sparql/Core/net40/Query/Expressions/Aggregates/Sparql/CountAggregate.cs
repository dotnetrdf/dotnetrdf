using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Query.Grouping;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Aggregates.Sparql
{
    public class CountAggregate
        : BaseAggregate
    {
        private readonly IExpression _argument;

        public CountAggregate(IExpression arg)
        {
            if (arg == null) throw new ArgumentNullException("arg");
            this._argument = arg;
        }

        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordCount; }
        }

        public override IAccumulator CreateAccumulator()
        {
            return new CountAccumulator(this._argument);
        }

        public override IExpression Copy(IEnumerable<IExpression> arguments)
        {
            return new CountAggregate(arguments.FirstOrDefault());
        }
    }
}
