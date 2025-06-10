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
using Xunit;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.FullText.Indexing.Lucene;
using VDS.RDF.Query.FullText.Schema;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.FullText.Search.Lucene;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.FullText;

[Collection("FullText")]
public class FullTextDatasetTests
{
    private NTriplesFormatter _formatter = new NTriplesFormatter();

    [Fact]
    public void FullTextDatasetLucene1()
    {
        //Lucene Index
        Directory dir = new RAMDirectory();
        var indexer = new LuceneSubjectsIndexer(dir, new StandardAnalyzer(LuceneTestHarness.LuceneVersion), new DefaultIndexSchema());

        //Test Dataset
        var memData = new InMemoryDataset();
        var dataset = new FullTextIndexedDataset(memData, indexer, false);

        //Test Graph
        var g = new Graph(new UriNode(new Uri("http://example.com/graph1")));
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

        dataset.AddGraph(g);
        Assert.True(dataset.HasGraph(g.Name), "Graph should exist in dataset");

        //Now do a search to check all the triples got indexed
        var searchTerm = "http";
        IEnumerable<Triple> searchTriples = g.Triples.Where(t => t.Object.NodeType == NodeType.Literal && ((ILiteralNode)t.Object).Value.Contains("http"));
        var searcher = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, dir);
        foreach (Triple searchTriple in searchTriples)
        {
            INode targetNode = searchTriple.Subject;
            IEnumerable<IFullTextSearchResult> results = searcher.Match(searchTerm);
            Assert.True(results.Any(r => r.Node.Equals(targetNode)), "Did not find expected node " + targetNode.ToString(_formatter) + " in search results using search term '" + searchTerm + "' (found " + results.Count() + " results)");
            Console.WriteLine();
        }

        //Now remove the Graph
        dataset.RemoveGraph(g.Name);

        //Repeat the search to check all the triples got unindexed
        foreach (Triple searchTriple in searchTriples)
        {
            INode targetNode = searchTriple.Subject;
            IEnumerable<IFullTextSearchResult> results = searcher.Match(searchTerm);
            Assert.False(results.Any(r => r.Node.Equals(targetNode)), "Found unexpected node " + targetNode.ToString(_formatter) + " in search results using search term '" + searchTerm + "' (found " + results.Count() + " results)");
            Console.WriteLine();
        }

        searcher.Dispose();
        indexer.Dispose();
    }

    [Fact]
    public void FullTextDatasetLucene2()
    {
        //Lucene Index
        Directory dir = new RAMDirectory();
        var indexer = new LuceneSubjectsIndexer(dir, new StandardAnalyzer(LuceneTestHarness.LuceneVersion), new DefaultIndexSchema());
        indexer.Flush();
        var searcher = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, dir);

        //Test Dataset
        var memData = new InMemoryDataset();
        var dataset = new FullTextIndexedDataset(memData, indexer, false);

        //Test Graph
        var g = new Graph(new UriNode(new Uri("http://example.com/graph2")));
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

        dataset.AddGraph(g);
        Assert.True(dataset.HasGraph(g.Name), "Graph should exist in dataset");

        //Now do a search to check all the triples got indexed
        var searchTerm = "http";
        IEnumerable<Triple> searchTriples = g.Triples.Where(t => t.Object.NodeType == NodeType.Literal && ((ILiteralNode)t.Object).Value.Contains("http"));
        foreach (Triple searchTriple in searchTriples)
        {
            INode targetNode = searchTriple.Subject;
            IEnumerable<IFullTextSearchResult> results = searcher.Match(searchTerm);
            Assert.True(results.Any(r => r.Node.Equals(targetNode)), "Did not find expected node " + targetNode.ToString(_formatter) + " in search results using search term '" + searchTerm + "' (found " + results.Count() + " results)");
            Console.WriteLine();
        }

        //Now remove the Graph
        dataset.RemoveGraph(g.Name);

        //Repeat the search to check all the triples got unindexed
        foreach (Triple searchTriple in searchTriples)
        {
            INode targetNode = searchTriple.Subject;
            IEnumerable<IFullTextSearchResult> results = searcher.Match(searchTerm);
            Assert.False(results.Any(r => r.Node.Equals(targetNode)), "Found unexpected node " + targetNode.ToString(_formatter) + " in search results using search term '" + searchTerm + "' (found " + results.Count() + " results)");
            Console.WriteLine();
        }

        searcher.Dispose();
        indexer.Dispose();
    }
}
#endif