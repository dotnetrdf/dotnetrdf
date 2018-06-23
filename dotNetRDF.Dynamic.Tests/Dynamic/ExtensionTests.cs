namespace Dynamic
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Linq;
    using VDS.RDF;

    [TestClass]
    public class ExtensionTests
    {
        [TestMethod]
        public void m1()
        {
            var value = new Graph().AsDynamic();

            Assert.IsInstanceOfType(value, typeof(DynamicGraph));
        }

        [TestMethod]
        public void m2()
        {
            var value = new NodeFactory().CreateBlankNode().AsDynamic();

            Assert.IsInstanceOfType(value, typeof(DynamicNode));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void m3()
        {
            var result = new NodeFactory().CreateLiteralNode(string.Empty).AsDynamic();
        }
    }
}
