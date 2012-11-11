using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Arithmetic;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Test.Builder.Expressions
{
    [TestClass]
    public class NumericExpressionTests
    {
        private ISparqlExpression _left, _right;

        [TestInitialize]
        public void Setup()
        {
            _left = 10.ToConstantTerm();
            _right = 15.ToConstantTerm();
        }

        [TestMethod]
        public void ShouldAllowComparingGenericAndNongenericNumericExpressions()
        {
            var left = new NumericExpression(_left);
            var right = new NumericExpression<int>(15);
            _right = right.Expression;

            AssertExpressionTypeAndCorrectArguments<EqualsExpression>(left == right);
            AssertExpressionTypeAndCorrectArguments<GreaterThanExpression>(left > right);
            AssertExpressionTypeAndCorrectArguments<GreaterThanOrEqualToExpression>(left >= right);
            AssertExpressionTypeAndCorrectArguments<LessThanExpression>(left < right);
            AssertExpressionTypeAndCorrectArguments<LessThanOrEqualToExpression>(left <= right);
            AssertExpressionTypeAndCorrectArguments<NotEqualsExpression>(left != right);
        }

        [TestMethod]
        public void ShouldAllowComparingGenericAndNongenericNumericExpressionsReversed()
        {
            var right = new NumericExpression(_right);
            var left = new NumericExpression<int>(15);
            _left = left.Expression;

            AssertExpressionTypeAndCorrectArguments<EqualsExpression>(left == right);
            AssertExpressionTypeAndCorrectArguments<GreaterThanExpression>(left > right);
            AssertExpressionTypeAndCorrectArguments<GreaterThanOrEqualToExpression>(left >= right);
            AssertExpressionTypeAndCorrectArguments<LessThanExpression>(left < right);
            AssertExpressionTypeAndCorrectArguments<LessThanOrEqualToExpression>(left <= right);
            AssertExpressionTypeAndCorrectArguments<NotEqualsExpression>(left != right);
        }

        [TestMethod]
        public void ShouldAllowArithmeticOperatorsWithGenericAndNongenericNumericExpressions()
        {
            NumericExpression left = new NumericExpression(_left);
            NumericExpression right = new NumericExpression<int>(15);
            _right = right.Expression;

            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(left * right);
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(left / right);
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(left + right);
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(left - right);
        }

        [TestMethod]
        public void ShouldAllowArithmeticOperatorsWithGenericAndNongenericNumericExpressionsReversed()
        {
            NumericExpression left = new NumericExpression(_left);
            NumericExpression right = new NumericExpression<int>(15);
            _right = right.Expression;

            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(left * right);
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(left / right);
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(left + right);
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(left - right);
        }

        [TestMethod]
        public void ShouldAllowUsingArithmeticOperatorsWithNumericExpressionAndVariableExpression()
        {
            // given
            NumericExpression left = new NumericExpression(_left);
            VariableExpression right = new VariableExpression("number");
            _right = right.Expression;

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(left * right);
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(left / right);
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(left + right);
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(left - right);
        }

        [TestMethod]
        public void ShouldAllowUsingArithmeticOperatorsWithVariableExpressionAndNumericExpression()
        {
            // given
            NumericExpression right = new NumericExpression(_right);
            VariableExpression left = new VariableExpression("number");
            _left = left.Expression;

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(left * right);
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(left / right);
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(left + right);
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(left - right);
        }

        [TestMethod]
        public void ShouldAllowUsingArithmeticOperatorsWithNumericExpressionAndInteger()
        {
            // given
            const int operandValue = 10;
            NumericExpression right = new NumericExpression(_right);

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(operandValue * right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(operandValue / right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(operandValue + right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(operandValue - right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
        }

        [TestMethod]
        public void ShouldAllowUsingArithmeticOperatorsWithIntegerAndNumericExpression()
        {
            // given
            const int operandValue = 10;
            NumericExpression left = new NumericExpression(_left);

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(left * operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(left / operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(left + operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(left - operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
        }

        [TestMethod]
        public void ShouldAllowUsingArithmeticOperatorsWithNumericExpressionAndLongInteger()
        {
            // given
            const long operandValue = 10;
            NumericExpression right = new NumericExpression(_right);

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(operandValue * right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(operandValue / right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(operandValue + right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(operandValue - right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
        }

        [TestMethod]
        public void ShouldAllowUsingArithmeticOperatorsWithLongIntegerAndNumericExpression()
        {
            // given
            const long operandValue = 10;
            NumericExpression left = new NumericExpression(_left);

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(left * operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(left / operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(left + operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(left - operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
        }

        [TestMethod]
        public void ShouldAllowUsingArithmeticOperatorsWithNumericExpressionAndShortInteger()
        {
            // given
            const short operandValue = 10;
            NumericExpression right = new NumericExpression(_right);

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(operandValue * right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(operandValue / right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(operandValue + right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(operandValue - right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
        }

        [TestMethod]
        public void ShouldAllowUsingArithmeticOperatorsWithShortIntegerAndNumericExpression()
        {
            // given
            const short operandValue = 10;
            NumericExpression left = new NumericExpression(_left);

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(left * operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(left / operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(left + operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(left - operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
        }

        [TestMethod]
        public void ShouldAllowUsingArithmeticOperatorsWithNumericExpressionAndDecimal()
        {
            // given
            const decimal operandValue = 10.5m;
            NumericExpression right = new NumericExpression(_right);

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(operandValue * right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(operandValue / right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(operandValue + right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(operandValue - right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
        }

        [TestMethod]
        public void ShouldAllowUsingArithmeticOperatorsWithDecimalAndNumericExpression()
        {
            // given
            const decimal operandValue = 10.5m;
            NumericExpression left = new NumericExpression(_left);

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(left * operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(left / operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(left + operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(left - operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
        }

        [TestMethod]
        public void ShouldAllowUsingArithmeticOperatorsWithNumericExpressionAndDouble()
        {
            // given
            const double operandValue = 10.5d;
            NumericExpression right = new NumericExpression(_right);

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(operandValue * right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(operandValue / right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(operandValue + right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(operandValue - right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
        }

        [TestMethod]
        public void ShouldAllowUsingArithmeticOperatorsWithDoubleAndNumericExpression()
        {
            // given
            const double operandValue = 10.5d;
            NumericExpression left = new NumericExpression(_left);

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(left * operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(left / operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(left + operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(left - operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
        }

        [TestMethod]
        public void ShouldAllowUsingArithmeticOperatorsWithNumericExpressionAndFloat()
        {
            // given
            const float operandValue = 10.5f;
            NumericExpression right = new NumericExpression(_right);

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(operandValue * right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(operandValue / right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(operandValue + right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(operandValue - right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
        }

        [TestMethod]
        public void ShouldAllowUsingArithmeticOperatorsWithFloatAndNumericExpression()
        {
            // given
            const float operandValue = 10.5f;
            NumericExpression left = new NumericExpression(_left);

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(left * operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(left / operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(left + operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(left - operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
        }

        [TestMethod]
        public void ShouldAllowUsingArithmeticOperatorsWithNumericExpressionAndByte()
        {
            // given
            const byte operandValue = 10;
            NumericExpression right = new NumericExpression(_right);

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(operandValue * right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(operandValue / right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(operandValue + right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(operandValue - right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
        }

        [TestMethod]
        public void ShouldAllowUsingArithmeticOperatorsWithByteAndNumericExpression()
        {
            // given
            const byte operandValue = 10;
            NumericExpression left = new NumericExpression(_left);

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(left * operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(left / operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(left + operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(left - operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
        }

        [TestMethod]
        public void ShouldAllowUsingArithmeticOperatorsWithNumericExpressionAndSignedByte()
        {
            // given
            const sbyte operandValue = 10;
            NumericExpression right = new NumericExpression(_right);

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(operandValue * right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(operandValue / right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(operandValue + right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(operandValue - right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
        }

        [TestMethod]
        public void ShouldAllowUsingArithmeticOperatorsWithSignedByteAndNumericExpression()
        {
            // given
            const sbyte operandValue = 10;
            NumericExpression left = new NumericExpression(_left);

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(left * operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(left / operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(left + operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(left - operandValue,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, operandValue));
        }

        private void AssertExpressionTypeAndCorrectArguments<TExpressionType>(SparqlExpression expression,
            Action<ISparqlExpression> assertLeftOperand = null,
            Action<ISparqlExpression> assertRightOperand = null)
        {
            Assert.AreEqual(typeof(TExpressionType), expression.Expression.GetType());
            if (assertLeftOperand == null)
            {
                Assert.AreSame(_left, expression.Expression.Arguments.ElementAt(0));
            }
            else
            {
                assertLeftOperand(expression.Expression.Arguments.ElementAt(0));
            }
            if (assertRightOperand == null)
            {
                Assert.AreSame(_right, expression.Expression.Arguments.ElementAt(1));
            }
            else
            {
                assertRightOperand(expression.Expression.Arguments.ElementAt(1));
            }
        }

        private void AssertCorrectConstantTerm<TConstant>(ISparqlExpression operand, TConstant value)
        {
            Assert.IsTrue(operand is ConstantTerm);
            Assert.AreEqual(value.ToConstantTerm().ToString(), operand.ToString());
        }
    }
}