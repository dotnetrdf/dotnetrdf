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

using Xunit;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Comparison;

namespace VDS.RDF.Query.Builder.Expressions
{

    public class TypedLiteralExpressionTests : SparqlExpressionTestsBase
    {
        [Fact]
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

        [Fact]
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

        [Fact]
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