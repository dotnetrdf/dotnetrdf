using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Comparison;

namespace VDS.RDF.Test.Builder.Expressions
{
    [TestClass]
    public class TypedLiteralExpressionTests : SparqlExpressionTestsBase
    {
        [TestMethod]
        public void ShouldAllowComparisonOperationOnTypedLiteralExpressions()
        {
            // given
            TypedLiteralExpression<bool> left = new TypedLiteralExpression<bool>(true);
            Left = left.Expression;
            TypedLiteralExpression<bool> right = new TypedLiteralExpression<bool>(true);
            Right = right.Expression;

            // then
            AssertExpressionTypeAndCorrectArguments<EqualsExpression>(left == right);
            AssertExpressionTypeAndCorrectArguments<GreaterThanExpression>(left > right);
            AssertExpressionTypeAndCorrectArguments<GreaterThanOrEqualToExpression>(left >= right);
            AssertExpressionTypeAndCorrectArguments<LessThanExpression>(left < right);
            AssertExpressionTypeAndCorrectArguments<LessThanOrEqualToExpression>(left <= right);
            AssertExpressionTypeAndCorrectArguments<NotEqualsExpression>(left != right);
        }

        [TestMethod]
        public void ShouldAllowComparisonOperationOnTypedLiteralExpressionAndLiteralValue()
        {
            // given
            const decimal value = 10;
            TypedLiteralExpression<decimal> left = new TypedLiteralExpression<decimal>(120);
            Left = left.Expression;

            // then
            AssertExpressionTypeAndCorrectArguments<EqualsExpression>(left == value,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, value));
            AssertExpressionTypeAndCorrectArguments<GreaterThanExpression>(left > value,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, value));
            AssertExpressionTypeAndCorrectArguments<GreaterThanOrEqualToExpression>(left >= value,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, value));
            AssertExpressionTypeAndCorrectArguments<LessThanExpression>(left < value,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, value));
            AssertExpressionTypeAndCorrectArguments<LessThanOrEqualToExpression>(left <= value,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, value));
            AssertExpressionTypeAndCorrectArguments<NotEqualsExpression>(left != value,
                assertRightOperand: ex => AssertCorrectConstantTerm(ex, value));
        }

        [TestMethod]
        public void ShouldAllowComparisonOperationOnLiteralValueAndTypedLiteralExpression()
        {
            // given
            const float value = 10.5f;
            TypedLiteralExpression<float> right = new TypedLiteralExpression<float>(120);
            Right = right.Expression;

            // then
            AssertExpressionTypeAndCorrectArguments<EqualsExpression>(value == right,
                assertLeftOperand: ex=>AssertCorrectConstantTerm(ex, value));
            AssertExpressionTypeAndCorrectArguments<GreaterThanExpression>(value > right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, value));
            AssertExpressionTypeAndCorrectArguments<GreaterThanOrEqualToExpression>(value >= right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, value));
            AssertExpressionTypeAndCorrectArguments<LessThanExpression>(value < right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, value));
            AssertExpressionTypeAndCorrectArguments<LessThanOrEqualToExpression>(value <= right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, value));
            AssertExpressionTypeAndCorrectArguments<NotEqualsExpression>(value != right,
                assertLeftOperand: ex => AssertCorrectConstantTerm(ex, value));
        }
    }
}