using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder;
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
            var variable = _builder.Variable("varName");

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
            var variableTerm = new VariableTerm("person");

            // when
            var bound = _builder.Bound(variableTerm).Expression;

            // then
            Assert.IsTrue(bound is BoundFunction);
            Assert.AreSame(variableTerm, bound.Arguments.ElementAt(0));
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
    }
}