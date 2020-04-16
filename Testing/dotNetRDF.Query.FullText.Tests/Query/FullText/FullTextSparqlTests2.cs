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
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.FullText;
using VDS.RDF.Query.FullText.Indexing;
using VDS.RDF.Query.FullText.Indexing.Lucene;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.FullText.Search.Lucene;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.FullText
{
    [Trait("category", "explicit")]
    [Trait("category", "fulltext")]
    [Collection("FullText")]
    public class FullTextSparqlTests2
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private INamespaceMapper _nsmap;
        private ISparqlDataset _dataset;

        public FullTextSparqlTests2()
        {
            this._nsmap = new NamespaceMapper();
            this._nsmap.AddNamespace("pf", new Uri(FullTextHelper.FullTextMatchNamespace));
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            store.Add(g);
            this._dataset = new InMemoryDataset(store, true);
        }

        private INamespaceMapper GetQueryNamespaces()
        {
            return this._nsmap;
        }

        private void RunTest(IFullTextIndexer indexer, String query, IEnumerable<INode> expected)
        {

            indexer.Index(this._dataset);
            indexer.Dispose();

            //Build the SPARQL Query and parse it
            SparqlParameterizedString queryString = new SparqlParameterizedString(query);
            queryString.Namespaces = this.GetQueryNamespaces();
            SparqlQuery q = this._parser.ParseFromString(queryString);

            SparqlFormatter formatter = new SparqlFormatter(q.NamespaceMap);
            Console.WriteLine("Parsed Query:");
            Console.WriteLine(formatter.Format(q));

            Console.WriteLine("Expected Results:");
            foreach (INode n in expected)
            {
                Console.WriteLine(n.ToString(formatter));
            }
            Console.WriteLine();

            LuceneSearchProvider provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
            try
            {
                q.AlgebraOptimisers = new IAlgebraOptimiser[] { new FullTextOptimiser(provider) };
                Options.AlgebraOptimisation = true;

                LeviathanQueryProcessor processor = new LeviathanQueryProcessor(this._dataset);
                SparqlResultSet results = processor.ProcessQuery(q) as SparqlResultSet;
                if (results != null)
                {
                    TestTools.ShowResults(results);

                    foreach (INode n in expected)
                    {
                        Assert.True(results.Any(r => r.HasValue("match") && r["match"] != null && r["match"].Equals(n)), "Did not get expected ?match => " + formatter.Format(n));
                    }
                    foreach (SparqlResult r in results)
                    {
                        Assert.True(r.HasValue("match") && r["match"] != null && expected.Contains(r["match"]), "Unexpected Match " + formatter.Format(r["match"]));
                    }
                }
                else
                {
                    Assert.True(false, "Did not get a SPARQL Result Set as expected");
                }
            }
            finally
            {
                provider.Dispose();
                LuceneTestHarness.Index.Dispose();
            }
        }

        [Fact]
        public void FullTextSparqlComplexLuceneSubjects1()
        {
            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT * WHERE { ?match pf:textMatch 'http' . ?match a <http://example.org/noSuchThing> }", Enumerable.Empty<INode>());
        }

        [Fact]
        public void FullTextSparqlComplexLuceneSubjects2()
        {
            List<INode> expected = (from t in this._dataset.Triples
                                    where t.Object.NodeType == NodeType.Literal
                                          && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                                    select t.Subject).ToList();
            NodeFactory factory = new NodeFactory();
            expected.RemoveAll(n => !this._dataset.ContainsTriple(new Triple(Tools.CopyNode(n, factory), factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class")))));
            Assert.True(expected.Any());

            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT * WHERE { ?match pf:textMatch 'http' . ?match a rdfs:Class }", expected);
        }

        [Fact]
        public void FullTextSparqlComplexLuceneSubjects3()
        {
            List<INode> expected = (from t in this._dataset.Triples
                                    where t.Object.NodeType == NodeType.Literal
                                          && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                                    select t.Subject).ToList();
            NodeFactory factory = new NodeFactory();
            expected.RemoveAll(n => !this._dataset.ContainsTriple(new Triple(Tools.CopyNode(n, factory), factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class")))));
            Assert.True(expected.Any());

            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT * WHERE { ?match a rdfs:Class . { ?match pf:textMatch 'http' } }", expected);
        }

        [Fact]
        public void FullTextSparqlComplexLuceneSubjects4()
        {
            List<INode> expected = (from t in this._dataset.Triples
                                    where t.Object.NodeType == NodeType.Literal
                                          && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                                    select t.Subject).ToList();
            NodeFactory factory = new NodeFactory();
            expected.RemoveAll(n => !this._dataset.ContainsTriple(new Triple(Tools.CopyNode(n, factory), factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class")))));
            expected.RemoveAll(n => !this._dataset.GetTriplesWithPredicateObject(factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "domain")), n).Any());
            Assert.True(expected.Any());

            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT * WHERE { ?match pf:textMatch 'http' . ?match a rdfs:Class . ?property rdfs:domain ?match }", expected);
        }

        [Fact]
        public void FullTextSparqlComplexLuceneSubjects5()
        {
            List<INode> expected = (from t in this._dataset.Triples
                                    where t.Object.NodeType == NodeType.Literal
                                          && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                                    select t.Subject).ToList();
            NodeFactory factory = new NodeFactory();
            expected.RemoveAll(n => !this._dataset.ContainsTriple(new Triple(Tools.CopyNode(n, factory), factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class")))));
            expected.RemoveAll(n => !this._dataset.GetTriplesWithPredicateObject(factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "domain")), n).Any());


            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT * WHERE { ?match pf:textMatch 'http' . ?match a rdfs:Class . ?property rdfs:domain ?match . OPTIONAL { ?property rdfs:label ?label } }", expected);
        }

        [Fact]
        public void FullTextSparqlComplexLuceneSubjects6()
        {
            List<INode> expected = (from t in this._dataset.Triples
                                    where t.Object.NodeType == NodeType.Literal
                                          && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                                    select t.Subject).ToList();
            NodeFactory factory = new NodeFactory();

            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT * WHERE { ?match pf:textMatch 'http' . OPTIONAL { ?match ?p ?o } }", expected);
        }
    }
}
#endif