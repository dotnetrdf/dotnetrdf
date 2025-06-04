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

using System.Linq;
using Xunit;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using VDS.RDF.Query.FullText.Indexing.Lucene;
using VDS.RDF.Query.FullText.Schema;
using VDS.RDF.Query.FullText.Search.Lucene;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.FullText;

[Collection("FullText")]
public class FullTextIncrementalIndexAndSearch
{
    private readonly NTriplesFormatter _formatter = new NTriplesFormatter();
    private readonly ITestOutputHelper _output;

    public FullTextIncrementalIndexAndSearch(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void FullTextIncrementalIndexingLucene1()
    {
        //Lucene Index
        Directory dir = new RAMDirectory();
        var indexer = new LuceneSubjectsIndexer(dir, new StandardAnalyzer(LuceneTestHarness.LuceneVersion), new DefaultIndexSchema());

        //Test Graph
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        
        //Try indexing in 100 Triple chunks
        var searchTerm = "http";
        for (var i = 0; i < g.Triples.Count; i += 100)
        {
            //Index the Triples
            var ts = g.Triples.Skip(i).Take(100).ToList();
            foreach (var t in ts)
            {
                indexer.Index(g, t);
            }
            indexer.Flush();

            //Now do a search to check some of those triples got indexed
            //Pick the first multi-word string literal we can find from the batch and grab one word from it
            var targetNode = ts.Where(t => t.Object.NodeType == NodeType.Literal && ((ILiteralNode)t.Object).Value.Contains("http")).Select(t => t.Subject).FirstOrDefault();
            if (targetNode == null) continue;

            _output.WriteLine("Picked " + targetNode.ToString(_formatter) + " as search target with search term '" + searchTerm + "'");

            var searcher = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, dir);
            var results = searcher.Match(searchTerm).ToList();
            foreach (var r in results)
            {
                _output.WriteLine("Got result " + r.Node.ToString(_formatter) + " with score " + r.Score);
            }

            Assert.True(results.Any(r => r.Node.Equals(targetNode)), "Did not find expected node in search results");
            searcher.Dispose();
            _output.WriteLine(string.Empty);
        }
        indexer.Dispose();
    }

    [Fact]
    public void FullTextIncrementalIndexingLucene2()
    {
        //Lucene Index
        Directory dir = new RAMDirectory();
        var indexer = new LuceneSubjectsIndexer(dir, new StandardAnalyzer(LuceneTestHarness.LuceneVersion), new DefaultIndexSchema());
        indexer.Flush();
        var searcher = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, dir);

        //Test Graph
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

        //Try indexing in 100 Triple chunks
        var searchTerm = "http";
        for (var i = 0; i < g.Triples.Count; i += 100)
        {
            //Index the Triples
            var ts = g.Triples.Skip(i).Take(100).ToList();
            foreach (var t in ts)
            {
                indexer.Index(g, t);
            }
            indexer.Flush();

            //Now do a search to check some of those triples got indexed
            //Pick the first multi-word string literal we can find from the batch and grab one word from it
            var targetNode = ts.Where(t => t.Object.NodeType == NodeType.Literal && ((ILiteralNode)t.Object).Value.Contains("http")).Select(t => t.Subject).FirstOrDefault();
            if (targetNode == null) continue;

            _output.WriteLine("Picked " + targetNode.ToString(_formatter) + " as search target with search term '" + searchTerm + "'");

            var results = searcher.Match(searchTerm).ToList();
            foreach (var r in results)
            {
                _output.WriteLine("Got result " + r.Node.ToString(_formatter) + " with score " + r.Score);
            }

            Assert.True(results.Any(r => r.Node.Equals(targetNode)), "Did not find expected node in search results");
            _output.WriteLine(string.Empty);
        }
        searcher.Dispose();
        indexer.Dispose();
    }
}
#endif