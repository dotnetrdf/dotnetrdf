using System;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    public static class LiteralExtensions
    {
        public static ILiteralNode ToLiteral<T>(this T numericValue)
        {
            if (typeof(T) == typeof(int))
            {
                return ((int)(object)numericValue).ToLiteral(LiteralExpression.NodeFactory);
            }
            if (typeof(T) == typeof(decimal))
            {
                return ((decimal)(object)numericValue).ToLiteral(LiteralExpression.NodeFactory);
            }
            if (typeof(T) == typeof(short))
            {
                return ((short)(object)numericValue).ToLiteral(LiteralExpression.NodeFactory);
            }
            if (typeof(T) == typeof(long))
            {
                return ((long)(object)numericValue).ToLiteral(LiteralExpression.NodeFactory);
            }
            if (typeof(T) == typeof(float))
            {
                return ((float)(object)numericValue).ToLiteral(LiteralExpression.NodeFactory);
            }
            if (typeof(T) == typeof(double))
            {
                return ((double)(object)numericValue).ToLiteral(LiteralExpression.NodeFactory);
            }
            if (typeof(T) == typeof(byte))
            {
                return ((byte)(object)numericValue).ToLiteral(LiteralExpression.NodeFactory);
            }
            if (typeof(T) == typeof(sbyte))
            {
                return ((sbyte)(object)numericValue).ToLiteral(LiteralExpression.NodeFactory);
            }
            if (typeof(T) == typeof(string))
            {
                return ((string)(object)numericValue).ToLiteral(LiteralExpression.NodeFactory);
            }
            if (typeof(T) == typeof(DateTime))
            {
                return ((DateTime)(object)numericValue).ToLiteral(LiteralExpression.NodeFactory);
            }
            if (typeof(T) == typeof(TimeSpan))
            {
                return ((TimeSpan)(object)numericValue).ToLiteral(LiteralExpression.NodeFactory);
            }
            if (typeof(T) == typeof(bool))
            {
                return ((bool)(object)numericValue).ToLiteral(LiteralExpression.NodeFactory);
            }

            throw new ArgumentException(string.Format("Unsupported type for literal node: {0}", typeof(T)));
        }
    }

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
            return Gt(left.Expression, right.Expression);
        }

        public static BooleanExpression operator <(TypedLiteralExpression<T> left, TypedLiteralExpression<T> right)
        {
            return Lt(left.Expression, right.Expression);
        }

        public static BooleanExpression operator >=(TypedLiteralExpression<T> left, TypedLiteralExpression<T> right)
        {
            return Ge(left.Expression, right.Expression);
        }

        public static BooleanExpression operator <=(TypedLiteralExpression<T> left, TypedLiteralExpression<T> right)
        {
            return Le(left.Expression, right.Expression);
        }

        public static BooleanExpression operator ==(TypedLiteralExpression<T> left, T right)
        {
            return new BooleanExpression(new EqualsExpression(left.Expression, CreateConstantTerm(right)));
        }

        public static BooleanExpression operator !=(TypedLiteralExpression<T> left, T right)
        {
            return !(left == right);
        }

        public static BooleanExpression operator ==(T left, TypedLiteralExpression<T> right)
        {
            return new BooleanExpression(new EqualsExpression(CreateConstantTerm(left), right.Expression));
        }

        public static BooleanExpression operator !=(T left, TypedLiteralExpression<T> right)
        {
            return !(left == right);
        }
    }
}