using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test
{
    [TestClass]
    public class GraphDiffTests
    {
        [TestMethod]
        public void DiffEqualGraphs()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            h = g;

            GraphDiffReport report = g.Difference(h);
            ShowDifferences(report);

            Assert.IsTrue(report.AreEqual, "Graphs should be equal");
        }

        [TestMethod]
        public void DiffDifferentGraphs()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            FileLoader.Load(h, "Turtle.ttl");

            GraphDiffReport report = g.Difference(h);
            ShowDifferences(report);

            Assert.IsFalse(report.AreEqual, "Graphs should not be equal");
        }

        [TestMethod]
        public void DiffEqualGraphs2()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            FileLoader.Load(h, "InferenceTest.ttl");

            GraphDiffReport report = g.Difference(h);
            ShowDifferences(report);

            Assert.IsTrue(report.AreEqual, "Graphs should be equal");
        }

        [TestMethod]
        public void DiffRemovedGroundTriples()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            FileLoader.Load(h, "InferenceTest.ttl");

            //Remove Triples about Ford Fiestas from 2nd Graph
            h.Retract(h.GetTriplesWithSubject(new Uri("http://example.org/vehicles/FordFiesta")));

            GraphDiffReport report = g.Difference(h);
            ShowDifferences(report);

            Assert.IsFalse(report.AreEqual, "Graphs should not have been reported as equal");
            Assert.IsTrue(report.RemovedTriples.Any(), "Difference should have reported some Removed Triples");
        }

        [TestMethod]
        public void DiffAddedGroundTriples()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            FileLoader.Load(h, "InferenceTest.ttl");

            //Add additional Triple to 2nd Graph
            UriNode spaceVehicle = h.CreateUriNode("eg:SpaceVehicle");
            UriNode subClass = h.CreateUriNode("rdfs:subClassOf");
            UriNode vehicle = h.CreateUriNode("eg:Vehicle");
            h.Assert(new Triple(spaceVehicle, subClass, vehicle));

            GraphDiffReport report = g.Difference(h);
            ShowDifferences(report);

            Assert.IsFalse(report.AreEqual, "Graphs should not have been reported as equal");
            Assert.IsTrue(report.AddedTriples.Any(), "Difference should have reported some Added Triples");
        }

        [TestMethod]
        public void DiffAddedMSG()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            FileLoader.Load(h, "InferenceTest.ttl");

            //Add additional Triple to 2nd Graph
            INode blank = h.CreateBlankNode();
            UriNode subClass = h.CreateUriNode("rdfs:subClassOf");
            UriNode vehicle = h.CreateUriNode("eg:Vehicle");
            h.Assert(new Triple(blank, subClass, vehicle));

            GraphDiffReport report = g.Difference(h);
            ShowDifferences(report);

            Assert.IsFalse(report.AreEqual, "Graphs should not have been reported as equal");
            Assert.IsTrue(report.AddedMSGs.Any(), "Difference should have reported some Added MSGs");
        }

        [TestMethod]
        public void DiffRemovedMSG()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            FileLoader.Load(h, "InferenceTest.ttl");

            //Remove MSG from 2nd Graph
            h.Retract(h.GetTriplesWithSubject(h.GetBlankNode("autos1")));

            GraphDiffReport report = g.Difference(h);
            ShowDifferences(report);

            Assert.IsFalse(report.AreEqual, "Graphs should not have been reported as equal");
            Assert.IsTrue(report.RemovedMSGs.Any(), "Difference should have reported some Removed MSGs");
        }

        private static void ShowDifferences(GraphDiffReport report)
        {
            NTriplesFormatter formatter = new NTriplesFormatter();

            if (report.AreEqual)
            {
                Console.WriteLine("Graphs are Equal");
                Console.WriteLine();
                Console.WriteLine("Blank Node Mapping between Graphs:");
                foreach (KeyValuePair<INode, INode> kvp in report.Mapping)
                {
                    Console.WriteLine(kvp.Key.ToString(formatter) + " => " + kvp.Value.ToString(formatter));
                }
            }
            else
            {
                Console.WriteLine("Graphs are non-equal");
                Console.WriteLine();
                Console.WriteLine("Triples added to 1st Graph to give 2nd Graph:");
                foreach (Triple t in report.AddedTriples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }
                Console.WriteLine();
                Console.WriteLine("Triples removed from 1st Graph to given 2nd Graph:");
                foreach (Triple t in report.RemovedTriples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }
                Console.WriteLine();
                Console.WriteLine("Blank Node Mapping between Graphs:");
                foreach (KeyValuePair<INode, INode> kvp in report.Mapping)
                {
                    Console.WriteLine(kvp.Key.ToString(formatter) + " => " + kvp.Value.ToString(formatter));
                }
                Console.WriteLine();
                Console.WriteLine("MSGs added to 1st Graph to give 2nd Graph:");
                foreach (IGraph msg in report.AddedMSGs)
                {
                    foreach (Triple t in msg.Triples)
                    {
                        Console.WriteLine(t.ToString(formatter));
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
                Console.WriteLine("MSGs removed from 1st Graph to give 2nd Graph:");
                foreach (IGraph msg in report.RemovedMSGs)
                {
                    foreach (Triple t in msg.Triples)
                    {
                        Console.WriteLine(t.ToString(formatter));
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
