using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Storage
{
    [TestClass]
    public class FusekiTest
    {
        private NTriplesFormatter _formatter = new NTriplesFormatter();

        public static FusekiConnector GetConnection()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseFuseki))
            {
                Assert.Inconclusive("Test Configuration marks Fuseki as unavailable, test cannot be run");
            }
            return new FusekiConnector(TestConfigManager.GetSetting(TestConfigManager.FusekiServer));
        }

        [TestMethod]
        public void StorageFusekiSaveGraph()
        {
            try
            {
                Options.UriLoaderCaching = false;

                Graph g = new Graph();
                FileLoader.Load(g, "InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/fusekiTest");

                //Save Graph to Fuseki
                FusekiConnector fuseki = FusekiTest.GetConnection();
                fuseki.SaveGraph(g);
                Console.WriteLine("Graph saved to Fuseki OK");

                //Now retrieve Graph from Fuseki
                Graph h = new Graph();
                fuseki.LoadGraph(h, "http://example.org/fusekiTest");

                Console.WriteLine();
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(this._formatter));
                }

                Assert.AreEqual(g, h, "Graphs should be equal");
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageFusekiSaveDefaultGraph()
        {
            try
            {
                Options.UriLoaderCaching = false;

                Graph g = new Graph();
                FileLoader.Load(g, "InferenceTest.ttl");
                g.BaseUri = null;

                //Save Graph to Fuseki
                FusekiConnector fuseki = FusekiTest.GetConnection();
                fuseki.SaveGraph(g);
                Console.WriteLine("Graph saved to Fuseki OK");

                //Now retrieve Graph from Fuseki
                Graph h = new Graph();
                fuseki.LoadGraph(h, (Uri)null);

                Console.WriteLine();
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(this._formatter));
                }

                Assert.AreEqual(g, h, "Graphs should be equal");
                Assert.IsNull(h.BaseUri, "Retrieved Graph should have a null Base URI");
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageFusekiSaveDefaultGraph2()
        {
            try
            {
                Options.UriLoaderCaching = false;

                Graph g = new Graph();
                FileLoader.Load(g, "InferenceTest.ttl");
                g.BaseUri = null;

                //Save Graph to Fuseki
                FusekiConnector fuseki = FusekiTest.GetConnection();
                fuseki.SaveGraph(g);
                Console.WriteLine("Graph saved to Fuseki OK");

                //Now retrieve Graph from Fuseki
                Graph h = new Graph();
                fuseki.LoadGraph(h, (String)null);

                Console.WriteLine();
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(this._formatter));
                }

                Assert.AreEqual(g, h, "Graphs should be equal");
                Assert.IsNull(h.BaseUri, "Retrieved Graph should have a null Base URI");
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageFusekiLoadGraph()
        {
            try
            {
                Options.UriLoaderCaching = false;

                //Ensure that the Graph will be there using the SaveGraph() test
                StorageFusekiSaveGraph();

                Graph g = new Graph();
                FileLoader.Load(g, "InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/fusekiTest");

                //Try to load the relevant Graph back from the Store
                FusekiConnector fuseki = FusekiTest.GetConnection();

                Graph h = new Graph();
                fuseki.LoadGraph(h, "http://example.org/fusekiTest");

                Console.WriteLine();
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(this._formatter));
                }

                Assert.AreEqual(g, h, "Graphs should be equal");
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageFusekiDeleteGraph()
        {
            try
            {
                Options.UriLoaderCaching = false;

                StorageFusekiSaveGraph();

                FusekiConnector fuseki = FusekiTest.GetConnection();
                fuseki.DeleteGraph("http://example.org/fusekiTest");

                Graph g = new Graph();
                try
                {
                    fuseki.LoadGraph(g, "http://example.org/fusekiTest");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Errored as expected since the Graph was deleted");
                    TestTools.ReportError("Error", ex);
                }
                Console.WriteLine();

                //If we do get here without erroring then the Graph should be empty
                Assert.IsTrue(g.IsEmpty, "Graph should be empty even if an error wasn't thrown as the data should have been deleted from the Store");
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageFusekiDeleteDefaultGraph()
        {
            try
            {
                Options.UriLoaderCaching = false;

                StorageFusekiSaveDefaultGraph();

                FusekiConnector fuseki = FusekiTest.GetConnection();
                fuseki.DeleteGraph((Uri)null);

                Graph g = new Graph();
                try
                {
                    fuseki.LoadGraph(g, (Uri)null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Errored as expected since the Graph was deleted");
                    TestTools.ReportError("Error", ex);
                }
                Console.WriteLine();

                //If we do get here without erroring then the Graph should be empty
                Assert.IsTrue(g.IsEmpty, "Graph should be empty even if an error wasn't thrown as the data should have been deleted from the Store");
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageFusekiDeleteDefaultGraph2()
        {
            try
            {
                Options.UriLoaderCaching = false;

                StorageFusekiSaveDefaultGraph();

                FusekiConnector fuseki = FusekiTest.GetConnection();
                fuseki.DeleteGraph((String)null);

                Graph g = new Graph();
                try
                {
                    fuseki.LoadGraph(g, (Uri)null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Errored as expected since the Graph was deleted");
                    TestTools.ReportError("Error", ex);
                }
                Console.WriteLine();

                //If we do get here without erroring then the Graph should be empty
                Assert.IsTrue(g.IsEmpty, "Graph should be empty even if an error wasn't thrown as the data should have been deleted from the Store");
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageFusekiAddTriples()
        {
            try
            {
                Options.UriLoaderCaching = false;

                StorageFusekiSaveGraph();

                Graph g = new Graph();
                List<Triple> ts = new List<Triple>();
                ts.Add(new Triple(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object"))));

                FusekiConnector fuseki = FusekiTest.GetConnection();
                fuseki.UpdateGraph("http://example.org/fusekiTest", ts, null);

                fuseki.LoadGraph(g, "http://example.org/fusekiTest");
                Assert.IsTrue(ts.All(t => g.ContainsTriple(t)), "Added Triple should have been in the Graph");
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageFusekiRemoveTriples()
        {
            try
            {
                Options.UriLoaderCaching = false;

                StorageFusekiSaveGraph();

                Graph g = new Graph();
                List<Triple> ts = new List<Triple>();
                ts.Add(new Triple(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object"))));

                FusekiConnector fuseki = FusekiTest.GetConnection();
                fuseki.UpdateGraph("http://example.org/fusekiTest", null, ts);

                fuseki.LoadGraph(g, "http://example.org/fusekiTest");
                Assert.IsTrue(ts.All(t => !g.ContainsTriple(t)), "Removed Triple should not have been in the Graph");
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageFusekiQuery()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();

            Object results = fuseki.Query("SELECT * WHERE { {?s ?p ?o} UNION { GRAPH ?g {?s ?p ?o} } }");
            if (results is SparqlResultSet)
            {
                TestTools.ShowResults(results);
            }
            else
            {
                Assert.Fail("Did not get a SPARQL Result Set as expected");
            }
        }

        [TestMethod]
        public void StorageFusekiUpdate()
        {
            try
            {
                Options.HttpDebugging = true;

                FusekiConnector fuseki = FusekiTest.GetConnection();

                //Try doing a SPARQL Update LOAD command
                String command = "LOAD <http://dbpedia.org/resource/Ilkeston> INTO GRAPH <http://example.org/Ilson>";
                fuseki.Update(command);

                //Then see if we can retrieve the newly loaded graph
                IGraph g = new Graph();
                fuseki.LoadGraph(g, "http://example.org/Ilson");
                Assert.IsFalse(g.IsEmpty, "Graph should be non-empty");
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString(this._formatter));
                }
                Console.WriteLine();

                //Try a DROP Graph to see if that works
                command = "DROP GRAPH <http://example.org/Ilson>";
                fuseki.Update(command);

                g = new Graph();
                fuseki.LoadGraph(g, "http://example.org/Ilson");
                Assert.IsTrue(g.IsEmpty, "Graph should be empty as it should have been DROPped by Fuseki");
            }
            finally
            {
                Options.HttpDebugging = false;
            }
            
        }

        [TestMethod]
        public void StorageFusekiDescribe()
        {
            try
            {
                Options.HttpDebugging = true;

                FusekiConnector fuseki = FusekiTest.GetConnection();

                Object results = fuseki.Query("DESCRIBE <http://example.org/vehicles/FordFiesta>");
                if (results is IGraph)
                {
                    TestTools.ShowGraph((IGraph)results);
                }
                else
                {
                    Assert.Fail("Did not return a Graph as expected");
                }
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }
    }
}
