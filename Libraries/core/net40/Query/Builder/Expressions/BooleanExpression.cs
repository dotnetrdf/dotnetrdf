using System;
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

        public static BooleanExpression operator &(BooleanExpression left, BooleanExpression right)
        {
            return EitherArgumentNull(left, right) ?? new BooleanExpression(new AndExpression(left.Expression, right.Expression));
        }

        public static BooleanExpression operator |(BooleanExpression left, BooleanExpression right)
        {
            return EitherArgumentNull(left, right) ?? new BooleanExpression(new OrExpression(left.Expression, right.Expression));
        }

        private static BooleanExpression EitherArgumentNull(BooleanExpression left, BooleanExpression right)
        {
            if(left == null && right == null)
            {
                throw new ArgumentNullException("left", "Either operator argument must be not-null");
            }
            if(left == null)
            {
                return right;
            }
            if(right == null)
            {
                return left;
            }
            return null;
        }

        public static bool operator false(BooleanExpression left)
        {
            return false;
        }

        public static bool operator true(BooleanExpression left)
        {
            return false;
        }

        public static BooleanExpression operator !(BooleanExpression expression)
        {
            return new BooleanExpression(new NotExpression(expression.Expression));
        }
    }
}