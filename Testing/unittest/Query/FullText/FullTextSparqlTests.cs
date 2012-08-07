/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

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
using VDS.RDF.Query.PropertyFunctions;
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
                PropertyFunctionFactory.RemoveFactory(factory);
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
        public void FullTextSparqlSearchLuceneObjects5()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LuceneObjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 3) } ORDER BY DESC(?score)", Math.Min(expected, 3), false);
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
        public void FullTextSparqlSearchLuceneSubjects5()
        {
            this.EnsureTestData();

            int expected = (from t in this._dataset.Triples
                            where t.Object.NodeType == NodeType.Literal
                                  && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                            select t).Count();

            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT ?score ?match WHERE { (?match ?score) pf:textMatch ('http' 3) } ORDER BY DESC(?score)", Math.Min(expected, 3), false);
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

        [TestMethod]
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
