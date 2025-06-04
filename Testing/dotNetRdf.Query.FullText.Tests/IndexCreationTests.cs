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
using System.Linq;
using Xunit;
using VDS.RDF.Query.FullText.Indexing;
using VDS.RDF.Query.FullText.Indexing.Lucene;
using VDS.RDF.Query.FullText.Schema;
using VDS.RDF.Query.FullText.Search.Lucene;

namespace VDS.RDF.Query.FullText;


[Collection("FullText")]
public class IndexCreationTests
{
    private LuceneTestHarness _testHarness = new LuceneTestHarness();

    private IGraph GetTestData()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        return g;
    }

    [Fact]
    public void FullTextIndexCreationLuceneObjects()
    {
        IFullTextIndexer indexer = null;
        try
        {
            indexer = new LuceneObjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema);
            indexer.Index(GetTestData());
        }
        finally
        {
            if (indexer != null) indexer.Dispose();
        }
    }

    [Fact]
    public void FullTextIndexCreationLuceneSubjects()
    {
        IFullTextIndexer indexer = null;
        try
        {
            indexer = new LuceneSubjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema);
            indexer.Index(GetTestData());
        }
        finally
        {
            if (indexer != null) indexer.Dispose();
        }
    }

    [Fact]
    public void FullTextIndexCreationLucenePredicates()
    {
        IFullTextIndexer indexer = null;
        try
        {
            indexer = new LucenePredicatesIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema);
            indexer.Index(GetTestData());
        }
        finally
        {
            if (indexer != null) indexer.Dispose();
        }
    }

    [Fact]
    public void FullTextIndexDestructionLuceneSubjects()
    {
        IFullTextIndexer indexer = null;
        try
        {
            LuceneSearchProvider provider = null;
            int origCount;
            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, _testHarness.Index);
                origCount = provider.Match("http").Count();
                provider.Dispose();
            }
            catch
            {
                origCount = 0;
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }
            Console.WriteLine("Prior to indexing search returns " + origCount + " result(s)");

            indexer = new LuceneSubjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema);
            IGraph g = GetTestData();
            indexer.Index(g);
            indexer.Dispose();
            indexer = null;

            int currCount;
            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, _testHarness.Index);
                currCount = provider.Match("http").Count();
                provider.Dispose();
            }
            catch
            {
                currCount = 0;
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }
            Console.WriteLine("After indexing search returns " + currCount + " result(s)");

            indexer = new LuceneSubjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema);
            indexer.Unindex(g);
            indexer.Dispose();
            indexer = null;
            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, _testHarness.Index);
                currCount = provider.Match("http").Count();
                Console.WriteLine("After unindexing search returns " + currCount + " result(s)");
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }

            Assert.Equal(origCount, currCount);
        }
        finally
        {
            if (indexer != null) indexer.Dispose();
            _testHarness.Index.Dispose();
        }
    }

    [Fact]
    public void FullTextIndexDestructionLuceneObjects()
    {
        IFullTextIndexer indexer = null;
        try
        {
            LuceneSearchProvider provider = null;
            int origCount;
            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, _testHarness.Index);
                origCount = provider.Match("http").Count();
                provider.Dispose();
            }
            catch
            {
                origCount = 0;
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }
            Console.WriteLine("Prior to indexing search returns " + origCount + " result(s)");

            indexer = new LuceneObjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema);
            IGraph g = GetTestData();
            indexer.Index(g);
            indexer.Dispose();
            indexer = null;

            int currCount;
            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, _testHarness.Index);
                currCount = provider.Match("http").Count();
                provider.Dispose();
            }
            catch
            {
                currCount = 0;
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }
            Console.WriteLine("After indexing search returns " + currCount + " result(s)");

            indexer = new LuceneObjectsIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema);
            indexer.Unindex(g);
            indexer.Dispose();
            indexer = null;
            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, _testHarness.Index);
                currCount = provider.Match("http").Count();
                Console.WriteLine("After unindexing search returns " + currCount + " result(s)");
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }

            Assert.Equal(origCount, currCount);
        }
        finally
        {
            if (indexer != null) indexer.Dispose();
            _testHarness.Index.Dispose();
        }
    }

    [Fact]
    public void FullTextIndexDestructionLucenePredicates()
    {
        IFullTextIndexer indexer = null;
        try
        {
            LuceneSearchProvider provider = null;
            int origCount;
            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, _testHarness.Index);
                origCount = provider.Match("http").Count();
                provider.Dispose();
            }
            catch
            {
                origCount = 0;
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }
            Console.WriteLine("Prior to indexing search returns " + origCount + " result(s)");

            indexer = new LucenePredicatesIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema);
            IGraph g = GetTestData();
            indexer.Index(g);
            indexer.Dispose();
            indexer = null;

            int currCount;
            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, _testHarness.Index);
                currCount = provider.Match("http").Count();
                provider.Dispose();
            }
            catch
            {
                currCount = 0;
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }
            Console.WriteLine("After indexing search returns " + currCount + " result(s)");

            indexer = new LucenePredicatesIndexer(_testHarness.Index, _testHarness.Analyzer, _testHarness.Schema);
            indexer.Unindex(g);
            indexer.Dispose();
            indexer = null;
            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, _testHarness.Index);
                currCount = provider.Match("http").Count();
                Console.WriteLine("After unindexing search returns " + currCount + " result(s)");
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }

            Assert.Equal(origCount, currCount);
        }
        finally
        {
            if (indexer != null) indexer.Dispose();
            _testHarness.Index.Dispose();
        }
    }

    [Fact]
    public void FullTextIndexMultiOccurrenceRemoval()
    {
        IFullTextIndexer indexer = null;
        try
        {
            indexer = new LuceneObjectsIndexer(_testHarness.Index, _testHarness.Analyzer, new DefaultIndexSchema());
            
            var g = new Graph();
            INode example = g.CreateLiteralNode("This is an example node which we'll index multiple times");

            for (var i = 0; i < 10; i++)
            {
                g.Assert(new Triple(g.CreateBlankNode(), g.CreateUriNode(UriFactory.Root.Create("ex:predicate")), example));
            }
            indexer.Index(g);

            var searcher = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, _testHarness.Index);
            Assert.Equal(10, searcher.Match("example").Count());

            for (var i = 9; i >= 0; i--)
            {
                indexer.Unindex(g, g.Triples.First());
                indexer.Flush();
                Assert.Equal(i, searcher.Match("example").Count());
            }
        }
        finally
        {
            if (indexer != null) indexer.Dispose();
        }
    }
}
#endif