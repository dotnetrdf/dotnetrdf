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
            LuceneSubjectsIndexer indexer = new LuceneSubjectsIndexer(dir, new StandardAnalyzer(LuceneTestHarness.LuceneVersion), new DefaultIndexSchema());

            //Test Dataset
            InMemoryDataset memData = new InMemoryDataset();
            FullTextIndexedDataset dataset = new FullTextIndexedDataset(memData, indexer, false);

            //Test Graph
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            dataset.AddGraph(g);
            Assert.IsTrue(dataset.HasGraph(g.BaseUri), "Graph should exist in dataset");

            //Now do a search to check all the triples got indexed
            String searchTerm = "http";
            IEnumerable<Triple> searchTriples = g.Triples.Where(t => t.Object.NodeType == NodeType.Literal && t.Object.ToString().Contains("http"));
            LuceneSearchProvider searcher = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, dir);
            foreach (Triple searchTriple in searchTriples)
            {
                INode targetNode = searchTriple.Subject;
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
            LuceneSubjectsIndexer indexer = new LuceneSubjectsIndexer(dir, new StandardAnalyzer(LuceneTestHarness.LuceneVersion), new DefaultIndexSchema());
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
            String searchTerm = "http";
            IEnumerable<Triple> searchTriples = g.Triples.Where(t => t.Object.NodeType == NodeType.Literal && t.Object.ToString().Contains("http"));
            foreach (Triple searchTriple in searchTriples)
            {
                INode targetNode = searchTriple.Subject;
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
                IEnumerable<IFullTextSearchResult> results = searcher.Match(searchTerm);
                Assert.IsFalse(results.Any(r => r.Node.Equals(targetNode)), "Found unexpected node " + targetNode.ToString(this._formatter) + " in search results using search term '" + searchTerm + "' (found " + results.Count() + " results)");
                Console.WriteLine();
            }

            searcher.Dispose();
            indexer.Dispose();
        }
    }
}
