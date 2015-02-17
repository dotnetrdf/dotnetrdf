using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Aggregation;
using VDS.RDF.Query.Aggregation.XPath;
using VDS.RDF.Query.Expressions.Factories;

namespace VDS.RDF.Query.Expressions.Aggregates.XPath
{
    public class StringJoinAggregate
        : BaseAggregate
    {
        public StringJoinAggregate(IExpression expr)
            : this(expr, null) {}

        public StringJoinAggregate(IExpression expr, IExpression separatorExpr)
            : base(MakeArguments(expr, separatorExpr)) {}

        private static IEnumerable<IExpression> MakeArguments(IExpression expr, IExpression separatorExpr)
        {
            return separatorExpr != null ? new IExpression[] {expr, separatorExpr} : expr.AsEnumerable();
        }

        public override IExpression Copy(IEnumerable<IExpression> args)
        {
            List<IExpression> exprs = args.ToList();
            return exprs.Count == 1 ? new StringJoinAggregate(exprs[0], null) : new StringJoinAggregate(exprs[0], exprs[1]);
        }

        public override string Functor
        {
            get { return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.StringJoin; }
        }

        public override IAccumulator CreateAccumulator()
        {
            return this.Arguments.Count > 1 ? new StringJoinAccumulator(this.Arguments[0], this.Arguments[1]) : new StringJoinAccumulator(this.Arguments[0]);
        }
    }
}
