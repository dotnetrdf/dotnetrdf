using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        //private const String FourStoreTestUri = "http://nottm-virtual.ecs.soton.ac.uk:8080/";
        private const String FourStoreTestUri = "http://nottm-virtual:8080";

        private NTriplesFormatter _formatter = new NTriplesFormatter();

        [TestMethod]
        public void StorageFourStoreSaveGraph()
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
        public void StorageFourStoreLoadGraph()
        {
            StorageFourStoreSaveGraph();

            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/4storeTest");

            FourStoreConnector fourstore = new FourStoreConnector(FourStoreTestUri);

            Graph h = new Graph();
            fourstore.LoadGraph(h, "http://example.org/4storeTest");

            Assert.AreEqual(g, h, "Graphs should be equal");
        }

        [TestMethod]
        public void StorageFourStoreDeleteGraph()
        {
            StorageFourStoreSaveGraph();

            FourStoreConnector fourstore = new FourStoreConnector(FourStoreTestUri);
            fourstore.DeleteGraph("http://example.org/4storeTest");

            Graph g = new Graph();
            fourstore.LoadGraph(g, "http://example.org/4storeTest");

            Assert.IsTrue(g.IsEmpty, "Graph should be empty as it was deleted from 4store");
        }

        [TestMethod]
        public void StorageFourStoreAddTriples()
        {
            StorageFourStoreDeleteGraph();
            StorageFourStoreSaveGraph();

            Graph g = new Graph();
            List<Triple> ts = new List<Triple>();
            ts.Add(new Triple(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object"))));

            FourStoreConnector fourstore = new FourStoreConnector(FourStoreTestUri);
            fourstore.UpdateGraph("http://example.org/4storeTest", ts, null);

            fourstore.LoadGraph(g, "http://example.org/4storeTest");

            Assert.IsTrue(ts.All(t => g.ContainsTriple(t)), "Added Triple should not have been in the Graph");
        }

        [TestMethod]
        public void StorageFourStoreRemoveTriples()
        {
            StorageFourStoreAddTriples();

            Graph g = new Graph();
            List<Triple> ts = new List<Triple>();
            ts.Add(new Triple(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object"))));

            FourStoreConnector fourstore = new FourStoreConnector(FourStoreTestUri);
            fourstore.UpdateGraph("http://example.org/4storeTest", null, ts);

            Thread.Sleep(2500);

            fourstore.LoadGraph(g, "http://example.org/4storeTest");

            Assert.IsTrue(ts.All(t => !g.ContainsTriple(t)), "Removed Triple should not have been in the Graph");
        }
    }
}
