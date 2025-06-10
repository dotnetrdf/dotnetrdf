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
using System.Threading.Tasks;

namespace VDS.RDF.Storage;

/// <summary>
/// Static Helper class containing internal extensions methods used to support the <see cref="BaseAsyncSafeConnector">BaseAsyncSafeConnector</see> class.
/// </summary>
internal static class AsyncStorageExtensions
{
    private static void LoadGraph(IStorageProvider storage, IGraph g, Uri graphUri)
    {
        storage.LoadGraph(g, graphUri);
    }

    /// <summary>
    /// Loads a Graph asynchronously.
    /// </summary>
    /// <param name="storage">Storage Provider.</param>
    /// <param name="g">Graph to load into.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    internal static void AsyncLoadGraph(this IStorageProvider storage, IGraph g, Uri graphUri, AsyncStorageCallback callback, object state)
    {
        Task.Factory.StartNew(() => LoadGraph(storage, g, graphUri)).ContinueWith(antecedent =>
        {
            callback(storage,
                antecedent.IsFaulted
                    ? new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadGraph, g, antecedent.Exception)
                    : new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadGraph, g), state);
        });
    }

    private static void LoadGraph(IStorageProvider storage, IRdfHandler handler, Uri graphUri)
    {
        storage.LoadGraph(handler, graphUri);   
    }
  
    /// <summary>
    /// Loads a Graph asynchronously.
    /// </summary>
    /// <param name="storage">Storage Provider.</param>
    /// <param name="handler">Handler to load with.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    internal static void AsyncLoadGraph(this IStorageProvider storage, IRdfHandler handler, Uri graphUri, AsyncStorageCallback callback, object state)
    {
        Task.Factory.StartNew(() => LoadGraph(storage, handler, graphUri)).ContinueWith(antecedent =>
            callback(storage,
                antecedent.IsFaulted
                    ? new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler,
                        antecedent.Exception)
                    : new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler),
                state));
    }

    private static void SaveGraph(IStorageProvider storage, IGraph g)
    {
        storage.SaveGraph(g);
    }

    /// <summary>
    /// Saves a Graph asynchronously.
    /// </summary>
    /// <param name="storage">Storage Provider.</param>
    /// <param name="g">Graph to save.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    internal static void AsyncSaveGraph(this IStorageProvider storage, IGraph g, AsyncStorageCallback callback, object state)
    {
        Task.Factory.StartNew(() => SaveGraph(storage, g)).ContinueWith(antecedent =>
            callback(storage,
                antecedent.IsFaulted
                    ? new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g, antecedent.Exception)
                    : new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g), state));
    }

    private static void UpdateGraph(IStorageProvider storage, Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
    {
        storage.UpdateGraph(graphUri == null ? null : new UriNode(graphUri), additions, removals);
    }

    /// <summary>
    /// Updates a Graph asynchronously.
    /// </summary>
    /// <param name="storage">Storage Provider.</param>
    /// <param name="graphUri">URI of the Graph to update.</param>
    /// <param name="additions">Triples to add.</param>
    /// <param name="removals">Triples to remove.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    internal static void AsyncUpdateGraph(this IStorageProvider storage, Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, object state)
    {
        Task.Factory.StartNew(() => UpdateGraph(storage, graphUri, additions, removals)).ContinueWith(antecedent =>
            callback(storage,
                antecedent.IsFaulted
                    ? new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri,
                        antecedent.Exception)
                    : new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri), state));
    }

    private static void DeleteGraph(IStorageProvider storage, Uri graphUri)
    {
        storage.DeleteGraph(graphUri);
    }

    /// <summary>
    /// Deletes a Graph asynchronously.
    /// </summary>
    /// <param name="storage">Storage Provider.</param>
    /// <param name="graphUri">URI of the Graph to delete.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    internal static void AsyncDeleteGraph(this IStorageProvider storage, Uri graphUri, AsyncStorageCallback callback, object state)
    {
        Task.Factory.StartNew(() => DeleteGraph(storage, graphUri)).ContinueWith(antecedent =>
            callback(storage,
                antecedent.IsFaulted
                    ? new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri,
                        antecedent.Exception)
                    : new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri), state));
    }

    private static IEnumerable<Uri> ListGraphs(IStorageProvider storage)
    {
        foreach (var graphName in storage.ListGraphNames())
        {
            if (Uri.TryCreate(graphName, UriKind.Absolute, out Uri u))
            {
                yield return u;
            }
        }
    }

    /// <summary>
    /// Lists Graphs in the store asynchronously.
    /// </summary>
    /// <param name="storage">Storage Provider.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    internal static void AsyncListGraphs(this IStorageProvider storage, AsyncStorageCallback callback, object state)
    {
        Task.Factory.StartNew(() => ListGraphs(storage)).ContinueWith(antecedent =>
        {
            callback(storage,
                antecedent.IsFaulted
                    ? new AsyncStorageCallbackArgs(AsyncStorageOperation.ListGraphs, antecedent.Exception)
                    : new AsyncStorageCallbackArgs(AsyncStorageOperation.ListGraphs, antecedent.Result), state);
        });
    }

    private static object Query(IQueryableStorage storage, string query)
    {
        return storage.Query(query);
    }

    /// <summary>
    /// Queries a store asynchronously.
    /// </summary>
    /// <param name="storage">Storage Provider.</param>
    /// <param name="query">SPARQL Query.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    internal static void AsyncQuery(this IQueryableStorage storage, string query, AsyncStorageCallback callback, object state)
    {
        Task.Factory.StartNew(() => Query(storage, query)).ContinueWith(antecedent =>
            callback(storage,
                antecedent.IsFaulted
                    ? new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQuery, query, antecedent.Exception)
                    : new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQuery, query, antecedent.Result),
                state));
    }

    private static void QueryHandlers(IQueryableStorage storage, string query, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler)
    {
        storage.Query(rdfHandler, resultsHandler, query);
    }

    /// <summary>
    /// Queries a store asynchronously.
    /// </summary>
    /// <param name="storage">Storage Provider.</param>
    /// <param name="query">SPARQL Query.</param>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    internal static void AsyncQueryHandlers(this IQueryableStorage storage, string query, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, AsyncStorageCallback callback, object state)
    {
        Task.Factory.StartNew(() => QueryHandlers(storage, query, rdfHandler, resultsHandler)).ContinueWith(
            antecedent =>
                callback(storage,
                    antecedent.IsFaulted
                        ? new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, query,
                            rdfHandler, resultsHandler, antecedent.Exception)
                        : new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, query,
                            rdfHandler, resultsHandler), state));
    }

    private static void Update(IUpdateableStorage storage, string updates)
    {
        storage.Update(updates);
    }

    /// <summary>
    /// Updates a store asynchronously.
    /// </summary>
    /// <param name="storage">Storage Provider.</param>
    /// <param name="updates">SPARQL Update.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    internal static void AsyncUpdate(this IUpdateableStorage storage, string updates, AsyncStorageCallback callback, object state)
    {
        Task.Factory.StartNew(() => Update(storage, updates)).ContinueWith(antecedent =>
            callback(storage,
                antecedent.IsFaulted
                    ? new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, updates,
                        antecedent.Exception)
                    : new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, updates), state));
    }
}
