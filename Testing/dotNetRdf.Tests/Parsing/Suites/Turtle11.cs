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

using FluentAssertions;
using System;
using System.IO;
using System.Linq;
using VDS.RDF.Writing.Formatting;
using Xunit;

namespace VDS.RDF.Parsing.Suites;


public class Turtle11Unofficial
    : BaseRdfParserSuite
{
    public Turtle11Unofficial()
        : base(new TurtleParser(TurtleSyntax.W3C, false), new NTriplesParser(), "turtle11-unofficial") { }

    [Fact]
    public void ParsingSuiteTurtleW3CUnofficialTests()
    {
        //Run manifests
        RunManifest("resources/turtle11-unofficial/manifest.ttl", true);
        RunManifest("resources/turtle11-unofficial/manifest-bad.ttl", false);

        if (Count == 0) Assert.Fail("No tests found");

        Console.WriteLine(Count + " Tests - " + Passed + " Passed - " + Failed + " Failed");
        Console.WriteLine(((Passed / (double)Count) * 100) + "% Passed");

        if (Failed > 0) Assert.Fail(Failed + " Tests failed");
        Assert.SkipWhen(Indeterminate > 0, Indeterminate + " Tests are indeterminate");
    }
}



public class Turtle11
    : BaseRdfParserSuite
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Turtle11(ITestOutputHelper testOutputHelper)
        : base(new TurtleParser(TurtleSyntax.W3C, true), new NTriplesParser(NTriplesSyntax.Rdf11), "turtle11")
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void ParsingSuiteTurtleW3C()
    {
        //Nodes for positive and negative tests
        var g = new Graph();
        g.NamespaceMap.AddNamespace("rdft", UriFactory.Root.Create("http://www.w3.org/ns/rdftest#"));
        INode posSyntaxTest = g.CreateUriNode("rdft:TestTurtlePositiveSyntax");
        INode negSyntaxTest = g.CreateUriNode("rdft:TestTurtleNegativeSyntax");
        INode negEvalTest = g.CreateUriNode("rdft:TestTurtleNegativeEval");

        //Run manifests
        RunManifest(Path.Combine("resources", "turtle11", "manifest.ttl"), new[] { posSyntaxTest }, new[] { negSyntaxTest, negEvalTest });

        if (Count == 0) Assert.Fail("No tests found");

        _testOutputHelper.WriteLine(Count + " Tests - " + Passed + " Passed - " + Failed + " Failed - " + Indeterminate + " Indeterminate");
        _testOutputHelper.WriteLine(((Passed / (double)Count) * 100) + "% Passed");

        if (Failed > 0)
        {
            if (Indeterminate == 0)
            {
                Assert.Fail(Failed + " Tests failed and " + Passed + " Tests Passed");
            }
            else
            {
                Assert.Fail(Failed + " Test failed, " + Indeterminate + " Tests are indeterminate and " + Passed + " Tests Passed");
            }
        }
        Assert.SkipWhen(Indeterminate > 0, Indeterminate + " Tests are indeterminate and " + Passed + " Tests Passed");
    }

    [Fact]
    public void ParsingTurtleW3CComplexPrefixedNames1()
    {
        var input = "AZazÀÖØöø˿ͰͽͿ῿‌‍⁰↏Ⰰ⿯、퟿豈﷏ﷰ�𐀀�:";
        Assert.True(TurtleSpecsHelper.IsValidPrefix(input, TurtleSyntax.W3C));
    }

    [Fact]
    public void ParsingTurtleW3CComplexPrefixedNames2()
    {
        var input = "AZazÀÖØöø˿ͰͽͿ῿‌‍⁰↏Ⰰ⿯、퟿豈﷏ﷰ�𐀀�:o";
        Assert.True(TurtleSpecsHelper.IsValidQName(input, TurtleSyntax.W3C));
    }

    [Fact]
    public void ParsingTurtleW3CComplexPrefixedNames3()
    {
        var input = ":a~b";
        Assert.False(TurtleSpecsHelper.IsValidQName(input, TurtleSyntax.W3C));
    }

    [Fact]
    public void ParsingTurtleW3CComplexPrefixedNames4()
    {
        var input = ":a%b";
        Assert.False(TurtleSpecsHelper.IsValidQName(input, TurtleSyntax.W3C));
    }

    [Fact]
    public void ParsingTurtleW3CComplexPrefixedNames5()
    {
        var input = @":a\~b";
        Assert.True(TurtleSpecsHelper.IsValidQName(input, TurtleSyntax.W3C));
    }

    [Fact]
    public void ParsingTurtleW3CComplexPrefixedNames6()
    {
        var input = ":a%bb";
        Assert.True(TurtleSpecsHelper.IsValidQName(input, TurtleSyntax.W3C));
    }

    [Fact]
    public void ParsingTurtleW3CComplexPrefixedNames7()
    {
        var input = @":\~";
        Assert.True(TurtleSpecsHelper.IsValidQName(input, TurtleSyntax.W3C));
    }

    [Fact]
    public void ParsingTurtleW3CComplexPrefixedNames8()
    {
        var input = ":%bb";
        Assert.True(TurtleSpecsHelper.IsValidQName(input, TurtleSyntax.W3C));
    }

    [Fact]
    public void ParsingTurtleW3CComplexPrefixedNames9()
    {
        var input = @"p:AZazÀÖØöø˿Ͱͽ΄῾‌‍⁰↉Ⰰ⿕、ퟻ﨎ﷇﷰ￯𐀀󠇯";
        Assert.True(TurtleSpecsHelper.IsValidQName(input, TurtleSyntax.W3C));
    }

    [Fact]
    public void ParsingTurtleW3CComplexPrefixedNames10()
    {
        var formatter = new NTriplesFormatter();

        var ttl = new Graph();
        ttl.LoadFromFile(Path.Combine("resources", "turtle11", "localName_with_non_leading_extras.ttl"), new TurtleParser(TurtleSyntax.W3C, false));
        Assert.False(ttl.IsEmpty);
        Console.WriteLine("Subject from Turtle: " + ttl.Triples.First().Subject.ToString(formatter));

        var nt = new Graph();
        var parser = new NTriplesParser();
        parser.Warning += TestTools.WarningPrinter;
        nt.LoadFromFile(Path.Combine("resources", "turtle11", "localName_with_non_leading_extras.nt"), parser);
        Assert.False(nt.IsEmpty);
        Console.WriteLine("Subject from NTriples: " + nt.Triples.First().Subject.ToString(formatter));

        Assert.Equal(ttl.Triples.First().Subject, nt.Triples.First().Subject);
    }

    [Fact]
    public void ParsingTurtleW3CNumericLiterals1()
    {
        var input = "123.E+1";
        Assert.True(TurtleSpecsHelper.IsValidDouble(input));
    }

    [Fact]
    public void ParsingTurtleW3CNumericLiterals2()
    {
        var input = @"@prefix : <http://example.org/> .
:subject :predicate 123.E+1.";
        var g = new Graph();
        g.LoadFromString(input, new TurtleParser(TurtleSyntax.W3C, false));
        Assert.False(g.IsEmpty);
        Assert.Equal(1, g.Triples.Count);
    }

    [Fact]
    public void ParsingTurtleW3CLiteralEscapes1()
    {
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "turtle11", "literal_with_escaped_BACKSPACE.ttl"));
        Assert.False(g.IsEmpty);
        Assert.Equal(1, g.Triples.Count);
        Triple t = g.Triples.First();
        Assert.Equal(NodeType.Literal, t.Object.NodeType);
        var lit = (ILiteralNode)t.Object;
        Assert.Equal(1, lit.Value.Length);
    }

    [Fact]
    public void ParsingTurtleW3CComplexLiterals1()
    {
        var formatter = new NTriplesFormatter();

        var ttl = new Graph();
        ttl.LoadFromFile(Path.Combine("resources", "turtle11", "LITERAL1_ascii_boundaries.ttl"));
        Assert.False(ttl.IsEmpty);
        Console.WriteLine("Object from Turtle: " + ttl.Triples.First().Object.ToString(formatter));

        var nt = new Graph();
        nt.LoadFromFile(Path.Combine("resources", "turtle11", "LITERAL1_ascii_boundaries.nt"));
        Assert.False(nt.IsEmpty);
        Console.WriteLine("Object from NTriples: " + nt.Triples.First().Object.ToString(formatter));

        Assert.Equal(ttl.Triples.First().Object, nt.Triples.First().Object);
    }

    [Fact]
    public void ParsingTurtleW3CComplexLiterals2()
    {
        var g = new Graph();

        Assert.Throws<RdfParseException>(() => g.LoadFromFile(Path.Combine("resources", "turtle11", "turtle-syntax-bad-string-04.ttl")));
    }

    [Fact]
    public void ParsingTurtleW3CBaseTurtleStyle1()
    {
        //Dot required
        var graph = "@base <http://example.org/> .";
        var g = new Graph();
        Parser.Load(g, new StringReader(graph));

        Assert.Equal(new Uri("http://example.org"), g.BaseUri);
    }

    [Fact]
    public void ShouldThrowWhenTurtleStyleBaseIsMissingDot()
    {
        //Missing dot
        var graph = "@base <http://example.org/>";
        var g = new Graph();
        Assert.Throws<RdfParseException>(() => Parser.Load(g, new StringReader(graph)));

        Assert.Equal(new Uri("http://example.org"), g.BaseUri);
    }

    [Fact]
    public void ParsingTurtleW3CBaseTurtleStyle3()
    {
        //@base is case sensitive in Turtle
        var graph = "@BASE <http://example.org/> .";
        var g = new Graph();

        Assert.Throws<RdfParseException>(() => Parser.Load(g, new StringReader(graph)));
    }

    [Fact]
    public void ParsingTurtleW3CBaseSparqlStyle1()
    {
        //Forbidden dot
        var graph = "BASE <http://example.org/> .";
        var g = new Graph();
        Assert.Throws<RdfParseException>(() =>  Parser.Load(g, new StringReader(graph)));

        Assert.Equal(new Uri("http://example.org"), g.BaseUri);
    }

    [Fact]
    public void ShouldSuccessfullyParseValidSparqlStyleW3CBase()
    {
        //No dot required
        var graph = "BASE <http://example.org/>";
        var g = new Graph();
        Parser.Load(g, new StringReader(graph));

        Assert.Equal(new Uri("http://example.org"), g.BaseUri);
    }

    [Fact]
    public void ParsingTurtleW3CBaseSparqlStyle3()
    {
        //No dot required and case insensitive
        var graph = "BaSe <http://example.org/>";
        var g = new Graph();
        Parser.Load(g, new StringReader(graph));

        Assert.Equal(new Uri("http://example.org"), g.BaseUri);
    }

    [Fact]
    public void ParsingTurtleW3CPrefixTurtleStyle1()
    {
        //Dot required
        var graph = "@prefix ex: <http://example.org/> .";
        var g = new Graph();
        Parser.Load(g, new StringReader(graph));

        Assert.Equal(new Uri("http://example.org"), g.NamespaceMap.GetNamespaceUri("ex"));
    }

    [Fact]
    public void ShouldThrowWhenTurtleStylePrefixIsMissingDot()
    {
        //Missing dot
        var graph = "@prefix ex: <http://example.org/>";
        var g = new Graph();
        Assert.Throws<RdfParseException>(() => Parser.Load(g, new StringReader(graph)));

        Assert.Equal(new Uri("http://example.org"), g.NamespaceMap.GetNamespaceUri("ex"));
    }

    [Fact]
    public void ParsingTurtleW3CPrefixTurtleStyle3()
    {
        //@prefix is case sensitive in Turtle
        var graph = "@PREFIX ex: <http://example.org/> .";
        var g = new Graph();

        Assert.Throws<RdfParseException>(() => Parser.Load(g, new StringReader(graph)));
    }

    [Fact]
    public void ParsingTurtleW3CPrefixSparqlStyle1()
    {
        //Forbidden dot
        var graph = "PREFIX ex: <http://example.org/> .";
        var g = new Graph();
        Assert.Throws<RdfParseException>(() => Parser.Load(g, new StringReader(graph)));

        Assert.Equal(new Uri("http://example.org"), g.NamespaceMap.GetNamespaceUri("ex"));
    }

    [Fact]
    public void ParsingTurtleW3CPrefixSparqlStyle3()
    {
        //No dot required and case insensitive
        var graph = "PrEfIx ex: <http://example.org/>";
        var g = new Graph();
        Parser.Load(g, new StringReader(graph));

        Assert.Equal(new Uri("http://example.org"), g.NamespaceMap.GetNamespaceUri("ex"));
    }

    [Fact]
    public void ParsingTurtlePrefixDeclarations1()
    {
        // No white space between prefix and URI
        const String graph = "@prefix pre:<http://example.org> .";
        var g = new Graph();
        Parser.Load(g, new StringReader(graph));

        Assert.Equal(new Uri("http://example.org"), g.NamespaceMap.GetNamespaceUri("pre"));
    }

    [Fact]
    public void ParsingTurtlePrefixDeclarations2()
    {
        const String graph = "@prefix pre: <http://example.org> .";
        var g = new Graph();
        Parser.Load(g, new StringReader(graph));

        Assert.Equal(new Uri("http://example.org"), g.NamespaceMap.GetNamespaceUri("pre"));
    }

    [Fact]
    public void ParsingTurtlePrefixDeclarations3()
    {
        // Multiple : are not supported
        const String graph = "@prefix pre:pre: <http://example.org> .";
        var g = new Graph();
        Assert.Throws<RdfParseException>(() => Parser.Load(g, new StringReader(graph)));
    }

    [Fact]
    public void ParsingTurtleLiteralEscapes1()
    {
        const String data = @"<http://s> <http://p> ""literal\'quote"" .";

        var g = new Graph();
        Assert.Throws<RdfParseException>(() => g.LoadFromString(data, new TurtleParser(TurtleSyntax.Original, false)));
    }

    [Fact]
    public void ParsingTurtleLiteralEscapes2()
    {
        const String data = @"<http://s> <http://p> ""literal\""quote"" .";

        var g = new Graph();
        g.LoadFromString(data, new TurtleParser(TurtleSyntax.Original, false));

        Assert.False(g.IsEmpty);
        Assert.Equal(1, g.Triples.Count);
    }

    [Fact]
    public void ParsingTurtleW3CLiteralEscapes2()
    {
        const String data = @"<http://s> <http://p> ""literal\'quote"" .";

        var g = new Graph();
        g.LoadFromString(data, Parser);

        Assert.False(g.IsEmpty);
        Assert.Equal(1, g.Triples.Count);
    }

    [Fact]
    public void ParsingTurtleW3CLiteralEscapes3()
    {
        const String data = @"<http://s> <http://p> ""literal\""quote"" .";

        var g = new Graph();
        g.LoadFromString(data, Parser);

        Assert.False(g.IsEmpty);
        Assert.Equal(1, g.Triples.Count);
    }
}

public class TurtleStar11 : BaseRdfParserSuite
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TurtleStar11(ITestOutputHelper testOutputHelper)
        : base(new TurtleParser(TurtleSyntax.Rdf11Star, true), new NTriplesParser(NTriplesSyntax.Rdf11Star), "turtle11")
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void ParsingSuiteTurtleW3C()
    {
        //Nodes for positive and negative tests
        var g = new Graph();
        g.NamespaceMap.AddNamespace("rdft", UriFactory.Root.Create("http://www.w3.org/ns/rdftest#"));
        INode posSyntaxTest = g.CreateUriNode("rdft:TestTurtlePositiveSyntax");
        INode negSyntaxTest = g.CreateUriNode("rdft:TestTurtleNegativeSyntax");
        INode negEvalTest = g.CreateUriNode("rdft:TestTurtleNegativeEval");

        //Run manifests
        RunManifest(Path.Combine("resources", "turtle11", "manifest.ttl"), new[] { posSyntaxTest }, new[] { negSyntaxTest, negEvalTest });

        if (Count == 0) Assert.Fail("No tests found");

        _testOutputHelper.WriteLine(Count + " Tests - " + Passed + " Passed - " + Failed + " Failed - " + Indeterminate + " Indeterminate");
        _testOutputHelper.WriteLine(((Passed / (double)Count) * 100) + "% Passed");

        if (Failed > 0)
        {
            if (Indeterminate == 0)
            {
                Assert.Fail(Failed + " Tests failed and " + Passed + " Tests Passed\n\t" + string.Join("\n\t", FailedTests));
            }
            else
            {
                Assert.Fail(Failed + " Test failed, " + Indeterminate + " Tests are indeterminate and " + Passed + " Tests Passed\n\t" + string.Join("\n\t", FailedTests));
            }
        }
        Assert.SkipWhen(Indeterminate > 0, Indeterminate + " Tests are indeterminate and " + Passed + " Tests Passed");
    }

    [Fact]
    public void TestReservedEscapedLocalName()
    {
        var g = new Graph();
        g.LoadFromString(@"@prefix p: <http://a.example/>.
p:\_\~\.\-\!\$\&\'\(\)\*\+\,\;\=\/\?\#\@\%00 <http://a.example/p> <http://a.example/o> .",
            new TurtleParser(TurtleSyntax.Rdf11Star, false));
        var uriNode = g.Triples.First().Subject.Should().BeAssignableTo<IUriNode>().Subject;
        uriNode.Uri.AbsoluteUri.Should()
            .Be("http://a.example/_~.-!$&'()*+,;=/?#@%00");
    }
}
