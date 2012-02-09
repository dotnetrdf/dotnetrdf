using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lucene.Net;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using VDS.RDF;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.FullText.Indexing;
using VDS.RDF.Query.FullText.Indexing.Lucene;
using VDS.RDF.Query.FullText.Schema;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.FullText.Search.Lucene;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Query.FullText
{
    [TestClass]
    public class FullTextDatasetTests
    {
        private NTriplesFormatter _formatter = new NTriplesFormatter();

        [TestMethod]
        public void FullTextDatasetLucene1()
        {
            //Lucene Index
            Directory dir = new RAMDirectory();
            LuceneSubjectsIndexer indexer = new LuceneSubjectsIndexer(dir, new StandardAnalyzer(), new DefaultIndexSchema());

            //Test Dataset
            InMemoryDataset memData = new InMemoryDataset();
            FullTextIndexedDataset dataset = new FullTextIndexedDataset(memData, indexer, false);

            //Test Graph
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            dataset.AddGraph(g);
            Assert.IsTrue(dataset.HasGraph(g.BaseUri), "Graph should exist in dataset");

            //Now do a search to check all the triples got indexed
            IEnumerable<Triple> searchTriples = g.Triples.Where(t => t.Object.NodeType == NodeType.Literal && t.Subject.NodeType == NodeType.Uri && t.Object.ToString().Contains(' '));
            Random rnd = new Random();
            LuceneSearchProvider searcher = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, dir);
            foreach (Triple searchTriple in searchTriples)
            {
                INode targetNode = searchTriple.Subject;
                String[] terms = searchTriple.Object.ToString().Split(' ').Where(t => t.Length >= 5 && t.ToCharArray().All(c => Char.IsLetter(c))).ToArray();
                if (terms.Length == 0) continue;
                String searchTerm = terms[rnd.Next(terms.Length - 1)];;

                IEnumerable<IFullTextSearchResult> results = searcher.Match(searchTerm);
                Assert.IsTrue(results.Any(r => r.Node.Equals(targetNode)), "Did not find expected node " + targetNode.ToString(this._formatter) + " in search results using search term '" + searchTerm + "' (found " + results.Count() + " results)");
                Console.WriteLine();
            }

            //Now remove the Graph
            dataset.RemoveGraph(g.BaseUri);

            //Repeat the search to check all the triples got unindexed
            foreach (Triple searchTriple in searchTriples)
            {
                INode targetNode = searchTriple.Subject;
                String[] terms = searchTriple.Object.ToString().Split(' ').Where(t => t.Length >= 5 && t.ToCharArray().All(c => Char.IsLetter(c))).ToArray();
                if (terms.Length == 0) continue;
                String searchTerm = terms[rnd.Next(terms.Length - 1)];

                IEnumerable<IFullTextSearchResult> results = searcher.Match(searchTerm);
                Assert.IsFalse(results.Any(r => r.Node.Equals(targetNode)), "Found unexpected node " + targetNode.ToString(this._formatter) + " in search results using search term '" + searchTerm + "' (found " + results.Count() + " results)");
                Console.WriteLine();
            }

            searcher.Dispose();
            indexer.Dispose();
        }

        [TestMethod]
        public void FullTextDatasetLucene2()
        {
            //Lucene Index
            Directory dir = new RAMDirectory();
            LuceneSubjectsIndexer indexer = new LuceneSubjectsIndexer(dir, new StandardAnalyzer(), new DefaultIndexSchema());
            LuceneSearchProvider searcher = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, dir);

            //Test Dataset
            InMemoryDataset memData = new InMemoryDataset();
            FullTextIndexedDataset dataset = new FullTextIndexedDataset(memData, indexer, false);

            //Test Graph
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            dataset.AddGraph(g);
            Assert.IsTrue(dataset.HasGraph(g.BaseUri), "Graph should exist in dataset");

            //Now do a search to check all the triples got indexed
            IEnumerable<Triple> searchTriples = g.Triples.Where(t => t.Object.NodeType == NodeType.Literal && t.Subject.NodeType == NodeType.Uri && t.Object.ToString().Contains(' '));
            Random rnd = new Random();
            foreach (Triple searchTriple in searchTriples)
            {
                INode targetNode = searchTriple.Subject;
                String[] terms = searchTriple.Object.ToString().Split(' ').Where(t => t.Length >= 5 && t.ToCharArray().All(c => Char.IsLetter(c))).ToArray();
                if (terms.Length == 0) continue;
                String searchTerm = terms[rnd.Next(terms.Length - 1)];

                IEnumerable<IFullTextSearchResult> results = searcher.Match(searchTerm);
                Assert.IsTrue(results.Any(r => r.Node.Equals(targetNode)), "Did not find expected node " + targetNode.ToString(this._formatter) + " in search results using search term '" + searchTerm + "' (found " + results.Count() + " results)");
                Console.WriteLine();
            }

            //Now remove the Graph
            dataset.RemoveGraph(g.BaseUri);

            //Repeat the search to check all the triples got unindexed
            foreach (Triple searchTriple in searchTriples)
            {
                INode targetNode = searchTriple.Subject;
                String[] terms = searchTriple.Object.ToString().Split(' ').Where(t => t.Length >= 5 && t.ToCharArray().All(c => Char.IsLetter(c))).ToArray();
                if (terms.Length == 0) continue;
                String searchTerm = terms[rnd.Next(terms.Length - 1)];

                IEnumerable<IFullTextSearchResult> results = searcher.Match(searchTerm);
                Assert.IsFalse(results.Any(r => r.Node.Equals(targetNode)), "Found unexpected node " + targetNode.ToString(this._formatter) + " in search results using search term '" + searchTerm + "' (found " + results.Count() + " results)");
                Console.WriteLine();
            }

            searcher.Dispose();
            indexer.Dispose();
        }
    }
}
