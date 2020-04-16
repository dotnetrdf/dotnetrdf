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
using System.Text;
using Xunit;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using LucUtil = Lucene.Net.Util;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.FullText;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.FullText.Indexing.Lucene;
using VDS.RDF.Query.FullText.Search.Lucene;
using VDS.RDF.Query.FullText.Schema;
using VDS.RDF.Query.Optimisation;

namespace VDS.RDF.Query.FullText
{
    [Trait("category", "explicit")]
    [Trait("category", "fulltext")]
    public class FullTextGraphScopingTests
    {
        private const String FullTextPrefix = "PREFIX pf: <" + FullTextHelper.FullTextMatchNamespace + ">";

        private SparqlQueryParser _parser = new SparqlQueryParser();
        private TripleStore _store;
        private Directory _index;

        public FullTextGraphScopingTests()
        {
            String data = @"<http://x> <http://p> ""This is sample text"" <http://g1> .
<http://y> <http://p> ""This is sample text"" <http://g2> .
<http://y> <http://p> ""Additional sample"" <http://g2> .";

            this._store = new TripleStore();
            StringParser.ParseDataset(this._store, data, new NQuadsParser());

            this._index = new RAMDirectory();
            using (LuceneSubjectsIndexer indexer = new LuceneSubjectsIndexer(this._index, new StandardAnalyzer(LucUtil.Version.LUCENE_30), new DefaultIndexSchema()))
            {
                foreach (IGraph g in this._store.Graphs)
                {
                    indexer.Index(g);
                }
            }
        }

        [Fact]
        public void FullTextGraphScoping1()
        {
            //With no Graph scope all results should be returned
            using (LuceneSearchProvider searcher = new LuceneSearchProvider(LucUtil.Version.LUCENE_30, this._index, new StandardAnalyzer(LucUtil.Version.LUCENE_30)))
            {
                IEnumerable<IFullTextSearchResult> results = searcher.Match("sample");
                Assert.Equal(3, results.Count());
            }
        }

        [Fact]
        public void FullTextGraphScoping2()
        {
            //With Graph scope to g1 only one result should be returned
            using (LuceneSearchProvider searcher = new LuceneSearchProvider(LucUtil.Version.LUCENE_30, this._index, new StandardAnalyzer(LucUtil.Version.LUCENE_30)))
            {
                IEnumerable<IFullTextSearchResult> results = searcher.Match(new Uri[] { new Uri("http://g1") }, "sample");
                Assert.Single(results);
                Assert.Equal(new Uri("http://x"), ((IUriNode)results.First().Node).Uri);
            }
        }


        [Fact]
        public void FullTextGraphScoping3()
        {
            //With Graph scope to g2 only two results should be returned
            using (LuceneSearchProvider searcher = new LuceneSearchProvider(LucUtil.Version.LUCENE_30, this._index, new StandardAnalyzer(LucUtil.Version.LUCENE_30)))
            {
                IEnumerable<IFullTextSearchResult> results = searcher.Match(new Uri[] { new Uri("http://g2") }, "sample");
                Assert.Equal(2, results.Count());
                Assert.True(results.All(r => EqualityHelper.AreUrisEqual(new Uri("http://y"), ((IUriNode)r.Node).Uri)));
            }
        }

        [Fact]
        public void FullTextGraphSparqlScoping1()
        {
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(this._store);

            //No results should be returned as no results in default graph
            using (LuceneSearchProvider searcher = new LuceneSearchProvider(LucUtil.Version.LUCENE_30, this._index, new StandardAnalyzer(LucUtil.Version.LUCENE_30)))
            {
                SparqlQuery q = this._parser.ParseFromString(FullTextPrefix + "SELECT * WHERE { ?s pf:textMatch 'sample' }");
                q.AlgebraOptimisers = new IAlgebraOptimiser[] { new FullTextOptimiser(searcher) };

                SparqlResultSet results = processor.ProcessQuery(q) as SparqlResultSet;
                Assert.NotNull(results);
                Assert.Equal(0, results.Count);
            }
        }

        [Fact]
        public void FullTextGraphSparqlScoping2()
        {
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(this._store);

            //1 result should be returned as only one result in the given graph g1
            using (LuceneSearchProvider searcher = new LuceneSearchProvider(LucUtil.Version.LUCENE_30, this._index, new StandardAnalyzer(LucUtil.Version.LUCENE_30)))
            {
                SparqlQuery q = this._parser.ParseFromString(FullTextPrefix + " SELECT * WHERE { GRAPH <http://g1> { ?s pf:textMatch 'sample' } }");
                q.AlgebraOptimisers = new IAlgebraOptimiser[] { new FullTextOptimiser(searcher) };

                SparqlResultSet results = processor.ProcessQuery(q) as SparqlResultSet;
                Assert.NotNull(results);
                Assert.Equal(1, results.Count);
            }
        }

        [Fact]
        public void FullTextGraphSparqlScoping3()
        {
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(this._store);

            //2 results should be returned as two results in the given graph g2
            using (LuceneSearchProvider searcher = new LuceneSearchProvider(LucUtil.Version.LUCENE_30, this._index, new StandardAnalyzer(LucUtil.Version.LUCENE_30)))
            {
                SparqlQuery q = this._parser.ParseFromString(FullTextPrefix + " SELECT * WHERE { GRAPH <http://g2> { ?s pf:textMatch 'sample' } }");
                q.AlgebraOptimisers = new IAlgebraOptimiser[] { new FullTextOptimiser(searcher) };

                SparqlResultSet results = processor.ProcessQuery(q) as SparqlResultSet;
                Assert.NotNull(results);
                Assert.Equal(2, results.Count);
            }
        }

        [Fact]
        public void FullTextGraphSparqlScoping4()
        {
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(this._store);

            //All results should be returned because all graphs are considered
            using (LuceneSearchProvider searcher = new LuceneSearchProvider(LucUtil.Version.LUCENE_30, this._index, new StandardAnalyzer(LucUtil.Version.LUCENE_30)))
            {
                SparqlQuery q = this._parser.ParseFromString(FullTextPrefix + " SELECT * WHERE { GRAPH ?g { ?s pf:textMatch 'sample' } }");
                q.AlgebraOptimisers = new IAlgebraOptimiser[] { new FullTextOptimiser(searcher) };

                SparqlResultSet results = processor.ProcessQuery(q) as SparqlResultSet;
                Assert.NotNull(results);
                Assert.Equal(3, results.Count);
            }
        }

        [Fact]
        public void FullTextGraphSparqlScoping5()
        {
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(this._store);

            //Interaction of graph scope with limit
            using (LuceneSearchProvider searcher = new LuceneSearchProvider(LucUtil.Version.LUCENE_30, this._index, new StandardAnalyzer(LucUtil.Version.LUCENE_30)))
            {
                SparqlQuery q = this._parser.ParseFromString(FullTextPrefix + " SELECT * WHERE { GRAPH <http://g2> { ?s pf:textMatch ( 'sample' 1 ) } }");
                q.AlgebraOptimisers = new IAlgebraOptimiser[] { new FullTextOptimiser(searcher) };

                SparqlResultSet results = processor.ProcessQuery(q) as SparqlResultSet;
                Assert.NotNull(results);
                Assert.Equal(1, results.Count);
                Assert.Equal(new Uri("http://y"), ((IUriNode)results.First()["s"]).Uri);
            }
        }

        [Fact]
        public void FullTextGraphSparqlScoping6()
        {
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(this._store);

            //Interaction of graph scope with limit
            using (LuceneSearchProvider searcher = new LuceneSearchProvider(LucUtil.Version.LUCENE_30, this._index, new StandardAnalyzer(LucUtil.Version.LUCENE_30)))
            {
                SparqlQuery q = this._parser.ParseFromString(FullTextPrefix + " SELECT * WHERE { GRAPH <http://g2> { ?s pf:textMatch ( 'sample' 5 ) } }");
                q.AlgebraOptimisers = new IAlgebraOptimiser[] { new FullTextOptimiser(searcher) };

                SparqlResultSet results = processor.ProcessQuery(q) as SparqlResultSet;
                Assert.NotNull(results);
                Assert.Equal(2, results.Count);
                Assert.Equal(new Uri("http://y"), ((IUriNode)results.First()["s"]).Uri);
            }
        }
    }
}
#endif