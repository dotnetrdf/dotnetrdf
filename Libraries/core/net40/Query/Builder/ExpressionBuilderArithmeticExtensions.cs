using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Arithmetic;

namespace VDS.RDF.Query.Builder
{
    public static class ExpressionBuilderArithmeticExtensions
    {
        public static NumericExpression<T> Multiply<T>(this T left, NumericExpression<T> right)
        {
            return new NumericExpression<T>(left).Multiply(right);
        }

        public static NumericExpression Multiply(this int left, NumericExpression right)
        {
            return new NumericExpression<int>(left).Multiply(right);
        }

        public static NumericExpression Multiply(this decimal left, NumericExpression right)
        {
            return new NumericExpression<decimal>(left).Multiply(right);
        }

        public static NumericExpression Multiply(this long left, NumericExpression right)
        {
            return new NumericExpression<long>(left).Multiply(right);
        }

        public static NumericExpression Multiply(this byte left, NumericExpression right)
        {
            return new NumericExpression<byte>(left).Multiply(right);
        }

        public static NumericExpression Multiply(this sbyte left, NumericExpression right)
        {
            return new NumericExpression<sbyte>(left).Multiply(right);
        }

        public static NumericExpression Multiply(this short left, NumericExpression right)
        {
            return new NumericExpression<short>(left).Multiply(right);
        }

        public static NumericExpression Multiply(this float left, NumericExpression right)
        {
            return new NumericExpression<float>(left).Multiply(right);
        }

        public static NumericExpression Multiply(this double left, NumericExpression right)
        {
            return new NumericExpression<double>(left).Multiply(right);
        }

        public static NumericExpression<T> Divide<T>(this T left, NumericExpression<T> right)
        {
            return new NumericExpression<T>(left).Divide(right);
        }

        public static NumericExpression Divide(this int left, NumericExpression right)
        {
            return new NumericExpression<int>(left).Divide(right);
        }

        public static NumericExpression Divide(this decimal left, NumericExpression right)
        {
            return new NumericExpression<decimal>(left).Divide(right);
        }

        public static NumericExpression Divide(this long left, NumericExpression right)
        {
            return new NumericExpression<long>(left).Divide(right);
        }

        public static NumericExpression Divide(this byte left, NumericExpression right)
        {
            return new NumericExpression<byte>(left).Divide(right);
        }

        public static NumericExpression Divide(this sbyte left, NumericExpression right)
        {
            return new NumericExpression<sbyte>(left).Divide(right);
        }

        public static NumericExpression Divide(this short left, NumericExpression right)
        {
            return new NumericExpression<short>(left).Divide(right);
        }

        public static NumericExpression Divide(this float left, NumericExpression right)
        {
            return new NumericExpression<float>(left).Divide(right);
        }

        public static NumericExpression Divide(this double left, NumericExpression right)
        {
            return new NumericExpression<double>(left).Divide(right);
        }

        public static NumericExpression<T> Add<T>(this T left, NumericExpression<T> right)
        {
            return new NumericExpression<T>(left).Add(right);
        }

        public static NumericExpression Add(this int left, NumericExpression right)
        {
            return new NumericExpression<int>(left).Add(right);
        }

        public static NumericExpression Add(this decimal left, NumericExpression right)
        {
            return new NumericExpression<decimal>(left).Add(right);
        }

        public static NumericExpression Add(this long left, NumericExpression right)
        {
            return new NumericExpression<long>(left).Add(right);
        }

        public static NumericExpression Add(this byte left, NumericExpression right)
        {
            return new NumericExpression<byte>(left).Add(right);
        }

        public static NumericExpression Add(this sbyte left, NumericExpression right)
        {
            return new NumericExpression<sbyte>(left).Add(right);
        }

        public static NumericExpression Add(this short left, NumericExpression right)
        {
            return new NumericExpression<short>(left).Add(right);
        }

        public static NumericExpression Add(this float left, NumericExpression right)
        {
            return new NumericExpression<float>(left).Add(right);
        }

        public static NumericExpression Add(this double left, NumericExpression right)
        {
            return new NumericExpression<double>(left).Add(right);
        }

        public static NumericExpression<T> Subtract<T>(this T left, NumericExpression<T> right)
        {
            return new NumericExpression<T>(left).Subtract(right);
        }

        public static NumericExpression Subtract(this int left, NumericExpression right)
        {
            return new NumericExpression<int>(left).Subtract(right);
        }

        public static NumericExpression Subtract(this decimal left, NumericExpression right)
        {
            return new NumericExpression<decimal>(left).Subtract(right);
        }

        public static NumericExpression Subtract(this long left, NumericExpression right)
        {
            return new NumericExpression<long>(left).Subtract(right);
        }

        public static NumericExpression Subtract(this byte left, NumericExpression right)
        {
            return new NumericExpression<byte>(left).Subtract(right);
        }

        public static NumericExpression Subtract(this sbyte left, NumericExpression right)
        {
            return new NumericExpression<sbyte>(left).Subtract(right);
        }

        public static NumericExpression Subtract(this short left, NumericExpression right)
        {
            return new NumericExpression<short>(left).Subtract(right);
        }

        public static NumericExpression Subtract(this float left, NumericExpression right)
        {
            return new NumericExpression<float>(left).Subtract(right);
        }

        public static NumericExpression Subtract(this double left, NumericExpression right)
        {
            return new NumericExpression<double>(left).Subtract(right);
        }
    }
}