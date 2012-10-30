using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;

namespace VDS.RDF.Query.Builder.Expressions
{
    public abstract class SparqlExpression
    {
        protected SparqlExpression(ISparqlExpression expression)
        {
            Expression = expression;
        }

        public ISparqlExpression Expression { get; protected set; }

        public BooleanExpression Eq(SparqlExpression rightExpression)
        {
            var equalsExpression = new EqualsExpression(Expression, rightExpression.Expression);
            return new BooleanExpression(equalsExpression);
        }

        public BooleanExpression Gt(SparqlExpression rightExpression)
        {
            var equalsExpression = new GreaterThanExpression(Expression, rightExpression.Expression);
            return new BooleanExpression(equalsExpression);
        }

        public BooleanExpression Lt(SparqlExpression rightExpression)
        {
            var equalsExpression = new LessThanExpression(Expression, rightExpression.Expression);
            return new BooleanExpression(equalsExpression);
        }

        public BooleanExpression Ge<T>(TypedSparqlExpression<T> rightExpression) where T : ISparqlExpression
        {
            var equalsExpression = new GreaterThanOrEqualToExpression(Expression, rightExpression.Expression);
            return new BooleanExpression(equalsExpression);
        }

        public BooleanExpression Le(SparqlExpression rightExpression)
        {
            var equalsExpression = new LessThanOrEqualToExpression(Expression, rightExpression.Expression);
            return new BooleanExpression(equalsExpression);
        }
    }
}