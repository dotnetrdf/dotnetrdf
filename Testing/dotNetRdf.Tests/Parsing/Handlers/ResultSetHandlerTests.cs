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
using VDS.RDF.Query;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Parsing.Handlers;

public class ResultSetHandlerTests
{
    private void EnsureTestData(String file)
    {
        var writer = MimeTypesHelper.GetSparqlWriterByFileExtension(Path.GetExtension(file));
        EnsureTestData(file, writer);
    }

    private void EnsureTestData(String file, ISparqlResultsWriter writer)
    {
        if (!File.Exists(file))
        {
            var g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.Retract(g.Triples.Where(t => !t.IsGroundTriple).ToList());
            var results = g.ExecuteQuery("SELECT * WHERE { ?s ?p ?o }") as SparqlResultSet;
            if (results == null) Assert.Fail("Failed to generate sample SPARQL Results");

            writer.Save(results, file);
        }
    }

    [Fact]
    public void ParsingResultSetHandlerImplicitSparqlXml()
    {
        EnsureTestData("test.srx");

        var parser = new SparqlXmlParser();
        var results = new SparqlResultSet();
        parser.Load(results, "test.srx");

        var formatter = new NTriplesFormatter();
        foreach (SparqlResult r in results)
        {
            Console.WriteLine(r.ToString(formatter));
        }

        Assert.False(results.IsEmpty, "Result Set should not be empty");
        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
    }

    [Fact]
    public void ParsingResultSetHandlerImplicitSparqlJson()
    {
        EnsureTestData("test.srj");

        var parser = new SparqlJsonParser();
        var results = new SparqlResultSet();
        parser.Load(results, "test.srj");

        var formatter = new NTriplesFormatter();
        foreach (SparqlResult r in results)
        {
            Console.WriteLine(r.ToString(formatter));
        }

        Assert.False(results.IsEmpty, "Result Set should not be empty");
        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
    }

    [Fact]
    public void ParsingResultSetHandlerImplicitSparqlRdfNTriples()
    {
        EnsureTestData("test.sparql.nt", new SparqlRdfWriter(new NTriplesWriter()));

        var parser = new SparqlRdfParser(new NTriplesParser());
        var results = new SparqlResultSet();
        parser.Load(results, "test.sparql.nt");

        var formatter = new NTriplesFormatter();
        foreach (SparqlResult r in results)
        {
            Console.WriteLine(r.ToString(formatter));
        }

        Assert.False(results.IsEmpty, "Result Set should not be empty");
        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
    }

    [Fact]
    public void ParsingResultSetHandlerImplicitSparqlRdfTurtle()
    {
        EnsureTestData("test.sparql.ttl", new SparqlRdfWriter(new CompressingTurtleWriter()));

        var parser = new SparqlRdfParser(new TurtleParser());
        var results = new SparqlResultSet();
        parser.Load(results, "test.sparql.ttl");

        var formatter = new NTriplesFormatter();
        foreach (SparqlResult r in results)
        {
            Console.WriteLine(r.ToString(formatter));
        }

        Assert.False(results.IsEmpty, "Result Set should not be empty");
        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
    }

    [Fact]
    public void ParsingResultSetHandlerImplicitSparqlRdfNotation3()
    {
        EnsureTestData("test.sparql.n3", new SparqlRdfWriter(new Notation3Writer()));

        var parser = new SparqlRdfParser(new Notation3Parser());
        var results = new SparqlResultSet();
        parser.Load(results, "test.sparql.n3");

        var formatter = new NTriplesFormatter();
        foreach (SparqlResult r in results)
        {
            Console.WriteLine(r.ToString(formatter));
        }

        Assert.False(results.IsEmpty, "Result Set should not be empty");
        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
    }

    [Fact]
    public void ParsingResultSetHandlerImplicitSparqlRdfJson()
    {
        EnsureTestData("test.sparql.json", new SparqlRdfWriter(new RdfJsonWriter()));

        var parser = new SparqlRdfParser(new RdfJsonParser());
        var results = new SparqlResultSet();
        parser.Load(results, "test.sparql.json");

        var formatter = new NTriplesFormatter();
        foreach (SparqlResult r in results)
        {
            Console.WriteLine(r.ToString(formatter));
        }

        Assert.False(results.IsEmpty, "Result Set should not be empty");
        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
    }

    [Fact]
    public void ParsingResultSetHandlerExplicitSparqlXml()
    {
        EnsureTestData("test.srx");

        var parser = new SparqlXmlParser();
        var results = new SparqlResultSet();
        parser.Load(new ResultSetHandler(results), "test.srx");

        var formatter = new NTriplesFormatter();
        foreach (SparqlResult r in results)
        {
            Console.WriteLine(r.ToString(formatter));
        }

        Assert.False(results.IsEmpty, "Result Set should not be empty");
        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
    }

    [Fact]
    public void ParsingResultSetHandlerExplicitSparqlJson()
    {
        EnsureTestData("test.srj");

        var parser = new SparqlJsonParser();
        var results = new SparqlResultSet();
        parser.Load(new ResultSetHandler(results), "test.srj");

        var formatter = new NTriplesFormatter();
        foreach (SparqlResult r in results)
        {
            Console.WriteLine(r.ToString(formatter));
        }

        Assert.False(results.IsEmpty, "Result Set should not be empty");
        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
    }

    [Fact]
    public void ParsingResultSetHandlerExplicitSparqlRdfNTriples()
    {
        EnsureTestData("test.sparql.nt", new SparqlRdfWriter(new NTriplesWriter()));

        var parser = new SparqlRdfParser(new NTriplesParser());
        var results = new SparqlResultSet();
        parser.Load(new ResultSetHandler(results), "test.sparql.nt");

        var formatter = new NTriplesFormatter();
        foreach (SparqlResult r in results)
        {
            Console.WriteLine(r.ToString(formatter));
        }

        Assert.False(results.IsEmpty, "Result Set should not be empty");
        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
    }

    [Fact]
    public void ParsingResultSetHandlerExplicitSparqlRdfTurtle()
    {
        EnsureTestData("test.sparql.ttl", new SparqlRdfWriter(new CompressingTurtleWriter()));

        var parser = new SparqlRdfParser(new TurtleParser());
        var results = new SparqlResultSet();
        parser.Load(new ResultSetHandler(results), "test.sparql.ttl");

        var formatter = new NTriplesFormatter();
        foreach (SparqlResult r in results)
        {
            Console.WriteLine(r.ToString(formatter));
        }

        Assert.False(results.IsEmpty, "Result Set should not be empty");
        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
    }

    [Fact]
    public void ParsingResultSetHandlerExplicitSparqlRdfNotation3()
    {
        EnsureTestData("test.sparql.n3", new SparqlRdfWriter(new Notation3Writer()));

        var parser = new SparqlRdfParser(new Notation3Parser());
        var results = new SparqlResultSet();
        parser.Load(new ResultSetHandler(results), "test.sparql.n3");

        var formatter = new NTriplesFormatter();
        foreach (SparqlResult r in results)
        {
            Console.WriteLine(r.ToString(formatter));
        }

        Assert.False(results.IsEmpty, "Result Set should not be empty");
        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
    }

    [Fact]
    public void ParsingResultSetHandlerExplicitSparqlRdfJson()
    {
        EnsureTestData("test.sparql.json", new SparqlRdfWriter(new RdfJsonWriter()));

        var parser = new SparqlRdfParser(new RdfJsonParser());
        var results = new SparqlResultSet();
        parser.Load(new ResultSetHandler(results), "test.sparql.json");

        var formatter = new NTriplesFormatter();
        foreach (SparqlResult r in results)
        {
            Console.WriteLine(r.ToString(formatter));
        }

        Assert.False(results.IsEmpty, "Result Set should not be empty");
        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
    }

    [Fact]
    public void ParsingMergingResultSetHandler()
    {
        EnsureTestData("test.srx", new SparqlXmlWriter());

        var parser = new SparqlXmlParser();
        var results = new SparqlResultSet();
        var handler = new MergingResultSetHandler(results);
        parser.Load(handler, "test.srx");

        Assert.False(results.IsEmpty, "Result Set should not be empty");
        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);

        var count = results.Count;

        //Load again
        parser.Load(handler, "test.srx");

        Assert.Equal(count * 2, results.Count);
    }

    [Fact]
    public void ParsingResultsWriteThroughHandlerSparqlXml()
    {
        var g = new Graph();
        //g.LoadFromEmbeddedResource("VDS.RDF.Query.Optimisation.OptimiserStats.ttl");
        g.LoadFromFile(Path.Combine("resources", "rvesse.ttl"));
        var original = g.ExecuteQuery("SELECT * WHERE { ?s ?p ?o }") as SparqlResultSet;
        var sparqlWriter = new SparqlXmlWriter();
        sparqlWriter.Save(original, "test.custom.srx");

        var parser = new SparqlXmlParser();
        var writer = new System.IO.StringWriter();
        var handler = new ResultWriteThroughHandler(new SparqlXmlFormatter(), writer, false);
        parser.Load(handler, "test.custom.srx");

        Console.WriteLine(writer.ToString());

        var results = new SparqlResultSet();
        parser.Load(results, new StringReader(writer.ToString()));

        Console.WriteLine("Original Result Count: " + original.Count);
        Console.WriteLine("Round Trip Result Count: " + results.Count);

        Assert.Equal(original, results);
    }

    [Fact]
    public void ParsingResultSetHandlerImplicitSparqlRdfXml()
    {
        EnsureTestData("test.sparql.rdf", new SparqlRdfWriter(new RdfXmlWriter()));

        var parser = new SparqlRdfParser(new RdfXmlParser());
        var results = new SparqlResultSet();
        parser.Load(results, "test.sparql.rdf");

        var formatter = new NTriplesFormatter();
        foreach (SparqlResult r in results)
        {
            Console.WriteLine(r.ToString(formatter));
        }

        Assert.False(results.IsEmpty, "Result Set should not be empty");
        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
    }

    [Fact]
    public void ParsingResultSetHandlerExplicitSparqlRdfXml()
    {
        EnsureTestData("test.sparql.rdf", new SparqlRdfWriter(new RdfXmlWriter()));

        var parser = new SparqlRdfParser(new RdfXmlParser());
        var results = new SparqlResultSet();
        parser.Load(new ResultSetHandler(results), "test.sparql.rdf");

        var formatter = new NTriplesFormatter();
        foreach (SparqlResult r in results)
        {
            Console.WriteLine(r.ToString(formatter));
        }

        Assert.False(results.IsEmpty, "Result Set should not be empty");
        Assert.Equal(SparqlResultsType.VariableBindings, results.ResultsType);
    }
}
