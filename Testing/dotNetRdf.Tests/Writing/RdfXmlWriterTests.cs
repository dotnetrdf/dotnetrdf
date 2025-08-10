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
using System.IO.Compression;
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing;

public class RdfXmlWriterTests
{
    private readonly List<IRdfWriter> _writers = new List<IRdfWriter>()
    {
        new RdfXmlWriter(WriterCompressionLevel.High),
        new RdfXmlWriter(WriterCompressionLevel.High, false),
        new PrettyRdfXmlWriter(WriterCompressionLevel.High),
        new PrettyRdfXmlWriter(WriterCompressionLevel.High, false),
        new PrettyRdfXmlWriter(WriterCompressionLevel.High, true, false)
    };

    private readonly IRdfReader _parser = new RdfXmlParser();
    private readonly NTriplesFormatter _formatter = new NTriplesFormatter();
    private readonly ITestOutputHelper _output;

    public RdfXmlWriterTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private void CheckRoundTrip(IGraph g, IEnumerable<Type> exceptions)
    {
        foreach (IRdfWriter writer in _writers)
        {
            _output.WriteLine("Checking round trip with " + writer.GetType().Name);
            var strWriter = new System.IO.StringWriter();
            writer.Save(g, strWriter);
            _output.WriteLine(string.Empty);

            var h = new Graph();
            try
            {
                _parser.Load(h, new StringReader(strWriter.ToString()));
                _output.WriteLine("Output was parsed OK");
            }
            catch
            {
                _output.WriteLine("Invalid Output:");
                _output.WriteLine(strWriter.ToString());
                throw;
            }
            _output.WriteLine(string.Empty);

            if (exceptions.Contains(writer.GetType()))
            {
                _output.WriteLine("Graph Equality test was skipped");
            }
            else
            {
                try
                {
                    Assert.Equal(g, h);
                }
                catch
                {
                    _output.WriteLine("Output did not round trip:");
                    _output.WriteLine(strWriter.ToString());
                    throw;
                }
                _output.WriteLine("Graphs are equal");
            }
        }
    }

    private void CheckRoundTrip(IGraph g)
    {
        CheckRoundTrip(g, Enumerable.Empty<Type>());
    }

    private void CheckFailure(IGraph g)
    {
        _output.WriteLine("Original Triples:");
        foreach (Triple t in g.Triples)
        {
            _output.WriteLine(t.ToString(_formatter));
        }
        _output.WriteLine(string.Empty);

        foreach (IRdfWriter writer in _writers)
        {
            _output.WriteLine("Checking for Failure with " + writer.GetType().Name);
            var failed = false;
            try
            {
                var sw = new System.IO.StringWriter();
                writer.Save(g, sw);

                _output.WriteLine("Produced Output when failure was expected:");
                _output.WriteLine(sw.ToString());

                failed = true;
            }
            catch (Exception ex)
            {
                _output.WriteLine("Failed as expected - " + ex.Message);
            }
            if (failed) Assert.Fail(writer.GetType().Name + " produced output when failure was expected");
            _output.WriteLine(string.Empty);
        }
    }

    [Fact]
    public void WritingRdfXmlLiteralsWithLanguageTags()
    {
        var g = new Graph();
        INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
        INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
        INode o = g.CreateLiteralNode("string", "en");
        g.Assert(s, p, o);

        CheckRoundTrip(g);
    }

    [Fact]
    public void WritingRdfXmlLiteralsWithReservedCharacters()
    {
        var g = new Graph();
        INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
        INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
        INode o = g.CreateLiteralNode("<tag>");
        g.Assert(s, p, o);

        CheckRoundTrip(g);
    }

    [Fact]
    public void WritingRdfXmlLiteralsWithReservedCharacters2()
    {
        var g = new Graph();
        INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
        INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
        INode o = g.CreateLiteralNode("&lt;tag>");
        g.Assert(s, p, o);

        CheckRoundTrip(g, new Type[] { typeof(PrettyRdfXmlWriter) });
    }

    [Fact]
    public void WritingRdfXmlLiteralsWithReservedCharacters3()
    {
        var g = new Graph();
        INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
        INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
        INode o = g.CreateLiteralNode("string", new Uri("http://example.org/object?a=b&c=d"));
        g.Assert(s, p, o);

        CheckRoundTrip(g);
    }

    [Fact]
    public void WritingRdfXmlLiterals()
    {
        var g = new Graph();
        INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
        INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
        INode o = g.CreateLiteralNode("<tag />", new Uri(RdfSpecsHelper.RdfXmlLiteral));
        g.Assert(s, p, o);

        CheckRoundTrip(g);
    }

    [Fact]
    public void WritingRdfXmlLiterals2()
    {
        var g = new Graph();
        INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
        INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
        INode o = g.CreateLiteralNode("<tag>this &amp; that</tag>", new Uri(RdfSpecsHelper.RdfXmlLiteral));
        g.Assert(s, p, o);

        CheckRoundTrip(g);
    }

    [Fact]
    public void WritingRdfXmlUrisWithReservedCharacters()
    {
        var g = new Graph();
        INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
        INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
        INode o = g.CreateUriNode(new Uri("http://example.org/object?a=b&c=d"));
        g.Assert(s, p, o);

        CheckRoundTrip(g);
    }

    [Fact]
    public void WritingRdfXmlBNodes1()
    {
        var g = new Graph();
        INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
        INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
        INode o = g.CreateBlankNode();
        g.Assert(s, p, o);

        s = o;
        p = g.CreateUriNode(new Uri("http://example.org/nextPredicate"));
        o = g.CreateLiteralNode("string");

        g.Assert(s, p, o);

        CheckRoundTrip(g);
    }

    [Fact]
    public void WritingRdfXmlBNodes2()
    {
        var data = "@prefix : <http://example.org/>. [a :bNode ; :connectsTo [a :bNode ; :connectsTo []]] a [] .";
        var g = new Graph();
        g.LoadFromString(data, new TurtleParser());

        CheckRoundTrip(g);
    }

    [Fact]
    public void WritingRdfXmlBNodes3()
    {
        var g = new Graph();
        INode s = g.CreateUriNode(new Uri("http://example.org/subject"));
        INode p = g.CreateUriNode(new Uri("http://example.org/predicate"));
        INode o = g.CreateBlankNode("foo");
        g.Assert(s, p, o);

        s = o;
        p = g.CreateUriNode(new Uri("http://example.org/nextPredicate"));
        o = g.CreateLiteralNode("string");

        g.Assert(s, p, o);

        CheckRoundTrip(g);
    }

    [Fact]
    public void WritingRdfXmlSimpleBNodeCollection()
    {
        var fragment = "@prefix : <http://example.org/>. :subj :pred [ :something :else ].";

        var g = new Graph();
        g.LoadFromString(fragment);

        CheckRoundTrip(g);
    }

    [Fact]
    public void WritingRdfXmlSimpleBNodeCollection2()
    {
        var fragment = "@prefix : <http://example.org/>. :subj :pred [ :something :else ; :another :thing ].";

        var g = new Graph();
        g.LoadFromString(fragment);

        CheckRoundTrip(g);
    }

    [Fact]
    public void WritingRdfXmlSimpleBNodeCollection3()
    {
        var fragment = "@prefix : <http://example.org/>. :subj :pred [ a :BNode ; :another :thing ].";

        var g = new Graph();
        g.LoadFromString(fragment);

        CheckRoundTrip(g);
    }

    [Fact]
    public void WritingRdfXmlInvalidPredicates1()
    {
        var fragment = "@prefix ex: <http://example.org/>. ex:subj ex:123 ex:object .";
        var g = new Graph();
        g.LoadFromString(fragment);

        CheckFailure(g);
    }

    [Fact]
    public void WritingRdfXmlPrettySubjectCollection1()
    {
        var graph = @"@prefix ex: <http://example.com/>. (1) ex:someProp ""Value"".";
        var g = new Graph();
        g.LoadFromString(graph, new TurtleParser());

        var writer = new PrettyRdfXmlWriter();
        var strWriter = new System.IO.StringWriter();
        writer.Save(g, strWriter);

        _output.WriteLine(strWriter.ToString());
        _output.WriteLine(string.Empty);

        var h = new Graph();
        h.LoadFromString(strWriter.ToString(), new RdfXmlParser());

        Assert.Equal(g, h);
    }

    [Fact]
    public void WritingRdfXmlEntityCompactionLeadingDigits()
    {
        const String data = "@prefix ex: <http://example.org/> . ex:1s ex:p ex:2o .";
        var g = new Graph();
        g.LoadFromString(data, new TurtleParser());

        var writer = new RdfXmlWriter(WriterCompressionLevel.High);
        var outData = StringWriter.Write(g, writer);

        Assert.Contains("rdf:about=\"&ex;1s\"", outData);
        Assert.Contains("rdf:resource=\"&ex;2o\"", outData);
    }

    /// <summary>
    /// Reproduces issue #243
    /// </summary>
    [Fact]
    public void RdfXmlEntityEscapesApostrophes()
    {
        const string data =
            "@prefix ex: <http://dbpedia.org/resource/Buyer's_Remorse:> . ex:s <http://example.org/p> <http://example.org/o> .";
        var g = new Graph();
        g.LoadFromString(data, new TurtleParser());

        var writer = new RdfXmlWriter {CompressionLevel = WriterCompressionLevel.High};
        var outData = StringWriter.Write(g, writer);
        Assert.Contains("<!ENTITY ex 'http://dbpedia.org/resource/Buyer&apos;s_Remorse:'>", outData);
        Assert.Contains("xmlns:ex=\"http://dbpedia.org/resource/Buyer&apos;s_Remorse:\"", outData);
        Assert.Contains("rdf:about=\"&ex;s", outData);
    }

    [Fact]
    public void WritingRdfXmlSimpleCollection()
    {
        var fragment = "@prefix : <http://example.org/>. :subj :pred ( 1 2 3 ).";

        var g = new Graph();
        g.LoadFromString(fragment);

        CheckRoundTrip(g);
    }

    [Fact]
    [Trait("Coverage", "Skip")]
    public void WritingRdfXmlComplex()
    {
        var g = new Graph();
        var parser = new TurtleParser();
        parser.Load(new PagingHandler(new GraphHandler(g), 1000), Path.Combine("resources", "chado-in-owl.ttl"));

        CheckRoundTrip(g);
    }

    [Fact]
    public void WritingRdfXmlWithDtds()
    {
        var fragment = "@prefix xsd: <" + NamespaceMapper.XMLSCHEMA + ">. @prefix : <http://example.org/>. :subj a :obj ; :has \"string\"^^xsd:string ; :has 23 .";
        var g = new Graph();
        g.LoadFromString(fragment);

        CheckRoundTrip(g);
    }

    [Fact]
    public void PredicateQNameReductionWithExistingNamespace()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
        g.NamespaceMap.AddNamespace("u", new Uri("urn:uuid:"));
        g.Assert(new Triple(g.CreateUriNode("ex:s"), g.CreateUriNode("u:255fed9d-7b10-44e1-8b46-2cabca0706ac"),
            g.CreateUriNode("ex:p")));
        g.Assert(new Triple(g.CreateUriNode("ex:s"), g.CreateUriNode("u:e1601555-4367-4c69-982b-b7375fbf76b6"),
            g.CreateUriNode("ex:p")));
        CheckRoundTrip(g);
    }
}
