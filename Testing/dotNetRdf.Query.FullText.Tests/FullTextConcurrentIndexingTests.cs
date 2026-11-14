/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

#if !NO_FULLTEXT
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Xunit;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using VDS.RDF.Query.FullText.Indexing.Lucene;
using VDS.RDF.Query.FullText.Schema;

namespace VDS.RDF.Query.FullText;

/// <summary>
/// Independent indexers (own directory, analyzer, schema and writer) must be usable from
/// different threads at the same time. See https://github.com/dotnetrdf/dotnetrdf/issues/913.
/// </summary>
[Trait("category", "fulltext")]
[Collection("FullText")]
public class FullTextConcurrentIndexingTests
{
    private const int IndexerCount = 16;
    private const int TriplesPerIndexer = 2000;

    [Fact]
    public void IndependentIndexersCanIndexAndUnindexConcurrently()
    {
        Directory[] directories = Enumerable.Range(0, IndexerCount).Select(_ => (Directory)new RAMDirectory()).ToArray();
        IGraph[] graphs = Enumerable.Range(0, IndexerCount).Select(CreateGraph).ToArray();

        RunConcurrently(build => Index(directories[build], graphs[build]));
        Assert.All(directories, directory => Assert.Equal(TriplesPerIndexer, CountDocuments(directory)));

        RunConcurrently(build => Unindex(directories[build], graphs[build]));
        Assert.All(directories, directory => Assert.Equal(0, CountDocuments(directory)));
    }

    [Fact]
    public void Sha256HashIsStable()
    {
        // The hash field is how existing index documents are found for unindexing,
        // so the digest and its lower-case hex encoding must not change between versions.
        const string expected = "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad";

        Assert.Equal(expected, "abc".GetSha256Hash());
        Assert.Equal(expected, "abc".GetSha256Hash());
    }

    private static void RunConcurrently(Action<int> work)
    {
        var failures = new ConcurrentQueue<Exception>();
        using var start = new ManualResetEventSlim();
        Thread[] threads = Enumerable.Range(0, IndexerCount).Select(build => new Thread(() =>
        {
            start.Wait();
            try
            {
                work(build);
            }
            catch (Exception ex)
            {
                failures.Enqueue(ex);
            }
        })).ToArray();

        foreach (Thread thread in threads)
        {
            thread.Start();
        }
        start.Set();
        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        Assert.True(failures.IsEmpty,
            $"{failures.Count} of {IndexerCount} indexers failed:{Environment.NewLine}" +
            string.Join(Environment.NewLine, failures.Select(f => f.GetType().FullName + ": " + f.Message)));
    }

    private static void Index(Directory directory, IGraph graph)
    {
        using var analyzer = new StandardAnalyzer(LuceneTestHarness.LuceneVersion);
        using var indexer = new LuceneSubjectsIndexer(directory, analyzer, new DefaultIndexSchema());
        indexer.Index(graph);
    }

    private static void Unindex(Directory directory, IGraph graph)
    {
        using var analyzer = new StandardAnalyzer(LuceneTestHarness.LuceneVersion);
        using var indexer = new LuceneSubjectsIndexer(directory, analyzer, new DefaultIndexSchema());
        indexer.Unindex(graph);
    }

    private static int CountDocuments(Directory directory)
    {
        using DirectoryReader reader = DirectoryReader.Open(directory);
        return reader.NumDocs;
    }

    private static IGraph CreateGraph(int build)
    {
        var graph = new Graph(new UriNode(new Uri($"http://example.org/graph/{build}")));
        IUriNode name = graph.CreateUriNode(new Uri("http://schema.org/name"));
        for (var i = 0; i < TriplesPerIndexer; i++)
        {
            graph.Assert(
                graph.CreateUriNode(new Uri($"http://example.org/{build}/{i}")),
                name,
                graph.CreateLiteralNode($"build {build} item {i}"));
        }
        return graph;
    }
}
#endif
