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
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using LucUtil = Lucene.Net.Util;
using VDS.RDF.Parsing;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.FullText.Indexing.Lucene;
using VDS.RDF.Query.FullText.Search.Lucene;
using VDS.RDF.Query.FullText.Schema;
using VDS.RDF.Query.Optimisation;
using Lucene.Net.Util;

namespace VDS.RDF.Query.FullText;

[Trait("category", "explicit")]
[Trait("category", "fulltext")]
[Collection("FullText")]
public class FullTextGraphScopingTests
{
    private const String FullTextPrefix = "PREFIX pf: <" + FullTextHelper.FullTextMatchNamespace + ">";

    private readonly SparqlQueryParser _parser = new SparqlQueryParser();
    private readonly TripleStore _store;
    private readonly Directory _index;

    public FullTextGraphScopingTests()
    {
        var data = @"<http://x> <http://p> ""This is sample text"" <http://g1> .
<http://y> <http://p> ""This is sample text"" <http://g2> .
<http://y> <http://p> ""Additional sample"" <http://g2> .";

        _store = new TripleStore();
        StringParser.ParseDataset(_store, data, new NQuadsParser());

        _index = new RAMDirectory();
        using (var indexer = new LuceneSubjectsIndexer(_index, new StandardAnalyzer(LuceneVersion.LUCENE_48), new DefaultIndexSchema()))
        {
            foreach (IGraph g in _store.Graphs)
            {
                indexer.Index(g);
            }
        }
    }

    [Fact]
    public void FullTextGraphScoping1()
    {
        //With no Graph scope all results should be returned
        using (var searcher = new LuceneSearchProvider(LuceneVersion.LUCENE_48, _index, new StandardAnalyzer(LuceneVersion.LUCENE_48)))
        {
            IEnumerable<IFullTextSearchResult> results = searcher.Match("sample");
            Assert.Equal(3, results.Count());
        }
    }

    [Fact]
    public void FullTextGraphScoping2()
    {
        //With Graph scope to g1 only one result should be returned
        using (var searcher = new LuceneSearchProvider(LuceneVersion.LUCENE_48, _index, new StandardAnalyzer(LuceneVersion.LUCENE_48)))
        {
            IEnumerable<IFullTextSearchResult> results = searcher.Match(new [] { new UriNode(new  Uri("http://g1")) }, "sample");
            Assert.Single(results);
            Assert.Equal(new Uri("http://x"), ((IUriNode)results.First().Node).Uri);
        }
    }


    [Fact]
    public void FullTextGraphScoping3()
    {
        //With Graph scope to g2 only two results should be returned
        using (var searcher = new LuceneSearchProvider(LuceneVersion.LUCENE_48, _index, new StandardAnalyzer(LuceneVersion.LUCENE_48)))
        {
            IEnumerable<IFullTextSearchResult> results = searcher.Match(new [] { new UriNode(new Uri("http://g2")) }, "sample");
            Assert.Equal(2, results.Count());
            Assert.True(results.All(r => EqualityHelper.AreUrisEqual(new Uri("http://y"), ((IUriNode)r.Node).Uri)));
        }
    }

    [Fact]
    public void FullTextGraphSparqlScoping1()
    {
        var processor = new LeviathanQueryProcessor(_store);

        //No results should be returned as no results in default graph
        using (var searcher = new LuceneSearchProvider(LuceneVersion.LUCENE_48, _index, new StandardAnalyzer(LuceneVersion.LUCENE_48)))
        {
            SparqlQuery q = _parser.ParseFromString(FullTextPrefix + "SELECT * WHERE { ?s pf:textMatch 'sample' }");
            q.AlgebraOptimisers = new IAlgebraOptimiser[] { new FullTextOptimiser(searcher) };

            var results = processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(0, results.Count);
        }
    }

    [Fact]
    public void FullTextGraphSparqlScoping2()
    {
        var processor = new LeviathanQueryProcessor(_store);

        //1 result should be returned as only one result in the given graph g1
        using (var searcher = new LuceneSearchProvider(LuceneVersion.LUCENE_48, _index, new StandardAnalyzer(LuceneVersion.LUCENE_48)))
        {
            SparqlQuery q = _parser.ParseFromString(FullTextPrefix + " SELECT * WHERE { GRAPH <http://g1> { ?s pf:textMatch 'sample' } }");
            q.AlgebraOptimisers = new IAlgebraOptimiser[] { new FullTextOptimiser(searcher) };

            var results = processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(1, results.Count);
        }
    }

    [Fact]
    public void FullTextGraphSparqlScoping3()
    {
        var processor = new LeviathanQueryProcessor(_store);

        //2 results should be returned as two results in the given graph g2
        using (var searcher = new LuceneSearchProvider(LuceneVersion.LUCENE_48, _index, new StandardAnalyzer(LuceneVersion.LUCENE_48)))
        {
            SparqlQuery q = _parser.ParseFromString(FullTextPrefix + " SELECT * WHERE { GRAPH <http://g2> { ?s pf:textMatch 'sample' } }");
            q.AlgebraOptimisers = new IAlgebraOptimiser[] { new FullTextOptimiser(searcher) };

            var results = processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(2, results.Count);
        }
    }

    [Fact]
    public void FullTextGraphSparqlScoping4()
    {
        var processor = new LeviathanQueryProcessor(_store);

        //All results should be returned because all graphs are considered
        using (var searcher = new LuceneSearchProvider(LuceneVersion.LUCENE_48, _index, new StandardAnalyzer(LuceneVersion.LUCENE_48)))
        {
            SparqlQuery q = _parser.ParseFromString(FullTextPrefix + " SELECT * WHERE { GRAPH ?g { ?s pf:textMatch 'sample' } }");
            q.AlgebraOptimisers = new IAlgebraOptimiser[] { new FullTextOptimiser(searcher) };

            var results = processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(3, results.Count);
        }
    }

    [Fact]
    public void FullTextGraphSparqlScoping5()
    {
        var processor = new LeviathanQueryProcessor(_store);

        //Interaction of graph scope with limit
        using (var searcher = new LuceneSearchProvider(LuceneVersion.LUCENE_48, _index, new StandardAnalyzer(LuceneVersion.LUCENE_48)))
        {
            SparqlQuery q = _parser.ParseFromString(FullTextPrefix + " SELECT * WHERE { GRAPH <http://g2> { ?s pf:textMatch ( 'sample' 1 ) } }");
            q.AlgebraOptimisers = new IAlgebraOptimiser[] { new FullTextOptimiser(searcher) };

            var results = processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(1, results.Count);
            Assert.Equal(new Uri("http://y"), ((IUriNode)results.First()["s"]).Uri);
        }
    }

    [Fact]
    public void FullTextGraphSparqlScoping6()
    {
        var processor = new LeviathanQueryProcessor(_store);

        //Interaction of graph scope with limit
        using (var searcher = new LuceneSearchProvider(LuceneVersion.LUCENE_48, _index, new StandardAnalyzer(LuceneVersion.LUCENE_48)))
        {
            SparqlQuery q = _parser.ParseFromString(FullTextPrefix + " SELECT * WHERE { GRAPH <http://g2> { ?s pf:textMatch ( 'sample' 5 ) } }");
            q.AlgebraOptimisers = new IAlgebraOptimiser[] { new FullTextOptimiser(searcher) };

            var results = processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            Assert.Equal(2, results.Count);
            Assert.Equal(new Uri("http://y"), ((IUriNode)results.First()["s"]).Uri);
        }
    }
}
#endif