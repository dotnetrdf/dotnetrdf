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
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Optimisation;

namespace VDS.RDF.Query;

public class SparqlTests2
{
    private readonly ITestOutputHelper _output;

    public SparqlTests2(ITestOutputHelper output)
    {
        _output = output;
    }

    private static ISparqlDataset AsDataset(IInMemoryQueryableStore store)
    {
        return store.Graphs.Count == 1 ? new InMemoryDataset(store, store.Graphs.First().Name) : new InMemoryDataset(store);
    }

    [Fact]
    public void SparqlBind()
    {
        const string query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace + "> SELECT ?triple WHERE { ?s ?p ?o . BIND(fn:concat(STR(?s), ' ', STR(?p), ' ', STR(?o)) AS ?triple) }";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        var processor = new LeviathanQueryProcessor(AsDataset(store));
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
            }
            Assert.True(rset.Count > 0, "Expected 1 or more results");
        }
    }

    [Fact]
    public void SparqlBindLazy()
    {
        var optimisers = new IAlgebraOptimiser[] { new LazyBgpOptimiser() };

        var query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace +
                    "> SELECT ?triple WHERE { ?s ?p ?o . BIND(fn:concat(STR(?s), ' ', STR(?p), ' ', STR(?o)) AS ?triple) } LIMIT 1";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        Assert.True(q.ToAlgebra(true, optimisers).ToString().Contains("LazyBgp"),
            "Should have been optimised to use a Lazy BGP");

        var processor = new LeviathanQueryProcessor(AsDataset(store),
            options => options.AlgebraOptimisers = optimisers);
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
            }

            Assert.True(rset.Count == 1, "Expected exactly 1 results");
            Assert.True(rset.All(r => r.HasValue("triple")), "All Results should have had a value for ?triple");
        }
    }

    [Fact]
    public void SparqlBindLazy2()
    {
        var optimisers = new IAlgebraOptimiser[] { new LazyBgpOptimiser() };
        var query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace +
                    "> SELECT * WHERE { ?s ?p ?o . BIND(fn:concat(STR(?s), ' ', STR(?p), ' ', STR(?o)) AS ?triple) } LIMIT 10";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        Assert.True(q.ToAlgebra(true, optimisers).ToString().Contains("LazyBgp"),
            "Should have been optimised to use a Lazy BGP");

        var processor = new LeviathanQueryProcessor(AsDataset(store),
            options => options.AlgebraOptimisers = optimisers);
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
            }

            Assert.True(rset.Count == 10, "Expected exactly 10 results");
            Assert.True(
                rset.All(r => r.HasValue("s") && r.HasValue("p") && r.HasValue("o") && r.HasValue("triple")),
                "Expected ?s, ?p, ?o and ?triple values for every result");
        }
    }

    [Fact]
    public void SparqlBindLazy3()
    {
        var optimisers = new IAlgebraOptimiser[] { new LazyBgpOptimiser() };
        var query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace +
                    "> SELECT * WHERE { ?s ?p ?o . BIND(fn:concat(STR(?s), ' ', STR(?p), ' ', STR(?o)) AS ?triple) } LIMIT 10 OFFSET 10";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        Assert.True(q.ToAlgebra(true, optimisers).ToString().Contains("LazyBgp"),
            "Should have been optimised to use a Lazy BGP");

        var processor =
            new LeviathanQueryProcessor(AsDataset(store), options => options.AlgebraOptimisers = optimisers);
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
            }

            Assert.True(rset.Count == 10, "Expected exactly 10 results");
            Assert.True(
                rset.All(r => r.HasValue("s") && r.HasValue("p") && r.HasValue("o") && r.HasValue("triple")),
                "Expected ?s, ?p, ?o and ?triple values for every result");
        }
    }

    //[Fact]
    //public void SparqlBindNested()
    //{
    //    String query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace + "> SELECT ?triple WHERE { ?s ?p ?o .{ BIND(fn:concat(STR(?s), ' ', STR(?p), ' ', STR(?o)) AS ?triple) } FILTER(BOUND(?triple))}";

    //    TripleStore store = new TripleStore();
    //    Graph g = new Graph();
    //    FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
    //    store.Add(g);

    //    SparqlQueryParser parser = new SparqlQueryParser();
    //    SparqlQuery q = parser.ParseFromString(query);

    //    Object results = q.Evaluate(store);
    //    if (results is SparqlResultSet)
    //    {
    //        SparqlResultSet rset = (SparqlResultSet)results;
    //        foreach (SparqlResult r in rset)
    //        {
    //            _output.WriteLine(r.ToString());
    //        }
    //        Assert.True(rset.Count == 0, "Expected no results");
    //    }
    //    else
    //    {
    //        Assert.True(false, "Expected a SPARQL Result Set");
    //    }
    //}

    [Fact]
    public void SparqlBindIn10Standard()
    {
        var query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace + "> SELECT ?triple WHERE { ?s ?p ?o . BIND(fn:concat(STR(?s), ' ', STR(?p), ' ', STR(?o)) AS ?triple) }";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_0);
        Assert.Throws<RdfParseException>(() =>
        {
            SparqlQuery _ = parser.ParseFromString(query);
        });
    }

    [Fact]
    public void SparqlBindToExistingVariable()
    {
        var query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace + "> SELECT * WHERE { ?s ?p ?o . BIND(?s AS ?p) }";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser();
        Assert.Throws<RdfParseException>(() => { SparqlQuery _ = parser.ParseFromString(query); });
    }

    [Fact]
    public void SparqlBindToExistingVariableLazy()
    {
        var query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace +
                    "> SELECT * WHERE { ?s ?p ?o . BIND(?s AS ?p) } LIMIT 1";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser();
        Assert.Throws<RdfParseException>(() =>
        {
            SparqlQuery _ = parser.ParseFromString(query);
        });
    }

    [Fact]
    public void SparqlBindScope1()
    {
        var query = @"PREFIX : <http://www.example.org>
 SELECT *
 WHERE {
    {
    :s :p ?o .
    :s :q ?o1 .
    }
    BIND((1+?o) AS ?o1)
 }";

        var parser = new SparqlQueryParser();

        Assert.Throws<RdfParseException>(() => parser.ParseFromString(query));
    }

    [Fact]
    public void SparqlBindScope2()
    {
        var query = @"PREFIX : <http://www.example.org>
 SELECT *
 WHERE {
    :s :p ?o .
    { BIND((1 + ?o) AS ?o1) } UNION { BIND((2 + ?o) AS ?o1) }
 }";

        var parser = new SparqlQueryParser();
        parser.ParseFromString(query);
    }

    [Fact]
    public void SparqlBindScope3()
    {
        var query = @" PREFIX : <http://www.example.org>
 SELECT *
 WHERE {
    :s :p ?o .
    :s :q ?o1
    { BIND((1+?o) AS ?o1) }
 }";

        var parser = new SparqlQueryParser();
        parser.ParseFromString(query);
    }

    [Fact]
    public void SparqlBindScope4()
    {
        var query = @" PREFIX : <http://www.example.org>
 SELECT *
 WHERE {
    { 
    :s :p ?o .
    :s :q ?o1
    }
    { BIND((1+?o) AS ?o1) }
 }";

        var parser = new SparqlQueryParser();
        parser.ParseFromString(query);
    }

    [Fact]
    public void SparqlBindScope5()
    {
        var query = @"PREFIX : <http://example.org>
SELECT *
WHERE
{
  GRAPH ?g { :s :p ?o }
  BIND (?g AS ?in)
}";

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        _output.WriteLine(q.ToString());

        ISparqlAlgebra algebra = q.ToAlgebra();
        _output.WriteLine(algebra.ToString());
        Assert.IsType<Select>(algebra, exactMatch: false);

        algebra = ((IUnaryOperator)algebra).InnerAlgebra;
        Assert.IsType<Extend>(algebra, exactMatch: false);
    }

    [Fact]
    public void SparqlBindScope6()
    {
        var query = @"PREFIX : <http://example.org>
SELECT *
WHERE
{
  {
    GRAPH ?g { :s :p ?o }
    BIND (?g AS ?in)
  }
  UNION
  {
    :s :p ?o .
    BIND('default' AS ?in)
  }
}";

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        _output.WriteLine(q.ToString());

        ISparqlAlgebra algebra = q.ToAlgebra();
        _output.WriteLine(algebra.ToString());
        Assert.IsType<Select>(algebra, exactMatch: false);

        algebra = ((IUnaryOperator)algebra).InnerAlgebra;
        Assert.IsType<Union>(algebra, exactMatch: false);

        IUnion union = (Union)algebra;
        ISparqlAlgebra lhs = union.Lhs;
        Assert.IsType<Extend>(lhs, exactMatch: false);

        ISparqlAlgebra rhs = union.Rhs;
        Assert.IsType<Join>(rhs, exactMatch: false);
    }

    [Fact]
    public void SparqlLet()
    {
        var query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace + "> SELECT ?triple WHERE { ?s ?p ?o . LET (?triple := fn:concat(STR(?s), ' ', STR(?p), ' ', STR(?o))) }";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser(SparqlQuerySyntax.Extended);
        SparqlQuery q = parser.ParseFromString(query);

        var processor = new LeviathanQueryProcessor(AsDataset(store));
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
            }
            Assert.True(rset.Count > 0, "Expected 1 or more results");
        }
    }

    [Fact]
    public void SparqlLetIn11Standard()
    {
        var query = "PREFIX fn: <" + XPathFunctionFactory.XPathFunctionsNamespace + "> SELECT ?triple WHERE { ?s ?p ?o . LET (?triple := fn:concat(STR(?s), ' ', STR(?p), ' ', STR(?o))) }";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser(SparqlQuerySyntax.Sparql_1_1);
        Assert.Throws<RdfParseException>(() =>
        {
            SparqlQuery _ = parser.ParseFromString(query);
        });
    }

    //[Fact]
    //public void SparqlSubQueryLazy()
    //{
    //    String query = "SELECT * WHERE { {SELECT * WHERE { ?s ?p ?o}}} LIMIT 1";

    //    TripleStore store = new TripleStore();
    //    Graph g = new Graph();
    //    FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
    //    store.Add(g);

    //    SparqlQueryParser parser = new SparqlQueryParser();
    //    SparqlQuery q = parser.ParseFromString(query);

    //    _output.WriteLine(q.ToAlgebra().ToString());
    //    Assert.True(q.ToAlgebra().ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");
    //    _output.WriteLine(string.Empty);

    //    Object results = q.Evaluate(store);
    //    if (results is SparqlResultSet)
    //    {
    //        SparqlResultSet rset = (SparqlResultSet)results;
    //        foreach (SparqlResult r in rset)
    //        {
    //            _output.WriteLine(r.ToString());
    //        }
    //        Assert.True(rset.Count == 1, "Expected exactly 1 results");
    //    }
    //    else
    //    {
    //        Assert.True(false, "Expected a SPARQL Result Set");
    //    }
    //}

    //[Fact]
    //public void SparqlSubQueryLazy2()
    //{
    //    String query = "SELECT * WHERE { {SELECT * WHERE { ?s ?p ?o}}} LIMIT 10";

    //    TripleStore store = new TripleStore();
    //    Graph g = new Graph();
    //    FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
    //    store.Add(g);

    //    SparqlQueryParser parser = new SparqlQueryParser();
    //    SparqlQuery q = parser.ParseFromString(query);

    //    _output.WriteLine(q.ToAlgebra().ToString());
    //    Assert.True(q.ToAlgebra().ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");
    //    _output.WriteLine(string.Empty);

    //    Object results = q.Evaluate(store);
    //    if (results is SparqlResultSet)
    //    {
    //        SparqlResultSet rset = (SparqlResultSet)results;
    //        foreach (SparqlResult r in rset)
    //        {
    //            _output.WriteLine(r.ToString());
    //        }
    //        Assert.True(rset.Count == 10, "Expected exactly 10 results");
    //    }
    //    else
    //    {
    //        Assert.True(false, "Expected a SPARQL Result Set");
    //    }
    //}

    //[Fact]
    //public void SparqlSubQueryLazy3()
    //{
    //    String query = "SELECT * WHERE { {SELECT * WHERE { ?s ?p ?o}}} LIMIT 10 OFFSET 10";

    //    TripleStore store = new TripleStore();
    //    Graph g = new Graph();
    //    FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
    //    store.Add(g);

    //    SparqlQueryParser parser = new SparqlQueryParser();
    //    SparqlQuery q = parser.ParseFromString(query);

    //    _output.WriteLine(q.ToAlgebra().ToString());
    //    Assert.True(q.ToAlgebra().ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");
    //    _output.WriteLine(string.Empty);

    //    Object results = q.Evaluate(store);
    //    if (results is SparqlResultSet)
    //    {
    //        SparqlResultSet rset = (SparqlResultSet)results;
    //        foreach (SparqlResult r in rset)
    //        {
    //            _output.WriteLine(r.ToString());
    //        }
    //        Assert.True(rset.Count == 10, "Expected exactly 10 results");
    //    }
    //    else
    //    {
    //        Assert.True(false, "Expected a SPARQL Result Set");
    //    }
    //}

    //[Fact]
    //public void SparqlSubQueryLazyComplex()
    //{
    //    String query = "SELECT * WHERE { ?s a <http://example.org/vehicles/Car> . {SELECT * WHERE { ?s <http://example.org/vehicles/Speed> ?speed}}} LIMIT 1";

    //    TripleStore store = new TripleStore();
    //    Graph g = new Graph();
    //    FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
    //    store.Add(g);

    //    SparqlQueryParser parser = new SparqlQueryParser();
    //    SparqlQuery q = parser.ParseFromString(query);

    //    _output.WriteLine(q.ToAlgebra().ToString());
    //    Assert.True(q.ToAlgebra().ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");
    //    _output.WriteLine(string.Empty);

    //    Object results = q.Evaluate(store);
    //    if (results is SparqlResultSet)
    //    {
    //        SparqlResultSet rset = (SparqlResultSet)results;
    //        foreach (SparqlResult r in rset)
    //        {
    //            _output.WriteLine(r.ToString());
    //        }
    //        Assert.True(rset.Count == 1, "Expected exactly 1 results");
    //    }
    //    else
    //    {
    //        Assert.True(false, "Expected a SPARQL Result Set");
    //    }
    //}

    //[Fact]
    //public void SparqlSubQueryLazyComplex2()
    //{
    //    String query = "SELECT * WHERE { ?s a <http://example.org/vehicles/Car> . {SELECT * WHERE { ?s <http://example.org/vehicles/Speed> ?speed}}} LIMIT 5";

    //    TripleStore store = new TripleStore();
    //    Graph g = new Graph();
    //    FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
    //    store.Add(g);

    //    SparqlQueryParser parser = new SparqlQueryParser();
    //    SparqlQuery q = parser.ParseFromString(query);

    //    _output.WriteLine(q.ToAlgebra().ToString());
    //    Assert.True(q.ToAlgebra().ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");
    //    _output.WriteLine(string.Empty);

    //    Object results = q.Evaluate(store);
    //    if (results is SparqlResultSet)
    //    {
    //        SparqlResultSet rset = (SparqlResultSet)results;
    //        foreach (SparqlResult r in rset)
    //        {
    //            _output.WriteLine(r.ToString());
    //        }
    //        Assert.True(rset.Count <= 5, "Expected at most 5 results");
    //    }
    //    else
    //    {
    //        Assert.True(false, "Expected a SPARQL Result Set");
    //    }
    //}

    [Fact]
    public void SparqlOrderBySubjectLazyAscending()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . } ORDER BY ?s LIMIT 1";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        Assert.True(q.ToAlgebra(true, LeviathanOptimiser.AlgebraOptimisers).ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");

        var processor = new LeviathanQueryProcessor(AsDataset(store));
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
            }
            Assert.True(rset.Count == 1, "Expected exactly 1 results");
        }
    }

    [Fact]
    public void SparqlOrderBySubjectLazyAscendingExplicit()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . } ORDER BY ASC(?s) LIMIT 1";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        Assert.True(q.ToAlgebra(true, LeviathanOptimiser.AlgebraOptimisers).ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");

        var processor = new LeviathanQueryProcessor(AsDataset(store));
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
            }
            Assert.True(rset.Count == 1, "Expected exactly 1 results");
        }
    }

    [Fact]
    public void SparqlOrderBySubjectLazyDescending()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . } ORDER BY DESC(?s) LIMIT 1";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        Assert.True(q.ToAlgebra(true, LeviathanOptimiser.AlgebraOptimisers).ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");

        var processor = new LeviathanQueryProcessor(AsDataset(store));
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
            }
            Assert.True(rset.Count == 1, "Expected exactly 1 results");
        }
    }

    [Fact]
    public void SparqlOrderByPredicateLazyAscending()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . } ORDER BY ?p LIMIT 1";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        Assert.True(q.ToAlgebra(true, LeviathanOptimiser.AlgebraOptimisers).ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");

        var processor = new LeviathanQueryProcessor(AsDataset(store));
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
            }
            Assert.True(rset.Count == 1, "Expected exactly 1 results");
        }
    }

    [Fact]
    public void SparqlOrderByPredicateLazyAscendingExplicit()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . } ORDER BY ASC(?p) LIMIT 1";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        Assert.True(q.ToAlgebra(true, LeviathanOptimiser.AlgebraOptimisers).ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");

        var processor = new LeviathanQueryProcessor(AsDataset(store));
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
            }
            Assert.True(rset.Count == 1, "Expected exactly 1 results");
        }
    }

    [Fact]
    public void SparqlOrderByPredicateLazyDescending()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . } ORDER BY DESC(?p) LIMIT 1";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        Assert.True(q.ToAlgebra(true, LeviathanOptimiser.AlgebraOptimisers).ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");

        var processor = new LeviathanQueryProcessor(AsDataset(store));
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
            }
            Assert.True(rset.Count == 1, "Expected exactly 1 results");
        }
    }

    [Fact]
    public void SparqlOrderByComplexLazy()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . } ORDER BY ?s DESC(?p) LIMIT 5";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        ISparqlAlgebra algebra = q.ToAlgebra(true, LeviathanOptimiser.AlgebraOptimisers);
        Assert.True(algebra.ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");

        ISparqlQueryProcessor processor = new LeviathanQueryProcessor(AsDataset(store));
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
            }
            Assert.True(rset.Count == 5, "Expected exactly 5 results");
        }
    }

    [Fact]
    [Trait("Coverage", "Skip")]
    public void SparqlOrderByComplexLazyPerformance()
    {
        var query = "SELECT * WHERE { ?s ?p ?o . } ORDER BY ?s DESC(?p) LIMIT 5";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "dataset_50.ttl.gz"));
        store.Add(g);

        var parser = new SparqlQueryParser();

        //First do with Optimisation
        var timer = new Stopwatch();
        SparqlQuery q = parser.ParseFromString(query);

        Assert.True(q.ToAlgebra(true, LeviathanOptimiser.AlgebraOptimisers).ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");

        ISparqlDataset dataset = AsDataset(store);
        var processor = new LeviathanQueryProcessor(dataset);
        timer.Start();
        var results = processor.ProcessQuery(q);
        timer.Stop();
        _output.WriteLine("Took " + timer.Elapsed + " to execute when Optimised");
        timer.Reset();
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset1)
        {
            foreach (SparqlResult r in rset1)
            {
                _output.WriteLine(r.ToString());
            }
            Assert.True(rset1.Count == 5, "Expected exactly 5 results");
        }

        //Then do without optimisation
        processor = new LeviathanQueryProcessor(dataset, options => { options.AlgebraOptimisation = false;});
        timer.Start();
        results = processor.ProcessQuery(q);
        timer.Stop();
        _output.WriteLine("Took " + timer.Elapsed + " to execute when Unoptimised");
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
            }
            Assert.True(rset.Count == 5, "Expected exactly 5 results");
        }
    }

    [Fact]
    public void SparqlOrderByComplexLazy2()
    {
        var query = "SELECT * WHERE { ?s a ?vehicle . ?s <http://example.org/vehicles/Speed> ?speed } ORDER BY DESC(?speed) LIMIT 3";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        Assert.True(q.ToAlgebra(true, LeviathanOptimiser.AlgebraOptimisers).ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");

        var processor = new LeviathanQueryProcessor(AsDataset(store));
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
            }
            Assert.Equal(3, rset.Count);
        }
    }

    [Fact]
    public void SparqlFilterLazy()
    {
        IEnumerable<IAlgebraOptimiser> optimisers = new List<IAlgebraOptimiser> { new LazyBgpOptimiser() };

        var query =
            "SELECT * WHERE { ?s a ?vehicle . FILTER (SAMETERM(?vehicle, <http://example.org/vehicles/Car>)) } LIMIT 3";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        ISparqlAlgebra algebra = q.ToAlgebra(true, optimisers);
        Assert.True(algebra.ToString().Contains("LazyBgp"), "Should have been optimised to use a Lazy BGP");

        var processor = new LeviathanQueryProcessor(AsDataset(store),
            options => options.AlgebraOptimisers = optimisers);
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
            }

            Assert.True(rset.Count == 3, "Expected exactly 3 results");
        }
    }

    [Fact]
    public void SparqlFilterLazy2()
    {
        // NOTE: The URI for Car is purposefully wrong in this case so no results should be returned
        var optimisers = new IAlgebraOptimiser[] { new LazyBgpOptimiser() };

        var query =
            "SELECT * WHERE { ?s a ?vehicle . FILTER (SAMETERM(?vehicle, <http://example.org/Vehicles/Car>)) } LIMIT 3";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        _output.WriteLine(q.ToAlgebra().ToString());
        Assert.True(q.ToAlgebra(true, optimisers).ToString().Contains("LazyBgp"),
            "Should have been optimised to use a Lazy BGP");
        _output.WriteLine(string.Empty);

        var processor =
            new LeviathanQueryProcessor(AsDataset(store), options => options.AlgebraOptimisers = optimisers);
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
            }

            Assert.True(rset.Count == 0, "Expected no results");
        }
    }

    [Fact]
    public void SparqlFilterLazy3()
    {
        var optimisers = new IAlgebraOptimiser[] { new LazyBgpOptimiser() };

        var query =
            "SELECT * WHERE { ?s a ?vehicle . FILTER (SAMETERM(?vehicle, <http://example.org/vehicles/Car>)) . ?s <http://example.org/vehicles/Speed> ?speed } LIMIT 3";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);
        q.Timeout = 0;

        Assert.True(q.ToAlgebra(true, optimisers).ToString().Contains("LazyBgp"),
            "Should have been optimised to use a Lazy BGP");

        var processor = new LeviathanQueryProcessor(AsDataset(store),
            options =>
            {
                options.QueryExecutionTimeout = 0;
                options.AlgebraOptimisers = optimisers;
            });
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
            }

            Assert.True(rset.Count == 3, "Expected exactly 3 results");
        }
    }

    [Fact]
    public void SparqlFilterLazy4()
    {
        var optimisers = new IAlgebraOptimiser[] { new LazyBgpOptimiser() };

        var query =
            "SELECT * WHERE { ?s a <http://example.org/vehicles/Car> ; <http://example.org/vehicles/Speed> ?speed } LIMIT 3";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        Assert.True(q.ToAlgebra(true, optimisers).ToString().Contains("LazyBgp"),
            "Should have been optimised to use a Lazy BGP");

        var processor =
            new LeviathanQueryProcessor(AsDataset(store), options => options.AlgebraOptimisers = optimisers);
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
            }

            Assert.True(rset.Count == 3, "Expected exactly 3 results");
        }
    }

    [Fact]
    public void SparqlFilterLazyDBPedia()
    {
        var optimisers = new IAlgebraOptimiser[] { new LazyBgpOptimiser() };

        var query = new SparqlParameterizedString();
        query.Namespaces.AddNamespace("rdfs", new Uri(NamespaceMapper.RDFS));
        query.CommandText =
            "SELECT * WHERE {?s ?p ?label . FILTER(ISLITERAL(?label) && LANGMATCHES(LANG(?label), \"en\")) } LIMIT 5";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "rdfserver", "southampton.rdf"));
        store.Add(g);

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        Assert.True(q.ToAlgebra(true, optimisers).ToString().Contains("LazyBgp"),
            "Should have been optimised to use a Lazy BGP");

        var processor =
            new LeviathanQueryProcessor(AsDataset(store), options => options.AlgebraOptimisers = optimisers);
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
            }

            Assert.True(rset.Count == 5, "Expected exactly 5 results");
        }
    }

    [Fact]
    public void SparqlLazyWithAndWithoutOffset()
    {
        var optimisers = new IAlgebraOptimiser[] { new LazyBgpOptimiser() };
        var query =
            "SELECT * WHERE { ?s a ?vehicle . FILTER (SAMETERM(?vehicle, <http://example.org/vehicles/Car>)) } LIMIT 3";
        var query2 =
            "SELECT * WHERE { ?s a ?vehicle . FILTER (SAMETERM(?vehicle, <http://example.org/vehicles/Car>)) } LIMIT 3 OFFSET 3";

        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);
        SparqlQuery q2 = parser.ParseFromString(query2);

        Assert.True(q.ToAlgebra(true, optimisers).ToString().Contains("LazyBgp"),
            "Should have been optimised to use a Lazy BGP");

        Assert.True(q2.ToAlgebra(true, optimisers).ToString().Contains("LazyBgp"),
            "Should have been optimised to use a Lazy BGP");

        var processor =
            new LeviathanQueryProcessor(AsDataset(store), options => options.AlgebraOptimisers = optimisers);
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet rset)
        {
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
            }

            Assert.True(rset.Count == 3, "Expected exactly 3 results");

            var results2 = processor.ProcessQuery(q2);
            Assert.IsType<SparqlResultSet>(results2, exactMatch: false);
            if (results2 is SparqlResultSet rset2)
            {
                foreach (SparqlResult r in rset2)
                {
                    _output.WriteLine(r.ToString());
                }

                Assert.True(rset2.Count == 1, "Expected exactly 1 results");
            }
        }
    }

    [Fact]
    public void SparqlLazyLimitSimple1()
    {
        var optimisers = new IAlgebraOptimiser[] { new LazyBgpOptimiser() };

        const string query = @"PREFIX eg:
<http://example.org/vehicles/> PREFIX rdf:
<http://www.w3.org/1999/02/22-rdf-syntax-ns#> SELECT ?car ?speed WHERE
{ ?car rdf:type eg:Car . ?car eg:Speed ?speed } LIMIT 1";

        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);
        q.AlgebraOptimisers = optimisers;
        var processor = new LeviathanQueryProcessor(new InMemoryDataset(g),
            options => options.AlgebraOptimisers = optimisers);
        var results = processor.ProcessQuery(q);
        Assert.True(results is SparqlResultSet, "Expected a SPARQL results set");
        var rset = results as SparqlResultSet;
        foreach (SparqlResult r in rset)
        {
            _output.WriteLine(r.ToString());
            Assert.Equal(2, r.Count);
        }
    }

    [Fact]
    public void SparqlLazyLimitSimple2()
    {
        var optimisers = new IAlgebraOptimiser[] { new LazyBgpOptimiser() };

        const string query = @"PREFIX eg:
<http://example.org/vehicles/> PREFIX rdf:
<http://www.w3.org/1999/02/22-rdf-syntax-ns#> SELECT ?car ?speed WHERE
{ ?car rdf:type eg:Car . ?car eg:Speed ?speed } LIMIT 20";

            var g = new Graph();
            FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

            var parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);
            var processor = new LeviathanQueryProcessor(new InMemoryDataset(g),
                options => options.AlgebraOptimisers = optimisers);
            var results = processor.ProcessQuery(q);
            Assert.True(results is SparqlResultSet, "Expected a SPARQL results set");
            var rset = results as SparqlResultSet;
            foreach (SparqlResult r in rset)
            {
                _output.WriteLine(r.ToString());
                Assert.Equal(2, r.Count);
            }
    }

    [Fact]
    public void SparqlNestedOptionalCore406()
    {
        IGraph g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "core-406.ttl"));

        SparqlQuery query = new SparqlQueryParser().ParseFromFile(Path.Combine("resources", "core-406.rq"));

        var processor = new LeviathanQueryProcessor(new InMemoryDataset(g));
        var results = processor.ProcessQuery(query) as SparqlResultSet;
        Assert.NotNull(results);

        TestTools.ShowResults(results);

        foreach (SparqlResult result in results)
        {
            Assert.True(result.HasBoundValue("first"), "Row " + result + " failed to contain ?first binding");
        }
    }

    [Fact]
    [Trait("Coverage", "Skip")]
    [Trait("Category", "performance")]
    public void SparqlSubQueryGraphInteractionCore416_Serial()
    {
        var store = new TripleStore();
        store.LoadFromFile(Path.Combine("resources", "core-416.trig"));
        SparqlQuery q = new SparqlQueryParser().ParseFromFile(Path.Combine("resources", "core-416.rq"));
        ISparqlDataset dataset = AsDataset(store);
        var processor = new LeviathanQueryProcessor(dataset,
            options => { options.UsePLinqEvaluation = false;});
        var total = new TimeSpan();
        const int totalRuns = 1000;
        for (var i = 0; i < totalRuns; i++)
        {
            var results = processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);

            if (q.QueryExecutionTime != null)
            {
                total = total + q.QueryExecutionTime.Value;
            }
            if (results.Count != 4) TestTools.ShowResults(results);
            Assert.Equal(4, results.Count);
        }
        _output.WriteLine("Total Execution Time: " + total);
    }

    [Fact]
    [Trait("Coverage", "Skip")]
    [Trait("Category", "performance")]
    public void SparqlSubQueryGraphInteractionCore416_Parallel()
    {
        var store = new TripleStore();
        store.LoadFromFile(Path.Combine("resources", "core-416.trig"));

        SparqlQuery q = new SparqlQueryParser().ParseFromFile(Path.Combine("resources", "core-416.rq"));
        _output.WriteLine(q.ToAlgebra().ToString());
        //SparqlFormatter formatter = new SparqlFormatter();
        //_output.WriteLine(formatter.Format(q));

        ISparqlDataset dataset = AsDataset(store);

        //ExplainQueryProcessor processor = new ExplainQueryProcessor(dataset, ExplanationLevel.OutputToConsoleStdOut | ExplanationLevel.ShowAll | ExplanationLevel.AnalyseNamedGraphs);
        var processor = new LeviathanQueryProcessor(dataset, options =>
        {
            options.UsePLinqEvaluation = true;
        });
        var total = new TimeSpan();
        const int totalRuns = 1000;
        for (var i = 0; i < totalRuns; i++)
        {
            var results = processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);

            if (q.QueryExecutionTime != null)
            {
                _output.WriteLine("Execution Time: " + q.QueryExecutionTime.Value);
                total = total + q.QueryExecutionTime.Value;
            }
            if (results.Count != 4) TestTools.ShowResults(results);
            Assert.Equal(4, results.Count);
        }

        _output.WriteLine("Total Execution Time: " + total);
        // Assert.True(total < new TimeSpan(0, 0, 1 * (totalRuns / 10)));
    }

    [Fact]
    public void SparqlInfiniteLoopCore439_01()
    {
        var store = new TripleStore();
        store.LoadFromFile(Path.Combine("resources", "core-439", "data.trig"));

        SparqlQuery q = new SparqlQueryParser().ParseFromFile(Path.Combine("resources", "core-439", "bad-query.rq"));
        //q.Timeout = 10000;
        _output.WriteLine(q.ToAlgebra().ToString());

        ISparqlDataset dataset = AsDataset(store);

        var processor = new LeviathanQueryProcessor(dataset);
        var results = processor.ProcessQuery(q) as SparqlResultSet;

        Assert.NotNull(results);

        Assert.Equal(10, results.Count);
    }

    [Fact]
    public void SparqlInfiniteLoopCore439_02()
    {
        var store = new TripleStore();
        store.LoadFromFile(Path.Combine("resources", "core-439", "data.trig"));

        SparqlQuery q = new SparqlQueryParser().ParseFromFile(Path.Combine("resources", "core-439", "good-query.rq"));
        //q.Timeout = 3000;
        _output.WriteLine(q.ToAlgebra().ToString());

        ISparqlDataset dataset = AsDataset(store);

        var processor = new LeviathanQueryProcessor(dataset);
        var results = processor.ProcessQuery(q) as SparqlResultSet;
        Assert.NotNull(results);

        Assert.Equal(10, results.Count);
    }

    [Fact]
    public void SparqlInfiniteLoopCore439_03()
    {
        var store = new TripleStore();
        store.LoadFromFile(Path.Combine("resources", "core-439", "data.trig"));

        SparqlQuery q = new SparqlQueryParser().ParseFromFile(Path.Combine("resources", "core-439", "from-query.rq"));
        //q.Timeout = 3000;
        _output.WriteLine(q.ToAlgebra().ToString());

        ISparqlDataset dataset = AsDataset(store);

        var processor = new LeviathanQueryProcessor(dataset);
        var results = processor.ProcessQuery(q) as SparqlResultSet;
        Assert.NotNull(results);

        Assert.Equal(10, results.Count);
    }

    [Fact]
    public void SparqlSubQueryOrderByLimitInteractionCore437()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        var dataset = new InMemoryDataset(g);

        SparqlQuery q = new SparqlQueryParser().ParseFromFile(Path.Combine("resources", "core-437.rq"));
        var processor = new LeviathanQueryProcessor(dataset);
        var results = processor.ProcessQuery(q) as SparqlResultSet;
        Assert.NotNull(results);

        TestTools.ShowResults(results);
    }

    private void RunCore457(String query, bool usePLinq)
    {
        var store = new TripleStore();
        store.LoadFromFile(Path.Combine("resources", "core-457", "data.nq"));
        var dataset = new InMemoryDataset(store);

        SparqlQuery q = new SparqlQueryParser().ParseFromFile(Path.Combine("resources","core-457", query));
        q.Timeout = 15000;
        var processor = new LeviathanQueryProcessor(dataset,
            options => { options.UsePLinqEvaluation = usePLinq;});
        var results = processor.ProcessQuery(q) as SparqlResultSet;
        Assert.NotNull(results);
        Assert.True(q.QueryExecutionTime.HasValue);
        _output.WriteLine(q.QueryExecutionTime.Value.ToString());

        //TestTools.ShowResults(results);
    }

    [Fact(Skip = "the query requires generating ~4.7 million solutions so is fundamentally unsolvable")]
    public void SparqlGraphOptionalInteractionCore457_1()
    {
        RunCore457("optional.rq", true);
    }


    [Fact(Skip = "the query requires generating ~4.7 million solutions so is fundamentally unsolvable")]
    public void SparqlGraphOptionalInteractionCore457_2()
    {
        RunCore457("optional.rq", false);
    }

    [Fact]
    public void SparqlGraphOptionalInteractionCore457_3()
    {
        RunCore457("optional2.rq", true);
    }

    [Fact(Skip = "the query requires generating ~4.7 million solutions so is fundamentally unsolvable")]
    public void SparqlGraphExistsInteractionCore457_1()
    {
        RunCore457("exists.rq", true);
    }

    [Fact(Skip = "the query requires generating ~4.7 million solutions so is fundamentally unsolvable")]
    public void SparqlGraphExistsInteractionCore457_2()
    {
        RunCore457("exists.rq", false);
    }

    [Fact]
    public void SparqlGraphExistsInteractionCore457_3()
    {
        RunCore457("exists2.rq", true);
    }

    [Fact]
    public void SparqlGraphExistsInteractionCore457_4()
    {
        RunCore457("exists3.rq", true);
    }

    [Fact(Skip = "the query requires generating ~4.7 million solutions so is fundamentally unsolvable")]
    public void SparqlGraphExistsInteractionCore457_5()
    {
        RunCore457("exists-limit.rq", true);
    }
}