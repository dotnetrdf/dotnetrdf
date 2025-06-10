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
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage.Management;

namespace VDS.RDF.Storage;

/// <summary>
/// Abstract Base Class for HTTP Based <see cref="IAsyncStorageProvider">IAsyncStorageProvider</see> implementations.
/// </summary>
/// <remarks>
/// <para>
/// It is expected that most classes extending from this will also then implement <see cref="IStorageProvider"/> separately for their synchronous communication, this class purely provides partial helper implementations for the asynchronous communication.
/// </para>
/// </remarks>
public abstract class BaseAsyncHttpConnector
    : BaseHttpConnector, IAsyncStorageProvider
{
    /// <summary>
    /// Gets the parent server (if any).
    /// </summary>
    public virtual IStorageServer ParentServer => null;

    /// <summary>
    /// Gets the parent server (if any).
    /// </summary>
    public virtual IAsyncStorageServer AsyncParentServer => null;

    /// <summary>
    /// Create a new connector.
    /// </summary>
    /// <param name="httpClientHandler"></param>
    protected BaseAsyncHttpConnector(HttpClientHandler httpClientHandler):base(httpClientHandler){}

    /// <summary>
    /// Create a new connector.
    /// </summary>
    protected BaseAsyncHttpConnector() {}

    /// <summary>
    /// Loads a Graph from the Store asynchronously.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public virtual void LoadGraph(IGraph g, Uri graphUri, AsyncStorageCallback callback, object state)
    {
        LoadGraph(g, graphUri.ToSafeString(), callback, state);
    }

    /// <summary>
    /// Loads a Graph from the Store asynchronously.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public virtual void LoadGraph(IGraph g, string graphUri, AsyncStorageCallback callback, object state)
    {
        LoadGraph(new GraphHandler(g), graphUri, (sender, args, st) => callback(sender, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadGraph, g, args.Error), st), state);
    }

    /// <summary>
    /// Loads a Graph from the Store asynchronously.
    /// </summary>
    /// <param name="handler">Handler to load with.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public virtual void LoadGraph(IRdfHandler handler, Uri graphUri, AsyncStorageCallback callback, object state)
    {
        LoadGraph(handler, graphUri.ToSafeString(), callback, state);
    }

    /// <summary>
    /// Loads a Graph from the Store asynchronously.
    /// </summary>
    /// <param name="handler">Handler to load with.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public abstract void LoadGraph(IRdfHandler handler, string graphUri, AsyncStorageCallback callback, object state);

    /// <summary>
    /// Loads a graph from the store asynchronously.
    /// </summary>
    /// <param name="g">The graph to load data into.</param>
    /// <param name="graphName">The name of the graph to retrieve from the store.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <remarks>This implementation delegates to the <see cref="LoadGraphAsync(IRdfHandler,string,CancellationToken)"/> method by wrapping <paramref name="g"/> in a <see cref="GraphHandler"/>.</remarks>
    public virtual Task LoadGraphAsync(IGraph g, string graphName, CancellationToken cancellationToken)
    {
        return LoadGraphAsync(new GraphHandler(g), graphName, cancellationToken);
    }

    /// <inheritdoc />
    public abstract Task LoadGraphAsync(IRdfHandler handler, string graphName, CancellationToken cancellationToken);

    /// <summary>
    /// Helper method for doing async load operations, callers just need to provide an appropriately prepared HTTP request.
    /// </summary>
    /// <param name="request">HTTP Request.</param>
    /// <param name="handler">Handler to load with.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    protected internal void LoadGraphAsync(HttpRequestMessage request, IRdfHandler handler,
        AsyncStorageCallback callback, object state)
    {
        HttpClient.SendAsync(request).ContinueWith(requestTask =>
        {
            if (requestTask.IsCanceled || requestTask.IsFaulted)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler,
                        requestTask.IsCanceled
                            ? new RdfStorageException("The operation was cancelled.")
                            : StorageHelper.HandleError(requestTask.Exception,
                                "loading a Graph asynchronously from")),
                    state);
            }
            else
            {

                HttpResponseMessage response = requestTask.Result;
                if (!response.IsSuccessStatusCode)
                {
                    // Return an empty graph on a 404
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler),
                            state);
                    }
                    else
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler,
                                StorageHelper.HandleHttpError(response, "loading a Graph asynchronously from")),
                            state);
                    }
                }
                else
                {
                    try
                    {
                        IRdfReader parser =
                            MimeTypesHelper.GetParser(response.Content.Headers.ContentType.MediaType);
                        response.Content.ReadAsStreamAsync().ContinueWith(readTask =>
                        {
                            if (readTask.IsCanceled || readTask.IsFaulted)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler,
                                        handler,
                                        readTask.IsCanceled
                                            ? new RdfStorageException("The operation was cancelled.")
                                            : StorageHelper.HandleError(readTask.Exception,
                                                "loading a Graph asynchronously from")),
                                    state);
                            }
                            else
                            {
                                parser.Load(handler, new StreamReader(readTask.Result));
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler),
                                    state);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler,
                                StorageHelper.HandleError(ex, "loading a Graph asynchronously from")), state);
                    }
                }
            }
        }).ConfigureAwait(true);
    }

    /// <summary>
    /// Helper method for doing async load operations, callers just need to provide an appropriately prepared HTTP request.
    /// </summary>
    /// <param name="request">HTTP Request.</param>
    /// <param name="handler">Handler to load with.</param>
    /// <param name="cancellationToken"></param>
    protected internal async Task LoadGraphAsync(HttpRequestMessage request, IRdfHandler handler, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // Return an empty graph on a 4040
                return;
            }

            throw StorageHelper.HandleHttpError(response, "loading a graph asynchronously from");
        }

        try
        {
            IRdfReader parser =
                MimeTypesHelper.GetParser(response.Content.Headers.ContentType.MediaType);
            Stream data = await response.Content.ReadAsStreamAsync();
            parser.Load(handler, new StreamReader(data));
        }
        catch (RdfStorageException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "loading a graph asynchronously from");
        }
    }

    /// <summary>
    /// Saves a Graph to the Store asynchronously.
    /// </summary>
    /// <param name="g">Graph to save.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public abstract void SaveGraph(IGraph g, AsyncStorageCallback callback, object state);

    /// <inheritdoc />
    public abstract Task SaveGraphAsync(IGraph g, CancellationToken cancellationToken);

    /// <summary>
    /// Helper method for doing callback-based async save operations.
    /// </summary>
    /// <param name="request">A request message whose content will be set by this method.</param>
    /// <param name="writer">RDF writer to use to write graph content to the request stream.</param>
    /// <param name="g">The graph to be saved.</param>
    /// <param name="callback">The callback to invoke on completion.</param>
    /// <param name="state">State to pass to the callback.</param>
    protected internal void SaveGraphAsync(HttpRequestMessage request, IRdfWriter writer, IGraph g,
        AsyncStorageCallback callback, object state)
    {
        request.Content  = new GraphContent(g, writer);
        SaveGraphAsync(request, g, callback, state);
    }

    /// <summary>
    /// Helper method for doing callback-based async save operations.
    /// </summary>
    /// <param name="request">A request message with the request content already set.</param>
    /// <param name="g">The graph to be saved.</param>
    /// <param name="callback">The callback to invoke on completion.</param>
    /// <param name="state">State to pass to the callback.</param>
    protected internal void SaveGraphAsync(HttpRequestMessage request, IGraph g, AsyncStorageCallback callback, object state)
    {
        HttpClient.SendAsync(request).ContinueWith(sendTask =>
        {
            if (sendTask.IsFaulted)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g,
                        StorageHelper.HandleError(sendTask.Exception, "saving a Graph asynchronously to")), state);
            }

            if (sendTask.IsCanceled)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g,
                        new RdfStorageException("Operation was cancelled.")), state);
            }

            HttpResponseMessage responseMessage = sendTask.Result;
            if (!responseMessage.IsSuccessStatusCode)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g,
                    StorageHelper.HandleHttpError(responseMessage, "saving a Graph asynchronously to")), state);
            }

            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g), state);
        }).ConfigureAwait(true);
    }

    /// <summary>
    /// Helper method for doing async save operations.
    /// </summary>
    /// <param name="request">A request message with the request content already set.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async internal Task SaveGraphAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw StorageHelper.HandleHttpError(response, "saving a Graph asynchronously to");
            }
        }
        catch (RdfStorageException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "saving a Graph asynchronously to");
        }
    }

    /// <summary>
    /// Helper method for doing async save operations, callers just need to provide an appropriately prepared HTTP requests and a RDF writer which will be used to write the data to the request body.
    /// </summary>
    /// <param name="request">HTTP request.</param>
    /// <param name="writer">RDF Writer.</param>
    /// <param name="g">Graph to save.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    [Obsolete("This method is obsolete and will be removed in a future release.")]
    protected internal void SaveGraphAsync(HttpWebRequest request, IRdfWriter writer, IGraph g, AsyncStorageCallback callback, object state)
    {
        request.BeginGetRequestStream(r =>
        {
            try
            {
                Stream reqStream = request.EndGetRequestStream(r);
                writer.Save(g, new StreamWriter(reqStream));

                request.BeginGetResponse(r2 =>
                {
                    try
                    {
                        var response = (HttpWebResponse)request.EndGetResponse(r2);
                        // If we get here then it was OK
                        response.Close();
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g), state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g, StorageHelper.HandleHttpError(webEx, "saving a Graph asynchronously to")), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g, StorageHelper.HandleError(ex, "saving a Graph asynchronously to")), state);
                    }
                }, state);
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g, StorageHelper.HandleHttpError(webEx, "saving a Graph asynchronously to")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g, StorageHelper.HandleError(ex, "saving a Graph asynchronously to")), state);
            }
        }, state);
    }

    /// <summary>
    /// Updates a Graph in the Store asynchronously.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to update.</param>
    /// <param name="additions">Triples to be added.</param>
    /// <param name="removals">Triples to be removed.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public virtual void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, object state)
    {
        UpdateGraph(graphUri.ToSafeString(), additions, removals, callback, state);
    }

    /// <summary>
    /// Updates a Graph in the Store asynchronously.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to update.</param>
    /// <param name="additions">Triples to be added.</param>
    /// <param name="removals">Triples to be removed.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public abstract void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, object state);

    /// <inheritdoc />
    public abstract Task UpdateGraphAsync(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals,
        CancellationToken cancellationToken);

    /// <summary>
    /// Helper method for doing async update operations, callers just need to provide an appropriately prepared HTTP request and a RDF writer which will be used to write the data to the request body.
    /// </summary>
    /// <param name="request">HTTP Request.</param>
    /// <param name="writer">RDF writer.</param>
    /// <param name="graphUri">URI of the Graph to update.</param>
    /// <param name="ts">Triples.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    [Obsolete("This method is obsolete and will be removed in a future release")]
    protected internal void UpdateGraphAsync(HttpWebRequest request, IRdfWriter writer, Uri graphUri, IEnumerable<Triple> ts, AsyncStorageCallback callback, object state)
    {
        var g = new Graph();
        g.Assert(ts);

        request.BeginGetRequestStream(r =>
        {
            try
            {
                Stream reqStream = request.EndGetRequestStream(r);
                writer.Save(g, new StreamWriter(reqStream));

                request.BeginGetResponse(r2 =>
                {
                    try
                    {
                        var response = (HttpWebResponse)request.EndGetResponse(r2);
                        // If we get here then it was OK
                        response.Close();
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri), state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri, StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri, StorageHelper.HandleError(ex, "updating a Graph asynchronously in")), state);
                    }
                }, state);
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri, StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri, StorageHelper.HandleError(ex, "updating a Graph asynchronously in")), state);
            }
        }, state);
    }

    /// <summary>
    /// Helper method for doing async update operations, callers just need to provide an appropriately prepared HTTP request and a RDF writer which will be used to write the data to the request body.
    /// </summary>
    /// <param name="request">HTTP Request.</param>
    /// <param name="writer">RDF writer.</param>
    /// <param name="graphUri">URI of the Graph to update.</param>
    /// <param name="additions">Triples.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    protected internal void UpdateGraphAsync(HttpRequestMessage request, IRdfWriter writer, Uri graphUri,
        IEnumerable<Triple> additions, AsyncStorageCallback callback, object state)
    {
        var g = new Graph();
        g.Assert(additions);
        request.Content = new GraphContent(g, writer);
        HttpClient.SendAsync(request).ContinueWith(requestTask =>
        {
            if (requestTask.IsCanceled || requestTask.IsFaulted)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri,
                        requestTask.IsCanceled
                            ? new RdfStorageException("The operation was cancelled")
                            : StorageHelper.HandleError(requestTask.Exception,
                                "updating a Graph asynchronously in")), state);
            }
            else
            {
                using HttpResponseMessage response = requestTask.Result;
                if (!response.IsSuccessStatusCode)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri,
                            StorageHelper.HandleHttpError(response,
                                "updating a Graph asynchronously in")), state);
                }
                else
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri),
                        state);
                }
            }
        }).ConfigureAwait(true);
    }

    /// <summary>
    /// Helper method for simple updates that only add triples to a graph in the store.
    /// </summary>
    /// <param name="request">A pre-prepared request message without its content set.</param>
    /// <param name="writer">The RDF writer to use to create the request content.</param>
    /// <param name="additions">The triples to be added to the store.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <remarks>This helper method sets the content of <paramref name="request"/> to a new <see cref="GraphContent"/> instance using the provided <paramref name="writer"/>. It then sends the request to the server and provides a default handling of the response message.</remarks>
    protected internal async Task UpdateGraphAsync(HttpRequestMessage request, IRdfWriter writer,
        IEnumerable<Triple> additions, CancellationToken cancellationToken)
    {
        var g = new Graph();
        g.Assert(additions);
        request.Content = new GraphContent(g, writer);
        HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw StorageHelper.HandleHttpError(response, "updating a graph asynchronously in");
        }
    }

    /// <summary>
    /// Deletes a Graph from the Store.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to delete.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public virtual void DeleteGraph(Uri graphUri, AsyncStorageCallback callback, object state)
    {
        DeleteGraph(graphUri.ToSafeString(), callback, state);
    }

    /// <summary>
    /// Deletes a Graph from the Store.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to delete.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    public abstract void DeleteGraph(string graphUri, AsyncStorageCallback callback, object state);

    /// <inheritdoc />
    public abstract Task DeleteGraphAsync(string graphName, CancellationToken cancellationToken);

    /// <summary>
    /// Helper method for doing async delete operations, callers just need to provide an appropriately prepared HTTP request.
    /// </summary>
    /// <param name="request">HTTP request.</param>
    /// <param name="allow404">Whether a 404 response counts as success.</param>
    /// <param name="graphUri">URI of the Graph to delete.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    [Obsolete("This method is obsolete and will be removed in a future release")]
    protected internal void DeleteGraphAsync(HttpWebRequest request, bool allow404, string graphUri, AsyncStorageCallback callback, object state)
    {
        request.BeginGetResponse(r =>
        {
            try
            {
                var response = (HttpWebResponse)request.EndGetResponse(r);

                // Assume if returns to here we deleted the Graph OK
                response.Close();
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri()), state);
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
                }

                // Don't throw the error if we get a 404 - this means we couldn't do a delete as the graph didn't exist to start with
                if (webEx.Response == null || (webEx.Response != null && (!allow404 || ((HttpWebResponse)webEx.Response).StatusCode != HttpStatusCode.NotFound)))
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), new RdfStorageException("A HTTP Error occurred while trying to delete a Graph from the Store asynchronously", webEx)), state);
                }
                else
                {
                    // Consider a 404 as a success in some cases
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri()), state);
                }
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), StorageHelper.HandleError(ex, "deleting a Graph asynchronously from")), state);
            }
        }, state);
    }

    /// <summary>
    /// Helper method for doing async delete operations, callers just need to provide an appropriately prepared HTTP request.
    /// </summary>
    /// <param name="request">HTTP request.</param>
    /// <param name="allow404">Whether a 404 response counts as success.</param>
    /// <param name="graphUri">URI of the Graph to delete.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    protected internal void DeleteGraphAsync(HttpRequestMessage request, bool allow404, string graphUri,
        AsyncStorageCallback callback, object state)
    {
        HttpClient.SendAsync(request).ContinueWith(sendTask =>
        {
            if (sendTask.IsCanceled)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(),
                        new RdfStorageException("Operation was cancelled.")), state);
            }
            else if (sendTask.IsFaulted)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(),
                    StorageHelper.HandleError(sendTask.Exception, "deleting a Graph asynchronously from")), state);
            }
            else
            {
                HttpResponseMessage response = sendTask.Result;
                if (response.IsSuccessStatusCode || (response.StatusCode == HttpStatusCode.NotFound && allow404))
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri()),
                        state);
                }
                else
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(),
                            StorageHelper.HandleHttpError(response, "deleting a Graph asynchronously from")),
                        state);
                }
            }
        }).ConfigureAwait(true);
    }

    /// <summary>
    /// Helper method for deleting a graph from a store.
    /// </summary>
    /// <param name="request">The delete request to send.</param>
    /// <param name="allow404">True if the client should ignore a 404 returned as the result of trying to delete a non-existent graph.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    protected internal async Task DeleteGraphAsync(HttpRequestMessage request, bool allow404, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await HttpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.NotFound && allow404) return;
            throw StorageHelper.HandleHttpError(response, "deleting a graph asynchronously from");
        }
    }

    /// <summary>
    /// Lists the Graphs in the Store asynchronously.
    /// </summary>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    [Obsolete("Replaced with ListGraphsAsync(CancellationToken)")]
    public virtual void ListGraphs(AsyncStorageCallback callback, object state)
    {
        if (this is IAsyncQueryableStorage)
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
        else
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListGraphs, new RdfStorageException("Underlying store does not supported listing graphs asynchronously or has failed to appropriately override this method")), state);
        }
    }

    /// <summary>
    /// List the names of the graph on the remote server asynchronously.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>Task that returns the list of graph names.</returns>
    /// <exception cref="RdfStorageException">Raised if the store does not support the asynchronous SPARQL query required to retrieve a list of graph names.</exception>
    /// <remarks>This implementation uses a SPARQL query to list the names of the graphs in the store. Many stores support more efficient means of listing graphs and so this method SHOULD be overridden.
    /// If the store does not implement the <see cref="IAsyncQueryableStorage"/> interface, then this method MUST be overridden.</remarks>
    public virtual async Task<IEnumerable<string>> ListGraphsAsync(CancellationToken cancellationToken)
    {
        if (!(this is IAsyncQueryableStorage queryableStore))
        {
            throw new RdfStorageException(
                "Underlying store does not supported listing graphs asynchronously or has failed to appropriately override this method");
        }

        var handler = new ListUrisHandler("g");
        await queryableStore.QueryAsync(null, handler, "SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }", cancellationToken);
        return handler.Uris.Select(u => u.AbsoluteUri).ToList();
    }

    /// <summary>
    /// Indicates whether the Store is ready to accept requests.
    /// </summary>
    public abstract bool IsReady
    {
        get;
    }

    /// <summary>
    /// Gets whether the Store is read only.
    /// </summary>
    public abstract bool IsReadOnly
    {
        get;
    }

    /// <summary>
    /// Gets the IO Behaviour of the Store.
    /// </summary>
    public abstract IOBehaviour IOBehaviour
    {
        get;
    }

    /// <summary>
    /// Gets whether the Store supports Triple level updates via the <see cref="BaseAsyncHttpConnector.UpdateGraph(Uri,IEnumerable{Triple},IEnumerable{Triple},AsyncStorageCallback,object)">UpdateGraph()</see> method.
    /// </summary>
    public abstract bool UpdateSupported
    {
        get;
    }

    /// <summary>
    /// Gets whether the Store supports Graph deletion via the <see cref="BaseAsyncHttpConnector.DeleteGraph(Uri, AsyncStorageCallback, object)">DeleteGraph()</see> method.
    /// </summary>
    public abstract bool DeleteSupported
    {
        get;
    }

    /// <summary>
    /// Gets whether the Store supports listing graphs via the <see cref="BaseAsyncHttpConnector.ListGraphs(AsyncStorageCallback, object)">ListGraphs()</see> method.
    /// </summary>
    public abstract bool ListGraphsSupported
    {
        get;
    }

    /// <summary>
    /// Gets or sets the URI factory for the connector to use.
    /// </summary>
    protected IUriFactory UriFactory { get; set; } = new CachingUriFactory();

    /// <summary>
    /// Helper method for doing async operations where a sequence of HTTP requests must be run.
    /// </summary>
    /// <param name="requests">HTTP requests.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    protected internal void MakeRequestSequence(IEnumerable<HttpRequestMessage> requests, AsyncStorageCallback callback,
        object state)
    {
        var cts = new CancellationTokenSource();
        Task[] requestTasks = requests.Select(r =>
            HttpClient.SendAsync(r, cts.Token)
                .ContinueWith(t =>
                {
                    if (t.IsCanceled) return;
                    if (t.IsFaulted) cts.Cancel();
                    if (!t.Result.IsSuccessStatusCode)
                    {
                        cts.Cancel();
                        throw StorageHelper.HandleHttpError(t.Result,
                            "processing asynchronous request sequence on");
                    }
                }, cts.Token))
            .ToArray();
        Task.WaitAll(requestTasks, cts.Token);
        if (requestTasks.All(t => t.Status == TaskStatus.RanToCompletion))
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.Unknown), state);
        }
        else
        {
            callback(this,
                new AsyncStorageCallbackArgs(AsyncStorageOperation.Unknown,
                    new RdfStorageException(
                        "Unexpected error while making a sequence of asynchronous requests to the Store, see inner exception for details",
                        requestTasks.FirstOrDefault(t => t.IsFaulted)?.Exception)),
                state);
        }
    }

}