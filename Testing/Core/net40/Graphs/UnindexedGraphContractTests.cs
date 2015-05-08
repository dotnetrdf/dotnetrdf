using NUnit.Framework;
using VDS.RDF.Collections;

namespace VDS.RDF.Graphs
{
    [TestFixture]
    public class UnindexedGraphContractTests
        : AbstractGraphContractTests
    {

        protected override IGraph GetGraphInstance()
        {
            return new Graph(new TripleCollection());
        }
    }
}