using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Test.Builder.Expressions
{
    public partial class ExpressionBuilderTests
    {
        [TestMethod]
        public void CanCreateRegexExpressionWithVariableAndString()
        {
            // given
            VariableExpression var = new VariableExpression("mail");

            // when
            var regex = Builder.Regex(var, "@gmail.com$").Expression;

            // then
            Assert.IsTrue(regex is RegexFunction);
            Assert.IsTrue(regex.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(regex.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToStrlenFunctionWithVariableParameter()
        {
            // given
            VariableExpression var = new VariableExpression("mail");

            // when
            NumericExpression<int> strlen = Builder.StrLen(var);

            // then
            Assert.IsTrue(strlen.Expression is StrLenFunction);
            Assert.AreSame(var.Expression, strlen.Expression.Arguments.ElementAt(0));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToStrlenFunctionWithStringLiteralParameter()
        {
            // given
            var literal = new TypedLiteralExpression<string>("mail");

            // when
            NumericExpression<int> strlen = Builder.StrLen(literal);

            // then
            Assert.IsTrue(strlen.Expression is StrLenFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
        }
    }
}