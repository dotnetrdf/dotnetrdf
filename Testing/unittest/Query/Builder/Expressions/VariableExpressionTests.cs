using System;
using NUnit.Framework;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Arithmetic;

namespace VDS.RDF.Query.Builder.Expressions
{
    [TestFixture]
    public class VariableExpressionTests : SparqlExpressionTestsBase
    {
        [TestInitialize]
        public void Setup()
        {
            Left = 10.ToConstantTerm();
            Right = 15.ToConstantTerm();
        }

        [Test]
        public void ShouldAllowUsingArithmeticOperatorsWithVariableExpressionAndInteger()
        {
            // given
            const int operandValue = 10;
            var right = new VariableExpression("var"); Right = right.Expression;

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

        [Test]
        public void ShouldAllowUsingArithmeticOperatorsWithIntegerAndVariableExpression()
        {
            // given
            const int operandValue = 10;
            var left = new VariableExpression("var"); Left = left.Expression;

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

        [Test]
        public void ShouldAllowUsingArithmeticOperatorsWithVariableExpressionAndShortInteger()
        {
            // given
            const short operandValue = 10;
            var right = new VariableExpression("var"); Right = right.Expression;

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

        [Test]
        public void ShouldAllowUsingArithmeticOperatorsWithShortIntegerAndVariableExpression()
        {
            // given
            const short operandValue = 10;
            var left = new VariableExpression("var"); Left = left.Expression;

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

        [Test]
        public void ShouldAllowUsingArithmeticOperatorsWithVariableExpressionAndLongInteger()
        {
            // given
            const long operandValue = 10;
            var right = new VariableExpression("var"); Right = right.Expression;

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

        [Test]
        public void ShouldAllowUsingArithmeticOperatorsWithLongIntegerAndVariableExpression()
        {
            // given
            const long operandValue = 10;
            var left = new VariableExpression("var"); Left = left.Expression;

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

        [Test]
        public void ShouldAllowUsingArithmeticOperatorsWithVariableExpressionAndByte()
        {
            // given
            const byte operandValue = 10;
            var right = new VariableExpression("var"); Right = right.Expression;

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

        [Test]
        public void ShouldAllowUsingArithmeticOperatorsWithByteAndVariableExpression()
        {
            // given
            const byte operandValue = 10;
            var left = new VariableExpression("var"); Left = left.Expression;

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

        [Test]
        public void ShouldAllowUsingArithmeticOperatorsWithVariableExpressionAndSignedByte()
        {
            // given
            const sbyte operandValue = 10;
            var right = new VariableExpression("var"); Right = right.Expression;

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

        [Test]
        public void ShouldAllowUsingArithmeticOperatorsWithSignedByteAndVariableExpression()
        {
            // given
            const sbyte operandValue = 10;
            var left = new VariableExpression("var"); Left = left.Expression;

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

        [Test]
        public void ShouldAllowUsingArithmeticOperatorsWithVariableExpressionAndDecimal()
        {
            // given
            const Decimal operandValue = 10;
            var right = new VariableExpression("var"); Right = right.Expression;

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

        [Test]
        public void ShouldAllowUsingArithmeticOperatorsWithDecimalAndVariableExpression()
        {
            // given
            const Decimal operandValue = 10;
            var left = new VariableExpression("var"); Left = left.Expression;

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

        [Test]
        public void ShouldAllowUsingArithmeticOperatorsWithVariableExpressionAndFloat()
        {
            // given
            const float operandValue = 10;
            var right = new VariableExpression("var"); Right = right.Expression;

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

        [Test]
        public void ShouldAllowUsingArithmeticOperatorsWithFloatAndVariableExpression()
        {
            // given
            const float operandValue = 10;
            var left = new VariableExpression("var"); Left = left.Expression;

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

        [Test]
        public void ShouldAllowUsingArithmeticOperatorsWithVariableExpressionAndDouble()
        {
            // given
            const Double operandValue = 10;
            var right = new VariableExpression("var"); Right = right.Expression;

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

        [Test]
        public void ShouldAllowUsingArithmeticOperatorsWithDoubleAndVariableExpression()
        {
            // given
            const Double operandValue = 10;
            var left = new VariableExpression("var"); Left = left.Expression;

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

        [Test]
        public void ShouldAllowUsingArithmeticOperatorsBetweenVariableExpressions()
        {
            // given
            var left = new VariableExpression("left");
            Left = left.Expression;
            var right = new VariableExpression("right");
            Right = right.Expression;

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(left * right);
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(left / right);
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(left + right);
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(left - right);
        }
    }
}