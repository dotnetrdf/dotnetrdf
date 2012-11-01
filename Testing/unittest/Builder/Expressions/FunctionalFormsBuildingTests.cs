using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Test.Builder.Expressions
{
    [TestClass]
    public class FunctionalFormsBuildingTests : ExpressionBuilderTestsBase
    {
        [TestMethod]
        public void CanCreateBoundFunctionUsingVariableTerm()
        {
            // when
            BooleanExpression bound = Builder.Bound("person");

            // then
            Assert.IsTrue(bound.Expression is BoundFunction);
            Assert.IsTrue(bound.Expression.Arguments.ElementAt(0) is VariableTerm);
        }

        [TestMethod]
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

        [TestMethod]
        public void CanCreateIfFunctionCall()
        {
            // given
            SparqlExpression ifExpr = new BooleanExpression(new VariableTerm("if"));
            SparqlExpression thenExpr = new StringExpression("then this");
            SparqlExpression elseExpr = new StringExpression("else that");

            // when
            RdfTermExpression expression = Builder.If(ifExpr).Then(thenExpr).Else(elseExpr);

            // then
            Assert.IsTrue(expression.Expression is IfElseFunction);
            Assert.AreSame(expression.Expression.Arguments.ElementAt(0), ifExpr.Expression);
            Assert.AreSame(expression.Expression.Arguments.ElementAt(1), thenExpr.Expression);
            Assert.AreSame(expression.Expression.Arguments.ElementAt(2), elseExpr.Expression);
        }

        [TestMethod]
        public void CanCreateIfFunctionCallUsingVariables()
        {
            // given
            SparqlExpression ifExpr = new VariableExpression("if");
            SparqlExpression thenExpr = new VariableExpression("then this");
            SparqlExpression elseExpr = new VariableExpression("else that");

            // when
            RdfTermExpression expression = Builder.If(ifExpr).Then(thenExpr).Else(elseExpr);

            // then
            Assert.IsTrue(expression.Expression is IfElseFunction);
            Assert.AreSame(expression.Expression.Arguments.ElementAt(0), ifExpr.Expression);
            Assert.AreSame(expression.Expression.Arguments.ElementAt(1), thenExpr.Expression);
            Assert.AreSame(expression.Expression.Arguments.ElementAt(2), elseExpr.Expression);
        }
    }
}