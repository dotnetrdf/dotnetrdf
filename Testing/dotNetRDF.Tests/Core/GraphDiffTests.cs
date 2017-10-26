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
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;
using VDS.RDF.XunitExtensions;

namespace VDS.RDF
{

    public class GraphDiffTests
    {
        [Fact]
        public void GraphDiffEqualGraphs()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");
            h = g;

            GraphDiffReport report = g.Difference(h);
            TestTools.ShowDifferences(report);

            Assert.True(report.AreEqual, "Graphs should be equal");
        }

        [Fact]
        public void GraphDiffDifferentGraphs()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");
            h.LoadFromFile("resources\\Turtle.ttl");

            GraphDiffReport report = g.Difference(h);
            TestTools.ShowDifferences(report);

            Assert.False(report.AreEqual, "Graphs should not be equal");
        }

        [Fact]
        public void GraphDiffEqualGraphs2()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");
            h.LoadFromFile("resources\\InferenceTest.ttl");

            GraphDiffReport report = g.Difference(h);
            TestTools.ShowDifferences(report);

            Assert.True(report.AreEqual, "Graphs should be equal");
        }

        [Fact]
        public void GraphDiffRemovedGroundTriples()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");
            h.LoadFromFile("resources\\InferenceTest.ttl");

            //Remove Triples about Ford Fiestas from 2nd Graph
            h.Retract(h.GetTriplesWithSubject(new Uri("http://example.org/vehicles/FordFiesta")).ToList());

            GraphDiffReport report = g.Difference(h);
            TestTools.ShowDifferences(report);

            Assert.False(report.AreEqual, "Graphs should not have been reported as equal");
            Assert.True(report.RemovedTriples.Any(), "Difference should have reported some Removed Triples");
        }

        [Fact]
        public void GraphDiffAddedGroundTriples()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");
            h.LoadFromFile("resources\\InferenceTest.ttl");

            //Add additional Triple to 2nd Graph
            IUriNode spaceVehicle = h.CreateUriNode("eg:SpaceVehicle");
            IUriNode subClass = h.CreateUriNode("rdfs:subClassOf");
            IUriNode vehicle = h.CreateUriNode("eg:Vehicle");
            h.Assert(new Triple(spaceVehicle, subClass, vehicle));

            GraphDiffReport report = g.Difference(h);
            TestTools.ShowDifferences(report);

            Assert.False(report.AreEqual, "Graphs should not have been reported as equal");
            Assert.True(report.AddedTriples.Any(), "Difference should have reported some Added Triples");
        }

        [Fact]
        public void GraphDiffAddedMSG()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");
            h.LoadFromFile("resources\\InferenceTest.ttl");

            //Add additional Triple to 2nd Graph
            INode blank = h.CreateBlankNode();
            IUriNode subClass = h.CreateUriNode("rdfs:subClassOf");
            IUriNode vehicle = h.CreateUriNode("eg:Vehicle");
            h.Assert(new Triple(blank, subClass, vehicle));

            GraphDiffReport report = g.Difference(h);
            TestTools.ShowDifferences(report);

            Assert.False(report.AreEqual, "Graphs should not have been reported as equal");
            Assert.True(report.AddedMSGs.Any(), "Difference should have reported some Added MSGs");
        }

        [SkippableFact]
        public void GraphDiffRemovedMSG()
        {
            Graph g = new Graph();
            Graph h = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");
            h.LoadFromFile("resources\\InferenceTest.ttl");

            //Remove MSG from 2nd Graph
            INode toRemove = h.Nodes.BlankNodes().FirstOrDefault();
            if (toRemove == null) throw new SkipTestException("No MSGs in test graph");
            h.Retract(h.GetTriplesWithSubject(toRemove).ToList());

            GraphDiffReport report = g.Difference(h);
            TestTools.ShowDifferences(report);

            Assert.False(report.AreEqual, "Graphs should not have been reported as equal");
            Assert.True(report.RemovedMSGs.Any(), "Difference should have reported some Removed MSGs");
        }

        [Fact]
        public void GraphDiffNullReferenceBoth()
        {
            GraphDiff diff = new GraphDiff();
            GraphDiffReport report = diff.Difference(null, null);

            TestTools.ShowDifferences(report);

            Assert.True(report.AreEqual, "Graphs should have been reported as equal for two null references");
            Assert.False(report.AreDifferentSizes, "Graphs should have been reported same size for two null references");
        }

        [Fact]
        public void GraphDiffNullReferenceA()
        {
            Graph g = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");

            GraphDiff diff = new GraphDiff();
            GraphDiffReport report = diff.Difference(null, g);
            TestTools.ShowDifferences(report);

            Assert.False(report.AreEqual, "Graphs should have been reported as non-equal for one null reference");
            Assert.True(report.AreDifferentSizes, "Graphs should have been reported as different sizes for one null reference");
            Assert.True(report.AddedTriples.Any(), "Report should list added triples");
        }

        [Fact]
        public void GraphDiffNullReferenceB()
        {
            Graph g = new Graph();
            g.LoadFromFile("resources\\InferenceTest.ttl");

            GraphDiffReport report = g.Difference(null);
            TestTools.ShowDifferences(report);

            Assert.False(report.AreEqual, "Graphs should have been reported as non-equal for one null reference");
            Assert.True(report.AreDifferentSizes, "Graphs should have been reported as different sizes for one null reference");
            Assert.True(report.RemovedTriples.Any(), "Report should list removed triples");
        }

        [Fact]
        public void GraphDiffSlowOnEqualGraphsCase1()
        {
            const string testGraphName = "case1";
            TestGraphDiff(testGraphName);
        }

        [Fact]
        public void GraphDiffSlowOnEqualGraphsCase2()
        {
            const string testGraphName = "case2";
            TestGraphDiff(testGraphName);
        }

        [Fact]
        public void GraphDiffSlowOnEqualGraphsCase3()
        {
            const string testGraphName = "case3";
            TestGraphDiff(testGraphName);
        }

        [Fact]
        public void GraphDiffSlowOnEqualGraphsCase4()
        {
            const string testGraphName = "case4";
            TestGraphDiff(testGraphName);
        }

        [Fact]
        public void GraphDiffSlowOnEqualGraphsCase5()
        {
            const string testGraphName = "case5";
            TestGraphDiff(testGraphName);
        }

        [Fact]
        public void GraphDiffSlowOnEqualGraphsCase6()
        {
            const string testGraphName = "case6";
            TestGraphDiff(testGraphName);
        }

        private static void TestGraphDiff(string testGraphName)
        {
            Graph a = new Graph();
            a.LoadFromFile(string.Format("resources\\diff_cases\\{0}_a.ttl", testGraphName));
            Graph b = new Graph();
            b.LoadFromFile(string.Format("resources\\diff_cases\\{0}_b.ttl", testGraphName));

            var diff = a.Difference(b);

            if (!diff.AreEqual)
            {
                TestTools.ShowDifferences(diff);
            }
            Assert.True(diff.AreEqual);
        }
    }
}
