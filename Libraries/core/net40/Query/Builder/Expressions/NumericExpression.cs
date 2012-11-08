using System;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Arithmetic;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    public class NumericExpression<T> : TypedLiteralExpression<T>
    {
        public NumericExpression(T numericValue)
            : base(new ConstantTerm(CreateLiteralNode(numericValue)))
        {
        }

        internal NumericExpression(ISparqlExpression expression)
            : base(expression)
        {
        }

        private static ILiteralNode CreateLiteralNode(T numericValue)
        {
            if (typeof(T) == typeof(int))
            {
                return ((int)(object)numericValue).ToLiteral(NodeFactory);
            }
            if (typeof(T) == typeof(decimal))
            {
                return ((decimal)(object)numericValue).ToLiteral(NodeFactory);
            }
            if (typeof(T) == typeof(short))
            {
                return ((short)(object)numericValue).ToLiteral(NodeFactory);
            }
            if (typeof(T) == typeof(long))
            {
                return ((long)(object)numericValue).ToLiteral(NodeFactory);
            }
            if (typeof(T) == typeof(float))
            {
                return ((float)(object)numericValue).ToLiteral(NodeFactory);
            }
            if (typeof(T) == typeof(double))
            {
                return ((double)(object)numericValue).ToLiteral(NodeFactory);
            }
            if (typeof(T) == typeof(byte))
            {
                return ((byte)(object)numericValue).ToLiteral(NodeFactory);
            }
            if (typeof(T) == typeof(sbyte))
            {
                return ((sbyte)(object)numericValue).ToLiteral(NodeFactory);
            }

            throw new ArgumentException(string.Format("Unsupported type for numeric node: {0}", typeof(T)));
        }

        public static implicit operator NumericExpression(NumericExpression<T> expression)
        {
            return new NumericExpression(expression.Expression);
        }

        public static NumericExpression<T> operator *(NumericExpression<T> left, T right)
        {
            return left * new NumericExpression<T>(right);
        }

        public static NumericExpression<T> operator *(NumericExpression<T> left, NumericExpression<T> right)
        {
            var multiplication = new MultiplicationExpression(left.Expression, right.Expression);
            return new NumericExpression<T>(multiplication);
        }

        public static NumericExpression operator *(NumericExpression<T> left, NumericExpression right)
        {
            return (NumericExpression)left * right;
        }

        public static NumericExpression<T> operator *(T left, NumericExpression<T> right)
        {
            return new NumericExpression<T>(left) * right;
        }

        public static NumericExpression<T> operator /(NumericExpression<T> left, T right)
        {
            return left / new NumericExpression<T>(right);
        }

        public static NumericExpression<T> operator /(NumericExpression<T> left, NumericExpression<T> right)
        {
            var multiplication = new DivisionExpression(left.Expression, right.Expression);
            return new NumericExpression<T>(multiplication);
        }

        public static NumericExpression operator /(NumericExpression<T> left, NumericExpression right)
        {
            return (NumericExpression)left / right;
        }

        public static NumericExpression<T> operator /(T left, NumericExpression<T> right)
        {
            return new NumericExpression<T>(left) / right;
        }

        public static NumericExpression<T> operator +(NumericExpression<T> left, T right)
        {
            return left + new NumericExpression<T>(right);
        }

        public static NumericExpression<T> operator +(NumericExpression<T> left, NumericExpression<T> right)
        {
            var multiplication = new AdditionExpression(left.Expression, right.Expression);
            return new NumericExpression<T>(multiplication);
        }

        public static NumericExpression operator +(NumericExpression<T> left, NumericExpression right)
        {
            return (NumericExpression)left + right;
        }

        public static NumericExpression<T> operator +(T left, NumericExpression<T> right)
        {
            return new NumericExpression<T>(left) + right;
        }

        public static NumericExpression<T> operator -(NumericExpression<T> left, T right)
        {
            return left - new NumericExpression<T>(right);
        }

        public static NumericExpression<T> operator -(NumericExpression<T> left, NumericExpression<T> right)
        {
            var multiplication = new SubtractionExpression(left.Expression, right.Expression);
            return new NumericExpression<T>(multiplication);
        }

        public static NumericExpression operator -(NumericExpression<T> left, NumericExpression right)
        {
            return (NumericExpression)left - right;
        }

        public static NumericExpression<T> operator -(T left, NumericExpression<T> right)
        {
            return new NumericExpression<T>(left) - right;
        }

        public static BooleanExpression operator >(NumericExpression<T> leftExpression, NumericExpression rightExpression)
        {
            return (NumericExpression)leftExpression > rightExpression;
        }

        public static BooleanExpression operator <(NumericExpression<T> leftExpression, NumericExpression rightExpression)
        {
            return (NumericExpression)leftExpression < rightExpression;
        }

        public static BooleanExpression operator >(NumericExpression leftExpression, NumericExpression<T> rightExpression)
        {
            return leftExpression > (NumericExpression)rightExpression;
        }

        public static BooleanExpression operator <(NumericExpression leftExpression, NumericExpression<T> rightExpression)
        {
            return leftExpression < (NumericExpression)rightExpression;
        }

        public static BooleanExpression operator >=(NumericExpression<T> leftExpression, NumericExpression rightExpression)
        {
            return (NumericExpression)leftExpression >= rightExpression;
        }

        public static BooleanExpression operator <=(NumericExpression<T> leftExpression, NumericExpression rightExpression)
        {
            return (NumericExpression)leftExpression <= rightExpression;
        }

        public static BooleanExpression operator >=(NumericExpression leftExpression, NumericExpression<T> rightExpression)
        {
            return leftExpression >= (NumericExpression)rightExpression;
        }

        public static BooleanExpression operator <=(NumericExpression leftExpression, NumericExpression<T> rightExpression)
        {
            return leftExpression <= (NumericExpression)rightExpression;
        }
    }

    public class NumericExpression : LiteralExpression
    {
        internal NumericExpression(ISparqlExpression expression)
            : base(expression)
        {
        }

        public static NumericExpression operator *(NumericExpression left, NumericExpression right)
        {
            return new NumericExpression(new MultiplicationExpression(left.Expression, right.Expression));
        }

        public static NumericExpression operator *(NumericExpression left, int right)
        {
            return left * new NumericExpression<int>(right);
        }

        public static NumericExpression operator *(NumericExpression left, decimal right)
        {
            return left * new NumericExpression<decimal>(right);
        }

        public static NumericExpression operator *(NumericExpression left, byte right)
        {
            return left * new NumericExpression<byte>(right);
        }

        public static NumericExpression operator *(NumericExpression left, sbyte right)
        {
            return left * new NumericExpression<sbyte>(right);
        }

        public static NumericExpression operator *(NumericExpression left, short right)
        {
            return left * new NumericExpression<short>(right);
        }

        public static NumericExpression operator *(NumericExpression left, float right)
        {
            return left * new NumericExpression<float>(right);
        }

        public static NumericExpression operator *(NumericExpression left, double right)
        {
            return left * new NumericExpression<double>(right);
        }

        public static NumericExpression operator *(int left, NumericExpression right)
        {
            return new NumericExpression<int>(left) * right;
        }

        public static NumericExpression operator *(decimal left, NumericExpression right)
        {
            return new NumericExpression<decimal>(left) * right;
        }

        public static NumericExpression operator *(long left, NumericExpression right)
        {
            return new NumericExpression<long>(left) * right;
        }

        public static NumericExpression operator *(byte left, NumericExpression right)
        {
            return new NumericExpression<byte>(left) * right;
        }

        public static NumericExpression operator *(sbyte left, NumericExpression right)
        {
            return new NumericExpression<sbyte>(left) * right;
        }

        public static NumericExpression operator *(short left, NumericExpression right)
        {
            return new NumericExpression<short>(left) * right;
        }

        public static NumericExpression operator *(float left, NumericExpression right)
        {
            return new NumericExpression<float>(left) * right;
        }

        public static NumericExpression operator *(double left, NumericExpression right)
        {
            return new NumericExpression<double>(left) * right;
        }

        public static NumericExpression operator /(NumericExpression left, NumericExpression right)
        {
            return new NumericExpression(new DivisionExpression(left.Expression, right.Expression));
        }

        public static NumericExpression operator /(NumericExpression left, int right)
        {
            return left / new NumericExpression<int>(right);
        }

        public static NumericExpression operator /(NumericExpression left, decimal right)
        {
            return left / new NumericExpression<decimal>(right);
        }

        public static NumericExpression operator /(NumericExpression left, byte right)
        {
            return left / new NumericExpression<byte>(right);
        }

        public static NumericExpression operator /(NumericExpression left, sbyte right)
        {
            return left / new NumericExpression<sbyte>(right);
        }

        public static NumericExpression operator /(NumericExpression left, short right)
        {
            return left / new NumericExpression<short>(right);
        }

        public static NumericExpression operator /(NumericExpression left, float right)
        {
            return left / new NumericExpression<float>(right);
        }

        public static NumericExpression operator /(NumericExpression left, double right)
        {
            return left / new NumericExpression<double>(right);
        }

        public static NumericExpression operator /(int left, NumericExpression right)
        {
            return new NumericExpression<int>(left) / right;
        }

        public static NumericExpression operator /(decimal left, NumericExpression right)
        {
            return new NumericExpression<decimal>(left) / right;
        }

        public static NumericExpression operator /(long left, NumericExpression right)
        {
            return new NumericExpression<long>(left) / right;
        }

        public static NumericExpression operator /(byte left, NumericExpression right)
        {
            return new NumericExpression<byte>(left) / right;
        }

        public static NumericExpression operator /(sbyte left, NumericExpression right)
        {
            return new NumericExpression<sbyte>(left) / right;
        }

        public static NumericExpression operator /(short left, NumericExpression right)
        {
            return new NumericExpression<short>(left) / right;
        }

        public static NumericExpression operator /(float left, NumericExpression right)
        {
            return new NumericExpression<float>(left) / right;
        }

        public static NumericExpression operator /(double left, NumericExpression right)
        {
            return new NumericExpression<double>(left) / right;
        }

        public static NumericExpression operator +(NumericExpression left, NumericExpression right)
        {
            return new NumericExpression(new AdditionExpression(left.Expression, right.Expression));
        }

        public static NumericExpression operator +(NumericExpression left, int right)
        {
            return left + new NumericExpression<int>(right);
        }

        public static NumericExpression operator +(NumericExpression left, decimal right)
        {
            return left + new NumericExpression<decimal>(right);
        }

        public static NumericExpression operator +(NumericExpression left, byte right)
        {
            return left + new NumericExpression<byte>(right);
        }

        public static NumericExpression operator +(NumericExpression left, sbyte right)
        {
            return left + new NumericExpression<sbyte>(right);
        }

        public static NumericExpression operator +(NumericExpression left, short right)
        {
            return left + new NumericExpression<short>(right);
        }

        public static NumericExpression operator +(NumericExpression left, float right)
        {
            return left + new NumericExpression<float>(right);
        }

        public static NumericExpression operator +(NumericExpression left, double right)
        {
            return left + new NumericExpression<double>(right);
        }

        public static NumericExpression operator +(int left, NumericExpression right)
        {
            return new NumericExpression<int>(left) + right;
        }

        public static NumericExpression operator +(decimal left, NumericExpression right)
        {
            return new NumericExpression<decimal>(left) + right;
        }

        public static NumericExpression operator +(long left, NumericExpression right)
        {
            return new NumericExpression<long>(left) + right;
        }

        public static NumericExpression operator +(byte left, NumericExpression right)
        {
            return new NumericExpression<byte>(left) + right;
        }

        public static NumericExpression operator +(sbyte left, NumericExpression right)
        {
            return new NumericExpression<sbyte>(left) + right;
        }

        public static NumericExpression operator +(short left, NumericExpression right)
        {
            return new NumericExpression<short>(left) + right;
        }

        public static NumericExpression operator +(float left, NumericExpression right)
        {
            return new NumericExpression<float>(left) + right;
        }

        public static NumericExpression operator +(double left, NumericExpression right)
        {
            return new NumericExpression<double>(left) + right;
        }

        public static NumericExpression operator -(NumericExpression left, NumericExpression right)
        {
            return new NumericExpression(new SubtractionExpression(left.Expression, right.Expression));
        }

        public static NumericExpression operator -(NumericExpression left, int right)
        {
            return left - new NumericExpression<int>(right);
        }

        public static NumericExpression operator -(NumericExpression left, decimal right)
        {
            return left - new NumericExpression<decimal>(right);
        }

        public static NumericExpression operator -(NumericExpression left, byte right)
        {
            return left - new NumericExpression<byte>(right);
        }

        public static NumericExpression operator -(NumericExpression left, sbyte right)
        {
            return left - new NumericExpression<sbyte>(right);
        }

        public static NumericExpression operator -(NumericExpression left, short right)
        {
            return left - new NumericExpression<short>(right);
        }

        public static NumericExpression operator -(NumericExpression left, float right)
        {
            return left - new NumericExpression<float>(right);
        }

        public static NumericExpression operator -(NumericExpression left, double right)
        {
            return left - new NumericExpression<double>(right);
        }

        public static NumericExpression operator -(int left, NumericExpression right)
        {
            return new NumericExpression<int>(left) - right;
        }

        public static NumericExpression operator -(decimal left, NumericExpression right)
        {
            return new NumericExpression<decimal>(left) - right;
        }

        public static NumericExpression operator -(long left, NumericExpression right)
        {
            return new NumericExpression<long>(left) - right;
        }

        public static NumericExpression operator -(byte left, NumericExpression right)
        {
            return new NumericExpression<byte>(left) - right;
        }

        public static NumericExpression operator -(sbyte left, NumericExpression right)
        {
            return new NumericExpression<sbyte>(left) - right;
        }

        public static NumericExpression operator -(short left, NumericExpression right)
        {
            return new NumericExpression<short>(left) - right;
        }

        public static NumericExpression operator -(float left, NumericExpression right)
        {
            return new NumericExpression<float>(left) - right;
        }

        public static NumericExpression operator -(double left, NumericExpression right)
        {
            return new NumericExpression<double>(left) - right;
        }

        public static BooleanExpression operator >(NumericExpression leftExpression, NumericExpression rightExpression)
        {
            return Gt(leftExpression.Expression, rightExpression);
        }

        public static BooleanExpression operator <(NumericExpression leftExpression, NumericExpression rightExpression)
        {
            return Lt(leftExpression.Expression, rightExpression);
        }

        public static BooleanExpression operator >=(NumericExpression leftExpression, NumericExpression rightExpression)
        {
            return Ge(leftExpression.Expression, rightExpression);
        }

        public static BooleanExpression operator <=(NumericExpression leftExpression, NumericExpression rightExpression)
        {
            return Le(leftExpression.Expression, rightExpression);
        }
    }
}