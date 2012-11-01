using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Conditional;

namespace VDS.RDF.Query.Builder.Expressions
{
    public sealed class BooleanExpression : TypedLiteralExpression<bool>
    {
        public BooleanExpression(ISparqlExpression expression)
            : base(expression)
        {
        }

        public BooleanExpression And(BooleanExpression rightExpression)
        {
            return new BooleanExpression(new AndExpression(Expression, rightExpression.Expression));
        }

        public BooleanExpression Or(BooleanExpression rightExpression)
        {
            return new BooleanExpression(new OrExpression(Expression, rightExpression.Expression));
        }
    }
}