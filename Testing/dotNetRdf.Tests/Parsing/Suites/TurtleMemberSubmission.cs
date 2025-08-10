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
using System.IO;
using Xunit;

namespace VDS.RDF.Parsing.Suites;


public class TurtleMemberSubmission
    : BaseRdfParserSuite
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TurtleMemberSubmission(ITestOutputHelper testOutputHelper)
        : base(new TurtleParser(TurtleSyntax.Original, false), 
            new NTriplesParser(NTriplesSyntax.Original), //KA: test-29 will be indeterminate if using NTriples 1.1 syntax due to \t escape in the expected output result file
            "turtle")
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void ParsingSuiteTurtleOriginal()
    {
        //Run manifests
        RunManifest(Path.Combine("resources", "turtle", "manifest.ttl"), true);
        RunManifest(Path.Combine("resources", "turtle", "manifest-bad.ttl"), false);

        if (Count == 0) Assert.Fail("No tests found");

        _testOutputHelper.WriteLine(Count + " Tests - " + Passed + " Passed - " + Failed + " Failed");
        _testOutputHelper.WriteLine(((Passed / (double)Count) * 100) + "% Passed");

        if (Failed > 0)
        {
            Assert.Fail(Failed + " Tests failed: " + string.Join(", ", FailedTests));
        }
        Assert.SkipWhen(Indeterminate > 0, Indeterminate + " Tests are indeterminate: " + string.Join(", ", IndeterminateTests));
    }

    [Fact]
    public void ParsingTurtleOriginalBaseTurtleStyle1()
    {
        //Dot required
        var graph = "@base <http://example.org/> .";
        var g = new Graph();
        Parser.Load(g, new StringReader(graph));

        Assert.Equal(new Uri("http://example.org"), g.BaseUri);
    }

    [Fact]
    public void ParsingTurtleOriginalBaseTurtleStyle2()
    {
        //Missing dot
        var graph = "@base <http://example.org/>";
        var g = new Graph();
        Assert.Throws<RdfParseException>(() => Parser.Load(g, new StringReader(graph)));

        Assert.Equal(new Uri("http://example.org"), g.BaseUri);
    }

    [Fact]
    public void ParsingTurtleOriginalBaseSparqlStyle1()
    {
        //Forbidden in Original Turtle
        var graph = "BASE <http://example.org/> .";
        var g = new Graph();
        Assert.Throws<RdfParseException>(() => Parser.Load(g, new StringReader(graph)));
    }

    [Fact]
    public void ParsingTurtleOriginalBaseSparqlStyle2()
    {
        //Forbidden in Original Turtle
        var graph = "BASE <http://example.org/>";
        var g = new Graph();
        Assert.Throws<RdfParseException>(() => Parser.Load(g, new StringReader(graph)));
    }

    [Fact]
    public void ParsingTurtleOriginalPrefixTurtleStyle1()
    {
        //Dot required
        var graph = "@prefix ex: <http://example.org/> .";
        var g = new Graph();
        Parser.Load(g, new StringReader(graph));
        Assert.Equal(new Uri("http://example.org"), g.NamespaceMap.GetNamespaceUri("ex"));
    }

    [Fact]
    public void ParsingTurtleOriginalPrefixTurtleStyle2()
    {
        //Missing dot
        var graph = "@prefix ex: <http://example.org/>";
        var g = new Graph();
        Assert.Throws<RdfParseException>(() => Parser.Load(g, new StringReader(graph)));

        Assert.Equal(new Uri("http://example.org"), g.NamespaceMap.GetNamespaceUri("ex"));
    }

    [Fact]
    public void ParsingTurtleOriginalPrefixSparqlStyle1()
    {
        //Forbidden in Original Turtle
        var graph = "PREFIX ex: <http://example.org/> .";
        var g = new Graph();
        Assert.Throws<RdfParseException>(() => Parser.Load(g, new StringReader(graph)));
    }

    [Fact]
    public void ParsingTurtleOriginalPrefixSparqlStyle2()
    {
        //Forbidden in Original Turtle
        var graph = "PREFIX ex: <http://example.org/>";
        var g = new Graph();
        Assert.Throws<RdfParseException>(() => Parser.Load(g, new StringReader(graph)));
    }

    [Fact]
    public void ParsingTurtleOriginalPrefixedNames1()
    {
        Assert.True(TurtleSpecsHelper.IsValidQName(":a1", TurtleSyntax.Original));
    }

    [Fact]
    public void ParsingTurtleOriginalPrefixedNames2()
    {
        Parser.Load(new Graph(), Path.Combine("resources", "turtle", "test-14.ttl"));
    }
}
