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
using VDS.RDF.Query.FullText.Indexing;
using VDS.RDF.Query.FullText.Indexing.Lucene;
using VDS.RDF.Query.FullText.Schema;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.FullText.Search.Lucene;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Query.FullText
{
    [TestClass]
    public class FullTextIncrementalIndexAndSearch
    {
        private NTriplesFormatter _formatter = new NTriplesFormatter();

        [TestMethod]
        public void FullTextIncrementalIndexingLucene1()
        {
            //Lucene Index
            Directory dir = new RAMDirectory();
            LuceneSubjectsIndexer indexer = new LuceneSubjectsIndexer(dir, new StandardAnalyzer(), new DefaultIndexSchema());

            //Test Graph
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            
            //Try indexing in 100 Triple chunks
            Random rnd = new Random();
            String searchTerm = "http";
            for (int i = 0; i < g.Triples.Count; i += 100)
            {
                //Index the Triples
                List<Triple> ts = g.Triples.Skip(i).Take(100).ToList();
                foreach (Triple t in ts)
                {
                    indexer.Index(t);
                }
                indexer.Flush();

                //Now do a search to check some of those triples got indexed
                //Pick the first multi-word string literal we can find from the batch and grab one word from it
                INode targetNode = ts.Where(t => t.Object.NodeType == NodeType.Literal && t.Object.ToString().Contains("http")).Select(t => t.Subject).FirstOrDefault();
                if (targetNode == null) continue;

                Console.WriteLine("Picked " + targetNode.ToString(this._formatter) + " as search target with search term '" + searchTerm + "'");

                LuceneSearchProvider searcher = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, dir);
                IEnumerable<IFullTextSearchResult> results = searcher.Match(searchTerm);
                foreach (IFullTextSearchResult r in results)
                {
                    Console.WriteLine("Got result " + r.Node.ToString(this._formatter) + " with score " + r.Score);
                }

                Assert.IsTrue(results.Any(r => r.Node.Equals(targetNode)), "Did not find expected node in search results");
                searcher.Dispose();
                Console.WriteLine();
            }
            indexer.Dispose();
        }

        [TestMethod]
        public void FullTextIncrementalIndexingLucene2()
        {
            //Lucene Index
            Directory dir = new RAMDirectory();
            LuceneSubjectsIndexer indexer = new LuceneSubjectsIndexer(dir, new StandardAnalyzer(), new DefaultIndexSchema());
            LuceneSearchProvider searcher = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, dir);

            //Test Graph
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            //Try indexing in 100 Triple chunks
            Random rnd = new Random();
            String searchTerm = "http";
            for (int i = 0; i < g.Triples.Count; i += 100)
            {
                //Index the Triples
                List<Triple> ts = g.Triples.Skip(i).Take(100).ToList();
                foreach (Triple t in ts)
                {
                    indexer.Index(t);
                }
                indexer.Flush();

                //Now do a search to check some of those triples got indexed
                //Pick the first multi-word string literal we can find from the batch and grab one word from it
                INode targetNode = ts.Where(t => t.Object.NodeType == NodeType.Literal && t.Object.ToString().Contains("http")).Select(t => t.Subject).FirstOrDefault();
                if (targetNode == null) continue;

                Console.WriteLine("Picked " + targetNode.ToString(this._formatter) + " as search target with search term '" + searchTerm + "'");

                IEnumerable<IFullTextSearchResult> results = searcher.Match(searchTerm);
                foreach (IFullTextSearchResult r in results)
                {
                    Console.WriteLine("Got result " + r.Node.ToString(this._formatter) + " with score " + r.Score);
                }

                Assert.IsTrue(results.Any(r => r.Node.Equals(targetNode)), "Did not find expected node in search results");
                Console.WriteLine();
            }
            searcher.Dispose();
            indexer.Dispose();
        }
    }
}
