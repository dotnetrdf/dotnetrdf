/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing;


public class NTriplesTests
{
    private void Test(String literal, IRdfWriter writer, IRdfReader parser)
    {
        IGraph g = new Graph();
        g.NamespaceMap.AddNamespace(String.Empty, UriFactory.Root.Create("http://example/"));
        g.Assert(g.CreateUriNode(":subj"), g.CreateUriNode(":pred"), g.CreateLiteralNode(literal));

        var strWriter = new System.IO.StringWriter();
        writer.Save(g, strWriter);

        Console.WriteLine(strWriter.ToString());

        IGraph h = new Graph();
        parser.Load(h, new StringReader(strWriter.ToString()));

        Assert.Equal(g, h);
    }

    [Fact]
    public void WritingNTriplesAsciiChars1()
    {
        var builder = new StringBuilder();
        for (var i = 0; i <= 127; i++)
        {
            builder.Append((char)i);
        }

        Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Original), new NTriplesParser(NTriplesSyntax.Original));
    }

    [Fact]
    public void WritingNTriplesAsciiChars2()
    {
        var builder = new StringBuilder();
        for (var i = 0; i <= 127; i++)
        {
            builder.Append((char)i);
        }

        Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Rdf11), new NTriplesParser(NTriplesSyntax.Original));
    }

    [Fact]
    public void WritingNTriplesAsciiChars3()
    {
        var builder = new StringBuilder();
        for (var i = 0; i <= 127; i++)
        {
            builder.Append((char)i);
        }

        Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Original), new NTriplesParser(NTriplesSyntax.Rdf11));
    }

    [Fact]
    public void WritingNTriplesAsciiChars4()
    {
        var builder = new StringBuilder();
        for (var i = 0; i <= 127; i++)
        {
            builder.Append((char)i);
        }

        Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Rdf11), new NTriplesParser(NTriplesSyntax.Rdf11));
    }

    [Fact]
    public void WritingNTriplesNonAsciiChars1()
    {
        var builder = new StringBuilder();
        for (var i = 128; i <= 255; i++)
        {
            builder.Append((char) i);
        }

       Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Original), new NTriplesParser(NTriplesSyntax.Original));
    }

    [Fact]
    public void WritingNTriplesNonAsciiChars2()
    {
        var builder = new StringBuilder();
        for (var i = 128; i <= 255; i++)
        {
            builder.Append((char)i);
        }

        Assert.Throws<RdfParseException>(() =>
            Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Rdf11), new NTriplesParser(NTriplesSyntax.Original))
        );
    }

    [Fact]
    public void WritingNTriplesNonAsciiChars3()
    {
        var builder = new StringBuilder();
        for (var i = 128; i <= 255; i++)
        {
            builder.Append((char)i);
        }

        Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Original), new NTriplesParser(NTriplesSyntax.Rdf11));
    }

    [Fact]
    public void WritingNTriplesNonAsciiChars4()
    {
        var builder = new StringBuilder();
        for (var i = 128; i <= 255; i++)
        {
            builder.Append((char)i);
        }

        Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Rdf11), new NTriplesParser(NTriplesSyntax.Rdf11));
    }

    [Fact]
    public void WritingNTriplesMixedChars1()
    {
        var builder = new StringBuilder();
        for (int i = 0, j = 128; i <= 127 && j <= 255; i++, j++)
        {
            builder.Append((char) i);
            builder.Append((char) j);
        }

        Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Original), new NTriplesParser(NTriplesSyntax.Original));
    }

    [Fact]
    public void WritingNTriplesMixedChars2()
    {
        var builder = new StringBuilder();
        var ascii = new Queue<char>(Enumerable.Range(0, 128).Select(c => (char) c));
        var nonAscii = new Queue<char>(Enumerable.Range(128, 128).Select(c => (char) c));

        var i = 1;
        while (ascii.Count > 0)
        {
            for (var x = 0; x < i && ascii.Count > 0; x++)
            {
                builder.Append(ascii.Dequeue());
            }
            for (var x = 0; x < i && nonAscii.Count > 0; x++)
            {
                builder.Append(nonAscii.Dequeue());
            }
            i++;
        }

        Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Original), new NTriplesParser(NTriplesSyntax.Original));
    }

    [Fact]
    public void WritingNTriplesMixedChars3()
    {
        var builder = new StringBuilder();
        for (int i = 0, j = 128; i <= 127 && j <= 255; i++, j++)
        {
            builder.Append((char)i);
            builder.Append((char)j);
        }

        Assert.Throws<RdfParseException>(() =>
            Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Rdf11), new NTriplesParser(NTriplesSyntax.Original))
        );
    }

    [Fact]
    public void WritingNTriplesMixedChars4()
    {
        var builder = new StringBuilder();
        var ascii = new Queue<char>(Enumerable.Range(0, 128).Select(c => (char)c));
        var nonAscii = new Queue<char>(Enumerable.Range(128, 128).Select(c => (char)c));

        var i = 1;
        while (ascii.Count > 0)
        {
            for (var x = 0; x < i && ascii.Count > 0; x++)
            {
                builder.Append(ascii.Dequeue());
            }
            for (var x = 0; x < i && nonAscii.Count > 0; x++)
            {
                builder.Append(nonAscii.Dequeue());
            }
            i++;
        }

        Assert.Throws<RdfParseException>(() =>
            Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Rdf11), new NTriplesParser(NTriplesSyntax.Original))
        );
    }

    [Fact]
    public void WritingNTriplesMixedChars5()
    {
        var builder = new StringBuilder();
        for (int i = 0, j = 128; i <= 127 && j <= 255; i++, j++)
        {
            builder.Append((char)i);
            builder.Append((char)j);
        }

        Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Original), new NTriplesParser(NTriplesSyntax.Rdf11));
    }

    [Fact]
    public void WritingNTriplesMixedChars6()
    {
        var builder = new StringBuilder();
        var ascii = new Queue<char>(Enumerable.Range(0, 128).Select(c => (char)c));
        var nonAscii = new Queue<char>(Enumerable.Range(128, 128).Select(c => (char)c));

        var i = 1;
        while (ascii.Count > 0)
        {
            for (var x = 0; x < i && ascii.Count > 0; x++)
            {
                builder.Append(ascii.Dequeue());
            }
            for (var x = 0; x < i && nonAscii.Count > 0; x++)
            {
                builder.Append(nonAscii.Dequeue());
            }
            i++;
        }

        Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Original), new NTriplesParser(NTriplesSyntax.Rdf11));
    }

    [Fact]
    public void WritingNTriplesMixedChars7()
    {
        var builder = new StringBuilder();
        for (int i = 0, j = 128; i <= 127 && j <= 255; i++, j++)
        {
            builder.Append((char)i);
            builder.Append((char)j);
        }

        Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Rdf11), new NTriplesParser(NTriplesSyntax.Rdf11));
    }

    [Fact]
    public void WritingNTriplesMixedChars8()
    {
        var builder = new StringBuilder();
        var ascii = new Queue<char>(Enumerable.Range(0, 128).Select(c => (char)c));
        var nonAscii = new Queue<char>(Enumerable.Range(128, 128).Select(c => (char)c));

        var i = 1;
        while (ascii.Count > 0)
        {
            for (var x = 0; x < i && ascii.Count > 0; x++)
            {
                builder.Append(ascii.Dequeue());
            }
            for (var x = 0; x < i && nonAscii.Count > 0; x++)
            {
                builder.Append(nonAscii.Dequeue());
            }
            i++;
        }

        Test(builder.ToString(), new NTriplesWriter(NTriplesSyntax.Rdf11), new NTriplesParser(NTriplesSyntax.Rdf11));
    }

    [Fact]
    public void WritingNTriplesStar1()
    {
        var g = new Graph();g.NamespaceMap.AddNamespace("", new Uri("http://example.org/"));
        g.Assert(new Triple(
            g.CreateUriNode(":s"),
            g.CreateUriNode(":p"),
            g.CreateTripleNode(
                new Triple(g.CreateUriNode(":a"),
                    g.CreateUriNode(":b"),
                    g.CreateUriNode(":c")))));
        var writer = new NTriplesWriter(NTriplesSyntax.Rdf11Star);
        var strWriter = new System.IO.StringWriter();
        writer.Save(g, strWriter);
        strWriter.ToString().Should().Be(
            "<http://example.org/s> <http://example.org/p> << <http://example.org/a> <http://example.org/b> <http://example.org/c> >> ." + Environment.NewLine);
    }

    [Fact]
    public void WritingNTriplesStar2()
    {
        var g = new Graph(); g.NamespaceMap.AddNamespace("", new Uri("http://example.org/"));
        g.Assert(new Triple(
            g.CreateTripleNode(
                new Triple(g.CreateUriNode(":a"),
                    g.CreateUriNode(":b"),
                    g.CreateUriNode(":c"))),
            g.CreateUriNode(":p"),
            g.CreateUriNode(":o")
        ));
        var writer = new NTriplesWriter(NTriplesSyntax.Rdf11Star);
        var strWriter = new System.IO.StringWriter();
        writer.Save(g, strWriter);
        strWriter.ToString().Should().Be(
            "<< <http://example.org/a> <http://example.org/b> <http://example.org/c> >> <http://example.org/p> <http://example.org/o> ." + Environment.NewLine);
    }

    [Fact]
    public void WritingNTriplesStar3()
    {
        var g = new Graph(); g.NamespaceMap.AddNamespace("", new Uri("http://example.org/"));
        g.Assert(new Triple(
            g.CreateTripleNode(
                new Triple(g.CreateUriNode(":a"),
                    g.CreateUriNode(":b"),
                    g.CreateUriNode(":c"))),
            g.CreateUriNode(":p"),
            g.CreateUriNode(":o")
        ));
        var writer = new NTriplesWriter(NTriplesSyntax.Rdf11Star);
        var strWriter = new System.IO.StringWriter();
        writer.Save(g, strWriter);
        strWriter.ToString().Should().Be(
            "<< <http://example.org/a> <http://example.org/b> <http://example.org/c> >> <http://example.org/p> <http://example.org/o> ." + Environment.NewLine);
    }

    [Fact]
    public void WritingNTriplesStar4()
    {
        var g = new Graph(); g.NamespaceMap.AddNamespace("", new Uri("http://example.org/"));
        g.Assert(new Triple(
            g.CreateTripleNode(
                new Triple(g.CreateUriNode(":a"),
                    g.CreateUriNode(":b"),
                    g.CreateTripleNode(
                        new Triple(
                            g.CreateUriNode(":c"),
                            g.CreateUriNode(":d"),
                            g.CreateUriNode(":e")))
                    )),
            g.CreateUriNode(":p"),
            g.CreateUriNode(":o")
        ));
        var writer = new NTriplesWriter(NTriplesSyntax.Rdf11Star);
        var strWriter = new System.IO.StringWriter();
        writer.Save(g, strWriter);
        strWriter.ToString().Should().Be(
            "<< <http://example.org/a> <http://example.org/b> << <http://example.org/c> <http://example.org/d> <http://example.org/e> >> >> <http://example.org/p> <http://example.org/o> ." + Environment.NewLine);
    }

    [Fact]
    public void WritingNTriplesStar5()
    {
        var g = new Graph(); g.NamespaceMap.AddNamespace("", new Uri("http://example.org/"));
        g.Assert(new Triple(
            g.CreateUriNode(":s"),
            g.CreateUriNode(":p"),
            g.CreateTripleNode(
                new Triple(g.CreateUriNode(":a"),
                    g.CreateUriNode(":b"),
                    g.CreateUriNode(":c")))));
        var writer = new NTriplesWriter(NTriplesSyntax.Rdf11);
        var strWriter = new System.IO.StringWriter();
        RdfOutputException ex = Assert.Throws<RdfOutputException>(() => writer.Save(g, strWriter));
        ex.Message.Should().Be(WriterErrorMessages.TripleNodesUnserializable("NTriples (RDF 1.1)"));
    }

    private static void TestBNodeFormatting(IBlankNode b, INodeFormatter formatter, String expected)
    {
        var actual = formatter.Format(b);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void WritingNTriplesBlankNodeIDs1()
    {
        var formatter = new NTriplesFormatter(NTriplesSyntax.Original);
        var g = new Graph();

        // Simple IDs which are valid in Original NTriples and RDF 1.1 NTriples
        IBlankNode b = g.CreateBlankNode("simple");
        TestBNodeFormatting(b, formatter, "_:simple");
        b = g.CreateBlankNode("simple1234");
        TestBNodeFormatting(b, formatter, "_:simple1234");

        // Complex IDs which are only valid in RDF 1.1 NTriples
        // When using Original syntax these should be rewritten
        b = g.CreateBlankNode("complex-dash");
        TestBNodeFormatting(b, formatter, "_:autos1");
        b = g.CreateBlankNode("complex_underscore");
        TestBNodeFormatting(b, formatter, "_:autos2");
        b = g.CreateBlankNode("complex.dot");
        TestBNodeFormatting(b, formatter, "_:autos3");
        b = g.CreateBlankNode("complex-dash_underscore.dot");
        TestBNodeFormatting(b, formatter, "_:autos4");
        b = g.CreateBlankNode("комплекс");
        TestBNodeFormatting(b, formatter, "_:autos5");
    }

    [Fact]
    public void WritingNTriplesBlankNodeIDs2()
    {
        var formatter = new NTriplesFormatter(NTriplesSyntax.Rdf11);
        var g = new Graph();

        // Simple IDs which are valid in Original NTriples and RDF 1.1 NTriples
        IBlankNode b = g.CreateBlankNode("simple");
        TestBNodeFormatting(b, formatter, "_:simple");
        b = g.CreateBlankNode("simple1234");
        TestBNodeFormatting(b, formatter, "_:simple1234");

        // Complex IDs which are only valid in RDF 1.1 NTriples
        // When using RDF 1.1 syntax these will be left as-is
        b = g.CreateBlankNode("complex-dash");
        TestBNodeFormatting(b, formatter, "_:complex-dash");
        b = g.CreateBlankNode("complex_underscore");
        TestBNodeFormatting(b, formatter, "_:complex_underscore");
        b = g.CreateBlankNode("complex.dot");
        TestBNodeFormatting(b, formatter, "_:complex.dot");
        b = g.CreateBlankNode("complex-dash_underscore.dot");
        TestBNodeFormatting(b, formatter, "_:complex-dash_underscore.dot");
        b = g.CreateBlankNode("комплекс");
        TestBNodeFormatting(b, formatter, "_:комплекс");
    }
}
