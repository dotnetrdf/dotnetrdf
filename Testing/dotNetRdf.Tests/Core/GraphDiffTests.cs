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
using System.IO;
using System.Linq;
using Xunit;

namespace VDS.RDF;


public class GraphDiffTests
{
    [Fact]
    public void GraphDiffEqualGraphs()
    {
        var g = new Graph();
        var h = new Graph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));
        h = g;

        GraphDiffReport report = g.Difference(h);
        TestTools.ShowDifferences(report);

        Assert.True(report.AreEqual, "Graphs should be equal");
    }

    [Fact]
    public void GraphDiffDifferentGraphs()
    {
        var g = new Graph();
        var h = new Graph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));
        h.LoadFromFile(Path.Combine("resources", "Turtle.ttl"));

        GraphDiffReport report = g.Difference(h);
        TestTools.ShowDifferences(report);

        Assert.False(report.AreEqual, "Graphs should not be equal");
    }

    [Fact]
    public void GraphDiffEqualGraphs2()
    {
        var g = new Graph();
        var h = new Graph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));
        h.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));

        GraphDiffReport report = g.Difference(h);
        TestTools.ShowDifferences(report);

        Assert.True(report.AreEqual, "Graphs should be equal");
    }

    [Fact]
    public void GraphDiffRemovedGroundTriples()
    {
        var g = new Graph();
        var h = new Graph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));
        h.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));

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
        var g = new Graph();
        var h = new Graph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));
        h.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));

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
        var g = new Graph();
        var h = new Graph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));
        h.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));

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

    [Fact]
    public void GraphDiffRemovedMSG()
    {
        var g = new Graph();
        var h = new Graph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));
        h.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));

        //Remove MSG from 2nd Graph
        INode toRemove = h.Nodes.BlankNodes().FirstOrDefault();
        Assert.SkipWhen(toRemove == null, "No MSGs in test graph");
        h.Retract(h.GetTriplesWithSubject(toRemove).ToList());

        GraphDiffReport report = g.Difference(h);
        TestTools.ShowDifferences(report);

        Assert.False(report.AreEqual, "Graphs should not have been reported as equal");
        Assert.True(report.RemovedMSGs.Any(), "Difference should have reported some Removed MSGs");
    }

    [Fact]
    public void GraphDiffNullReferenceBoth()
    {
        var diff = new GraphDiff();
        GraphDiffReport report = diff.Difference(null, null);

        TestTools.ShowDifferences(report);

        Assert.True(report.AreEqual, "Graphs should have been reported as equal for two null references");
        Assert.False(report.AreDifferentSizes, "Graphs should have been reported same size for two null references");
    }

    [Fact]
    public void GraphDiffNullReferenceA()
    {
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));

        var diff = new GraphDiff();
        GraphDiffReport report = diff.Difference(null, g);
        TestTools.ShowDifferences(report);

        Assert.False(report.AreEqual, "Graphs should have been reported as non-equal for one null reference");
        Assert.True(report.AreDifferentSizes, "Graphs should have been reported as different sizes for one null reference");
        Assert.True(report.AddedTriples.Any(), "Report should list added triples");
    }

    [Fact]
    public void GraphDiffNullReferenceB()
    {
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));

        GraphDiffReport report = g.Difference(null);
        TestTools.ShowDifferences(report);

        Assert.False(report.AreEqual, "Graphs should have been reported as non-equal for one null reference");
        Assert.True(report.AreDifferentSizes, "Graphs should have been reported as different sizes for one null reference");
        Assert.True(report.RemovedTriples.Any(), "Report should list removed triples");
    }

    public static IEnumerable<TheoryDataRow<string>> DiffCases()
    {
        var resourceDirectory = new DirectoryInfo(Path.Combine("resources", "diff_cases"));
        foreach (FileInfo fileA in resourceDirectory.EnumerateFiles("*_a.ttl"))
        {
            var testCase = fileA.Name;
            testCase = testCase.Substring(0, testCase.LastIndexOf('_'));
            yield return new(testCase);
        }
    }

    [Theory]
    [MemberData(nameof(DiffCases))]
    public void EqualGraphTest(string testGraphName)
    {
        TestGraphDiff(testGraphName);
    }

    /* Helper test for debugging
    [Fact]
    public void TestSingle()
    {
        TestGraphDiff("case7");
    }
    */

    private static void TestGraphDiff(string testGraphName)
    {
        var a = new Graph();
        a.LoadFromFile(Path.Combine("resources", "diff_cases", $"{testGraphName}_a.ttl"));
        var b = new Graph();
        b.LoadFromFile(Path.Combine("resources", "diff_cases", $"{testGraphName}_b.ttl"));

        GraphDiffReport diff = a.Difference(b);

        if (!diff.AreEqual)
        {
            TestTools.ShowDifferences(diff);
        }
        Assert.True(diff.AreEqual);
    }
}
