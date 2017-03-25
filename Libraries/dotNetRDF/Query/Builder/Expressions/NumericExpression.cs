/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Arithmetic;

namespace VDS.RDF.Query.Builder.Expressions
{
    /// <summary>
    /// Represents a numeric expression of known type
    /// </summary>
    public sealed class NumericExpression<T> : TypedLiteralExpression<T>
    {
        internal NumericExpression(T numericValue)
            : base(numericValue)
        {
        }

        /// <summary>
        /// Wraps the <paramref name="expression"/> as a typed numeric expression
        /// </summary>
        public NumericExpression(ISparqlExpression expression)
            : base(expression)
        {
        }

#pragma warning disable 1591
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
            var multiplication = NumericExpression.Multiply(left, right);
            return new NumericExpression<T>(multiplication.Expression);
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
            var division = NumericExpression.Divide(left, right);
            return new NumericExpression<T>(division.Expression);
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
            var addition = NumericExpression.Add(left, right);
            return new NumericExpression<T>(addition.Expression);
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
            var subtraction = NumericExpression.Subtract(left, right);
            return new NumericExpression<T>(subtraction.Expression);
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
#pragma warning restore 1591
    }

    /// <summary>
    /// Represents a numeric expression of undefined type
    /// </summary>
    public class NumericExpression : LiteralExpression
    {
        /// <summary>
        /// Wraps the <paramref name="expression"/> as a numeric expression
        /// </summary>
        public NumericExpression(ISparqlExpression expression)
            : base(expression)
        {
        }

#pragma warning disable 1591
        internal static NumericExpression Multiply(SparqlExpression left, SparqlExpression right)
        {
            return new NumericExpression(new MultiplicationExpression(left.Expression, right.Expression));
        }

        public static NumericExpression operator *(NumericExpression left, NumericExpression right)
        {
            return Multiply(left, right);
        }

        public static NumericExpression operator *(VariableExpression left, NumericExpression right)
        {
            return Multiply(left, right);
        }

        public static NumericExpression operator *(NumericExpression left, VariableExpression right)
        {
            return Multiply(left, right);
        }

        public static NumericExpression operator *(NumericExpression left, int right)
        {
            return left * new NumericExpression<int>(right);
        }

        public static NumericExpression operator *(NumericExpression left, long right)
        {
            return left * new NumericExpression<long>(right);
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

        internal static NumericExpression Divide(SparqlExpression left, SparqlExpression right)
        {
            return new NumericExpression(new DivisionExpression(left.Expression, right.Expression));
        }

        public static NumericExpression operator /(NumericExpression left, NumericExpression right)
        {
            return Divide(left, right);
        }

        public static NumericExpression operator /(NumericExpression left, VariableExpression right)
        {
            return Divide(left, right);
        }

        public static NumericExpression operator /(VariableExpression left, NumericExpression right)
        {
            return Divide(left, right);
        }

        public static NumericExpression operator /(NumericExpression left, int right)
        {
            return left / new NumericExpression<int>(right);
        }

        public static NumericExpression operator /(NumericExpression left, long right)
        {
            return left / new NumericExpression<long>(right);
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

        internal static NumericExpression Add(SparqlExpression left, SparqlExpression right)
        {
            return new NumericExpression(new AdditionExpression(left.Expression, right.Expression));
        }

        public static NumericExpression operator +(NumericExpression left, NumericExpression right)
        {
            return Add(left, right);
        }

        public static NumericExpression operator +(VariableExpression left, NumericExpression right)
        {
            return Add(left, right);
        }

        public static NumericExpression operator +(NumericExpression left, VariableExpression right)
        {
            return Add(left, right);
        }

        public static NumericExpression operator +(NumericExpression left, int right)
        {
            return left + new NumericExpression<int>(right);
        }

        public static NumericExpression operator +(NumericExpression left, long right)
        {
            return left + new NumericExpression<long>(right);
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

        internal static NumericExpression Subtract(SparqlExpression left, SparqlExpression right)
        {
            return new NumericExpression(new SubtractionExpression(left.Expression, right.Expression));
        }

        public static NumericExpression operator -(NumericExpression left, NumericExpression right)
        {
            return Subtract(left, right);
        }

        public static NumericExpression operator -(VariableExpression left, NumericExpression right)
        {
            return Subtract(left, right);
        }

        public static NumericExpression operator -(NumericExpression left, VariableExpression right)
        {
            return Subtract(left, right);
        }

        public static NumericExpression operator -(NumericExpression left, int right)
        {
            return left - new NumericExpression<int>(right);
        }

        public static NumericExpression operator -(NumericExpression left, long right)
        {
            return left - new NumericExpression<long>(right);
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
            return Gt(leftExpression.Expression, rightExpression.Expression);
        }

        public static BooleanExpression operator <(NumericExpression leftExpression, NumericExpression rightExpression)
        {
            return Lt(leftExpression.Expression, rightExpression.Expression);
        }

        public static BooleanExpression operator >=(NumericExpression leftExpression, NumericExpression rightExpression)
        {
            return Ge(leftExpression.Expression, rightExpression.Expression);
        }

        public static BooleanExpression operator <=(NumericExpression leftExpression, NumericExpression rightExpression)
        {
            return Le(leftExpression.Expression, rightExpression.Expression);
        }
#pragma warning restore 1591
    }
}