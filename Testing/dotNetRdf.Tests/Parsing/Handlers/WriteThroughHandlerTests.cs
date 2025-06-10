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
using Xunit;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Parsing.Handlers;


public class WriteThroughHandlerTests
{
    private void ParsingUsingWriteThroughHandler(ITripleFormatter formatter)
    {
        if (!System.IO.File.Exists("write_through_handler_tests_temp.ttl"))
        {
            var g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
            g.SaveToFile("write_through_handler_tests_temp.ttl");
        }

        var handler = new WriteThroughHandler(formatter, Console.Out, false);
        var parser = new TurtleParser();
        parser.Load(handler, "write_through_handler_tests_temp.ttl");
    }

    private void ParsingUsingWriteThroughHandler(Type formatterType)
    {
        if (!System.IO.File.Exists("write_through_handler_tests_temp.ttl"))
        {
            var g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
            g.SaveToFile("write_through_handler_tests_temp.ttl");
        }

        var handler = new WriteThroughHandler(formatterType, Console.Out, false);
        var parser = new TurtleParser();
        parser.Load(handler, "write_through_handler_tests_temp.ttl");
    }

    [Fact]
    public void ParsingWriteThroughHandlerNTriples()
    {
        ParsingUsingWriteThroughHandler(new NTriplesFormatter());
    }

    [Fact]
    public void ParsingWriteThroughHandlerTurtle()
    {
        ParsingUsingWriteThroughHandler(new TurtleFormatter());
    }

    [Fact]
    public void ParsingWriteThroughHandlerNotation3()
    {
        ParsingUsingWriteThroughHandler(new Notation3Formatter());
    }

    [Fact]
    public void ParsingWriteThroughHandlerNQuads()
    {
        ParsingUsingWriteThroughHandler(new NQuadsFormatter());
    }

    [Fact]
    public void ParsingWriteThroughHandlerUncompressedNotation3()
    {
        ParsingUsingWriteThroughHandler(new UncompressedNotation3Formatter());
    }

    [Fact]
    public void ParsingWriteThroughHandlerUncompressedTurtle()
    {
        ParsingUsingWriteThroughHandler(new UncompressedTurtleFormatter());
    }

    [Fact]
    public void ParsingWriteThroughHandlerCsv()
    {
        ParsingUsingWriteThroughHandler(new CsvFormatter());
    }

    [Fact]
    public void ParsingWriteThroughHandlerTsv()
    {
        ParsingUsingWriteThroughHandler(new TsvFormatter());
    }

    [Fact]
    public void ParsingWriteThroughHandlerRdfXml()
    {
        ParsingUsingWriteThroughHandler(new RdfXmlFormatter());
    }

    [Fact]
    public void ParsingWriteThroughHandlerRdfXml2()
    {
        ParsingUsingWriteThroughHandler(typeof(RdfXmlFormatter));
    }

    [Fact]
    public void ParsingWriteThroughHandlerSparql()
    {
        ParsingUsingWriteThroughHandler(new SparqlFormatter());
    }

    [Fact]
    public void ParsingWriteThroughHandlerNTriples2()
    {
        ParsingUsingWriteThroughHandler(typeof(NTriplesFormatter));
    }

    [Fact]
    public void ParsingWriteThroughHandlerTurtle2()
    {
        ParsingUsingWriteThroughHandler(typeof(TurtleFormatter));
    }

    [Fact]
    public void ParsingWriteThroughHandlerNotation32()
    {
        ParsingUsingWriteThroughHandler(typeof(Notation3Formatter));
    }

    [Fact]
    public void ParsingWriteThroughHandlerNQuads2()
    {
        ParsingUsingWriteThroughHandler(typeof(NQuadsFormatter));
    }

    [Fact]
    public void ParsingWriteThroughHandlerUncompressedNotation32()
    {
        ParsingUsingWriteThroughHandler(typeof(UncompressedNotation3Formatter));
    }

    [Fact]
    public void ParsingWriteThroughHandlerUncompressedTurtle2()
    {
        ParsingUsingWriteThroughHandler(typeof(UncompressedTurtleFormatter));
    }

    [Fact]
    public void ParsingWriteThroughHandlerCsv2()
    {
        ParsingUsingWriteThroughHandler(typeof(CsvFormatter));
    }

    [Fact]
    public void ParsingWriteThroughHandlerTsv2()
    {
        ParsingUsingWriteThroughHandler(typeof(TsvFormatter));
    }

    [Fact]
    public void ParsingWriteThroughHandlerSparql2()
    {
        ParsingUsingWriteThroughHandler(typeof(SparqlFormatter));
    }
}
