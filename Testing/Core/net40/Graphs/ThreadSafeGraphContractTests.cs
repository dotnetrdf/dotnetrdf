using NUnit.Framework;

namespace VDS.RDF.Graphs
{
    [TestFixture]
    public class ThreadSafeGraphContractTests
        : AbstractGraphContractTests
    {
        protected override IGraph GetGraphInstance()
        {
            return new ThreadSafeGraph();
        }
    }
}