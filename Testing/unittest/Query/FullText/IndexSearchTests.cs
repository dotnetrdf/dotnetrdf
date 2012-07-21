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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query;
using VDS.RDF.Query.FullText;
using VDS.RDF.Query.FullText.Indexing;
using VDS.RDF.Query.FullText.Indexing.Lucene;
using VDS.RDF.Query.FullText.Search;
using VDS.RDF.Query.FullText.Search.Lucene;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Query.FullText
{
    [TestClass]
    public class IndexSearchTests
    {
        private IGraph GetTestData()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            return g;
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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
