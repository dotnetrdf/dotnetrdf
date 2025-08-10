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

#if !NO_FULLTEXT

using System;
using System.Linq;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.FullText.Indexing;
using VDS.RDF.Query.FullText.Indexing.Lucene;
using VDS.RDF.Query.FullText.Search.Lucene;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.PropertyFunctions;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.FullText;

[Trait("category", "explicit")]
[Trait("category", "fulltext")]
[Collection("FullText")]
public class FullTextSparqlTests
{
    private SparqlQueryParser _parser = new SparqlQueryParser();
    private INamespaceMapper _nsmap;
    private ISparqlDataset _dataset;
    private LuceneTestHarness _testHarness = new LuceneTestHarness();

    private INamespaceMapper GetQueryNamespaces()
    {
        if (_nsmap == null)
        {
            _nsmap = new NamespaceMapper();
            _nsmap.AddNamespace("pf", new Uri(FullTextHelper.FullTextMatchNamespace));
        }
        return _nsmap;
    }

    private void EnsureTestData()
    {
        if (_dataset == null)
        {
            var store = new TripleStore();
            var g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            store.Add(g);

            _dataset = new InMemoryDataset(store, g.Name);
        }
    }

    private void RunTest(IFullTextIndexer indexer, String query, int expectedResults, bool exact)
    {
        EnsureTestData();

        indexer.Index(_dataset);
        indexer.Dispose();

        //Build the SPARQL Query and parse it
        var queryString = new SparqlParameterizedString(query)
        {
            Namespaces = GetQueryNamespaces()
        };
        SparqlQuery q = _parser.ParseFromString(queryString);

        var formatter = new SparqlFormatter(q.NamespaceMap);
        Console.WriteLine("Parsed Query:");
        Console.WriteLine(formatter.Format(q));

        var provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, _testHarness.Index);
        var factory = new FullTextPropertyFunctionFactory();
        try
        {
            PropertyFunctionFactory.AddFactory(factory);
            q.AlgebraOptimisers = new IAlgebraOptimiser[] { new FullTextOptimiser(provider) };

            var processor = new LeviathanQueryProcessor(_dataset);
            var results = processor.ProcessQuery(q) as SparqlResultSet;
            if (results != null)
            {
                TestTools.ShowResults(results);

                if (exact)
                {
                    Assert.Equal(expectedResults, results.Count);
                }
                else
                {
                    Assert.True(expectedResults >= results.Count, "Got more results that the expected maximum");
                }
            }
            else
            {
                Assert.Fail("Did not get a SPARQL Result Set as expected");
            }
        }
        finally
        {
            PropertyFunctionFactory.RemoveFactory(factory);
            provider.Dispose();
            _testHarness.Index.Dispose();
        }
    }

    private void RunTest(IFullTextIndexer indexer, String query, int expectedResults)
    {
        RunTest(indexer, query, expectedResults, true);
    }

    [Fact]
    public void FullTextSparqlSearchLuceneObjects1()
    {
        EnsureTestData();

        var expected = (from t in _dataset.Triples
                        where t.Object.NodeType == NodeType.Literal
                              && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                        select t).Count();

        RunTest(new LuceneObjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT * WHERE { ?s pf:textMatch 'http' }", expected);
    }

    [Fact]
    public void FullTextSparqlSearchLuceneObjects2()
    {
        EnsureTestData();

        var expected = (from t in _dataset.Triples
                        where t.Object.NodeType == NodeType.Literal
                              && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                        select t).Count();

        RunTest(new LuceneObjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch 'http' } ORDER BY DESC(?score)", expected);
    }

    [Fact]
    public void FullTextSparqlSearchLuceneObjects3()
    {
        EnsureTestData();

        var expected = (from t in _dataset.Triples
                        where t.Object.NodeType == NodeType.Literal
                              && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                        select t).Count();

        RunTest(new LuceneObjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 1.5) } ORDER BY DESC(?score)", expected, false);
    }

    [Fact]
    public void FullTextSparqlSearchLuceneObjects4()
    {
        EnsureTestData();

        var expected = (from t in _dataset.Triples
                        where t.Object.NodeType == NodeType.Literal
                              && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                        select t).Count();

        RunTest(new LuceneObjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 1.0 3) } ORDER BY DESC(?score)", Math.Min(expected, 3), false);
    }

    [Fact]
    public void FullTextSparqlSearchLuceneObjects5()
    {
        EnsureTestData();

        var expected = (from t in _dataset.Triples
                        where t.Object.NodeType == NodeType.Literal
                              && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                        select t).Count();

        RunTest(new LuceneObjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 3) } ORDER BY DESC(?score)", Math.Min(expected, 3), false);
    }

    [Fact]
    public void FullTextSparqlSearchLuceneSubjects1()
    {
        EnsureTestData();

        var expected = (from t in _dataset.Triples
                        where t.Object.NodeType == NodeType.Literal
                              && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                        select t).Count();

        RunTest(new LuceneSubjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT * WHERE { ?s pf:textMatch 'http' }", expected);
    }

    [Fact]
    public void FullTextSparqlSearchLuceneSubjects2()
    {
        EnsureTestData();

        var expected = (from t in _dataset.Triples
                        where t.Object.NodeType == NodeType.Literal
                              && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                        select t).Count();

        RunTest(new LuceneSubjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch 'http' } ORDER BY DESC(?score)", expected);
    }

    [Fact]
    public void FullTextSparqlSearchLuceneSubjects3()
    {
        EnsureTestData();

        var expected = (from t in _dataset.Triples
                        where t.Object.NodeType == NodeType.Literal
                              && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                        select t).Count();

        RunTest(new LuceneSubjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 1.5) } ORDER BY DESC(?score)", expected, false);
    }

    [Fact]
    public void FullTextSparqlSearchLuceneSubjects4()
    {
        EnsureTestData();

        var expected = (from t in _dataset.Triples
                        where t.Object.NodeType == NodeType.Literal
                              && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                        select t).Count();

        RunTest(new LuceneSubjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 1.0 3) } ORDER BY DESC(?score)", Math.Min(expected, 3), false);
    }

    [Fact]
    public void FullTextSparqlSearchLuceneSubjects5()
    {
        EnsureTestData();

        var expected = (from t in _dataset.Triples
                        where t.Object.NodeType == NodeType.Literal
                              && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                        select t).Count();

        RunTest(new LuceneSubjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 3) } ORDER BY DESC(?score)", Math.Min(expected, 3), false);
    }

    [Fact]
    public void FullTextSparqlSearchLucenePredicates1()
    {
        EnsureTestData();

        var expected = (from t in _dataset.Triples
                        where t.Object.NodeType == NodeType.Literal
                              && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                        select t).Count();

        RunTest(new LucenePredicatesIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT * WHERE { ?s pf:textMatch 'http' }", expected);
    }

    [Fact]
    public void FullTextSparqlSearchLucenePredicates2()
    {
        EnsureTestData();

        var expected = (from t in _dataset.Triples
                        where t.Object.NodeType == NodeType.Literal
                              && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                        select t).Count();

        RunTest(new LucenePredicatesIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch 'http' } ORDER BY DESC(?score)", expected);
    }

    [Fact]
    public void FullTextSparqlSearchLucenePredicates3()
    {
        EnsureTestData();

        var expected = (from t in _dataset.Triples
                        where t.Object.NodeType == NodeType.Literal
                              && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                        select t).Count();

        RunTest(new LucenePredicatesIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 1.5) } ORDER BY DESC(?score)", expected, false);
    }

    [Fact]
    public void FullTextSparqlSearchLucenePredicates4()
    {
        EnsureTestData();

        var expected = (from t in _dataset.Triples
                        where t.Object.NodeType == NodeType.Literal
                              && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                        select t).Count();

        RunTest(new LucenePredicatesIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 1.0 3) } ORDER BY DESC(?score)", Math.Min(expected, 3), false);
    }

    [Fact]
    public void FullTextSparqlSearchLucenePredicates5()
    {
        EnsureTestData();

        var expected = (from t in _dataset.Triples
                        where t.Object.NodeType == NodeType.Literal
                              && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                        select t).Count();

        RunTest(new LucenePredicatesIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 3) } ORDER BY DESC(?score)", Math.Min(expected, 3), false);
    }
}
#endif