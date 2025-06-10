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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage;

/// <summary>
/// Class for connecting to any dataset that can be exposed via Fuseki.
/// </summary>
/// <remarks>
/// <para>
/// Uses all three Services provided by a Fuseki instance - Query, Update and HTTP Update.
/// </para>
/// </remarks>
public class FusekiConnector 
    : SparqlHttpProtocolConnector, IAsyncUpdateableStorage, IUpdateableStorage
{
    private readonly SparqlFormatter _formatter = new SparqlFormatter();
    private readonly string _updateUri;
    private readonly string _queryUri;

    /// <summary>
    /// Creates a new connection to a Fuseki Server.
    /// </summary>
    /// <param name="serviceUri">The /data URI of the Fuseki Server.</param>
    /// <param name="writerMimeTypeDefinition">The MIME type of the syntax to use when sending RDF data to the server. Defaults to RDF/XML.</param>
    public FusekiConnector(Uri serviceUri, MimeTypeDefinition writerMimeTypeDefinition = null)
        : this(serviceUri.ToSafeString(), writerMimeTypeDefinition) { }

    /// <summary>
    /// Creates a new connection to a Fuseki Server.
    /// </summary>
    /// <param name="serviceUri">The /data URI of the Fuseki Server.</param>
    /// <param name="writerMimeTypeDefinition">The MIME type of the syntax to use when sending RDF data to the server. Defaults to RDF/XML.</param>
    public FusekiConnector(string serviceUri, MimeTypeDefinition writerMimeTypeDefinition = null)
        : base(serviceUri, writerMimeTypeDefinition) 
    {
        if (!serviceUri.EndsWith("/data")) throw new ArgumentException("This does not appear to be a valid Fuseki Server URI, you must provide the URI that ends with /data", "serviceUri");

        _updateUri = serviceUri.Substring(0, serviceUri.Length - 4) + "update";
        _queryUri = serviceUri.Substring(0, serviceUri.Length - 4) + "query";
    }

    /// <summary>
    /// Creates a new connection to a Fuseki Server.
    /// </summary>
    /// <param name="serviceUri">The /data URI of the Fuseki Server.</param>
    /// <param name="proxy">Proxy Server.</param>
    public FusekiConnector(Uri serviceUri, IWebProxy proxy)
        : this(serviceUri.ToSafeString(), proxy) { }

    /// <summary>
    /// Creates a new connection to a Fuseki Server.
    /// </summary>
    /// <param name="serviceUri">The /data URI of the Fuseki Server.</param>
    /// <param name="proxy">Proxy Server.</param>
    public FusekiConnector(string serviceUri, IWebProxy proxy)
        : this(serviceUri)
    {
        Proxy = proxy;
    }

    /// <summary>
    /// Returns that Listing Graphs is supported.
    /// </summary>
    public override bool ListGraphsSupported
    {
        get
        {
            return true;
        }
    }

    /// <summary>
    /// Gets the IO Behaviour of the Store.
    /// </summary>
    public override IOBehaviour IOBehaviour
    {
        get
        {
            return base.IOBehaviour | IOBehaviour.CanUpdateDeleteTriples;
        }
    }

    /// <summary>
    /// Returns that Triple level updates are supported using Fuseki.
    /// </summary>
    public override bool UpdateSupported
    {
        get
        {
            return true;
        }
    }

    /// <summary>
    /// Gets the List of Graphs from the store.
    /// </summary>
    /// <returns></returns>
    [Obsolete("Replaced by ListGraphNames")]
    public override IEnumerable<Uri> ListGraphs()
    {
        try
        {
            var results = Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }") as SparqlResultSet;
            if (results != null)
            {
                var uris = new List<Uri>();
                foreach (SparqlResult r in results)
                {
                    if (r.HasValue("g"))
                    {
                        INode n = r["g"];
                        if (n != null && n.NodeType == NodeType.Uri)
                        {
                            uris.Add(((IUriNode)n).Uri);
                        }
                    }
                }
                return uris;
            }
            else
            {
                throw new RdfStorageException("Tried to list graphs from Fuseki but failed to get a SPARQL Result Set as expected");
            }
        }
        catch (RdfStorageException)
        {
            throw;
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
    public override IEnumerable<string> ListGraphNames()
    {
        try
        {
            var results = Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }") as SparqlResultSet;
            if (results == null)
            {
                throw new RdfStorageException(
                    "Tried to list graphs from Fuseki but failed to get a SPARQL Result Set as expected");
            }

            var graphNames = new List<string>();
            foreach (SparqlResult r in results)
            {
                if (r.HasValue("g"))
                {
                    INode n = r["g"];
                    if (n != null)
                    {
                        switch (n.NodeType)
                        {
                            case NodeType.Uri:
                                graphNames.Add(((IUriNode)n).Uri.AbsoluteUri);
                                break;
                            case NodeType.Blank:
                                graphNames.Add(((IBlankNode)n).InternalID);
                                break;
                        }
                    }
                }
            }
            return graphNames;

        }
        catch (RdfStorageException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "listing Graphs from");
        }
    }

    /// <summary>
    /// Updates a Graph in the Fuseki store.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to update.</param>
    /// <param name="additions">Triples to be added.</param>
    /// <param name="removals">Triples to be removed.</param>
    public override void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
    {
        try
        {
            var graph = (graphUri != null && !graphUri.Equals(string.Empty))
                ? "GRAPH <" + _formatter.FormatUri(graphUri) + "> {"
                : string.Empty;
            var update = new StringBuilder();

            if (additions != null)
            {
                if (additions.Any())
                {
                    update.AppendLine("INSERT DATA {");
                    if (!graph.Equals(string.Empty)) update.AppendLine(graph);

                    foreach (Triple t in additions)
                    {
                        update.AppendLine(_formatter.Format(t));
                    }

                    if (!graph.Equals(string.Empty)) update.AppendLine("}");
                    update.AppendLine("}");
                }
            }

            if (removals != null)
            {
                if (removals.Any())
                {
                    if (update.Length > 0) update.AppendLine(";");

                    update.AppendLine("DELETE DATA {");
                    if (!graph.Equals(string.Empty)) update.AppendLine(graph);

                    foreach (Triple t in removals)
                    {
                        update.AppendLine(_formatter.Format(t));
                    }

                    if (!graph.Equals(string.Empty)) update.AppendLine("}");
                    update.AppendLine("}");
                }
            }

            if (update.Length > 0)
            {
                // Make the SPARQL Update Request
                var request = new HttpRequestMessage(HttpMethod.Post, _updateUri)
                {
                    Content = new StringContent(update.ToString(), Encoding.UTF8, MimeTypesHelper.SparqlUpdate),
                };
                using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode)
                {
                    throw StorageHelper.HandleHttpError(response, "updating a Graph in");
                }
            }
        }
        catch (RdfStorageException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "updating a Graph in");
        }
    }

    /// <summary>
    /// Updates a Graph in the Fuseki store.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to update.</param>
    /// <param name="additions">Triples to be added.</param>
    /// <param name="removals">Triples to be removed.</param>
    public override void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
    {
        UpdateGraph(graphUri.ToSafeString(), additions, removals);
    }

    /// <summary>
    /// Executes a SPARQL Query on the Fuseki store.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <returns></returns>
    public object Query(string sparqlQuery)
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
    /// Executes a SPARQL Query on the Fuseki store processing the results using an appropriate handler from those provided.
    /// </summary>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <returns></returns>
    public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
    {
        try
        {
            HttpRequestMessage request;

            // Create the Request
            var queryUri = _queryUri;
            if (sparqlQuery.Length < 2048)
            {
                queryUri += "?query=" + Uri.EscapeDataString(sparqlQuery);
                request = new HttpRequestMessage(HttpMethod.Get, queryUri);
                request.Headers.Add("Accept", MimeTypesHelper.HttpRdfOrSparqlAcceptHeader);
            }
            else
            {
                request = new HttpRequestMessage(HttpMethod.Post, queryUri);
                request.Headers.Add("Accept", MimeTypesHelper.HttpRdfOrSparqlAcceptHeader);
                request.Content =
                    new FormUrlEncodedContent(new[] {new KeyValuePair<string, string>("query", sparqlQuery)});
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
                // Is the Content Type referring to a RDF format?
                IRdfReader rdfReader = MimeTypesHelper.GetParser(ctype);
                rdfReader.Load(rdfHandler, data);
            }
            catch (RdfParserSelectionException)
            {
                // If we get a Parser selection exception then the Content Type isn't valid for a RDF Graph

                // Is the Content Type referring to a Sparql Result Set format?
                ISparqlResultsReader sparqlParser = MimeTypesHelper.GetSparqlParser(ctype, true);
                sparqlParser.Load(resultsHandler, data);
            }
        }
        catch (RdfQueryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "querying");
        }
    }

    /// <summary>
    /// Executes SPARQL Updates against the Fuseki store.
    /// </summary>
    /// <param name="sparqlUpdate">SPARQL Update.</param>
    public void Update(string sparqlUpdate)
    {
        try
        {
            // Make the SPARQL Update Request
            var request = new HttpRequestMessage(HttpMethod.Post, _updateUri)
            {
                Content = new StringContent(sparqlUpdate, Encoding.UTF8, MimeTypesHelper.SparqlUpdate),
            };
            using HttpResponseMessage response = HttpClient.SendAsync(request).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, "updating");
            }
        }
        catch (RdfStorageException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "updating");
        }
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
            if (results.ResultsType != SparqlResultsType.Unknown)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQuery, sparqlQuery, results, args.Error), state);
            }
            else
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQuery, sparqlQuery, g, args.Error), state);
            }
        }, state);
    }

    /// <summary>
    /// Executes a SPARQL Query on the Fuseki store processing the results using an appropriate handler from those provided.
    /// </summary>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    /// <returns></returns>
    public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery,
        AsyncStorageCallback callback, object state)
    {
        try
        {
            HttpRequestMessage request = CreateQueryRequestMessage(sparqlQuery);
            HttpClient.SendAsync(request).ContinueWith(requestTask =>
            {
                if (requestTask.IsCanceled || requestTask.IsFaulted)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                            requestTask.IsCanceled
                                ? new RdfStorageException("The operation was cancelled")
                                : StorageHelper.HandleError(requestTask.Exception, "querying")),
                        state);
                }
                else
                {
                    HttpResponseMessage response = requestTask.Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                                StorageHelper.HandleHttpError(response, "querying")),
                            state);
                    }
                    else
                    {
                        response.Content.ReadAsStreamAsync().ContinueWith(readTask =>
                        {
                            if (readTask.IsCanceled || readTask.IsFaulted)
                            {
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                                        readTask.IsCanceled
                                            ? new RdfStorageException("The operation was cancelled")
                                            : StorageHelper.HandleError(readTask.Exception, "querying")),
                                    state);
                            }
                            else
                            {
                                try
                                {
                                    var data = new StreamReader(readTask.Result);
                                    var ctype = response.Content.Headers.ContentType.MediaType;
                                    try
                                    {
                                        // Is the Content Type referring to a Sparql Result Set format?
                                        ISparqlResultsReader resreader =
                                            MimeTypesHelper.GetSparqlParser(ctype, true);
                                        resreader.Load(resultsHandler, data);
                                    }
                                    catch (RdfParserSelectionException)
                                    {
                                        // If we get a Parse exception then the Content Type isn't valid for a Sparql Result Set

                                        // Is the Content Type referring to a RDF format?
                                        IRdfReader rdfreader = MimeTypesHelper.GetParser(ctype);
                                        rdfreader.Load(rdfHandler, data);
                                    }

                                    callback(this,
                                        new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                                            sparqlQuery, rdfHandler, resultsHandler), state);
                                }
                                catch (Exception ex)
                                {
                                    callback(this,
                                        new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                                            StorageHelper.HandleError(ex, "querying")),
                                        state);
                                }
                            }
                        });
                    }
                }
            }).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            callback(this,
                new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                    StorageHelper.HandleError(ex, "querying")),
                state);
        }
    }

    private HttpRequestMessage CreateQueryRequestMessage(string sparqlQuery)
    {
        // Create the Request, always use POST for async for simplicity
        var queryUri = _queryUri;

        var request = new HttpRequestMessage(HttpMethod.Post, queryUri);
        request.Headers.Add("Accept", MimeTypesHelper.HttpRdfOrSparqlAcceptHeader);

        // Build the Post Data and add to the Request Body
        request.Content =
            new FormUrlEncodedContent(new[] {new KeyValuePair<string, string>("query", sparqlQuery)});
        return request;
    }

    /// <inheritdoc />
    public async Task<object> QueryAsync(string sparqlQuery, CancellationToken cancellationToken)
    {
        var g = new Graph();
        var results = new SparqlResultSet();
        await QueryAsync(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery, cancellationToken);
        return results.ResultsType == SparqlResultsType.Unknown ? (object)g : results;
    }

    /// <inheritdoc />
    public async Task QueryAsync(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery,
        CancellationToken cancellationToken)
    {
        HttpRequestMessage request = CreateQueryRequestMessage(sparqlQuery);
        HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw StorageHelper.HandleHttpQueryError(response);
        }

        try
        {
            var data = new StreamReader(await response.Content.ReadAsStreamAsync());
            var contentType = response.Content.Headers.ContentType.MediaType;
            try
            {
                // Is the Content Type referring to a Sparql Result Set format?
                ISparqlResultsReader resultsReader =
                    MimeTypesHelper.GetSparqlParser(contentType, true);
                resultsReader.Load(resultsHandler, data);
            }
            catch (RdfParserSelectionException)
            {
                // If we get a Parse exception then the Content Type isn't valid for a Sparql Result Set

                // Is the Content Type referring to a RDF format?
                IRdfReader rdfReader = MimeTypesHelper.GetParser(contentType);
                rdfReader.Load(rdfHandler, data);
            }
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "querying");
        }
    }

    /// <summary>
    /// Executes SPARQL Updates against the Fuseki store.
    /// </summary>
    /// <param name="sparqlUpdate">SPARQL Update.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public void Update(string sparqlUpdate, AsyncStorageCallback callback, object state)
    {
        try
        {
            // Make the SPARQL Update Request
            var request = new HttpRequestMessage(HttpMethod.Post, _updateUri)
            {
                Content = new StringContent(sparqlUpdate, Encoding.UTF8, MimeTypesHelper.SparqlUpdate),
            };
            HttpClient.SendAsync(request).ContinueWith(requestTask =>
            {
                if (requestTask.IsCanceled || requestTask.IsFaulted)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate,
                            requestTask.IsCanceled
                                ? new RdfStorageException("The operation was cancelled")
                                : StorageHelper.HandleError(requestTask.Exception, "updating")),
                        state);
                }
                else
                {
                    HttpResponseMessage response = requestTask.Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate,
                                StorageHelper.HandleHttpError(response, "updating")),
                            state);
                    }
                    else
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate), state);
                    }
                }
            }).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, StorageHelper.HandleError(ex, "updating")), state);
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(string sparqlUpdates, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _updateUri)
        {
            Content = new StringContent(sparqlUpdates, Encoding.UTF8, MimeTypesHelper.SparqlUpdate),
        };
        HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw StorageHelper.HandleHttpError(response, "updating");
        }
    }

    /// <summary>
    /// Lists the graph sin the Store asynchronously.
    /// </summary>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    [Obsolete("Replaced with ListGraphsAsync(CancellationToken)")]
    public override void ListGraphs(AsyncStorageCallback callback, object state)
    {
        // Use ListUrisHandler and make an async query to list the graphs, when that returns we invoke the correct callback
        var handler = new ListUrisHandler("g");
        ((IAsyncQueryableStorage)this).Query(null, handler, "SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }", (sender, args, st) =>
        {
            if (args.WasSuccessful)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListGraphs, handler.Uris), state);
            }
            else
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListGraphs, args.Error), state);
            }
        }, state);
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<string>> ListGraphsAsync(CancellationToken cancellationToken)
    {
        var handler = new ListUrisHandler("g");
        await QueryAsync(null, handler, "SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }", cancellationToken);
        return handler.Uris.Select(u => u?.AbsoluteUri);
    }

    /// <summary>
    /// Updates a Graph on the Fuseki Server.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to update.</param>
    /// <param name="additions">Triples to be added.</param>
    /// <param name="removals">Triples to be removed.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public override void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals,
        AsyncStorageCallback callback, object state)
    {
        try
        {
            var update = new StringBuilder();
            MakeSparqlUpdate(graphUri, additions, removals, update);

            if (update.Length > 0)
            {
                Update(update.ToString(), (sender, args, st) =>
                {
                    if (args.WasSuccessful)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()),
                            state);
                    }
                    else
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(),
                                args.Error), state);
                    }
                }, state);
            }
            else
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
            }
        }
        catch (Exception ex)
        {
            callback(this,
                new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(),
                    StorageHelper.HandleError(ex, "updating a Graph asynchronously")), state);
        }
    }

    private void MakeSparqlUpdate(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, StringBuilder update)
    {
        var graph = (graphUri != null && !graphUri.Equals(string.Empty))
            ? "GRAPH <" + _formatter.FormatUri(graphUri) + "> {"
            : string.Empty;

        if (additions != null)
        {
            if (additions.Any())
            {
                update.AppendLine("INSERT DATA {");
                if (!graph.Equals(string.Empty)) update.AppendLine(graph);

                foreach (Triple t in additions)
                {
                    update.AppendLine(_formatter.Format(t));
                }

                if (!graph.Equals(string.Empty)) update.AppendLine("}");
                update.AppendLine("}");
            }
        }

        if (removals != null)
        {
            if (removals.Any())
            {
                if (update.Length > 0) update.AppendLine(";");

                update.AppendLine("DELETE DATA {");
                if (!graph.Equals(string.Empty)) update.AppendLine(graph);

                foreach (Triple t in removals)
                {
                    update.AppendLine(_formatter.Format(t));
                }

                if (!graph.Equals(string.Empty)) update.AppendLine("}");
                update.AppendLine("}");
            }
        }
    }


    /// <inheritdoc />
    public override async Task UpdateGraphAsync(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals,
        CancellationToken cancellationToken)
    {
        var update = new StringBuilder();
        MakeSparqlUpdate(graphUri, additions, removals, update);
        if (update.Length > 0)
        {
            await UpdateAsync(update.ToString(), cancellationToken);
        }
    }

    /// <summary>
    /// Gets a String which gives details of the Connection.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "[Fuseki] " + _serviceUri;
    }

    #region IConfigurationSerializable Members

    /// <summary>
    /// Serializes the connection's configuration.
    /// </summary>
    /// <param name="context">Configuration Serialization Context.</param>
    public override void SerializeConfiguration(ConfigurationSerializationContext context)
    {
        INode manager = context.NextSubject;
        INode rdfType = context.Graph.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfType));
        INode rdfsLabel = context.Graph.CreateUriNode(context.UriFactory.Create(NamespaceMapper.RDFS + "label"));
        INode dnrType = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyType));
        INode genericManager = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.ClassStorageProvider));
        INode server = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyServer));

        context.Graph.Assert(new Triple(manager, rdfType, genericManager));
        context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(ToString())));
        context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(GetType().FullName)));
        context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(_serviceUri)));
    }

    #endregion
}