using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Test.Builder.Expressions
{
    [TestClass]
    public class LogicalAndComparisonOperatorTests
    {
        [TestMethod]
        public void CanJoinTwoBooleanExpressionWithAndOperator()
        {
            // given 
            BooleanExpression b1 = new BooleanExpression(new VariableTerm("a"));
            BooleanExpression b2 = new BooleanExpression(new VariableTerm("b"));

            // when
            var conjunction = (b1 && b2).Expression;

            // then
            Assert.IsTrue(conjunction is AndExpression);
            Assert.AreSame(b1.Expression, conjunction.Arguments.ElementAt(0));
            Assert.AreSame(b2.Expression, conjunction.Arguments.ElementAt(1));
        }

        [TestMethod]
        public void ShouldReturnTheExpressionIfAppliedLogicalOperatoWithNull()
        {
            // todo: TP: not sure this is expected behaviour
            // given
            BooleanExpression notNull = new BooleanExpression(new VariableTerm("var"));

            // then
            Assert.AreSame(notNull, notNull || null);
            Assert.AreSame(notNull, null || notNull);
// ReSharper disable ConditionIsAlwaysTrueOrFalse
            Assert.AreSame(notNull, notNull && null);
            Assert.AreSame(notNull, null && notNull);
// ReSharper restore ConditionIsAlwaysTrueOrFalse
        }

        [TestMethod]
        public void CanJoinTwoExpressionWithOrOperator()
        {
            // given 
            BooleanExpression b1 = new BooleanExpression(new VariableTerm("a"));
            BooleanExpression b2 = new BooleanExpression(new VariableTerm("b"));

            // when
            var disjunction = (b1 || b2).Expression;

            // then
            Assert.IsTrue(disjunction is OrExpression);
            Assert.AreSame(b1.Expression, disjunction.Arguments.ElementAt(0));
            Assert.AreSame(b2.Expression, disjunction.Arguments.ElementAt(1));
        }

        [TestMethod]
        public void CanCreateEqualityComparisonBetweenVariables()
        {
            // given
            VariableExpression v1 = new VariableExpression("v1");
            VariableExpression v2 = new VariableExpression("v2");

            // when
            var areEqual = (v1 == v2).Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [TestMethod]
        public void CanCreateEqualityComparisonBetweenVariableAndLiteral()
        {
            // given
            VariableExpression v1 = new VariableExpression("v1");
            LiteralExpression lit = new StringExpression("text");

            // when
            var areEqual = (v1 == lit).Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanCreateEqualityComparisonBetweenTypedLiteralAndConcreteValue()
        {
            // given
            StringExpression lit = new StringExpression("text");

            // when
            var areEqual = (lit == "some value").Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanCreateEqualityComparisonBetweenTypedLiteralAndConcreteValueReversed()
        {
            // given
            StringExpression lit = new StringExpression("text");

            // when
            var areEqual = ("some value" == lit).Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanCreateEqualityComparisonBetweenUntypedLiteralAndConcreteValue()
        {
            // given
            LiteralExpression lit = new StringExpression("text");

            // when
            var areEqual = ("some value" == lit).Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanCreateEqualityComparisonBetweenUntypedLiteralAndConcreteValueReversed()
        {
            // given
            LiteralExpression lit = new StringExpression("text");

            // when
            var areEqual = ("some value" == lit).Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanCreateEqualityComparisonBetweenConstantAndVariable()
        {
            // given
            VariableExpression v1 = new VariableExpression("v1");
            LiteralExpression lit = new StringExpression("text");

            // when
            var areEqual = (lit == v1).Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [TestMethod]
        public void CanCreateGreaterThanOperatorBetweenVariables()
        {
            // given
            VariableExpression v1 = new VariableExpression("v1");
            VariableExpression v2 = new VariableExpression("v2");

            // when
            var areEqual = (v1 > v2).Expression;

            // then
            Assert.IsTrue(areEqual is GreaterThanExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [TestMethod]
        public void CanCreateGreaterThanOperatorBetweenVariableAndLiteral()
        {
            // given
            VariableExpression v1 = new VariableExpression("v1");
            LiteralExpression literal = new NumericExpression<int>(10);

            // when
            var areEqual = (v1 > literal).Expression;

            // then
            Assert.IsTrue(areEqual is GreaterThanExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void CanCreateGreaterThanOrEqualOperatorBetweenVariables()
        {
            // given
            VariableExpression v1 = new VariableExpression("v1");
            VariableExpression v2 = new VariableExpression("v2");

            // when
            var areEqual = (v1 >= v2).Expression;

            // then
            Assert.IsTrue(areEqual is GreaterThanOrEqualToExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [TestMethod]
        public void CanCreateLessThanOperatorBetweenVariables()
        {
            // given
            VariableExpression v1 = new VariableExpression("v1");
            VariableExpression v2 = new VariableExpression("v2");

            // when
            var areEqual = (v1 < v2).Expression;

            // then
            Assert.IsTrue(areEqual is LessThanExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [TestMethod]
        public void CanCreateLessThanOrEqualOperatorBetweenVariables()
        {
            // given
            VariableExpression v1 = new VariableExpression("v1");
            VariableExpression v2 = new VariableExpression("v2");

            // when
            var areEqual = (v1 <= v2).Expression;

            // then
            Assert.IsTrue(areEqual is LessThanOrEqualToExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [TestMethod]
        public void CanCreateEqualityComparisonBetweenRdfTerms()
        {
            // given
            IriExpression left = new IriExpression("urn:unit:test1");
            IriExpression right = new IriExpression("urn:unit:test1");

            // when
            var areEqual = (left == right).Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.AreSame(left.Expression, areEqual.Arguments.ElementAt(0));
            Assert.AreSame(right.Expression, areEqual.Arguments.ElementAt(1));
        }

        [TestMethod]
        public void CanApplyLessThanOperatorBetweenSimpleValuesAndVariables()
        {
            // given
            VariableExpression var = new VariableExpression("var");

            // then
            Assert.IsTrue((10 < var).Expression is LessThanExpression);
            Assert.IsTrue((var < 10).Expression is LessThanExpression);
            Assert.IsTrue((10m < var).Expression is LessThanExpression);
            Assert.IsTrue((var < 10m).Expression is LessThanExpression);
            Assert.IsTrue((10f < var).Expression is LessThanExpression);
            Assert.IsTrue((var < 10f).Expression is LessThanExpression);
            Assert.IsTrue((10d < var).Expression is LessThanExpression);
            Assert.IsTrue((var < 10d).Expression is LessThanExpression);
            Assert.IsTrue(("10" < var).Expression is LessThanExpression);
            Assert.IsTrue((var < "10").Expression is LessThanExpression);
            Assert.IsTrue((true < var).Expression is LessThanExpression);
            Assert.IsTrue((var < true).Expression is LessThanExpression);
            Assert.IsTrue((new DateTime(2010, 10, 10) < var).Expression is LessThanExpression);
            Assert.IsTrue((var < new DateTime(2010, 10, 10)).Expression is LessThanExpression);
        }

        [TestMethod]
        public void CanApplyLessThanOrEqualOperatorBetweenSimpleValuesAndVariables()
        {
            // given
            VariableExpression var = new VariableExpression("var");

            // then
            Assert.IsTrue((10 <= var).Expression is LessThanExpression);
            Assert.IsTrue((var <= 10).Expression is LessThanExpression);
            Assert.IsTrue((10m <= var).Expression is LessThanExpression);
            Assert.IsTrue((var <= 10m).Expression is LessThanExpression);
            Assert.IsTrue((10f <= var).Expression is LessThanExpression);
            Assert.IsTrue((var <= 10f).Expression is LessThanExpression);
            Assert.IsTrue((10d <= var).Expression is LessThanExpression);
            Assert.IsTrue((var <= 10d).Expression is LessThanExpression);
            Assert.IsTrue(("10" <= var).Expression is LessThanExpression);
            Assert.IsTrue((var <= "10").Expression is LessThanExpression);
            Assert.IsTrue((true <= var).Expression is LessThanExpression);
            Assert.IsTrue((var <= true).Expression is LessThanExpression);
            Assert.IsTrue((new DateTime(2010, 10, 10) <= var).Expression is LessThanExpression);
            Assert.IsTrue((var <= new DateTime(2010, 10, 10)).Expression is LessThanExpression);
        }

        [TestMethod]
        public void CanApplyGreaterThanOperatorBetweenSimpleValuesAndVariables()
        {
            // given
            VariableExpression var = new VariableExpression("var");

            // then
            Assert.IsTrue((10 > var).Expression is LessThanExpression);
            Assert.IsTrue((var > 10).Expression is LessThanExpression);
            Assert.IsTrue((10m > var).Expression is LessThanExpression);
            Assert.IsTrue((var > 10m).Expression is LessThanExpression);
            Assert.IsTrue((10f > var).Expression is LessThanExpression);
            Assert.IsTrue((var > 10f).Expression is LessThanExpression);
            Assert.IsTrue((10d > var).Expression is LessThanExpression);
            Assert.IsTrue((var > 10d).Expression is LessThanExpression);
            Assert.IsTrue(("10" > var).Expression is LessThanExpression);
            Assert.IsTrue((var > "10").Expression is LessThanExpression);
            Assert.IsTrue((true > var).Expression is LessThanExpression);
            Assert.IsTrue((var > true).Expression is LessThanExpression);
            Assert.IsTrue((new DateTime(2010, 10, 10) > var).Expression is LessThanExpression);
            Assert.IsTrue((var > new DateTime(2010, 10, 10)).Expression is LessThanExpression);
        }

        [TestMethod]
        public void CanApplyGreaterThanOrEqualOperatorBetweenSimpleValuesAndVariables()
        {
            // given
            VariableExpression var = new VariableExpression("var");

            // then
            Assert.IsTrue((10 >= var).Expression is LessThanExpression);
            Assert.IsTrue((var >= 10).Expression is LessThanExpression);
            Assert.IsTrue((10m >= var).Expression is LessThanExpression);
            Assert.IsTrue((var >= 10m).Expression is LessThanExpression);
            Assert.IsTrue((10f >= var).Expression is LessThanExpression);
            Assert.IsTrue((var >= 10f).Expression is LessThanExpression);
            Assert.IsTrue((10d >= var).Expression is LessThanExpression);
            Assert.IsTrue((var >= 10d).Expression is LessThanExpression);
            Assert.IsTrue(("10" >= var).Expression is LessThanExpression);
            Assert.IsTrue((var >= "10").Expression is LessThanExpression);
            Assert.IsTrue((true >= var).Expression is LessThanExpression);
            Assert.IsTrue((var >= true).Expression is LessThanExpression);
            Assert.IsTrue((new DateTime(2010, 10, 10) >= var).Expression is LessThanExpression);
            Assert.IsTrue((var >= new DateTime(2010, 10, 10)).Expression is LessThanExpression);
        }
    }
}