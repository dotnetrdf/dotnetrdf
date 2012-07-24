using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VDS.RDF.Test.Core
{
    [TestClass]
    public class GraphCollectionTests
    {
        [TestMethod]
        public void GraphCollectionBasic1()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            store.Add(g);

            Assert.IsTrue(store.HasGraph(g.BaseUri), "Graph Collection should contain the Graph");
            Assert.AreEqual(g, store.Graph(g.BaseUri), "Graphs should be equal");
        }

        [TestMethod]
        public void GraphCollectionBasic2()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            store.Add(g);

            Assert.IsTrue(store.HasGraph(g.BaseUri), "Graph Collection should contain the Graph");
            Assert.AreEqual(g, store.Graph(g.BaseUri), "Graphs should be equal");
        }

        [TestMethod]
        public void GraphCollectionDiskDemand1()
        {
            TripleStore store = new TripleStore(new DiskDemandGraphCollection());
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            g.BaseUri = new Uri("file:///" + Path.GetFullPath("InferenceTest.ttl"));

            Assert.IsTrue(store.HasGraph(g.BaseUri), "Graph Collection should contain the Graph");
            Assert.AreEqual(g, store.Graph(g.BaseUri), "Graphs should be equal");
        }

        [TestMethod]
        public void GraphCollectionDiskDemand2()
        {
            //Test that on-demand loading does not kick in for pre-existing graphs
            TripleStore store = new TripleStore(new DiskDemandGraphCollection());

            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            g.BaseUri = new Uri("file:///" + Path.GetFullPath("InferenceTest.ttl"));

            Graph empty = new Graph();
            empty.BaseUri = g.BaseUri;
            store.Add(empty);

            Assert.IsTrue(store.HasGraph(g.BaseUri), "Graph Collection should contain the Graph");
            Assert.AreNotEqual(g, store.Graph(g.BaseUri), "Graphs should not be equal");
        }

        [TestMethod]
        public void GraphCollectionWebDemand1()
        {
            TripleStore store = new TripleStore(new WebDemandGraphCollection());
            Graph g = new Graph();
            Uri u = new Uri("http://www.dotnetrdf.org/configuration#");
            g.LoadFromUri(u);
            g.BaseUri = u; 

            Assert.IsTrue(store.HasGraph(g.BaseUri), "Graph Collection should contain the Graph");
            Assert.AreEqual(g, store.Graph(g.BaseUri), "Graphs should be equal");
        }

        [TestMethod]
        public void GraphCollectionWebDemand2()
        {
            //Test that on-demand loading does not kick in for pre-existing graphs
            TripleStore store = new TripleStore(new WebDemandGraphCollection());

            Graph g = new Graph();
            Uri u = new Uri("http://www.dotnetrdf.org/configuration#");
            g.LoadFromUri(u);
            g.BaseUri = u;

            Graph empty = new Graph();
            empty.BaseUri = g.BaseUri;
            store.Add(empty);

            Assert.IsTrue(store.HasGraph(g.BaseUri), "Graph Collection should contain the Graph");
            Assert.AreNotEqual(g, store.Graph(g.BaseUri), "Graphs should not be equal");
        }
    }
}
