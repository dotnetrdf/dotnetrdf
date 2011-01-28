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
    [TestClass]
    public class FusekiTest
    {
        private NTriplesFormatter _formatter = new NTriplesFormatter();

        private const String FusekiTestUri = "http://localhost:3030/dataset/data";

        [TestMethod]
        public void FusekiSaveGraph()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "Turtle.ttl");
            g.BaseUri = new Uri("http://example.org/fusekiTest");

            //Save Graph to Fuseki
            FusekiConnector fuseki = new FusekiConnector(FusekiTestUri);
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
                Assert.IsFalse(diff.AddedMSGs.Any(), "Should not be any MSG differences (MSGs added)");
                Assert.IsFalse(diff.RemovedMSGs.Any(), "Should not be any MSG differences (MSGs removed)");
            }
        }

        [TestMethod]
        public void FusekiLoadGraph()
        {
            //Ensure that the Graph will be there using the SaveGraph() test
            FusekiSaveGraph();

            Graph g = new Graph();
            FileLoader.Load(g, "Turtle.ttl");
            g.BaseUri = new Uri("http://example.org/fusekiTest");

            //Try to load the relevant Graph back from the Store
            FusekiConnector fuseki = new FusekiConnector(FusekiTestUri);

            Graph h = new Graph();
            fuseki.LoadGraph(h, "http://example.org/fusekiTest");

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
        public void FusekiDeleteGraph()
        {
            FusekiSaveGraph();

            FusekiConnector fuseki = new FusekiConnector(new Uri(FusekiTestUri));

            //Ensure that there is a 2nd Graph in the Store in case Fuseki is treating the 1st Graph as the default Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/fusekiTest2");
            fuseki.SaveGraph(g);

            fuseki.DeleteGraph("http://example.org/fusekiTest");

            Graph h = new Graph();
            try
            {
                fuseki.LoadGraph(h, "http://example.org/fusekiTest");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errored as expected since the Graph was deleted");
                TestTools.ReportError("Error", ex, false);
            }
            Console.WriteLine();

            //If we do get here without erroring then the Graph should be empty
            Assert.IsTrue(h.IsEmpty, "Graph should be empty even if an error wasn't thrown as the data should have been deleted from the Store");
        }

        [TestMethod]
        public void FusekiAddTriples()
        {
            Assert.Inconclusive("Test Not Implemented");
        }

        [TestMethod]
        public void FusekiRemoveTriples()
        {
            Assert.Inconclusive("Test Not Implemented");
        }

        [TestMethod]
        public void FusekiQuery()
        {
            Assert.Inconclusive("Test Not Implemented");
        }

        [TestMethod]
        public void FusekiUpdate()
        {
            FusekiConnector fuseki = new FusekiConnector(new Uri(FusekiTestUri));
            
            //Try doing a SPARQL Update LOAD command
            String command = "LOAD <http://dbpedia.org/resource/Ilkeston> INTO GRAPH <http://example.org/Ilson>";
            fuseki.Update(command);

            //Then see if we can retrieve the newly loaded graph
            Graph g = new Graph();
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
            g.Clear();
            fuseki.LoadGraph(g, "http://example.org/Ilson");
            Assert.IsTrue(g.IsEmpty, "Graph should be empty as it should have been DROPped by Fuseki");
            
        }
    }
}
