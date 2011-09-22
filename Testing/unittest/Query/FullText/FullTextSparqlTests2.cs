using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

namespace VDS.RDF.Test.Query.FullText
{
    [TestClass]
    public class FullTextSparqlTests2
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

        private void RunTest(IFullTextIndexer indexer, String query, IEnumerable<INode> expected)
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

                    foreach (INode n in expected)
                    {
                        Assert.IsTrue(results.Any(r => r.HasValue("match") && r["match"] != null && r["match"].Equals(n)), "Did not get expected ?match => " + formatter.Format(n));
                    }
                    Assert.IsTrue(results.All(r => r.HasValue("match") && r["match"] != null && expected.Contains(r["match"])), "Not all matches were expected");
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

        [TestMethod]
        public void FullTextSparqlComplexLuceneSubjects1()
        {
            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT * WHERE { ?match pf:textMatch 'http' . ?match a <http://example.org/noSuchThing> }", Enumerable.Empty<INode>());
        }

        [TestMethod]
        public void FullTextSparqlComplexLuceneSubjects2()
        {
            this.EnsureTestData();

            List<INode> expected = (from t in this._dataset.Triples
                                    where t.Object.NodeType == NodeType.Literal
                                          && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                                    select t.Subject).ToList();
            NodeFactory factory = new NodeFactory();
            expected.RemoveAll(n => !this._dataset.ContainsTriple(new Triple(Tools.CopyNode(n, factory), factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class")))));

            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT * WHERE { ?match pf:textMatch 'http' . ?match a rdfs:Class }", expected);
        }

        [TestMethod]
        public void FullTextSparqlComplexLuceneSubjects3()
        {
            this.EnsureTestData();

            List<INode> expected = (from t in this._dataset.Triples
                                    where t.Object.NodeType == NodeType.Literal
                                          && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                                    select t.Subject).ToList();
            NodeFactory factory = new NodeFactory();
            expected.RemoveAll(n => !this._dataset.ContainsTriple(new Triple(Tools.CopyNode(n, factory), factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class")))));

            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT * WHERE { ?match a rdfs:Class . { ?match pf:textMatch 'http' } }", expected);
        }

        [TestMethod]
        public void FullTextSparqlComplexLuceneSubjects4()
        {
            this.EnsureTestData();

            List<INode> expected = (from t in this._dataset.Triples
                                    where t.Object.NodeType == NodeType.Literal
                                          && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                                    select t.Subject).ToList();
            NodeFactory factory = new NodeFactory();
            expected.RemoveAll(n => !this._dataset.ContainsTriple(new Triple(Tools.CopyNode(n, factory), factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class")))));
            expected.RemoveAll(n => !this._dataset.GetTriplesWithPredicateObject(factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "domain")), n).Any());

            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT * WHERE { ?match pf:textMatch 'http' . ?match a rdfs:Class . ?property rdfs:domain ?match }", expected);
        }

        [TestMethod]
        public void FullTextSparqlComplexLuceneSubjects5()
        {
            this.EnsureTestData();

            List<INode> expected = (from t in this._dataset.Triples
                                    where t.Object.NodeType == NodeType.Literal
                                          && ((ILiteralNode)t.Object).Value.ToLower().Contains("http")
                                    select t.Subject).ToList();
            NodeFactory factory = new NodeFactory();
            expected.RemoveAll(n => !this._dataset.ContainsTriple(new Triple(Tools.CopyNode(n, factory), factory.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "Class")))));
            expected.RemoveAll(n => !this._dataset.GetTriplesWithPredicateObject(factory.CreateUriNode(new Uri(NamespaceMapper.RDFS + "domain")), n).Any());

            this.RunTest(new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema), "SELECT * WHERE { ?match pf:textMatch 'http' . ?match a rdfs:Class . ?property rdfs:domain ?match . OPTIONAL { ?property rdfs:label ?label } }", expected);
        }

        //[TestMethod]
        //public void GenerateQuarters()
        //{
        //    foreach (String quarter in GetQuarters(3, 2011, 10))
        //    {
        //        Console.WriteLine(quarter);
        //    }
        //}


        //public IEnumerable<String> GetQuarters(int current, int year, int count)
        //{
        //    return (from q in Enumerable.Range(0, count)
        //            select String.Format("Q{0}-{1}", (current - q) + (((q + 1) / 4) * 4) , year - ((q + 1) / 4)));
        //}

    }
}
