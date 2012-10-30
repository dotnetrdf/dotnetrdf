using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder;

namespace VDS.RDF.Test.Builder
{
    [TestClass]
    public class ExpressionBuilderTests
    {
        private ExpressionBuilder _builder;

        [TestInitialize]
        public void Setup()
        {
            _builder = new ExpressionBuilder();
        }

        [TestMethod]
        public void CanCreateRegexExpression()
        {
            // when
           // _builder.Regex("");
        }
    }
}