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

using System.Linq;
using Xunit;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    public partial class ExpressionBuilderTests
    {
        [Fact]
        public void CanCreateBoundFunctionUsingVariableTerm()
        {
            // when
            BooleanExpression bound = Builder.Bound("person");

            // then
            Assert.True(bound.Expression is BoundFunction);
            Assert.True(bound.Expression.Arguments.ElementAt(0) is VariableTerm);
        }

        [Fact]
        public void CanCreateBoundFunctionUsingVariableName()
        {
            // given
            var variableTerm = new VariableExpression("person");

            // when
            BooleanExpression bound = Builder.Bound(variableTerm);

            // then
            Assert.True(bound.Expression is BoundFunction);
            Assert.Same(variableTerm.Expression, bound.Expression.Arguments.ElementAt(0));
        }

        [Fact]
        public void CanCreateIfFunctionCall()
        {
            // given
            var ifExpr = new BooleanExpression(new VariableTerm("if"));
            SparqlExpression thenExpr = new TypedLiteralExpression<string>("then this");
            SparqlExpression elseExpr = new TypedLiteralExpression<string>("else that");

            // when
            RdfTermExpression expression = Builder.If(ifExpr).Then(thenExpr).Else(elseExpr);

            // then
            Assert.True(expression.Expression is IfElseFunction);
            Assert.Same(expression.Expression.Arguments.ElementAt(0), ifExpr.Expression);
            Assert.Same(expression.Expression.Arguments.ElementAt(1), thenExpr.Expression);
            Assert.Same(expression.Expression.Arguments.ElementAt(2), elseExpr.Expression);
        }

        [Fact]
        public void CanCreateIfFunctionCallUsingVariables()
        {
            // given
            var ifExpr = new VariableExpression("if");
            SparqlExpression thenExpr = new VariableExpression("then this");
            SparqlExpression elseExpr = new VariableExpression("else that");

            // when
            RdfTermExpression expression = Builder.If(ifExpr).Then(thenExpr).Else(elseExpr);

            // then
            Assert.True(expression.Expression is IfElseFunction);
            Assert.Same(ifExpr.Expression, expression.Expression.Arguments.ElementAt(0));
            Assert.Same(thenExpr.Expression, expression.Expression.Arguments.ElementAt(1));
            Assert.Same(elseExpr.Expression, expression.Expression.Arguments.ElementAt(2));
        }

        [Fact]
        public void CanCreateTheCoalesceFunctionCall()
        {
            // given
            SparqlExpression expr1 = new VariableExpression("x");
            SparqlExpression expr2 = new TypedLiteralExpression<string>("str");
            SparqlExpression expr3 = new NumericExpression<int>(10);
            SparqlExpression expr4 = new NumericExpression<float>(10.5f) / new NumericExpression<float>(0);

            // when
            RdfTermExpression coalesce = Builder.Coalesce(expr1, expr2, expr3, expr4);

            // then
            Assert.True(coalesce.Expression is CoalesceFunction);
            Assert.Same(expr1.Expression, coalesce.Expression.Arguments.ElementAt(0));
            Assert.Same(expr2.Expression, coalesce.Expression.Arguments.ElementAt(1));
            Assert.Same(expr3.Expression, coalesce.Expression.Arguments.ElementAt(2));
            Assert.Same(expr4.Expression, coalesce.Expression.Arguments.ElementAt(3));
        }
    }
}