using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Query.Aggregation.Sparql;
using VDS.RDF.Query.Sorting;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Aggregates.Sparql
{
    public class MaxAggregate
        : BaseAggregate
    {
        public MaxAggregate(IExpression expr)
            : base(expr.AsEnumerable()) { }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            return new MaxAggregate(args.First());
        }

        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordMax; }
        }

        public override IAccumulator CreateAccumulator()
        {
            return new SortingAccumulator(this.Arguments[0], new SparqlOrderingComparer());
        }
    }
}
