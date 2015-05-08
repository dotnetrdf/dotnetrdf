using NUnit.Framework;

namespace VDS.RDF.Graphs
{
    [TestFixture]
    public class WrapperGraphContractTests
        : AbstractGraphContractTests
    {
        protected override IGraph GetGraphInstance()
        {
            return new TestWrapperGraph();
        }

        private class TestWrapperGraph
            : WrapperGraph {}
    }
}