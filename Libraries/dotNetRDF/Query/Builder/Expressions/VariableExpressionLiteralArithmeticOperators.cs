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

namespace VDS.RDF.Query.Builder.Expressions
{
    public partial class VariableExpression
    {
#pragma warning disable 1591
        public static NumericExpression operator -(VariableExpression left, VariableExpression right)
        {
            return new NumericExpression(left.Expression) - new NumericExpression(right.Expression);
        }

        public static NumericExpression operator +(VariableExpression left, VariableExpression right)
        {
            return new NumericExpression(left.Expression) + new NumericExpression(right.Expression);
        }

        public static NumericExpression operator /(VariableExpression left, VariableExpression right)
        {
            return new NumericExpression(left.Expression) / new NumericExpression(right.Expression);
        }

        public static NumericExpression operator *(VariableExpression left, VariableExpression right)
        {
            return new NumericExpression(left.Expression) * new NumericExpression(right.Expression);
        }

        public static NumericExpression operator -(int left, VariableExpression right)
        {
            return new NumericExpression<int>(left) - new NumericExpression(right.Expression);
        }

        public static NumericExpression operator -(VariableExpression left, int right)
        {
            return new NumericExpression(left.Expression) - new NumericExpression<int>(right);
        }

        public static NumericExpression operator *(int left, VariableExpression right)
        {
            return new NumericExpression<int>(left) * new NumericExpression(right.Expression);
        }

        public static NumericExpression operator *(VariableExpression left, int right)
        {
            return new NumericExpression(left.Expression) * new NumericExpression<int>(right);
        }

        public static NumericExpression operator /(int left, VariableExpression right)
        {
            return new NumericExpression<int>(left) / new NumericExpression(right.Expression);
        }

        public static NumericExpression operator /(VariableExpression left, int right)
        {
            return new NumericExpression(left.Expression) / new NumericExpression<int>(right);
        }

        public static NumericExpression operator +(int left, VariableExpression right)
        {
            return new NumericExpression<int>(left) + new NumericExpression(right.Expression);
        }

        public static NumericExpression operator +(VariableExpression left, int right)
        {
            return new NumericExpression(left.Expression) + new NumericExpression<int>(right);
        }

        public static NumericExpression operator -(short left, VariableExpression right)
        {
            return new NumericExpression<short>(left) - new NumericExpression(right.Expression);
        }

        public static NumericExpression operator -(VariableExpression left, short right)
        {
            return new NumericExpression(left.Expression) - new NumericExpression<short>(right);
        }

        public static NumericExpression operator *(short left, VariableExpression right)
        {
            return new NumericExpression<short>(left) * new NumericExpression(right.Expression);
        }

        public static NumericExpression operator *(VariableExpression left, short right)
        {
            return new NumericExpression(left.Expression) * new NumericExpression<short>(right);
        }

        public static NumericExpression operator /(short left, VariableExpression right)
        {
            return new NumericExpression<short>(left) / new NumericExpression(right.Expression);
        }

        public static NumericExpression operator /(VariableExpression left, short right)
        {
            return new NumericExpression(left.Expression) / new NumericExpression<short>(right);
        }

        public static NumericExpression operator +(short left, VariableExpression right)
        {
            return new NumericExpression<short>(left) + new NumericExpression(right.Expression);
        }

        public static NumericExpression operator +(VariableExpression left, short right)
        {
            return new NumericExpression(left.Expression) + new NumericExpression<short>(right);
        }

        public static NumericExpression operator -(long left, VariableExpression right)
        {
            return new NumericExpression<long>(left) - new NumericExpression(right.Expression);
        }

        public static NumericExpression operator -(VariableExpression left, long right)
        {
            return new NumericExpression(left.Expression) - new NumericExpression<long>(right);
        }

        public static NumericExpression operator *(long left, VariableExpression right)
        {
            return new NumericExpression<long>(left) * new NumericExpression(right.Expression);
        }

        public static NumericExpression operator *(VariableExpression left, long right)
        {
            return new NumericExpression(left.Expression) * new NumericExpression<long>(right);
        }

        public static NumericExpression operator /(long left, VariableExpression right)
        {
            return new NumericExpression<long>(left) / new NumericExpression(right.Expression);
        }

        public static NumericExpression operator /(VariableExpression left, long right)
        {
            return new NumericExpression(left.Expression) / new NumericExpression<long>(right);
        }

        public static NumericExpression operator +(long left, VariableExpression right)
        {
            return new NumericExpression<long>(left) + new NumericExpression(right.Expression);
        }

        public static NumericExpression operator +(VariableExpression left, long right)
        {
            return new NumericExpression(left.Expression) + new NumericExpression<long>(right);
        }

        public static NumericExpression operator -(byte left, VariableExpression right)
        {
            return new NumericExpression<byte>(left) - new NumericExpression(right.Expression);
        }

        public static NumericExpression operator -(VariableExpression left, byte right)
        {
            return new NumericExpression(left.Expression) - new NumericExpression<byte>(right);
        }

        public static NumericExpression operator *(byte left, VariableExpression right)
        {
            return new NumericExpression<byte>(left) * new NumericExpression(right.Expression);
        }

        public static NumericExpression operator *(VariableExpression left, byte right)
        {
            return new NumericExpression(left.Expression) * new NumericExpression<byte>(right);
        }

        public static NumericExpression operator /(byte left, VariableExpression right)
        {
            return new NumericExpression<byte>(left) / new NumericExpression(right.Expression);
        }

        public static NumericExpression operator /(VariableExpression left, byte right)
        {
            return new NumericExpression(left.Expression) / new NumericExpression<byte>(right);
        }

        public static NumericExpression operator +(byte left, VariableExpression right)
        {
            return new NumericExpression<byte>(left) + new NumericExpression(right.Expression);
        }

        public static NumericExpression operator +(VariableExpression left, byte right)
        {
            return new NumericExpression(left.Expression) + new NumericExpression<byte>(right);
        }

        public static NumericExpression operator -(sbyte left, VariableExpression right)
        {
            return new NumericExpression<sbyte>(left) - new NumericExpression(right.Expression);
        }

        public static NumericExpression operator -(VariableExpression left, sbyte right)
        {
            return new NumericExpression(left.Expression) - new NumericExpression<sbyte>(right);
        }

        public static NumericExpression operator *(sbyte left, VariableExpression right)
        {
            return new NumericExpression<sbyte>(left) * new NumericExpression(right.Expression);
        }

        public static NumericExpression operator *(VariableExpression left, sbyte right)
        {
            return new NumericExpression(left.Expression) * new NumericExpression<sbyte>(right);
        }

        public static NumericExpression operator /(sbyte left, VariableExpression right)
        {
            return new NumericExpression<sbyte>(left) / new NumericExpression(right.Expression);
        }

        public static NumericExpression operator /(VariableExpression left, sbyte right)
        {
            return new NumericExpression(left.Expression) / new NumericExpression<sbyte>(right);
        }

        public static NumericExpression operator +(sbyte left, VariableExpression right)
        {
            return new NumericExpression<sbyte>(left) + new NumericExpression(right.Expression);
        }

        public static NumericExpression operator +(VariableExpression left, sbyte right)
        {
            return new NumericExpression(left.Expression) + new NumericExpression<sbyte>(right);
        }

        public static NumericExpression operator -(float left, VariableExpression right)
        {
            return new NumericExpression<float>(left) - new NumericExpression(right.Expression);
        }

        public static NumericExpression operator -(VariableExpression left, float right)
        {
            return new NumericExpression(left.Expression) - new NumericExpression<float>(right);
        }

        public static NumericExpression operator *(float left, VariableExpression right)
        {
            return new NumericExpression<float>(left) * new NumericExpression(right.Expression);
        }

        public static NumericExpression operator *(VariableExpression left, float right)
        {
            return new NumericExpression(left.Expression) * new NumericExpression<float>(right);
        }

        public static NumericExpression operator /(float left, VariableExpression right)
        {
            return new NumericExpression<float>(left) / new NumericExpression(right.Expression);
        }

        public static NumericExpression operator /(VariableExpression left, float right)
        {
            return new NumericExpression(left.Expression) / new NumericExpression<float>(right);
        }

        public static NumericExpression operator +(float left, VariableExpression right)
        {
            return new NumericExpression<float>(left) + new NumericExpression(right.Expression);
        }

        public static NumericExpression operator +(VariableExpression left, float right)
        {
            return new NumericExpression(left.Expression) + new NumericExpression<float>(right);
        }

        public static NumericExpression operator -(double left, VariableExpression right)
        {
            return new NumericExpression<double>(left) - new NumericExpression(right.Expression);
        }

        public static NumericExpression operator -(VariableExpression left, double right)
        {
            return new NumericExpression(left.Expression) - new NumericExpression<double>(right);
        }

        public static NumericExpression operator *(double left, VariableExpression right)
        {
            return new NumericExpression<double>(left) * new NumericExpression(right.Expression);
        }

        public static NumericExpression operator *(VariableExpression left, double right)
        {
            return new NumericExpression(left.Expression) * new NumericExpression<double>(right);
        }

        public static NumericExpression operator /(double left, VariableExpression right)
        {
            return new NumericExpression<double>(left) / new NumericExpression(right.Expression);
        }

        public static NumericExpression operator /(VariableExpression left, double right)
        {
            return new NumericExpression(left.Expression) / new NumericExpression<double>(right);
        }

        public static NumericExpression operator +(double left, VariableExpression right)
        {
            return new NumericExpression<double>(left) + new NumericExpression(right.Expression);
        }

        public static NumericExpression operator +(VariableExpression left, double right)
        {
            return new NumericExpression(left.Expression) + new NumericExpression<double>(right);
        }

        public static NumericExpression operator -(decimal left, VariableExpression right)
        {
            return new NumericExpression<decimal>(left) - new NumericExpression(right.Expression);
        }

        public static NumericExpression operator -(VariableExpression left, decimal right)
        {
            return new NumericExpression(left.Expression) - new NumericExpression<decimal>(right);
        }

        public static NumericExpression operator *(decimal left, VariableExpression right)
        {
            return new NumericExpression<decimal>(left) * new NumericExpression(right.Expression);
        }

        public static NumericExpression operator *(VariableExpression left, decimal right)
        {
            return new NumericExpression(left.Expression) * new NumericExpression<decimal>(right);
        }

        public static NumericExpression operator /(decimal left, VariableExpression right)
        {
            return new NumericExpression<decimal>(left) / new NumericExpression(right.Expression);
        }

        public static NumericExpression operator /(VariableExpression left, decimal right)
        {
            return new NumericExpression(left.Expression) / new NumericExpression<decimal>(right);
        }

        public static NumericExpression operator +(decimal left, VariableExpression right)
        {
            return new NumericExpression<decimal>(left) + new NumericExpression(right.Expression);
        }

        public static NumericExpression operator +(VariableExpression left, decimal right)
        {
            return new NumericExpression(left.Expression) + new NumericExpression<decimal>(right);
        }
#pragma warning restore 1591
    }
}