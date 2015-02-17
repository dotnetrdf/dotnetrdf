using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Query.Aggregation.Leviathan;
using VDS.RDF.Query.Aggregation.Sparql;
using VDS.RDF.Query.Expressions.Factories;

namespace VDS.RDF.Query.Expressions.Aggregates.Leviathan
{
    public class ModeDistinctAggregate
        : BaseDistinctAggregate
    {
        public ModeDistinctAggregate(IExpression arg)
            : base(arg.AsEnumerable()) {}

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            return new ModeDistinctAggregate(args.FirstOrDefault());
        }

        public override string Functor
        {
            get { return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Mode; }
        }

        public override IAccumulator CreateAccumulator()
        {
            // Asking for mode of DISTINCT values is essentially pointless since if
            // DISTINCT is applied every value is equally popular so this is essentially the same
            // as SAMPLE
            // This does mean that DISTINCT applied to mode is non-deterministic but seeing as asking
            // for it is non-sensical anyway we don't really care
            return new SampleAccumulator(this.Arguments[0]);
        }
    }
}