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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage;

/// <summary>
/// Class for connecting to 4store.
/// </summary>
/// <remarks>
/// <para>
/// Depending on the version of <a href="http://librdf.org/rasqal/">RASQAL</a> used for your 4store instance and the options it was built with some kinds of queries may not suceed or return unexpected results.
/// </para>
/// <para>
/// Prior to the 1.x releases 4store did not permit the saving of unamed Graphs to the Store or Triple level updates.  There was a branch of 4store that supports Triple level updates and you could tell the connector if your 4store instance supports this when you instantiate it.  From the 0.4.0 release of the library onwards this support was enabled by default since the 1.x builds of 4store have this feature integrated into them by default.
/// </para>
/// </remarks>
public class FourStoreConnector
    : BaseAsyncHttpConnector, IAsyncUpdateableStorage, IConfigurationSerializable
    , IUpdateableStorage
{
    private readonly string _baseUri;
    private readonly SparqlQueryClient _queryClient;
    private readonly SparqlUpdateClient _updateClient;
    private readonly bool _updatesEnabled = true;
    private readonly SparqlFormatter _formatter = new SparqlFormatter();

    /// <summary>
    /// Creates a new 4store connector which manages access to the services provided by a 4store server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the 4store.</param>
    /// <remarks>
    /// <strong>Note:</strong> As of the 0.4.0 release 4store support defaults to Triple Level updates enabled as all recent 4store releases have supported this.  You can still optionally disable this with the two argument version of the constructor.
    /// </remarks>
    public FourStoreConnector(string baseUri) 
    {
        if (baseUri == null) throw new ArgumentNullException(nameof(baseUri));
        // Determine the appropriate actual Base Uri
        if (baseUri.EndsWith("sparql/"))
        {
            _baseUri = baseUri.Substring(0, baseUri.IndexOf("sparql/", StringComparison.Ordinal));
        }
        else if (baseUri.EndsWith("data/"))
        {
            _baseUri = baseUri.Substring(0, baseUri.IndexOf("data/", StringComparison.Ordinal));
        }
        else if (!baseUri.EndsWith("/"))
        {
            _baseUri = baseUri + "/";
        }
        else
        {
            _baseUri = baseUri;
        }

        _queryClient = new SparqlQueryClient(HttpClient, UriFactory.Create(_baseUri + "sparql/"));
        _updateClient = new SparqlUpdateClient(HttpClient, UriFactory.Create(_baseUri + "update/"));
        HttpClient.Timeout = TimeSpan.FromMilliseconds(60000);
    }

    /// <summary>
    /// Creates a new 4store connector which manages access to the services provided by a 4store server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the 4store.</param>
    /// <param name="enableUpdateSupport">Indicates to the connector that you are using a 4store instance that supports Triple level updates.</param>
    /// <remarks>
    /// If you enable Update support but are using a 4store instance that does not support Triple level updates then you will almost certainly experience errors while using the connector.
    /// </remarks>
    public FourStoreConnector(string baseUri, bool enableUpdateSupport)
        : this(baseUri)
    {
        _updatesEnabled = enableUpdateSupport;
    }

    /// <summary>
    /// Creates a new 4store connector which manages access to the services provided by a 4store server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the 4store.</param>
    /// <param name="proxy">Proxy Server.</param>
    /// <remarks>
    /// <strong>Note:</strong> As of the 0.4.0 release 4store support defaults to Triple Level updates enabled as all recent 4store releases have supported this.  You can still optionally disable this with the two argument version of the constructor.
    /// </remarks>
    public FourStoreConnector(string baseUri, IWebProxy proxy)
        : this(baseUri)
    {
        Proxy = proxy;
    }

    /// <summary>
    /// Creates a new 4store connector which manages access to the services provided by a 4store server.
    /// </summary>
    /// <param name="baseUri">Base Uri of the 4store.</param>
    /// <param name="enableUpdateSupport">Indicates to the connector that you are using a 4store instance that supports Triple level updates.</param>
    /// <param name="proxy">Proxy Server.</param>
    /// <remarks>
    /// If you enable Update support but are using a 4store instance that does not support Triple level updates then you will almost certainly experience errors while using the connector.
    /// </remarks>
    public FourStoreConnector(string baseUri, bool enableUpdateSupport, IWebProxy proxy)
        : this(baseUri, enableUpdateSupport)
    {
        Proxy = proxy;
    }

    /// <summary>
    /// Returns whether this connector has been instantiated with update support or not.
    /// </summary>
    /// <remarks>
    /// If this property returns true it does not guarantee that the 4store instance actually supports updates it simply indicates that the user has enabled updates on the connector.  If Updates are enabled and the 4store server being connected to does not support updates then errors will occur.
    /// </remarks>
    public override bool UpdateSupported
    {
        get
        {
            return false;
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
    /// Gets the IO Behaviour of 4store.
    /// </summary>
    public override IOBehaviour IOBehaviour
    {
        get
        {
            return IOBehaviour.IsQuadStore | IOBehaviour.HasNamedGraphs | IOBehaviour.OverwriteNamed | IOBehaviour.CanUpdateTriples;
        }
    }

    /// <summary>
    /// Returns that deleting Graph is supported.
    /// </summary>
    public override bool DeleteSupported
    {
        get
        {
            return true;
        }
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
    /// Loads a Graph from the 4store instance.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="graphUri">Uri of the Graph to load.</param>
    public void LoadGraph(IGraph g, Uri graphUri)
    {
        LoadGraph(g, graphUri.ToSafeString());
    }

    /// <summary>
    /// Loads a Graph from the 4store instance using an RDF Handler.
    /// </summary>
    /// <param name="handler">RDF Handler.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    public void LoadGraph(IRdfHandler handler, Uri graphUri)
    {
        LoadGraph(handler, graphUri.ToSafeString());
    }

    /// <summary>
    /// Loads a Graph from the 4store instance.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    public void LoadGraph(IGraph g, string graphUri)
    {
        if (g.IsEmpty && graphUri != null & !graphUri.Equals(string.Empty))
        {
            g.BaseUri = UriFactory.Create(graphUri);
        }
        LoadGraph(new GraphHandler(g), graphUri);
    }

    /// <summary>
    /// Loads a Graph from the 4store instance.
    /// </summary>
    /// <param name="handler">RDF Handler.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    public void LoadGraph(IRdfHandler handler, string graphUri)
    {
        if (!graphUri.Equals(string.Empty))
        {
            _queryClient.QueryWithResultGraphAsync("CONSTRUCT { ?s ?p ?o } FROM <" + graphUri.Replace(">", "\\>") + "> WHERE { ?s ?p ?o }", handler).Wait();
        }
        else
        {
            throw new RdfStorageException("Cannot retrieve a Graph from 4store without specifying a Graph URI");
        }
    }

    /// <summary>
    /// Saves a Graph to a 4store instance (Warning: Completely replaces any existing Graph with the same URI).
    /// </summary>
    /// <param name="g">Graph to save.</param>
    /// <remarks>
    /// <para>
    /// Completely replaces any existing Graph with the same Uri in the store.
    /// </para>
    /// <para>
    /// Attempting to save a Graph which doesn't have a Base Uri will result in an error.
    /// </para>
    /// </remarks>
    /// <exception cref="RdfStorageException">Thrown if you try and save a Graph without a Base Uri or if there is an error communicating with the 4store instance.</exception>
    public void SaveGraph(IGraph g)
    {
        if (g.Name == null)
        {
            throw new RdfStorageException("Cannot save a Graph without a Base URI to a 4store Server");
        }

        var requestUri = new Uri(_baseUri + "data/" + Uri.EscapeUriString(g.Name.ToSafeString()));
        var request = new HttpRequestMessage(HttpMethod.Put, requestUri)
        {
            Content = new GraphContent(g, new CompressingTurtleWriter(WriterCompressionLevel.High))
            {
                Encoding = new UTF8Encoding(false),
                ContentLengthRequired = true,
            },
        };
        HttpResponseMessage response = HttpClient.SendAsync(request).Result;
        if (!response.IsSuccessStatusCode)
        {
            throw StorageHelper.HandleHttpError(response, "saving a Graph to");
        }
    }

    /// <summary>
    /// Updates a Graph in the store.
    /// </summary>
    /// <param name="graphUri">Uri of the Graph to Update.</param>
    /// <param name="additions">Triples to be added.</param>
    /// <param name="removals">Triples to be removed.</param>
    /// <remarks>
    /// May throw an error since the default builds of 4store don't support Triple level updates.  There are builds that do support this and the user can instantiate the connector with support for this enabled if they wish, if they do so and the underlying 4store doesn't support updates errors will occur when updates are attempted.
    /// </remarks>
    public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
    {
        UpdateGraph(graphUri.ToSafeString(), additions, removals);
    }

    /// <summary>
    /// Updates a graph in the store.
    /// </summary>
    /// <param name="graphName">Name of the graph to update.</param>
    /// <param name="additions">Triples to be added.</param>
    /// <param name="removals">Triples to be removed.</param>
    /// <remarks>
    /// May throw an error since the default builds of 4store don't support Triple level updates.  There are builds that do support this and the user can instantiate the connector with support for this enabled if they wish, if they do so and the underlying 4store doesn't support updates errors will occur when updates are attempted.
    /// </remarks>
    public void UpdateGraph(IRefNode graphName, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
    {
        UpdateGraph(graphName.ToSafeString(), additions, removals);
    }

    /// <summary>
    /// Updates a Graph in the store.
    /// </summary>
    /// <param name="graphUri">Uri of the Graph to Update.</param>
    /// <param name="additions">Triples to be added.</param>
    /// <param name="removals">Triples to be removed.</param>
    /// <remarks>
    /// May throw an error since the default builds of 4store don't support Triple level updates.  There are builds that do support this and the user can instantiate the connector with support for this enabled if they wish, if they do so and the underlying 4store doesn't support updates errors will occur when updates are attempted.
    /// </remarks>
    public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
    {
        if (!_updatesEnabled)
        {
            throw new RdfStorageException("4store does not support Triple level updates");
        }
        else if (graphUri.Equals(string.Empty))
        {
            throw new RdfStorageException("Cannot update a Graph without a Graph URI on a 4store Server");
        }
        else
        {
            try
            {
                var delete = new StringBuilder();
                if (removals != null)
                {
                    if (removals.Any())
                    {
                        // Build up the DELETE command and execute
                        delete.AppendLine("DELETE DATA");
                        delete.AppendLine("{ GRAPH <" + graphUri.Replace(">", "\\>") + "> {");
                        foreach (Triple t in removals)
                        {
                            delete.AppendLine(t.ToString(_formatter));
                        }
                        delete.AppendLine("}}");
                    }
                }

                var insert = new StringBuilder();
                if (additions != null)
                {
                    if (additions.Any())
                    {
                        // Build up the INSERT command and execute
                        insert.AppendLine("INSERT DATA");
                        insert.AppendLine("{ GRAPH <" + graphUri.Replace(">", "\\>") + "> {");
                        foreach (Triple t in additions)
                        {
                            insert.AppendLine(t.ToString(_formatter));
                        }
                        insert.AppendLine("}}");
                    }
                }

                // Use Update() method to send the updates
                if (delete.Length > 0)
                {
                    if (insert.Length > 0)
                    {
                        Update(delete + "\n;\n"  + insert);
                    }
                    else
                    {
                        Update(delete.ToString());
                    }
                }
                else if (insert.Length > 0)
                {
                    Update(insert.ToString());
                }
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpError(webEx, "updating a Graph in");
            }
        }
    }

    /// <summary>
    /// Makes a SPARQL Query against the underlying 4store Instance.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <returns>A <see cref="Graph">Graph</see> or a <see cref="SparqlResultSet">SparqlResultSet</see>.</returns>
    /// <remarks>
    /// Depending on the version of <a href="http://librdf.org/rasqal/">RASQAL</a> used and the options it was built with some kinds of queries may not suceed or return unexpected results.
    /// </remarks>
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
    /// Makes a SPARQL Query against the underlying 4store Instance processing the results with the appropriate handler from those provided.
    /// </summary>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
    {
        _queryClient.QueryAsync(sparqlQuery, rdfHandler, resultsHandler, CancellationToken.None).Wait();
    }

    /// <summary>
    /// Deletes a Graph from the 4store server.
    /// </summary>
    /// <param name="graphUri">Uri of Graph to delete.</param>
    public void DeleteGraph(Uri graphUri)
    {
        if (graphUri == null)
        {
            throw new RdfStorageException("You must specify a valid URI in order to delete a Graph from 4store");
        }
        else
        {
            DeleteGraph(graphUri.AbsoluteUri);
        }
    }

    /// <summary>
    /// Deletes a Graph from the 4store server.
    /// </summary>
    /// <param name="graphUri">Uri of Graph to delete.</param>
    public void DeleteGraph(string graphUri)
    {
        if (string.IsNullOrEmpty(graphUri))
        {
            throw new RdfStorageException("Cannot delete a Graph without a Base URI from a 4store Server");
        }

        var requestUri = _baseUri + "data/" + Uri.EscapeUriString(graphUri);
        HttpResponseMessage response = HttpClient.DeleteAsync(requestUri).Result;
        if (!response.IsSuccessStatusCode)
        {
            StorageHelper.HandleHttpError(response, "deleting a Graph from");
        }
    }

    /// <summary>
    /// Lists the Graphs in the Store.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Uri> ListGraphs()
    {
        try
        {
            if (Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }") is SparqlResultSet resultSet)
            {
                var graphs = new List<Uri>();
                foreach (SparqlResult r in resultSet.Where(x=>x.HasValue("g")))
                {
                    INode temp = r["g"];
                    if (temp.NodeType == NodeType.Uri)
                    {
                        graphs.Add(((IUriNode)temp).Uri);
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
    /// Gets an enumeration of the names of the graphs in the store.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// Implementations should implement this method only if they need to provide a custom way of listing Graphs.  If the Store for which you are providing a manager can efficiently return the Graphs using a SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } } query then there should be no need to implement this function.
    /// </para>
    /// </remarks>
    public IEnumerable<string> ListGraphNames()
    {
        try
        {
            if (Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }") is SparqlResultSet resultSet)
            {
                var graphs = new List<string>();
                foreach (SparqlResult r in resultSet.Where(x => x.HasValue("g")))
                {
                    INode temp = r["g"];
                    if (temp.NodeType == NodeType.Uri)
                    {
                        graphs.Add(((IUriNode)temp).Uri.AbsoluteUri);
                    }
                    else if (temp.NodeType == NodeType.Blank)
                    {
                        graphs.Add("_:" + ((IBlankNode)temp).InternalID);
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
    /// Applies a SPARQL Update against 4store.
    /// </summary>
    /// <param name="sparqlUpdate">SPARQL Update.</param>
    /// <remarks>
    /// <strong>Note:</strong> Please be aware that some valid SPARQL Updates may not be accepted by 4store since the SPARQL parser used by 4store does not support some of the latest editors draft syntax changes.
    /// </remarks>
    public void Update(string sparqlUpdate)
    {
        _updateClient.UpdateAsync(sparqlUpdate).Wait();
    }

    /// <summary>
    /// Saves a Graph to the Store asynchronously.
    /// </summary>
    /// <param name="g">Graph to save.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public override void SaveGraph(IGraph g, AsyncStorageCallback callback, object state)
    {
        HttpRequestMessage request = MakeSaveGraphRequestMessage(g);
        SaveGraphAsync(request, g, callback, state);
    }

    /// <inheritdoc />
    public override Task SaveGraphAsync(IGraph g, CancellationToken cancellationToken)
    {
        HttpRequestMessage request = MakeSaveGraphRequestMessage(g);
        return SaveGraphAsync(request, cancellationToken);
    }

    private HttpRequestMessage MakeSaveGraphRequestMessage(IGraph g)
    {
        // Set up the Request
        if (g.Name == null)
        {
            throw new RdfStorageException("Cannot save a Graph without a name to a 4store Server");
        }

        var request = new HttpRequestMessage(HttpMethod.Put, _baseUri + "data/" + g.Name);
        var writer = new CompressingTurtleWriter(WriterCompressionLevel.High);

        // Write the Graph as Turtle to the Request Stream
        request.Content =
            new GraphContent(g, writer) {Encoding = new UTF8Encoding(false), ContentLengthRequired = true};
        return request;
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
        if (!graphUri.Equals(string.Empty))
        {
            _queryClient
                .QueryWithResultGraphAsync(
                    "CONSTRUCT { ?s ?p ?o } FROM <" + graphUri.Replace(">", "\\>") + "> WHERE { ?s ?p ?o }",
                    handler)
                .ContinueWith(queryTask =>
                {
                    if (queryTask.IsFaulted)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler,
                                StorageHelper.HandleError(queryTask.Exception, "retrieving a Graph from")), state);
                    }
                    else if (queryTask.IsCanceled)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler,
                                new RdfStorageException("Operation was cancelled.")), state);
                    }
                    else
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler),
                            state);
                    }
                });
        }
        else
        {
            callback(this,
                new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler,
                    new RdfStorageException("Cannot retrieve a Graph from 4store without specifying a Graph URI")),
                state);
        }
    }

    /// <inheritdoc />
    public override Task LoadGraphAsync(IRdfHandler handler, string graphName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(graphName))
        {
            throw new RdfStorageException("Cannot retrieve a Graph from 4store without specifying a Graph URI");
        }

        return _queryClient.QueryWithResultGraphAsync(
            "CONSTRUCT { ?s ?p ?o } FROM <" + graphName.Replace(">", "\\>") + "> WHERE { ?s ?p ?o }",
            handler, cancellationToken);
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
        if (!_updatesEnabled)
        {
            throw new RdfStorageException("4store does not support Triple level updates");
        }
        else if (graphUri.Equals(string.Empty))
        {
            throw new RdfStorageException("Cannot update a Graph without a Graph URI on a 4store Server");
        }
        else
        {
            try
            {
                StringBuilder delete = MakeDeleteCommand(graphUri, removals);

                StringBuilder insert = MakeInsertCommand(graphUri, additions);

                // Use Update() method to send the updates
                if (delete.Length > 0)
                {
                    if (insert.Length > 0)
                    {
                        Update(delete + "\n;\n" + insert, (sender, args, st) =>
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(), args.Error), state);
                            }, state);
                    }
                    else
                    {
                        Update(delete.ToString(), (sender, args, st) =>
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(), args.Error), state);
                            }, state);
                    }
                }
                else if (insert.Length > 0)
                {
                    Update(insert.ToString(), (sender, args, st) =>
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(), args.Error), state);
                    }, state);
                }
                else
                {
                    // Nothing to do
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
                }
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(), StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")), state);
            }
        }
    }

    /// <inheritdoc />
    public override Task UpdateGraphAsync(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals,
        CancellationToken cancellationToken)
    {
        if (!_updatesEnabled)
        {
            throw new RdfStorageException("4store does not support Triple level updates");
        }

        if (string.IsNullOrEmpty(graphUri))
        {
            throw new RdfStorageException("Cannot update a Graph without a Graph URI on a 4store Server");
        }

        StringBuilder delete = MakeDeleteCommand(graphUri, removals);
        StringBuilder insert = MakeInsertCommand(graphUri, additions);
        if (delete.Length > 0)
        {
            return insert.Length > 0 ? UpdateAsync(delete + "\n;\n" + insert, cancellationToken) : UpdateAsync(delete.ToString(), cancellationToken);
        }

        return insert.Length > 0 ? UpdateAsync(insert.ToString(), cancellationToken) : Task.CompletedTask;
    }

    private StringBuilder MakeInsertCommand(string graphUri, IEnumerable<Triple> additions)
    {
        var insert = new StringBuilder();
        if (additions != null)
        {
            if (additions.Any())
            {
                // Build up the INSERT command and execute
                insert.AppendLine("INSERT DATA");
                insert.AppendLine("{ GRAPH <" + graphUri.Replace(">", "\\>") + "> {");
                foreach (Triple t in additions)
                {
                    insert.AppendLine(t.ToString(_formatter));
                }

                insert.AppendLine("}}");
            }
        }

        return insert;
    }

    private StringBuilder MakeDeleteCommand(string graphUri, IEnumerable<Triple> removals)
    {
        var delete = new StringBuilder();
        if (removals != null)
        {
            if (removals.Any())
            {
                // Build up the DELETE command and execute
                delete.AppendLine("DELETE DATA");
                delete.AppendLine("{ GRAPH <" + graphUri.Replace(">", "\\>") + "> {");
                foreach (Triple t in removals)
                {
                    delete.AppendLine(t.ToString(_formatter));
                }

                delete.AppendLine("}}");
            }
        }

        return delete;
    }

    /// <summary>
    /// Deletes a Graph from the Store.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to delete.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public override void DeleteGraph(string graphUri, AsyncStorageCallback callback, object state)
    {
        if (string.IsNullOrEmpty(graphUri))
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, new RdfStorageException("Cannot delete a Graph without a Base URI from a 4store Server")), state);
            return;
        }

        var request = new HttpRequestMessage(HttpMethod.Delete, _baseUri + "data/" + Uri.EscapeUriString(graphUri));
        DeleteGraphAsync(request, false, graphUri, callback, state);
    }

    /// <inheritdoc />
    public override Task DeleteGraphAsync(string graphName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(graphName))
        {
            throw new RdfStorageException("Cannot delete a graph without a base URI from a 4store server.");
        }
        var request = new HttpRequestMessage(HttpMethod.Delete, _baseUri + "data/" + Uri.EscapeUriString(graphName));
        return DeleteGraphAsync(request, false, cancellationToken);
    }

    /// <summary>
    /// Updates the store asynchronously.
    /// </summary>
    /// <param name="sparqlUpdates">SPARQL Update.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public void Update(string sparqlUpdates, AsyncStorageCallback callback, object state)
    {
        _updateClient.UpdateAsync(sparqlUpdates).ContinueWith(updateTask =>
        {
            if (updateTask.IsCanceled)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate,
                        new RdfStorageException("Operation was cancelled.")), state);
            }
            else if (updateTask.IsFaulted)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate,
                    StorageHelper.HandleError(updateTask.Exception, "executing SPARQL update on")), state);
            }
            else
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdates),
                    state);
            }
        }).ConfigureAwait(true);
    }

    /// <inheritdoc />
    public Task UpdateAsync(string sparqlUpdates, CancellationToken cancellationToken)
    {
        return _updateClient.UpdateAsync(sparqlUpdates, cancellationToken);
    }

    /// <summary>
    /// Queries the store asynchronously.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
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

    /// <inheritdoc />
    public async Task<object> QueryAsync(string sparqlQuery, CancellationToken cancellationToken)
    {
        var g = new Graph();
        var results = new SparqlResultSet();
        await QueryAsync(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery, cancellationToken);
        return results.ResultsType == SparqlResultsType.Unknown ? (object)g : results;
    }
    /// <summary>
    /// Queries the store asynchronously.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery,
        AsyncStorageCallback callback, object state)
    {
        _queryClient.QueryAsync(sparqlQuery, rdfHandler, resultsHandler, CancellationToken.None).ContinueWith(
            queryTask =>
            {
                if (queryTask.IsFaulted)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                            StorageHelper.HandleError(queryTask.Exception, "executing a query on")), state);
                }
                else if (queryTask.IsCanceled)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                            new RdfStorageException("Operation was cancelled.")), state);
                }
                else
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, sparqlQuery,
                            rdfHandler, resultsHandler), state);
            }).ConfigureAwait(true);
    }

    /// <inheritdoc />
    public Task QueryAsync(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery,
        CancellationToken cancellationToken)
    {
        return _queryClient.QueryAsync(sparqlQuery, rdfHandler, resultsHandler, cancellationToken);
    }

    /// <summary>
    /// Gets a String which gives details of the Connection.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "[4store] " + _baseUri;
    }

    /// <summary>
    /// Serializes the connection's configuration.
    /// </summary>
    /// <param name="context"></param>
    public void SerializeConfiguration(ConfigurationSerializationContext context)
    {
        INode manager = context.NextSubject;
        INode rdfType = context.Graph.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfType));
        INode rdfsLabel = context.Graph.CreateUriNode(context.UriFactory.Create(NamespaceMapper.RDFS + "label"));
        INode dnrType = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyType));
        INode genericManager = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.ClassStorageProvider));
        INode server = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyServer));
        INode enableUpdates = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyEnableUpdates));

        context.Graph.Assert(new Triple(manager, rdfType, genericManager));
        context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(ToString())));
        context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(GetType().FullName)));
        context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(_baseUri)));
        context.Graph.Assert(new Triple(manager, enableUpdates, _updatesEnabled.ToLiteral(context.Graph)));

        SerializeStandardConfig(manager, context);
    }
}