using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    public class VariableExpression : SparqlExpression
    {
        public VariableExpression(string variable)
            : base(new VariableTerm(variable))
        {
        }

        public new VariableTerm Expression
        {
            get
            {
                return (VariableTerm)base.Expression;
            }
        }

        public static BooleanExpression operator >(VariableExpression leftExpression, VariableExpression rightExpression)
        {
            return Gt(leftExpression.Expression, rightExpression);
        }

        public static BooleanExpression operator <(VariableExpression leftExpression, VariableExpression rightExpression)
        {
            return Lt(leftExpression.Expression, rightExpression);
        }

        public static BooleanExpression operator >=(VariableExpression leftExpression, VariableExpression rightExpression)
        {
            return Ge(leftExpression.Expression, rightExpression);
        }

        public static BooleanExpression operator <=(VariableExpression leftExpression, VariableExpression rightExpression)
        {
            return Le(leftExpression.Expression, rightExpression);
        }

        public static BooleanExpression operator >(VariableExpression leftExpression, LiteralExpression rightExpression)
        {
            return Gt(leftExpression.Expression, rightExpression);
        }

        public static BooleanExpression operator <(VariableExpression leftExpression, LiteralExpression rightExpression)
        {
            return Lt(leftExpression.Expression, rightExpression);
        }

        public static BooleanExpression operator >(LiteralExpression leftExpression, VariableExpression rightExpression)
        {
            return Gt(leftExpression.Expression, rightExpression);
        }

        public static BooleanExpression operator <(LiteralExpression leftExpression, VariableExpression rightExpression)
        {
            return Lt(leftExpression.Expression, rightExpression);
        }

        public static BooleanExpression operator >=(VariableExpression leftExpression, LiteralExpression rightExpression)
        {
            return Ge(leftExpression.Expression, rightExpression);
        }

        public static BooleanExpression operator <=(VariableExpression leftExpression, LiteralExpression rightExpression)
        {
            return Le(leftExpression.Expression, rightExpression);
        }

        public static BooleanExpression operator >=(LiteralExpression leftExpression, VariableExpression rightExpression)
        {
            return Ge(leftExpression.Expression, rightExpression);
        }

        public static BooleanExpression operator <=(LiteralExpression leftExpression, VariableExpression rightExpression)
        {
            return Le(leftExpression.Expression, rightExpression);
        }
    }
}