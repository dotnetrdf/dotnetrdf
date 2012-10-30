using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;

namespace VDS.RDF.Query.Builder
{
    public abstract class SparqlExpression<T> where T : ISparqlExpression
    {
        protected SparqlExpression(T expression)
        {
            Expression = expression;
        }

        public T Expression { get; protected set; }

        public BooleanExpression Eq(SparqlExpression<T> rightExpression)
        {
            var equalsExpression = new EqualsExpression(Expression, rightExpression.Expression);
            return new BooleanExpression(equalsExpression);
        }
    }
}