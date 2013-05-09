using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Arithmetic;

namespace VDS.RDF.Test.Builder.Expressions
{
    [TestClass]
    public class VariableExpressionTests : SparqlExpressionTestsBase
    {
        [TestInitialize]
        public void Setup()
        {
            Left = 10.ToConstantTerm();
            Right = 15.ToConstantTerm();
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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