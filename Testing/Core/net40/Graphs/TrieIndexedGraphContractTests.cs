using NUnit.Framework;
using VDS.RDF.Collections;

namespace VDS.RDF.Graphs
{
    [TestFixture]
    public class TrieIndexedGraphContractTests
        : AbstractGraphContractTests
    {
        protected override IGraph GetGraphInstance()
        {
            return new Graph(new TrieIndexedTripleCollection());
        }
    }
}