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
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query;


public class BindingsTests
{
    private SparqlQueryParser _parser = new SparqlQueryParser();

    [Fact]
    public void SparqlBindingsSimple()
    {
        var query = new SparqlParameterizedString();
        query.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
        query.CommandText = "SELECT ?subj WHERE { ?subj a ex:Car }";

        var bindingsQuery = new SparqlParameterizedString
        {
            Namespaces = query.Namespaces,
            CommandText = "SELECT ?subj WHERE { ?subj a ?type } VALUES ?type { ex:Car }"
        };

        TestBindings(GetTestData(), bindingsQuery, query);
    }

    [Fact]
    public void SparqlBindingsSimple2()
    {
        var query = new SparqlParameterizedString();
        query.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
        query.CommandText = "SELECT ?subj WHERE { { ?subj a ex:Car } UNION { ?subj a ex:Plane } } ORDER BY ?subj";

        var bindingsQuery = new SparqlParameterizedString
        {
            Namespaces = query.Namespaces,
            CommandText = "SELECT ?subj WHERE { ?subj a ?type } ORDER BY ?subj VALUES ?type { ex:Car ex:Plane }"
        };

        TestBindings(GetTestData(), bindingsQuery, query);
    }
    [Fact]
    public void SparqlBindingsSimple3()
    {
        var query = new SparqlParameterizedString();
        query.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
        query.CommandText = "SELECT ?subj WHERE { ?subj a ex:Car } VALUES ?subj {ex:FordFiesta}";

        var bindingsQuery = new SparqlParameterizedString
        {
            Namespaces = query.Namespaces,
            CommandText = "SELECT ?subj WHERE { ?subj a ?type } VALUES ( ?subj ?type ) { (ex:FordFiesta ex:Car) }"
        };

        TestBindings(GetTestData(), bindingsQuery, query);
    }


    private void TestBindings(ISparqlDataset data, SparqlParameterizedString queryWithBindings, SparqlParameterizedString queryWithoutBindings)
    {
        TestBindings(data, queryWithBindings.ToString(), queryWithoutBindings.ToString());
    }

    private void TestBindings(ISparqlDataset data, String queryWithBindings, String queryWithoutBindings)
    {
        var processor = new LeviathanQueryProcessor(data);
        SparqlQuery bindingsQuery = _parser.ParseFromString(queryWithBindings);
        SparqlQuery noBindingsQuery = _parser.ParseFromString(queryWithoutBindings);

        var bindingsResults = processor.ProcessQuery(bindingsQuery) as SparqlResultSet;
        var noBindingsResults = processor.ProcessQuery(noBindingsQuery) as SparqlResultSet;

        if (bindingsResults == null) Assert.Fail("Did not get a SPARQL Result Set for the Bindings Query");
        if (noBindingsResults == null) Assert.Fail("Did not get a SPARQL Result Set for the Non-Bindings Query");

        Console.WriteLine("Bindings Results");
        TestTools.ShowResults(bindingsResults);
        Console.WriteLine();
        Console.WriteLine("Non-Bindings Results");
        TestTools.ShowResults(noBindingsResults);
        Console.WriteLine();

        Assert.Equal(noBindingsResults, bindingsResults);
    }

    private void TestBindings(ISparqlDataset data, String queryWithBindings)
    {
        var processor = new LeviathanQueryProcessor(data);
        SparqlQuery bindingsQuery = _parser.ParseFromString(queryWithBindings);

        var bindingsResults = processor.ProcessQuery(bindingsQuery) as SparqlResultSet;

        if (bindingsResults == null) Assert.Fail("Did not get a SPARQL Result Set for the Bindings Query");

        Console.WriteLine("Bindings Results");
        TestTools.ShowResults(bindingsResults);
        Console.WriteLine();

        Assert.True(bindingsResults.IsEmpty, "Result Set should be empty");
    }

    private ISparqlDataset GetTestData()
    {
        var dataset = new InMemoryDataset();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        dataset.AddGraph(g);

        return dataset;
    }
}
