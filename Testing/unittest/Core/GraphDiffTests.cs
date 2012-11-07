/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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
            h.Retract(h.GetTriplesWithSubject(new Uri("http://example.org/vehicles/FordFiesta")).ToList());

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
            IUriNode spaceVehicle = h.CreateUriNode("eg:SpaceVehicle");
            IUriNode subClass = h.CreateUriNode("rdfs:subClassOf");
            IUriNode vehicle = h.CreateUriNode("eg:Vehicle");
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
            IUriNode subClass = h.CreateUriNode("rdfs:subClassOf");
            IUriNode vehicle = h.CreateUriNode("eg:Vehicle");
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
            h.Retract(h.GetTriplesWithSubject(h.GetBlankNode("autos1")).ToList());

            GraphDiffReport report = g.Difference(h);
            TestTools.ShowDifferences(report);

            Assert.IsFalse(report.AreEqual, "Graphs should not have been reported as equal");
            Assert.IsTrue(report.RemovedMSGs.Any(), "Difference should have reported some Removed MSGs");
        }

        [TestMethod]
        public void GraphDiffNullReferenceBoth()
        {
            GraphDiff diff = new GraphDiff();
            GraphDiffReport report = diff.Difference(null, null);

            TestTools.ShowDifferences(report);

            Assert.IsTrue(report.AreEqual, "Graphs should have been reported as equal for two null references");
            Assert.IsFalse(report.AreDifferentSizes, "Graphs should have been reported same size for two null references");
        }

        [TestMethod]
        public void GraphDiffNullReferenceA()
        {
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");

            GraphDiff diff = new GraphDiff();
            GraphDiffReport report = diff.Difference(null, g);
            TestTools.ShowDifferences(report);

            Assert.IsFalse(report.AreEqual, "Graphs should have been reported as non-equal for one null reference");
            Assert.IsTrue(report.AreDifferentSizes, "Graphs should have been reported as different sizes for one null reference");
            Assert.IsTrue(report.AddedTriples.Any(), "Report should list added triples");
        }

        [TestMethod]
        public void GraphDiffNullReferenceB()
        {
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");

            GraphDiffReport report = g.Difference(null);
            TestTools.ShowDifferences(report);

            Assert.IsFalse(report.AreEqual, "Graphs should have been reported as non-equal for one null reference");
            Assert.IsTrue(report.AreDifferentSizes, "Graphs should have been reported as different sizes for one null reference");
            Assert.IsTrue(report.RemovedTriples.Any(), "Report should list removed triples");
        }
    }
}
