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

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VDS.RDF.Query;
using VDS.RDF.Query.FullText;
using VDS.RDF.Query.FullText.Indexing;
using VDS.RDF.Query.FullText.Indexing.Lucene;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.FullText.Search.Lucene;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.FullText
{
    [TestFixture]
    public class IndexSearchTests
    {
        private IGraph GetTestData()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            return g;
        }

        [Test]
        public void FullTextIndexSearchLuceneObjects()
        {
            IFullTextIndexer indexer = null;
            IFullTextSearchProvider provider = null;
            try
            {
                indexer = new LuceneObjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                indexer.Index(this.GetTestData());
            }
            finally
            {
                if (indexer != null) indexer.Dispose();
            }

            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
                NTriplesFormatter formatter = new NTriplesFormatter();

                foreach (IFullTextSearchResult result in provider.Match("http"))
                {
                    Console.WriteLine(result.Node.ToString(formatter) + " - Scores " + result.Score);
                }
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }

        }

        [Test]
        public void FullTextIndexSearchLuceneObjectsWithLimit()
        {
            IFullTextIndexer indexer = null;
            IFullTextSearchProvider provider = null;
            try
            {
                indexer = new LuceneObjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                indexer.Index(this.GetTestData());
            }
            finally
            {
                if (indexer != null) indexer.Dispose();
            }

            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
                NTriplesFormatter formatter = new NTriplesFormatter();

                int i = 0;
                foreach (IFullTextSearchResult result in provider.Match("http", 5))
                {
                    i++;
                    Console.WriteLine(result.Node.ToString(formatter) + " - Scores " + result.Score);
                }
                Assert.IsTrue(i <= 5, "Should be a max of 5 results");
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }

        }

        [Test]
        public void FullTextIndexSearchLuceneObjectsWithThreshold()
        {
            IFullTextIndexer indexer = null;
            IFullTextSearchProvider provider = null;
            try
            {
                indexer = new LuceneObjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                indexer.Index(this.GetTestData());
            }
            finally
            {
                if (indexer != null) indexer.Dispose();
            }

            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
                NTriplesFormatter formatter = new NTriplesFormatter();

                foreach (IFullTextSearchResult result in provider.Match("http", 0.75d))
                {
                    Console.WriteLine(result.Node.ToString(formatter) + " - Scores " + result.Score);
                    Assert.IsTrue(result.Score >= 0.75d, "Score should be higher than desired threshold");
                }
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }

        }

        [Test]
        public void FullTextIndexSearchLuceneObjectsWithLimitAndThreshold()
        {
            IFullTextIndexer indexer = null;
            IFullTextSearchProvider provider = null;
            try
            {
                indexer = new LuceneObjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                indexer.Index(this.GetTestData());
            }
            finally
            {
                if (indexer != null) indexer.Dispose();
            }

            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
                NTriplesFormatter formatter = new NTriplesFormatter();

                int i = 0;
                foreach (IFullTextSearchResult result in provider.Match("http", 1.0d, 5))
                {
                    i++;
                    Console.WriteLine(result.Node.ToString(formatter) + " - Scores " + result.Score);
                    Assert.IsTrue(result.Score >= 1.0d, "Score should be higher than desired threshold");
                }
                Assert.IsTrue(i <= 5, "Should be a max of 5 results");
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }

        }

        [Test]
        public void FullTextIndexSearchLuceneSubjects()
        {
            IFullTextIndexer indexer = null;
            IFullTextSearchProvider provider = null;
            try
            {
                indexer = new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                indexer.Index(this.GetTestData());
            }
            finally
            {
                if (indexer != null) indexer.Dispose();
            }

            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
                NTriplesFormatter formatter = new NTriplesFormatter();

                foreach (IFullTextSearchResult result in provider.Match("http"))
                {
                    Console.WriteLine(result.Node.ToString(formatter) + " - Scores " + result.Score);
                }
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }

        }

        [Test]
        public void FullTextIndexSearchLuceneSubjectsWithLimit()
        {
            IFullTextIndexer indexer = null;
            IFullTextSearchProvider provider = null;
            try
            {
                indexer = new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                indexer.Index(this.GetTestData());
            }
            finally
            {
                if (indexer != null) indexer.Dispose();
            }

            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
                NTriplesFormatter formatter = new NTriplesFormatter();

                int i = 0;
                foreach (IFullTextSearchResult result in provider.Match("http", 5))
                {
                    i++;
                    Console.WriteLine(result.Node.ToString(formatter) + " - Scores " + result.Score);
                }
                Assert.IsTrue(i <= 5, "Should be a max of 5 results");
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }

        }

        [Test]
        public void FullTextIndexSearchLuceneSubjectsWithThreshold()
        {
            IFullTextIndexer indexer = null;
            IFullTextSearchProvider provider = null;
            try
            {
                indexer = new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                indexer.Index(this.GetTestData());
            }
            finally
            {
                if (indexer != null) indexer.Dispose();
            }

            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
                NTriplesFormatter formatter = new NTriplesFormatter();

                foreach (IFullTextSearchResult result in provider.Match("http", 0.75d))
                {
                    Console.WriteLine(result.Node.ToString(formatter) + " - Scores " + result.Score);
                    Assert.IsTrue(result.Score >= 0.75d, "Score should be higher than desired threshold");
                }
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }

        }

        [Test]
        public void FullTextIndexSearchLuceneSubjectsWithLimitAndThreshold()
        {
            IFullTextIndexer indexer = null;
            IFullTextSearchProvider provider = null;
            try
            {
                indexer = new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                indexer.Index(this.GetTestData());
            }
            finally
            {
                if (indexer != null) indexer.Dispose();
            }

            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
                NTriplesFormatter formatter = new NTriplesFormatter();

                int i = 0;
                foreach (IFullTextSearchResult result in provider.Match("http", 1.0d, 5))
                {
                    i++;
                    Console.WriteLine(result.Node.ToString(formatter) + " - Scores " + result.Score);
                    Assert.IsTrue(result.Score >= 1.0d, "Score should be higher than desired threshold");
                }
                Assert.IsTrue(i <= 5, "Should be a max of 5 results");
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }

        }

        [Test]
        public void FullTextIndexSearchLucenePredicates()
        {
            IFullTextIndexer indexer = null;
            IFullTextSearchProvider provider = null;
            try
            {
                indexer = new LucenePredicatesIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                indexer.Index(this.GetTestData());
            }
            finally
            {
                if (indexer != null) indexer.Dispose();
            }

            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
                NTriplesFormatter formatter = new NTriplesFormatter();

                foreach (IFullTextSearchResult result in provider.Match("http"))
                {
                    Console.WriteLine(result.Node.ToString(formatter) + " - Scores " + result.Score);
                }
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }

        }

        [Test]
        public void FullTextIndexSearchLucenePredicatesWithLimit()
        {
            IFullTextIndexer indexer = null;
            IFullTextSearchProvider provider = null;
            try
            {
                indexer = new LucenePredicatesIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                indexer.Index(this.GetTestData());
            }
            finally
            {
                if (indexer != null) indexer.Dispose();
            }

            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
                NTriplesFormatter formatter = new NTriplesFormatter();

                int i = 0;
                foreach (IFullTextSearchResult result in provider.Match("http", 5))
                {
                    i++;
                    Console.WriteLine(result.Node.ToString(formatter) + " - Scores " + result.Score);
                }
                Assert.IsTrue(i <= 5, "Should be a max of 5 results");
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }

        }

        [Test]
        public void FullTextIndexSearchLucenePredicatesWithThreshold()
        {
            IFullTextIndexer indexer = null;
            IFullTextSearchProvider provider = null;
            try
            {
                indexer = new LucenePredicatesIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                indexer.Index(this.GetTestData());
            }
            finally
            {
                if (indexer != null) indexer.Dispose();
            }

            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
                NTriplesFormatter formatter = new NTriplesFormatter();

                foreach (IFullTextSearchResult result in provider.Match("http", 0.75d))
                {
                    Console.WriteLine(result.Node.ToString(formatter) + " - Scores " + result.Score);
                    Assert.IsTrue(result.Score >= 0.75d, "Score should be higher than desired threshold");
                }
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }

        }

        [Test]
        public void FullTextIndexSearchLucenePredicatesWithLimitAndThreshold()
        {
            IFullTextIndexer indexer = null;
            IFullTextSearchProvider provider = null;
            try
            {
                indexer = new LucenePredicatesIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                indexer.Index(this.GetTestData());
            }
            finally
            {
                if (indexer != null) indexer.Dispose();
            }

            try
            {
                provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
                NTriplesFormatter formatter = new NTriplesFormatter();

                int i = 0;
                foreach (IFullTextSearchResult result in provider.Match("http", 1.0d, 5))
                {
                    i++;
                    Console.WriteLine(result.Node.ToString(formatter) + " - Scores " + result.Score);
                    Assert.IsTrue(result.Score >= 1.0d, "Score should be higher than desired threshold");
                }
                Assert.IsTrue(i <= 5, "Should be a max of 5 results");
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }

        }

    }
}
