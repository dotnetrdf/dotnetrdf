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
using Lucene.Net;
using Lucene.Net.Search;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.FullText.Indexing;
using VDS.RDF.Query.FullText.Indexing.Lucene;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.FullText.Search.Lucene;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.PropertyFunctions;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.FullText
{
    [Trait("category", "explicit")]
    [Trait("category", "fulltext")]
    [Collection("FullText")]
    public class FullTextSparqlTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private INamespaceMapper _nsmap;
        private ISparqlDataset _dataset;

        private INamespaceMapper GetQueryNamespaces()
        {
            if (this._nsmap == null)
            {
                this._nsmap = new NamespaceMapper();
                this._nsmap.AddNamespace("pf", new Uri(FullTextHelper.FullTextMatchNamespace));
            }
            return this._nsmap;
        }

        private void EnsureTestData()
        {
            if (this._dataset == null)
            {
                TripleStore store = new TripleStore();
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                store.Add(g);

                this._dataset = new InMemoryDataset(store, g.BaseUri);
            }
        }

        private void RunTest(IFullTextIndexer indexer, String query, int expectedResults, bool exact)
        {
            this.EnsureTestData();

            indexer.Index(this._dataset);
            indexer.Dispose();

            //Build the SPARQL Query and parse it
            SparqlParameterizedString queryString = new SparqlParameterizedString(query);
            queryString.Namespaces = this.GetQueryNamespaces();
            SparqlQuery q = this._parser.ParseFromString(queryString);

            SparqlFormatter formatter = new SparqlFormatter(q.NamespaceMap);
            Console.WriteLine("Parsed Query:");
            Console.WriteLine(formatter.Format(q));

            LuceneSearchProvider provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
            FullTextPropertyFunctionFactory factory = new FullTextPropertyFunctionFactory();
            try
            {
                PropertyFunctionFactory.AddFactory(factory);
                q.AlgebraOptimisers = new IAlgebraOptimiser[] { new FullTextOptimiser(provider) };
                Options.AlgebraOptimisation = true;

                LeviathanQueryProcessor processor = new LeviathanQueryProcessor(this._dataset);
                SparqlResultSet results = processor.ProcessQuery(q) as SparqlResultSet;
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
                    Assert.True(false, "Did not get a SPARQL Result Set as expected");
                }
            }
            finally
            {
                PropertyFunctionFactory.RemoveFactory(factory);
                provider.Dispose();
                LuceneTestHarness.Index.Dispose();
            }
        }

        private void RunTest(IFullTextIndexer indexer, String query, int expectedResults)
        {
            this.RunTest(indexer, query, expectedResults, true);
        }

        [Fact]
        public void FullTextSparqlSearchLuceneObjects1()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LuceneObjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT * WHERE { ?s pf:textMatch 'http' }", expected);
        }

        [Fact]
        public void FullTextSparqlSearchLuceneObjects2()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LuceneObjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch 'http' } ORDER BY DESC(?score)", expected);
        }

        [Fact]
        public void FullTextSparqlSearchLuceneObjects3()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LuceneObjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 1.5) } ORDER BY DESC(?score)", expected, false);
        }

        [Fact]
        public void FullTextSparqlSearchLuceneObjects4()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LuceneObjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 1.0 3) } ORDER BY DESC(?score)", Math.Min(expected, 3), false);
        }

        [Fact]
        public void FullTextSparqlSearchLuceneObjects5()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LuceneObjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 3) } ORDER BY DESC(?score)", Math.Min(expected, 3), false);
        }

        [Fact]
        public void FullTextSparqlSearchLuceneSubjects1()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT * WHERE { ?s pf:textMatch 'http' }", expected);
        }

        [Fact]
        public void FullTextSparqlSearchLuceneSubjects2()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch 'http' } ORDER BY DESC(?score)", expected);
        }

        [Fact]
        public void FullTextSparqlSearchLuceneSubjects3()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 1.5) } ORDER BY DESC(?score)", expected, false);
        }

        [Fact]
        public void FullTextSparqlSearchLuceneSubjects4()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 1.0 3) } ORDER BY DESC(?score)", Math.Min(expected, 3), false);
        }

        [Fact]
        public void FullTextSparqlSearchLuceneSubjects5()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 3) } ORDER BY DESC(?score)", Math.Min(expected, 3), false);
        }

        [Fact]
        public void FullTextSparqlSearchLucenePredicates1()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LucenePredicatesIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT * WHERE { ?s pf:textMatch 'http' }", expected);
        }

        [Fact]
        public void FullTextSparqlSearchLucenePredicates2()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LucenePredicatesIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch 'http' } ORDER BY DESC(?score)", expected);
        }

        [Fact]
        public void FullTextSparqlSearchLucenePredicates3()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LucenePredicatesIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 1.5) } ORDER BY DESC(?score)", expected, false);
        }

        [Fact]
        public void FullTextSparqlSearchLucenePredicates4()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LucenePredicatesIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 1.0 3) } ORDER BY DESC(?score)", Math.Min(expected, 3), false);
        }

        [Fact]
        public void FullTextSparqlSearchLucenePredicates5()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LucenePredicatesIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 3) } ORDER BY DESC(?score)", Math.Min(expected, 3), false);
        }
    }
}
#endif