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
using System.Text;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

#pragma warning disable 618

namespace VDS.RDF.Writing;

public class WriterTests
    : CompressionTests
{
    public WriterTests(ITestOutputHelper output): base(output) { }

    [Fact]
    public void WritingBlankNodeOutput()
    {
        //Create a Graph and add a couple of Triples which when serialized have
        //potentially colliding IDs

        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org"));
        IUriNode subj = g.CreateUriNode("ex:subject");
        IUriNode pred = g.CreateUriNode("ex:predicate");
        IUriNode name = g.CreateUriNode("ex:name");
        IBlankNode b1 = g.CreateBlankNode("autos1");
        IBlankNode b2 = g.CreateBlankNode("1");

        g.Assert(subj, pred, b1);
        g.Assert(b1, name, g.CreateLiteralNode("First Triple"));
        g.Assert(subj, pred, b2);
        g.Assert(b2, name, g.CreateLiteralNode("Second Triple"));

        var ttlwriter = new CompressingTurtleWriter();
        ttlwriter.Save(g, "bnode-output-test.ttl");

        var ttlparser = new TurtleParser();
        var h = new Graph();
        ttlparser.Load(h, "bnode-output-test.ttl");

        Assert.Equal(g.Triples.Count, h.Triples.Count);
    }

    [Fact]
    public void WritingOwlCharEscaping()
    {
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "charescaping.owl"));

        var ttlwriter = new CompressingTurtleWriter();
        var serialized = VDS.RDF.Writing.StringWriter.Write(g, ttlwriter);

        var h = new Graph();
        StringParser.Parse(h, serialized);

        Assert.Equal(g, h);
    }

    [Fact]
    public void WritingHtmlWriter()
    {
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        var writer = new HtmlWriter();
        var data = VDS.RDF.Writing.StringWriter.Write(g, writer);

        var h = new Graph
        {
            BaseUri = new Uri("http://example.org")
        };
        StringParser.Parse(h, data, new RdfAParser());

        Assert.Equal(g, h);
    }

    [Fact]
    public void WritingRdfCollections()
    {
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "swrc.owl"));
        var ttlwriter = new CompressingTurtleWriter(WriterCompressionLevel.High);
        ttlwriter.Save(g, Console.Out);
    }

    [Fact]
    public void WritingXmlAmpersandEscaping()
    {
        var inputs = new List<string>()
        {
            "&value",
            "&amp;",
            "&",
            "&value&next",
            "& &squot; < > ' \"",
            new String('&', 1000)
        };

        var outputs = new List<string>()
        {
            "&amp;value",
            "&amp;",
            "&amp;",
            "&amp;value&amp;next",
            "&amp; &squot; &lt; &gt; &apos; &quot;"
        };
        var temp = new StringBuilder();
        for (var i = 0; i < 1000; i++)
        {
            temp.Append("&amp;");
        }
        outputs.Add(temp.ToString());

        for (var i = 0; i < inputs.Count; i++)
        {
            Console.WriteLine("Input: " + inputs[i] + " - Expected Output: " + outputs[i] + " - Actual Output: " + WriterHelper.EncodeForXml(inputs[i]));
            Assert.Equal(outputs[i], WriterHelper.EncodeForXml(inputs[i]));
        }
    }

    [Theory]
    [MemberData(nameof(RoundTripTestData), Formats.All ^ Formats.RdfXml)]
    public void WritingI18NCharacters(IRdfWriter writer, IRdfReader parser)
    {
        var g = new Graph
        {
            BaseUri = new Uri("http://example.org/植物")
        };
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/植物"));
        IUriNode subj = g.CreateUriNode(new Uri("http://example.org/植物/名=しそ;使用部=葉"));
        IUriNode pred = g.CreateUriNode(new Uri("http://example.org/植物#使用部"));
        IUriNode obj = g.CreateUriNode(new Uri("http://example.org/葉"));

        g.Assert(subj, pred, obj);

        RoundTripTest(g, writer, parser);
    }

    [Theory]
    [MemberData(nameof(RoundTripTestData))]
    public void WritingUriEscaping(IRdfWriter writer, IRdfReader parser)
    {
        var g = new Graph
        {
            BaseUri = new Uri("http://example.org/space in/base")
        };
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/space in/namespace"));
        IUriNode subj = g.CreateUriNode(new Uri("http://example.org/subject"));
        IUriNode pred = g.CreateUriNode(new Uri("http://example.org/predicate"));
        IUriNode obj = g.CreateUriNode(new Uri("http://example.org/with%20uri%20escapes"));
        IUriNode obj2 = g.CreateUriNode(new Uri("http://example.org/needs escapes"));

        g.Assert(subj, pred, obj);
        g.Assert(subj, pred, obj2);

        RoundTripTest(g, writer, parser);
    }

    [Theory]
    [MemberData(nameof(RoundTripTestData))]
    public void WritingTripleNodes(IRdfWriter writer, IRdfReader parser)
    {
        if (writer is IRdfStarCapableWriter rdfStarWriter && rdfStarWriter.CanWriteTripleNodes)
        {
            var g = new Graph();
            g.NamespaceMap.AddNamespace("", new Uri("http://example.org/"));
            ITripleNode quoted = new TripleNode(new Triple(g.CreateUriNode(":a"), g.CreateUriNode(":b"),
                g.CreateUriNode(":c")));
            IUriNode subj = g.CreateUriNode(":s");
            IUriNode pred = g.CreateUriNode(":p");
            IUriNode obj = g.CreateUriNode(":o");
            g.Assert(subj, pred, quoted);
            g.Assert(quoted, pred, obj);
            RoundTripTest(g, writer, parser);
        }
    }

    private void RoundTripTest(Graph original, IRdfWriter writer, IRdfReader parser)
    {
        var formatter = new NTriplesFormatter(NTriplesSyntax.Rdf11Star);

        _output.WriteLine("Input Data:");
        foreach (Triple t in original.Triples)
        {
            _output.WriteLine(t.ToString(formatter));
        }
        _output.WriteLine("");

        _output.WriteLine("Serialized Data:");
        var strWriter = new System.IO.StringWriter();
        writer.Save(original, strWriter);
        _output.WriteLine(strWriter.ToString());
        _output.WriteLine("");
        Console.Out.Flush();

        var parsed = new Graph(true);
        parser.Load(parsed, new StringReader(strWriter.ToString()));
        _output.WriteLine("Parsed Data:");
        foreach (Triple t in parsed.Triples)
        {
            _output.WriteLine(t.ToString(formatter));
        }
        _output.WriteLine("");

        Assert.Equal(original, parsed);
        if (parsed.NamespaceMap.HasNamespace("ex"))
        {
            Assert.Equal(original.NamespaceMap.GetNamespaceUri("ex"), parsed.NamespaceMap.GetNamespaceUri("ex"));
        }
        if (parsed.BaseUri != null)
        {
            Assert.Equal(original.BaseUri, parsed.BaseUri);
        }

        Assert.True(original.Difference(parsed).AreEqual);
    }

    [Fact]
    public void WritingQNameValidation()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
        INode subj = g.CreateUriNode("ex:subject");
        INode pred = g.CreateUriNode("ex:predicate");
        var objects = new List<INode>()
        {
            g.CreateUriNode("ex:123"),
            g.CreateBlankNode("a_blank_node"),
            g.CreateBlankNode("_blank"),
            g.CreateBlankNode("-blank"),
            g.CreateBlankNode("123blank"),
            g.CreateUriNode("ex:_object"),
            g.CreateUriNode("ex:-object")
        };
        foreach (INode obj in objects)
        {
            g.Assert(subj, pred, obj);
        }

        CheckCompressionRoundTrip(g);
    }

    [Theory]
    [InlineData(typeof(NTriplesWriter))]
    [InlineData(typeof(RdfXmlWriter))]
    [InlineData(typeof(PrettyRdfXmlWriter))]
    [InlineData(typeof(RdfJsonWriter))]
    [InlineData(typeof(CompressingTurtleWriter))]
    [InlineData(typeof(HtmlWriter))]
    [InlineData(typeof(GraphVizWriter))]
    public void TextWriterCanBeLeftOpen(Type writerType)
    {
        var g = new Graph();
        var writer = new System.IO.StringWriter();
        var rdfWriter = writerType.GetConstructor(new Type[0]).Invoke(new object[0]) as IRdfWriter;
        rdfWriter.Save(g, writer, true);
        writer.Write("\n"); // This should not throw because the writer is still open
        rdfWriter.Save(g, writer);
        Assert.Throws<ObjectDisposedException>(() => writer.Write("\n"));
    }

    public static IEnumerable<TheoryDataRow<IRdfWriter, IRdfReader>> RoundTripTestData()
    {
        return RoundTripTestData(Formats.All);
    }

    public static IEnumerable<TheoryDataRow<IRdfWriter, IRdfReader>> RoundTripTestData(Formats formats)
    {
        if (formats.HasFlag(Formats.Turtle))
        {
            yield return new(new CompressingTurtleWriter(TurtleSyntax.Original), new TurtleParser(TurtleSyntax.Original));
            yield return new(new CompressingTurtleWriter(TurtleSyntax.W3C), new TurtleParser(TurtleSyntax.W3C));
            yield return new(new CompressingTurtleWriter(TurtleSyntax.Rdf11Star), new TurtleParser(TurtleSyntax.Rdf11Star));
            yield return new(new Notation3Writer(), new Notation3Parser());
        }

        if (formats.HasFlag(Formats.RdfXml))
        {
            yield return new(new PrettyRdfXmlWriter(), new RdfXmlParser());
            yield return new(new RdfXmlWriter(), new RdfXmlParser());
            yield return new(new PrettyRdfXmlWriter(), new RdfXmlParser());
        }

        if (formats.HasFlag(Formats.NTriples))
        {
            yield return new(new NTriplesWriter(NTriplesSyntax.Original), new NTriplesParser(NTriplesSyntax.Original));
            yield return new(new NTriplesWriter(NTriplesSyntax.Rdf11), new NTriplesParser(NTriplesSyntax.Rdf11));
            yield return new(new NTriplesWriter(NTriplesSyntax.Rdf11Star), new NTriplesParser(NTriplesSyntax.Rdf11Star));
        }

        if (formats.HasFlag(Formats.RdfA))
        {
            yield return new(new HtmlWriter(), new RdfAParser());
        }


        if (formats.HasFlag(Formats.RdfJson))
        {
            yield return new(new RdfJsonWriter(), new RdfJsonParser());
        }
    }

    [Flags]
    public enum Formats
    {
        Turtle,
        RdfXml,
        RdfA,
        RdfJson,
        NTriples,
        All = ~0,
    }
}
