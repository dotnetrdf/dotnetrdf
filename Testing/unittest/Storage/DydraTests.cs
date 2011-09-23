using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace VDS.RDF.Test.Storage
{
    [TestClass]
    public class DydraTests
    {
        private String _apiKey;

        public const String DydraAccount = "rvesse",
                            DydraRepository = "test",
                            DydraApiKeyFile = "dydra-api-key.txt",
                            DydraTestGraphUri = "http://example.org/dydraTest";

        private DydraConnector GetConnection()
        {
            if (this._apiKey == null)
            {
                //Read in API Key if not yet read
                if (File.Exists(DydraApiKeyFile))
                {
                    using (StreamReader reader = new StreamReader(DydraApiKeyFile))
                    {
                        this._apiKey = reader.ReadToEnd();
                        reader.Close();
                    }
                }
                else
                {
                    Assert.Fail("You must specify your Dydra API Key in the " + DydraApiKeyFile + " file found in the resources directory");
                }
            }

            return new DydraConnector(DydraAccount, DydraRepository, this._apiKey);
        }

        [TestMethod]
        public void StorageDydraSaveToDefaultGraph()
        {
            try
            {
                Options.HttpDebugging = true;

                Graph orig = new Graph();
                orig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                orig.BaseUri = null;

                DydraConnector dydra = this.GetConnection();
                dydra.SaveGraph(orig);

                Graph g = new Graph();
                dydra.LoadGraph(g, (Uri)null);

                if (orig.Triples.Count == g.Triples.Count)
                {
                    Assert.AreEqual(orig, g, "Graphs should be equal");
                }
                else
                {
                    Assert.IsTrue(g.HasSubGraph(orig), "Original Graph should be a sub-graph of retrieved Graph");
                }
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
            finally
            {
                Options.HttpFullDebugging = false;
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void StorageDydraSaveToNamedGraph()
        {
            try
            {
                Options.HttpDebugging = true;

                Graph orig = new Graph();
                orig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                orig.BaseUri = new Uri(DydraTestGraphUri);

                DydraConnector dydra = this.GetConnection();
                dydra.SaveGraph(orig);

                Graph g = new Graph();
                dydra.LoadGraph(g, orig.BaseUri);

                if (orig.Triples.Count == g.Triples.Count)
                {
                    GraphDiffReport report = orig.Difference(g);
                    if (!report.AreEqual)
                    {
                        TestTools.ShowDifferences(report);
                    }
                    Assert.AreEqual(orig, g, "Graphs should be equal");
                }
                else
                {
                    Assert.IsTrue(g.HasSubGraph(orig), "Original Graph should be a sub-graph of retrieved Graph");
                }
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
            finally
            {
                Options.HttpFullDebugging = false;
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void StorageDydraLoadDefaultGraph()
        {
            try
            {
                Options.HttpDebugging = true;

                DydraConnector dydra = this.GetConnection();
                Graph g = new Graph();
                dydra.LoadGraph(g, String.Empty);

                TestTools.ShowGraph(g);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void StorageDydraLoadNamedGraph()
        {
            try
            {
                Options.HttpDebugging = true;

                DydraConnector dydra = this.GetConnection();
                Graph g = new Graph();
                dydra.LoadGraph(g, new Uri(DydraTestGraphUri));

                TestTools.ShowGraph(g);

                Assert.IsTrue(g.Triples.Count > 0, "Should be 1 or more triples returned");
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void StorageDydraLoadGraphWithHandler()
        {
            try
            {
                Options.HttpDebugging = true;

                Graph orig = new Graph();
                orig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                orig.BaseUri = new Uri("http://example.org/dydra/tests/counting");

                DydraConnector dydra = this.GetConnection();
                dydra.SaveGraph(orig);

                CountHandler handler = new CountHandler();
                dydra.LoadGraph(handler, orig.BaseUri);

                Assert.AreEqual(orig.Triples.Count, handler.Count, "Triple Counts should be equal");
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        [TestMethod/*,ExpectedException(typeof(NotSupportedException))*/]
        public void StorageDydraDeleteGraph()
        {
            try
            {
                Options.HttpDebugging = true;

                Graph orig = new Graph();
                orig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                orig.BaseUri = new Uri("http://example.org/dydra/deleteTest");

                DydraConnector dydra = this.GetConnection();
                dydra.SaveGraph(orig);

                Graph g = new Graph();
                dydra.LoadGraph(g, orig.BaseUri);

                if (orig.Triples.Count == g.Triples.Count)
                {
                    GraphDiffReport report = orig.Difference(g);
                    if (!report.AreEqual)
                    {
                        TestTools.ShowDifferences(report);
                    }
                    Assert.AreEqual(orig, g, "Graphs should be equal");
                }
                else
                {
                    Assert.IsTrue(g.HasSubGraph(orig), "Original Graph should be a sub-graph of retrieved Graph");
                }

                //Now delete the Graph
                dydra.DeleteGraph(orig.BaseUri);

                //And retrieve it again
                g = new Graph();
                dydra.LoadGraph(g, orig.BaseUri);

                Assert.IsTrue(g.IsEmpty, "Graph should be empty as was deleted from repository");
                Assert.AreNotEqual(orig, g, "Graphs should not be equal");
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void StorageDydraListGraphs()
        {
            DydraConnector dydra = this.GetConnection();
            List<Uri> graphUris = dydra.ListGraphs().ToList();

            Console.WriteLine("Dydra returned " + graphUris.Count + " Graph URIs");
            foreach (Uri u in graphUris)
            {
                Console.WriteLine(u.ToString());
            }
        }

        [TestMethod]
        public void StorageDydraQuery()
        {
            try
            {
                Options.HttpDebugging = true;
                //Options.HttpFullDebugging = true;

                DydraConnector dydra = this.GetConnection();
                Object results = dydra.Query("SELECT * WHERE { ?s a ?type }");
                if (results is SparqlResultSet)
                {
                    SparqlResultSet rset = (SparqlResultSet)results;
                    TestTools.ShowResults(rset);

                    Assert.IsFalse(rset.IsEmpty, "Results should not be empty");
                }
                else
                {
                    Assert.Fail("Did not get a SPARQL Result Set as expected");
                }
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
            finally
            {
                Options.HttpFullDebugging = false;
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void StorageDydraConstructQuery()
        {
            try
            {
                Options.HttpDebugging = true;
                //Options.HttpFullDebugging = true;

                DydraConnector dydra = this.GetConnection();
                Object results = dydra.Query("CONSTRUCT { ?s a ?type } WHERE { ?s a ?type }");
                if (results is IGraph)
                {
                    IGraph g = (IGraph)results;
                    TestTools.ShowResults(g);

                    Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
                }
                else
                {
                    Assert.Fail("Did not get a Graph as expected");
                }
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
            finally
            {
                Options.HttpFullDebugging = false;
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void StorageDydraAddTriples()
        {
            try
            {
                Options.HttpDebugging = true;

                DydraConnector dydra = this.GetConnection();

                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/dydra/addRemoveTest");
                dydra.SaveGraph(g);

                List<Triple> ts = new List<Triple>();
                ts.Add(new Triple(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object"))));

                dydra.UpdateGraph("http://example.org/dydra/addRemoveTest", ts, null);

                g = new Graph();
                dydra.LoadGraph(g, "http://example.org/dydra/addRemoveTest");

                Assert.IsTrue(ts.All(t => g.ContainsTriple(t)), "Added Triple should be in the Graph");
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void StorageFourStoreRemoveTriples()
        {
            try
            {
                Options.HttpDebugging = true;

                DydraConnector dydra = this.GetConnection();

                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/dydra/addRemoveTest");
                dydra.SaveGraph(g);

                List<Triple> ts = new List<Triple>();
                ts.Add(new Triple(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object"))));

                dydra.UpdateGraph("http://example.org/dydra/addRemoveTest", ts, null);

                g = new Graph();
                dydra.LoadGraph(g, "http://example.org/dydra/addRemoveTest");

                Assert.IsTrue(ts.All(t => g.ContainsTriple(t)), "Added Triple should be in the Graph");
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }
    }
}

