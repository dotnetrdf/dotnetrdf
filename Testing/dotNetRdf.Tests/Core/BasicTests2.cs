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
using FluentAssertions;
using Xunit;
using VDS.RDF.Parsing;

#pragma warning disable CS1718 // Comparison made to same variable

namespace VDS.RDF;

[Collection("RdfServer")]
public class BasicTests2 : BaseTest
{
    private readonly RdfServerFixture _serverFixture;

    public BasicTests2(RdfServerFixture serverFixture, ITestOutputHelper output) : base(output)
    {
        _serverFixture = serverFixture;
    }

    [Fact]
    public void GraphWithBNodeEquality()
    {
        Debug("Testing Graph Equality when the Graphs have Blank Nodes");
        var g = new Graph();
        var h = new Graph();

        var ttlparser = new TurtleParser();
        ttlparser.Load(g, Path.Combine("resources", "MergePart1.ttl"));
        ttlparser.Load(h, Path.Combine("resources", "MergePart1.ttl"));

        Assert.Equal(g.BaseUri, h.BaseUri);
        //TestTools.CompareGraphs(g, h, true);
        Dictionary<INode, INode> mapping;
        var equals = g.Equals(h, out mapping);
        Assert.True(@equals, "Graphs should have been equal");
        if (mapping != null)
        {
            Debug("Blank Node Mapping was:");
            foreach (KeyValuePair<INode, INode> pair in mapping)
            {
                Debug(pair.Key.ToString() + " => " + pair.Value.ToString());
            }
        }
    }

    [Fact]
    public void NodesEqualityOperator()
    {
        Debug("Testing that the overridden operators for Nodes work as expected");

        var g = new Graph();
        IBlankNode a = g.CreateBlankNode();
        IBlankNode b = g.CreateBlankNode();

        Debug("Testing using Equals() method");
        Assert.False(a.Equals(b), "Two different Blank Nodes should be non-equal");
        Assert.True(a.Equals(a), "A Blank Node should be equal to itself");
        Assert.True(b.Equals(b), "A Blank Node should be equal to itself");
        Debug("OK");

        Debug();

        Debug("Testing using == operator");
        Assert.False(a == b, "Two different Blank Nodes should be non-equal");
        Assert.True(a == a, "A Blank Node should be equal to itself");
        Assert.True(b == b, "A Blank Node should be equal to itself");
        Debug("OK");

        Debug();

        //Test typed as INode
        INode c = g.CreateBlankNode();
        INode d = g.CreateBlankNode();

        Debug("Now testing with typed as INode using Equals()");
        Assert.False(c.Equals(d), "Two different Nodes should be non-equal");
        Assert.True(c.Equals(c), "A Node should be equal to itself");
        Assert.True(d.Equals(d), "A Node should be equal to itself");
        Debug("OK");

        Debug();

        Debug("Now testing with typed as INode using == operator");
        Assert.False(c == d, "Two different Nodes should be non-equal");
        Assert.True(c == c, "A Node should be equal to itself");
        Assert.True(d == d, "A Node should be equal to itself");
        Debug("OK");
    }

    [Fact]
    public void GraphPersistenceWrapperNodeCreation()
    {
        var g = new Graph();
        var wrapper = new GraphPersistenceWrapper(g);

        INode s = wrapper.CreateBlankNode();
        INode p = wrapper.CreateUriNode("rdf:type");
        INode o = wrapper.CreateUriNode("rdfs:Class");

        wrapper.Assert(s, p, o);
    }

    [Fact]
    public void GraphEquality()
    {
        var g = new Graph();
        var h = new Graph();

        g.LoadFromFile(Path.Combine("resources", "pp.rdf"));
        h.LoadFromFile(Path.Combine("resources", "pp.rdf"));

        //Should have same Base Uri
        Assert.Equal(g.BaseUri, h.BaseUri);

        //Do equality check
        var equals = g.Equals(h, out Dictionary<INode, INode> mapping);
        Assert.True(equals, "Graphs should have been equal");
        mapping.Should().NotBeNull("Equality function should return a blank node mapping");
        mapping.Should().NotBeEmpty("Matched graphs contain mapped blank nodes");

        //Get a third graph of something different
        var i = new Graph();
        i.LoadFromFile(Path.Combine("resources", "czech-royals.ttl"));

        //Should have different Base URIs and be non-equal
        Assert.NotEqual(g.BaseUri, i.BaseUri);
        Assert.NotEqual(h.BaseUri, i.BaseUri);
        Assert.False(g.Equals(i));
        Assert.False(h.Equals(i));
    }

    [Fact]
    public void GraphSubGraphMatching()
    {
        var parent = new Graph();
        FileLoader.Load(parent,  Path.Combine("resources", "InferenceTest.ttl"));
        var subgraph = new Graph();
        subgraph.NamespaceMap.Import(parent.NamespaceMap);
        subgraph.Assert(parent.GetTriplesWithSubject(parent.CreateUriNode("eg:FordFiesta")));

        //Check method calls
        Dictionary<INode, INode> mapping;
        Debug("Doing basic sub-graph matching with no BNode tests");
        Assert.True(parent.HasSubGraph(subgraph, out mapping), "Failed to match the sub-graph as expected");
        Assert.False(parent.IsSubGraphOf(subgraph, out mapping), "Parent should not be a sub-graph of the sub-graph");
        Assert.False(subgraph.HasSubGraph(parent, out mapping), "Sub-graph should not have parent as its sub-graph");
        Assert.True(subgraph.IsSubGraphOf(parent, out mapping), "Failed to match the sub-graph as expected");
        Debug("OK");
        Debug();

        //Add an extra triple into the Graph which will cause it to no longer be a sub-graph
        Debug("Adding an extra Triple so the sub-graph is no longer such");
        subgraph.Assert(new Triple(subgraph.CreateUriNode("eg:Rocket"), subgraph.CreateUriNode("rdf:type"), subgraph.CreateUriNode("eg:AirVehicle")));
        Assert.False(parent.HasSubGraph(subgraph, out mapping), "Sub-graph should no longer be considered a sub-graph");
        Assert.False(subgraph.IsSubGraphOf(parent, out mapping), "Sub-graph should no longer be considered a sub-graph");
        Debug("OK");
        Debug();

        //Reset the sub-graph
        Debug("Resetting the sub-graph");
        subgraph = new Graph();
        subgraph.NamespaceMap.Import(parent.NamespaceMap);
        subgraph.Assert(parent.GetTriplesWithSubject(parent.CreateUriNode("eg:FordFiesta")));
        Debug("Adding additional information to the parent Graph, this should not affect the fact that the sub-graph is a sub-graph of it");
        Assert.True(parent.HasSubGraph(subgraph, out mapping), "Failed to match the sub-graph as expected");
        Assert.False(parent.IsSubGraphOf(subgraph, out mapping), "Parent should not be a sub-graph of the sub-graph");
        Assert.False(subgraph.HasSubGraph(parent, out mapping), "Sub-graph should not have parent as its sub-graph");
        Assert.True(subgraph.IsSubGraphOf(parent, out mapping), "Failed to match the sub-graph as expected");
        Debug("OK");
        Debug();

        //Remove stuff from parent graph so it won't match any more
        Debug("Removing stuff from parent graph so that it won't have the sub-graph anymore");
        parent.Retract(parent.GetTriplesWithSubject(parent.CreateUriNode("eg:FordFiesta")).ToList());
        Assert.False(parent.HasSubGraph(subgraph, out mapping), "Parent should no longer contian the sub-graph");
        Assert.False(subgraph.IsSubGraphOf(parent, out mapping), "Parent should no longer contain the sub-graph");
        Debug("OK");
        Debug();
    }

    [Fact]
    public void GraphSubGraphMatchingWithBNodes()
    {
        var parent = new Graph();
        FileLoader.Load(parent, Path.Combine("resources", "Turtle.ttl"));
        var subgraph = new Graph();
        subgraph.Assert(parent.Triples.Where(t => !t.IsGroundTriple));

        //Check method calls
        Dictionary<INode, INode> mapping;
        Debug("Doing basic sub-graph matching with BNode tests");
        Assert.True(parent.HasSubGraph(subgraph, out mapping), "Failed to match the sub-graph as expected");
        Assert.False(parent.IsSubGraphOf(subgraph, out mapping), "Parent should not be a sub-graph of the sub-graph");
        Assert.False(subgraph.HasSubGraph(parent, out mapping), "Sub-graph should not have parent as its sub-graph");
        Assert.True(subgraph.IsSubGraphOf(parent, out mapping), "Failed to match the sub-graph as expected");
        Debug("OK");
        Debug();

        //Eliminate some of the Triples from the sub-graph
        Debug("Eliminating some Triples from the sub-graph and seeing if the mapping still computes OK");
        subgraph.Retract(subgraph.Triples.Skip(2).Take(5).ToList());
        Assert.True(parent.HasSubGraph(subgraph, out mapping), "Failed to match the sub-graph as expected");
        Assert.False(parent.IsSubGraphOf(subgraph, out mapping), "Parent should not be a sub-graph of the sub-graph");
        Assert.False(subgraph.HasSubGraph(parent, out mapping), "Sub-graph should not have parent as its sub-graph");
        Assert.True(subgraph.IsSubGraphOf(parent, out mapping), "Failed to match the sub-graph as expected");
        Debug("OK");
        Debug();

        Debug("Eliminating Blank Nodes from the parent Graph to check that the sub-graph is no longer considered as such afterwards");
        parent.Retract(parent.Triples.Where(t => !t.IsGroundTriple).ToList());
        Assert.False(parent.HasSubGraph(subgraph), "Sub-graph should no longer be considered as such");
        Assert.False(subgraph.IsSubGraphOf(parent), "Sub-graph should no longer be considered as such");

    }

    [Fact]
    public void ParsingUriLoader()
    {
        var loader = new Loader(_serverFixture.Client);

        var testUris = new List<Uri>()
        {
            _serverFixture.UriFor("/resource/Southampton"),
            _serverFixture.UriFor("/resource/czech-royals"),
            new Uri("file://resources/MergePart1.ttl"),
        };

        foreach (Uri u in testUris)
        {
            //Load the Test RDF
            var g = new Graph();
            Assert.NotNull(g);
            loader.LoadGraph(g, u);

            if (!u.IsFile)
            {
                Assert.Equal(u, g.BaseUri);
            }
        }
    }
}
