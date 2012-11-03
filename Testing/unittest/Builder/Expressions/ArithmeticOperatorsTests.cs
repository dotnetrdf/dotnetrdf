using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Arithmetic;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Test.Builder.Expressions
{
    [TestClass]
    public class ArithmeticOperatorsTests
    {
        [TestMethod]
        public void CanMultiplyTypedNumericsOfMatchingTypes()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression<int> left = new NumericExpression<int>(10);

            // when
            var multiplication = (left * right).Expression;

            // then
            Assert.IsTrue(multiplication is MultiplicationExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanMultiplyTypedNumericsOfdifferentTypes()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression<decimal> left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left * right).Expression;

            // then
            Assert.IsTrue(multiplication is MultiplicationExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanMultiplyTypedNumericAndUntypedNumeric()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (right * left).Expression;

            // then
            Assert.IsTrue(multiplication is MultiplicationExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanMultiplyTypedNumericAndUntypedNumeric2()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left * right).Expression;

            // then
            Assert.IsTrue(multiplication is MultiplicationExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanMultiplyTypedNumericBySimpleValue()
        {
            // given
            var left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left * 10m).Expression;

            // then
            Assert.IsTrue(multiplication is MultiplicationExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanMultiplySimpleValueByTypedNumeric()
        {
            // given
            var right = new NumericExpression<decimal>(10);

            // when
            var multiplication = (10m * right).Expression;

            // then
            Assert.IsTrue(multiplication is MultiplicationExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanMultiplyNumericBySimpleValue()
        {
            // given
            var left = new NumericExpression(new VariableTerm("x"));

            // when
            var multiplication = (left * 10).Expression;

            // then
            Assert.IsTrue(multiplication is MultiplicationExpression);
            Assert.AreSame(left.Expression, multiplication.Arguments.ElementAt(0));
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanMultiplySimpleValueByNumeric()
        {
            // given
            var right = new NumericExpression(new VariableTerm("x"));

            // when
            var multiplication = (10 * right).Expression;

            // then
            Assert.IsTrue(multiplication is MultiplicationExpression);
            Assert.AreSame(right.Expression, multiplication.Arguments.ElementAt(1));
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
        }

        [TestMethod]
        public void CanChainMultiplicationOfNumerics()
        {
            // given
            NumericExpression<int> op2 = new NumericExpression<int>(10);
            NumericExpression<decimal> op1 = new NumericExpression<decimal>(10);
            NumericExpression<int> op3 = new NumericExpression<int>(5);

            // when
            var multiplication = (op1 * op2 * op3).Expression;

            // then
            Assert.IsTrue(multiplication is MultiplicationExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is MultiplicationExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanDivideTypedNumericsOfMatchingTypes()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression<int> left = new NumericExpression<int>(10);

            // when
            var multiplication = (left / right).Expression;

            // then
            Assert.IsTrue(multiplication is DivisionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanDivideTypedNumericsOfdifferentTypes()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression<decimal> left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left / right).Expression;

            // then
            Assert.IsTrue(multiplication is DivisionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanDivideTypedNumericByUntypedNumeric()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (right / left).Expression;

            // then
            Assert.IsTrue(multiplication is DivisionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanDivideTypedNumericByUntypedNumeric2()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left / right).Expression;

            // then
            Assert.IsTrue(multiplication is DivisionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanDivideTypedNumericBySimpleValue()
        {
            // given
            var left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left / 10m).Expression;

            // then
            Assert.IsTrue(multiplication is DivisionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanDivideSimpleValueByTypedNumeric()
        {
            // given
            var right = new NumericExpression<decimal>(10);

            // when
            var multiplication = (10m / right).Expression;

            // then
            Assert.IsTrue(multiplication is DivisionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanChainDivisionsOfNumerics()
        {
            // given
            NumericExpression<int> op2 = new NumericExpression<int>(10);
            NumericExpression<decimal> op1 = new NumericExpression<decimal>(10);
            NumericExpression<int> op3 = new NumericExpression<int>(5);

            // when
            var multiplication = (op1 / op2 / op3).Expression;

            // then
            Assert.IsTrue(multiplication is DivisionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is DivisionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanAddTypedNumericsOfMatchingTypes()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression<int> left = new NumericExpression<int>(10);

            // when
            var multiplication = (left + right).Expression;

            // then
            Assert.IsTrue(multiplication is AdditionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanAddTypedNumericsOfdifferentTypes()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression<decimal> left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left + right).Expression;

            // then
            Assert.IsTrue(multiplication is AdditionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanAddTypedNumericToUntypedNumeric()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (right + left).Expression;

            // then
            Assert.IsTrue(multiplication is AdditionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanAddTypedNumericToUntypedNumeric2()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left + right).Expression;

            // then
            Assert.IsTrue(multiplication is AdditionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanAddSimpleValueToTypedNumeric()
        {
            // given
            var left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left + 10m).Expression;

            // then
            Assert.IsTrue(multiplication is AdditionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanAddTypedNumericToSimpleValue()
        {
            // given
            var right = new NumericExpression<decimal>(10);

            // when
            var multiplication = (10m + right).Expression;

            // then
            Assert.IsTrue(multiplication is AdditionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanChainAdditionsOfNumerics()
        {
            // given
            NumericExpression<int> op2 = new NumericExpression<int>(10);
            NumericExpression<decimal> op1 = new NumericExpression<decimal>(10);
            NumericExpression<int> op3 = new NumericExpression<int>(5);

            // when
            var multiplication = (op1 + op2 + op3).Expression;

            // then
            Assert.IsTrue(multiplication is AdditionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is AdditionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanSubtractTypedNumericsOfMatchingTypes()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression<int> left = new NumericExpression<int>(10);

            // when
            var multiplication = (left - right).Expression;

            // then
            Assert.IsTrue(multiplication is SubtractionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanSubtractTypedNumericsOfdifferentTypes()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression<decimal> left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left - right).Expression;

            // then
            Assert.IsTrue(multiplication is SubtractionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanSubtractTypedNumericToUntypedNumeric()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (right - left).Expression;

            // then
            Assert.IsTrue(multiplication is SubtractionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanSubtractTypedNumericToUntypedNumeric2()
        {
            // given
            NumericExpression<int> right = new NumericExpression<int>(10);
            NumericExpression left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left - right).Expression;

            // then
            Assert.IsTrue(multiplication is SubtractionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanSubtractSimpleValueFromTypedNumeric()
        {
            // given
            var left = new NumericExpression<decimal>(10);

            // when
            var multiplication = (left - 10m).Expression;

            // then
            Assert.IsTrue(multiplication is SubtractionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanSubtractTypedNumericFromSimpleValue()
        {
            // given
            var right = new NumericExpression<decimal>(10);

            // when
            var multiplication = (10m - right).Expression;

            // then
            Assert.IsTrue(multiplication is SubtractionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanChainSubtractionsOfNumerics()
        {
            // given
            NumericExpression<int> op2 = new NumericExpression<int>(10);
            NumericExpression<decimal> op1 = new NumericExpression<decimal>(10);
            NumericExpression<int> op3 = new NumericExpression<int>(5);

            // when
            var multiplication = (op1 - op2 - op3).Expression;

            // then
            Assert.IsTrue(multiplication is SubtractionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is SubtractionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }
    }
}