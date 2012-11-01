using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Arithmetic;

namespace VDS.RDF.Query.Builder
{
    public static class ExpressionBuilderArithmeticExtensions
    {
        public static NumericExpression Multiply(this NumericExpression left, NumericExpression right)
        {
            var multiplication = new MultiplicationExpression(left.Expression, right.Expression);
            return new NumericExpression(multiplication);
        }

        public static NumericExpression<T> Multiply<T>(this NumericExpression<T> left, NumericExpression<T> right)
        {
            var multiplication = new MultiplicationExpression(left.Expression, right.Expression);
            return new NumericExpression<T>(multiplication);
        }

        public static NumericExpression Multiply<T>(this NumericExpression<T> left, NumericExpression right)
        {
            var multiplication = new MultiplicationExpression(left.Expression, right.Expression);
            return new NumericExpression(multiplication);
        }

        public static NumericExpression<T> Multiply<T>(this T left, NumericExpression<T> right)
        {
            return new NumericExpression<T>(left).Multiply(right);
        }

        public static NumericExpression Divide(this NumericExpression left, NumericExpression right)
        {
            var multiplication = new DivisionExpression(left.Expression, right.Expression);
            return new NumericExpression(multiplication);
        }

        public static NumericExpression<T> Divide<T>(this NumericExpression<T> left, NumericExpression<T> right)
        {
            var multiplication = new DivisionExpression(left.Expression, right.Expression);
            return new NumericExpression<T>(multiplication);
        }

        public static NumericExpression Divide<T>(this NumericExpression<T> left, NumericExpression right)
        {
            var multiplication = new DivisionExpression(left.Expression, right.Expression);
            return new NumericExpression(multiplication);
        }

        public static NumericExpression<T> Divide<T>(this T left, NumericExpression<T> right)
        {
            return new NumericExpression<T>(left).Divide(right);
        }

        public static NumericExpression Add(this NumericExpression left, NumericExpression right)
        {
            var multiplication = new AdditionExpression(left.Expression, right.Expression);
            return new NumericExpression(multiplication);
        }

        public static NumericExpression<T> Add<T>(this NumericExpression<T> left, NumericExpression<T> right)
        {
            var multiplication = new AdditionExpression(left.Expression, right.Expression);
            return new NumericExpression<T>(multiplication);
        }

        public static NumericExpression Add<T>(this NumericExpression<T> left, NumericExpression right)
        {
            var multiplication = new AdditionExpression(left.Expression, right.Expression);
            return new NumericExpression(multiplication);
        }

        public static NumericExpression<T> Add<T>(this T left, NumericExpression<T> right)
        {
            return new NumericExpression<T>(left).Add(right);
        }

        public static NumericExpression Subtract(this NumericExpression left, NumericExpression right)
        {
            var multiplication = new SubtractionExpression(left.Expression, right.Expression);
            return new NumericExpression(multiplication);
        }

        public static NumericExpression<T> Subtract<T>(this NumericExpression<T> left, NumericExpression<T> right)
        {
            var multiplication = new SubtractionExpression(left.Expression, right.Expression);
            return new NumericExpression<T>(multiplication);
        }

        public static NumericExpression Subtract<T>(this NumericExpression<T> left, NumericExpression right)
        {
            var multiplication = new SubtractionExpression(left.Expression, right.Expression);
            return new NumericExpression(multiplication);
        }

        public static NumericExpression<T> Subtract<T>(this T left, NumericExpression<T> right)
        {
            return new NumericExpression<T>(left).Subtract(right);
        }

    }
}