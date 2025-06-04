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
using System.Threading;
using System.Threading.Tasks;
using VDS.RDF.Storage.Management;

namespace VDS.RDF.Storage;

/// <summary>
/// Interface for storage providers which provide asynchronous read/write functionality to some arbitrary storage layer.
/// </summary>
/// <remarks>
/// Designed to allow for arbitrary Triple Stores to be plugged into the library as required by the end user.
/// </remarks>
public interface IAsyncStorageProvider
    : IStorageCapabilities, IDisposable
{
    /// <summary>
    /// Gets the Parent Server on which this store is hosted (if any).
    /// </summary>
    /// <remarks>
    /// <para>
    /// For storage back-ends which support multiple stores this is useful because it provides a way to access all the stores on that backend.  For stores which are standalone they should simply return null.
    /// </para>
    /// </remarks>
    IAsyncStorageServer AsyncParentServer
    {
        get;
    }

    /// <summary>
    /// Loads a Graph from the Store asynchronously.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    [Obsolete("Replaced with LoadGraphAsync(IGraph, string, CancellationToken)")]
    void LoadGraph(IGraph g, Uri graphUri, AsyncStorageCallback callback, object state);

    /// <summary>
    /// Loads a Graph from the Store asynchronously.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    [Obsolete("Replaced with LoadGraphAsync(IGraph, string, CancellationToken)")]
    void LoadGraph(IGraph g, string graphUri, AsyncStorageCallback callback, object state);

    /// <summary>
    /// Loads a Graph from the Store asynchronously.
    /// </summary>
    /// <param name="handler">Handler to load with.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    [Obsolete("Replaced with LoadGraphAsync(IRdfHandler, string, CancellationToken)")]
    void LoadGraph(IRdfHandler handler, Uri graphUri, AsyncStorageCallback callback, object state);

    /// <summary>
    /// Loads a Graph from the Store asynchronously.
    /// </summary>
    /// <param name="handler">Handler to load with.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    [Obsolete("Replaced with LoadGraphAsync(IRdfHandler, string, CancellationToken)")]
    void LoadGraph(IRdfHandler handler, string graphUri, AsyncStorageCallback callback, object state);

    /// <summary>
    /// Loads a graph from the store asynchronously.
    /// </summary>
    /// <param name="g">The target graph to load into.</param>
    /// <param name="graphName">Name of the graph to load.</param>
    /// <param name="cancellationToken"></param>
    Task LoadGraphAsync(IGraph g, string graphName, CancellationToken cancellationToken);

    /// <summary>
    /// Loads a graph from the store asynchronously.
    /// </summary>
    /// <param name="handler">The handler to receive the loaded triples.</param>
    /// <param name="graphName">Name of the graph to load.</param>
    /// <param name="cancellationToken"></param>
    Task LoadGraphAsync(IRdfHandler handler, string graphName, CancellationToken cancellationToken);

    /// <summary>
    /// Saves a Graph to the Store asynchronously.
    /// </summary>
    /// <param name="g">Graph to save.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    [Obsolete("Replaced with SaveGraphAsync(IGraph, CancellationToken)")]
    void SaveGraph(IGraph g, AsyncStorageCallback callback, object state);

    /// <summary>
    /// Saves a Graph to the Store asynchronously.
    /// </summary>
    /// <param name="g">Graph to save.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveGraphAsync(IGraph g, CancellationToken cancellationToken);

    /// <summary>
    /// Updates a Graph in the Store asynchronously.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to update.</param>
    /// <param name="additions">Triples to be added.</param>
    /// <param name="removals">Triples to be removed.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    [Obsolete("Replaced with UpdateGraphAsync(string, IEnumerable<Triple>, IEnumerable<Triple>, CancellationToken")]
    void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, object state);

    /// <summary>
    /// Updates a Graph in the Store asynchronously.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to update.</param>
    /// <param name="additions">Triples to be added.</param>
    /// <param name="removals">Triples to be removed.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    [Obsolete("Replaced with UpdateGraphAsync(string, IEnumerable<Triple>, IEnumerable<Triple>, CancellationToken")]
    void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, object state);

    /// <summary>
    /// Updates a graph in the store asynchronously.
    /// </summary>
    /// <param name="graphName">Name of the graph to update.</param>
    /// <param name="additions">Triples to be added.</param>
    /// <param name="removals">Triples to be removed.</param>
    /// <param name="cancellationToken"></param>
    Task UpdateGraphAsync(string graphName, IEnumerable<Triple> additions, IEnumerable<Triple> removals,
        CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a Graph from the Store.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to delete.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    [Obsolete("Replaced by DeleteGraphAsync(string, CancellationToken)")]
    void DeleteGraph(Uri graphUri, AsyncStorageCallback callback, object state);

    /// <summary>
    /// Deletes a Graph from the Store.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to delete.</param>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    [Obsolete("Replaced with DeleteGraphAsync(string, CancellationToken)")]
    void DeleteGraph(string graphUri, AsyncStorageCallback callback, object state);

    /// <summary>
    /// Deletes a graph from the store asynchronously.
    /// </summary>
    /// <param name="graphName">Name of the graph to delete.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DeleteGraphAsync(string graphName, CancellationToken cancellationToken);

    /// <summary>
    /// Lists the Graphs in the Store asynchronously.
    /// </summary>
    /// <param name="callback">Callback.</param>
    /// <param name="state">State to pass to the callback.</param>
    [Obsolete("Replaced with ListGraphsAsync(CancellationToken)")]
    void ListGraphs(AsyncStorageCallback callback, object state);

    /// <summary>
    /// Lists the names of the graphs in the store asynchronously.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IEnumerable<string>> ListGraphsAsync(CancellationToken cancellationToken);
}