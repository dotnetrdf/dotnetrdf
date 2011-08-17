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
            catch (Exception ex)
            {
                TestTools.ReportError("Index Creation Error", ex, true);
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
            catch (Exception ex)
            {
                TestTools.ReportError("Index Search Error", ex, true);
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
            catch (Exception ex)
            {
                TestTools.ReportError("Index Creation Error", ex, true);
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
            catch (Exception ex)
            {
                TestTools.ReportError("Index Search Error", ex, true);
            }
            finally
            {
                if (provider != null) provider.Dispose();
            }

        }
    }
}
