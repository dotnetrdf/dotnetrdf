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
using System.Linq;
using Xunit;

namespace VDS.RDF.Parsing.Suites;


public class NTriples
    : BaseRdfParserSuite
{
    private readonly ITestOutputHelper _testOutputHelper;

    public NTriples(ITestOutputHelper testOutputHelper)
        : base(new NTriplesParser(NTriplesSyntax.Original), new NTriplesParser(NTriplesSyntax.Original), "ntriples")
    {
        _testOutputHelper = testOutputHelper;
        CheckResults = false;
    }

    [Fact]
    public void ParsingSuiteNTriples()
    {
        //Run manifests
        RunDirectory(f => Path.GetExtension(f).Equals(".nt"), true);

        if (Count == 0) Assert.Fail("No tests found");

        _testOutputHelper.WriteLine(Count + " Tests - " + Passed + " Passed - " + Failed + " Failed");
        _testOutputHelper.WriteLine(((Passed/(double) Count)*100) + "% Passed");

        if (Failed > 0) Assert.Fail(Failed + " Tests failed");
        Assert.SkipWhen(Indeterminate > 0, Indeterminate + " Tests are indeterminate");
    }

    [Fact]
    public void ParsingNTriplesUnicodeEscapes1()
    {
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "turtle11", "localName_with_assigned_nfc_bmp_PN_CHARS_BASE_character_boundaries.nt"), Parser);
        Assert.False(g.IsEmpty);
        Assert.Equal(1, g.Triples.Count);
    }

    [Fact]
    public void ParsingNTriplesComplexBNodeIDs()
    {
        const String data = @"_:node-id.complex_id.blah <http://p> <http://o> .
<http://s> <http://p> _:node.id.";

        var g = new Graph();
        Assert.Throws<RdfParseException>(() => g.LoadFromString(data, Parser));
    }

    [Fact]
    public void ParsingNTriplesLiteralEscapes1()
    {
        const String data = @"<http://s> <http://p> ""literal\'quote"" .";

        var g = new Graph();
        Assert.Throws<RdfParseException>(() => g.LoadFromString(data, Parser));
    }

    [Fact]
    public void ParsingNTriplesLiteralEscapes2()
    {
        const String data = @"<http://s> <http://p> ""literal\""quote"" .";

        var g = new Graph();
        g.LoadFromString(data, Parser);

        Assert.False(g.IsEmpty);
        Assert.Equal(1, g.Triples.Count);
    }
}


public class NTriples11
    : BaseRdfParserSuite
{
    public NTriples11()
        : base(new NTriplesParser(NTriplesSyntax.Rdf11), new NTriplesParser(NTriplesSyntax.Rdf11), @"ntriples11")
    {
        CheckResults = false;
        Parser.Warning += TestTools.WarningPrinter;
    }

    [Fact]
    public void ParsingSuiteNTriples11()
    {
        //Nodes for positive and negative tests
        var g = new Graph();
        g.NamespaceMap.AddNamespace("rdft", UriFactory.Root.Create("http://www.w3.org/ns/rdftest#"));
        INode posSyntaxTest = g.CreateUriNode("rdft:TestNTriplesPositiveSyntax");
        INode negSyntaxTest = g.CreateUriNode("rdft:TestNTriplesNegativeSyntax");

        //Run manifests
        RunManifest(Path.Combine("resources", "ntriples11", "manifest.ttl"), posSyntaxTest, negSyntaxTest);

        if (Count == 0) Assert.Fail("No tests found");

        Console.WriteLine(Count + " Tests - " + Passed + " Passed - " + Failed + " Failed");
        Console.WriteLine(((Passed/(double) Count)*100) + "% Passed");

        if (Failed > 0) Assert.Fail(Failed + " Tests failed");
        Assert.SkipWhen(Indeterminate > 0, Indeterminate + " Tests are indeterminate");
    }

    [Fact]
    public void ParsingNTriples11ComplexBNodeIDs()
    {
        const String data = @"_:node-id.complex_id.blah <http://p> <http://o> .
<http://s> <http://p> _:node.id.";

        var g = new Graph();
        g.LoadFromString(data, Parser);
        Assert.False(g.IsEmpty);
        Assert.Equal(2, g.Triples.Count);
    }

    [Fact]
    public void ParsingNTriples11LiteralEscapes1()
    {
        const String data = @"<http://s> <http://p> ""literal\'quote"" .";

        var g = new Graph();
        g.LoadFromString(data, Parser);

        Assert.False(g.IsEmpty);
        Assert.Equal(1, g.Triples.Count);
    }

    [Fact]
    public void ParsingNTriples11LiteralEscapes2()
    {
        const String data = @"<http://s> <http://p> ""literal\""quote"" .";

        var g = new Graph();
        g.LoadFromString(data, Parser);

        Assert.False(g.IsEmpty);
        Assert.Equal(1, g.Triples.Count);
    }
}

public class NTriplesStar : BaseRdfParserSuite
{
    public NTriplesStar()
        : base(new NTriplesParser(NTriplesSyntax.Rdf11Star), new NTriplesParser(NTriplesSyntax.Rdf11Star), @"ntriples11")
    {
        CheckResults = false;
        Parser.Warning += TestTools.WarningPrinter;
    }

    [Fact]
    public void ParsingSuiteNTriples11()
    {
        //Nodes for positive and negative tests
        var g = new Graph();
        g.NamespaceMap.AddNamespace("rdft", UriFactory.Root.Create("http://www.w3.org/ns/rdftest#"));
        INode posSyntaxTest = g.CreateUriNode("rdft:TestNTriplesPositiveSyntax");
        INode negSyntaxTest = g.CreateUriNode("rdft:TestNTriplesNegativeSyntax");

        //Run manifests
        RunManifest(Path.Combine("resources", "ntriples11", "manifest.ttl"), posSyntaxTest, negSyntaxTest);

        if (Count == 0) Assert.Fail("No tests found");

        Console.WriteLine(Count + " Tests - " + Passed + " Passed - " + Failed + " Failed");
        Console.WriteLine(((Passed / (double)Count) * 100) + "% Passed");

        if (Failed > 0) Assert.Fail(Failed + " Tests failed: " + string.Join("\n", FailedTests.Select(f=>f.ToString())));
        Assert.SkipWhen(Indeterminate > 0, Indeterminate + " Tests are indeterminate");
    }

}