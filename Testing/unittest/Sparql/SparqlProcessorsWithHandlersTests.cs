using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class SparqlProcessorsWithHandlersTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();

        #region Test Runner Methods

        private void TestCountHandler(ISparqlQueryProcessor processor, String query)
        {
            SparqlQuery q = this._parser.ParseFromString(query);

            Graph expected = processor.ProcessQuery(q) as Graph;
            if (expected == null) Assert.Fail("Query failed to return a Graph as expected");

            CountHandler handler = new CountHandler();
            processor.ProcessQuery(handler, null, q);

            Assert.AreEqual(expected.Triples.Count, handler.Count, "Counts should have been equal");
        }

        private void TestGraphHandler(ISparqlQueryProcessor processor, String query)
        {
            SparqlQuery q = this._parser.ParseFromString(query);

            Graph expected = processor.ProcessQuery(q) as Graph;
            if (expected == null) Assert.Fail("Query failed to return a Grah as expected");

            Graph actual = new Graph();
            GraphHandler handler = new GraphHandler(actual);
            processor.ProcessQuery(handler, null, q);

            Assert.AreEqual(expected, actual, "Graphs should be equal");
        }

        private void TestPagingHandler(ISparqlQueryProcessor processor, String query, int limit, int offset)
        {
            throw new NotImplementedException();
        }

        private void TestResultCountHandler(ISparqlQueryProcessor processor, String query)
        {
            SparqlQuery q = this._parser.ParseFromString(query);

            SparqlResultSet expected = processor.ProcessQuery(q) as SparqlResultSet;
            if (expected == null) Assert.Fail("Query failed to return a Result Set as expected");

            ResultCountHandler handler = new ResultCountHandler();
            processor.ProcessQuery(null, handler, q);

            Assert.AreEqual(expected.Count, handler.Count, "Counts should have been equal");
        }

        private void TestResultSetHandler(ISparqlQueryProcessor processor, String query)
        {
            SparqlQuery q = this._parser.ParseFromString(query);

            SparqlResultSet expected = processor.ProcessQuery(q) as SparqlResultSet;
            if (expected == null) Assert.Fail("Query failed to return a Result Set as expected");

            SparqlResultSet actual = new SparqlResultSet();
            ResultSetHandler handler = new ResultSetHandler(actual);
            processor.ProcessQuery(null, handler, q);

            Assert.AreEqual(expected, actual, "Result Sets should be equal");
        }

        private void TestWriteThroughHandler(ISparqlQueryProcessor processor, String query)
        {
            NTriplesFormatter formatter = new NTriplesFormatter();
            StringWriter data = new StringWriter();

            SparqlQuery q = this._parser.ParseFromString(query);
            Graph expected = processor.ProcessQuery(q) as Graph;
            if (expected == null) Assert.Fail("Query did not produce a Graph as expected");

            WriteThroughHandler handler = new WriteThroughHandler(formatter, data, false);
            processor.ProcessQuery(handler, null, q);
            Console.WriteLine(data.ToString());

            Graph actual = new Graph();
            StringParser.Parse(actual, data.ToString(), new NTriplesParser());

            Assert.AreEqual(expected, actual, "Graphs should be equal");
        }

        #endregion

        #region Test Batch Methods

        private void TestCountHandlers(ISparqlQueryProcessor processor)
        {
            this.TestResultCountHandler(processor, "SELECT * WHERE { ?s a ?type }");
            this.TestResultCountHandler(processor, "PREFIX rdfs: <" + NamespaceMapper.RDFS + "> SELECT * WHERE { ?child rdfs:subClassOf ?parent }");
            this.TestCountHandler(processor, "CONSTRUCT { ?s a ?type } WHERE { ?s a ?type }");
            this.TestCountHandler(processor, "PREFIX rdfs: <" + NamespaceMapper.RDFS + "> CONSTRUCT WHERE { ?child rdfs:subClassOf ?parent }");
        }

        private void TestGraphHandlers(ISparqlQueryProcessor processor)
        {
            this.TestGraphHandler(processor, "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }");
            this.TestGraphHandler(processor, "PREFIX rdfs: <" + NamespaceMapper.RDFS + "> CONSTRUCT WHERE { ?s a ?type ; rdfs:subClassOf ?parent }");
        }

        private void TestPagingHandlers(ISparqlQueryProcessor processor)
        {

        }

        private void TestWriteThroughHandlers(ISparqlQueryProcessor processor)
        {
            this.TestWriteThroughHandler(processor, "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }");
            this.TestWriteThroughHandler(processor, "PREFIX rdfs: <" + NamespaceMapper.RDFS + "> CONSTRUCT WHERE { ?s a ?type ; rdfs:subClassOf ?parent }");
        }

        #endregion

        #region Leviathan Tests

        private ISparqlDataset _dataset;
        private ISparqlQueryProcessor _leviathan;
        private ISparqlQueryProcessor _explainer;

        private void EnsureLeviathanReady()
        {
            if (this._dataset == null)
            {
                TripleStore store = new TripleStore();
                Graph g = new Graph();
                g.LoadFromFile("InferenceTest.ttl");
                store.Add(g);

                this._dataset = new InMemoryDataset(store);
            }
            if (this._leviathan == null)
            {
                this._leviathan = new LeviathanQueryProcessor(this._dataset);
            }
            if (this._explainer == null)
            {
                this._explainer = new ExplainQueryProcessor(this._dataset, ExplanationLevel.Default);
            }
        }

        [TestMethod]
        public void SparqlWithHandlersLeviathanCount()
        {
            this.EnsureLeviathanReady();
            this.TestCountHandlers(this._leviathan);
        }

        [TestMethod]
        public void SparqlWithHandlersLeviathanExplainCount()
        {
            this.EnsureLeviathanReady();
            this.TestCountHandlers(this._explainer);
        }

        [TestMethod]
        public void SparqlWithHandlersLeviathanGraph()
        {
            this.EnsureLeviathanReady();
            this.TestGraphHandlers(this._leviathan);
        }

        [TestMethod]
        public void SparqlWithHandlersLeviathanExplainGraph()
        {
            this.EnsureLeviathanReady();
            this.TestGraphHandlers(this._explainer);
        }

        [TestMethod]
        public void SparqlWithHandlersLeviathanWriteThrough()
        {
            this.EnsureLeviathanReady();
            this.TestWriteThroughHandlers(this._leviathan);
        }

        [TestMethod]
        public void SparqlWithHandlersLeviathanExplainWriteThrough()
        {
            this.EnsureLeviathanReady();
            this.TestWriteThroughHandlers(this._explainer);
        }

        #endregion

        #region Remote Tests

        private ISparqlQueryProcessor _remote;

        private void EnsureRemoteReady()
        {
            if (this._remote == null)
            {
                this._remote = new RemoteQueryProcessor(new SparqlRemoteEndpoint(new Uri("http://localhost/demos/leviathan/")));
            }
        }

        [TestMethod]
        public void SparqlWithHandlersRemoteCount()
        {
            this.EnsureRemoteReady();
            this.TestCountHandlers(this._remote);
        }

        [TestMethod]
        public void SparqlWithHandlersRemoteGraph()
        {
            this.EnsureRemoteReady();
            this.TestGraphHandlers(this._remote);
        }

        [TestMethod]
        public void SparqlWithHandlersRemoteWriteThrough()
        {
            this.EnsureRemoteReady();
            this.TestWriteThroughHandlers(this._remote);
        }

        #endregion

        #region Pellet Tests

        private ISparqlQueryProcessor _pellet;

        private void EnsurePelletReady()
        {
            if (this._pellet == null)
            {
                this._pellet = new PelletQueryProcessor(new Uri(PelletTests.PelletTestServer), "wine");
            }
        }

        [TestMethod]
        public void SparqlWithHandlersPelletCount()
        {
            this.EnsurePelletReady();
            this.TestCountHandlers(this._pellet);
        }

        //[TestMethod]
        //public void SparqlWithHandlersPelletGraph()
        //{
        //    this.EnsurePelletReady();
        //    this.TestGraphHandlers(this._pellet);
        //}

        //[TestMethod]
        //public void SparqlWithHandlersPelletWriteThrough()
        //{
        //    this.EnsurePelletReady();
        //    this.TestWriteThroughHandlers(this._pellet);
        //}

        #endregion

        #region Generic Tests

        private ISparqlQueryProcessor _generic;

        private void EnsureGenericReady()
        {
            this.EnsureLeviathanReady();
            if (this._generic == null)
            {
                this._generic = new GenericQueryProcessor(new InMemoryManager(this._dataset));
            }
        }

        [TestMethod]
        public void SparqlWithHandlersGenericCount()
        {
            this.EnsureGenericReady();
            this.TestCountHandlers(this._generic);
        }

        [TestMethod]
        public void SparqlWithHandlersGenericGraph()
        {
            this.EnsureGenericReady();
            this.TestGraphHandlers(this._generic);
        }

        [TestMethod]
        public void SparqlWithHandlersGenericWriteThrough()
        {
            this.EnsureGenericReady();
            this.TestWriteThroughHandlers(this._generic);
        }

        #endregion
    }
}
