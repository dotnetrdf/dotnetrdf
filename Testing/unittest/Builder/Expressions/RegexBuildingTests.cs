using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Test.Builder.Expressions
{
    [TestClass]
    public class RegexBuildingTests  : ExpressionBuilderTestsBase
    {
        [TestMethod]
        public void CanCreateRegexExpressionWithVariableAndString()
        {
            // when
            var regex = Builder.Regex(Builder.Variable("mail"), "@gmail.com$").Expression;

            // then
            Assert.IsTrue(regex is RegexFunction);
            Assert.IsTrue(regex.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(regex.Arguments.ElementAt(1) is ConstantTerm);
        }
    }
}