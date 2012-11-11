using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Comparison;

namespace VDS.RDF.Test.Builder.Expressions
{
    [TestClass]
    public class LiteralExpressionTests : SparqlExpressionTestsBase
    {
        private const string TestingStringValue = "text";

        [TestInitialize]
        public void Setup()
        {
            Left = "text".ToConstantTerm();
            Right = "text".ToConstantTerm();
        }

        [TestMethod]
        public void ShouldAllowEuqlityComparisonBetweenLiteralExpressionAndString()
        {
            // given
            var left = new LiteralExpression(Left);

            // then
            AssertExpressionTypeAndCorrectArguments<EqualsExpression>(left == TestingStringValue,
                assertRightOperand: op => AssertCorrectConstantTerm(op, TestingStringValue));
            AssertExpressionTypeAndCorrectArguments<NotEqualsExpression>(left != TestingStringValue,
                assertRightOperand: op => AssertCorrectConstantTerm(op, TestingStringValue));
        }

        [TestMethod]
        public void ShouldAllowEuqlityComparisonBetweenStringAndLiteralExpression()
        {
            // given
            var right = new LiteralExpression(Right);

            // then
            AssertExpressionTypeAndCorrectArguments<EqualsExpression>(TestingStringValue == right,
                assertLeftOperand: op => AssertCorrectConstantTerm(op, TestingStringValue));
            AssertExpressionTypeAndCorrectArguments<NotEqualsExpression>(TestingStringValue != right,
                assertLeftOperand: op => AssertCorrectConstantTerm(op, TestingStringValue));
        }
    }
}