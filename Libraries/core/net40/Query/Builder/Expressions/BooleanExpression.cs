using System;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Conditional;

namespace VDS.RDF.Query.Builder.Expressions
{
    /// <summary>
    /// Represents a logical expression
    /// </summary>
    public sealed class BooleanExpression : TypedLiteralExpression<bool>
    {
        /// <summary>
        /// Wraps the <paramref name="expression"/> as a boolean expression
        /// </summary>
        public BooleanExpression(ISparqlExpression expression)
            : base(expression)
        {
        }

#pragma warning disable 1591
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
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            {
                throw new ArgumentNullException("left", "Either operator argument must be not-null");
            }
            if (ReferenceEquals(left, null))
            {
                return right;
            }
            if (ReferenceEquals(right, null))
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
#pragma warning restore 1591
    }
}