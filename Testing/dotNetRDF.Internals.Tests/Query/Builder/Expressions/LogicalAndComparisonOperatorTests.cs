/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Linq;
using Xunit;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{

    public class LogicalAndComparisonOperatorTests
    {
        [Fact]
        public void CanJoinTwoBooleanExpressionWithAndOperator()
        {
            // given 
            BooleanExpression b1 = new BooleanExpression(new VariableTerm("a"));
            BooleanExpression b2 = new BooleanExpression(new VariableTerm("b"));

            // when
            var conjunction = (b1 && b2).Expression;

            // then
            Assert.True(conjunction is AndExpression);
            Assert.Same(b1.Expression, conjunction.Arguments.ElementAt(0));
            Assert.Same(b2.Expression, conjunction.Arguments.ElementAt(1));
        }

        [Fact]
        public void ShouldReturnTheExpressionIfAppliedLogicalOperatoWithNull()
        {
            // todo: TP: not sure this is expected behaviour
            // given
            BooleanExpression notNull = new BooleanExpression(new VariableTerm("var"));

            // then
            Assert.Same(notNull, notNull || null);
            Assert.Same(notNull, null || notNull);
// ReSharper disable ConditionIsAlwaysTrueOrFalse
            Assert.Same(notNull, notNull && null);
            Assert.Same(notNull, null && notNull);
// ReSharper restore ConditionIsAlwaysTrueOrFalse
        }

        [Fact]
        public void CanJoinTwoExpressionWithOrOperator()
        {
            // given 
            BooleanExpression b1 = new BooleanExpression(new VariableTerm("a"));
            BooleanExpression b2 = new BooleanExpression(new VariableTerm("b"));

            // when
            var disjunction = (b1 || b2).Expression;

            // then
            Assert.True(disjunction is OrExpression);
            Assert.Same(b1.Expression, disjunction.Arguments.ElementAt(0));
            Assert.Same(b2.Expression, disjunction.Arguments.ElementAt(1));
        }

        [Fact]
        public void CanCreateEqualityComparisonBetweenVariables()
        {
            // given
            VariableExpression v1 = new VariableExpression("v1");
            VariableExpression v2 = new VariableExpression("v2");

            // when
            var areEqual = (v1 == v2).Expression;

            // then
            Assert.True(areEqual is EqualsExpression);
            Assert.True(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.True(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [Fact]
        public void CanCreateEqualityComparisonBetweenVariableAndLiteral()
        {
            // given
            VariableExpression v1 = new VariableExpression("v1");
            LiteralExpression lit = new TypedLiteralExpression<string>("text");

            // when
            var areEqual = (v1 == lit).Expression;

            // then
            Assert.True(areEqual is EqualsExpression);
            Assert.True(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.True(areEqual.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanCreateEqualityComparisonBetweenTypedLiteralAndConcreteValue()
        {
            // given
            var lit = new TypedLiteralExpression<string>("text");

            // when
            var areEqual = (lit == "some value").Expression;

            // then
            Assert.True(areEqual is EqualsExpression);
            Assert.True(areEqual.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(areEqual.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanCreateEqualityComparisonBetweenTypedLiteralAndConcreteValueReversed()
        {
            // given
            var lit = new TypedLiteralExpression<string>("text");

            // when
            var areEqual = ("some value" == lit).Expression;

            // then
            Assert.True(areEqual is EqualsExpression);
            Assert.True(areEqual.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(areEqual.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanCreateEqualityComparisonBetweenUntypedLiteralAndConcreteValue()
        {
            // given
            LiteralExpression lit = new TypedLiteralExpression<string>("text");

            // when
            var areEqual = ("some value" == lit).Expression;

            // then
            Assert.True(areEqual is EqualsExpression);
            Assert.True(areEqual.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(areEqual.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanCreateEqualityComparisonBetweenUntypedLiteralAndConcreteValueReversed()
        {
            // given
            LiteralExpression lit = new TypedLiteralExpression<string>("text");

            // when
            var areEqual = ("some value" == lit).Expression;

            // then
            Assert.True(areEqual is EqualsExpression);
            Assert.True(areEqual.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(areEqual.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanCreateEqualityComparisonBetweenConstantAndVariable()
        {
            // given
            VariableExpression v1 = new VariableExpression("v1");
            LiteralExpression lit = new TypedLiteralExpression<string>("text");

            // when
            var areEqual = (lit == v1).Expression;

            // then
            Assert.True(areEqual is EqualsExpression);
            Assert.True(areEqual.Arguments.ElementAt(0) is ConstantTerm);
            Assert.True(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [Fact]
        public void CanCreateGreaterThanOperatorBetweenVariables()
        {
            // given
            VariableExpression v1 = new VariableExpression("v1");
            VariableExpression v2 = new VariableExpression("v2");

            // when
            var areEqual = (v1 > v2).Expression;

            // then
            Assert.True(areEqual is GreaterThanExpression);
            Assert.True(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.True(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [Fact]
        public void CanCreateGreaterThanOperatorBetweenVariableAndLiteral()
        {
            // given
            VariableExpression v1 = new VariableExpression("v1");
            LiteralExpression literal = new NumericExpression<int>(10);

            // when
            var areEqual = (v1 > literal).Expression;

            // then
            Assert.True(areEqual is GreaterThanExpression);
            Assert.True(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.True(areEqual.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Fact]
        public void CanCreateGreaterThanOrEqualOperatorBetweenVariables()
        {
            // given
            VariableExpression v1 = new VariableExpression("v1");
            VariableExpression v2 = new VariableExpression("v2");

            // when
            var areEqual = (v1 >= v2).Expression;

            // then
            Assert.True(areEqual is GreaterThanOrEqualToExpression);
            Assert.True(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.True(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [Fact]
        public void CanCreateLessThanOperatorBetweenVariables()
        {
            // given
            VariableExpression v1 = new VariableExpression("v1");
            VariableExpression v2 = new VariableExpression("v2");

            // when
            var areEqual = (v1 < v2).Expression;

            // then
            Assert.True(areEqual is LessThanExpression);
            Assert.True(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.True(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [Fact]
        public void CanCreateLessThanOrEqualOperatorBetweenVariables()
        {
            // given
            VariableExpression v1 = new VariableExpression("v1");
            VariableExpression v2 = new VariableExpression("v2");

            // when
            var areEqual = (v1 <= v2).Expression;

            // then
            Assert.True(areEqual is LessThanOrEqualToExpression);
            Assert.True(areEqual.Arguments.ElementAt(0) is VariableTerm);
            Assert.True(areEqual.Arguments.ElementAt(1) is VariableTerm);
        }

        [Fact]
        public void CanCreateEqualityComparisonBetweenRdfTerms()
        {
            // given
            IriExpression left = new IriExpression(new Uri("urn:unit:test1"));
            IriExpression right = new IriExpression(new Uri("urn:unit:test1"));

            // when
            var areEqual = (left == right).Expression;

            // then
            Assert.True(areEqual is EqualsExpression);
            Assert.Same(left.Expression, areEqual.Arguments.ElementAt(0));
            Assert.Same(right.Expression, areEqual.Arguments.ElementAt(1));
        }

        [Fact]
        public void CanApplyLessThanOperatorBetweenSimpleValuesAndVariables()
        {
            // given
            VariableExpression var = new VariableExpression("var");

            // then
            Assert.True((10 < var).Expression is LessThanExpression);
            Assert.True((var < 10).Expression is LessThanExpression);
            Assert.True((10m < var).Expression is LessThanExpression);
            Assert.True((var < 10m).Expression is LessThanExpression);
            Assert.True((10f < var).Expression is LessThanExpression);
            Assert.True((var < 10f).Expression is LessThanExpression);
            Assert.True((10d < var).Expression is LessThanExpression);
            Assert.True((var < 10d).Expression is LessThanExpression);
            Assert.True(("10" < var).Expression is LessThanExpression);
            Assert.True((var < "10").Expression is LessThanExpression);
            Assert.True((true < var).Expression is LessThanExpression);
            Assert.True((var < true).Expression is LessThanExpression);
            Assert.True((new DateTime(2010, 10, 10) < var).Expression is LessThanExpression);
            Assert.True((var < new DateTime(2010, 10, 10)).Expression is LessThanExpression);
        }

        [Fact]
        public void CanApplyLessThanOrEqualOperatorBetweenSimpleValuesAndVariables()
        {
            // given
            VariableExpression var = new VariableExpression("var");

            // then
            Assert.True((10 <= var).Expression is LessThanOrEqualToExpression);
            Assert.True((var <= 10).Expression is LessThanOrEqualToExpression);
            Assert.True((10m <= var).Expression is LessThanOrEqualToExpression);
            Assert.True((var <= 10m).Expression is LessThanOrEqualToExpression);
            Assert.True((10f <= var).Expression is LessThanOrEqualToExpression);
            Assert.True((var <= 10f).Expression is LessThanOrEqualToExpression);
            Assert.True((10d <= var).Expression is LessThanOrEqualToExpression);
            Assert.True((var <= 10d).Expression is LessThanOrEqualToExpression);
            Assert.True(("10" <= var).Expression is LessThanOrEqualToExpression);
            Assert.True((var <= "10").Expression is LessThanOrEqualToExpression);
            Assert.True((true <= var).Expression is LessThanOrEqualToExpression);
            Assert.True((var <= true).Expression is LessThanOrEqualToExpression);
            Assert.True((new DateTime(2010, 10, 10) <= var).Expression is LessThanOrEqualToExpression);
            Assert.True((var <= new DateTime(2010, 10, 10)).Expression is LessThanOrEqualToExpression);
        }

        [Fact]
        public void CanApplyGreaterThanOperatorBetweenSimpleValuesAndVariables()
        {
            // given
            VariableExpression var = new VariableExpression("var");

            // then
            Assert.True((10 > var).Expression is GreaterThanExpression);
            Assert.True((var > 10).Expression is GreaterThanExpression);
            Assert.True((10m > var).Expression is GreaterThanExpression);
            Assert.True((var > 10m).Expression is GreaterThanExpression);
            Assert.True((10f > var).Expression is GreaterThanExpression);
            Assert.True((var > 10f).Expression is GreaterThanExpression);
            Assert.True((10d > var).Expression is GreaterThanExpression);
            Assert.True((var > 10d).Expression is GreaterThanExpression);
            Assert.True(("10" > var).Expression is GreaterThanExpression);
            Assert.True((var > "10").Expression is GreaterThanExpression);
            Assert.True((true > var).Expression is GreaterThanExpression);
            Assert.True((var > true).Expression is GreaterThanExpression);
            Assert.True((new DateTime(2010, 10, 10) > var).Expression is GreaterThanExpression);
            Assert.True((var > new DateTime(2010, 10, 10)).Expression is GreaterThanExpression);
        }

        [Fact]
        public void CanApplyGreaterThanOrEqualOperatorBetweenSimpleValuesAndVariables()
        {
            // given
            VariableExpression var = new VariableExpression("var");

            // then
            Assert.True((10 >= var).Expression is GreaterThanOrEqualToExpression);
            Assert.True((var >= 10).Expression is GreaterThanOrEqualToExpression);
            Assert.True((10m >= var).Expression is GreaterThanOrEqualToExpression);
            Assert.True((var >= 10m).Expression is GreaterThanOrEqualToExpression);
            Assert.True((10f >= var).Expression is GreaterThanOrEqualToExpression);
            Assert.True((var >= 10f).Expression is GreaterThanOrEqualToExpression);
            Assert.True((10d >= var).Expression is GreaterThanOrEqualToExpression);
            Assert.True((var >= 10d).Expression is GreaterThanOrEqualToExpression);
            Assert.True(("10" >= var).Expression is GreaterThanOrEqualToExpression);
            Assert.True((var >= "10").Expression is GreaterThanOrEqualToExpression);
            Assert.True((true >= var).Expression is GreaterThanOrEqualToExpression);
            Assert.True((var >= true).Expression is GreaterThanOrEqualToExpression);
            Assert.True((new DateTime(2010, 10, 10) >= var).Expression is GreaterThanOrEqualToExpression);
            Assert.True((var >= new DateTime(2010, 10, 10)).Expression is GreaterThanOrEqualToExpression);
        }

        [Fact]
        public void CanApplyEqualsOperatorBetweenSimpleValuesAndVariables()
        {
            // given
            VariableExpression var = new VariableExpression("var");

            // then
            Assert.True((10 == var).Expression is EqualsExpression);
            Assert.True((var == 10).Expression is EqualsExpression);
            Assert.True((10m == var).Expression is EqualsExpression);
            Assert.True((var == 10m).Expression is EqualsExpression);
            Assert.True((10f == var).Expression is EqualsExpression);
            Assert.True((var == 10f).Expression is EqualsExpression);
            Assert.True((10d == var).Expression is EqualsExpression);
            Assert.True((var == 10d).Expression is EqualsExpression);
            Assert.True(("10" == var).Expression is EqualsExpression);
            Assert.True((var == "10").Expression is EqualsExpression);
            Assert.True((true == var).Expression is EqualsExpression);
            Assert.True((var == true).Expression is EqualsExpression);
            Assert.True((new DateTime(2010, 10, 10) == var).Expression is EqualsExpression);
            Assert.True((var == new DateTime(2010, 10, 10)).Expression is EqualsExpression);
        }

        [Fact]
        public void CanApplyNotEqualsOperatorBetweenSimpleValuesAndVariables()
        {
            // given
            VariableExpression var = new VariableExpression("var");

            // then
            Assert.True((10 != var).Expression is NotEqualsExpression);
            Assert.True((var != 10).Expression is NotEqualsExpression);
            Assert.True((10m != var).Expression is NotEqualsExpression);
            Assert.True((var != 10m).Expression is NotEqualsExpression);
            Assert.True((10f != var).Expression is NotEqualsExpression);
            Assert.True((var != 10f).Expression is NotEqualsExpression);
            Assert.True((10d != var).Expression is NotEqualsExpression);
            Assert.True((var != 10d).Expression is NotEqualsExpression);
            Assert.True(("10" != var).Expression is NotEqualsExpression);
            Assert.True((var != "10").Expression is NotEqualsExpression);
            Assert.True((true != var).Expression is NotEqualsExpression);
            Assert.True((var != true).Expression is NotEqualsExpression);
            Assert.True((new DateTime(2010, 10, 10) != var).Expression is NotEqualsExpression);
            Assert.True((var != new DateTime(2010, 10, 10)).Expression is NotEqualsExpression);
        }
    }
}