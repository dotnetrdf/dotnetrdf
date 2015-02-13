using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Collections;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Query.Expressions.Factories;
using VDS.RDF.Query.Sorting;

namespace VDS.RDF.Query.Expressions.Aggregates.Leviathan
{
    public class NumericMinAggregate
        : BaseAggregate
    {
        public NumericMinAggregate(IExpression arg)
            : base(arg.AsEnumerable()) { }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            return new NumericMinAggregate(args.FirstOrDefault());
        }

        public override string Functor
        {
            get { return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.NumericMin; }
        }

        public override IAccumulator CreateAccumulator()
        {
            return new NumericSortingAccumulator(this.Arguments[0], new ReversedComparer<IValuedNode>(new SparqlNodeComparer()));
        }
    }
}
