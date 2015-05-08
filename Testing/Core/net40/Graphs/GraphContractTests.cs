using NUnit.Framework;

namespace VDS.RDF.Graphs
{
    [TestFixture]
    public class GraphContractTests
        : AbstractGraphContractTests
    {
        protected override IGraph GetGraphInstance()
        {
            return new Graph();
        }
    }
}