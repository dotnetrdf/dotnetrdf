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
using VDS.RDF.Storage;

namespace VDS.RDF.Query;


public class NegationTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly SparqlQueryParser _parser = new SparqlQueryParser();

    public NegationTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private static ISparqlDataset GetTestData()
    {
        var dataset = new InMemoryDataset();
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));
        dataset.AddGraph(g);

        return dataset;
    }

    [Fact]
    public void SparqlNegationMinusSimple()
    {
        var negQuery = new SparqlParameterizedString();
        negQuery.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
        negQuery.CommandText = "SELECT ?s WHERE { ?s a ex:Car MINUS { ?s ex:Speed ?speed } }";

        var query = new SparqlParameterizedString
        {
            Namespaces = negQuery.Namespaces,
            CommandText = "SELECT ?s WHERE { ?s a ex:Car OPTIONAL { ?s ex:Speed ?speed } FILTER(!BOUND(?speed)) }"
        };

        TestNegation(GetTestData(), negQuery, query);
    }

    [Fact]
    public void SparqlNegationMinusComplex()
    {
        var negQuery = new SparqlParameterizedString();
        negQuery.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
        negQuery.CommandText = "SELECT ?s WHERE { ?s a ex:Car MINUS { ?s ex:Speed ?speed FILTER(EXISTS { ?s ex:PassengerCapacity ?passengers }) } }";

        var query = new SparqlParameterizedString
        {
            Namespaces = negQuery.Namespaces,
            CommandText = "SELECT ?s WHERE { ?s a ex:Car OPTIONAL { ?s ex:Speed ?speed ; ex:PassengerCapacity ?passengers} FILTER(!BOUND(?speed)) }"
        };

        TestNegation(GetTestData(), negQuery, query);
    }

    [Fact]
    public void SparqlNegationMinusComplex2()
    {
        var negQuery = new SparqlParameterizedString();
        negQuery.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
        negQuery.CommandText = "SELECT ?s WHERE { ?s a ex:Car MINUS { ?s ex:Speed ?speed FILTER(NOT EXISTS { ?s ex:PassengerCapacity ?passengers }) } }";

        var query = new SparqlParameterizedString
        {
            Namespaces = negQuery.Namespaces,
            CommandText = "SELECT ?s WHERE { ?s a ex:Car OPTIONAL { ?s ex:Speed ?speed OPTIONAL { ?s ex:PassengerCapacity ?passengers} FILTER(!BOUND(?passengers)) } FILTER(!BOUND(?speed)) }"
        };

        TestNegation(GetTestData(), negQuery, query);
    }

    [Fact]
    public void SparqlNegationMinusDisjoint()
    {
        var query = "SELECT ?s ?p ?o WHERE { ?s ?p ?o }";
        var negQuery = "SELECT ?s ?p ?o WHERE { ?s ?p ?o MINUS { ?x ?y ?z }}";

        TestNegation(GetTestData(), negQuery, query);
    }

    [Fact]
    public void SparqlNegationNotExistsDisjoint()
    {
        var negQuery = "SELECT ?s ?p ?o WHERE { ?s ?p ?o FILTER(NOT EXISTS { ?x ?y ?z }) }";

        TestNegation(GetTestData(), negQuery);
    }

    [Fact]
    public void SparqlNegationNotExistsSimple()
    {
        var negQuery = new SparqlParameterizedString();
        negQuery.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
        negQuery.CommandText = "SELECT ?s WHERE { ?s a ex:Car FILTER(NOT EXISTS { ?s ex:Speed ?speed }) }";

        var query = new SparqlParameterizedString
        {
            Namespaces = negQuery.Namespaces,
            CommandText = "SELECT ?s WHERE { ?s a ex:Car OPTIONAL { ?s ex:Speed ?speed } FILTER(!BOUND(?speed)) }"
        };

        TestNegation(GetTestData(), negQuery, query);
    }

    [Fact]
    public void SparqlNegationExistsSimple()
    {
        var negQuery = new SparqlParameterizedString();
        negQuery.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
        negQuery.CommandText = "SELECT ?s WHERE { ?s a ex:Car FILTER(EXISTS { ?s ex:Speed ?speed }) }";

        var query = new SparqlParameterizedString
        {
            Namespaces = negQuery.Namespaces,
            CommandText = "SELECT ?s WHERE { ?s a ex:Car OPTIONAL { ?s ex:Speed ?speed } FILTER(BOUND(?speed)) }"
        };

        TestNegation(GetTestData(), negQuery, query);
    }

    [Fact]
    public void SparqlNegationFullMinued()
    {
        SparqlQuery lhsQuery = _parser.ParseFromFile(Path.Combine("resources", "full-minuend-lhs.rq"));
        SparqlQuery rhsQuery = _parser.ParseFromFile(Path.Combine("resources", "full-minuend-rhs.rq"));
        SparqlQuery query = _parser.ParseFromFile(Path.Combine("resources", "full-minuend.rq"));
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "full-minuend.ttl"));
        var processor = new LeviathanQueryProcessor(new InMemoryQuadDataset(g));

        var lhs = processor.ProcessQuery(lhsQuery) as SparqlResultSet;
        _testOutputHelper.WriteLine("LHS Intermediate Results");
        TestTools.ShowResults(lhs, _testOutputHelper);
        _testOutputHelper.WriteLine();

        var rhs = processor.ProcessQuery(rhsQuery) as SparqlResultSet;
        _testOutputHelper.WriteLine("RHS Intermediate Results");
        TestTools.ShowResults(rhs, _testOutputHelper);
        _testOutputHelper.WriteLine();

        var actual = processor.ProcessQuery(query) as SparqlResultSet;
        if (actual == null) Assert.Fail("Null results");
        var expected = new SparqlResultSet();
        var parser = new SparqlXmlParser();
        parser.Load(expected, Path.Combine("resources", "full-minuend.srx"));

        _testOutputHelper.WriteLine("Actual Results:");
        TestTools.ShowResults(actual, _testOutputHelper);
        _testOutputHelper.WriteLine();
        _testOutputHelper.WriteLine("Expected Results:");
        TestTools.ShowResults(expected);

        Assert.Equal(expected, actual);
    }

    private void TestNegation(ISparqlDataset data, SparqlParameterizedString queryWithNegation, SparqlParameterizedString queryWithoutNegation)
    {
        TestNegation(data, queryWithNegation.ToString(), queryWithoutNegation.ToString());
    }

    private void TestNegation(ISparqlDataset data, String queryWithNegation, String queryWithoutNegation)
    {
        var processor = new LeviathanQueryProcessor(data);
        SparqlQuery negQuery = _parser.ParseFromString(queryWithNegation);
        SparqlQuery noNegQuery = _parser.ParseFromString(queryWithoutNegation);

        var negResults = processor.ProcessQuery(negQuery) as SparqlResultSet;
        var noNegResults = processor.ProcessQuery(noNegQuery) as SparqlResultSet;

        if (negResults == null) Assert.Fail("Did not get a SPARQL Result Set for the Negation Query");
        if (noNegResults == null) Assert.Fail("Did not get a SPARQL Result Set for the Non-Negation Query");

        Console.WriteLine("Negation Results");
        TestTools.ShowResults(negResults);
        Console.WriteLine();
        Console.WriteLine("Non-Negation Results");
        TestTools.ShowResults(noNegResults);
        Console.WriteLine();

        Assert.Equal(noNegResults, negResults);
    }

    private void TestNegation(ISparqlDataset data, String queryWithNegation)
    {
        var processor = new LeviathanQueryProcessor(data);
        SparqlQuery negQuery = _parser.ParseFromString(queryWithNegation);

        var negResults = processor.ProcessQuery(negQuery) as SparqlResultSet;

        if (negResults == null) Assert.Fail("Did not get a SPARQL Result Set for the Negation Query");

        Console.WriteLine("Negation Results");
        TestTools.ShowResults(negResults);
        Console.WriteLine();

        Assert.True(negResults.IsEmpty, "Result Set should be empty");
    }

    private const string TestData = @"<http://r> <http://r> <http://r> .";

    private const string MinusQuery = @"
SELECT *
WHERE
{
    { ?s ?p ?o }
    MINUS
    { ?s ?p ?o }
}
";

    private const string UnionQuery = @"
SELECT *
WHERE
{
    { ?s ?p ?o }
    UNION
    {
        { ?s ?p ?o }
        MINUS
        { ?s ?p ?o }
    }
}
";

    private const string MinusNamedGraphQuery = @"
SELECT *
WHERE
{
    { GRAPH <http://g> { ?s ?p ?o } }
    MINUS
    { GRAPH <http://g> { ?s ?p ?o } }
}
";

    private const string UnionNamedGraphQuery = @"
SELECT *
WHERE
{
    { GRAPH <http://g> { ?s ?p ?o } }
    UNION
    {
        { GRAPH <http://g> { ?s ?p ?o } }
        MINUS
        { GRAPH <http://g> { ?s ?p ?o } }
    }
}
";

    [Fact]
    public void SparqlNegationSimpleMinus()
    {
        Test(MinusQuery, false, 0);
    }

    [Fact]
    public void SparqlNegationSimpleMinusAndUnion()
    {
        Test(UnionQuery, false, 1);
    }

    [Fact]
    public void SparqlNegationMinusWithNamedGraph()
    {
        Test(MinusNamedGraphQuery, true, 0);
    }

    [Fact]
    public void SparqlNegationMinusAndUnionWithNamedGraph()
    {
        Test(UnionNamedGraphQuery, true, 1);
    }

    private static void Test(string query, bool isNamedGraph, int expectedCount)
    {
        IGraph graph = isNamedGraph ? new Graph(new UriNode(new Uri("http://g"))) : new Graph();
        new TurtleParser().Load(graph, new StringReader(TestData));

        IInMemoryQueryableStore store = new TripleStore();
        store.Add(graph);
        IQueryableStorage storage = new InMemoryManager(store);

        using (var resultSet = (SparqlResultSet)storage.Query(query))
        {
            Assert.Equal(expectedCount, resultSet.Count);
        }
    }
}