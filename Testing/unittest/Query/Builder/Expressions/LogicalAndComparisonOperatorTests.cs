using System;
using System.Linq;
using NUnit.Framework;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    [TestFixture]
    public class LogicalAndComparisonOperatorTests
    {
        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void CanCreateEqualityComparisonBetweenVariableAndLiteral()
        {
            // given
            VariableExpression v1 = new VariableExpression("v1");
            LiteralExpression lit = new TypedLiteralExpression<string>("text");

            // when
            var areEqual = (v1 == lit).Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Test]
        public void CanCreateEqualityComparisonBetweenTypedLiteralAndConcreteValue()
        {
            // given
            var lit = new TypedLiteralExpression<string>("text");

            // when
            var areEqual = (lit == "some value").Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Test]
        public void CanCreateEqualityComparisonBetweenTypedLiteralAndConcreteValueReversed()
        {
            // given
            var lit = new TypedLiteralExpression<string>("text");

            // when
            var areEqual = ("some value" == lit).Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Test]
        public void CanCreateEqualityComparisonBetweenUntypedLiteralAndConcreteValue()
        {
            // given
            LiteralExpression lit = new TypedLiteralExpression<string>("text");

            // when
            var areEqual = ("some value" == lit).Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Test]
        public void CanCreateEqualityComparisonBetweenUntypedLiteralAndConcreteValueReversed()
        {
            // given
            LiteralExpression lit = new TypedLiteralExpression<string>("text");

            // when
            var areEqual = ("some value" == lit).Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Test]
        public void CanCreateEqualityComparisonBetweenConstantAndVariable()
        {
            // given
            VariableExpression v1 = new VariableExpression("v1");
            LiteralExpression lit = new TypedLiteralExpression<string>("text");

            // when
            var areEqual = (lit == v1).Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.IsTrue(areEqual.Arguments.ElementAt(0) is ConstantTerm);
            Assert.IsTrue(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void CanCreateEqualityComparisonBetweenRdfTerms()
        {
            // given
            IriExpression left = new IriExpression(new Uri("urn:unit:test1"));
            IriExpression right = new IriExpression(new Uri("urn:unit:test1"));

            // when
            var areEqual = (left == right).Expression;

            // then
            Assert.IsTrue(areEqual is EqualsExpression);
            Assert.AreSame(left.Expression, areEqual.Arguments.ElementAt(0));
            Assert.AreSame(right.Expression, areEqual.Arguments.ElementAt(1));
        }

        [Test]
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

        [Test]
        public void CanApplyLessThanOrEqualOperatorBetweenSimpleValuesAndVariables()
        {
            // given
            VariableExpression var = new VariableExpression("var");

            // then
            Assert.IsTrue((10 <= var).Expression is LessThanOrEqualToExpression);
            Assert.IsTrue((var <= 10).Expression is LessThanOrEqualToExpression);
            Assert.IsTrue((10m <= var).Expression is LessThanOrEqualToExpression);
            Assert.IsTrue((var <= 10m).Expression is LessThanOrEqualToExpression);
            Assert.IsTrue((10f <= var).Expression is LessThanOrEqualToExpression);
            Assert.IsTrue((var <= 10f).Expression is LessThanOrEqualToExpression);
            Assert.IsTrue((10d <= var).Expression is LessThanOrEqualToExpression);
            Assert.IsTrue((var <= 10d).Expression is LessThanOrEqualToExpression);
            Assert.IsTrue(("10" <= var).Expression is LessThanOrEqualToExpression);
            Assert.IsTrue((var <= "10").Expression is LessThanOrEqualToExpression);
            Assert.IsTrue((true <= var).Expression is LessThanOrEqualToExpression);
            Assert.IsTrue((var <= true).Expression is LessThanOrEqualToExpression);
            Assert.IsTrue((new DateTime(2010, 10, 10) <= var).Expression is LessThanOrEqualToExpression);
            Assert.IsTrue((var <= new DateTime(2010, 10, 10)).Expression is LessThanOrEqualToExpression);
        }

        [Test]
        public void CanApplyGreaterThanOperatorBetweenSimpleValuesAndVariables()
        {
            // given
            VariableExpression var = new VariableExpression("var");

            // then
            Assert.IsTrue((10 > var).Expression is GreaterThanExpression);
            Assert.IsTrue((var > 10).Expression is GreaterThanExpression);
            Assert.IsTrue((10m > var).Expression is GreaterThanExpression);
            Assert.IsTrue((var > 10m).Expression is GreaterThanExpression);
            Assert.IsTrue((10f > var).Expression is GreaterThanExpression);
            Assert.IsTrue((var > 10f).Expression is GreaterThanExpression);
            Assert.IsTrue((10d > var).Expression is GreaterThanExpression);
            Assert.IsTrue((var > 10d).Expression is GreaterThanExpression);
            Assert.IsTrue(("10" > var).Expression is GreaterThanExpression);
            Assert.IsTrue((var > "10").Expression is GreaterThanExpression);
            Assert.IsTrue((true > var).Expression is GreaterThanExpression);
            Assert.IsTrue((var > true).Expression is GreaterThanExpression);
            Assert.IsTrue((new DateTime(2010, 10, 10) > var).Expression is GreaterThanExpression);
            Assert.IsTrue((var > new DateTime(2010, 10, 10)).Expression is GreaterThanExpression);
        }

        [Test]
        public void CanApplyGreaterThanOrEqualOperatorBetweenSimpleValuesAndVariables()
        {
            // given
            VariableExpression var = new VariableExpression("var");

            // then
            Assert.IsTrue((10 >= var).Expression is GreaterThanOrEqualToExpression);
            Assert.IsTrue((var >= 10).Expression is GreaterThanOrEqualToExpression);
            Assert.IsTrue((10m >= var).Expression is GreaterThanOrEqualToExpression);
            Assert.IsTrue((var >= 10m).Expression is GreaterThanOrEqualToExpression);
            Assert.IsTrue((10f >= var).Expression is GreaterThanOrEqualToExpression);
            Assert.IsTrue((var >= 10f).Expression is GreaterThanOrEqualToExpression);
            Assert.IsTrue((10d >= var).Expression is GreaterThanOrEqualToExpression);
            Assert.IsTrue((var >= 10d).Expression is GreaterThanOrEqualToExpression);
            Assert.IsTrue(("10" >= var).Expression is GreaterThanOrEqualToExpression);
            Assert.IsTrue((var >= "10").Expression is GreaterThanOrEqualToExpression);
            Assert.IsTrue((true >= var).Expression is GreaterThanOrEqualToExpression);
            Assert.IsTrue((var >= true).Expression is GreaterThanOrEqualToExpression);
            Assert.IsTrue((new DateTime(2010, 10, 10) >= var).Expression is GreaterThanOrEqualToExpression);
            Assert.IsTrue((var >= new DateTime(2010, 10, 10)).Expression is GreaterThanOrEqualToExpression);
        }

        [Test]
        public void CanApplyEqualsOperatorBetweenSimpleValuesAndVariables()
        {
            // given
            VariableExpression var = new VariableExpression("var");

            // then
            Assert.IsTrue((10 == var).Expression is EqualsExpression);
            Assert.IsTrue((var == 10).Expression is EqualsExpression);
            Assert.IsTrue((10m == var).Expression is EqualsExpression);
            Assert.IsTrue((var == 10m).Expression is EqualsExpression);
            Assert.IsTrue((10f == var).Expression is EqualsExpression);
            Assert.IsTrue((var == 10f).Expression is EqualsExpression);
            Assert.IsTrue((10d == var).Expression is EqualsExpression);
            Assert.IsTrue((var == 10d).Expression is EqualsExpression);
            Assert.IsTrue(("10" == var).Expression is EqualsExpression);
            Assert.IsTrue((var == "10").Expression is EqualsExpression);
            Assert.IsTrue((true == var).Expression is EqualsExpression);
            Assert.IsTrue((var == true).Expression is EqualsExpression);
            Assert.IsTrue((new DateTime(2010, 10, 10) == var).Expression is EqualsExpression);
            Assert.IsTrue((var == new DateTime(2010, 10, 10)).Expression is EqualsExpression);
        }

        [Test]
        public void CanApplyNotEqualsOperatorBetweenSimpleValuesAndVariables()
        {
            // given
            VariableExpression var = new VariableExpression("var");

            // then
            Assert.IsTrue((10 != var).Expression is NotEqualsExpression);
            Assert.IsTrue((var != 10).Expression is NotEqualsExpression);
            Assert.IsTrue((10m != var).Expression is NotEqualsExpression);
            Assert.IsTrue((var != 10m).Expression is NotEqualsExpression);
            Assert.IsTrue((10f != var).Expression is NotEqualsExpression);
            Assert.IsTrue((var != 10f).Expression is NotEqualsExpression);
            Assert.IsTrue((10d != var).Expression is NotEqualsExpression);
            Assert.IsTrue((var != 10d).Expression is NotEqualsExpression);
            Assert.IsTrue(("10" != var).Expression is NotEqualsExpression);
            Assert.IsTrue((var != "10").Expression is NotEqualsExpression);
            Assert.IsTrue((true != var).Expression is NotEqualsExpression);
            Assert.IsTrue((var != true).Expression is NotEqualsExpression);
            Assert.IsTrue((new DateTime(2010, 10, 10) != var).Expression is NotEqualsExpression);
            Assert.IsTrue((var != new DateTime(2010, 10, 10)).Expression is NotEqualsExpression);
        }
    }
}