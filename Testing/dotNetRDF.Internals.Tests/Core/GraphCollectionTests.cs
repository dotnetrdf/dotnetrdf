using System.Linq;
using Xunit;

namespace VDS.RDF.Core
{
    public class GraphCollectionTests
    {
        [Fact]
        public void GraphCollectionBasic3()
        {
            GraphCollection collection = new GraphCollection();
            Graph g = new Graph();
            collection.Add(g, true);

            Assert.True(collection.Contains(g.BaseUri));
        }

        [Fact]
        public void GraphCollectionBasic4()
        {
            GraphCollection collection = new GraphCollection();
            Graph g = new Graph();
            collection.Add(g, true);

            Assert.True(collection.Contains(g.BaseUri));
            Assert.True(collection.GraphUris.Contains(null));
        }

    }
}
