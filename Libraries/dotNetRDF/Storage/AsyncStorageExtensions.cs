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

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Static Helper class containing internal extensions methods used to support the <see cref="BaseAsyncSafeConnector">BaseAsyncSafeConnector</see> class
    /// </summary>
    internal static class AsyncStorageExtensions
    {
        private delegate void AsyncLoadGraphDelegate(IStorageProvider storage, IGraph g, Uri graphUri);

        private static void LoadGraph(IStorageProvider storage, IGraph g, Uri graphUri)
        {
            storage.LoadGraph(g, graphUri);
        }

        /// <summary>
        /// Loads a Graph asynchronously
        /// </summary>
        /// <param name="storage">Storage Provider</param>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        internal static void AsyncLoadGraph(this IStorageProvider storage, IGraph g, Uri graphUri, AsyncStorageCallback callback, Object state)
        {
            AsyncLoadGraphDelegate d = new AsyncLoadGraphDelegate(LoadGraph);
            d.BeginInvoke(storage, g, graphUri, r =>
                {
                    try
                    {
                        d.EndInvoke(r);
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadGraph, g), state);
                    }
                    catch (Exception e)
                    {
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadGraph, g, e), state);
                    }
                }, state);
        }

        private delegate void AsyncLoadGraphHandlersDelegate(IStorageProvider storage, IRdfHandler handler, Uri graphUri);

        private static void LoadGraph(IStorageProvider storage, IRdfHandler handler, Uri graphUri)
        {
            storage.LoadGraph(handler, graphUri);   
        }
      
        /// <summary>
        /// Loads a Graph asynchronously
        /// </summary>
        /// <param name="storage">Storage Provider</param>
        /// <param name="handler">Handler to load with</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        internal static void AsyncLoadGraph(this IStorageProvider storage, IRdfHandler handler, Uri graphUri, AsyncStorageCallback callback, Object state)
        {
            AsyncLoadGraphHandlersDelegate d = new AsyncLoadGraphHandlersDelegate(LoadGraph);
            d.BeginInvoke(storage, handler, graphUri, r =>
                {
                    try
                    {
                        d.EndInvoke(r);
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler), state);
                    }
                    catch (Exception e)
                    {
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler, e), state);
                    }
                }, state);
        }

        private delegate void AsyncSaveGraphDelegate(IStorageProvider storage, IGraph g);

        private static void SaveGraph(IStorageProvider storage, IGraph g)
        {
            storage.SaveGraph(g);
        }

        /// <summary>
        /// Saves a Graph aynchronously
        /// </summary>
        /// <param name="storage">Storage Provider</param>
        /// <param name="g">Graph to save</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        internal static void AsyncSaveGraph(this IStorageProvider storage, IGraph g, AsyncStorageCallback callback, Object state)
        {
            AsyncSaveGraphDelegate d = new AsyncSaveGraphDelegate(SaveGraph);
            d.BeginInvoke(storage, g, r =>
                {
                    try
                    {
                        d.EndInvoke(r);
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g), state);
                    }
                    catch (Exception e)
                    {
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g, e), state);
                    }
                }, state);
        }

        private delegate void AsyncUpdateGraphDelegate(IStorageProvider storage, Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals);

        private static void UpdateGraph(IStorageProvider storage, Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            storage.UpdateGraph(graphUri, additions, removals);
        }

        /// <summary>
        /// Updates a Graph asynchronously
        /// </summary>
        /// <param name="storage">Storage Provider</param>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to add</param>
        /// <param name="removals">Triples to remove</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        internal static void AsyncUpdateGraph(this IStorageProvider storage, Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, Object state)
        {
            AsyncUpdateGraphDelegate d = new AsyncUpdateGraphDelegate(UpdateGraph);
            d.BeginInvoke(storage, graphUri, additions, removals, r =>
                {
                    try
                    {
                        d.EndInvoke(r);
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri), state);
                    }
                    catch (Exception e)
                    {
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri, e), state);
                    }
                }, state);
        }

        private delegate void AsyncDeleteGraphDelegate(IStorageProvider storage, Uri graphUri);

        private static void DeleteGraph(IStorageProvider storage, Uri graphUri)
        {
            storage.DeleteGraph(graphUri);
        }

        /// <summary>
        /// Deletes a Graph asynchronously
        /// </summary>
        /// <param name="storage">Storage Provider</param>
        /// <param name="graphUri">URI of the Graph to delete</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        internal static void AsyncDeleteGraph(this IStorageProvider storage, Uri graphUri, AsyncStorageCallback callback, Object state)
        {
            AsyncDeleteGraphDelegate d = new AsyncDeleteGraphDelegate(DeleteGraph);
            d.BeginInvoke(storage, graphUri, r =>
                {
                    try
                    {
                        d.EndInvoke(r);
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri), state);
                    }
                    catch (Exception e)
                    {
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri, e), state);
                    }
                }, state);
        }

        private delegate IEnumerable<Uri> AsyncListGraphsDelegate(IStorageProvider storage);

        private static IEnumerable<Uri> ListGraphs(IStorageProvider storage)
        {
            return storage.ListGraphs();
        }

        /// <summary>
        /// Lists Graphs in the store asynchronously
        /// </summary>
        /// <param name="storage">Storage Provider</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        internal static void AsyncListGraphs(this IStorageProvider storage, AsyncStorageCallback callback, Object state)
        {
            AsyncListGraphsDelegate d = new AsyncListGraphsDelegate(ListGraphs);
            d.BeginInvoke(storage, r =>
                {
                    try
                    {
                        IEnumerable<Uri> graphs = d.EndInvoke(r);
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListGraphs, graphs), state);
                    }
                    catch (Exception e)
                    {
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListGraphs, e), state);
                    }
                }, state);
        }

        private delegate Object AsyncQueryDelegate(IQueryableStorage storage, String query);

        private static Object Query(IQueryableStorage storage, String query)
        {
            return storage.Query(query);
        }

        /// <summary>
        /// Queries a store asynchronously
        /// </summary>
        /// <param name="storage">Storage Provider</param>
        /// <param name="query">SPARQL Query</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        internal static void AsyncQuery(this IQueryableStorage storage, String query, AsyncStorageCallback callback, Object state)
        {
            AsyncQueryDelegate d = new AsyncQueryDelegate(Query);
            d.BeginInvoke(storage, query, r =>
                {
                    try
                    {
                        Object results = d.EndInvoke(r);
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQuery, query, results), state);
                    }
                    catch (Exception e)
                    {
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQuery, query, e), state);
                    }
                }, state);
        }

        private delegate void AsyncQueryHandlersDelegate(IQueryableStorage storage, String query, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler);

        private static void QueryHandlers(IQueryableStorage storage, String query, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler)
        {
            storage.Query(rdfHandler, resultsHandler, query);
        }

        /// <summary>
        /// Queries a store asynchronously
        /// </summary>
        /// <param name="storage">Storage Provider</param>
        /// <param name="query">SPARQL Query</param>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        internal static void AsyncQueryHandlers(this IQueryableStorage storage, String query, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, AsyncStorageCallback callback, Object state)
        {
            AsyncQueryHandlersDelegate d = new AsyncQueryHandlersDelegate(QueryHandlers);
            d.BeginInvoke(storage, query, rdfHandler, resultsHandler, r =>
                {
                    try
                    {
                        d.EndInvoke(r);
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, query, rdfHandler, resultsHandler), state);
                    }
                    catch (Exception e)
                    {
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, query, rdfHandler, resultsHandler, e), state);
                    }
                }, state);
        }

        private delegate void AsyncUpdateDelegate(IUpdateableStorage storage, String updates);

        private static void Update(IUpdateableStorage storage, String updates)
        {
            storage.Update(updates);
        }

        /// <summary>
        /// Updates a store asynchronously
        /// </summary>
        /// <param name="storage">Storage Provider</param>
        /// <param name="updates">SPARQL Update</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        internal static void AsyncUpdate(this IUpdateableStorage storage, String updates, AsyncStorageCallback callback, Object state)
        {
            AsyncUpdateDelegate d = new AsyncUpdateDelegate(Update);
            d.BeginInvoke(storage, updates, r =>
                {
                    try
                    {
                        d.EndInvoke(r);
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, updates), state);
                    }
                    catch (Exception e)
                    {
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, updates, e), state);
                    }
                }, state);
        }
    }
}
