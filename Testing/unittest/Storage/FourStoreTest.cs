using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Storage
{
    /// <summary>
    /// Summary description for FourStoreTest
    /// </summary>
    [TestClass]
    public class FourStoreTest
    {
        private const String FourStoreTestUri = "http://nottm-virtual:8080/";

        private NTriplesFormatter _formatter = new NTriplesFormatter();

        [TestMethod]
        public void FourStoreSaveGraph()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/4storeTest");

            FourStoreConnector fourstore = new FourStoreConnector(FourStoreTestUri);
            fourstore.SaveGraph(g);

            Graph h = new Graph();
            fourstore.LoadGraph(h, "http://example.org/4storeTest");

            Assert.AreEqual(g, h, "Graphs should be equal");
        }

        [TestMethod]
        public void FourStoreLoadGraph()
        {
            FourStoreSaveGraph();

            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/4storeTest");

            FourStoreConnector fourstore = new FourStoreConnector(FourStoreTestUri);

            Graph h = new Graph();
            fourstore.LoadGraph(h, "http://example.org/4storeTest");

            Assert.AreEqual(g, h, "Graphs should be equal");
        }

        [TestMethod]
        public void FourStoreDeleteGraph()
        {
            FourStoreSaveGraph();

            FourStoreConnector fourstore = new FourStoreConnector(FourStoreTestUri);
            fourstore.DeleteGraph("http://example.org/4storeTest");

            Graph g = new Graph();
            fourstore.LoadGraph(g, "http://example.org/4storeTest");

            Assert.IsTrue(g.IsEmpty, "Graph should be empty as it was deleted from 4store");
        }

        [TestMethod]
        public void FourStoreAddTriples()
        {
            FourStoreDeleteGraph();
            FourStoreSaveGraph();

            Graph g = new Graph();
            FileLoader.Load(g, "Turtle.ttl");

            FourStoreConnector fourstore = new FourStoreConnector(FourStoreTestUri);
            fourstore.UpdateGraph("http://example.org/4storeTest", g.Triples, null);

            Graph h = new Graph();
            fourstore.LoadGraph(h, "http://example.org/4storeTest");

            Assert.IsTrue(g.IsSubGraphOf(h), "Should be a sub-graph of the retrieved Graph");
        }

        [TestMethod]
        public void FourStoreRemoveTriples()
        {
            FourStoreAddTriples();

            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            FourStoreConnector fourstore = new FourStoreConnector(FourStoreTestUri);
            fourstore.UpdateGraph("http://example.org/4storeTest", null, g.Triples);

            Graph h = new Graph();
            fourstore.LoadGraph(h, "http://example.org/4storeTest");

            Assert.IsFalse(g.IsSubGraphOf(h), "Subgraph should not be present as Triples were removed");
        }
    }
}
