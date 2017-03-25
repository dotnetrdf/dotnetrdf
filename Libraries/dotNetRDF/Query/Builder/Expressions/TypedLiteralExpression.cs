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
using VDS.RDF.Query.Expressions.Comparison;

namespace VDS.RDF.Query.Builder.Expressions
{
    /// <summary>
    /// Represents a typed literal
    /// </summary>
#pragma warning disable 660,661
    public class TypedLiteralExpression<T> : LiteralExpression
#pragma warning restore 660,661
    {
        internal TypedLiteralExpression(T literalValue)
            : base(literalValue.ToConstantTerm())
        {
        }

        /// <summary>
        /// Wraps the <paramref name="expression"/> as a typed literal expression
        /// </summary>
        public TypedLiteralExpression(ISparqlExpression expression)
            : base(expression)
        {
        }

#pragma warning disable 1591
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
            return new BooleanExpression(new EqualsExpression(left.Expression, right.ToConstantTerm()));
        }

        public static BooleanExpression operator !=(TypedLiteralExpression<T> left, T right)
        {
            return new BooleanExpression(new NotEqualsExpression(left.Expression, right.ToConstantTerm()));
        }

        public static BooleanExpression operator ==(T left, TypedLiteralExpression<T> right)
        {
            return new BooleanExpression(new EqualsExpression(left.ToConstantTerm(), right.Expression));
        }

        public static BooleanExpression operator !=(T left, TypedLiteralExpression<T> right)
        {
            return new BooleanExpression(new NotEqualsExpression(left.ToConstantTerm(), right.Expression));
        }

        public static BooleanExpression operator >(TypedLiteralExpression<T> left, T right)
        {
            return Gt(left.Expression, right.ToConstantTerm());
        }

        public static BooleanExpression operator <(TypedLiteralExpression<T> left, T right)
        {
            return Lt(left.Expression, right.ToConstantTerm());
        }

        public static BooleanExpression operator >=(TypedLiteralExpression<T> left, T right)
        {
            return Ge(left.Expression, right.ToConstantTerm());
        }

        public static BooleanExpression operator <=(TypedLiteralExpression<T> left, T right)
        {
            return Le(left.Expression, right.ToConstantTerm());
        }

        public static BooleanExpression operator >(T left, TypedLiteralExpression<T> right)
        {
            return Gt(left.ToConstantTerm(), right.Expression);
        }

        public static BooleanExpression operator <(T left, TypedLiteralExpression<T> right)
        {
            return Lt(left.ToConstantTerm(), right.Expression);
        }

        public static BooleanExpression operator >=(T left, TypedLiteralExpression<T> right)
        {
            return Ge(left.ToConstantTerm(), right.Expression);
        }

        public static BooleanExpression operator <=(T left, TypedLiteralExpression<T> right)
        {
            return Le(left.ToConstantTerm(), right.Expression);
        }
#pragma warning restore 1591
    }
}