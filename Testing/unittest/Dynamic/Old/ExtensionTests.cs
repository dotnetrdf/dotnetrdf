namespace VDS.RDF.Dynamic.Old
{
    using System;
    using Xunit;

    public class ExtensionTests
    {
        [Fact]
        public void m1()
        {
            var value = new Graph().AsDynamic();

            Assert.IsType<DynamicGraph>(value);
        }

        [Fact]
        public void m2()
        {
            var value = new Graph().CreateBlankNode().AsDynamic();

            Assert.IsType<DynamicNode>(value);
        }

        [Fact]
        public void m3()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var result = new NodeFactory().CreateLiteralNode(string.Empty).AsDynamic();
            });
        }
    }
}
