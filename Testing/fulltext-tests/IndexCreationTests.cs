using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query;
using VDS.RDF.Query.FullText;
using VDS.RDF.Query.FullText.Indexing;
using VDS.RDF.Query.FullText.Indexing.Lucene;

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
    }
}
