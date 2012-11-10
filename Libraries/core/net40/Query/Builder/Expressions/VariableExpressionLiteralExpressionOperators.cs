using System;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    public partial class VariableExpression
    {

        public static BooleanExpression operator >(VariableExpression left, LiteralExpression right)
        {
            return Gt(left.Expression, right.Expression);
        }

        public static BooleanExpression operator <(VariableExpression left, LiteralExpression right)
        {
            return Lt(left.Expression, right.Expression);
        }

        public static BooleanExpression operator >(LiteralExpression left, VariableExpression right)
        {
            return Gt(left.Expression, right.Expression);
        }

        public static BooleanExpression operator <(LiteralExpression left, VariableExpression right)
        {
            return Lt(left.Expression, right.Expression);
        }

        public static BooleanExpression operator >=(VariableExpression left, LiteralExpression right)
        {
            return Ge(left.Expression, right.Expression);
        }

        public static BooleanExpression operator <=(VariableExpression left, LiteralExpression right)
        {
            return Le(left.Expression, right.Expression);
        }

        public static BooleanExpression operator >=(LiteralExpression left, VariableExpression right)
        {
            return Ge(left.Expression, right.Expression);
        }

        public static BooleanExpression operator <=(LiteralExpression left, VariableExpression right)
        {
            return Le(left.Expression, right.Expression);
        }
    }
}