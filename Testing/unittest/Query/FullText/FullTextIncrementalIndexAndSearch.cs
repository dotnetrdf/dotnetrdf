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
