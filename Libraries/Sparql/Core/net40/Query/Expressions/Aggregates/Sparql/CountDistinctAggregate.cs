using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Aggregation;

namespace VDS.RDF.Query.Expressions.Aggregates.Sparql
{
    public class CountDistinctAggregate
        : CountAggregate
    {
        public CountDistinctAggregate(IExpression arg) 
            : base(arg) {}

        public override IAccumulator CreateAccumulator()
        {
            return new DistinctAccumulator(new CountAccumulator(this.Arguments[0]));
        }

        public override IExpression Copy(IEnumerable<IExpression> arguments)
        {
            return new CountAggregate(arguments.FirstOrDefault());
        }

        // TODO Need to override string formatting to include DISTINCT keyword
    }
}
