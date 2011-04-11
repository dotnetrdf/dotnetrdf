using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;


namespace VDS.RDF.Test.Storage
{
    [TestClass]
    public class AllegroGraphTests
    {
        [TestMethod]
        public void StorageAllegroGraphSaveLoad()
        {
            try
            {
                Graph g = new Graph();
                FileLoader.Load(g, "InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/AllegroGraphTest");

                AllegroGraphConnector agraph = new AllegroGraphConnector("http://localhost:9875", "test", "unit-test");
                agraph.SaveGraph(g);

                Graph h = new Graph();
                agraph.LoadGraph(h, "http://example.org/AllegroGraphTest");
                Assert.IsFalse(h.IsEmpty, "Graph should not be empty after loading");

                Assert.AreEqual(g, h, "Graphs should have been equal");
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
        }

        [TestMethod]
        public void StorageAllegroGraphDeleteTriples()
        {
            try
            {
                Graph g = new Graph();
                FileLoader.Load(g, "InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/AllegroGraphTest");

                AllegroGraphConnector agraph = new AllegroGraphConnector("http://localhost:9875", "test", "unit-test");
                agraph.SaveGraph(g);

                Console.WriteLine("Graph before deletion");
                TestTools.ShowGraph(g);

                //Delete all Triples about the Ford Fiesta
                agraph.UpdateGraph(g.BaseUri, null, g.GetTriplesWithSubject(new Uri("http://example.org/vehicles/FordFiesta")));

                Graph h = new Graph();
                agraph.LoadGraph(h, g.BaseUri);

                Console.WriteLine("Graph after deletion");
                TestTools.ShowGraph(h);

                Assert.IsFalse(h.IsEmpty, "Graph should not be completely empty");
                Assert.IsTrue(g.HasSubGraph(h), "Graph retrieved with missing Triples should be a sub-graph of the original Graph");
                Assert.IsFalse(g.Equals(h), "Graph retrieved should not be equal to original Graph");

                Object results = agraph.Query("ASK WHERE { GRAPH <http://example.org/AllegroGraphTest> { <http://example.org/vehicles/FordFiesta> ?p ?o } }");
                if (results is SparqlResultSet)
                {
                    Assert.IsFalse(((SparqlResultSet)results).Result, "There should no longer be any triples about the Ford Fiesta present");
                }
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
        }

        [TestMethod]
        public void StorageAllegroGraphAsk()
        {
            try
            {
                AllegroGraphConnector agraph = new AllegroGraphConnector("http://localhost:9875", "test", "unit-test");

                String ask = "ASK WHERE { ?s ?p ?o }";

                Object results = agraph.Query(ask);
                if (results is SparqlResultSet)
                {
                    TestTools.ShowResults(results);
                }
                else
                {
                    Assert.Fail("Failed to get a Result Set as expected");
                }
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
        }

        [TestMethod]
        public void StorageAllegroGraphDescribe()
        {
            try
            {
                AllegroGraphConnector agraph = new AllegroGraphConnector("http://localhost:9875", "test", "unit-test");

                String describe = "DESCRIBE <http://example.org/Vehicles/FordFiesta>";

                Object results = agraph.Query(describe);
                if (results is IGraph)
                {
                    TestTools.ShowGraph((IGraph)results);
                }
                else
                {
                    Assert.Fail("Failed to get a Graph as expected");
                }
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
        }
    }
}
