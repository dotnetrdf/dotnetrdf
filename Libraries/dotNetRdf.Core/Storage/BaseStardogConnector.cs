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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage.Management;
using VDS.RDF.Writing;

namespace VDS.RDF.Storage;

/// <summary>
/// Abstract implementation of a connector for Stardog that connects using the HTTP protocol.
/// </summary>
/// <remarks>
/// <para>
/// Has full support for Stardog Transactions, connection is in auto-commit mode by default i.e. all write operations (Delete/Save/Update) will create and use a dedicated transaction for their operation, if the operation fails the transaction will automatically be rolled back.  You can manage Transactions using the <see cref="BaseStardogConnector.Begin()">Begin()</see>, <see cref="BaseStardogConnector.Commit()">Commit()</see> and <see cref="BaseStardogConnector.Rollback()">Rollback()</see> methods.
/// </para>
/// <para>
/// The connector maintains a single transaction which is shared across all threads since Stardog is currently provides only MRSW (Multiple Reader Single Writer) concurrency and does not permit multiple transactions to occur simultaneously.  
/// </para>
/// </remarks>
public abstract class BaseStardogConnector
    : BaseAsyncHttpConnector, IAsyncQueryableStorage, IAsyncTransactionalStorage, IConfigurationSerializable
        , IQueryableStorage, ITransactionalStorage, IReasoningQueryableStorage
{
    /// <summary>
    /// Constant for the default Anonymous user account and password used by Stardog if you have not supplied a shiro.ini file or otherwise disabled security.
    /// </summary>
    public const string AnonymousUser = "anonymous";

    private readonly string _baseUri, _kb, _username, _pwd;
    private readonly bool _hasCredentials;
    private StardogReasoningMode _reasoning = StardogReasoningMode.None;

    private string _activeTrans;
    private readonly TriGWriter _writer = new TriGWriter();

    private bool? _isReady = null;

    /// <summary>
    /// The underlying server connection.
    /// </summary>
    protected BaseStardogServer Server;

    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    /// <param name="reasoning">Reasoning Mode.</param>
    protected BaseStardogConnector(string baseUri, string kbID, StardogReasoningMode reasoning)
        : this(baseUri, kbID, reasoning, null, null)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    protected BaseStardogConnector(string baseUri, string kbID)
        : this(baseUri, kbID, StardogReasoningMode.None)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    protected BaseStardogConnector(string baseUri, string kbID, string username, string password)
        : this(baseUri, kbID, StardogReasoningMode.None, username, password)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    /// <param name="reasoning">Reasoning Mode.</param>
    protected BaseStardogConnector(string baseUri, string kbID, StardogReasoningMode reasoning, string username,
        string password)
    {
        _baseUri = baseUri;
        if (!_baseUri.EndsWith("/")) _baseUri += "/";
        _kb = kbID;
        _reasoning = reasoning;

        // Prep the writer
        _writer.HighSpeedModePermitted = true;
        _writer.CompressionLevel = WriterCompressionLevel.None;
        _writer.UseMultiThreadedWriting = false;

        _username = username;
        _pwd = password;
        _hasCredentials = !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
        if (_hasCredentials) SetCredentials(username, password);

        // Server reference
        Server = new StardogV1Server(_baseUri, _username, _pwd);
    }

    /// <summary>
    /// Create a new connection to a Stardog store.
    /// </summary>
    /// <param name="baseUri">Base URI of the server.</param>
    /// <param name="kbId">Knowledge base ID.</param>
    /// <param name="reasoning">Reasoning mode.</param>
    /// <param name="httpClientHandler">Handler to configure outgoing HTTP requests.</param>
    protected BaseStardogConnector(string baseUri, string kbId, StardogReasoningMode reasoning,
        HttpClientHandler httpClientHandler) :
        base(httpClientHandler)
    {
        _baseUri = baseUri;
        if (!_baseUri.EndsWith("/")) _baseUri += "/";
        _kb = kbId;
        _reasoning = reasoning;

        // Prep the writer
        _writer.HighSpeedModePermitted = true;
        _writer.CompressionLevel = WriterCompressionLevel.None;
        _writer.UseMultiThreadedWriting = false;

        // Server reference
        Server = new StardogV1Server(_baseUri, httpClientHandler);
    }

    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    /// <param name="reasoning">Reasoning Mode.</param>
    /// <param name="proxy">Proxy Server.</param>
    protected BaseStardogConnector(string baseUri, string kbID, StardogReasoningMode reasoning, IWebProxy proxy)
        : this(baseUri, kbID, reasoning, null, null, proxy)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    /// <param name="reasoning">Reasoning Mode.</param>
    /// <param name="proxy">Proxy Server.</param>
    protected BaseStardogConnector(string baseUri, string kbID, StardogReasoningMode reasoning, string username,
        string password, IWebProxy proxy)
        : this(baseUri, kbID, reasoning, username, password)
    {
        Proxy = proxy;
    }

    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    /// <param name="proxy">Proxy Server.</param>
    protected BaseStardogConnector(string baseUri, string kbID, IWebProxy proxy)
        : this(baseUri, kbID, StardogReasoningMode.None, proxy)
    {
    }

    /// <summary>
    /// Creates a new connection to a Stardog Store.
    /// </summary>
    /// <param name="baseUri">Base Uri of the Server.</param>
    /// <param name="kbID">Knowledge Base (i.e. Database) ID.</param>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    /// <param name="proxy">Proxy Server.</param>
    protected BaseStardogConnector(string baseUri, string kbID, string username, string password, IWebProxy proxy)
        : this(baseUri, kbID, StardogReasoningMode.None, username, password, proxy)
    {
    }

    /// <summary>
    /// Gets the Base URI of the Stardog server.
    /// </summary>
    public string BaseUri => _baseUri;

    /// <summary>
    /// Gets the knowledge base ID being used by this connector.
    /// </summary>
    public string KbId => _kb;

    /// <summary>
    /// Gets/Sets the reasoning mode to use for queries.
    /// </summary>
    [Description("What reasoning mode (if any) is currently in use for SPARQL Queries")]
    public virtual StardogReasoningMode Reasoning
    {
        get => _reasoning;
        set => _reasoning = value;
    }

    /// <summary>
    /// Gets the IO Behaviour of Stardog.
    /// </summary>
    public override IOBehaviour IOBehaviour => IOBehaviour.GraphStore | IOBehaviour.CanUpdateTriples;

    /// <summary>
    /// Returns that listing Graphs is supported.
    /// </summary>
    public override bool ListGraphsSupported => true;

    /// <summary>
    /// Returns that the Connection is ready.
    /// </summary>
    /// <remarks>Returns true if the knowledge base specified in the class constructor is found on the server, false otherwise.</remarks>
    public override bool IsReady
    {
        get
        {
            if (!_isReady.HasValue)
            {
                _isReady = Server.ListStores().Contains(_kb);
            }
            return _isReady.Value;
        }
    }

    /// <summary>
    /// Returns that the Connection is not read-only.
    /// </summary>
    public override bool IsReadOnly => false;

    /// <summary>
    /// Returns that Updates are supported on Stardog Stores.
    /// </summary>
    public override bool UpdateSupported => true;

    /// <summary>
    /// Returns that deleting graphs from the Stardog store is not yet supported (due to a .Net specific issue).
    /// </summary>
    public override bool DeleteSupported => true;

    /// <summary>
    /// Gets the parent server.
    /// </summary>
    public override IStorageServer ParentServer => Server;

    /// <summary>
    /// Makes a SPARQL Query against the underlying Store using whatever reasoning mode is currently in-use.
    /// </summary>
    /// <param name="sparqlQuery">Sparql Query.</param>
    /// <returns></returns>
    public virtual object Query(string sparqlQuery)
    {
        var g = new Graph();
        var results = new SparqlResultSet();
        Query(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery);

        if (results.ResultsType != SparqlResultsType.Unknown)
        {
            return results;
        }
        else
        {
            return g;
        }
    }

    /// <summary>
    /// Makes a SPARQL Query against the underlying Store using whatever reasoning mode is currently in-use, the reasoning can be set by query.
    /// </summary>
    /// <param name="sparqlQuery">Sparql Query.</param>
    /// <param name="reasoning"></param>
    /// <returns></returns>
    public virtual object Query(string sparqlQuery, bool reasoning)
    {
        var g = new Graph();
        var results = new SparqlResultSet();
        Query(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery, reasoning);

        if (results.ResultsType != SparqlResultsType.Unknown)
        {
            return results;
        }
        else
        {
            return g;
        }
    }

    /// <summary>
    /// Makes a SPARQL Query against the underlying Store using whatever reasoning mode is currently in-use processing the results using an appropriate handler from those provided.
    /// </summary>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <returns></returns>
    public virtual void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
    {
        HttpRequestMessage request;

        var tID = (_activeTrans == null) ? string.Empty : "/" + _activeTrans;

        // String accept = MimeTypesHelper.HttpRdfOrSparqlAcceptHeader;
        var accept =
            MimeTypesHelper.CustomHttpAcceptHeader(
                MimeTypesHelper.SparqlResultsXml.Concat(
                    MimeTypesHelper.Definitions.Where(d => d.CanParseRdf).SelectMany(d => d.MimeTypes)));

        // Create the Request
        var queryParams = new Dictionary<string, string>();
        if (sparqlQuery.Length < 2048)
        {
            queryParams.Add("query", sparqlQuery);
            request = CreateRequest(_kb + tID + "/query", accept, HttpMethod.Get, queryParams);
        }
        else
        {
            request = CreateRequest(_kb + tID + "/query", accept, HttpMethod.Post, queryParams);

            // Build the Post Data and add to the Request Body
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("query", sparqlQuery),
            });
        }

        // Get the Response and process based on the Content Type
        using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
        if (!response.IsSuccessStatusCode)
        {
            throw StorageHelper.HandleHttpQueryError(response);
        }

        var data = new StreamReader(response.Content.ReadAsStreamAsync().Result);
        var ctype = response.Content.Headers.ContentType.MediaType;
        try
        {
            // Is the Content Type referring to a Sparql Result Set format?
            ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(ctype,
                Regex.IsMatch(sparqlQuery, "ASK", RegexOptions.IgnoreCase));
            resreader.Load(resultsHandler, data);
        }
        catch (RdfParserSelectionException)
        {
            // If we get a Parser Selection exception then the Content Type isn't valid for a Sparql Result Set

            // Is the Content Type referring to a RDF format?
            IRdfReader rdfreader = MimeTypesHelper.GetParser(ctype);
            rdfreader.Load(rdfHandler, data);
        }
    }


    /// <summary>
    /// Makes a SPARQL Query against the underlying Store using whatever reasoning mode is currently in-use processing the results using an appropriate handler from those provided, the reasoning can be set by query.
    /// </summary>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <param name="reasoning"></param>
    /// <returns></returns>
    public virtual void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery, bool reasoning)
    {
        try
        {
            HttpRequestMessage request;

            var transactionId = (_activeTrans == null) ? string.Empty : "/" + _activeTrans;

            // String accept = MimeTypesHelper.HttpRdfOrSparqlAcceptHeader;
            var accept =
                MimeTypesHelper.CustomHttpAcceptHeader(
                    MimeTypesHelper.SparqlResultsXml.Concat(
                        MimeTypesHelper.Definitions.Where(d => d.CanParseRdf).SelectMany(d => d.MimeTypes)));

            // Create the Request
            var queryParams = new Dictionary<string, string>();
            if (sparqlQuery.Length < 2048)
            {
                queryParams.Add("query", sparqlQuery);
                queryParams.Add("reasoning", reasoning.ToString());

                request = CreateRequest(_kb + transactionId + "/query", accept, HttpMethod.Get, queryParams);
            }
            else
            {
                request = CreateRequest(_kb + transactionId + "/query", accept, HttpMethod.Post, queryParams);

                // Build the Post Data and add to the Request Body
                var formData = new List<KeyValuePair<string, string>>();
                if (reasoning) formData.Add(new KeyValuePair<string, string>("reasoning", "true"));
                formData.Add(new KeyValuePair<string, string>("query", sparqlQuery));
                request.Content = new FormUrlEncodedContent(formData);
            }

            // Get the Response and process based on the Content Type
            using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
            var data = new StreamReader(response.Content.ReadAsStreamAsync().Result);
            var contentType = response.Content.Headers.ContentType.MediaType;
            try
            {
                // Is the Content Type referring to a Sparql Result Set format?
                ISparqlResultsReader sparqlResultsReader = MimeTypesHelper.GetSparqlParser(contentType,
                    Regex.IsMatch(sparqlQuery, "ASK", RegexOptions.IgnoreCase));
                sparqlResultsReader.Load(resultsHandler, data);
            }
            catch (RdfParserSelectionException)
            {
                // If we get a Parser Selection exception then the Content Type isn't valid for a Sparql Result Set

                // Is the Content Type referring to a RDF format?
                IRdfReader rdfReader = MimeTypesHelper.GetParser(contentType);
                rdfReader.Load(rdfHandler, data);
            }
        }
        catch (WebException webEx)
        {
            throw StorageHelper.HandleHttpQueryError(webEx);
        }
    }

    /// <summary>
    /// Loads a Graph from the Store.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    /// <remarks>
    /// If an empty/null URI is specified then the Default Graph of the Store will be loaded.
    /// </remarks>
    public virtual void LoadGraph(IGraph g, Uri graphUri)
    {
        LoadGraph(g, graphUri.ToSafeString());
    }

    /// <summary>
    /// Loads a Graph from the Store.
    /// </summary>
    /// <param name="handler">RDF Handler.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    /// <remarks>
    /// If an empty/null URI is specified then the Default Graph of the Store will be loaded.
    /// </remarks>
    public virtual void LoadGraph(IRdfHandler handler, Uri graphUri)
    {
        LoadGraph(handler, graphUri.ToSafeString());
    }

    /// <summary>
    /// Loads a Graph from the Store.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="graphUri">Uri of the Graph to load.</param>
    /// <remarks>
    /// If an empty/null Uri is specified then the Default Graph of the Store will be loaded.
    /// </remarks>
    public virtual void LoadGraph(IGraph g, string graphUri)
    {
        if (g.IsEmpty && graphUri != null && !graphUri.Equals(string.Empty))
        {
            g.BaseUri = UriFactory.Create(graphUri);
        }
        LoadGraph(new GraphHandler(g), graphUri);
    }

    /// <summary>
    /// Loads a Graph from the Store.
    /// </summary>
    /// <param name="handler">RDF Handler.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    /// <remarks>
    /// If an empty/null URI is specified then the Default Graph of the Store will be loaded.
    /// </remarks>
    public virtual void LoadGraph(IRdfHandler handler, string graphUri)
    {
        var serviceParams = new Dictionary<string, string>();

        var transactionId = (_activeTrans == null) ? string.Empty : "/" + _activeTrans;
        var requestUri = _kb + transactionId + "/query";
        var construct = string.IsNullOrEmpty(graphUri)
            ? "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }"
            : $"CONSTRUCT {{ ?s ?p ?o }} WHERE {{ GRAPH <{graphUri}> {{ ?s ?p ?o }} }}";

        serviceParams.Add("query", construct);

        HttpRequestMessage request = CreateRequest(requestUri, MimeTypesHelper.HttpAcceptHeader, HttpMethod.Get,
            serviceParams);

        using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
        if (!response.IsSuccessStatusCode)
            throw StorageHelper.HandleHttpError(response, "loading a Graph from");
        IRdfReader parser = MimeTypesHelper.GetParser(response.Content.Headers.ContentType.MediaType);
        parser.Load(handler, new StreamReader(response.Content.ReadAsStreamAsync().Result));
    }

    /// <summary>
    /// Saves a Graph into the Store (see remarks for notes on merge/overwrite behaviour).
    /// </summary>
    /// <param name="g">Graph to save.</param>
    /// <remarks>
    /// <para>
    /// If the Graph has no URI then the contents will be appended to the Store's Default Graph.  If the Graph has a URI then existing Graph associated with that URI will be replaced.  To append to a named Graph use the <see cref="BaseStardogConnector.UpdateGraph(Uri,System.Collections.Generic.IEnumerable{VDS.RDF.Triple},IEnumerable{Triple})">UpdateGraph()</see> method instead.
    /// </para>
    /// </remarks>
    public virtual void SaveGraph(IGraph g)
    {
        string transactionId = null;
        try
        {
            // Have to do the delete first as that requires a separate transaction
            if (g.Name != null)
            {
                try
                {
                    DeleteGraph(g.Name.ToString());
                }
                catch (Exception ex)
                {
                    throw new RdfStorageException(
                        "Unable to save a Named Graph to the Store as this requires deleting any existing Named Graph with this name which failed, see inner exception for more detail",
                        ex);
                }
            }

            // Get a Transaction ID, if there is no active Transaction then this operation will be auto-committed
            transactionId = _activeTrans ?? BeginTransaction();

            HttpRequestMessage request = CreateRequest(_kb + "/" + transactionId + "/add", MimeTypesHelper.Any,
                HttpMethod.Post, new Dictionary<string, string>());
            request.Content = new DatasetContent(g, _writer);

            using (HttpResponseMessage response = HttpClient.SendAsync(request).Result)
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw StorageHelper.HandleHttpError(response, "saving a Graph to");
                }
            }

            // Commit Transaction only if in auto-commit mode (active transaction will be null)
            if (_activeTrans != null) return;
            try
            {
                CommitTransaction(transactionId);
            }
            catch (Exception ex)
            {
                throw new RdfStorageException("Stardog failed to commit a Transaction", ex);
            }
        }
        catch (RdfStorageException)
        {
            // Rollback Transaction only if got as far as creating a Transaction
            // and in auto-commit mode
            if (transactionId != null)
            {
                if (_activeTrans == null)
                {
                    try
                    {
                        RollbackTransaction(transactionId);
                    }
                    catch (Exception ex)
                    {
                        throw new RdfStorageException("Stardog failed to rollback a Transaction", ex);
                    }
                }
            }

            throw;
        }
    }

    /// <inheritdoc />
    public void UpdateGraph(IRefNode graphName, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
    {
        if (graphName is IUriNode u)
        {
            UpdateGraph(u.Uri, additions, removals);
        }
        else
        {
            throw new RdfStorageException("Updating a blank-node named graph is not supported.");
        }
    }

    /// <summary>
    /// Updates a Graph in the Stardog Store.
    /// </summary>
    /// <param name="graphUri">Uri of the Graph to update.</param>
    /// <param name="additions">Triples to be added.</param>
    /// <param name="removals">Triples to be removed.</param>
    /// <remarks>
    /// Removals happen before additions.
    /// </remarks>
    public virtual void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
    {
        // If there are no adds or deletes, just return and avoid creating empty transaction
        var anyData = (removals != null && removals.Any()) || (additions != null && additions.Any());
        if (!anyData) return;

        string tID = null;
        try
        {
            // Get a Transaction ID, if there is no active Transaction then this operation will be auto-committed
            tID = _activeTrans ?? BeginTransaction();

            // First do the Removals
            if (removals != null)
            {
                if (removals.Any())
                {
                    HttpRequestMessage request = CreateRequest(_kb + "/" + tID + "/remove",
                        MimeTypesHelper.Any, HttpMethod.Post, new Dictionary<string, string>());

                    // Save the Data to be removed as TriG to the Request Stream
                    var g = new Graph(graphUri);
                    g.Assert(removals);
                    request.Content = new DatasetContent(g, _writer);
                    using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
                    if (!response.IsSuccessStatusCode) throw StorageHelper.HandleHttpError(response, "updating a Graph in");
                }
            }

            // Then do the Additions
            if (additions != null)
            {
                if (additions.Any())
                {
                    HttpRequestMessage request = CreateRequest(_kb + "/" + tID + "/add", MimeTypesHelper.Any,
                        HttpMethod.Post, new Dictionary<string, string>());

                    // Save the Data to be added as TriG to the Request Stream
                    var g = new Graph(graphUri);
                    g.Assert(additions);
                    request.Content = new DatasetContent(g, _writer);

                    using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
                    if (!response.IsSuccessStatusCode) throw StorageHelper.HandleHttpError(response, "updating a Graph in");
                }
            }

            // Commit Transaction only if in auto-commit mode (active transaction will be null)
            if (_activeTrans != null) return;
            try
            {
                CommitTransaction(tID);
            }
            catch (Exception ex)
            {
                throw new RdfStorageException("Stardog failed to commit a Transaction", ex);
            }
        }
        catch (RdfStorageException)
        {
            // Rollback Transaction only if got as far as creating a Transaction
            // and in auto-commit mode
            if (tID != null)
            {
                if (_activeTrans == null)
                {
                    try
                    {
                        RollbackTransaction(tID);
                    }
                    catch (Exception ex)
                    {
                        throw new RdfStorageException("Stardog failed to rollback a Transaction", ex);
                    }
                }
            }

            throw;
        }
    }


    /// <summary>
    /// Updates a Graph in the Stardog store.
    /// </summary>
    /// <param name="graphUri">Uri of the Graph to update.</param>
    /// <param name="additions">Triples to be added.</param>
    /// <param name="removals">Triples to be removed.</param>
    public virtual void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
    {
        if (graphUri == null || graphUri.Equals(string.Empty))
        {
            UpdateGraph((Uri)null, additions, removals);
        }
        else
        {
            UpdateGraph(UriFactory.Create(graphUri), additions, removals);
        }
    }

    /// <summary>
    /// Deletes a Graph from the Stardog store.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to delete.</param>
    public virtual void DeleteGraph(Uri graphUri)
    {
        DeleteGraph(graphUri.ToSafeString());
    }

    /// <summary>
    /// Deletes a Graph from the Stardog store.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to delete.</param>
    public virtual void DeleteGraph(string graphUri)
    {
        string tID = null;
        try
        {
            // Get a Transaction ID, if there is no active Transaction then this operation will be auto-committed
            tID = _activeTrans ?? BeginTransaction();

            HttpRequestMessage request = CreateRequest(
                _kb + "/" + tID + "/clear/",
                MimeTypesHelper.Any,
                HttpMethod.Post, 
                new Dictionary<string, string>
                {
                    {"graph-uri", graphUri.Equals(string.Empty) ? "DEFAULT" : graphUri},
                }
            );
            request.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[0]);

            using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
            if (!response.IsSuccessStatusCode)
                throw StorageHelper.HandleHttpError(response, "deleting a Graph from");
            // If we get here then the Delete worked OK
            // Commit Transaction only if in auto-commit mode (active transaction will be null)
            if (_activeTrans != null) return;
            try
            {
                CommitTransaction(tID);
            }
            catch (Exception ex)
            {
                throw new RdfStorageException("Stardog failed to commit a Transaction", ex);
            }
        }
        catch (RdfStorageException)
        {
            // Rollback Transaction only if got as far as creating a Transaction
            // and in auto-commit mode
            if (tID != null)
            {
                if (_activeTrans == null)
                {
                    try
                    {
                        RollbackTransaction(tID);
                    }
                    catch (Exception ex)
                    {
                        throw new RdfStorageException("Stardog failed to rollback a Transaction", ex);
                    }
                }
            }
            throw;
        }
    }

    /// <summary>
    /// Gets the list of Graphs in the Stardog store.
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerable<Uri> ListGraphs()
    {
        try
        {
            var results = Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }");
            if (results is SparqlResultSet resultSet)
            {
                var graphs = new List<Uri>();
                foreach (SparqlResult r in resultSet)
                {
                    if (r.HasValue("g"))
                    {
                        INode temp = r["g"];
                        if (temp.NodeType == NodeType.Uri)
                        {
                            graphs.Add(((IUriNode)temp).Uri);
                        }
                    }
                }
                return graphs;
            }
            else
            {
                return Enumerable.Empty<Uri>();
            }
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "listing Graphs from");
        }
    }

    /// <summary>
    /// Gets an enumeration of the names of the graphs in the store.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// Implementations should implement this method only if they need to provide a custom way of listing Graphs.  If the Store for which you are providing a manager can efficiently return the Graphs using a SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } } query then there should be no need to implement this function.
    /// </para>
    /// </remarks>
    public virtual IEnumerable<string> ListGraphNames()
    {
        try
        {
            var results = Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }");
            if (results is SparqlResultSet resultSet)
            {
                var graphs = new List<string>();
                foreach (SparqlResult r in resultSet)
                {
                    if (r.HasValue("g"))
                    {
                        INode temp = r["g"];
                        if (temp.NodeType == NodeType.Uri)
                        {
                            graphs.Add(((IUriNode)temp).Uri.AbsoluteUri);
                        } else if (temp.NodeType == NodeType.Blank)
                        {
                            graphs.Add("_:" + ((IBlankNode)temp).InternalID);
                        }
                    }
                }
                return graphs;
            }

            return Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "listing Graphs from");
        }
    }

    /// <summary>
    /// Gets the parent server.
    /// </summary>
    public override IAsyncStorageServer AsyncParentServer => Server;

    /// <summary>
    /// Saves a Graph to the Store asynchronously.
    /// </summary>
    /// <param name="g">Graph to save.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public override void SaveGraph(IGraph g, AsyncStorageCallback callback, object state)
    {
        SaveGraphAsync(g, callback, state);
    }

    /// <inheritdoc />
    public override async Task SaveGraphAsync(IGraph g, CancellationToken cancellationToken)
    {
        if (_activeTrans != null)
        {
            await SaveGraphAsync(_activeTrans, false, g, cancellationToken);
        }
        else
        {
            await BeginAsync(cancellationToken);
            if (g.BaseUri != null)
            {
                await DeleteGraphAsync(g.BaseUri.AbsoluteUri, cancellationToken);
            }
            await SaveGraphAsync(_activeTrans, true, g, cancellationToken);
        }
    }

    /// <summary>
    /// Saves a Graph to the Store asynchronously.
    /// </summary>
    /// <param name="g">Graph to save.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    protected virtual void SaveGraphAsync(IGraph g, AsyncStorageCallback callback, object state)
    {
        // Get a Transaction ID, if there is no active Transaction then this operation will start a new transaction and be auto-committed
        if (_activeTrans != null)
        {
            SaveGraphAsync(_activeTrans, false, g, callback, state);
        }
        else
        {
            Begin((sender, args, st) =>
            {
                if (args.WasSuccessful)
                {
                    // Have to do the delete first as that requires a separate transaction
                    if (g.BaseUri != null)
                    {
                        DeleteGraph(g.BaseUri, (_1, delArgs, _2) =>
                        {
                            if (delArgs.WasSuccessful)
                            {
                                SaveGraphAsync(_activeTrans, true, g, callback, state);
                            }
                            else
                            {
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph,
                                        new RdfStorageException(
                                            "Unable to save a Named Graph to the Store as this requires deleted any existing Named Graph with this name which failed, see inner exception for more detail",
                                            delArgs.Error)), state);
                            }
                        }, state);
                    }
                    else
                    {
                        SaveGraphAsync(_activeTrans, true, g, callback, state);
                    }
                }
                else
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g, args.Error),
                        state);
                }
            }, state);
        }
    }

    /// <summary>
    /// Save a graph to the database asynchronously within the context of an open transaction.
    /// </summary>
    /// <param name="tID">The ID of the transaction to use for the update.</param>
    /// <param name="autoCommit">True to commit the transaction on completion.</param>
    /// <param name="g">The graph to write.</param>
    /// <param name="callback">Callback invoked on completion.</param>
    /// <param name="state">State parameter to pass to the callback.</param>
    protected virtual void SaveGraphAsync(string tID, bool autoCommit, IGraph g, AsyncStorageCallback callback,
        object state)
    {
        HttpRequestMessage request = MakeSaveGraphRequestMessage(tID, g);
        HttpClient.SendAsync(request).ContinueWith(requestTask =>
        {
            if (requestTask.IsCanceled)
            {
                if (autoCommit)
                {
                    // If something went wrong try to rollback, don't care what the rollback response is
                    Rollback((sender, args, st) => { }, state);
                }

                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph,
                        new RdfStorageException("Operation was cancelled.")),
                    state);
            }
            else if (requestTask.IsFaulted)
            {
                if (autoCommit)
                {
                    // If something went wrong try to rollback, don't care what the rollback response is
                    Rollback((sender, args, st) => { }, state);
                }

                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph,
                        StorageHelper.HandleError(requestTask.Exception, "saving a Graph asynchronously to")),
                    state);
            }
            else
            {
                HttpResponseMessage response = requestTask.Result;
                if (response.IsSuccessStatusCode)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g),
                        state);
                }
                else
                {
                    if (autoCommit)
                    {
                        // If something went wrong try to rollback, don't care what the rollback response is
                        Rollback((sender, args, st) => { }, state);
                    }

                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph,
                            StorageHelper.HandleHttpError(response, "saving a Graph asynchronously to")),
                        state);
                }
            }

        });
    }

    private HttpRequestMessage MakeSaveGraphRequestMessage(string tID, IGraph g)
    {
        HttpRequestMessage request = CreateRequest(_kb + "/" + tID + "/add", MimeTypesHelper.Any, HttpMethod.Post,
            new Dictionary<string, string>());
        request.Content = new DatasetContent(g, _writer);
        return request;
    }

    /// <summary>
    /// Save a graph asynchronously within the scope of an open transaction.
    /// </summary>
    /// <param name="transactionId">The ID of the transaction.</param>
    /// <param name="autoCommit">Whether to commit/rollback the transaction when the operation is completed.</param>
    /// <param name="g">The graph to save.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="RdfStorageException">Raised if the Stardog server responds with an error status code.</exception>
    protected virtual async Task SaveGraphAsync(string transactionId, bool autoCommit, IGraph g,
        CancellationToken cancellationToken)
    {
        HttpRequestMessage request = MakeSaveGraphRequestMessage(transactionId, g);
        try
        {
            HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, "saving a graph asynchronously to");
            }

            if (autoCommit)
            {
                await CommitAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            if (autoCommit && _activeTrans != null)
            {
                await RollbackAsync(cancellationToken);
            }

            throw ex switch
            {
                RdfStorageException storageException => storageException,
                OperationCanceledException canceledException => canceledException,
                _ => StorageHelper.HandleError(ex, "saving a graph asynchronously to")
            };
        }
    }

    /// <summary>
    /// Loads a Graph from the Store asynchronously.
    /// </summary>
    /// <param name="handler">Handler to load with.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public override void LoadGraph(IRdfHandler handler, string graphUri, AsyncStorageCallback callback,
        object state)
    {
        HttpRequestMessage request = MakeLoadGraphRequestMessage(graphUri);
        HttpClient.SendAsync(request).ContinueWith(requestTask =>
        {
            if (requestTask.IsCanceled)
            {
                callback(this, new AsyncStorageCallbackArgs(
                    AsyncStorageOperation.LoadWithHandler,
                    new RdfStorageException("Operation was cancelled.")), state);
            }
            else if (requestTask.IsFaulted)
            {
                callback(this, new AsyncStorageCallbackArgs(
                        AsyncStorageOperation.LoadWithHandler,
                        StorageHelper.HandleError(requestTask.Exception, "loading a Graph from")),
                    state);
            }
            else
            {
                HttpResponseMessage response = requestTask.Result;
                if (response.IsSuccessStatusCode)
                {
                    response.Content.ReadAsStreamAsync().ContinueWith(contentTask =>
                    {
                        if (contentTask.IsCanceled)
                        {
                            callback(this, new AsyncStorageCallbackArgs(
                                AsyncStorageOperation.LoadWithHandler,
                                new RdfStorageException("Operation was cancelled.")), state);
                        }
                        else if (contentTask.IsFaulted)
                        {
                            callback(this, new AsyncStorageCallbackArgs(
                                    AsyncStorageOperation.LoadWithHandler,
                                    StorageHelper.HandleError(contentTask.Exception, "loading a Graph from")),
                                state);
                        }
                        else
                        {
                            try
                            {
                                IRdfReader parser =
                                    MimeTypesHelper.GetParser(
                                        response.Content.Headers.ContentType.MediaType);
                                parser.Load(handler, new StreamReader(contentTask.Result));
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler,
                                        handler),
                                    state);
                            }
                            catch (Exception ex)
                            {
                                callback(this, new AsyncStorageCallbackArgs(
                                    AsyncStorageOperation.LoadWithHandler,
                                    new RdfStorageException("Error parsing content received from server.",
                                        ex)
                                ), state);
                            }
                        }
                    });
                }
            }
        });
    }

    private HttpRequestMessage MakeLoadGraphRequestMessage(string graphUri)
    {
        var serviceParams = new Dictionary<string, string>();

        var tID = (_activeTrans == null) ? string.Empty : "/" + _activeTrans;
        var requestUri = _kb + tID + "/query";
        var construct = string.IsNullOrEmpty(graphUri)
            ? "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }"
            : $"CONSTRUCT {{ ?s ?p ?o }} WHERE {{ GRAPH <{graphUri}> {{ ?s ?p ?o }} }}";

        serviceParams.Add("query", construct);

        HttpRequestMessage request = CreateRequest(requestUri, MimeTypesHelper.HttpAcceptHeader, HttpMethod.Get,
            serviceParams);
        return request;
    }

    /// <inheritdoc />
    public override async Task LoadGraphAsync(IRdfHandler handler, string graphName, CancellationToken cancellationToken)
    {
        try
        {
            HttpRequestMessage request = MakeLoadGraphRequestMessage(graphName);
            HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, "loading a graph from");
            }

            IRdfReader parser = MimeTypesHelper.GetParser(response.Content.Headers.ContentType.MediaType);
            parser.Load(handler, new StreamReader(await response.Content.ReadAsStreamAsync()));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (RdfStorageException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "loading a graph from");
        }
    }

    /// <summary>
    /// Updates a Graph in the Store asynchronously.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to update.</param>
    /// <param name="additions">Triples to be added.</param>
    /// <param name="removals">Triples to be removed.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public override void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals,
        AsyncStorageCallback callback, object state)
    {
        // If there are no adds or deletes, just callback and avoid creating empty transaction
        var anyData = removals != null && removals.Any();
        if (additions != null && additions.Any()) anyData = true;
        if (!anyData)
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()),
                state);
        }
        else
        {
            UpdateGraphAsync(graphUri, additions, removals, callback, state);
        }
    }

    /// <summary>
    /// Apply an update to a graph.
    /// </summary>
    /// <param name="graphUri">The URI of the graph to be updated.</param>
    /// <param name="additions">The triples to insert.</param>
    /// <param name="removals">The triples to delete.</param>
    /// <param name="callback">Callback invoked on completion.</param>
    /// <param name="state">Additional state passed to the callback.</param>
    /// <remarks>If a transaction is currently in progress, the update is applied
    /// as part of that transaction. Otherwise a new transaction is started and committed by this method.</remarks>
    protected virtual void UpdateGraphAsync(string graphUri, IEnumerable<Triple> additions,
        IEnumerable<Triple> removals, AsyncStorageCallback callback, object state)
    {
        // Get a Transaction ID, if there is no active Transaction then this operation will start a new transaction and be auto-committed
        if (_activeTrans != null)
        {
            UpdateGraphAsync(_activeTrans, false, graphUri, additions, removals, callback, state);
        }
        else
        {
            Begin((sender, args, st) =>
            {
                if (args.WasSuccessful)
                {
                    UpdateGraphAsync(_activeTrans, true, graphUri, additions, removals, callback, state);
                }
                else
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(),
                            args.Error), state);
                }
            }, state);
        }
    }

    /// <inheritdoc />
    public override async Task UpdateGraphAsync(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals,
        CancellationToken cancellationToken)
    {
        if (_activeTrans != null)
        {
            await UpdateGraphAsync(_activeTrans, false, graphUri, additions, removals, cancellationToken);
        }
        else
        {
            await BeginAsync(cancellationToken);
            await UpdateGraphAsync(_activeTrans, true, graphUri, additions, removals, cancellationToken);
        }
    }

    /// <summary>
    /// Apply an update to a graph as part of a transaction.
    /// </summary>
    /// <param name="tID">The ID of the open transaction to use.</param>
    /// <param name="autoCommit">True to commit the transaction at the end of the update, false otherwise.</param>
    /// <param name="graphUri">The URI of the graph to be updated.</param>
    /// <param name="additions">The triples to inser.</param>
    /// <param name="removals">The triples to remove.</param>
    /// <param name="callback">A callback to be invoked on completion.</param>
    /// <param name="state">Additional state to pass to the callback.</param>
    protected virtual void UpdateGraphAsync(string tID, bool autoCommit, string graphUri,
        IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, object state)
    {
        if (removals != null && removals.Any())
        {
            HttpRequestMessage request = MakeRemoveTriplesRequestMessage(tID, graphUri, removals);
            HttpClient.SendAsync(request).ContinueWith(removalRequestTask =>
            {
                if (removalRequestTask.IsCanceled)
                {
                    if (autoCommit)
                    {
                        // If something went wrong try to rollback, don't care what the rollback response is
                        Rollback((sender, args, st) => { }, state);
                    }

                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(),
                            new RdfStorageException("Operation was cancelled.")), state);
                }
                else if (removalRequestTask.IsFaulted)
                {
                    if (autoCommit)
                    {
                        // If something went wrong try to rollback, don't care what the rollback response is
                        Rollback((sender, args, st) => { }, state);
                    }

                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(),
                            StorageHelper.HandleError(removalRequestTask.Exception,
                                "updating a Graph asynchronously in")), state);
                }
                else
                {
                    HttpResponseMessage removalResponse = removalRequestTask.Result;
                    if (!removalResponse.IsSuccessStatusCode)
                    {
                        if (autoCommit)
                        {
                            // If something went wrong try to rollback, don't care what the rollback response is
                            Rollback((sender, args, st) => { }, state);
                        }

                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                graphUri.ToSafeUri(),
                                StorageHelper.HandleHttpError(removalResponse,
                                    "updating a Graph asynchronously in")), state);
                    }
                    else 
                    {
                        if (additions == null || !additions.Any())
                        {
                            // No additions to apply - commit if necessary and return success
                            if (autoCommit)
                            {
                                Commit((sender, args, st) =>
                                {
                                    if (args.WasSuccessful)
                                    {
                                        callback(this,
                                            new AsyncStorageCallbackArgs(
                                                AsyncStorageOperation.UpdateGraph,
                                                graphUri.ToSafeUri()), state);
                                    }
                                    else
                                    {
                                        callback(this,
                                            new AsyncStorageCallbackArgs(
                                                AsyncStorageOperation.UpdateGraph,
                                                graphUri.ToSafeUri(), args.Error), state);
                                    }
                                }, state);
                            }
                            else
                            {
                                callback(this,
                                    new AsyncStorageCallbackArgs(
                                        AsyncStorageOperation.UpdateGraph,
                                        graphUri.ToSafeUri()), state);
                            }

                            return;
                        }

                        // Apply additions
                        HttpRequestMessage addRequest = MakeAddTriplesRequestMessage(tID, graphUri, additions);
                        HttpClient.SendAsync(addRequest).ContinueWith(addRequestTask =>
                        {
                            if (addRequestTask.IsCanceled)
                            {
                                if (autoCommit)
                                {
                                    // If something went wrong try to rollback, don't care what the rollback response is
                                    Rollback((sender, args, st) => { }, state);
                                }

                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                        graphUri.ToSafeUri(),
                                        new RdfStorageException("Operation was cancelled.")), state);
                            }
                            else if (addRequestTask.IsFaulted)
                            {
                                if (autoCommit)
                                {
                                    // If something went wrong try to rollback, don't care what the rollback response is
                                    Rollback((sender, args, st) => { }, state);
                                }

                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                        graphUri.ToSafeUri(),
                                        StorageHelper.HandleError(addRequestTask.Exception,
                                            "updating a Graph asynchronously in")), state);
                            }
                            else
                            {
                                HttpResponseMessage addResponse = addRequestTask.Result;
                                if (!addResponse.IsSuccessStatusCode)
                                {
                                    if (autoCommit)
                                    {
                                        // If something went wrong try to rollback, don't care what the rollback response is
                                        Rollback((sender, args, st) => { }, state);
                                    }

                                    callback(this,
                                        new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                            graphUri.ToSafeUri(),
                                            StorageHelper.HandleHttpError(addResponse,
                                                "updating a Graph asynchronously in")), state);
                                }
                                else
                                {
                                    if (autoCommit)
                                    {
                                        Commit((sender, args, st) =>
                                        {
                                            if (args.WasSuccessful)
                                            {
                                                callback(this,
                                                    new AsyncStorageCallbackArgs(
                                                        AsyncStorageOperation.UpdateGraph,
                                                        graphUri.ToSafeUri()), state);
                                            }
                                            else
                                            {
                                                callback(this,
                                                    new AsyncStorageCallbackArgs(
                                                        AsyncStorageOperation.UpdateGraph,
                                                        graphUri.ToSafeUri(), args.Error), state);
                                            }
                                        }, state);
                                    }
                                    else
                                    {
                                        callback(this,
                                            new AsyncStorageCallbackArgs(
                                                AsyncStorageOperation.UpdateGraph,
                                                graphUri.ToSafeUri()), state);
                                    }
                                }
                            }
                        });
                    }
                }
            }).ConfigureAwait(true);
        }
        else if (additions != null && additions.Any())
        {
            HttpRequestMessage addRequest = MakeAddTriplesRequestMessage(tID, graphUri, additions);
            HttpClient.SendAsync(addRequest).ContinueWith(addRequestTask =>
            {
                if (addRequestTask.IsCanceled)
                {
                    if (autoCommit)
                    {
                        // If something went wrong try to rollback, don't care what the rollback response is
                        Rollback((sender, args, st) => { }, state);
                    }

                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                            graphUri.ToSafeUri(),
                            new RdfStorageException("Operation was cancelled.")), state);
                }
                else if (addRequestTask.IsFaulted)
                {
                    if (autoCommit)
                    {
                        // If something went wrong try to rollback, don't care what the rollback response is
                        Rollback((sender, args, st) => { }, state);
                    }

                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                            graphUri.ToSafeUri(),
                            StorageHelper.HandleError(addRequestTask.Exception,
                                "updating a Graph asynchronously in")), state);
                }
                else
                {
                    HttpResponseMessage addResponse = addRequestTask.Result;
                    if (!addResponse.IsSuccessStatusCode)
                    {
                        if (autoCommit)
                        {
                            // If something went wrong try to rollback, don't care what the rollback response is
                            Rollback((sender, args, st) => { }, state);
                        }

                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                graphUri.ToSafeUri(),
                                StorageHelper.HandleHttpError(addResponse,
                                    "updating a Graph asynchronously in")), state);
                    }
                    else
                    {
                        if (autoCommit)
                        {
                            Commit((sender, args, st) =>
                            {
                                if (args.WasSuccessful)
                                {
                                    callback(this,
                                        new AsyncStorageCallbackArgs(
                                            AsyncStorageOperation.UpdateGraph,
                                            graphUri.ToSafeUri()), state);
                                }
                                else
                                {
                                    callback(this,
                                        new AsyncStorageCallbackArgs(
                                            AsyncStorageOperation.UpdateGraph,
                                            graphUri.ToSafeUri(), args.Error), state);
                                }
                            }, state);
                        }
                        else
                        {
                            callback(this,
                                new AsyncStorageCallbackArgs(
                                    AsyncStorageOperation.UpdateGraph,
                                    graphUri.ToSafeUri()), state);
                        }
                    }
                }
            }).ConfigureAwait(true);
        }
        else
        {
            // Nothing to do, just invoke callback
            callback(this,
                new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()),
                state);
        }
    }

    /// <summary>
    /// Helper method to construct an HTTP request to add triples to a graph.
    /// </summary>
    /// <param name="transactionId">The ID of the transaction to use for the update.</param>
    /// <param name="graphName">The name of the graph to be updated.</param>
    /// <param name="additions">The triples to add to the graph.</param>
    /// <returns></returns>
    protected virtual HttpRequestMessage MakeAddTriplesRequestMessage(string transactionId, string graphName, IEnumerable<Triple> additions)
    {
        HttpRequestMessage addRequest = CreateRequest(_kb + "/" + transactionId + "/add",
            MimeTypesHelper.Any,
            HttpMethod.Post, new Dictionary<string, string>());
        var g = new Graph(graphName.ToSafeUri());
        g.Assert(additions);
        addRequest.Content = new DatasetContent(g, _writer);
        return addRequest;
    }

    /// <summary>
    /// Helper method to construct an HTTP request to remove triples from a graph.
    /// </summary>
    /// <param name="transactionId">The ID of the transaction to use for the update.</param>
    /// <param name="graphName">The name of the graph to be updated.</param>
    /// <param name="removals">The triples to remove from the graph.</param>
    /// <returns></returns>
    protected virtual HttpRequestMessage MakeRemoveTriplesRequestMessage(string transactionId, string graphName, IEnumerable<Triple> removals)
    {
        HttpRequestMessage request = CreateRequest(_kb + "/" + transactionId + "/remove", MimeTypesHelper.Any,
            HttpMethod.Post, new Dictionary<string, string>());
        var g = new Graph(graphName.ToSafeUri());
        g.Assert(removals);
        request.Content = new DatasetContent(g, _writer);
        return request;
    }


    /// <summary>
    /// Update a graph asynchronously within the context of a transaction.
    /// </summary>
    /// <param name="transactionId">The transaction to use.</param>
    /// <param name="autoCommit">Whether to automatically commit/rollback the transaction.</param>
    /// <param name="graphUri">The name of the graph to be updated.</param>
    /// <param name="additions">The triples to add to the graph.</param>
    /// <param name="removals">The triples to remove from the graph.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="RdfStorageException"></exception>
    protected virtual async Task UpdateGraphAsync(string transactionId, bool autoCommit, string graphUri,
        IEnumerable<Triple> additions, IEnumerable<Triple> removals, CancellationToken cancellationToken)
    {
        try
        {
            if (removals != null && removals.Any())
            {
                HttpRequestMessage request = MakeRemoveTriplesRequestMessage(transactionId, graphUri, removals);
                HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    throw StorageHelper.HandleHttpError(response, "updating a graph in");
                }

                if (additions != null && additions.Any())
                {
                    request = MakeAddTriplesRequestMessage(transactionId, graphUri, additions);
                    response = await HttpClient.SendAsync(request, cancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw StorageHelper.HandleHttpError(response, "updating a graph in");
                    }
                }

                if (autoCommit)
                {
                    await CommitAsync(cancellationToken);
                }
            }
            else if (additions != null && additions.Any())
            {
                HttpRequestMessage request = MakeAddTriplesRequestMessage(transactionId, graphUri, additions);
                HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    throw StorageHelper.HandleHttpError(response, "updating a graph in");
                }

                if (autoCommit)
                {
                    await CommitAsync(cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            if (autoCommit)
            {
                await RollbackAsync(cancellationToken);
            }

            throw ex switch
            {
                OperationCanceledException canceledException => canceledException,
                RdfStorageException storageException => storageException,
                _ => StorageHelper.HandleError(ex, "updating a graph in")
            };
        }
    }

    /// <summary>
    /// Deletes a Graph from the Store.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to delete.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public override void DeleteGraph(string graphUri, AsyncStorageCallback callback, object state)
    {
        // Get a Transaction ID, if there is no active Transaction then this operation will start a new transaction and be auto-committed
        if (_activeTrans != null)
        {
            DeleteGraphAsync(_activeTrans, false, graphUri, callback, state);
        }
        else
        {
            Begin((sender, args, st) =>
            {
                if (args.WasSuccessful)
                {
                    DeleteGraphAsync(_activeTrans, true, graphUri, callback, state);
                }
                else
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(),
                            args.Error), state);
                }
            }, state);
        }
    }

    /// <summary>
    /// Delete a graph as part of an open transaction.
    /// </summary>
    /// <param name="tID">The ID of the transaction to use.</param>
    /// <param name="autoCommit">True to commit the transaction at the end of the delete operation, false to leave the transaction open.</param>
    /// <param name="graphUri">The URI of the graph to delete.</param>
    /// <param name="callback">Callback to invoked on completion of the operation.</param>
    /// <param name="state">Additional state to pass into the callback.</param>
    protected virtual void DeleteGraphAsync(string tID, bool autoCommit, string graphUri,
        AsyncStorageCallback callback, object state)
    {
        HttpRequestMessage request = MakeDeleteGraphRequestMessage(tID, graphUri);
        HttpClient.SendAsync(request).ContinueWith(requestTask =>
        {
            if (requestTask.IsCanceled || requestTask.IsFaulted)
            {
                if (autoCommit)
                {
                    // If something went wrong try to rollback, don't care what the rollback response is
                    Rollback((sender, args, st) => { }, state);
                }

                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph,
                        graphUri.ToSafeUri(),
                        requestTask.IsCanceled
                            ? new RdfStorageException("Operation was cancelled.")
                            : StorageHelper.HandleError(requestTask.Exception,
                                "deleting a Graph asynchronously from")),
                    state);
            }
            else
            {
                HttpResponseMessage response = requestTask.Result;
                if (!response.IsSuccessStatusCode)
                {
                    if (autoCommit)
                    {
                        // If something went wrong try to rollback, don't care what the rollback response is
                        Rollback((sender, args, st) => { }, state);
                    }

                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph,
                            graphUri.ToSafeUri(),
                            StorageHelper.HandleHttpError(response, "deleting a Graph asynchronously from")),
                        state);
                }
                else
                {
                    if (autoCommit)
                    {
                        Commit((sender, args, st) =>
                        {
                            if (args.WasSuccessful)
                            {
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph,
                                        graphUri.ToSafeUri()), state);
                            }
                            else
                            {
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph,
                                        graphUri.ToSafeUri(), args.Error), state);
                            }
                        }, state);
                    }
                    else
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph,
                                graphUri.ToSafeUri()), state);
                    }
                }
            }
        }).ConfigureAwait(true);
    }

    /// <summary>
    /// Helper method that constructs the request message to send to the server to delete a graph.
    /// </summary>
    /// <param name="transactionId">The ID of the transaction within which the graph will be deleted.</param>
    /// <param name="graphName">The name of the graph to be deleted.</param>
    /// <returns>A configured <see cref="HttpRequestMessage"/> instance.</returns>
    protected virtual HttpRequestMessage MakeDeleteGraphRequestMessage(string transactionId, string graphName)
    {
        HttpRequestMessage request = CreateRequest(
            _kb + "/" + transactionId + "/clear/",
            MimeTypesHelper.Any,
            HttpMethod.Post,
            new Dictionary<string, string>() {{"graph-uri", graphName.Equals(string.Empty) ? "DEFAULT" : graphName},}
        );
        request.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[0]);
        return request;
    }

    /// <inheritdoc />
    public override async Task DeleteGraphAsync(string graphName, CancellationToken cancellationToken)
    {
        if (_activeTrans != null)
        {
            await DeleteGraphAsync(_activeTrans, graphName, false, cancellationToken);
        }
        else
        {
            await BeginAsync(cancellationToken);
            await DeleteGraphAsync(_activeTrans, graphName, true, cancellationToken);
        }
    }

    /// <summary>
    /// Delete a graph asynchronously withing the context of a transaction.
    /// </summary>
    /// <param name="transactionId">The transaction to use.</param>
    /// <param name="graphName">The name of the graph to be deleted.</param>
    /// <param name="autoCommit">Whether to automatically commit/rollback the transaction.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="RdfStorageException">Raised if the Stardog server responds with an error status code when deleting thre graph or commiting the transaction.</exception>
    protected virtual async Task DeleteGraphAsync(string transactionId, string graphName, bool autoCommit,
        CancellationToken cancellationToken)
    {
        HttpRequestMessage request = MakeDeleteGraphRequestMessage(transactionId, graphName);
        try
        {
            HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, "deleting a graph from");
            }

            if (autoCommit)
            {
                await CommitAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            if (autoCommit && _activeTrans != null)
            {
                await RollbackAsync(cancellationToken);
            }

            throw ex switch
            {
                OperationCanceledException cancelledException => cancelledException,
                RdfStorageException storageException => storageException,
                _ => StorageHelper.HandleError(ex, "deleting a graph from")
            };
        }
    }
    /// <summary>
    /// Queries the store asynchronously.
    /// </summary>
    /// <param name="query">SPARQL Query.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public virtual void Query(string query, AsyncStorageCallback callback, object state)
    {
        var g = new Graph();
        var results = new SparqlResultSet();
        Query(new GraphHandler(g), new ResultSetHandler(results), query, (sender, args, st) =>
        {
            if (results.ResultsType != SparqlResultsType.Unknown)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQuery, query, results, args.Error),
                    state);
            }
            else
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQuery, query, g, args.Error),
                    state);
            }
        }, state);
    }

    /// <inheritdoc />
    public virtual async Task<object> QueryAsync(string query, CancellationToken cancellationToken)
    {
        var g = new Graph();
        var results = new SparqlResultSet();
        await QueryAsync(new GraphHandler(g), new ResultSetHandler(results), query, cancellationToken);
        return results.ResultsType == SparqlResultsType.Unknown ? (object)g : results;
    }

    /// <summary>
    /// Queries the store asynchronously.
    /// </summary>
    /// <param name="query">SPARQL Query.</param>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public virtual void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string query,
        AsyncStorageCallback callback, object state)
    {
        HttpRequestMessage request = MakeQueryRequestMessage(query);
        HttpClient.SendAsync(request).ContinueWith(requestTask =>
        {
            if (requestTask.IsCanceled || requestTask.IsFaulted)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                    requestTask.IsCanceled
                        ? new RdfStorageException("The operation was cancelled.")
                        : StorageHelper.HandleError(requestTask.Exception, "executing a query on")), state);
            }
            else
            {
                HttpResponseMessage response = requestTask.Result;
                if (!response.IsSuccessStatusCode)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                        StorageHelper.HandleHttpError(response, "executing a query on")), state);
                }
                else
                {
                    response.Content.ReadAsStreamAsync().ContinueWith(responseTask =>
                    {
                        if (responseTask.IsCanceled || responseTask.IsFaulted)
                        {
                            callback(this, new AsyncStorageCallbackArgs(
                                    AsyncStorageOperation.SparqlQueryWithHandler,
                                    requestTask.IsCanceled
                                        ? new RdfStorageException("The operation was cancelled.")
                                        : StorageHelper.HandleError(requestTask.Exception,
                                            "executing a query on")),
                                state);
                        }
                        else
                        {
                            var contentType = response.Content.Headers.ContentType.MediaType;
                            using var data = new StreamReader(responseTask.Result);
                            try
                            {
                                // Is the Content Type referring to a Sparql Result Set format?
                                ISparqlResultsReader sparqlResultsReader = MimeTypesHelper.GetSparqlParser(
                                    contentType,
                                    Regex.IsMatch(query, "ASK", RegexOptions.IgnoreCase));
                                sparqlResultsReader.Load(resultsHandler, data);
                            }
                            catch (RdfParserSelectionException)
                            {
                                // If we get a Parser Selection exception then the Content Type isn't valid for a Sparql Result Set

                                // Is the Content Type referring to a RDF format?
                                IRdfReader rdfReader = MimeTypesHelper.GetParser(contentType);
                                rdfReader.Load(rdfHandler, data);
                            }

                            callback(this,
                                new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                                    query,
                                    rdfHandler, resultsHandler), state);
                        }
                    });
                }
            }
        }).ConfigureAwait(true);
    }

    /// <inheritdoc />
    public virtual async Task QueryAsync(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string query,
        CancellationToken cancellationToken)
    {
        HttpRequestMessage request = MakeQueryRequestMessage(query);
        HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw StorageHelper.HandleHttpQueryError(response);
        }

        var contentType = response.Content.Headers.ContentType.MediaType;
        using var data = new StreamReader(await response.Content.ReadAsStreamAsync());
        try
        {
            // Is the Content Type referring to a Sparql Result Set format?
            ISparqlResultsReader sparqlResultsReader = MimeTypesHelper.GetSparqlParser(
                contentType,
                Regex.IsMatch(query, "ASK", RegexOptions.IgnoreCase));
            sparqlResultsReader.Load(resultsHandler, data);
        }
        catch (RdfParserSelectionException)
        {
            // If we get a Parser Selection exception then the Content Type isn't valid for a Sparql Result Set

            // Is the Content Type referring to a RDF format?
            IRdfReader rdfReader = MimeTypesHelper.GetParser(contentType);
            rdfReader.Load(rdfHandler, data);
        }
    }

    private HttpRequestMessage MakeQueryRequestMessage(string query)
    {
        var transactionId = (_activeTrans == null) ? string.Empty : "/" + _activeTrans;

        // String accept = MimeTypesHelper.HttpRdfOrSparqlAcceptHeader;
        var accept =
            MimeTypesHelper.CustomHttpAcceptHeader(
                MimeTypesHelper.SparqlResultsXml.Concat(
                    MimeTypesHelper.Definitions.Where(d => d.CanParseRdf).SelectMany(d => d.MimeTypes)));

        // Create the Request, for simplicity async requests are always POST
        var queryParams = new Dictionary<string, string>();
        HttpRequestMessage request = CreateRequest(_kb + transactionId + "/query", accept, HttpMethod.Post, queryParams);

        // Build the Post Data and add to the Request Body
        request.Content = new FormUrlEncodedContent(new[] {new KeyValuePair<string, string>("query", query)});
        return request;
    }

    #region HTTP Helper Methods

    /// <summary>
    /// Helper method for creating HTTP Requests to the Store.
    /// </summary>
    /// <param name="servicePath">Path to the Service requested.</param>
    /// <param name="accept">Acceptable Content Types.</param>
    /// <param name="method">HTTP Method.</param>
    /// <param name="requestParams">Querystring Parameters.</param>
    /// <returns></returns>
    [Obsolete("This method has been replaced by CreateRequest(string, string, HttpMethod, Dictionary<string, string>.")]
    protected virtual HttpWebRequest CreateRequest(string servicePath, string accept, string method,
        Dictionary<string, string> requestParams)
    {
        // Build the Request Uri
        var requestUri = _baseUri + servicePath + "?";

        if (!(requestParams is null) && requestParams.Count > 0)
        {
            foreach (var p in requestParams.Keys)
            {
                requestUri += p + "=" + HttpUtility.UrlEncode(requestParams[p]) + "&";
            }
        }

        requestUri += GetReasoningParameter();

        // Create our Request
        var request = (HttpWebRequest)WebRequest.Create(requestUri);
        request.Accept = accept;
        request.Method = method;
        //request = ApplyRequestOptions(request);

        // Add the special Stardog Headers
        AddStardogHeaders(request);

        // Add Credentials if needed
        if (_hasCredentials)
        {
            var credentials = new NetworkCredential(_username, _pwd);
            request.Credentials = credentials;
            request.PreAuthenticate = true;
        }

        return request;
    }

    /// <summary>
    /// Helper method for creating HTTP Requests to the Store.
    /// </summary>
    /// <param name="servicePath">Path to the Service requested.</param>
    /// <param name="accept">Acceptable Content Types.</param>
    /// <param name="method">HTTP Method.</param>
    /// <param name="requestParams">Querystring Parameters.</param>
    /// <returns></returns>
    protected virtual HttpRequestMessage CreateRequest(string servicePath, string accept, HttpMethod method,
        Dictionary<string, string> requestParams)
    {
        // Build the Request Uri
        var requestUri = _baseUri + servicePath + "?";
        if (!(requestParams is null) && requestParams.Count > 0)
        {
            foreach (var p in requestParams.Keys)
            {
                requestUri += p + "=" + HttpUtility.UrlEncode(requestParams[p]) + "&";
            }
        }

        requestUri += GetReasoningParameter();
        var request = new HttpRequestMessage(method, requestUri);
        AddStardogHeaders(request);
        //if (HttpClientHandler.Credentials != null) HttpClientHandler.PreAuthenticate = true;
        return request;
    }

    /// <summary>
    /// Adds Stardog specific request headers; reasoning needed for &lt; 2.2.
    /// </summary>
    /// <param name="request"></param>
    [Obsolete("This method is obsolete and will be removed in a future release. Replaced by AddStardogHeaders(HttpRequestMessage).")]
    protected virtual void AddStardogHeaders(HttpWebRequest request)
    {
#if !NETCORE
        request.Headers.Add("SD-Connection-String", "kb=" + _kb + ";" + GetReasoningParameter());
        // removed persist=sync, no longer needed in latest stardog versions?
        request.Headers.Add("SD-Protocol", "1.0");
#else
        request.Headers["SD-Connection-String"] = "kb=" + this._kb + ";" + this.GetReasoningParameter();
        request.Headers["SD-Protocol"] = "1.0";
#endif
    }

    /// <summary>
    /// Adds Stardog specific request headers; reasoning needed for &lt; 2.2.
    /// </summary>
    /// <param name="request"></param>
    protected virtual void AddStardogHeaders(HttpRequestMessage request)
    {
        request.Headers.Add("SD-Connection-String", "kb=" + _kb + ";" + GetReasoningParameter());
        // removed persist=sync, no longer needed in latest stardog versions?
        request.Headers.Add("SD-Protocol", "1.0");
    }

    /// <summary>
    /// Get the query parameter string that specifies the current reasoning mode.
    /// </summary>
    /// <returns></returns>
    protected virtual string GetReasoningParameter()
    {
        return _reasoning switch
        {
            StardogReasoningMode.QL => "reasoning=QL",
            StardogReasoningMode.EL => "reasoning=EL",
            StardogReasoningMode.RL => "reasoning=RL",
            StardogReasoningMode.DL => "reasoning=DL",
            StardogReasoningMode.RDFS => "reasoning=RDFS",
            StardogReasoningMode.SL => throw new RdfStorageException(
                "Stardog 1.* does not support the SL reasoning level, please ensure you are using a Stardog 2.* connector if you wish to use this reasoning level"),
            _ => string.Empty
        };
    }

    #endregion

    #region Stardog Transaction Support

    /// <summary>
    /// Start a transaction.
    /// </summary>
    /// <param name="enableReasoning">Opens the transaction with reasoning enabled (requires StarDog v5.3.0 or later).</param>
    /// <returns>A transaction ID for the new transaction.</returns>
    protected virtual string BeginTransaction(bool enableReasoning = false)
    {
        string transactionId = null;

        var queryParams = new Dictionary<string, string>();
        if (enableReasoning) queryParams.Add("reasoning", "true");

        HttpRequestMessage request = CreateRequest(_kb + "/transaction/begin", "text/plain", HttpMethod.Post, queryParams);
        request.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[0]);
        try
        {
            using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, "beginning a Transaction in");
            }
            transactionId = response.Content.ReadAsStringAsync().Result;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "beginning a Transaction in");
        }

        if (string.IsNullOrEmpty(transactionId))
        {
            throw new RdfStorageException("Stardog failed to begin a Transaction");
        }
        return transactionId;
    }

    /// <summary>
    /// Commit an open transaction.
    /// </summary>
    /// <param name="transactionId">The ID of the transaction to commit.</param>
    protected virtual void CommitTransaction(string transactionId)
    {
        HttpRequestMessage request = CreateRequest(_kb + "/transaction/commit/" + transactionId, "text/plain",
            HttpMethod.Post, new Dictionary<string, string>());
        request.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[0]);

        using (HttpResponseMessage response = HttpClient.SendAsync(request).Result)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, "committing a Transaction in");
            }
        }

        // Reset the Active Transaction on this Thread if the IDs match up
        if (_activeTrans != null && _activeTrans.Equals(transactionId))
        {
            _activeTrans = null;
        }
    }

    /// <summary>
    /// Rollback an open transaction.
    /// </summary>
    /// <param name="transactionId">The ID of the transaction to rollback.</param>
    protected virtual void RollbackTransaction(string transactionId)
    {
        HttpRequestMessage request = CreateRequest(_kb + "/transaction/rollback/" + transactionId,
            MimeTypesHelper.Any, HttpMethod.Post, new Dictionary<string, string>());
        request.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[0]);
        using (HttpResponseMessage response = HttpClient.SendAsync(request).Result)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, "rolling back a Transaction in");
            }
        }

        // Reset the Active Transaction on this Thread if the IDs match up
        if (_activeTrans != null && _activeTrans.Equals(transactionId))
        {
            _activeTrans = null;
        }
    }

    /// <summary>
    /// Begins a new Transaction.
    /// </summary>
    /// <remarks>
    /// A single transaction.
    /// </remarks>
    public virtual void Begin()
    {
        Begin(false);
    }
    /// <summary>
    /// Begins a new Transaction.
    /// </summary>
    /// <param name="enableReasoning">Opens the transaction with reasoning enabled.</param>
    /// <remarks>
    /// A single transaction.
    /// </remarks>
    public virtual void Begin(bool enableReasoning)
    {
        try
        {
            Monitor.Enter(this);
            if (_activeTrans != null)
            {
                throw new RdfStorageException(
                    "Cannot start a new Transaction as there is already an active Transaction");
            }
            _activeTrans = BeginTransaction(enableReasoning);
        }
        finally
        {
            Monitor.Exit(this);
        }
    }

    /// <summary>
    /// Commits the active Transaction.
    /// </summary>
    /// <exception cref="RdfStorageException">Thrown if there is not an active Transaction on the current Thread.</exception>
    /// <remarks>
    /// Transactions are scoped to Managed Threads.
    /// </remarks>
    public virtual void Commit()
    {
        try
        {
            Monitor.Enter(this);
            if (_activeTrans == null)
            {
                throw new RdfStorageException(
                    "Cannot commit a Transaction as there is currently no active Transaction");
            }
            CommitTransaction(_activeTrans);
        }
        finally
        {
            Monitor.Exit(this);
        }
    }

    /// <summary>
    /// Rolls back the active Transaction.
    /// </summary>
    /// <exception cref="RdfStorageException">Thrown if there is not an active Transaction on the current Thread.</exception>
    /// <remarks>
    /// Transactions are scoped to Managed Threads.
    /// </remarks>
    public virtual void Rollback()
    {
        try
        {
            Monitor.Enter(this);
            if (_activeTrans == null)
            {
                throw new RdfStorageException(
                    "Cannot rollback a Transaction on the as there is currently no active Transaction");
            }
            RollbackTransaction(_activeTrans);
        }
        finally
        {
            Monitor.Exit(this);
        }
    }

    /// <summary>
    /// Begins a transaction asynchronously.
    /// </summary>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public virtual void Begin(AsyncStorageCallback callback, object state)
    {
        if (_activeTrans != null)
        {
            callback(this,
                new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin,
                    new RdfStorageException(
                        "Cannot start a new Transaction as there is already an active Transaction")), state);
        }
        else
        {
            HttpRequestMessage request = CreateRequest(_kb + "/transaction/begin", "text/plain",
                HttpMethod.Post, new Dictionary<string, string>());
            request.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[0]);
            HttpClient.SendAsync(request).ContinueWith(requestTask =>
            {
                if (requestTask.IsCanceled || requestTask.IsFaulted)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin,
                            requestTask.IsCanceled
                                ? new RdfStorageException("The operation was cancelled.")
                                : StorageHelper.HandleError(requestTask.Exception,
                                    "beginning a Transaction in")), state);
                }
                else
                {
                    HttpResponseMessage response = requestTask.Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin,
                                StorageHelper.HandleHttpError(response, "beginning a Transaction in")), state);
                    }
                    else
                    {
                        response.Content.ReadAsStringAsync().ContinueWith(t =>
                        {
                            if (t.IsCanceled || t.IsFaulted)
                            {
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin,
                                        requestTask.IsCanceled
                                            ? new RdfStorageException("The operation was cancelled.")
                                            : StorageHelper.HandleError(requestTask.Exception,
                                                "beginning a Transaction in")), state);
                            }
                            else
                            {
                                var transactionId = t.Result;
                                if (string.IsNullOrEmpty(transactionId))
                                {
                                    callback(this,
                                        new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin,
                                            new RdfStorageException("Stardog failed to begin a transaction")),
                                        state);
                                }
                                else
                                {
                                    _activeTrans = transactionId;
                                    callback(this,
                                        new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin),
                                        state);
                                }
                            }
                        });
                    }
                }
            }).ConfigureAwait(true);

        }
    }

    /// <summary>
    /// Begins a transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="RdfStorageException">Raised if there is already an active transaction for this connector or if the Stardog server responds with an error status code or a null or empty response.</exception>
    public virtual async Task BeginAsync(CancellationToken cancellationToken)
    {
        if (_activeTrans != null)
        {
            throw new RdfStorageException(
                "Cannot start a new Transaction as there is already an active Transaction");
        }

        try
        {
            HttpRequestMessage request = CreateRequest(_kb + "/transaction/begin", "text/plain",
                HttpMethod.Post, new Dictionary<string, string>());
            request.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[0]);
            HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, "beginning a Transaction in");
            }

            var transactionId = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(transactionId))
            {
                throw new RdfStorageException("Stardog failed to begin a transaction");
            }

            _activeTrans = transactionId;
        }
        catch (RdfStorageException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "beginning a transaction in");
        }
    }

    /// <summary>
    /// Commits a transaction asynchronously.
    /// </summary>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public virtual void Commit(AsyncStorageCallback callback, object state)
    {
        if (_activeTrans == null)
        {
            callback(this,
                new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit,
                    new RdfStorageException(
                        "Cannot commit a Transaction as there is currently no active Transaction")), state);
        }
        else
        {
            HttpRequestMessage request = CreateRequest(_kb + "/transaction/commit/" + _activeTrans,
                "text/plain", HttpMethod.Post, new Dictionary<string, string>());
            request.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[0]);
            HttpClient.SendAsync(request).ContinueWith(requestTask =>
            {
                if (requestTask.IsCanceled || requestTask.IsFaulted)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit,
                            requestTask.IsCanceled
                                ? new RdfStorageException("The operation was cancelled.")
                                : StorageHelper.HandleError(requestTask.Exception,
                                    "committing a Transaction to")), state);
                }
                else
                {
                    using HttpResponseMessage response = requestTask.Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit,
                                StorageHelper.HandleHttpError(response,
                                    "committing a Transaction to")), state);
                    }
                    else
                    {
                        _activeTrans = null;
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit),
                            state);
                    }
                }
            }).ConfigureAwait(true);

        }
    }

    /// <summary>
    /// Commit a transaction asynchronously.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="RdfStorageException">Raised if there is currently no open transaction for this connector, or if the Stardog server responds with an error status code.</exception>
    public virtual async Task CommitAsync(CancellationToken cancellationToken)
    {
        // TODO: Should we allow a cancellation during the commit?
        if (_activeTrans == null)
        {
            throw new RdfStorageException(
                "Cannot commit a Transaction as there is currently no active Transaction");
        }

        try
        {
            HttpRequestMessage request = CreateRequest(_kb + "/transaction/commit/" + _activeTrans,
                "text/plain", HttpMethod.Post, new Dictionary<string, string>());
            request.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[0]);
            HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, "committing a Transaction to");
            }

            _activeTrans = null;
        }
        catch (RdfStorageException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "commiting a transaction to");
        }
    }

    /// <summary>
    /// Rolls back a transaction asynchronously.
    /// </summary>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public virtual void Rollback(AsyncStorageCallback callback, object state)
    {
        if (_activeTrans == null)
        {
            callback(this,
                new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback,
                    new RdfStorageException(
                        "Cannot rollback a Transaction on the as there is currently no active Transaction")),
                state);
        }
        else
        {
            HttpRequestMessage request = CreateRequest(_kb + "/transaction/rollback/" + _activeTrans,
                MimeTypesHelper.Any, HttpMethod.Post, new Dictionary<string, string>());
            request.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[0]);
            HttpClient.SendAsync(request).ContinueWith(requestTask =>
            {
                if (requestTask.IsCanceled || requestTask.IsFaulted)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback,
                            requestTask.IsCanceled
                                ? new RdfStorageException("The operation was cancelled.")
                                : StorageHelper.HandleError(requestTask.Exception,
                                    "rolling back a Transaction from")), state);
                }
                else
                {
                    using HttpResponseMessage response = requestTask.Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback,
                                StorageHelper.HandleHttpError(response,
                                    "rolling back a Transaction from")), state);
                    }
                    else
                    {
                        _activeTrans = null;
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback),
                            state);
                    }
                }
            }).ConfigureAwait(true);
        }
    }

    /// <summary>
    /// Roll back the current transaction on this connector asynchronously.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="RdfStorageException">If there is no open transaction on the connector or if the Stardog server responds with an error status code.</exception>
    public virtual async Task RollbackAsync(CancellationToken cancellationToken)
    {
        // TODO: Should we allow a cancellation during the commit?
        if (_activeTrans == null)
        {
            throw new RdfStorageException(
                "Cannot rollback a Transaction on the as there is currently no active Transaction");
        }

        try
        {
            HttpRequestMessage request = CreateRequest(_kb + "/transaction/rollback/" + _activeTrans,
                MimeTypesHelper.Any, HttpMethod.Post, new Dictionary<string, string>());
            request.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[0]);
            HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, "rolling back a transaction from");
            }

            _activeTrans = null;
        }
        catch (RdfStorageException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "rolling back a transaction from");
        }
    }

    #endregion

    /// <summary>
    /// Disposes of the Connector.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        _isReady = false;
    }

    /// <summary>
    /// Gets a String which gives details of the Connection.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var mode = string.Empty;
        switch (_reasoning)
        {
            case StardogReasoningMode.QL:
                mode = " (OWL QL Reasoning)";
                break;
            case StardogReasoningMode.EL:
                mode = " (OWL EL Reasoning)";
                break;
            case StardogReasoningMode.RL:
                mode = " (OWL RL Reasoning)";
                break;
            case StardogReasoningMode.DL:
                mode = " (OWL DL Reasoning)";
                break;
            case StardogReasoningMode.RDFS:
                mode = " (RDFS Reasoning)";
                break;
            case StardogReasoningMode.SL:
                mode = " (SL Reasoning)";
                break;
        }
        return "[Stardog] Knowledge Base '" + _kb + "' on Server '" + _baseUri + "'" + mode;
    }

    /// <summary>
    /// Serializes the connection's configuration.
    /// </summary>
    /// <param name="context">Configuration Serialization Context.</param>
    public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
    {
        INode manager = context.NextSubject;
        INode rdfType = context.Graph.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfType));
        INode rdfsLabel = context.Graph.CreateUriNode(context.UriFactory.Create(NamespaceMapper.RDFS + "label"));
        INode dnrType = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyType));
        INode genericManager =
            context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.ClassStorageProvider));
        INode server = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyServer));
        INode store = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyStore));
        INode loadMode = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyLoadMode));

        // Add Core config
        context.Graph.Assert(new Triple(manager, rdfType, genericManager));
        context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(ToString())));
        context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(GetType().FullName)));
        context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(_baseUri)));
        context.Graph.Assert(new Triple(manager, store, context.Graph.CreateLiteralNode(_kb)));

        // Add reasoning mode
        if (_reasoning != StardogReasoningMode.None)
            context.Graph.Assert(new Triple(manager, loadMode,
                context.Graph.CreateLiteralNode(_reasoning.ToString())));

        // Add User Credentials
        if (_username != null && _pwd != null)
        {
            INode username = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyUser));
            INode pwd = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyPassword));
            context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(_username)));
            context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(_pwd)));
        }

        SerializeStandardConfig(manager, context);
    }
}