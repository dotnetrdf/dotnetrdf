using System.Linq;
using NUnit.Framework;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    public partial class ExpressionBuilderTests
    {
        [Test]
        public void CanCreateBoundFunctionUsingVariableTerm()
        {
            // when
            BooleanExpression bound = Builder.Bound("person");

            // then
            Assert.IsTrue(bound.Expression is BoundFunction);
            Assert.IsTrue(bound.Expression.Arguments.ElementAt(0) is VariableTerm);
        }

        [Test]
        public void CanCreateBoundFunctionUsingVariableName()
        {
            // given
            var variableTerm = new VariableExpression("person");

            // when
            BooleanExpression bound = Builder.Bound(variableTerm);

            // then
            Assert.IsTrue(bound.Expression is BoundFunction);
            Assert.AreSame(variableTerm.Expression, bound.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void CanCreateIfFunctionCall()
        {
            // given
            var ifExpr = new BooleanExpression(new VariableTerm("if"));
            SparqlExpression thenExpr = new TypedLiteralExpression<string>("then this");
            SparqlExpression elseExpr = new TypedLiteralExpression<string>("else that");

            // when
            RdfTermExpression expression = Builder.If(ifExpr).Then(thenExpr).Else(elseExpr);

            // then
            Assert.IsTrue(expression.Expression is IfElseFunction);
            Assert.AreSame(expression.Expression.Arguments.ElementAt(0), ifExpr.Expression);
            Assert.AreSame(expression.Expression.Arguments.ElementAt(1), thenExpr.Expression);
            Assert.AreSame(expression.Expression.Arguments.ElementAt(2), elseExpr.Expression);
        }

        [Test]
        public void CanCreateIfFunctionCallUsingVariables()
        {
            // given
            var ifExpr = new VariableExpression("if");
            SparqlExpression thenExpr = new VariableExpression("then this");
            SparqlExpression elseExpr = new VariableExpression("else that");

            // when
            RdfTermExpression expression = Builder.If(ifExpr).Then(thenExpr).Else(elseExpr);

            // then
            Assert.IsTrue(expression.Expression is IfElseFunction);
            Assert.AreSame(ifExpr.Expression, expression.Expression.Arguments.ElementAt(0));
            Assert.AreSame(thenExpr.Expression, expression.Expression.Arguments.ElementAt(1));
            Assert.AreSame(elseExpr.Expression, expression.Expression.Arguments.ElementAt(2));
        }

        [Test]
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
            Assert.IsTrue(coalesce.Expression is CoalesceFunction);
            Assert.AreSame(expr1.Expression, coalesce.Expression.Arguments.ElementAt(0));
            Assert.AreSame(expr2.Expression, coalesce.Expression.Arguments.ElementAt(1));
            Assert.AreSame(expr3.Expression, coalesce.Expression.Arguments.ElementAt(2));
            Assert.AreSame(expr4.Expression, coalesce.Expression.Arguments.ElementAt(3));
        }
    }
}