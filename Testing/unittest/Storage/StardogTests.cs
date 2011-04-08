using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Storage
{
    [TestClass]
    public class StardogTests
    {
        private const String StardogTestUri = "http://localhost:2011/";
        private const String StardogTestKB = "test";

        [TestMethod]
        public void StorageStardogLoadDefaultGraph()
        {
            StardogConnector stardog = new StardogConnector(StardogTestUri, StardogTestKB);
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
            StardogConnector stardog = new StardogConnector(StardogTestUri, StardogTestKB);
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

                StardogConnector stardog = new StardogConnector(StardogTestUri, StardogTestKB);
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

                StardogConnector stardog = new StardogConnector(StardogTestUri, StardogTestKB);
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

                StardogConnector stardog = new StardogConnector(StardogTestUri, StardogTestKB);
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

                StardogConnector stardog = new StardogConnector(StardogTestUri, StardogTestKB);
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

        [TestMethod]
        public void StorageStardogDeleteNamedGraph()
        {
            try
            {
                Options.UseBomForUtf8 = false;

                StardogConnector stardog = new StardogConnector(StardogTestUri, StardogTestKB);
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
    }
}
