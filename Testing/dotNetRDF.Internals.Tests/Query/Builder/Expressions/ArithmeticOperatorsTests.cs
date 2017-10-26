/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Linq;
using Xunit;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Arithmetic;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{

    public class ArithmeticOperatorsTests
    {
        [Fact]
        public void CanMultiplyTypedNumericsOfMatchingTypes()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression<int> left = new NumericExpression<int>(10);

            // when
            var multiplication = (left * right).Expression;

            // then
            Assert.True(multiplication is MultiplicationExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanMultiplyTypedNumericsOfdifferentTypes()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression<decimal> left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left * right).Expression;

            // then
            Assert.True(multiplication is MultiplicationExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanMultiplyTypedNumericAndUntypedNumeric()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (right * left).Expression;

            // then
            Assert.True(multiplication is MultiplicationExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanMultiplyTypedNumericAndUntypedNumeric2()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left * right).Expression;

            // then
            Assert.True(multiplication is MultiplicationExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanMultiplyTypedNumericBySimpleValue()
        {
            // given
            var left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left * 10m).Expression;

            // then
            Assert.True(multiplication is MultiplicationExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanMultiplySimpleValueByTypedNumeric()
        {
            // given
            var right = new NumericExpression<decimal>(10);

            // when
            var multiplication = (10m * right).Expression;

            // then
            Assert.True(multiplication is MultiplicationExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanMultiplyNumericBySimpleValue()
        {
            // given
            var left = new NumericExpression(new VariableTerm("x"));

            // when
            var multiplication = (left * 10).Expression;

            // then
            Assert.True(multiplication is MultiplicationExpression);
            Assert.Same(left.Expression, multiplication.Arguments.ElementAt(0));
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanMultiplySimpleValueByNumeric()
        {
            // given
            var right = new NumericExpression(new VariableTerm("x"));

            // when
            var multiplication = (10 * right).Expression;

            // then
            Assert.True(multiplication is MultiplicationExpression);
            Assert.Same(right.Expression, multiplication.Arguments.ElementAt(1));
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
        }

        [Fact]
        public void CanChainMultiplicationOfNumerics()
        {
            // given
            NumericExpression<int> op2 = new NumericExpression<int>(10);
            NumericExpression<decimal> op1 = new NumericExpression<decimal>(10);
            NumericExpression<int> op3 = new NumericExpression<int>(5);

            // when
            var multiplication = (op1 * op2 * op3).Expression;

            // then
            Assert.True(multiplication is MultiplicationExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is MultiplicationExpression);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanDivideTypedNumericsOfMatchingTypes()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression<int> left = new NumericExpression<int>(10);

            // when
            var multiplication = (left / right).Expression;

            // then
            Assert.True(multiplication is DivisionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanDivideTypedNumericsOfdifferentTypes()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression<decimal> left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left / right).Expression;

            // then
            Assert.True(multiplication is DivisionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanDivideTypedNumericByUntypedNumeric()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (right / left).Expression;

            // then
            Assert.True(multiplication is DivisionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanDivideTypedNumericByUntypedNumeric2()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left / right).Expression;

            // then
            Assert.True(multiplication is DivisionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanDivideTypedNumericBySimpleValue()
        {
            // given
            var left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left / 10m).Expression;

            // then
            Assert.True(multiplication is DivisionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanDivideSimpleValueByTypedNumeric()
        {
            // given
            var right = new NumericExpression<decimal>(10);

            // when
            var multiplication = (10m / right).Expression;

            // then
            Assert.True(multiplication is DivisionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanChainDivisionsOfNumerics()
        {
            // given
            NumericExpression<int> op2 = new NumericExpression<int>(10);
            NumericExpression<decimal> op1 = new NumericExpression<decimal>(10);
            NumericExpression<int> op3 = new NumericExpression<int>(5);

            // when
            var multiplication = (op1 / op2 / op3).Expression;

            // then
            Assert.True(multiplication is DivisionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is DivisionExpression);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanAddTypedNumericsOfMatchingTypes()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression<int> left = new NumericExpression<int>(10);

            // when
            var multiplication = (left + right).Expression;

            // then
            Assert.True(multiplication is AdditionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanAddTypedNumericsOfdifferentTypes()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression<decimal> left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left + right).Expression;

            // then
            Assert.True(multiplication is AdditionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanAddTypedNumericToUntypedNumeric()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (right + left).Expression;

            // then
            Assert.True(multiplication is AdditionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanAddTypedNumericToUntypedNumeric2()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left + right).Expression;

            // then
            Assert.True(multiplication is AdditionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanAddSimpleValueToTypedNumeric()
        {
            // given
            var left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left + 10m).Expression;

            // then
            Assert.True(multiplication is AdditionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanAddTypedNumericToSimpleValue()
        {
            // given
            var right = new NumericExpression<decimal>(10);

            // when
            var multiplication = (10m + right).Expression;

            // then
            Assert.True(multiplication is AdditionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanChainAdditionsOfNumerics()
        {
            // given
            NumericExpression<int> op2 = new NumericExpression<int>(10);
            NumericExpression<decimal> op1 = new NumericExpression<decimal>(10);
            NumericExpression<int> op3 = new NumericExpression<int>(5);

            // when
            var multiplication = (op1 + op2 + op3).Expression;

            // then
            Assert.True(multiplication is AdditionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is AdditionExpression);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanSubtractTypedNumericsOfMatchingTypes()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression<int> left = new NumericExpression<int>(10);

            // when
            var multiplication = (left - right).Expression;

            // then
            Assert.True(multiplication is SubtractionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanSubtractTypedNumericsOfdifferentTypes()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression<decimal> left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left - right).Expression;

            // then
            Assert.True(multiplication is SubtractionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanSubtractTypedNumericToUntypedNumeric()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (right - left).Expression;

            // then
            Assert.True(multiplication is SubtractionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanSubtractTypedNumericToUntypedNumeric2()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left - right).Expression;

            // then
            Assert.True(multiplication is SubtractionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanSubtractSimpleValueFromTypedNumeric()
        {
            // given
            var left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left - 10m).Expression;

            // then
            Assert.True(multiplication is SubtractionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanSubtractTypedNumericFromSimpleValue()
        {
            // given
            var right = new NumericExpression<decimal>(10);

            // when
            var multiplication = (10m - right).Expression;

            // then
            Assert.True(multiplication is SubtractionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanChainSubtractionsOfNumerics()
        {
            // given
            NumericExpression<int> op2 = new NumericExpression<int>(10);
            NumericExpression<decimal> op1 = new NumericExpression<decimal>(10);
            NumericExpression<int> op3 = new NumericExpression<int>(5);

            // when
            var multiplication = (op1 - op2 - op3).Expression;

            // then
            Assert.True(multiplication is SubtractionExpression);
            Assert.True(multiplication.Arguments.ElementAt(0) is SubtractionExpression);
            Assert.True(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }
    }
}