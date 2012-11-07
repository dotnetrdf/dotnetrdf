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

        public static BooleanExpression operator >(TypedLiteralExpression<T> left, TypedLiteralExpression<T> right)
        {
            return Gt(left.Expression, right);
        }

        public static BooleanExpression operator <(TypedLiteralExpression<T> left, TypedLiteralExpression<T> right)
        {
            return Lt(left.Expression, right);
        }

        public static BooleanExpression operator >=(TypedLiteralExpression<T> left, TypedLiteralExpression<T> right)
        {
            return Ge(left.Expression, right);
        }

        public static BooleanExpression operator <=(TypedLiteralExpression<T> left, TypedLiteralExpression<T> right)
        {
            return Le(left.Expression, right);
        }
    }
}