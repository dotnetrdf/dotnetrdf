using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Test.Builder
{
    [TestClass]
    public class GraphPatternBuilderTests
    {
        private GraphPatternBuilder _builder;

        [TestInitialize]
        public void Setup()
        {
            _builder = new GraphPatternBuilder(new NamespaceMapper());
        }

        [TestMethod]
        public void ShouldReturnNullIfThereAreNoFiltersTriplePatternsOrChildGraphPatterns()
        {
            // when
            var graphPattern = _builder.BuildGraphPattern();

            // then
            Assert.IsNull(graphPattern);
        }

        [TestMethod]
        public void ShouldAllowUsingISparqlExpressionForFilter()
        {
            // given
            ISparqlExpression expression = new IsIriFunction(new VariableTerm("x"));
            _builder.Filter(expression);

            // when
            GraphPattern graphPattern = _builder.BuildGraphPattern();

            // then
            Assert.IsTrue(graphPattern.IsFiltered);
            Assert.AreSame(expression, graphPattern.Filter.Expression);
        }
    }
}