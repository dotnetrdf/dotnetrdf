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
using System.Linq;
using Xunit;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Parsing.Handlers;

public class PagingHandlerTests
{
    private static void ParsingUsingPagingHandler(String tempFile, IRdfReader parser)
    {
        var g = new Graph();
        EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
        g.SaveToFile(tempFile);

        var h = new Graph();
        var handler = new PagingHandler(new GraphHandler(h), 25);
        parser.Load(handler, tempFile);
        h.Retract(h.Triples.Where(t => !t.IsGroundTriple).ToList());

        var formatter = new NTriplesFormatter();
        foreach (Triple t in h.Triples)
        {
            Console.WriteLine(t.ToString(formatter));
        }
        Console.WriteLine();

        Assert.False(h.IsEmpty, "Graph should not be empty");
        Assert.True(h.Triples.Count <= 25, "Graphs should have <= 25 Triples");

        var i = new Graph();
        handler = new PagingHandler(new GraphHandler(i), 25, 25);
        parser.Load(handler, tempFile);
        i.Retract(i.Triples.Where(t => !t.IsGroundTriple));

        foreach (Triple t in i.Triples)
        {
            Console.WriteLine(t.ToString(formatter));
        }
        Console.WriteLine();

        Assert.False(i.IsEmpty, "Graph should not be empty");
        Assert.True(i.Triples.Count <= 25, "Graphs should have <= 25 Triples");

        GraphDiffReport report = h.Difference(i);
        Assert.False(report.AreEqual, "Graphs should not be equal");
        Assert.Equal(i.Triples.Count, report.AddedTriples.Count());
        Assert.Equal(h.Triples.Count, report.RemovedTriples.Count());
    }

    private static void ParsingUsingPagingHandler2(String tempFile, IRdfReader parser)
    {
        var g = new Graph();
        EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
        g.SaveToFile(tempFile);

        var h = new Graph();
        var handler = new PagingHandler(new GraphHandler(h), 0);

        parser.Load(handler, tempFile);

        Assert.True(h.IsEmpty, "Graph should be empty");
    }

    private static void ParsingUsingPagingHandler3(String tempFile, IRdfReader parser)
    {
        var g = new Graph();
        EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
        g.SaveToFile(tempFile);

        var h = new Graph();
        var handler = new PagingHandler(new GraphHandler(h), -1, 100);

        parser.Load(handler, tempFile);

        Assert.False(h.IsEmpty, "Graph should not be empty");
        Assert.Equal(g.Triples.Count - 100, h.Triples.Count);
    }

    #region These tests take two slices from the graph (0-25) and (26-50) and ensure they are different

    [Fact]
    public void ParsingPagingHandlerNTriples()
    {
        ParsingUsingPagingHandler("paging_handler_tests_temp.nt", new NTriplesParser());
    }

    [Fact]
    public void ParsingPagingHandlerTurtle()
    {
        ParsingUsingPagingHandler("paging_handler_tests_temp.ttl", new TurtleParser());
    }

    [Fact]
    public void ParsingPagingHandlerNotation3()
    {
        ParsingUsingPagingHandler("paging_handler_tests_temp.n3", new Notation3Parser());
    }

    [Fact]
    public void ParsingPagingHandlerRdfA()
    {
        ParsingUsingPagingHandler("paging_handler_tests_temp.html", new RdfAParser());
    }

    [Fact]
    public void ParsingPagingHandlerRdfJson()
    {
        ParsingUsingPagingHandler("paging_handler_tests_temp.json", new RdfJsonParser());
    }

    #endregion

    #region These tests take 0 triples from the graph and ensure it is empty

    [Fact]
    public void ParsingPagingHandlerNTriples2()
    {
        ParsingUsingPagingHandler2("paging_handler_tests_temp.nt", new NTriplesParser());
    }

    [Fact]
    public void ParsingPagingHandlerTurtle2()
    {
        ParsingUsingPagingHandler2("paging_handler_tests_temp.ttl", new TurtleParser());
    }

    [Fact]
    public void ParsingPagingHandlerNotation3_2()
    {
        ParsingUsingPagingHandler2("paging_handler_tests_temp.n3", new Notation3Parser());
    }

    [Fact]
    public void ParsingPagingHandlerRdfA2()
    {
        ParsingUsingPagingHandler2("paging_handler_tests_temp.html", new RdfAParser());
    }

    [Fact]
    public void ParsingPagingHandlerRdfJson2()
    {
        ParsingUsingPagingHandler2("paging_handler_tests_temp.json", new RdfJsonParser());
    }

    #endregion

    #region These tests discard the first 100 triples and take the rest

    [Fact]
    public void ParsingPagingHandlerNTriples3()
    {
        ParsingUsingPagingHandler3("paging_handler_tests_temp.nt", new NTriplesParser());
    }

    [Fact]
    public void ParsingPagingHandlerTurtle3()
    {
        ParsingUsingPagingHandler3("paging_handler_tests_temp.ttl", new TurtleParser());
    }

    [Fact]
    public void ParsingPagingHandlerNotation3_3()
    {
        ParsingUsingPagingHandler3("paging_handler_tests_temp.n3", new Notation3Parser());
    }

    [Fact]
    public void ParsingPagingHandlerRdfA3()
    {
        ParsingUsingPagingHandler3("paging_handler_tests_temp.html", new RdfAParser());
    }

    [Fact]
    public void ParsingPagingHandlerRdfJson3()
    {
        ParsingUsingPagingHandler3("paging_handler_tests_temp.json", new RdfJsonParser());
    }

    #endregion

    [Fact]
    public void ParsingPagingHandlerRdfXml()
    {
        ParsingUsingPagingHandler("paging_handler_tests_temp.rdf", new RdfXmlParser());
    }

    [Fact]
    public void ParsingPagingHandlerRdfXml2()
    {
        ParsingUsingPagingHandler2("paging_handler_tests_temp.rdf", new RdfXmlParser());
    }

    [Fact]
    public void ParsingPagingHandlerRdfXml3()
    {
        ParsingUsingPagingHandler3("paging_handler_tests_temp.rdf", new RdfXmlParser());
    }
}
