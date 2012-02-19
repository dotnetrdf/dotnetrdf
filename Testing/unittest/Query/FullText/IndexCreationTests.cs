using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lucene.Net.Search;
using VDS.RDF.Query;
using VDS.RDF.Query.FullText;
using VDS.RDF.Query.FullText.Indexing;
using VDS.RDF.Query.FullText.Indexing.Lucene;
using VDS.RDF.Query.FullText.Search.Lucene;

namespace VDS.RDF.Test.Query.FullText
{
    [TestClass]
    public class IndexCreationTests
    {
        private IGraph GetTestData()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            return g;
        }

        [TestMethod]
        public void FullTextIndexCreationLuceneObjects()
        {
            IFullTextIndexer indexer = null;
            try
            {
                indexer = new LuceneObjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                indexer.Index(this.GetTestData());
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
            finally
            {
                if (indexer != null) indexer.Dispose();
            }
        }

        [TestMethod]
        public void FullTextIndexCreationLuceneSubjects()
        {
            IFullTextIndexer indexer = null;
            try
            {
                indexer = new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                indexer.Index(this.GetTestData());
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
            finally
            {
                if (indexer != null) indexer.Dispose();
            }
        }

        [TestMethod]
        public void FullTextIndexCreationLucenePredicates()
        {
            IFullTextIndexer indexer = null;
            try
            {
                indexer = new LucenePredicatesIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                indexer.Index(this.GetTestData());
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
            finally
            {
                if (indexer != null) indexer.Dispose();
            }
        }

        [TestMethod]
        public void FullTextIndexDestructionLuceneSubjects()
        {
            IFullTextIndexer indexer = null;
            try
            {
                LuceneSearchProvider provider = null;
                int origCount;
                try
                {
                    provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
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

                indexer = new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                IGraph g = this.GetTestData();
                indexer.Index(g);
                indexer.Dispose();
                indexer = null;

                int currCount;
                try
                {
                    provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
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

                indexer = new LuceneSubjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                indexer.Unindex(g);
                indexer.Dispose();
                indexer = null;
                try
                {
                    provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
                    currCount = provider.Match("http").Count();
                    Console.WriteLine("After unindexing search returns " + currCount + " result(s)");
                }
                finally
                {
                    if (provider != null) provider.Dispose();
                }

                Assert.AreEqual(origCount, currCount, "Current Count should match original count");
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
            finally
            {
                if (indexer != null) indexer.Dispose();
                LuceneTestHarness.Index.Close();
            }
        }

        [TestMethod]
        public void FullTextIndexDestructionLuceneObjects()
        {
            IFullTextIndexer indexer = null;
            try
            {
                LuceneSearchProvider provider = null;
                int origCount;
                try
                {
                    provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
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

                indexer = new LuceneObjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                IGraph g = this.GetTestData();
                indexer.Index(g);
                indexer.Dispose();
                indexer = null;

                int currCount;
                try
                {
                    provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
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

                indexer = new LuceneObjectsIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                indexer.Unindex(g);
                indexer.Dispose();
                indexer = null;
                try
                {
                    provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
                    currCount = provider.Match("http").Count();
                    Console.WriteLine("After unindexing search returns " + currCount + " result(s)");
                }
                finally
                {
                    if (provider != null) provider.Dispose();
                }

                Assert.AreEqual(origCount, currCount, "Current Count should match original count");
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
            finally
            {
                if (indexer != null) indexer.Dispose();
                LuceneTestHarness.Index.Close();
            }
        }

        [TestMethod]
        public void FullTextIndexDestructionLucenePredicates()
        {
            IFullTextIndexer indexer = null;
            try
            {
                LuceneSearchProvider provider = null;
                int origCount;
                try
                {
                    provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
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

                indexer = new LucenePredicatesIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                IGraph g = this.GetTestData();
                indexer.Index(g);
                indexer.Dispose();
                indexer = null;

                int currCount;
                try
                {
                    provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
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

                indexer = new LucenePredicatesIndexer(LuceneTestHarness.Index, LuceneTestHarness.Analyzer, LuceneTestHarness.Schema);
                indexer.Unindex(g);
                indexer.Dispose();
                indexer = null;
                try
                {
                    provider = new LuceneSearchProvider(LuceneTestHarness.LuceneVersion, LuceneTestHarness.Index);
                    currCount = provider.Match("http").Count();
                    Console.WriteLine("After unindexing search returns " + currCount + " result(s)");
                }
                finally
                {
                    if (provider != null) provider.Dispose();
                }

                Assert.AreEqual(origCount, currCount, "Current Count should match original count");
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
            finally
            {
                if (indexer != null) indexer.Dispose();
                LuceneTestHarness.Index.Close();
            }
        }
    }
}
