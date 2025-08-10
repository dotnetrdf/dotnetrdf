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
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Contexts;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Writing;

public class CollectionCompressionTests
    : CompressionTests
{
    public CollectionCompressionTests(ITestOutputHelper output):base(output)
    {
    }

    [Fact(Skip = "Commented out before, now fails (?)")]
    public void WritingCollectionCompressionSimple7()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
        INode n = g.CreateBlankNode();
        INode rdfType = g.CreateUriNode("rdf:type");

        g.Assert(n, rdfType, g.CreateUriNode("ex:Obj"));
        g.Assert(n, rdfType, g.CreateUriNode("ex:Test"));

        var collections = FindCollections(g);

        Assert.Single(collections);
        Assert.Single(collections.First().Value.Triples);

        CheckCompressionRoundTrip(g);
    }

    [Fact]
    public void WritingCollectionCompressionNamedListNodes3()
    {
        var g = new Graph();
        INode data1 = g.CreateBlankNode();
        g.Assert(data1, g.CreateUriNode(new Uri("http://property")), g.CreateLiteralNode("test1"));
        INode data2 = g.CreateBlankNode();
        g.Assert(data2, g.CreateUriNode(new Uri("http://property")), g.CreateLiteralNode("test2"));

        INode listEntry1 = g.CreateUriNode(new Uri("http://test/1"));
        INode rdfFirst = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListFirst));
        INode rdfRest = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListRest));
        INode rdfNil = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfListNil));
        g.Assert(listEntry1, rdfFirst, data1);
        g.Assert(listEntry1, rdfRest, rdfNil);

        INode listEntry2 = g.CreateUriNode(new Uri("http://test/2"));
        g.Assert(listEntry2, rdfFirst, data2);
        g.Assert(listEntry2, rdfRest, listEntry1);

        INode root = g.CreateUriNode(new Uri("http://root"));
        g.Assert(root, g.CreateUriNode(new Uri("http://list")), listEntry2);

        var formatter = new NTriplesFormatter();
        _output.WriteLine("Original Graph");
        foreach (Triple t in g.Triples)
        {
            _output.WriteLine(t.ToString(formatter));
        }
        _output.WriteLine("");

        var sw = new System.IO.StringWriter();
        var context = new CompressingTurtleWriterContext(g, sw);
        WriterHelper.FindCollections(context);
        _output.WriteLine(sw.ToString());
        _output.WriteLine(context.Collections.Count + " Collections Found");
        _output.WriteLine("");

        var strWriter = new System.IO.StringWriter();
        var writer = new CompressingTurtleWriter
        {
            CompressionLevel = WriterCompressionLevel.High
        };
        writer.Save(g, strWriter);

        _output.WriteLine("Compressed Turtle");
        _output.WriteLine(strWriter.ToString());
        _output.WriteLine("");

        var h = new Graph();
        var parser = new TurtleParser();
        StringParser.Parse(h, strWriter.ToString());
        _output.WriteLine("Graph after Round Trip to Compressed Turtle");
        foreach (Triple t in h.Triples)
        {
            _output.WriteLine(t.ToString(formatter));
        }

        Assert.Equal(g, h);
    }

    [Fact]
    public void WritingCollectionCompressionCyclic()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
        g.NamespaceMap.AddNamespace("dnr", new Uri(ConfigurationLoader.ConfigurationNamespace));
        INode a = g.CreateBlankNode();
        INode b = g.CreateBlankNode();
        INode c = g.CreateBlankNode();

        var pred = g.CreateUriNode("ex:pred");

        g.Assert(a, pred, b);
        g.Assert(a, pred, g.CreateLiteralNode("Value for A"));
        g.Assert(b, pred, c);
        g.Assert(b, pred, g.CreateLiteralNode("Value for B"));
        g.Assert(c, pred, a);
        g.Assert(c, pred, g.CreateLiteralNode("Value for C"));

        var collections = FindCollections(g);

        Assert.Equal(2, collections.Count);

        CheckCompressionRoundTrip(g);
    }

    [Fact]
    public void WritingCollectionCompressionCyclic2()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
        g.NamespaceMap.AddNamespace("dnr", new Uri(ConfigurationLoader.ConfigurationNamespace));
        INode a = g.CreateBlankNode();
        INode b = g.CreateBlankNode();
        INode c = g.CreateBlankNode();
        INode d = g.CreateBlankNode();

        INode pred = g.CreateUriNode("ex:pred");

        g.Assert(d, pred, a);
        g.Assert(d, pred, g.CreateLiteralNode("D"));
        g.Assert(a, pred, b);
        g.Assert(a, pred, g.CreateLiteralNode("A"));
        g.Assert(b, pred, c);
        g.Assert(b, pred, g.CreateLiteralNode("B"));
        g.Assert(c, pred, a);
        g.Assert(c, pred, g.CreateLiteralNode("C"));

        var collections = FindCollections(g);

        Assert.Equal(2, collections.Count);

        CheckCompressionRoundTrip(g);
    }

    [Fact]
    public void WritingCollectionCompressionCyclic3()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
        g.NamespaceMap.AddNamespace("dnr", new Uri(ConfigurationLoader.ConfigurationNamespace));
        INode a = g.CreateBlankNode();
        INode b = g.CreateBlankNode();
        INode c = g.CreateBlankNode();
        INode d = g.CreateBlankNode();
        INode e = g.CreateBlankNode();

        INode pred = g.CreateUriNode("ex:pred");

        g.Assert(d, pred, a);
        g.Assert(d, pred, g.CreateLiteralNode("D"));
        g.Assert(a, pred, b);
        g.Assert(a, pred, g.CreateLiteralNode("A"));
        g.Assert(b, pred, c);
        g.Assert(b, pred, g.CreateLiteralNode("B"));
        g.Assert(c, pred, a);
        g.Assert(c, pred, g.CreateLiteralNode("C"));
        g.Assert(e, pred, g.CreateLiteralNode("E"));

        var collections = FindCollections(g);

        Assert.Equal(3, collections.Count);

        CheckCompressionRoundTrip(g);
    }

    [Fact]
    public void WritingCollectionCompressionEmpty1()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
        INode n = g.CreateBlankNode();

        g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred"), n);

        var collections = FindCollections(g);

        Assert.Single(collections);
        Assert.Empty(collections.First().Value.Triples);

        CheckCompressionRoundTrip(g);
    }

    [Fact]
    public void WritingCollectionCompressionEmpty2()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
        INode rdfType = g.CreateUriNode("rdf:type");

        g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred"), g.CreateUriNode("rdf:nil"));

        var collections = FindCollections(g);

        Assert.Empty(collections);

        CheckCompressionRoundTrip(g);
    }

    [Fact]
    public void WritingCollectionCompressionSimple1()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
        INode n = g.CreateBlankNode();
        INode rdfType = g.CreateUriNode("rdf:type");

        g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred"), n);
        g.Assert(n, rdfType, g.CreateUriNode("ex:BlankNode"));

        var collections = FindCollections(g);

        Assert.Single(collections);
        Assert.Single(collections.First().Value.Triples);

        CheckCompressionRoundTrip(g);
    }

    private Dictionary<INode, OutputRdfCollection> FindCollections(IGraph g)
    {
        var sw = new System.IO.StringWriter();
        var context = new CompressingTurtleWriterContext(g, sw);
        _output.WriteLine(sw.ToString());
        WriterHelper.FindCollections(context);
        return context.Collections;
    }

    [Fact]
    public void WritingCollectionCompressionSimple2()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
        INode n = g.CreateBlankNode();
        INode rdfType = g.CreateUriNode("rdf:type");

        g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred"), n);
        g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred2"), n);
        g.Assert(n, rdfType, g.CreateUriNode("ex:BlankNode"));

        var collections = FindCollections(g);

        Assert.Empty(collections);

        CheckCompressionRoundTrip(g);
    }

    [Fact]
    public void WritingCollectionCompressionSimple3()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
        INode n = g.CreateBlankNode();
        INode rdfType = g.CreateUriNode("rdf:type");
        INode rdfFirst = g.CreateUriNode("rdf:first");
        INode rdfRest = g.CreateUriNode("rdf:rest");
        INode rdfNil = g.CreateUriNode("rdf:nil");

        g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred"), n);
        g.Assert(n, rdfFirst, g.CreateLiteralNode("first"));
        g.Assert(n, rdfRest, rdfNil);

        var collections = FindCollections(g);

        Assert.Single(collections);

        CheckCompressionRoundTrip(g);
    }

    [Fact]
    public void WritingCollectionCompressionSimple4()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
        INode n = g.CreateBlankNode();
        INode rdfType = g.CreateUriNode("rdf:type");
        INode rdfFirst = g.CreateUriNode("rdf:first");
        INode rdfRest = g.CreateUriNode("rdf:rest");
        INode rdfNil = g.CreateUriNode("rdf:nil");

        g.Assert(n, rdfFirst, g.CreateLiteralNode("first"));
        g.Assert(n, rdfRest, rdfNil);

        var collections = FindCollections(g);

        Assert.Empty(collections);

        CheckCompressionRoundTrip(g);
    }

    [Fact]
    public void WritingCollectionCompressionSimple5()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
        INode n = g.CreateBlankNode();
        INode rdfType = g.CreateUriNode("rdf:type");
        INode rdfFirst = g.CreateUriNode("rdf:first");
        INode rdfRest = g.CreateUriNode("rdf:rest");
        INode rdfNil = g.CreateUriNode("rdf:nil");

        g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred"), n);
        g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred2"), n);
        g.Assert(n, rdfFirst, g.CreateLiteralNode("first"));
        g.Assert(n, rdfRest, rdfNil);

        var collections = FindCollections(g);

        Assert.Empty(collections);

        CheckCompressionRoundTrip(g);
    }

    [Fact]
    public void WritingCollectionCompressionSimple6()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
        INode n = g.CreateBlankNode();
        INode rdfType = g.CreateUriNode("rdf:type");

        g.Assert(n, rdfType, g.CreateUriNode("ex:Obj"));

        var collections = FindCollections(g);

        Assert.Single(collections);
        Assert.Empty(collections.First().Value.Triples);

        CheckCompressionRoundTrip(g);
    }

    [Fact]
    [Obsolete("Test uses obsolete API")]
    public void WritingCollectionCompressionComplex1()
    {
        var connector = new SparqlConnector(new VDS.RDF.Query.SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql")));
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
        g.NamespaceMap.AddNamespace("dnr", new Uri(ConfigurationLoader.ConfigurationNamespace));
        INode n = g.CreateBlankNode();

        g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("dnr:genericManager"), n);
        var sContext = new ConfigurationSerializationContext(g)
        {
            NextSubject = n
        };
        connector.SerializeConfiguration(sContext);

        var collections = FindCollections(g);

        Assert.Equal(2, collections.Count);

        CheckCompressionRoundTrip(g);
    }

    [Fact]
    public void WritingCollectionCompressionComplex2()
    {
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "complex-collections.nt"));

        var context = new CompressingTurtleWriterContext(g, Console.Out);
        WriterHelper.FindCollections(context);

        var formatter = new NTriplesFormatter();
        foreach (KeyValuePair<INode, OutputRdfCollection> kvp in context.Collections)
        {
            _output.WriteLine("Collection Root - " + kvp.Key.ToString(formatter));
            _output.WriteLine("Collection Triples (" + kvp.Value.Triples.Count + ")");
            foreach (Triple t in kvp.Value.Triples)
            {
                _output.WriteLine(t.ToString(formatter));
            }
            _output.WriteLine("");
        }

        CheckCompressionRoundTrip(g);
    }

    [Fact]
    public void WritingCollectionCompressionNamedListNodes()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
        INode n = g.CreateUriNode("ex:list");
        INode rdfType = g.CreateUriNode("rdf:type");
        INode rdfFirst = g.CreateUriNode("rdf:first");
        INode rdfRest = g.CreateUriNode("rdf:rest");
        INode rdfNil = g.CreateUriNode("rdf:nil");

        g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred"), n);
        g.Assert(n, rdfFirst, g.CreateLiteralNode("first"));
        g.Assert(n, rdfRest, rdfNil);

        var collections = FindCollections(g);

        Assert.Empty(collections);

        CheckCompressionRoundTrip(g);
    }

    [Fact]
    public void WritingCollectionCompressionNamedListNodes2()
    {
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
        INode n = g.CreateUriNode("ex:listRoot");
        INode m = g.CreateUriNode("ex:listItem");
        INode rdfType = g.CreateUriNode("rdf:type");
        INode rdfFirst = g.CreateUriNode("rdf:first");
        INode rdfRest = g.CreateUriNode("rdf:rest");
        INode rdfNil = g.CreateUriNode("rdf:nil");

        g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred"), n);
        g.Assert(n, rdfFirst, g.CreateLiteralNode("first"));
        g.Assert(n, rdfRest, m);
        g.Assert(m, rdfFirst, g.CreateLiteralNode("second"));
        g.Assert(m, rdfRest, rdfNil);

        var collections = FindCollections(g);

        Assert.Empty(collections);

        CheckCompressionRoundTrip(g);
    }

    [Fact]
    public void WritingBlankNodeCollectionIssue279()
    {
        var g = new Graph();
        INode b1 = g.CreateBlankNode("b1");
        INode b2 = g.CreateBlankNode("b2");
        INode b3 = g.CreateBlankNode("b3");
        INode b4 = g.CreateBlankNode("b4");
        INode rdfType = g.CreateUriNode("rdf:type");
        INode rdfFirst = g.CreateUriNode("rdf:first");
        INode rdfRest = g.CreateUriNode("rdf:rest");
        INode rdfNil = g.CreateUriNode("rdf:nil");

        g.Assert(b1, rdfType, b2);
        g.Assert(b2, rdfFirst, b3);
        g.Assert(b2, rdfRest, rdfNil);
        g.Assert(b3, rdfType, b4);

        FindCollections(g);

        CheckCompressionRoundTrip(g);
    }

    [Fact]
    public void WritingBlankNodeCollection2()
    {
        var g = new Graph();
        INode b1 = g.CreateBlankNode("b1");
        INode b2 = g.CreateBlankNode("b2");
        INode b3 = g.CreateBlankNode("b3");
        INode b4 = g.CreateBlankNode("b4");
        INode b5 = g.CreateBlankNode("b5");
        INode rdfType = g.CreateUriNode("rdf:type");
        INode rdfFirst = g.CreateUriNode("rdf:first");
        INode rdfRest = g.CreateUriNode("rdf:rest");
        INode rdfNil = g.CreateUriNode("rdf:nil");

        g.Assert(b1, rdfType, b2);
        g.Assert(b2, rdfFirst, b3);
        g.Assert(b2, rdfRest, rdfNil);
        g.Assert(b3, rdfType, b4);
        g.Assert(b4, rdfType, b5);

        FindCollections(g);

        CheckCompressionRoundTrip(g);

    }

    [Fact]
    public void WritingBlankNodeCollection3()
    {
        var g = new Graph();
        INode b1 = g.CreateBlankNode("b1");
        INode b2 = g.CreateBlankNode("b2");
        INode b3 = g.CreateBlankNode("b3");
        INode b4 = g.CreateBlankNode("b4");
        INode b5 = g.CreateBlankNode("b5");
        INode rdfType = g.CreateUriNode("rdf:type");
        INode rdfFirst = g.CreateUriNode("rdf:first");
        INode rdfRest = g.CreateUriNode("rdf:rest");
        INode rdfNil = g.CreateUriNode("rdf:nil");

        g.Assert(b1, rdfType, b2);
        g.Assert(b2, rdfFirst, b3);
        g.Assert(b2, rdfRest, rdfNil);
        g.Assert(b3, rdfType, b4);
        g.Assert(b3, rdfType, b5);

        FindCollections(g);

        CheckCompressionRoundTrip(g);

    }

    [Fact]
    public void WritingBlankNodeCollection4()
    {
        var g = new Graph();
        INode b1 = g.CreateBlankNode("b1");
        INode b2 = g.CreateBlankNode("b2");
        INode b3 = g.CreateBlankNode("b3");
        INode b4 = g.CreateBlankNode("b4");
        INode b5 = g.CreateBlankNode("b5");
        INode b6 = g.CreateBlankNode("b6");
        INode rdfType = g.CreateUriNode("rdf:type");
        INode rdfFirst = g.CreateUriNode("rdf:first");
        INode rdfRest = g.CreateUriNode("rdf:rest");
        INode rdfNil = g.CreateUriNode("rdf:nil");

        g.Assert(b1, rdfType, b2);
        g.Assert(b2, rdfFirst, b3);
        g.Assert(b2, rdfRest, b4);
        g.Assert(b3, rdfType, b5);
        g.Assert(b4, rdfFirst, b6);
        g.Assert(b4, rdfRest, rdfNil);

        FindCollections(g);

        CheckCompressionRoundTrip(g);

    }


}
