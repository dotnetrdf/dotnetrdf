using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Set;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Test.Builder.Expressions
{
    [TestClass]
    public class SparqlExpressionTests
    {
        private class TestingSparqlExpression : SparqlExpression
        {
            public TestingSparqlExpression(ISparqlExpression expression) : base(expression)
            {
            }
        }

        [TestMethod]
        public void CanCreateInFunction()
        {
            // given
            SparqlExpression rdfTerm = new TestingSparqlExpression(new VariableTerm("x"));
 
            // when
            BooleanExpression expression = rdfTerm.In();

            // then
            Assert.IsTrue(expression.Expression is InFunction);
            Assert.AreEqual(1, expression.Expression.Arguments.Count());
        }
    }
}