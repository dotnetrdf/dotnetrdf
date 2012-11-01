using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder;
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
            var multiplication = left.Multiply(right).Expression;

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
            var multiplication = left.Multiply(right).Expression;

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
            var multiplication = right.Multiply(left).Expression;

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
            var multiplication = left.Multiply(right).Expression;

            // then
            Assert.IsTrue(multiplication is MultiplicationExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanChainMultiplicationOfNumerics()
        {
            // given
            NumericExpression<int> op2 = new NumericExpression<int>(10);
            NumericExpression<decimal> op1 = new NumericExpression<decimal>(10);
            NumericExpression<int> op3 = new NumericExpression<int>(5);

            // when
            var multiplication = op1.Multiply(op2).Multiply(op3).Expression;

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
            var multiplication = left.Divide(right).Expression;

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
            var multiplication = left.Divide(right).Expression;

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
            var multiplication = right.Divide(left).Expression;

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
            var multiplication = left.Divide(right).Expression;

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
            var multiplication = op1.Divide(op2).Divide(op3).Expression;

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
            var multiplication = left.Add(right).Expression;

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
            var multiplication = left.Add(right).Expression;

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
            var multiplication = right.Add(left).Expression;

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
            var multiplication = left.Add(right).Expression;

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
            var multiplication = op1.Add(op2).Add(op3).Expression;

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
            var multiplication = left.Subtract(right).Expression;

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
            var multiplication = left.Subtract(right).Expression;

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
            var multiplication = right.Subtract(left).Expression;

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
            var multiplication = left.Subtract(right).Expression;

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
            var multiplication = op1.Subtract(op2).Subtract(op3).Expression;

            // then
            Assert.IsTrue(multiplication is SubtractionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(0) is SubtractionExpression);
            Assert.IsTrue(multiplication.Arguments.ElementAt(1) is ConstantTerm);
        }
    }
}