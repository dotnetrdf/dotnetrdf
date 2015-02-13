using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Query.Aggregation.Leviathan;
using VDS.RDF.Query.Expressions.Factories;

namespace VDS.RDF.Query.Expressions.Aggregates.Leviathan
{
    public class AllAggregate
        : BaseAggregate
    {
        public AllAggregate(IExpression arg)
            : base(arg.AsEnumerable()) { }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            return new AllAggregate(args.FirstOrDefault());
        }

        public override string Functor
        {
            get { return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.All; }
        }

        public override IAccumulator CreateAccumulator()
        {
            return new AllAccumulator(this.Arguments[0]);
        }
    }
}
