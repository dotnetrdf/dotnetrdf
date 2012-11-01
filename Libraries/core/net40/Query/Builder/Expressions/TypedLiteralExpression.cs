using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    public abstract class TypedLiteralExpression<T> : LiteralExpression
    {
        protected TypedLiteralExpression(ILiteralNode expression)
            : base(new ConstantTerm(expression))
        {
        }

        protected TypedLiteralExpression(ISparqlExpression expression)
            : base(expression)
        {
        }

        public BooleanExpression Gt(TypedLiteralExpression<T> rightExpression)
        {
            return Gt(Expression, rightExpression);
        }

        public BooleanExpression Lt(TypedLiteralExpression<T> rightExpression)
        {
            return Lt(Expression, rightExpression);
        }

        public BooleanExpression Ge(TypedLiteralExpression<T> rightExpression)
        {
            return Lt(Expression, rightExpression);
        }

        public BooleanExpression Le(TypedLiteralExpression<T> rightExpression)
        {
            return Le(Expression, rightExpression);
        }
    }
}