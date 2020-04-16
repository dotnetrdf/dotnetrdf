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

namespace VDS.RDF.Query.FullText
{
    [Collection("FullText")]
    public class FullTextIncrementalIndexAndSearch
    {
        private NTriplesFormatter _formatter = new NTriplesFormatter();

        [Fact]
        public void FullTextIncrementalIndexingLucene1()
        {
            //Lucene Index
            Directory dir = new RAMDirectory();
            LuceneSubjectsIndexer indexer = new LuceneSubjectsIndexer(dir, new StandardAnalyzer(LuceneTestHarness.LuceneVersion), new DefaultIndexSchema());

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

                Assert.True(results.Any(r => r.Node.Equals(targetNode)), "Did not find expected node in search results");
                searcher.Dispose();
                Console.WriteLine();
            }
            indexer.Dispose();
        }

        [Fact]
        public void FullTextIncrementalIndexingLucene2()
        {
            //Lucene Index
            Directory dir = new RAMDirectory();
            LuceneSubjectsIndexer indexer = new LuceneSubjectsIndexer(dir, new StandardAnalyzer(LuceneTestHarness.LuceneVersion), new DefaultIndexSchema());
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

                Assert.True(results.Any(r => r.Node.Equals(targetNode)), "Did not find expected node in search results");
                Console.WriteLine();
            }
            searcher.Dispose();
            indexer.Dispose();
        }
    }
}
#endif