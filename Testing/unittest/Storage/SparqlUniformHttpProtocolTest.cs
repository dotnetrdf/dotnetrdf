using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Storage
{
    [TestClass]
    public class SparqlUniformHttpProtocolTest
    {
        private const String ProtocolTestUri = "http://localhost/demos/server/";
        private NTriplesFormatter _formatter = new NTriplesFormatter();

        [TestMethod]
        public void StorageSparqlUniformHttpProtocolSaveGraph()
        {
            try
            {
                Options.UriLoaderCaching = false;

                Graph g = new Graph();
                FileLoader.Load(g, "Turtle.ttl");
                g.BaseUri = new Uri("http://example.org/sparqlTest");

                //Save Graph to SPARQL Uniform Protocol
                SparqlHttpProtocolConnector sparql = new SparqlHttpProtocolConnector(new Uri(ProtocolTestUri));
                sparql.SaveGraph(g);
                Console.WriteLine("Graph saved to SPARQL Uniform Protocol OK");

                //Now retrieve Graph from SPARQL Uniform Protocol
                Graph h = new Graph();
                sparql.LoadGraph(h, "http://example.org/sparqlTest");

                Console.WriteLine();
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(this._formatter));
                }

                GraphDiffReport diff = g.Difference(h);
                if (!diff.AreEqual)
                {
                    Console.WriteLine();
                    Console.WriteLine("Graphs are different - should be 1 difference due to New Line Normalization");
                    Console.WriteLine("Added Triples");
                    foreach (Triple t in diff.AddedTriples)
                    {
                        Console.WriteLine(t.ToString(this._formatter));
                    }
                    Console.WriteLine("Removed Triples");
                    foreach (Triple t in diff.RemovedTriples)
                    {
                        Console.WriteLine(t.ToString(this._formatter));
                    }

                    Assert.IsTrue(diff.AddedTriples.Count() == 1, "Should only be 1 Triple difference due to New Line normalization");
                    Assert.IsTrue(diff.RemovedTriples.Count() == 1, "Should only be 1 Triple difference due to New Line normalization");
                    Assert.IsFalse(diff.AddedMSGs.Any(), "Should not be any MSG differences");
                    Assert.IsFalse(diff.RemovedMSGs.Any(), "Should not be any MSG differences");
                }
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageSparqlUniformHttpProtocolLoadGraph()
        {
            try
            {
                Options.UriLoaderCaching = false;
                //Ensure that the Graph will be there using the SaveGraph() test
                StorageSparqlUniformHttpProtocolSaveGraph();

                Graph g = new Graph();
                FileLoader.Load(g, "Turtle.ttl");
                g.BaseUri = new Uri("http://example.org/sparqlTest");

                //Try to load the relevant Graph back from the Store
                SparqlHttpProtocolConnector sparql = new SparqlHttpProtocolConnector(new Uri(ProtocolTestUri));

                Graph h = new Graph();
                sparql.LoadGraph(h, "http://example.org/sparqlTest");

                Console.WriteLine();
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(this._formatter));
                }

                GraphDiffReport diff = g.Difference(h);
                if (!diff.AreEqual)
                {
                    Console.WriteLine();
                    Console.WriteLine("Graphs are different - should be 1 difference due to New Line Normalization");
                    Console.WriteLine("Added Triples");
                    foreach (Triple t in diff.AddedTriples)
                    {
                        Console.WriteLine(t.ToString(this._formatter));
                    }
                    Console.WriteLine("Removed Triples");
                    foreach (Triple t in diff.RemovedTriples)
                    {
                        Console.WriteLine(t.ToString(this._formatter));
                    }

                    Assert.IsTrue(diff.AddedTriples.Count() == 1, "Should only be 1 Triple difference due to New Line normalization (added)");
                    Assert.IsTrue(diff.RemovedTriples.Count() == 1, "Should only be 1 Triple difference due to New Line normalization (removed)");
                    Assert.IsFalse(diff.AddedMSGs.Any(), "Should not be any MSG differences");
                    Assert.IsFalse(diff.RemovedMSGs.Any(), "Should not be any MSG differences");
                }
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageSparqlUniformHttpProtocolGraphExists()
        {
            try
            {
                Options.UriLoaderCaching = false;
                //Ensure that the Graph will be there using the SaveGraph() test
                StorageSparqlUniformHttpProtocolSaveGraph();

                //Check the Graph exists in the Store
                SparqlHttpProtocolConnector sparql = new SparqlHttpProtocolConnector(new Uri(ProtocolTestUri));
                Assert.IsTrue(sparql.GraphExists("http://example.org/sparqlTest"));
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageSparqlUniformHttpProtocolDeleteGraph()
        {
            try
            {
                Options.UriLoaderCaching = false;
                StorageSparqlUniformHttpProtocolSaveGraph();

                SparqlHttpProtocolConnector sparql = new SparqlHttpProtocolConnector(new Uri(ProtocolTestUri));
                sparql.DeleteGraph("http://example.org/sparqlTest");

                //Give SPARQL Uniform Protocol time to delete stuff
                Thread.Sleep(1000);

                try
                {
                    Graph g = new Graph();
                    sparql.LoadGraph(g, "http://example.org/sparqlTest");

                    //If we do get here without erroring then the Graph should be empty
                    Assert.IsTrue(g.IsEmpty, "If the Graph loaded without error then it should have been empty as we deleted it from the store");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Errored as expected since the Graph was deleted");
                    TestTools.ReportError("Error", ex, false);
                }
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageSparqlUniformHttpProtocolAddTriples()
        {
            try
            {
                Options.UriLoaderCaching = false;

                StorageSparqlUniformHttpProtocolSaveGraph();

                Graph g = new Graph();
                g.Retract(g.Triples.Where(t => !t.IsGroundTriple));
                FileLoader.Load(g, "InferenceTest.ttl");

                SparqlHttpProtocolConnector sparql = new SparqlHttpProtocolConnector(new Uri(ProtocolTestUri));
                sparql.UpdateGraph("http://example.org/sparqlTest", g.Triples, null);

                Graph h = new Graph();
                sparql.LoadGraph(h, "http://example.org/sparqlTest");

                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(this._formatter));
                }

                Assert.IsTrue(g.IsSubGraphOf(h), "Retrieved Graph should have the added Triples as a Sub Graph");
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageSparqlUniformHttpProtocolRemoveTriples()
        {
            try
            {
                Options.UriLoaderCaching = false;
                Graph g = new Graph();
                FileLoader.Load(g, "InferenceTest.ttl");

                try
                {
                    SparqlHttpProtocolConnector sparql = new SparqlHttpProtocolConnector(new Uri(ProtocolTestUri));
                    sparql.UpdateGraph("http://example.org/sparqlTest", null, g.Triples);

                    Assert.Fail("SPARQL Uniform HTTP Protocol does not support removing Triples");
                }
                catch (RdfStorageException storeEx)
                {
                    Console.WriteLine("Got an error as expected");
                    TestTools.ReportError("Storage Error", storeEx, false);
                }
                catch (NotSupportedException ex)
                {
                    Console.WriteLine("Got a Not Supported error as expected");
                    TestTools.ReportError("Not Supported", ex, false);
                }
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void StorageSparqlUniformHttpProtocolPostCreate()
        {
            SparqlHttpProtocolConnector connector = new SparqlHttpProtocolConnector("http://localhost/demos/server/");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost/demos/server/");
            request.Method = "POST";
            request.ContentType = "application/rdf+xml";

            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                RdfXmlWriter rdfxmlwriter = new RdfXmlWriter();
                rdfxmlwriter.Save(g, writer);
                writer.Close();
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                //Should get a 201 Created response
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    if (response.Headers["Location"] == null) Assert.Fail("A Location: Header containing the URI of the newly created Graph should have been returned");
                    Uri graphUri = new Uri(response.Headers["Location"]);

                    Console.WriteLine("New Graph URI is " + graphUri.ToString());

                    Console.WriteLine("Now attempting to retrieve this Graph from the Store");
                    Graph h = new Graph();
                    connector.LoadGraph(h, graphUri);

                    TestTools.ShowGraph(h);

                    Assert.AreEqual(g, h, "Graphs should have been equal");
                }
                else
                {
                    Assert.Fail("A 201 Created response should have been received but got a " + (int)response.StatusCode + " response");
                }
                response.Close();
            }
        }

        [TestMethod]
        public void StorageSparqlUniformHttpProtocolPostCreateMultiple()
        {
            SparqlHttpProtocolConnector connector = new SparqlHttpProtocolConnector("http://localhost/demos/server/");

            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            List<Uri> uris = new List<Uri>();
            for (int i = 0; i < 10; i++)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost/demos/server/");
                request.Method = "POST";
                request.ContentType = "application/rdf+xml";

                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                {
                    RdfXmlWriter rdfxmlwriter = new RdfXmlWriter();
                    rdfxmlwriter.Save(g, writer);
                    writer.Close();
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    //Should get a 201 Created response
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        if (response.Headers["Location"] == null) Assert.Fail("A Location: Header containing the URI of the newly created Graph should have been returned");
                        Uri graphUri = new Uri(response.Headers["Location"]);
                        uris.Add(graphUri);

                        Console.WriteLine("New Graph URI is " + graphUri.ToString());

                        Console.WriteLine("Now attempting to retrieve this Graph from the Store");
                        Graph h = new Graph();
                        connector.LoadGraph(h, graphUri);

                        Assert.AreEqual(g, h, "Graphs should have been equal");
                        Console.WriteLine("Graphs were equal as expected");
                    }
                    else
                    {
                        Assert.Fail("A 201 Created response should have been received but got a " + (int)response.StatusCode + " response");
                    }
                    response.Close();
                }
                Console.WriteLine();
            }

            Assert.IsTrue(uris.Distinct().Count() == 10, "Should have generated 10 distinct URIs");
        }
    }
}
