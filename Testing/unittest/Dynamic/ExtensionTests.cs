namespace VDS.RDF.Dynamic
{
    using System.Collections;
    using Xunit;

    public class ExtensionTests
    {
        [Fact]
        public void Augments_graph()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");
            var d = g.AsDynamic();

            Assert.Equal<IGraph>(g, d);
            Assert.IsType<DynamicGraph>(d);
        }

        [Fact]
        public void Augments_node()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");
            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var d = s.AsDynamic();

            Assert.Equal<INode>(s, d);
            Assert.IsType<DynamicNode>(d);
        }

        [Fact]
        public void Augments_enumerable()
        {
            var enumerable = new[] { 0, 1, 2 };
            var d = enumerable.AsRdfCollection();

            Assert.Equal<IEnumerable>(enumerable, d);
            Assert.IsType<RdfCollection>(d);
        }
    }
}
