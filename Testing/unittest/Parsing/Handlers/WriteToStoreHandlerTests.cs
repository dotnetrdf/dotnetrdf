using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Params;

namespace VDS.RDF.Test.Parsing.Handlers
{
    /// <summary>
    /// Summary description for WriteToStoreHandlerTests
    /// </summary>
    [TestClass]
    public class WriteToStoreHandlerTests
    {
        private readonly Uri TestGraphUri = new Uri("http://example.org/WriteToStoreHandlerTest");

        private void EnsureTestData()
        {
            if (!System.IO.File.Exists("temp.ttl"))
            {
                Graph g = new Graph();
                EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
                g.SaveToFile("temp.ttl");
            }
        }

        private void TestWriteToStoreHandler(IGenericIOManager manager)
        {
            //First ensure that our test file exists
            EnsureTestData();

            //Try to ensure that the target Graph does not exist
            if (manager.DeleteSupported)
            {
                manager.DeleteGraph(TestGraphUri);
            }
            else
            {
                Graph g = new Graph();
                g.BaseUri = TestGraphUri;
                manager.SaveGraph(g);
            }

            Graph temp = new Graph();
            manager.LoadGraph(temp, TestGraphUri);
            Assert.IsTrue(temp.IsEmpty, "Unable to ensure that Target Graph in Store is empty prior to running Test");

            WriteToStoreHandler handler = new WriteToStoreHandler(manager, TestGraphUri, 100);
            TurtleParser parser = new TurtleParser();
            parser.Load(handler, "temp.ttl");

            manager.LoadGraph(temp, TestGraphUri);
            Assert.IsFalse(temp.IsEmpty, "Graph should not be empty");

            Graph orig = new Graph();
            orig.LoadFromFile("temp.ttl");

            Assert.AreEqual(orig, temp, "Graphs should be equal");
        }

        private void TestWriteToStoreDatasetsHandler(IGenericIOManager manager)
        {
            NodeFactory factory = new NodeFactory();
            INode a = factory.CreateUriNode(new Uri("http://example.org/a"));
            INode b = factory.CreateUriNode(new Uri("http://example.org/b"));
            INode c = factory.CreateUriNode(new Uri("http://example.org/c"));
            INode d = factory.CreateUriNode(new Uri("http://example.org/d"));

            Uri graphB = new Uri("http://example.org/graphs/b");
            Uri graphD = new Uri("http://example.org/graphs/d");

            //Try to ensure that the target Graphs do not exist
            if (manager.DeleteSupported)
            {
                manager.DeleteGraph(TestGraphUri);
                manager.DeleteGraph(graphB);
                manager.DeleteGraph(graphD);
            }
            else
            {
                Graph g = new Graph();
                g.BaseUri = TestGraphUri;
                manager.SaveGraph(g);
                g.BaseUri = graphB;
                manager.SaveGraph(g);
                g.BaseUri = graphD;
                manager.SaveGraph(g);
            }

            //Do the parsing and thus the loading
            WriteToStoreHandler handler = new WriteToStoreHandler(manager, TestGraphUri);
            NQuadsParser parser = new NQuadsParser();
            parser.Load(handler, new StreamParams("writetostore.nq"));

            //Load the expected Graphs
            Graph def = new Graph();
            manager.LoadGraph(def, TestGraphUri);
            Graph gB = new Graph();
            manager.LoadGraph(gB, graphB);
            Graph gD = new Graph();
            manager.LoadGraph(gD, graphD);

            Assert.AreEqual(2, def.Triples.Count, "Should be two triples in the default Graph");
            Assert.IsTrue(def.ContainsTriple(new Triple(a, a, a)), "Default Graph should have the a triple");
            Assert.AreEqual(1, gB.Triples.Count, "Should be one triple in the b Graph");
            Assert.IsTrue(gB.ContainsTriple(new Triple(b, b, b)), "b Graph should have the b triple");
            Assert.IsTrue(def.ContainsTriple(new Triple(c, c, c)), "Default Graph should have the c triple");
            Assert.AreEqual(1, gD.Triples.Count, "Should be one triple in the d Graph");
            Assert.IsTrue(gD.ContainsTriple(new Triple(d, d, d)), "d Graph should have the d triple");
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void ParsingWriteToStoreHandlerBadInstantiation()
        {
            WriteToStoreHandler handler = new WriteToStoreHandler(null, null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ParsingWriteToStoreHandlerBadInstantiation2()
        {
            WriteToStoreHandler handler = new WriteToStoreHandler(new ReadOnlyConnector(new InMemoryManager()), null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ParsingWriteToStoreHandlerBadInstantiation3()
        {
            WriteToStoreHandler handler = new WriteToStoreHandler(new JosekiConnector("http://example.org/", "query"), null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void ParsingWriteToStoreHandlerBadInstantiation4()
        {
            WriteToStoreHandler handler = new WriteToStoreHandler(new InMemoryManager(), null, 0);
        }

        [TestMethod]
        public void ParsingWriteToStoreHandlerVirtuoso()
        {
            VirtuosoManager virtuoso = new VirtuosoManager("DB", "dba", "dba");
            this.TestWriteToStoreHandler(virtuoso);
        }

        [TestMethod]
        public void ParsingWriteToStoreHandlerAllegroGraph()
        {
            AllegroGraphConnector agraph = new AllegroGraphConnector("http://localhost:9875", "test", "unittest");
            this.TestWriteToStoreHandler(agraph);
        }

        [TestMethod]
        public void ParsingWriteToStoreHandlerFuseki()
        {
            FusekiConnector fuseki = new FusekiConnector("http://localhost:3030/dataset/data");
            this.TestWriteToStoreHandler(fuseki);
        }

        [TestMethod]
        public void ParsingWriteToStoreHandlerInMemory()
        {
            InMemoryManager mem = new InMemoryManager();
            this.TestWriteToStoreHandler(mem);
        }

        [TestMethod]
        public void ParsingWriteToStoreHandlerSql()
        {
            MicrosoftSqlStoreManager sql = new MicrosoftSqlStoreManager("unit_test", "example", "password");
            this.TestWriteToStoreHandler(sql);
            sql.Dispose();
        }

        [TestMethod]
        public void ParsingWriteToStoreHandlerDatasetsInMemory()
        {
            InMemoryManager manager = new InMemoryManager();
            this.TestWriteToStoreDatasetsHandler(manager);
        }

        [TestMethod]
        public void ParsingWriteToStoreHandlerDatasetsVirtuoso()
        {
            VirtuosoManager virtuoso = new VirtuosoManager("DB", "dba", "dba");
            this.TestWriteToStoreDatasetsHandler(virtuoso);
        }
    }
}
