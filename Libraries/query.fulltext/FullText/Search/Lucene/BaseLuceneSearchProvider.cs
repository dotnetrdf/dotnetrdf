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
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query.FullText.Schema;

namespace VDS.RDF.Query.FullText.Search.Lucene
{
    public abstract class BaseLuceneSearchProvider
        : IFullTextSearchProvider, IConfigurationSerializable
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

            if (this._searcher != null) this._searcher.Close();
        }

        protected virtual void DisposeInternal() { }

        public void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            context.EnsureObjectFactory(typeof(FullTextObjectFactory));

            INode searcherObj = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode searcherClass = context.Graph.CreateUriNode(new Uri(FullTextHelper.ClassSearcher));
            INode index = context.Graph.CreateUriNode(new Uri(FullTextHelper.PropertyIndex));
            INode schema = context.Graph.CreateUriNode(new Uri(FullTextHelper.PropertySchema));
            INode analyzer = context.Graph.CreateUriNode(new Uri(FullTextHelper.PropertyAnalyzer));

            //Basic Properties
            context.Graph.Assert(searcherObj, rdfType, searcherClass);
            context.Graph.Assert(searcherObj, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName + ", dotNetRDF.Query.FullText"));

            //Serialize and link the Index
            INode indexObj = context.Graph.CreateBlankNode();
            context.NextSubject = indexObj;
            this._indexDir.SerializeConfiguration(context);
            context.Graph.Assert(searcherObj, index, indexObj);

            //Serialize and link the Schema
            INode schemaObj = context.Graph.CreateBlankNode();
            context.NextSubject = schemaObj;
            if (this._schema is IConfigurationSerializable)
            {
                ((IConfigurationSerializable)this._schema).SerializeConfiguration(context);
            }
            else
            {
                throw new DotNetRdfConfigurationException("Unable to serialize configuration for this Lucene Search Provider as the IFullTextSchema used does not implement the IConfigurationSerializable interface");
            }
            context.Graph.Assert(searcherObj, schema, schemaObj);

            //Serialize and link the Analyzer
            INode analyzerObj = context.Graph.CreateBlankNode();
            context.NextSubject = analyzerObj;
            this._analyzer.SerializeConfiguration(context);
            context.Graph.Assert(searcherObj, index, analyzerObj);
        }
    }
}
