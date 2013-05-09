using NUnit.Framework;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    [TestFixture]
    public class LiteralExpressionTests : SparqlExpressionTestsBase
    {
        private const string TestingStringValue = "text";

        [TestInitialize]
        public void Setup()
        {
            Left = "text".ToConstantTerm();
            Right = "text".ToConstantTerm();
        }

        [Test]
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

        [Test]
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

        [Test]
        public void ShouldAllowExtractingUntypedLiteral()
        {
            // given
            var literal = new LiteralExpression(5.5.ToConstantTerm());

            // when
            LiteralExpression simpleLiteral = literal.ToSimpleLiteral();

            // then
            Assert.IsTrue(simpleLiteral.Expression is ConstantTerm);
            Assert.AreEqual("\"5.5\"", simpleLiteral.Expression.ToString());
        }
    }
}