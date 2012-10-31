using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder;

namespace VDS.RDF.Test.Builder.Expressions
{
    public class ExpressionBuilderTestsBase
    {
        public ExpressionBuilder Builder { get; private set; }

        [TestInitialize]
        public void Setup()
        {
            Builder = new ExpressionBuilder();
        }
    }
}