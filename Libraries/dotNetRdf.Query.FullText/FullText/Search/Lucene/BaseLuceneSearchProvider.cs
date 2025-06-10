/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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
using LucSearch = Lucene.Net.Search;
using Lucene.Net.Store;
using LucUtil = Lucene.Net.Util;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query.FullText.Schema;
using Lucene.Net.Util;
using Lucene.Net.QueryParsers.Flexible.Standard;
using Lucene.Net.QueryParsers.Flexible.Standard.Config;

namespace VDS.RDF.Query.FullText.Search.Lucene;

/// <summary>
/// Abstract Base Implementation of a Full Text Search Provider using Lucene.Net.
/// </summary>
/// <remarks>
/// Derived Implementations may only need to call the base constructor as the <see cref="LuceneSearchProvider">LuceneSearchProvider</see> does but if you've implemented custom indexing you may need to extend more of this class.
/// </remarks>
public abstract class BaseLuceneSearchProvider
    : IFullTextSearchProvider, IConfigurationSerializable
{
    private readonly Directory _indexDir;
    private DirectoryReader _indexReader;
    private LucSearch.IndexSearcher _searcher;
    private readonly StandardQueryParser _parser;
    private LuceneVersion _version;
    private readonly Analyzer _analyzer;
    private readonly IFullTextIndexSchema _schema;
    private UriComparer _uriComparer = new();

    /// <summary>
    /// Creates a new Base Lucene Search Provider.
    /// </summary>
    /// <param name="ver">Lucene Version.</param>
    /// <param name="indexDir">Directory.</param>
    /// <param name="analyzer">Analyzer.</param>
    /// <param name="schema">Index Schema.</param>
    /// <param name="autoSync">Whether the Search Provider should stay in sync with the underlying index.</param>
    protected BaseLuceneSearchProvider(LuceneVersion ver, Directory indexDir, Analyzer analyzer, IFullTextIndexSchema schema, bool autoSync)
    {
        _version = ver;
        _indexDir = indexDir;
        _analyzer = analyzer;
        _schema = schema;
        IsAutoSynced = autoSync;

        //Create necessary objects
        _indexReader = DirectoryReader.Open(indexDir);
        _searcher = new LucSearch.IndexSearcher(_indexReader);
        _parser = new StandardQueryParser();
        _parser.QueryConfigHandler.Set(ConfigurationKeys.ANALYZER, _analyzer);
    }

    /// <summary>
    /// Creates a new Base Lucene Search Provider.
    /// </summary>
    /// <param name="ver">Lucene Version.</param>
    /// <param name="indexDir">Directory.</param>
    /// <param name="analyzer">Analyzer.</param>
    /// <param name="schema">Index Schema.</param>
    protected BaseLuceneSearchProvider(LuceneVersion ver, Directory indexDir, Analyzer analyzer, IFullTextIndexSchema schema)
        : this(ver, indexDir, analyzer, schema, true) { }

    /// <summary>
    /// Destructor which ensures that the Search Provider is properly disposed of
    /// </summary>
    ~BaseLuceneSearchProvider()
    {
        Dispose(false);
    }

    /// <summary>
    /// Gets results that match the given query with the score threshold and limit applied.
    /// </summary>
    /// <param name="text">Search Query.</param>
    /// <param name="scoreThreshold">Score Threshold.</param>
    /// <param name="limit">Result Limit.</param>
    /// <returns></returns>
    public virtual IEnumerable<IFullTextSearchResult> Match(string text, double scoreThreshold, int limit)
    {
        EnsureCurrent();
        LucSearch.Query q = _parser.Parse(text, _schema.IndexField);
        LucSearch.TopDocs docs = _searcher.Search(q, limit);

        return from doc in docs.ScoreDocs
            where doc.Score > scoreThreshold
            select _searcher.Doc(doc.Doc).ToResult(doc.Score, _schema);
    }

    /// <summary>
    /// Gets results that match the given query with the score threshold applied.
    /// </summary>
    /// <param name="text">Search Query.</param>
    /// <param name="scoreThreshold">Score Threshold.</param>
    /// <returns></returns>
    public virtual IEnumerable<IFullTextSearchResult> Match(string text, double scoreThreshold)
    {
        EnsureCurrent();
        LucSearch.Query q = _parser.Parse(text, _schema.IndexField);
        var collector = new DocCollector(scoreThreshold);
        _searcher.Search(q, collector);
        return from doc in collector.Documents
            select _searcher.Doc(doc.Key).ToResult(doc.Value, _schema);
    }

    /// <summary>
    /// Gets results that match the given query with a limit applied.
    /// </summary>
    /// <param name="text">Search Query.</param>
    /// <param name="limit">Result Limit.</param>
    /// <returns></returns>
    public virtual IEnumerable<IFullTextSearchResult> Match(string text, int limit)
    {
        EnsureCurrent();
        LucSearch.Query q = _parser.Parse(text, _schema.IndexField);
        LucSearch.TopDocs docs = _searcher.Search(q, limit);

        return from doc in docs.ScoreDocs
            select _searcher.Doc(doc.Doc).ToResult(doc.Score, _schema);
    }

    /// <summary>
    /// Gets results that match the given query.
    /// </summary>
    /// <param name="text">Search Query.</param>
    /// <returns></returns>
    public virtual IEnumerable<IFullTextSearchResult> Match(string text)
    {
        EnsureCurrent();
        LucSearch.Query q = _parser.Parse(text, _schema.IndexField);
        var collector = new DocCollector();
        _searcher.Search(q, collector);
        return (from doc in collector.Documents
                select _searcher.Doc(doc.Key).ToResult(doc.Value, _schema));
    }

    /// <summary>
    /// Gets results that match the given query with the score threshold and limit applied.
    /// </summary>
    /// <param name="graphUris">Graph URIs.</param>
    /// <param name="text">Search Query.</param>
    /// <param name="scoreThreshold">Score Threshold.</param>
    /// <param name="limit">Result Limit.</param>
    /// <returns></returns>
    [Obsolete("Replaced by Match(IEnumerable<IRefNode>, string, double, int)")]
    public virtual IEnumerable<IFullTextSearchResult> Match(IEnumerable<Uri> graphUris, string text, double scoreThreshold, int limit)
    {
        EnsureCurrent();
        LucSearch.Query q = _parser.Parse(text, _schema.IndexField);
        var collector = new DocCollector();
        _searcher.Search(q, collector);

        IEnumerable<IFullTextSearchResult> results = from doc in collector.Documents
                                                     where doc.Value > scoreThreshold
                                                     select _searcher.Doc(doc.Key).ToResult(doc.Value, _schema);
        return FilterByGraph(graphUris, results).Take(limit);
    }

    /// <summary>
    /// Gets results that match the given query with the score threshold applied.
    /// </summary>
    /// <param name="graphUris">Graph URIs.</param>
    /// <param name="text">Search Query.</param>
    /// <param name="scoreThreshold">Score Threshold.</param>
    /// <returns></returns>
    [Obsolete("Replaced by Match(IEnumerable<IRefNode>, string, double)")]
    public virtual IEnumerable<IFullTextSearchResult> Match(IEnumerable<Uri> graphUris, string text, double scoreThreshold)
    {
        EnsureCurrent();
        LucSearch.Query q = _parser.Parse(text, _schema.IndexField);
        var collector = new DocCollector(scoreThreshold);
        _searcher.Search(q, collector);
        IEnumerable<IFullTextSearchResult> results = from doc in collector.Documents
                                                     select _searcher.Doc(doc.Key).ToResult(doc.Value, _schema);
        return FilterByGraph(graphUris, results);
    }

    /// <summary>
    /// Gets results that match the given query with a limit applied.
    /// </summary>
    /// <param name="graphUris">Graph URIs.</param>
    /// <param name="text">Search Query.</param>
    /// <param name="limit">Result Limit.</param>
    /// <returns></returns>
    [Obsolete("Replaced by Match(IEnumerable<IRefNode>, string, int)")]
    public virtual IEnumerable<IFullTextSearchResult> Match(IEnumerable<Uri> graphUris, string text, int limit)
    {
        EnsureCurrent();
        LucSearch.Query q = _parser.Parse(text, _schema.IndexField);
        var collector = new DocCollector();
        _searcher.Search(q, collector);

        IEnumerable<IFullTextSearchResult> results = from doc in collector.Documents
                                                     select _searcher.Doc(doc.Key).ToResult(doc.Value, _schema);
        return FilterByGraph(graphUris, results).Take(limit);
    }

    /// <summary>
    /// Gets results that match the given query.
    /// </summary>
    /// <param name="graphUris">Graph URIs.</param>
    /// <param name="text">Search Query.</param>
    /// <returns></returns>
    [Obsolete("Replaced by Match(IEnumerable<IRefNode>, string)")]
    public virtual IEnumerable<IFullTextSearchResult> Match(IEnumerable<Uri> graphUris, string text)
    {
        EnsureCurrent();
        LucSearch.Query q = _parser.Parse(text, _schema.IndexField);
        var collector = new DocCollector();
        _searcher.Search(q, collector);
        IEnumerable<IFullTextSearchResult> results = from doc in collector.Documents
                                                     select _searcher.Doc(doc.Key).ToResult(doc.Value, _schema);
        return FilterByGraph(graphUris, results);
    }

    /// <summary>
    /// Searches for matches for specific text.
    /// </summary>
    /// <param name="graphUris">Graph URIs.</param>
    /// <param name="text">Search Query.</param>
    /// <param name="scoreThreshold">Score Threshold.</param>
    /// <param name="limit">Result Limit.</param>
    /// <returns></returns>
    public IEnumerable<IFullTextSearchResult> Match(IEnumerable<IRefNode> graphUris, string text, double scoreThreshold, int limit)
    {
        EnsureCurrent();
        LucSearch.Query q = _parser.Parse(text, _schema.IndexField);
        var collector = new DocCollector();
        _searcher.Search(q, collector);

        IEnumerable<IFullTextSearchResult> results = from doc in collector.Documents
            where doc.Value > scoreThreshold
            select _searcher.Doc(doc.Key).ToResult(doc.Value, _schema);
        return FilterByGraph(graphUris, results).Take(limit);
    }

    /// <summary>
    /// Searches for matches for specific text.
    /// </summary>
    /// <param name="graphUris">Graph URIs.</param>
    /// <param name="text">Search Query.</param>
    /// <param name="scoreThreshold">Score Threshold.</param>
    public IEnumerable<IFullTextSearchResult> Match(IEnumerable<IRefNode> graphUris, string text, double scoreThreshold)
    {
        EnsureCurrent();
        LucSearch.Query q = _parser.Parse(text, _schema.IndexField);
        var collector = new DocCollector(scoreThreshold);
        _searcher.Search(q, collector);
        IEnumerable<IFullTextSearchResult> results = from doc in collector.Documents
            select _searcher.Doc(doc.Key).ToResult(doc.Value, _schema);
        return FilterByGraph(graphUris, results);
    }

    /// <summary>
    /// Searches for matches for specific text.
    /// </summary>
    /// <param name="graphUris">Graph URIs.</param>
    /// <param name="text">Search Query.</param>
    /// <param name="limit">Result Limit.</param>
    public IEnumerable<IFullTextSearchResult> Match(IEnumerable<IRefNode> graphUris, string text, int limit)
    {
        EnsureCurrent();
        LucSearch.Query q = _parser.Parse(text, _schema.IndexField);
        var collector = new DocCollector();
        _searcher.Search(q, collector);

        IEnumerable<IFullTextSearchResult> results = from doc in collector.Documents
            select _searcher.Doc(doc.Key).ToResult(doc.Value, _schema);
        return FilterByGraph(graphUris, results).Take(limit);
    }

    /// <summary>
    /// Searches for matches for specific text.
    /// </summary>
    /// <param name="graphUris">Graph URIs.</param>
    /// <param name="text">Search Query.</param>
    public IEnumerable<IFullTextSearchResult> Match(IEnumerable<IRefNode> graphUris, string text)
    {
        EnsureCurrent();
        LucSearch.Query q = _parser.Parse(text, _schema.IndexField);
        var collector = new DocCollector();
        _searcher.Search(q, collector);
        IEnumerable<IFullTextSearchResult> results = from doc in collector.Documents
            select _searcher.Doc(doc.Key).ToResult(doc.Value, _schema);
        return FilterByGraph(graphUris, results);
    }

    /// <summary>
    /// Filters a set of results to ensure they occur in the given Graph(s).
    /// </summary>
    /// <param name="graphUris">Graph URIs.</param>
    /// <param name="results">Results.</param>
    /// <returns></returns>
    [Obsolete("Replaced by FilterByGraph(IEnumerable<IRefNode>, IEnumerable<IFullTextSearchResult>")]
    private IEnumerable<IFullTextSearchResult> FilterByGraph(IEnumerable<Uri> graphUris, IEnumerable<IFullTextSearchResult> results)
    {
        if (graphUris == null)
        {
            return results;
        }
        else
        {
            var uris = new HashSet<Uri>(graphUris, new UriComparer());
            return uris.Count == 0 ? results : results.Where(r => uris.Contains(r.GraphUri));
        }
    }

    private IEnumerable<IFullTextSearchResult> FilterByGraph(IEnumerable<IRefNode> graphNames,
        IEnumerable<IFullTextSearchResult> results)
    {
        if (graphNames == null) return results;
        var names = new HashSet<IRefNode>(graphNames, new FastNodeComparer());
        return names.Count == 0 ? results : results.Where(r => names.Contains(r.GraphName));
    }

    /// <summary>
    /// Gets whether this search provider is always seeing the latest state of the index.
    /// </summary>
    public bool IsAutoSynced { get; }

    /// <summary>
    /// Ensures that the Index Searcher is searching the current Index unless this feature has been disabled by the user.
    /// </summary>
    private void EnsureCurrent()
    {
        if (!IsAutoSynced)
        {
            return;
        }

        var newReader = DirectoryReader.OpenIfChanged(_indexReader);
        if (newReader != null)
        {
            _indexReader.Dispose();
            _indexReader = newReader;
            _searcher = new LucSearch.IndexSearcher(_indexReader);
        }
    }

    /// <summary>
    /// Disposes of the Search Provider.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }

    /// <summary>
    /// Disposes of the Search Provider.
    /// </summary>
    /// <param name="disposing">Whether this was called by the Dispose method.</param>
    private void Dispose(bool disposing)
    {
        if (disposing) GC.SuppressFinalize(this);

        DisposeInternal();

        //if (_searcher != null) _searcher.Dispose();
    }

    /// <summary>
    /// Virtual method that can be overridden to add implementation specific dispose logic.
    /// </summary>
    protected virtual void DisposeInternal() { }

    /// <summary>
    /// Serializes Configuration of this Provider.
    /// </summary>
    /// <param name="context">Serialization Context.</param>
    public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
    {
        context.EnsureObjectFactory(typeof(FullTextObjectFactory));

        INode searcherObj = context.NextSubject;
        INode rdfType = context.Graph.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfType));
        INode dnrType = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyType));
        INode searcherClass = context.Graph.CreateUriNode(context.UriFactory.Create(FullTextHelper.ClassSearcher));
        INode index = context.Graph.CreateUriNode(context.UriFactory.Create(FullTextHelper.PropertyIndex));
        INode schema = context.Graph.CreateUriNode(context.UriFactory.Create(FullTextHelper.PropertySchema));
        INode analyzer = context.Graph.CreateUriNode(context.UriFactory.Create(FullTextHelper.PropertyAnalyzer));
        INode indexSync = context.Graph.CreateUriNode(context.UriFactory.Create(FullTextHelper.PropertyIndexSync));

        //Basic Properties
        context.Graph.Assert(searcherObj, rdfType, searcherClass);
        context.Graph.Assert(searcherObj, dnrType, context.Graph.CreateLiteralNode(GetType().FullName + ", dotNetRDF.Query.FullText"));

        //Serialize and link the Index
        INode indexObj = context.Graph.CreateBlankNode();
        context.NextSubject = indexObj;
        _indexDir.SerializeConfiguration(context);
        context.Graph.Assert(searcherObj, index, indexObj);

        //Serialize and link the Schema
        INode schemaObj = context.Graph.CreateBlankNode();
        context.NextSubject = schemaObj;
        if (_schema is IConfigurationSerializable)
        {
            ((IConfigurationSerializable)_schema).SerializeConfiguration(context);
        }
        else
        {
            throw new DotNetRdfConfigurationException("Unable to serialize configuration for this Lucene Search Provider as the IFullTextSchema used does not implement the IConfigurationSerializable interface");
        }
        context.Graph.Assert(searcherObj, schema, schemaObj);

        //Serialize and link the Analyzer
        INode analyzerObj = context.Graph.CreateBlankNode();
        context.NextSubject = analyzerObj;
        _analyzer.SerializeConfiguration(context);
        context.Graph.Assert(searcherObj, index, analyzerObj);

        //Serialize auto-sync settings
        context.Graph.Assert(searcherObj, indexSync, IsAutoSynced.ToLiteral(context.Graph));
    }
}
