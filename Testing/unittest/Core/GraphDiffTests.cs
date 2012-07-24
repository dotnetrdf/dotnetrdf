/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
