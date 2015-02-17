using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Query.Aggregation.Leviathan;
using VDS.RDF.Query.Expressions.Factories;

namespace VDS.RDF.Query.Expressions.Aggregates.Leviathan
{
    public class MedianAggregate
        : BaseAggregate
    {
        public MedianAggregate(IExpression arg)
            : base(arg.AsEnumerable()) {}

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            return new MedianAggregate(args.FirstOrDefault());
        }

        public override string Functor
        {
            get { return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Median; }
        }

        public override IAccumulator CreateAccumulator()
        {
            return new MedianAccumulator(this.Arguments[0]);
        }
    }
}