using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Test.Builder.Expressions
{
    [TestClass]
    public class BinaryOperatorTests : ExpressionBuilderTestsBase
    {
        [TestMethod]
        public void CanJoinTwoExpressionWithAndOperator()
        {
            // when
            var conjunction = Builder.Bound("s").And(Builder.Regex(Builder.Variable("s"), "^x")).Expression;

            // then
            Assert.IsTrue(conjunction is AndExpression);
            Assert.IsTrue(conjunction.Arguments.ElementAt(0) is BoundFunction);
            Assert.IsTrue(conjunction.Arguments.ElementAt(1) is RegexFunction);
        }

        [TestMethod]
        public void CanJoinTwoExpressionWithOrOperator()
        {
            // when
            var conjunction = Builder.Bound("s").Or(Builder.Regex(Builder.Variable("s"), "^x")).Expression;

            // then
            Assert.IsTrue(conjunction is OrExpression);
            Assert.IsTrue(conjunction.Arguments.ElementAt(0) is BoundFunction);
            Assert.IsTrue(conjunction.Arguments.ElementAt(1) is RegexFunction);
        }

        [TestMethod]
        public void CanCreateEqualityComparisonBetweenVariables()
        {
            // when
            var areEqual = Builder.Variable("mail1").Eq(Builder.Variable("mail2")).Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [TestMethod]
        public void CanCreateEqualityComparisonBetweenVariableAndConstant()
        {
            // when
            var areEqual = Builder.Variable("mail1").Eq(Builder.Constant("mail2")).Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanCreateEqualityComparisonBetweenConstantAndVariable()
        {
            // when
            var areEqual = Builder.Constant("mail1").Eq(Builder.Variable("mail2")).Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [TestMethod]
        public void CanCreateGreaterThanOperatorBetweenVariables()
        {
            // when
            var areEqual = Builder.Variable("mail1").Gt(Builder.Variable("mail2")).Expression;

            // then
            Assert.IsTrue(areEqual is GreaterThanExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [TestMethod]
        public void CanCreateGreaterThanOrEqualOperatorBetweenVariables()
        {
            // when
            var areEqual = Builder.Variable("mail1").Ge(Builder.Variable("mail2")).Expression;

            // then
            Assert.IsTrue(areEqual is GreaterThanOrEqualToExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [TestMethod]
        public void CanCreateLessThanOperatorBetweenVariables()
        {
            // when
            var areEqual = Builder.Variable("mail1").Lt(Builder.Variable("mail2")).Expression;

            // then
            Assert.IsTrue(areEqual is LessThanExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [TestMethod]
        public void CanCreateLessThanOrEqualOperatorBetweenVariables()
        {
            // when
            var areEqual = Builder.Variable("mail1").Le(Builder.Variable("mail2")).Expression;

            // then
            Assert.IsTrue(areEqual is LessThanOrEqualToExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }
    }
}