/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2020 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage.Management;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Abstract Base Class for connecting to any Store that supports the Sesame 2.0 HTTP Communication protocol.
    /// </summary>
    /// <remarks>
    /// <para>
    /// See <a href="http://www.openrdf.org/doc/sesame2/system/ch08.html">here</a> for the protocol specification, this base class supports Version 5 of the protocol which does not include SPARQL Update support.
    /// </para>
    /// </remarks>
    public abstract class BaseSesameHttpProtocolConnector
        : BaseAsyncHttpConnector, IAsyncQueryableStorage, IConfigurationSerializable
        , IQueryableStorage
    {
        /// <summary>
        /// Base Uri for the Store.
        /// </summary>
        protected string _baseUri;
        /// <summary>
        /// Store ID.
        /// </summary>
        protected string _store;

        /// <summary>
        /// Repositories Prefix.
        /// </summary>
        protected string _repositoriesPrefix = "repositories/";
        /// <summary>
        /// Query Path Prefix.
        /// </summary>
        protected string _queryPath = string.Empty;
        /// <summary>
        /// Update Path Prefix.
        /// </summary>
        protected string _updatePath = "/statements";
        /// <summary>
        /// Whether to do full encoding of contexts.
        /// </summary>
        protected bool _fullContextEncoding = true;

        /// <summary>
        /// Server the store is hosted on.
        /// </summary>
        protected SesameServer _server;

        private SparqlQueryParser _parser = new SparqlQueryParser();
        private NTriplesFormatter _formatter = new NTriplesFormatter();

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store.
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store.</param>
        /// <param name="storeID">Store ID.</param>
        public BaseSesameHttpProtocolConnector(string baseUri, string storeID)
            : this(baseUri, storeID, null, null) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store.
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store.</param>
        /// <param name="storeID">Store ID.</param>
        /// <param name="username">Username to use for requests that require authentication.</param>
        /// <param name="password">Password to use for requests that require authentication.</param>
        public BaseSesameHttpProtocolConnector(string baseUri, string storeID, string username, string password)
        {
            _baseUri = baseUri;
            if (!_baseUri.EndsWith("/")) _baseUri += "/";
            _store = storeID;

            SetCredentials(username, password);

            // Setup server
            _server = new SesameServer(_baseUri);
        }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store.
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store.</param>
        /// <param name="storeID">Store ID.</param>
        /// <param name="proxy">Proxy Server.</param>
        public BaseSesameHttpProtocolConnector(string baseUri, string storeID, IWebProxy proxy)
            : this(baseUri, storeID, null, null, proxy) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store.
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store.</param>
        /// <param name="storeID">Store ID.</param>
        /// <param name="username">Username to use for requests that require authentication.</param>
        /// <param name="password">Password to use for requests that require authentication.</param>
        /// <param name="proxy">Proxy Server.</param>
        public BaseSesameHttpProtocolConnector(string baseUri, string storeID, string username, string password, IWebProxy proxy)
            : this(baseUri, storeID, username, password)
        {
            Proxy = proxy;
        }

        /// <summary>
        /// Gets the Base URI to the repository.
        /// </summary>
        [Description("The Base URI for requests made to the store.")]
        public string BaseUri
        {
            get
            {
                return _baseUri;
            }
        }

        /// <summary>
        /// Gets the Repository Name that is in use.
        /// </summary>
        [Description("The Repository to which this is a connection.")]
        public string RepositoryName
        {
            get
            {
                return _store;
            }
        }

        /// <summary>
        /// Gets the Save Behaviour of Stores that use the Sesame HTTP Protocol.
        /// </summary>
        public override IOBehaviour IOBehaviour
        {
            get
            {
                return IOBehaviour.GraphStore | IOBehaviour.CanUpdateTriples;
            }
        }

        /// <summary>
        /// Returns that Updates are supported on Sesame HTTP Protocol supporting Stores.
        /// </summary>
        public override bool UpdateSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that deleting graphs from the Sesame store is supported.
        /// </summary>
        public override bool DeleteSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that listing Graphs is supported.
        /// </summary>
        public override bool ListGraphsSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that the Connection is ready.
        /// </summary>
        public override bool IsReady
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that the Connection is not read-only.
        /// </summary>
        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the parent server.
        /// </summary>
        public override IStorageServer ParentServer
        {
            get
            {
                return _server;
            }
        }

        /// <summary>
        /// Makes a SPARQL Query against the underlying Store.
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query.</param>
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

            return g;
        }

        /// <summary>
        /// Makes a SPARQL Query against the underlying Store processing the results with an appropriate handler from those provided.
        /// </summary>
        /// <param name="rdfHandler">RDF Handler.</param>
        /// <param name="resultsHandler">Results Handler.</param>
        /// <param name="sparqlQuery">SPARQL Query.</param>
        /// <returns></returns>
        public virtual void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
        {
            try
            {
                // Pre-parse the query to determine what the Query Type is
                bool isAsk;
                SparqlQuery q = null;
                try
                {
                    q = _parser.ParseFromString(sparqlQuery);
                    isAsk = q.QueryType == SparqlQueryType.Ask;
                }
                catch
                {
                    // If parsing error fallback to naive detection
                    isAsk = Regex.IsMatch(sparqlQuery, "ASK", RegexOptions.IgnoreCase);
                }

                // Select Accept Header
                string accept;
                if (q != null)
                {
                    accept = (SparqlSpecsHelper.IsSelectQuery(q.QueryType) || q.QueryType == SparqlQueryType.Ask
                        ? MimeTypesHelper.HttpSparqlAcceptHeader
                        : MimeTypesHelper.HttpAcceptHeader);
                }
                else
                {
                    accept = MimeTypesHelper.HttpRdfOrSparqlAcceptHeader;
                }

                // Create the Request
                // For Sesame we always POST queries because using GET doesn't always work (CORE-374)
                var queryParams = new Dictionary<string, string>();
                HttpRequestMessage request = CreateRequest(_repositoriesPrefix + _store + _queryPath, accept,
                    HttpMethod.Post, queryParams);

                // Build the Post Data and add to the Request Body
                KeyValuePair<string, string>[]
                    formData = {new KeyValuePair<string, string>("query", sparqlQuery)};
                request.Content = new FormUrlEncodedContent(formData);

                // Get the Response and process based on the Content Type
                using (HttpResponseMessage response = HttpClient.SendAsync(request).Result)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw StorageHelper.HandleHttpQueryError(response);
                    }

                    var data = new StreamReader(response.Content.ReadAsStreamAsync().Result);
                    var ctype = response.Content.Headers.ContentType.MediaType;
                    try
                    {
                        // Is the Content Type referring to a Sparql Result Set format?
                        ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(ctype, isAsk);
                        resreader.Load(resultsHandler, data);
                    }
                    catch (RdfParserSelectionException)
                    {
                        // If we get a Parser Selection exception then the Content Type isn't valid for a SPARQL Result Set
                        // HACK: As this is Sesame this may be it being buggy and sending application/xml instead of application/sparql-results+xml
                        if (ctype.StartsWith("application/xml"))
                        {
                            try
                            {
                                ISparqlResultsReader resreader =
                                    MimeTypesHelper.GetSparqlParser("application/sparql-results+xml");
                                resreader.Load(resultsHandler, data);

                            }
                            catch (RdfParserSelectionException)
                            {
                                // Ignore this and fall back to trying as an RDF format instead
                            }
                        }

                        // Is the Content Type referring to a RDF format?
                        IRdfReader rdfreader = MimeTypesHelper.GetParser(ctype);
                        if (q != null && (SparqlSpecsHelper.IsSelectQuery(q.QueryType) ||
                                          q.QueryType == SparqlQueryType.Ask))
                        {
                            var resreader = new SparqlRdfParser(rdfreader);
                            resreader.Load(resultsHandler, data);
                        }
                        else
                        {
                            rdfreader.Load(rdfHandler, data);
                        }
                    }
                }
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpQueryError(webEx);
            }
        }

        /// <summary>
        /// Escapes a Query to avoid a character encoding issue when communicating a query to Sesame.
        /// </summary>
        /// <param name="query">Query.</param>
        /// <returns></returns>
        protected virtual string EscapeQuery(string query)
        {
            var output = new StringBuilder();
            var cs = query.ToCharArray();
            for (var i = 0; i < cs.Length; i++)
            {
                var c = cs[i];
                if (c <= 127)
                {
                    output.Append(c);
                }
                else
                {
                    output.Append("\\u");
                    output.Append(((int)c).ToString("x4"));
                }
            }
            return output.ToString();
        }


        /// <summary>
        /// Gets the Content Type used to save data to the store i.e. the MIME type to use for the <strong>Content-Type</strong> header.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetSaveContentType()
        {
            return MimeTypesHelper.NTriples[0];
        }

        /// <summary>
        /// Creates an RDF Writer to use for saving data to the store.
        /// </summary>
        /// <returns></returns>
        protected virtual IRdfWriter CreateRdfWriter()
        {
            return new NTriplesWriter();
        }

        /// <summary>
        /// Loads a Graph from the Store.
        /// </summary>
        /// <param name="g">Graph to load into.</param>
        /// <param name="graphUri">Uri of the Graph to load.</param>
        /// <remarks>If a Null Uri is specified then the default graph (statements with no context in Sesame parlance) will be loaded.</remarks>
        public virtual void LoadGraph(IGraph g, Uri graphUri)
        {
            LoadGraph(g, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the Store.
        /// </summary>
        /// <param name="handler">RDF Handler.</param>
        /// <param name="graphUri">Uri of the Graph to load.</param>
        /// <remarks>If a Null Uri is specified then the default graph (statements with no context in Sesame parlance) will be loaded.</remarks>
        public virtual void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            LoadGraph(handler, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the Store.
        /// </summary>
        /// <param name="g">Graph to load into.</param>
        /// <param name="graphUri">Uri of the Graph to load.</param>
        /// <remarks>If a Null/Empty Uri is specified then the default graph (statements with no context in Sesame parlance) will be loaded.</remarks>
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
        /// <param name="graphUri">Uri of the Graph to load.</param>
        /// <remarks>If a Null/Empty Uri is specified then the default graph (statements with no context in Sesame parlance) will be loaded.</remarks>
        public virtual void LoadGraph(IRdfHandler handler, string graphUri)
        {
            try
            {
                var serviceParams = new Dictionary<string, string>();

                var requestUri = _repositoriesPrefix + _store + "/statements";
                if (!graphUri.Equals(string.Empty))
                {
                    serviceParams.Add("context", "<" + graphUri + ">");
                }
                else
                {
                    serviceParams.Add("context", "null");
                }

                HttpRequestMessage request = CreateRequest(requestUri, MimeTypesHelper.HttpAcceptHeader, HttpMethod.Get, serviceParams);

                using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode)
                {
                    throw StorageHelper.HandleHttpError(response, "load a Graph from");
                }
                IRdfReader parser = MimeTypesHelper.GetParser(response.Content.Headers.ContentType.MediaType);
                parser.Load(handler, new StreamReader(response.Content.ReadAsStreamAsync().Result));
            }
            catch (Exception ex)
            {
                throw StorageHelper.HandleError(ex, "load a Graph from");
            }
        }

        /// <summary>
        /// Saves a Graph into the Store (Warning: Completely replaces any existing Graph with the same URI unless there is no URI - see remarks for details).
        /// </summary>
        /// <param name="g">Graph to save.</param>
        /// <remarks>
        /// If the Graph has no URI then the contents will be appended to the Store, if the Graph has a URI then existing data associated with that URI will be replaced.
        /// </remarks>
        public virtual void SaveGraph(IGraph g)
        {
            try
            {
                HttpRequestMessage request;
                var serviceParams = new Dictionary<string, string>();

                if (g.BaseUri != null)
                {
                    if (_fullContextEncoding)
                    {
                        serviceParams.Add("context", "<" + g.BaseUri.AbsoluteUri + ">");
                    }
                    else
                    {
                        serviceParams.Add("context", g.BaseUri.AbsoluteUri);
                    }
                    request = CreateRequest(_repositoriesPrefix + _store + "/statements", "*/*", HttpMethod.Put, serviceParams);
                }
                else
                {
                    request = CreateRequest(_repositoriesPrefix + _store + "/statements", "*/*", HttpMethod.Post, serviceParams);
                }

                IRdfWriter rdfWriter = CreateRdfWriter();
                request.Content = new GraphContent(g, rdfWriter);

                using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode)
                {
                    throw StorageHelper.HandleHttpError(response, "save a Graph to");
                }
            }
            catch (Exception ex)
            {
                throw StorageHelper.HandleError(ex, "save a Graph to");
            }
        }

        /// <summary>
        /// Updates a Graph.
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to update.</param>
        /// <param name="additions">Triples to be added.</param>
        /// <param name="removals">Triples to be removed.</param>
        public virtual void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            UpdateGraph(graphUri.ToSafeString(), additions, removals);
        }

        /// <summary>
        /// Updates a Graph.
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to update.</param>
        /// <param name="additions">Triples to be added.</param>
        /// <param name="removals">Triples to be removed.</param>
        public virtual void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            try
            {
                HttpRequestMessage request;
                HttpResponseMessage response;
                var serviceParams = new Dictionary<string, string>();
                IRdfWriter rdfWriter = CreateRdfWriter();

                if (!graphUri.Equals(string.Empty))
                {
                    serviceParams.Add("context", "<" + graphUri + ">");
                }
                else
                {
                    serviceParams.Add("context", "null");
                }

                if (removals != null)
                {
                    if (removals.Any())
                    {
                        serviceParams.Add("subj", null);
                        serviceParams.Add("pred", null);
                        serviceParams.Add("obj", null);

                        // Have to do a DELETE for each individual Triple
                        foreach (Triple t in removals.Distinct())
                        {
                            serviceParams["subj"] = _formatter.Format(t.Subject);
                            serviceParams["pred"] = _formatter.Format(t.Predicate);
                            serviceParams["obj"] = _formatter.Format(t.Object);
                            request = CreateRequest(_repositoriesPrefix + _store + "/statements", "*/*", HttpMethod.Delete, serviceParams);

                            using (response = HttpClient.SendAsync(request).Result)
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    throw StorageHelper.HandleHttpError(response, "updating a Graph in");
                                }
                            }
                        }
                        serviceParams.Remove("subj");
                        serviceParams.Remove("pred");
                        serviceParams.Remove("obj");
                    }
                }

                if (additions != null)
                {
                    if (additions.Any())
                    {
                        // Add the new Triples
                        request = CreateRequest(_repositoriesPrefix + _store + "/statements", "*/*", HttpMethod.Post, serviceParams);
                        var h = new Graph();
                        h.Assert(additions);
                        request.Content = new GraphContent(h, rdfWriter);
                        using (response = HttpClient.SendAsync(request).Result)
                        {
                            if (!response.IsSuccessStatusCode)
                            {
                                throw StorageHelper.HandleHttpError(response, "updating a Graph in");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw StorageHelper.HandleError(ex, "updating a Graph in");
            }
        }

        /// <summary>
        /// Deletes a Graph from the Sesame store.
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete.</param>
        public virtual void DeleteGraph(Uri graphUri)
        {
            DeleteGraph(graphUri.ToSafeString());
        }

        /// <summary>
        /// Deletes a Graph from the Sesame store.
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete.</param>
        public virtual void DeleteGraph(string graphUri)
        {
            try
            {
                var serviceParams = new Dictionary<string, string>();

                if (!graphUri.Equals(string.Empty))
                {
                    serviceParams.Add("context", "<" + graphUri + ">");
                }
                else
                {
                    serviceParams.Add("context", "null");
                }

                HttpRequestMessage request = CreateRequest(_repositoriesPrefix + _store + "/statements", "*/*", HttpMethod.Delete, serviceParams);

                using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode)
                {
                    throw StorageHelper.HandleHttpError(response, "deleting a Graph from");
                }
            }
            catch (Exception ex)
            {
                throw StorageHelper.HandleError(ex, "deleting a Graph from");
            }
        }

        /// <summary>
        /// Gets the list of Graphs in the Sesame store.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<Uri> ListGraphs()
        {
            try
            {
                if (Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }") is SparqlResultSet resultSet)
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

                return Enumerable.Empty<Uri>();
            }
            catch (Exception ex)
            {
                throw StorageHelper.HandleError(ex, "listing Graphs from");
            }
        }

        /// <summary>
        /// Gets the parent server.
        /// </summary>
        public override IAsyncStorageServer AsyncParentServer
        {
            get
            {
                return _server;
            }
        }

        /// <summary>
        /// Saves a Graph to the Store asynchronously.
        /// </summary>
        /// <param name="g">Graph to save.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="state">State to pass to the callback.</param>
        public override void SaveGraph(IGraph g, AsyncStorageCallback callback, object state)
        {
            HttpRequestMessage request;
            var serviceParams = new Dictionary<string, string>();

            if (g.BaseUri != null)
            {
                if (_fullContextEncoding)
                {
                    serviceParams.Add("context", "<" + g.BaseUri.AbsoluteUri + ">");
                }
                else
                {
                    serviceParams.Add("context", g.BaseUri.AbsoluteUri);
                }
                request = CreateRequest(_repositoriesPrefix + _store + "/statements", "*/*", HttpMethod.Put, serviceParams);
            }
            else
            {
                request = CreateRequest(_repositoriesPrefix + _store + "/statements", "*/*", HttpMethod.Post, serviceParams);
            }

            request.Content = new GraphContent(g, GetSaveContentType());
            SaveGraphAsync(request, g, callback, state);
        }

        /// <summary>
        /// Loads a Graph from the Store asynchronously.
        /// </summary>
        /// <param name="handler">Handler to load with.</param>
        /// <param name="graphUri">URI of the Graph to load.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="state">State to pass to the callback.</param>
        public override void LoadGraph(IRdfHandler handler, string graphUri, AsyncStorageCallback callback, object state)
        {
            var serviceParams = new Dictionary<string, string>();

            var requestUri = _repositoriesPrefix + _store + "/statements";
            if (!graphUri.Equals(string.Empty))
            {
                serviceParams.Add("context", "<" + graphUri + ">");
            }

            HttpRequestMessage request = CreateRequest(requestUri, MimeTypesHelper.HttpAcceptHeader, HttpMethod.Get, serviceParams);

            LoadGraphAsync(request, handler, callback, state);
        }

        /// <summary>
        /// Updates a Graph in the Store asynchronously.
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update.</param>
        /// <param name="additions">Triples to be added.</param>
        /// <param name="removals">Triples to be removed.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="state">State to pass to the callback.</param>
        public override void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, object state)
        {
            HttpRequestMessage request;
            Dictionary<string, string> serviceParams;
            IRdfWriter rdfWriter = CreateRdfWriter();

            if (removals != null)
            {
                if (removals.Any())
                {
                    // For Async deletes we need to build up a sequence of requests to send
                    var requests = new Queue<HttpRequestMessage>();

                    // Have to do a DELETE for each individual Triple
                    foreach (Triple t in removals.Distinct())
                    {
                        // Prep Service Params
                        serviceParams = new Dictionary<string, string>();
                        if (!graphUri.Equals(string.Empty))
                        {
                            serviceParams.Add("context", "<" + graphUri + ">");
                        }
                        else
                        {
                            serviceParams.Add("context", "null");
                        }

                        serviceParams.Add("subj", _formatter.Format(t.Subject));
                        serviceParams.Add("pred", _formatter.Format(t.Predicate));
                        serviceParams.Add("obj", _formatter.Format(t.Object));
                        request = CreateRequest(_repositoriesPrefix + _store + "/statements", "*/*", HttpMethod.Delete, serviceParams);
                        requests.Enqueue(request);
                    }

                    // Run all the requests, if any error make an error callback and abort, if it succeeds then do any adds
                    MakeRequestSequence(requests, (sender, args, st) =>
                        {
                            if (!args.WasSuccessful)
                            {
                                // Deletes failed
                                // Invoke callback and bail out
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(), new RdfStorageException("An error occurred while trying to asynchronously delete triples from the Store, see inner exception for details", args.Error)), state);
                            }
                            else
                            {
                                // Deletes succeeded, perform any adds
                                if (additions != null)
                                {
                                    if (additions.Any())
                                    {
                                        // Prep Service Params
                                        serviceParams = new Dictionary<string, string>();
                                        if (!graphUri.Equals(string.Empty))
                                        {
                                            serviceParams.Add("context", "<" + graphUri + ">");
                                        }
                                        else
                                        {
                                            serviceParams.Add("context", "null");
                                        }

                                        // Add the new Triples
                                        request = CreateRequest(_repositoriesPrefix + _store + "/statements", "*/*", HttpMethod.Post, serviceParams);

                                        // Thankfully Sesame lets us do additions in one request so we don't end up with horrible code like for the removals above
                                        UpdateGraphAsync(request, rdfWriter, graphUri.ToSafeUri(), additions, callback, state);

                                        // Don't want to make the callback until the adds have finished
                                        // So we must return here as otherwise we will make the callback prematurely
                                        return;
                                    }
                                }

                                // If there were no adds we should make the callback now
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
                            }
                        }, state);

                    // Don't want to make the callback until deletes have finished and we've processed any subsequent adds
                    // So we must return here as otherwise we will make the callback prematurely
                    return;
                }
            }
            else if (additions != null)
            {
                if (additions.Any())
                {
                    // Prep Service Params
                    serviceParams = new Dictionary<string, string>();
                    if (!graphUri.Equals(string.Empty))
                    {
                        serviceParams.Add("context", "<" + graphUri + ">");
                    }
                    else
                    {
                        serviceParams.Add("context", "null");
                    }

                    // Add the new Triples
                    request = CreateRequest(_repositoriesPrefix + _store + "/statements", "*/*", HttpMethod.Post, serviceParams);

                    // Thankfully Sesame lets us do additions in one request so we don't end up with horrible code like for the removals above
                    UpdateGraphAsync(request, rdfWriter, graphUri.ToSafeUri(), additions, callback, state);

                    // Don't want to make the callback until the adds have finished
                    // So we must return here as otherwise we will make the callback prematurely
                    return;
                }
            }

            // If we get here then we had nothing to do
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
        }

        /// <summary>
        /// Deletes a Graph from the Store.
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="state">State to pass to the callback.</param>
        public override void DeleteGraph(string graphUri, AsyncStorageCallback callback, object state)
        {
            var serviceParams = new Dictionary<string, string>();

            if (!graphUri.Equals(string.Empty))
            {
                serviceParams.Add("context", "<" + graphUri + ">");
            }
            else
            {
                serviceParams.Add("context", "null");
            }

            HttpRequestMessage request = CreateRequest(_repositoriesPrefix + _store + "/statements", "*/*", HttpMethod.Delete, serviceParams);
            DeleteGraphAsync(request, false, graphUri, callback, state);
        }

        /// <summary>
        /// Makes a SPARQL Query against the underlying store.
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="state">State to pass to the callback.</param>
        /// <returns><see cref="SparqlResultSet">SparqlResultSet</see> or a <see cref="Graph">Graph</see> depending on the Sparql Query.</returns>
        public void Query(string sparqlQuery, AsyncStorageCallback callback, object state)
        {
            var g = new Graph();
            var results = new SparqlResultSet();
            Query(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery, (sender, args, st) =>
            {
                callback(this,
                    results.ResultsType != SparqlResultsType.Unknown
                        ? new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQuery, sparqlQuery, results,
                            args.Error)
                        : new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQuery, sparqlQuery, g, args.Error),
                    state);
            }, state);
        }

        /// <summary>
        /// Makes a SPARQL Query against the underlying store processing the resulting Graph/Result Set with a handler of your choice.
        /// </summary>
        /// <param name="rdfHandler">RDF Handler.</param>
        /// <param name="resultsHandler">SPARQL Results Handler.</param>
        /// <param name="sparqlQuery">SPARQL Query.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="state">State to pass to the callback.</param>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery,
            AsyncStorageCallback callback, object state)
        {
            try
            {
                // Pre-parse the query to determine what the Query Type is
                var isAsk = false;
                SparqlQuery q = null;
                try
                {
                    q = _parser.ParseFromString(sparqlQuery);
                    isAsk = q.QueryType == SparqlQueryType.Ask;
                }
                catch
                {
                    // If parsing error fallback to naive detection
                    isAsk = Regex.IsMatch(sparqlQuery, "ASK", RegexOptions.IgnoreCase);
                }

                // Select Accept Header
                string accept;
                if (q != null)
                {
                    accept = (SparqlSpecsHelper.IsSelectQuery(q.QueryType) || q.QueryType == SparqlQueryType.Ask
                        ? MimeTypesHelper.HttpSparqlAcceptHeader
                        : MimeTypesHelper.HttpAcceptHeader);
                }
                else
                {
                    accept = MimeTypesHelper.HttpRdfOrSparqlAcceptHeader;
                }

                // Create the Request, for simplicity async requests are always POST
                var queryParams = new Dictionary<string, string>();
                HttpRequestMessage request = CreateRequest(_repositoriesPrefix + _store + _queryPath, accept,
                    HttpMethod.Post, queryParams);

                // Build the Post Data and add to the Request Body
                request.Content =
                    new FormUrlEncodedContent(new[] {new KeyValuePair<string, string>("query", sparqlQuery)});

                HttpClient.SendAsync(request).ContinueWith(requestTask =>
                {
                    if (requestTask.IsCanceled || requestTask.IsFaulted)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                            requestTask.IsCanceled
                                ? new RdfStorageException("The operation was cancelled.")
                                : StorageHelper.HandleError(requestTask.Exception, "querying")
                        ), state);
                    }
                    else
                    {
                        HttpResponseMessage response = requestTask.Result;
                        if (!response.IsSuccessStatusCode)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                                StorageHelper.HandleHttpError(response, "querying")), state);
                        }
                        else
                        {
                            response.Content.ReadAsStreamAsync().ContinueWith(readTask =>
                            {
                                if (readTask.IsCanceled || readTask.IsFaulted)
                                {
                                    callback(this, new AsyncStorageCallbackArgs(
                                        AsyncStorageOperation.SparqlQueryWithHandler,
                                        readTask.IsCanceled
                                            ? new RdfStorageException("The operation was cancelled.")
                                            : StorageHelper.HandleError(readTask.Exception, "querying")
                                    ), state);
                                }
                                else
                                {
                                    var contentType = response.Content.Headers.ContentType.MediaType;
                                    var data = new StreamReader(readTask.Result);
                                    try
                                    {
                                        // Is the Content Type referring to a Sparql Result Set format?
                                        ISparqlResultsReader resreader =
                                            MimeTypesHelper.GetSparqlParser(contentType, isAsk);
                                        resreader.Load(resultsHandler, data);
                                        callback(this,
                                            new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                                                sparqlQuery, rdfHandler, resultsHandler), state);
                                    }
                                    catch (RdfParserSelectionException)
                                    {
                                        // If we get a Parser Selection exception then the Content Type isn't valid for a SPARQL Result Set
                                        // HACK: As this is Sesame this may be it being buggy and sending application/xml instead of application/sparql-results+xml
                                        if (contentType.StartsWith("application/xml"))
                                        {
                                            try
                                            {
                                                ISparqlResultsReader resreader =
                                                    MimeTypesHelper.GetSparqlParser("application/sparql-results+xml");
                                                resreader.Load(resultsHandler, data);
                                                callback(this,
                                                    new AsyncStorageCallbackArgs(
                                                        AsyncStorageOperation.SparqlQueryWithHandler, sparqlQuery,
                                                        rdfHandler, resultsHandler), state);

                                            }
                                            catch (RdfParserSelectionException)
                                            {
                                                // Ignore this and fall back to trying as an RDF format instead
                                            }
                                        }

                                        // Is the Content Type referring to a RDF format?
                                        IRdfReader rdfreader = MimeTypesHelper.GetParser(contentType);
                                        if (q != null && (SparqlSpecsHelper.IsSelectQuery(q.QueryType) ||
                                                          q.QueryType == SparqlQueryType.Ask))
                                        {
                                            var resreader = new SparqlRdfParser(rdfreader);
                                            resreader.Load(resultsHandler, data);
                                        }
                                        else
                                        {
                                            rdfreader.Load(rdfHandler, data);
                                        }

                                        callback(this,
                                            new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                                                sparqlQuery, rdfHandler, resultsHandler), state);
                                    }
                                }
                            });
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                    StorageHelper.HandleError(ex, "querying")), state);
            }
        }

        /// <summary>
        /// Helper method for creating HTTP Requests to the Store.
        /// </summary>
        /// <param name="servicePath">Path to the Service requested.</param>
        /// <param name="accept">Acceptable Content Types.</param>
        /// <param name="method">HTTP Method.</param>
        /// <param name="queryParams">Querystring Parameters.</param>
        /// <returns></returns>
        [Obsolete("This method is obsolete and will be removed in a future release. Use the overload that returns an HttpRequestMessage instead.")]
        protected virtual HttpWebRequest CreateRequest(string servicePath, string accept, string method, Dictionary<string, string> queryParams)
        {
            // Build the Request Uri
            var requestUri = _baseUri + servicePath;
            if (queryParams != null)
            {
                if (queryParams.Count > 0)
                {
                    requestUri += "?";
                    foreach (var p in queryParams.Keys)
                    {
                        requestUri += p + "=" + HttpUtility.UrlEncode(queryParams[p]) + "&";
                    }
                    requestUri = requestUri.Substring(0, requestUri.Length - 1);
                }
            }

            // Create our Request
            var request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Accept = accept;
            request.Method = method;
            return request;
            //return ApplyRequestOptions(request);
        }

        /// <summary>
        /// Helper method for creating HTTP Requests to the Store.
        /// </summary>
        /// <param name="servicePath">Path to the Service requested.</param>
        /// <param name="accept">Acceptable Content Types.</param>
        /// <param name="method">HTTP Method.</param>
        /// <param name="queryParams">Querystring Parameters.</param>
        /// <returns></returns>
        protected virtual HttpRequestMessage CreateRequest(string servicePath, string accept, HttpMethod method,
            Dictionary<string, string> queryParams)
        {
            // Build the Request Uri
            var requestUri = _baseUri + servicePath;
            if (queryParams != null)
            {
                if (queryParams.Count > 0)
                {
                    requestUri += "?";
                    foreach (var p in queryParams.Keys)
                    {
                        requestUri += p + "=" + HttpUtility.UrlEncode(queryParams[p]) + "&";
                    }
                    requestUri = requestUri.Substring(0, requestUri.Length - 1);
                }
            }

            // Create our Request
            var request = new HttpRequestMessage(method, requestUri);
            request.Headers.Add("Accept", accept);
            return request;
        }

        /// <summary>
        /// Gets a String which gives details of the Connection.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[Sesame] Store '" + _store + "' on Server '" + _baseUri + "'";
        }

        /// <summary>
        /// Serializes the connection's configuration.
        /// </summary>
        /// <param name="context">Configuration Serialization Context.</param>
        public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"));
            INode dnrType = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType));
            INode genericManager = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassStorageProvider));
            INode server = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyServer));
            INode store = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyStore));

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(GetType().FullName)));
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(_baseUri)));
            context.Graph.Assert(new Triple(manager, store, context.Graph.CreateLiteralNode(_store)));

            if (HttpClientHandler?.Credentials is NetworkCredential networkCredential)
            {
                INode username = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUser));
                INode pwd = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyPassword));
                context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(networkCredential.UserName)));
                context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(networkCredential.Password)));
            }

            SerializeStandardConfig(manager, context);
        }
    }

    /// <summary>
    /// Connector for connecting to a Store that supports the Sesame 2.0 HTTP Communication protocol.
    /// </summary>
    /// <remarks>
    /// Acts as a synonym for whatever the latest version of the Sesame HTTP Protocol that is supported by dotNetRDF might be.  Currently this is Version 6 which includes SPARQL Update support (Sesame 2.4+ required).
    /// </remarks>
    public class SesameHttpProtocolConnector
        : SesameHttpProtocolVersion6Connector
    {
        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store.
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store.</param>
        /// <param name="storeID">Store ID.</param>
        public SesameHttpProtocolConnector(string baseUri, string storeID)
            : base(baseUri, storeID) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store.
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store.</param>
        /// <param name="storeID">Store ID.</param>
        /// <param name="username">Username to use for requests that require authentication.</param>
        /// <param name="password">Password to use for requests that require authentication.</param>
        public SesameHttpProtocolConnector(string baseUri, string storeID, string username, string password)
            : base(baseUri, storeID, username, password) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store.
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store.</param>
        /// <param name="storeID">Store ID.</param>
        /// <param name="proxy">Proxy Server.</param>
        public SesameHttpProtocolConnector(string baseUri, string storeID, IWebProxy proxy)
            : base(baseUri, storeID, proxy) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store.
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store.</param>
        /// <param name="storeID">Store ID.</param>
        /// <param name="username">Username to use for requests that require authentication.</param>
        /// <param name="password">Password to use for requests that require authentication.</param>
        /// <param name="proxy">Proxy Server.</param>
        public SesameHttpProtocolConnector(string baseUri, string storeID, string username, string password, IWebProxy proxy)
            : base(baseUri, storeID, username, password, proxy) { }
    }

    /// <summary>
    /// Connector for connecting to a Store that supports the Sesame 2.0 HTTP Communication Protocol version 5 (i.e. no SPARQL Update support).
    /// </summary>
    public class SesameHttpProtocolVersion5Connector
        : BaseSesameHttpProtocolConnector
    {
        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store.
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store.</param>
        /// <param name="storeID">Store ID.</param>
        public SesameHttpProtocolVersion5Connector(string baseUri, string storeID)
            : base(baseUri, storeID) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store.
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store.</param>
        /// <param name="storeID">Store ID.</param>
        /// <param name="username">Username to use for requests that require authentication.</param>
        /// <param name="password">Password to use for requests that require authentication.</param>
        public SesameHttpProtocolVersion5Connector(string baseUri, string storeID, string username, string password)
            : base(baseUri, storeID, username, password) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store.
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store.</param>
        /// <param name="storeID">Store ID.</param>
        /// <param name="proxy">Proxy Server.</param>
        public SesameHttpProtocolVersion5Connector(string baseUri, string storeID, IWebProxy proxy)
            : base(baseUri, storeID, proxy) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store.
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store.</param>
        /// <param name="storeID">Store ID.</param>
        /// <param name="username">Username to use for requests that require authentication.</param>
        /// <param name="password">Password to use for requests that require authentication.</param>
        /// <param name="proxy">Proxy Server.</param>
        public SesameHttpProtocolVersion5Connector(string baseUri, string storeID, string username, string password, IWebProxy proxy)
            : base(baseUri, storeID, username, password, proxy) { }

    }

    /// <summary>
    /// Connector for connecting to a Store that supports the Sesame 2.0 HTTP Communication Protocol version 6 (i.e. includes SPARQL Update support).
    /// </summary>
    public class SesameHttpProtocolVersion6Connector
        : SesameHttpProtocolVersion5Connector, IUpdateableStorage
    {
        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store.
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store.</param>
        /// <param name="storeID">Store ID.</param>
        public SesameHttpProtocolVersion6Connector(string baseUri, string storeID)
            : base(baseUri, storeID) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store.
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store.</param>
        /// <param name="storeID">Store ID.</param>
        /// <param name="username">Username to use for requests that require authentication.</param>
        /// <param name="password">Password to use for requests that require authentication.</param>
        public SesameHttpProtocolVersion6Connector(string baseUri, string storeID, string username, string password)
            : base(baseUri, storeID, username, password) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store.
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store.</param>
        /// <param name="storeID">Store ID.</param>
        /// <param name="proxy">Proxy Server.</param>
        public SesameHttpProtocolVersion6Connector(string baseUri, string storeID, IWebProxy proxy)
            : base(baseUri, storeID, proxy) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store.
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store.</param>
        /// <param name="storeID">Store ID.</param>
        /// <param name="username">Username to use for requests that require authentication.</param>
        /// <param name="password">Password to use for requests that require authentication.</param>
        /// <param name="proxy">Proxy Server.</param>
        public SesameHttpProtocolVersion6Connector(string baseUri, string storeID, string username, string password, IWebProxy proxy)
            : base(baseUri, storeID, username, password, proxy) { }

        /// <summary>
        /// Makes a SPARQL Update request to the Sesame server.
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update.</param>
        public virtual void Update(string sparqlUpdate)
        {
            try
            {
                // Create the Request
                HttpRequestMessage request = CreateRequest(_repositoriesPrefix + _store + _updatePath,
                    MimeTypesHelper.Any, HttpMethod.Post, new Dictionary<string, string>());

                // Build the Post Data and add to the Request Body
                request.Content =
                    new FormUrlEncodedContent(new[] {new KeyValuePair<string, string>("update", sparqlUpdate)});

                // Get the Response and process based on the Content Type
                using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode)
                {
                    throw StorageHelper.HandleHttpError(response, "updating");
                }
            }
            catch (Exception ex)
            {
                throw StorageHelper.HandleError(ex, "updating");
            }
        }

        /// <summary>
        /// Makes a SPARQL Update request to the Sesame server.
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="state">State to pass to the callback.</param>
        public virtual void Update(string sparqlUpdate, AsyncStorageCallback callback, object state)
        {
            try
            {
                // Create the Request
                HttpRequestMessage request = CreateRequest(_repositoriesPrefix + _store + _updatePath,
                    MimeTypesHelper.Any, HttpMethod.Post, new Dictionary<string, string>());

                // Build the Post Data and add to the Request Body
                request.Content =
                    new FormUrlEncodedContent(new[] {new KeyValuePair<string, string>("update", sparqlUpdate)});
                HttpClient.SendAsync(request).ContinueWith(requestTask =>
                {
                    if (requestTask.IsCanceled || requestTask.IsFaulted)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate,
                                requestTask.IsCanceled
                                    ? new RdfStorageException("The operation was cancelled.")
                                    : StorageHelper.HandleError(requestTask.Exception, "updating")),
                            state);
                    }
                    else
                    {
                        HttpResponseMessage response = requestTask.Result;
                        if (!response.IsSuccessStatusCode)
                        {
                            callback(this,
                                new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate,
                                    StorageHelper.HandleHttpError(response, "updating")),
                                state);
                        }
                        else
                        {
                            callback(this,
                                new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate), state);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate,
                        StorageHelper.HandleError(ex, "updating")), state);
            }
        }
    }

}