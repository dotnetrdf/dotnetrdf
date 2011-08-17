using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using LucSearch = Lucene.Net.Search;
using Lucene.Net.Store;
using LucUtil = Lucene.Net.Util;
using VDS.RDF.Query.FullText.Schema;

namespace VDS.RDF.Query.FullText.Search.Lucene
{
    public abstract class BaseLuceneSearchProvider
        : IFullTextSearchProvider
    {
        private Directory _indexDir;
        private LucSearch.IndexSearcher _searcher;
        private QueryParser _parser;
        private LucUtil.Version _version;
        private Analyzer _analyzer;
        private IFullTextIndexSchema _schema;

        public BaseLuceneSearchProvider(LucUtil.Version ver, Directory indexDir, Analyzer analyzer, IFullTextIndexSchema schema)
        {
            this._version = ver;
            this._indexDir = indexDir;
            this._analyzer = analyzer;
            this._schema = schema;

            //Create necessary objects
            this._searcher = new LucSearch.IndexSearcher(this._indexDir);
            this._parser = new QueryParser(this._version, this._schema.IndexField, this._analyzer);
        }

        ~BaseLuceneSearchProvider()
        {
            this.Dispose(false);
        }

        #region IFullTextSearchProvider Members

        public IEnumerable<IFullTextSearchResult> Match(string text, double scoreThreshold, int limit)
        {
            LucSearch.Query q = this._parser.Parse(text);
            LucSearch.TopDocs docs = this._searcher.Search(q, limit);
            return (from doc in docs.scoreDocs
                    where doc.score > scoreThreshold
                    select this._searcher.Doc(doc.doc).ToResult(doc.score, this._schema));
        }

        public IEnumerable<IFullTextSearchResult> Match(string text, double scoreThreshold)
        {
            LucSearch.Query q = this._parser.Parse(text);
            DocCollector collector = new DocCollector(scoreThreshold);
            this._searcher.Search(q, collector);
            return (from doc in collector.Documents
                    select this._searcher.Doc(doc.Key).ToResult(doc.Value, this._schema));
        }

        public IEnumerable<IFullTextSearchResult> Match(string text, int limit)
        {
            LucSearch.Query q = this._parser.Parse(text);
            LucSearch.TopDocs docs = this._searcher.Search(q, limit);
            return (from doc in docs.scoreDocs
                    select this._searcher.Doc(doc.doc).ToResult(doc.score, this._schema));
        }

        public IEnumerable<IFullTextSearchResult> Match(string text)
        {
            LucSearch.Query q = this._parser.Parse(text);
            DocCollector collector = new DocCollector();
            this._searcher.Search(q, collector);
            return (from doc in collector.Documents
                    select this._searcher.Doc(doc.Key).ToResult(doc.Value, this._schema));
        }

        #endregion

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);

            this.DisposeInternal();

            this._searcher.Close();
            this._indexDir.Close();
        }

        protected virtual void DisposeInternal() { }
    }
}
