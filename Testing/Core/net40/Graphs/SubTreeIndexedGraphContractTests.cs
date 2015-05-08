using NUnit.Framework;
using VDS.RDF.Collections;

namespace VDS.RDF.Graphs
{
    [TestFixture]
    public class SubTreeIndexedGraphContractTests
        : AbstractGraphContractTests
    {
        protected override IGraph GetGraphInstance()
        {
            return new Graph(new SubTreeIndexedTripleCollection());
        }
    }
}