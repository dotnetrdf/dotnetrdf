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
        public void GraphDiffEqualGraphs()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            h = g;

            GraphDiffReport report = g.Difference(h);
            TestTools.ShowDifferences(report);

            Assert.IsTrue(report.AreEqual, "Graphs should be equal");
        }

        [TestMethod]
        public void GraphDiffDifferentGraphs()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            FileLoader.Load(h, "Turtle.ttl");

            GraphDiffReport report = g.Difference(h);
            TestTools.ShowDifferences(report);

            Assert.IsFalse(report.AreEqual, "Graphs should not be equal");
        }

        [TestMethod]
        public void GraphDiffEqualGraphs2()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            FileLoader.Load(h, "InferenceTest.ttl");

            GraphDiffReport report = g.Difference(h);
            TestTools.ShowDifferences(report);

            Assert.IsTrue(report.AreEqual, "Graphs should be equal");
        }

        [TestMethod]
        public void GraphDiffRemovedGroundTriples()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            FileLoader.Load(h, "InferenceTest.ttl");

            //Remove Triples about Ford Fiestas from 2nd Graph
            h.Retract(h.GetTriplesWithSubject(new Uri("http://example.org/vehicles/FordFiesta")));

            GraphDiffReport report = g.Difference(h);
            TestTools.ShowDifferences(report);

            Assert.IsFalse(report.AreEqual, "Graphs should not have been reported as equal");
            Assert.IsTrue(report.RemovedTriples.Any(), "Difference should have reported some Removed Triples");
        }

        [TestMethod]
        public void GraphDiffAddedGroundTriples()
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
            TestTools.ShowDifferences(report);

            Assert.IsFalse(report.AreEqual, "Graphs should not have been reported as equal");
            Assert.IsTrue(report.AddedTriples.Any(), "Difference should have reported some Added Triples");
        }

        [TestMethod]
        public void GraphDiffAddedMSG()
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
            TestTools.ShowDifferences(report);

            Assert.IsFalse(report.AreEqual, "Graphs should not have been reported as equal");
            Assert.IsTrue(report.AddedMSGs.Any(), "Difference should have reported some Added MSGs");
        }

        [TestMethod]
        public void GraphDiffRemovedMSG()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            FileLoader.Load(h, "InferenceTest.ttl");

            //Remove MSG from 2nd Graph
            h.Retract(h.GetTriplesWithSubject(h.GetBlankNode("autos1")));

            GraphDiffReport report = g.Difference(h);
            TestTools.ShowDifferences(report);

            Assert.IsFalse(report.AreEqual, "Graphs should not have been reported as equal");
            Assert.IsTrue(report.RemovedMSGs.Any(), "Difference should have reported some Removed MSGs");
        }

    }
}
