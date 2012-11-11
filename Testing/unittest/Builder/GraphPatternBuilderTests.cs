using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder;

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
    }
}