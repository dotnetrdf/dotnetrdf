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

using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query;


public class QueryTimeouts
{
    private readonly long[] _timeouts = { 50, 100, 250, 500, 1000 };
    private readonly SparqlQueryParser _parser = new SparqlQueryParser();
    private readonly ITestOutputHelper _output;

    public QueryTimeouts(ITestOutputHelper output)
    {
        _output = output;
    }

    private ISparqlDataset AsDataset(IInMemoryQueryableStore store)
    {
        if (store.Graphs.Count == 1)
        {
            return new InMemoryDataset(store, store.Graphs.First().Name);
        }
        else
        {
            return new InMemoryDataset(store);
        }
    }

    private void TestProductTimeout(IGraph data, string query, bool useProcessorTimeout,
        long defaultProcessorTimeout, int expectedResults)
    {
        _output.WriteLine("Maximum Expected Results: " + expectedResults);
        _output.WriteLine(string.Empty);

        _output.WriteLine(useProcessorTimeout
            ? "Processor Timeout setting in use"
            : "Per Query Timeout setting in use");
        _output.WriteLine(string.Empty);

        var store = new TripleStore();
        store.Add(data);
        var dataset = AsDataset(store);

        var q = _parser.ParseFromString(query);

        var formatter = new SparqlFormatter();


        //Evaluate for each Timeout
        foreach (var t in _timeouts)
        {
            var processor = new LeviathanQueryProcessor(dataset,
                options => { options.QueryExecutionTimeout = useProcessorTimeout ? t : defaultProcessorTimeout; });
            //Set the Timeout and ask for Partial Results
            if (!useProcessorTimeout)
            {
                q.Timeout = t;
            }

            q.PartialResultsOnTimeout = true;

            //Check that the reported Timeout matches the expected
            var context = new SparqlEvaluationContext(q, null, new LeviathanQueryOptions());
            long expected;
            if (useProcessorTimeout)
            {
                expected = t;
            }
            else
            {
                if (defaultProcessorTimeout > 0 && t <= defaultProcessorTimeout)
                {
                    expected = t;
                }
                else if (defaultProcessorTimeout == 0)
                {
                    expected = t;
                }
                else
                {
                    expected = defaultProcessorTimeout;
                }
            }

            Assert.Equal(expected, context.CalculateTimeout( useProcessorTimeout ? t : defaultProcessorTimeout));

            //Run the Query
            var results = processor.ProcessQuery(q);
            Assert.IsType<SparqlResultSet>(results, exactMatch: false);
            if (results is SparqlResultSet rset)
            {
                _output.WriteLine("Requested Timeout: " + t + " - Actual Timeout: " + expected + "ms - Results: " +
                                  rset.Count + " - Query Time: " + q.QueryExecutionTime);
                Assert.True(rset.Count <= expectedResults, "Results should be <= expected");
            }
        }
    }

    private void TestProductTimeoutGlobalOverride(IGraph data, string query, long globalTimeout,
        int expectedResults)
    {
        TestProductTimeout(data, query, false, globalTimeout, expectedResults);
    }

    [Fact]
    public void SparqlQueryTimeout()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . ?s ?p2 ?o2 . ?a ?b ?c }";
        var q = _parser.ParseFromString(query);
        q.Timeout = 1;

        var store = new TripleStore();
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        store.Add(g);
        var processor = new LeviathanQueryProcessor(AsDataset(store));
        Assert.Throws<RdfQueryTimeoutException>(() =>
        {
            //Try multiple times because sometimes machine load may mean we don't timeout
            for (var i = 0; i < 10; i++)
            {
                processor.ProcessQuery(q);
            }
        });
    }

    [Fact]
    public void SparqlQueryTimeoutDuringProduct()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

        var query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z }";
        TestProductTimeout(g, query, false, 180000, g.Triples.Count * g.Triples.Count);
    }

    [Fact]
    public void SparqlQueryTimeoutDuringProduct2()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

        var query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z . ?a ?b ?c }";
        TestProductTimeout(g, query, false, 180000, g.Triples.Count * g.Triples.Count * g.Triples.Count);
    }

    [Fact]
    public void SparqlQueryTimeoutGlobalDuringProduct()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

        var query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z }";
        TestProductTimeout(g, query, true, 0,g.Triples.Count * g.Triples.Count);
    }

    [Fact]
    public void SparqlQueryTimeoutGlobalDuringProduct2()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

        var query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z . ?a ?b ?c }";
        TestProductTimeout(g, query, true, 0,g.Triples.Count * g.Triples.Count * g.Triples.Count);
    }

    [Fact]
    public void SparqlQueryTimeoutDuringProductOverriddenByGlobal()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

        var query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z }";
        TestProductTimeoutGlobalOverride(g, query, 1000, g.Triples.Count * g.Triples.Count);
    }

    [Fact]
    public void SparqlQueryTimeoutDuringProductOverriddenByGlobal2()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

        var query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z . ?a ?b ?c }";
        TestProductTimeoutGlobalOverride(g, query, 1000, g.Triples.Count * g.Triples.Count * g.Triples.Count);
    }

    [Fact]
    public void SparqlQueryTimeoutDuringProductNotOverriddenByGlobal()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

        var query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z }";
        TestProductTimeoutGlobalOverride(g, query, 0, g.Triples.Count * g.Triples.Count);
    }

    [Fact]
    public void SparqlQueryTimeoutDuringProductNotOverriddenByGlobal2()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

        const string query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z . ?a ?b ?c }";
        TestProductTimeoutGlobalOverride(g, query, 0, g.Triples.Count * g.Triples.Count * g.Triples.Count);
    }

    [Fact]
    public void SparqlQueryTimeoutDuringProductLazy()
    {
        const string query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z . ?a ?b ?c }";
        var q = _parser.ParseFromString(query);
        q.Timeout = 1;

        var store = new TripleStore();
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        store.Add(g);
        var processor = new LeviathanQueryProcessor(AsDataset(store));
        Assert.Throws<RdfQueryTimeoutException>(() =>
        {
            processor.ProcessQuery(q);
        });
    }

    [Fact(Skip = "in practice it is surprisingly easy to compute this in under a millisecond given a reasonable machine since it only needs to compute one value")]
    public void SparqlQueryTimeoutDuringProductLazy2()
    {
        const string query = "ASK WHERE { ?s ?p ?o . ?x ?y ?z }";
        var q = _parser.ParseFromString(query);
        q.Timeout = 1;

        var store = new TripleStore();
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        store.Add(g);
        var processor = new LeviathanQueryProcessor(AsDataset(store));
        Assert.Throws<RdfQueryTimeoutException>(() =>
        {
            //Try multiple times because sometimes machine load may mean we don't timeout
            for (var i = 0; i < 100; i++)
            {
                processor.ProcessQuery(q);
            }
        });
    }

    [Fact]
    public void SparqlQueryTimeoutNone()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

        const string query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z }";

        var q = _parser.ParseFromString(query);
        var store = new TripleStore();
        store.Add(g);

        var processor = new LeviathanQueryProcessor(AsDataset(store),
            options => { options.QueryExecutionTimeout = 0; });
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            _output.WriteLine("Results: " + rset.Count + " - Query Time: " + q.QueryExecutionTime);
            Assert.Equal(g.Triples.Count * g.Triples.Count, rset.Count);
        }
    }

    [Fact]
    public void SparqlQueryTimeoutMinimal()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

        const string query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z . ?a ?b ?c }";

        var q = _parser.ParseFromString(query);
        q.PartialResultsOnTimeout = true;
        var store = new TripleStore();
        store.Add(g);

        var processor = new LeviathanQueryProcessor(AsDataset(store),
            options => { options.QueryExecutionTimeout = 1; });
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            _output.WriteLine("Results: " + rset.Count + " - Query Time: " + q.QueryExecutionTime);
            Assert.True(rset.Count < (g.Triples.Count * g.Triples.Count * g.Triples.Count));
        }
    }
}
