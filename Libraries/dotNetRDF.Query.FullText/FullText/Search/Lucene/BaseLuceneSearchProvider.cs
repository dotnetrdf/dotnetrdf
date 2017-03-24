/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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

using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Analysis;
using Lucene.Net.QueryParsers;
using LucSearch = Lucene.Net.Search;
using Lucene.Net.Store;
using LucUtil = Lucene.Net.Util;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query.FullText.Schema;

namespace VDS.RDF.Query.FullText.Search.Lucene
{
    /// <summary>
    /// Abstract Base Implementation of a Full Text Search Provider using Lucene.Net
    /// </summary>
    /// <remarks>
    /// Derived Implementations may only need to call the base constructor as the <see cref="LuceneSearchProvider">LuceneSearchProvider</see> does but if you've implemented custom indexing you may need to extend more of this class
    /// </remarks>
    public abstract class BaseLuceneSearchProvider
        : IFullTextSearchProvider, IConfigurationSerializable
    {
        private Directory _indexDir;
        private LucSearch.IndexSearcher _searcher;
        private QueryParser _parser;
        private LucUtil.Version _version;
        private Analyzer _analyzer;
        private IFullTextIndexSchema _schema;
        private bool _autoSync = true;
        private UriComparer _uriComparer = new UriComparer();

        /// <summary>
        /// Creates a new Base Lucene Search Provider
        /// </summary>
        /// <param name="ver">Lucene Version</param>
        /// <param name="indexDir">Directory</param>
        /// <param name="analyzer">Analyzer</param>
        /// <param name="schema">Index Schema</param>
        /// <param name="autoSync">Whether the Search Provider should stay in sync with the underlying index</param>
        public BaseLuceneSearchProvider(LucUtil.Version ver, Directory indexDir, Analyzer analyzer, IFullTextIndexSchema schema, bool autoSync)
        {
            this._version = ver;
            this._indexDir = indexDir;
            this._analyzer = analyzer;
            this._schema = schema;
            this._autoSync = autoSync;

            //Create necessary objects
            this._searcher = new LucSearch.IndexSearcher(this._indexDir, true);
            this._parser = new QueryParser(this._version, this._schema.IndexField, this._analyzer);
        }

        /// <summary>
        /// Creates a new Base Lucene Search Provider
        /// </summary>
        /// <param name="ver">Lucene Version</param>
        /// <param name="indexDir">Directory</param>
        /// <param name="analyzer">Analyzer</param>
        /// <param name="schema">Index Schema</param>
        public BaseLuceneSearchProvider(LucUtil.Version ver, Directory indexDir, Analyzer analyzer, IFullTextIndexSchema schema)
            : this(ver, indexDir, analyzer, schema, true) { }

        /// <summary>
        /// Destructor which ensures that the Search Provider is properly disposed of
        /// </summary>
        ~BaseLuceneSearchProvider()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets results that match the given query with the score threshold and limit applied
        /// </summary>
        /// <param name="text">Search Query</param>
        /// <param name="scoreThreshold">Score Threshold</param>
        /// <param name="limit">Result Limit</param>
        /// <returns></returns>
        public virtual IEnumerable<IFullTextSearchResult> Match(string text, double scoreThreshold, int limit)
        {
            this.EnsureCurrent();
            LucSearch.Query q = this._parser.Parse(text);
            LucSearch.TopDocs docs = this._searcher.Search(q, limit);

            return (from doc in docs.ScoreDocs
                    where doc.Score > scoreThreshold
                    select this._searcher.Doc(doc.Doc).ToResult(doc.Score, this._schema));
        }

        /// <summary>
        /// Gets results that match the given query with the score threshold applied
        /// </summary>
        /// <param name="text">Search Query</param>
        /// <param name="scoreThreshold">Score Threshold</param>
        /// <returns></returns>
        public virtual IEnumerable<IFullTextSearchResult> Match(string text, double scoreThreshold)
        {
            this.EnsureCurrent();
            LucSearch.Query q = this._parser.Parse(text);
            DocCollector collector = new DocCollector(scoreThreshold);
            this._searcher.Search(q, collector);
            return (from doc in collector.Documents
                    select this._searcher.Doc(doc.Key).ToResult(doc.Value, this._schema));
        }

        /// <summary>
        /// Gets results that match the given query with a limit applied
        /// </summary>
        /// <param name="text">Search Query</param>
        /// <param name="limit">Result Limit</param>
        /// <returns></returns>
        public virtual IEnumerable<IFullTextSearchResult> Match(string text, int limit)
        {
            this.EnsureCurrent();
            LucSearch.Query q = this._parser.Parse(text);
            LucSearch.TopDocs docs = this._searcher.Search(q, limit);

            return (from doc in docs.ScoreDocs
                    select this._searcher.Doc(doc.Doc).ToResult(doc.Score, this._schema));
        }

        /// <summary>
        /// Gets results that match the given query
        /// </summary>
        /// <param name="text">Search Query</param>
        /// <returns></returns>
        public virtual IEnumerable<IFullTextSearchResult> Match(string text)
        {
            this.EnsureCurrent();
            LucSearch.Query q = this._parser.Parse(text);
            DocCollector collector = new DocCollector();
            this._searcher.Search(q, collector);
            return (from doc in collector.Documents
                    select this._searcher.Doc(doc.Key).ToResult(doc.Value, this._schema));
        }

        /// <summary>
        /// Gets results that match the given query with the score threshold and limit applied
        /// </summary>
        /// <param name="graphUris">Graph URIs</param>
        /// <param name="text">Search Query</param>
        /// <param name="scoreThreshold">Score Threshold</param>
        /// <param name="limit">Result Limit</param>
        /// <returns></returns>
        public virtual IEnumerable<IFullTextSearchResult> Match(IEnumerable<Uri> graphUris, string text, double scoreThreshold, int limit)
        {
            this.EnsureCurrent();
            LucSearch.Query q = this._parser.Parse(text);
            DocCollector collector = new DocCollector();
            this._searcher.Search(q, collector);

            IEnumerable<IFullTextSearchResult> results = from doc in collector.Documents
                                                         where doc.Value > scoreThreshold
                                                         select this._searcher.Doc(doc.Key).ToResult(doc.Value, this._schema);
            return this.FilterByGraph(graphUris, results).Take(limit);
        }

        /// <summary>
        /// Gets results that match the given query with the score threshold applied
        /// </summary>
        /// <param name="graphUris">Graph URIs</param>
        /// <param name="text">Search Query</param>
        /// <param name="scoreThreshold">Score Threshold</param>
        /// <returns></returns>
        public virtual IEnumerable<IFullTextSearchResult> Match(IEnumerable<Uri> graphUris, string text, double scoreThreshold)
        {
            this.EnsureCurrent();
            LucSearch.Query q = this._parser.Parse(text);
            DocCollector collector = new DocCollector(scoreThreshold);
            this._searcher.Search(q, collector);
            IEnumerable<IFullTextSearchResult> results = from doc in collector.Documents
                                                         select this._searcher.Doc(doc.Key).ToResult(doc.Value, this._schema);
            return this.FilterByGraph(graphUris, results);
        }

        /// <summary>
        /// Gets results that match the given query with a limit applied
        /// </summary>
        /// <param name="graphUris">Graph URIs</param>
        /// <param name="text">Search Query</param>
        /// <param name="limit">Result Limit</param>
        /// <returns></returns>
        public virtual IEnumerable<IFullTextSearchResult> Match(IEnumerable<Uri> graphUris, string text, int limit)
        {
            this.EnsureCurrent();
            LucSearch.Query q = this._parser.Parse(text);
            DocCollector collector = new DocCollector();
            this._searcher.Search(q, collector);

            IEnumerable<IFullTextSearchResult> results = from doc in collector.Documents
                                                         select this._searcher.Doc(doc.Key).ToResult(doc.Value, this._schema);
            return this.FilterByGraph(graphUris, results).Take(limit);
        }

        /// <summary>
        /// Gets results that match the given query
        /// </summary>
        /// <param name="graphUris">Graph URIs</param>
        /// <param name="text">Search Query</param>
        /// <returns></returns>
        public virtual IEnumerable<IFullTextSearchResult> Match(IEnumerable<Uri> graphUris, string text)
        {
            this.EnsureCurrent();
            LucSearch.Query q = this._parser.Parse(text);
            DocCollector collector = new DocCollector();
            this._searcher.Search(q, collector);
            IEnumerable<IFullTextSearchResult> results = from doc in collector.Documents
                                                         select this._searcher.Doc(doc.Key).ToResult(doc.Value, this._schema);
            return this.FilterByGraph(graphUris, results);
        }

        /// <summary>
        /// Filters a set of results to ensure they occur in the given Graph(s)
        /// </summary>
        /// <param name="graphUris">Graph URIs</param>
        /// <param name="results">Results</param>
        /// <returns></returns>
        private IEnumerable<IFullTextSearchResult> FilterByGraph(IEnumerable<Uri> graphUris, IEnumerable<IFullTextSearchResult> results)
        {
            if (graphUris == null)
            {
                return results;
            }
            else
            {
                HashSet<Uri> uris = new HashSet<Uri>(graphUris, new UriComparer());
                if (uris.Count == 0) return results;
                return results.Where(r => uris.Contains(r.GraphUri));
            }
        }

        /// <summary>
        /// Gets whether this search provider is always seeing the latest state of the index
        /// </summary>
        public bool IsAutoSynced
        {
            get
            {
                return this._autoSync;
            }
        }

        /// <summary>
        /// Ensures that the Index Searcher is searching the current Index unless this feature has been disabled by the user
        /// </summary>
        private void EnsureCurrent()
        {
            if (this._autoSync)
            {
                if (!this._searcher.IndexReader.IsCurrent())
                {
                    IndexReader oldReader = this._searcher.IndexReader;
                    this._searcher = new LucSearch.IndexSearcher(oldReader.Reopen());
                    oldReader.Dispose();
                }
            }
        }

        /// <summary>
        /// Disposes of the Search Provider
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Disposes of the Search Provider
        /// </summary>
        /// <param name="disposing">Whether this was called by the Dispose method</param>
        private void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);

            this.DisposeInternal();

            if (this._searcher != null) this._searcher.Dispose();
        }

        /// <summary>
        /// Virtual method that can be overridden to add implementation specific dispose logic
        /// </summary>
        protected virtual void DisposeInternal() { }

        /// <summary>
        /// Serializes Configuration of this Provider
        /// </summary>
        /// <param name="context">Serialization Context</param>
        public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            context.EnsureObjectFactory(typeof(FullTextObjectFactory));

            INode searcherObj = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            INode dnrType = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType));
            INode searcherClass = context.Graph.CreateUriNode(UriFactory.Create(FullTextHelper.ClassSearcher));
            INode index = context.Graph.CreateUriNode(UriFactory.Create(FullTextHelper.PropertyIndex));
            INode schema = context.Graph.CreateUriNode(UriFactory.Create(FullTextHelper.PropertySchema));
            INode analyzer = context.Graph.CreateUriNode(UriFactory.Create(FullTextHelper.PropertyAnalyzer));
            INode indexSync = context.Graph.CreateUriNode(UriFactory.Create(FullTextHelper.PropertyIndexSync));

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

            //Serialize auto-sync settings
            context.Graph.Assert(searcherObj, indexSync, this._autoSync.ToLiteral(context.Graph));
        }
    }
}
