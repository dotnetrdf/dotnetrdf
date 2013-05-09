using NUnit.Framework;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Arithmetic;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    [TestFixture]
    public class NumericExpressionTests : SparqlExpressionTestsBase
    {
        [TestInitialize]
        public void Setup()
        {
            Left = 10.ToConstantTerm();
            Right = 15.ToConstantTerm();
        }

        [Test]
        public void ShouldAllowComparingGenericAndNongenericNumericExpressions()
        {
            var left = new NumericExpression(Left);
            var right = new NumericExpression<int>(15);
            Right = right.Expression;

            AssertExpressionTypeAndCorrectArguments<EqualsExpression>(left == right);
            AssertExpressionTypeAndCorrectArguments<GreaterThanExpression>(left > right);
            AssertExpressionTypeAndCorrectArguments<GreaterThanOrEqualToExpression>(left >= right);
            AssertExpressionTypeAndCorrectArguments<LessThanExpression>(left < right);
            AssertExpressionTypeAndCorrectArguments<LessThanOrEqualToExpression>(left <= right);
            AssertExpressionTypeAndCorrectArguments<NotEqualsExpression>(left != right);
        }

        [Test]
        public void ShouldAllowComparingGenericAndNongenericNumericExpressionsReversed()
        {
            var right = new NumericExpression(Right);
            var left = new NumericExpression<int>(15);
            Left = left.Expression;

            AssertExpressionTypeAndCorrectArguments<EqualsExpression>(left == right);
            AssertExpressionTypeAndCorrectArguments<GreaterThanExpression>(left > right);
            AssertExpressionTypeAndCorrectArguments<GreaterThanOrEqualToExpression>(left >= right);
            AssertExpressionTypeAndCorrectArguments<LessThanExpression>(left < right);
            AssertExpressionTypeAndCorrectArguments<LessThanOrEqualToExpression>(left <= right);
            AssertExpressionTypeAndCorrectArguments<NotEqualsExpression>(left != right);
        }

        [Test]
        public void ShouldAllowArithmeticOperatorsWithGenericAndNongenericNumericExpressions()
        {
            NumericExpression left = new NumericExpression(Left);
            NumericExpression right = new NumericExpression<int>(15);
            Right = right.Expression;

            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(left * right);
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(left / right);
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(left + right);
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(left - right);
        }

        [Test]
        public void ShouldAllowArithmeticOperatorsWithGenericAndNongenericNumericExpressionsReversed()
        {
            NumericExpression left = new NumericExpression(Left);
            NumericExpression right = new NumericExpression<int>(15);
            Right = right.Expression;

            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(left * right);
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(left / right);
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(left + right);
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(left - right);
        }

        [Test]
        public void ShouldAllowUsingArithmeticOperatorsWithNumericExpressionAndVariableExpression()
        {
            // given
            NumericExpression left = new NumericExpression(Left);
            VariableExpression right = new VariableExpression("number");
            Right = right.Expression;

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(left * right);
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(left / right);
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(left + right);
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(left - right);
        }

        [Test]
        public void ShouldAllowUsingArithmeticOperatorsWithVariableExpressionAndNumericExpression()
        {
            // given
            NumericExpression right = new NumericExpression(Right);
            VariableExpression left = new VariableExpression("number");
            Left = left.Expression;

            // then
            AssertExpressionTypeAndCorrectArguments<MultiplicationExpression>(left * right);
            AssertExpressionTypeAndCorrectArguments<DivisionExpression>(left / right);
            AssertExpressionTypeAndCorrectArguments<AdditionExpression>(left + right);
            AssertExpressionTypeAndCorrectArguments<SubtractionExpression>(left - right);
        }

        [Test]
        public void ShouldAllowUsingArithmeticOperatorsWithNumericExpressionAndInteger()
        {
            // given
            const int operandValue = 10;
            NumericExpression right = new NumericExpression(Right);

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
        public void ShouldAllowUsingArithmeticOperatorsWithIntegerAndNumericExpression()
        {
            // given
            const int operandValue = 10;
            NumericExpression left = new NumericExpression(Left);

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
        public void ShouldAllowUsingArithmeticOperatorsWithNumericExpressionAndLongInteger()
        {
            // given
            const long operandValue = 10;
            NumericExpression right = new NumericExpression(Right);

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
        public void ShouldAllowUsingArithmeticOperatorsWithLongIntegerAndNumericExpression()
        {
            // given
            const long operandValue = 10;
            NumericExpression left = new NumericExpression(Left);

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
        public void ShouldAllowUsingArithmeticOperatorsWithNumericExpressionAndShortInteger()
        {
            // given
            const short operandValue = 10;
            NumericExpression right = new NumericExpression(Right);

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
        public void ShouldAllowUsingArithmeticOperatorsWithShortIntegerAndNumericExpression()
        {
            // given
            const short operandValue = 10;
            NumericExpression left = new NumericExpression(Left);

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
        public void ShouldAllowUsingArithmeticOperatorsWithNumericExpressionAndDecimal()
        {
            // given
            const decimal operandValue = 10.5m;
            NumericExpression right = new NumericExpression(Right);

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
        public void ShouldAllowUsingArithmeticOperatorsWithDecimalAndNumericExpression()
        {
            // given
            const decimal operandValue = 10.5m;
            NumericExpression left = new NumericExpression(Left);

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
        public void ShouldAllowUsingArithmeticOperatorsWithNumericExpressionAndDouble()
        {
            // given
            const double operandValue = 10.5d;
            NumericExpression right = new NumericExpression(Right);

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
        public void ShouldAllowUsingArithmeticOperatorsWithDoubleAndNumericExpression()
        {
            // given
            const double operandValue = 10.5d;
            NumericExpression left = new NumericExpression(Left);

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
        public void ShouldAllowUsingArithmeticOperatorsWithNumericExpressionAndFloat()
        {
            // given
            const float operandValue = 10.5f;
            NumericExpression right = new NumericExpression(Right);

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
        public void ShouldAllowUsingArithmeticOperatorsWithFloatAndNumericExpression()
        {
            // given
            const float operandValue = 10.5f;
            NumericExpression left = new NumericExpression(Left);

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
        public void ShouldAllowUsingArithmeticOperatorsWithNumericExpressionAndByte()
        {
            // given
            const byte operandValue = 10;
            NumericExpression right = new NumericExpression(Right);

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
        public void ShouldAllowUsingArithmeticOperatorsWithByteAndNumericExpression()
        {
            // given
            const byte operandValue = 10;
            NumericExpression left = new NumericExpression(Left);

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
        public void ShouldAllowUsingArithmeticOperatorsWithNumericExpressionAndSignedByte()
        {
            // given
            const sbyte operandValue = 10;
            NumericExpression right = new NumericExpression(Right);

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
        public void ShouldAllowUsingArithmeticOperatorsWithSignedByteAndNumericExpression()
        {
            // given
            const sbyte operandValue = 10;
            NumericExpression left = new NumericExpression(Left);

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
    }
}