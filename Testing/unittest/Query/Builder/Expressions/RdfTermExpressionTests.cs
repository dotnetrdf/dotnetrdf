using System.Linq;
using NUnit.Framework;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Set;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    [TestFixture]
    public class SparqlExpressionTests
    {
        private class TestingSparqlExpression : SparqlExpression
        {
            public TestingSparqlExpression(ISparqlExpression expression) : base(expression)
            {
            }
        }

        [Test]
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