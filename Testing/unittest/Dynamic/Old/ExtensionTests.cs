namespace VDS.RDF.Dynamic.Old
{
    using System;
    using Xunit;

    public class ExtensionTests
    {
        public void m1()
        {
            var value = new Graph().AsDynamic();

            Assert.IsType<DynamicGraph>(value);
        }

        public void m2()
        {
            var value = new Graph().CreateBlankNode().AsDynamic();

            Assert.IsType<DynamicNode>(value);
        }

        public void m3()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var result = new NodeFactory().CreateLiteralNode(string.Empty).AsDynamic();
            });
        }
    }
}
