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
using System.Threading.Tasks;
using Xunit;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Parsing;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query;

public class QueryThreadSafety
{
    private readonly SparqlQueryParser _parser = new SparqlQueryParser();
    private readonly SparqlFormatter _formatter = new SparqlFormatter();
    private readonly ITestOutputHelper _output;

    public QueryThreadSafety(ITestOutputHelper output)
    {
        _output = output;
    }

    private void CheckThreadSafety(String query, bool expectThreadSafe)
    {
        var q = _parser.ParseFromString(query);

        Console.WriteLine(_formatter.Format(q));

        Assert.Equal(expectThreadSafe, q.UsesDefaultDataset);
    }

    [Fact]
    public void SparqlQueryThreadSafetyBasic()
    {
        var query = "SELECT * WHERE { }";
        CheckThreadSafety(query, true);
        query = "SELECT * WHERE { ?s ?p ?o }";
        CheckThreadSafety(query, true);
        query = "SELECT * WHERE { GRAPH ?g { ?s ?p ?o } }";
        CheckThreadSafety(query, false);
        query = "SELECT * WHERE { ?s ?p ?o . OPTIONAL { ?s a ?type } GRAPH ?g { ?s ?p ?o } }";
        CheckThreadSafety(query, false);
    }

    [Fact]
    public void SparqlQueryThreadSafetyFromClauses()
    {
        var query = "SELECT * FROM <test:test> WHERE { }";
        CheckThreadSafety(query, false);
        query = "SELECT * FROM NAMED <test:test> WHERE { }";
        CheckThreadSafety(query, true);
        query = "SELECT * FROM <test:test> WHERE { GRAPH ?g { } }";
        CheckThreadSafety(query, false);
        query = "SELECT * FROM NAMED <test:test> WHERE { GRAPH ?g { } }";
        CheckThreadSafety(query, false);
    }

    [Fact]
    public void SparqlQueryThreadSafetySubqueries()
    {
        var query = "SELECT * WHERE { { SELECT * WHERE { } } }";
        CheckThreadSafety(query, true);
        query = "SELECT * WHERE { { SELECT * WHERE { ?s ?p ?o } } }";
        CheckThreadSafety(query, true);
        query = "SELECT * WHERE { { SELECT * WHERE { GRAPH ?g { ?s ?p ?o } } } }";
        CheckThreadSafety(query, false);
        query = "SELECT * WHERE { { SELECT * WHERE { ?s ?p ?o . OPTIONAL { ?s a ?type } GRAPH ?g { ?s ?p ?o } } } }";
        CheckThreadSafety(query, false);
    }

    [Fact]
    public void SparqlQueryThreadSafetySubqueries2()
    {
        var query = "SELECT * WHERE { ?s ?p ?o { SELECT * WHERE { } } }";
        CheckThreadSafety(query, true);
        query = "SELECT * WHERE { ?s ?p ?o { SELECT * WHERE { { SELECT * WHERE { } } } } }";
        CheckThreadSafety(query, true);
        query = "SELECT * WHERE { ?s ?p ?o { SELECT * WHERE { { SELECT * WHERE { GRAPH ?g { } } } } } }";
        CheckThreadSafety(query, false);
    }

    [Fact]
    public void SparqlQueryThreadSafetyExpressions()
    {
        var query = "SELECT * WHERE { FILTER (EXISTS { GRAPH ?g { ?s ?p ?o } }) }";
        CheckThreadSafety(query, false);
        query = "SELECT * WHERE { BIND(EXISTS { GRAPH ?g { ?s ?p ?o } } AS ?test) }";
        CheckThreadSafety(query, false);
        query = "SELECT * WHERE { FILTER (EXISTS { ?s ?p ?o }) }";
        CheckThreadSafety(query, true);
        query = "SELECT * WHERE { BIND(EXISTS { ?s ?p ?o } AS ?test) }";
        CheckThreadSafety(query, true);

    }

    [Fact(SkipExceptions = [typeof(PlatformNotSupportedException)])]
    public void SparqlQueryThreadSafeEvaluation()
    {
        TestTools.TestInMTAThread(SparqlQueryThreadSafeEvaluationActual);
    }

    [Fact(SkipExceptions = [typeof(PlatformNotSupportedException)])]
    public void SparqlQueryAndUpdateThreadSafeEvaluation()
    {
        for (var i = 1; i <= 10; i++)
        {
            _output.WriteLine("Run #" + i);
            TestTools.TestInMTAThread(SparqlQueryAndUpdateThreadSafeEvaluationActual);
            _output.WriteLine(string.Empty);
        }
    }

    private void SparqlQueryThreadSafeEvaluationActual()
    {
        var query1 = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH <http://example.org/1> { ?s ?p ?o } }";
        var query2 = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH <http://example.org/2> { ?s ?p ?o } }";

        var q1 = _parser.ParseFromString(query1);
        var q2 = _parser.ParseFromString(query2);
        Assert.False(q1.UsesDefaultDataset, "Query 1 should not be thread safe");
        Assert.False(q2.UsesDefaultDataset, "Query 2 should not be thread safe");

        var dataset = new InMemoryDataset();
        var g = new Graph(new UriNode(new Uri("http://example.org/1")));
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        var h = new Graph(new UriNode(new Uri("http://example.org/2")));
        h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
        h.BaseUri = new Uri("http://example.org/2");

        dataset.AddGraph(g);
        dataset.AddGraph(h);
        var processor = new LeviathanQueryProcessor(dataset);

        var t1 = Task.Factory.StartNew(() => QueryWithGraph(q1, processor));
        var t2 = Task.Factory.StartNew(() => QueryWithGraph(q2, processor));
        Task.WaitAll(t1, t2);
        var gQuery = t1.Result;
        var hQuery = t2.Result;
        Assert.Equal(g, gQuery);
        Assert.Equal(h, hQuery);
        Assert.NotEqual(g, h);
    }

    private void SparqlQueryAndUpdateThreadSafeEvaluationActual()
    {
        var query1 = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH <http://example.org/1> { ?s ?p ?o } }";
        var query2 = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH <http://example.org/2> { ?s ?p ?o } }";
        var query3 = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH <http://example.org/3> { ?s ?p ?o } }";
        var update1 = "INSERT DATA { GRAPH <http://example.org/3> { <ex:subj> <ex:pred> <ex:obj> } }";

        var q1 = _parser.ParseFromString(query1);
        var q2 = _parser.ParseFromString(query2);
        var q3 = _parser.ParseFromString(query3);
        Assert.False(q1.UsesDefaultDataset, "Query 1 should not be thread safe");
        Assert.False(q2.UsesDefaultDataset, "Query 2 should not be thread safe");
        Assert.False(q3.UsesDefaultDataset, "Query 3 should not be thread safe");

        var parser = new SparqlUpdateParser();
        var cmds = parser.ParseFromString(update1);

        var dataset = new InMemoryDataset();
        var g = new Graph(new UriNode(new Uri("http://example.org/1")));
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        var h = new Graph(new UriNode(new Uri("http://example.org/2")));
        h.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
        var i = new Graph(new UriNode(new Uri("http://example.org/3")));

        dataset.AddGraph(g);
        dataset.AddGraph(h);
        dataset.AddGraph(i);
        var processor = new LeviathanQueryProcessor(dataset);
        var upProcessor = new LeviathanUpdateProcessor(dataset);

        var d = new QueryWithGraphDelegate(QueryWithGraph);
        var d2 = new RunUpdateDelegate(RunUpdate);
        var t1 = Task.Factory.StartNew(() => QueryWithGraph(q1, processor));
        var t2 = Task.Factory.StartNew(() => QueryWithGraph(q2, processor));
        var t3 = Task.Factory.StartNew(() => QueryWithGraph(q3, processor));
        var t4 = Task.Factory.StartNew(() => RunUpdate(cmds, upProcessor));
        Task.WaitAll(t1, t2, t3, t4);
        var gQuery = t1.Result;
        var hQuery = t2.Result;
        var iQuery = t3.Result;

        Assert.Equal(g, gQuery);
        Assert.Equal(h, hQuery);
        if (iQuery.IsEmpty)
        {
            _output.WriteLine("Query 3 executed before the INSERT DATA command - running again to get the resulting graph");
            iQuery = QueryWithGraph(q3, processor);
        }
        else
        {
            _output.WriteLine("Query 3 executed after the INSERT DATA command");
        }
        //Test iQuery against an empty Graph
        Assert.False(iQuery.IsEmpty, "Graph should not be empty as INSERT DATA should have inserted a Triple");
        Assert.NotEqual(new Graph(), iQuery);

        Assert.NotEqual(g, h);
    }

    private delegate IGraph QueryWithGraphDelegate(SparqlQuery q, ISparqlQueryProcessor processor);

    private IGraph QueryWithGraph(SparqlQuery q, ISparqlQueryProcessor processor)
    {
        var results = processor.ProcessQuery(q);
        if (results is IGraph)
        {
            return (IGraph)results;
        }
        else
        {
            Assert.Fail("Query did not produce a Graph as expected");
        }
        return null;
    }

    private delegate SparqlResultSet QueryWithResultsDelegate(SparqlQuery q, ISparqlQueryProcessor processor);

    private delegate void RunUpdateDelegate(SparqlUpdateCommandSet cmds, ISparqlUpdateProcessor processor);

    private void RunUpdate(SparqlUpdateCommandSet cmds, ISparqlUpdateProcessor processor)
    {
        processor.ProcessCommandSet(cmds);
    }

    private SparqlResultSet QueryWithResults(SparqlQuery q, ISparqlQueryProcessor processor)
    {
        var results = processor.ProcessQuery(q);
        if (results is SparqlResultSet)
        {
            return (SparqlResultSet)results;
        }
        else
        {
            Assert.Fail("Query did not produce a Result Set as expected");
        }
        return null;
    }
}
