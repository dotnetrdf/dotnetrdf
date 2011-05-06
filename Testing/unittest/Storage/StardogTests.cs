using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Storage
{
    [TestClass]
    public class StardogTests
    {
        private const String StardogTestUri = "http://localhost:2011/";
        private const String StardogTestKB = "test";
        private const String StardogUser = "anonymous";
        private const String StardogPassword = "anonymous";

        private StardogConnector GetConnection()
        {
            return new StardogConnector(StardogTestUri, StardogTestKB, StardogUser, StardogPassword);
        }

        [TestMethod]
        public void StorageStardogLoadDefaultGraph()
        {
            StardogConnector stardog = this.GetConnection();
            Graph g = new Graph();
            stardog.LoadGraph(g, (Uri)null);

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Assert.IsFalse(g.IsEmpty);
        }

        [TestMethod]
        public void StorageStardogLoadNamedGraph()
        {
            StardogConnector stardog = this.GetConnection();
            Graph g = new Graph();
            stardog.LoadGraph(g, new Uri("http://example.org/graph"));

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Assert.IsFalse(g.IsEmpty);
        }

        [TestMethod]
        public void StorageStardogSaveToDefaultGraph()
        {
            try
            {
                Options.UseBomForUtf8 = false;

                StardogConnector stardog = this.GetConnection();
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = null;
                stardog.SaveGraph(g);

                Graph h = new Graph();
                stardog.LoadGraph(h, (Uri)null);

                if (g.Triples.Count == h.Triples.Count)
                {
                    Assert.AreEqual(g, h, "Retrieved Graph should be equal to the Saved Graph");
                }
                else
                {
                    Assert.IsTrue(h.HasSubGraph(g), "Retrieved Graph should have the Saved Graph as a subgraph");
                }
            }
            finally
            {
                Options.UseBomForUtf8 = true;
            }
        }

        [TestMethod]
        public void StorageStardogSaveToNamedGraph()
        {
            try
            {
                Options.UseBomForUtf8 = false;

                StardogConnector stardog = this.GetConnection();
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/graph");
                stardog.SaveGraph(g);

                Graph h = new Graph();
                stardog.LoadGraph(h, new Uri("http://example.org/graph"));

                if (g.Triples.Count == h.Triples.Count)
                {
                    Assert.AreEqual(g, h, "Retrieved Graph should be equal to the Saved Graph");
                }
                else
                {
                    Assert.IsTrue(h.HasSubGraph(g), "Retrieved Graph should have the Saved Graph as a subgraph");
                }
            }
            finally
            {
                Options.UseBomForUtf8 = true;
            }
        }

        [TestMethod]
        public void StorageStardogUpdateNamedGraphRemoveTriples()
        {
            try
            {
                Options.UseBomForUtf8 = false;

                StardogConnector stardog = this.GetConnection();
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/graph");
                stardog.SaveGraph(g);

                INode rdfType = g.CreateUriNode(new Uri(VDS.RDF.Parsing.RdfSpecsHelper.RdfType));

                stardog.UpdateGraph(g.BaseUri, null, g.GetTriplesWithPredicate(rdfType));
                g.Retract(g.GetTriplesWithPredicate(rdfType));

                Graph h = new Graph();
                stardog.LoadGraph(h, new Uri("http://example.org/graph"));

                if (g.Triples.Count == h.Triples.Count)
                {
                    Assert.AreEqual(g, h, "Retrieved Graph should be equal to the Saved Graph");
                }
                else
                {
                    Assert.IsTrue(h.HasSubGraph(g), "Retrieved Graph should have the Saved Graph as a subgraph");
                }
                Assert.IsFalse(h.GetTriplesWithPredicate(rdfType).Any(), "Retrieved Graph should not contain any rdf:type Triples");
            }
            finally
            {
                Options.UseBomForUtf8 = true;
            }
        }

        [TestMethod]
        public void StorageStardogUpdateNamedGraphAddTriples()
        {
            try
            {
                Options.UseBomForUtf8 = false;

                StardogConnector stardog = this.GetConnection();
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/addGraph");

                INode rdfType = g.CreateUriNode(new Uri(VDS.RDF.Parsing.RdfSpecsHelper.RdfType));
                Graph types = new Graph();
                types.Assert(g.GetTriplesWithPredicate(rdfType));
                g.Retract(g.GetTriplesWithPredicate(rdfType));

                //Save the Graph without the rdf:type triples
                stardog.SaveGraph(g);
                //Then add back in the rdf:type triples
                stardog.UpdateGraph(g.BaseUri, types.Triples, null);

                Graph h = new Graph();
                stardog.LoadGraph(h, new Uri("http://example.org/addGraph"));

                if (g.Triples.Count == h.Triples.Count)
                {
                    Assert.AreEqual(g, h, "Retrieved Graph should be equal to the Saved Graph");
                }
                else
                {
                    Assert.IsTrue(h.HasSubGraph(g), "Retrieved Graph should have the Saved Graph as a subgraph");
                }
                Assert.IsTrue(h.GetTriplesWithPredicate(rdfType).Any(), "Retrieved Graph should not contain any rdf:type Triples");
            }
            finally
            {
                Options.UseBomForUtf8 = true;
            }
        }

        [TestMethod, ExpectedException(typeof(NotSupportedException))]
        //[TestMethod]
        public void StorageStardogDeleteNamedGraph()
        {
            try
            {
                Options.UseBomForUtf8 = false;

                StardogConnector stardog = this.GetConnection();
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/tempGraph");
                stardog.SaveGraph(g);

                Graph h = new Graph();
                stardog.LoadGraph(h, new Uri("http://example.org/tempGraph"));

                if (g.Triples.Count == h.Triples.Count)
                {
                    Assert.AreEqual(g, h, "Retrieved Graph should be equal to the Saved Graph");
                }
                else
                {
                    Assert.IsTrue(h.HasSubGraph(g), "Retrieved Graph should have the Saved Graph as a subgraph");
                }

                stardog.DeleteGraph("http://example.org/tempGraph");
                Graph i = new Graph();
                stardog.LoadGraph(i, new Uri("http://example.org/tempGraph"));

                Assert.IsTrue(i.IsEmpty, "Retrieved Graph should be empty since it has been deleted");
            }
            finally
            {
                Options.UseBomForUtf8 = true;
            }
        }

        [TestMethod]
        public void StorageStardogReasoningQL()
        {
            try
            {
                Options.UseBomForUtf8 = false;

                StardogConnector stardog = this.GetConnection();

                Graph g = new Graph();
                g.LoadFromFile("InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/reasoning");
                stardog.SaveGraph(g);

                String query = "PREFIX rdfs: <" + NamespaceMapper.RDFS + "> SELECT * WHERE { GRAPH <http://example.org/reasoning> { ?class rdfs:subClassOf <http://example.org/vehicles/Vehicle> } }";
                Console.WriteLine(query);
                Console.WriteLine();

                SparqlResultSet resultsNoReasoning = stardog.Query(query) as SparqlResultSet;
                if (resultsNoReasoning != null)
                {
                    Console.WriteLine("Results without Reasoning");
                    TestTools.ShowResults(resultsNoReasoning);
                }
                else
                {
                    Assert.Fail("Did not get a SPARQL Result Set as expected");
                }

                stardog.Reasoning = StardogReasoningMode.QL;
                SparqlResultSet resultsWithReasoning = stardog.Query(query) as SparqlResultSet;
                if (resultsWithReasoning != null)
                {
                    Console.WriteLine("Results with Reasoning");
                    TestTools.ShowResults(resultsWithReasoning);
                }
                else
                {
                    Assert.Fail("Did not get a SPARQL Result Set as expected");
                }

                Assert.IsTrue(resultsWithReasoning.Count >= resultsNoReasoning.Count, "Reasoning should yield as many if not more results");
            }
            finally
            {
                Options.UseBomForUtf8 = true;
            }
        }
    }
}
