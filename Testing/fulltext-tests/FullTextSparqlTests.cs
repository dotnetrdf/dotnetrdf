using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Query.FullText
{
    [TestClass]
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

                this._dataset = new InMemoryDataset(store);
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
            try
            {
                q.AlgebraOptimisers = new IAlgebraOptimiser[] { new FullTextOptimiser(provider) };

                LeviathanQueryProcessor processor = new LeviathanQueryProcessor(this._dataset);
                SparqlResultSet results = processor.ProcessQuery(q) as SparqlResultSet;
                if (results != null)
                {
                    TestTools.ShowResults(results);

                    if (exact)
                    {
                        Assert.AreEqual(expectedResults, results.Count, "Did not get expected number of results");
                    }
                    else
                    {
                        Assert.IsTrue(expectedResults >= results.Count, "Got more results that the expected maximum");
                    }
                }
                else
                {
                    Assert.Fail("Did not get a SPARQL Result Set as expected");
                }
            }
            finally
            {
                provider.Dispose();
                LuceneTestHarness.Index.Close();
            }
        }

        private void RunTest(IFullTextIndexer indexer, String query, int expectedResults)
        {
            this.RunTest(indexer, query, expectedResults, true);
        }

        [TestMethod]
        public void FullTextSparqlSearchLuceneObjects1()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LuceneObjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT * WHERE { ?s pf:textMatch 'http' }", expected);
        }

        [TestMethod]
        public void FullTextSparqlSearchLuceneObjects2()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LuceneObjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch 'http' } ORDER BY DESC(?score)", expected);
        }

        [TestMethod]
        public void FullTextSparqlSearchLuceneObjects3()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LuceneObjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 1.5) } ORDER BY DESC(?score)", expected, false);
        }

        [TestMethod]
        public void FullTextSparqlSearchLuceneObjects4()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LuceneObjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 1.0 3) } ORDER BY DESC(?score)", Math.Min(expected, 3), false);
        }

        [TestMethod]
        public void FullTextSparqlSearchLuceneSubjects1()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT * WHERE { ?s pf:textMatch 'http' }", expected);
        }

        [TestMethod]
        public void FullTextSparqlSearchLuceneSubjects2()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch 'http' } ORDER BY DESC(?score)", expected);
        }

        [TestMethod]
        public void FullTextSparqlSearchLuceneSubjects3()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 1.5) } ORDER BY DESC(?score)", expected, false);
        }

        [TestMethod]
        public void FullTextSparqlSearchLuceneSubjects4()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 1.0 3) } ORDER BY DESC(?score)", Math.Min(expected, 3), false);
        }

        [TestMethod]
        public void FullTextSparqlSearchLucenePredicates1()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LucenePredicatesIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT * WHERE { ?s pf:textMatch 'http' }", expected);
        }

        [TestMethod]
        public void FullTextSparqlSearchLucenePredicates2()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LucenePredicatesIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch 'http' } ORDER BY DESC(?score)", expected);
        }

        [TestMethod]
        public void FullTextSparqlSearchLucenePredicates3()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LucenePredicatesIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 1.5) } ORDER BY DESC(?score)", expected, false);
        }

        [TestMethod]
        public void FullTextSparqlSearchLucenePredicates4()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LucenePredicatesIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 1.0 3) } ORDER BY DESC(?score)", Math.Min(expected, 3), false);
        }
    }
}
