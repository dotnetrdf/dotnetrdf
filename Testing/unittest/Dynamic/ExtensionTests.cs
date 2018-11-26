namespace VDS.RDF.Dynamic
{
    using Xunit;

    public class ExtensionTests
    {
        [Fact]
        public void m1()
        {
            var value = new Graph().AsDynamic();

            Assert.NotNull(value);
            Assert.IsType<DynamicGraph>(value);
        }

        [Fact]
        public void m2()
        {
            var value = new Graph().CreateBlankNode().AsDynamic();

            Assert.NotNull(value);
            Assert.IsType<DynamicNode>(value);
        }

        [Fact]
        public void m3()
        {
            var value = new[] { 0, 1, 2 }.AsRdfCollection();

            Assert.NotNull(value);
            Assert.IsType<RdfCollection>(value);
        }
    }
}
