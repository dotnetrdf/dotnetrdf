using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Test.Builder
{
    [TestClass]
    public class ExpressionBuilderTests
    {
        private ExpressionBuilder _builder;

        [TestInitialize]
        public void Setup()
        {
            _builder = new ExpressionBuilder();
        }

        public void CanCreateVariableTerm()
        {
            // when
            var variable = _builder.Variable("varName").Expression;

            // then
            Assert.AreEqual("varName", variable.Variables.ElementAt(0));
        }

        [TestMethod]
        public void CanCreateRegexExpressionWithVariableAndString()
        {
            // when
            var regex = _builder.Regex(_builder.Variable("mail"), "@gmail.com$").Expression;

            // then
            Assert.IsTrue(regex is RegexFunction);
            Assert.IsTrue(regex.Arguments.ElementAt(0) is VariableTerm);
            Assert.AreEqual("@gmail.com$", regex.Arguments.ElementAt(1).ToString().Trim('"'));
        }

        [TestMethod]
        public void CanCreateBoundFunctionUsingVariableTerm()
        {
            // when
            var bound = _builder.Bound("person").Expression;

            // then
            Assert.IsTrue(bound is BoundFunction);
            Assert.IsTrue(bound.Arguments.ElementAt(0) is VariableTerm);
        }

        [TestMethod]
        public void CanCreateBoundFunctionUsingVariableName()
        {
            // given
            var variableTerm = new VariableExpression("person");

            // when
            var bound = _builder.Bound(variableTerm).Expression;

            // then
            Assert.IsTrue(bound is BoundFunction);
            Assert.AreSame(variableTerm.Expression, bound.Arguments.ElementAt(0));
        }

        [TestMethod]
        public void CanJoinTwoExpressionWithAndOperator()
        {
            // when
            var conjunction = _builder.Bound("s").And(_builder.Regex(_builder.Variable("s"), "^x")).Expression;

            // then
            Assert.IsTrue(conjunction is AndExpression);
            Assert.IsTrue(conjunction.Arguments.ElementAt(0) is BoundFunction);
            Assert.IsTrue(conjunction.Arguments.ElementAt(1) is RegexFunction);
        }

        [TestMethod]
        public void CanJoinTwoExpressionWithOrOperator()
        {
            // when
            var conjunction = _builder.Bound("s").Or(_builder.Regex(_builder.Variable("s"), "^x")).Expression;

            // then
            Assert.IsTrue(conjunction is OrExpression);
            Assert.IsTrue(conjunction.Arguments.ElementAt(0) is BoundFunction);
            Assert.IsTrue(conjunction.Arguments.ElementAt(1) is RegexFunction);
        }

        [TestMethod]
        public void CanApplyNegationToBooleanExpression()
        {
            // when
            var negatedBound = _builder.Not(_builder.Bound("mail")).Expression;

            // then
            Assert.IsTrue(negatedBound is NotExpression);
            Assert.IsTrue(negatedBound.Arguments.ElementAt(0) is BoundFunction);
        }

        [TestMethod]
        public void CanCreateEqualityComparisonBetweenVariables()
        {
            // when
            var areEqual = _builder.Variable("mail1").Eq(_builder.Variable("mail2")).Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [TestMethod]
        public void CanCreateEqualityComparisonBetweenVariableAndConstant()
        {
            // when
            var areEqual = _builder.Variable("mail1").Eq(_builder.Constant("mail2")).Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanCreateEqualityComparisonBetweenConstantAndVariable()
        {
            // when
            var areEqual = _builder.Constant("mail1").Eq(_builder.Variable("mail2")).Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [TestMethod]
        public void CanCreateGreaterThanOperatorBetweenVariables()
        {
            // when
            var areEqual = _builder.Variable("mail1").Gt(_builder.Variable("mail2")).Expression;

            // then
            Assert.IsTrue(areEqual is GreaterThanExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [TestMethod]
        public void CanCreateGreaterThanOrEqualOperatorBetweenVariables()
        {
            // when
            var areEqual = _builder.Variable("mail1").Ge(_builder.Variable("mail2")).Expression;

            // then
            Assert.IsTrue(areEqual is GreaterThanOrEqualToExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [TestMethod]
        public void CanCreateLessThanOperatorBetweenVariables()
        {
            // when
            var areEqual = _builder.Variable("mail1").Gt(_builder.Variable("mail2")).Expression;

            // then
            Assert.IsTrue(areEqual is LessThanExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [TestMethod]
        public void CanCreateLessThanOrEqualOperatorBetweenVariables()
        {
            // when
            var areEqual = _builder.Variable("mail1").Ge(_builder.Variable("mail2")).Expression;

            // then
            Assert.IsTrue(areEqual is LessThanOrEqualToExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }
    }
}