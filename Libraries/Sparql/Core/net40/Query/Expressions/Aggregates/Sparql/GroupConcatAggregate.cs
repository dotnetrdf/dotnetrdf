using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Expressions.Aggregates.Sparql
{
    public class GroupConcatAggregate
        : BaseAggregate
    {
        public GroupConcatAggregate(IExpression expr)
            : this(expr, null) {}

        public GroupConcatAggregate(IExpression expr, IExpression separatorExpr)
            : base(MakeArguments(expr, separatorExpr)) {}

        private static IEnumerable<IExpression> MakeArguments(IExpression expr, IExpression separatorExpr)
        {
            return separatorExpr != null ? new IExpression[] {expr, separatorExpr} : expr.AsEnumerable();
        }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            List<IExpression> exprs = args.ToList();
            return exprs.Count == 1 ? new GroupConcatAggregate(exprs[0], null) : new GroupConcatAggregate(exprs[0], exprs[1]);
        }

        public override string Functor
        {
            get { return SparqlSpecsHelper.SparqlKeywordGroupConcat; }
        }

        public override IAccumulator CreateAccumulator()
        {
            return this.Arguments.Count > 1 ? new GroupConcatAccumulator(this.Arguments[0], this.Arguments[1]) : new GroupConcatAccumulator(this.Arguments[0]);
        }

        // TODO Need to override string formatting to add with ;SEPARATOR = "" syntax
    }
}