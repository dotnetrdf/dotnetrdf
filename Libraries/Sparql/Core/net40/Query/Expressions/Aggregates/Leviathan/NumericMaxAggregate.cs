using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Query.Expressions.Factories;
using VDS.RDF.Query.Sorting;

namespace VDS.RDF.Query.Expressions.Aggregates.Leviathan
{
    public class NumericMaxAggregate
        : BaseAggregate
    {
        public NumericMaxAggregate(IExpression arg)
            : base(arg.AsEnumerable()) { }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            return new NumericMaxAggregate(args.FirstOrDefault());
        }

        public override string Functor
        {
            get { return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.NumericMax; }
        }

        public override IAccumulator CreateAccumulator()
        {
            return new NumericSortingAccumulator(this.Arguments[0], new SparqlNodeComparer());
        }
    }
}
