using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Storage
{
    [TestClass]
    public class SparqlUniformHttpProtocolTest
    {
        private const String ProtocolTestUri = "http://localhost/demos/server/";
        private NTriplesFormatter _formatter = new NTriplesFormatter();

        [TestMethod]
        public void SparqlUniformHttpProtocolSaveGraph()
        {
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

        [TestMethod]
        public void SparqlUniformHttpProtocolLoadGraph()
        {
            //Ensure that the Graph will be there using the SaveGraph() test
            SparqlUniformHttpProtocolSaveGraph();

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

                Assert.IsTrue(diff.AddedTriples.Count() == 1, "Should only be 1 Triple difference due to New Line normalization");
                Assert.IsTrue(diff.RemovedTriples.Count() == 1, "Should only be 1 Triple difference due to New Line normalization");
                Assert.IsFalse(diff.AddedMSGs.Any(), "Should not be any MSG differences");
                Assert.IsFalse(diff.RemovedMSGs.Any(), "Should not be any MSG differences");
            }
        }

        [TestMethod]
        public void SparqlUniformHttpProtocolDeleteGraph()
        {
            SparqlUniformHttpProtocolSaveGraph();

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
    }
}
