namespace Grom.NET.Tests.Grom
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using VDS.RDF;

    [TestClass]
    public class Class1
    {
        private dynamic dynamicGraph;

        [TestInitialize]
        public void Initialize()
        {
            var graph = new Graph();
            graph.NamespaceMap.AddNamespace("ex", new Uri("http://example.com/"));
            graph.BaseUri = new Uri("http://example.com/");
            graph.LoadFromString("<http://example.com/1> <http://example.com/2> <http://example.com/3> .");

            this.dynamicGraph = new GraphWrapper(graph);

        }

        [TestMethod]
        public void absolute()
        {
            this.NewMethod("http://example.com/1");
        }

        [TestMethod]
        public void qname()
        {
            this.NewMethod("ex:1");
        }

        [TestMethod]
        public void relative()
        {
            this.NewMethod("1");
        }

        [TestMethod]
        public void rootedrelative()
        {
            this.NewMethod("/1");
        }

        private void NewMethod(object index)
        {
            var actual = this.dynamicGraph[index];
            var expected = new NodeWrapper(new NodeFactory().CreateUriNode(new Uri("http://example.com/1")));

            Assert.AreEqual(expected, actual);
        }
    }
}
