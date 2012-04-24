using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage
{
    internal static class AsyncStorageExtensions
    {
        private delegate void AsyncLoadGraphDelegate(IStorageProvider storage, IGraph g, Uri graphUri);

        private static void LoadGraph(IStorageProvider storage, IGraph g, Uri graphUri)
        {
            storage.LoadGraph(g, graphUri);
        }

        internal static void AsyncLoadGraph(this IStorageProvider storage, IGraph g, Uri graphUri, AsyncStorageCallback callback, Object state)
        {
            AsyncLoadGraphDelegate d = new AsyncLoadGraphDelegate(LoadGraph);
            d.BeginInvoke(storage, g, graphUri, r =>
                {
                    try
                    {
                        d.EndInvoke(r);
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageAction.LoadGraph, g), state);
                    }
                    catch (Exception e)
                    {
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageAction.LoadGraph, g, e), state);
                    }
                }, state);
        }

        private delegate void AsyncLoadGraphHandlersDelegate(IStorageProvider storage, IRdfHandler handler, Uri graphUri);

        private static void LoadGraph(IStorageProvider storage, IRdfHandler handler, Uri graphUri)
        {
            storage.LoadGraph(handler, graphUri);   
        }

        internal static void AsyncLoadGraph(this IStorageProvider storage, IRdfHandler handler, Uri graphUri, AsyncStorageCallback callback, Object state)
        {
            AsyncLoadGraphHandlersDelegate d = new AsyncLoadGraphHandlersDelegate(LoadGraph);
            d.BeginInvoke(storage, handler, graphUri, r =>
                {
                    try
                    {
                        d.EndInvoke(r);
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageAction.LoadWithHandler, handler), state);
                    }
                    catch (Exception e)
                    {
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageAction.LoadWithHandler, handler, e), state);
                    }
                }, state);
        }

        private delegate void AsyncSaveGraphDelegate(IStorageProvider storage, IGraph g);

        private static void SaveGraph(IStorageProvider storage, IGraph g)
        {
            storage.SaveGraph(g);
        }

        internal static void AsyncSaveGraph(this IStorageProvider storage, IGraph g, AsyncStorageCallback callback, Object state)
        {
            AsyncSaveGraphDelegate d = new AsyncSaveGraphDelegate(SaveGraph);
            d.BeginInvoke(storage, g, r =>
                {
                    try
                    {
                        d.EndInvoke(r);
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageAction.SaveGraph, g), state);
                    }
                    catch (Exception e)
                    {
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageAction.SaveGraph, g, e), state);
                    }
                }, state);
        }

        private delegate void AsyncUpdateGraphDelegate(IStorageProvider storage, Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals);

        private static void UpdateGraph(IStorageProvider storage, Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            storage.UpdateGraph(graphUri, additions, removals);
        }

        internal static void AsyncUpdateGraph(this IStorageProvider storage, Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, Object state)
        {
            AsyncUpdateGraphDelegate d = new AsyncUpdateGraphDelegate(UpdateGraph);
            d.BeginInvoke(storage, graphUri, additions, removals, r =>
                {
                    try
                    {
                        d.EndInvoke(r);
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageAction.UpdateGraph, graphUri), state);
                    }
                    catch (Exception e)
                    {
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageAction.UpdateGraph, graphUri, e), state);
                    }
                }, state);
        }

        private delegate void AsyncDeleteGraphDelegate(IStorageProvider storage, Uri graphUri);

        private static void DeleteGraph(IStorageProvider storage, Uri graphUri)
        {
            storage.DeleteGraph(graphUri);
        }

        internal static void AsyncDeleteGraph(this IStorageProvider storage, Uri graphUri, AsyncStorageCallback callback, Object state)
        {
            AsyncDeleteGraphDelegate d = new AsyncDeleteGraphDelegate(DeleteGraph);
            d.BeginInvoke(storage, graphUri, r =>
                {
                    try
                    {
                        d.EndInvoke(r);
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageAction.DeleteGraph, graphUri), state);
                    }
                    catch (Exception e)
                    {
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageAction.DeleteGraph, graphUri, e), state);
                    }
                }, state);
        }

        private delegate IEnumerable<Uri> AsyncListGraphsDelegate(IStorageProvider storage);

        private static IEnumerable<Uri> ListGraphs(IStorageProvider storage)
        {
            return storage.ListGraphs();
        }

        internal static void AsyncListGraphs(this IStorageProvider storage, AsyncStorageCallback callback, Object state)
        {
            AsyncListGraphsDelegate d = new AsyncListGraphsDelegate(ListGraphs);
            d.BeginInvoke(storage, r =>
                {
                    try
                    {
                        IEnumerable<Uri> graphs = d.EndInvoke(r);
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageAction.ListGraphs, graphs), state);
                    }
                    catch (Exception e)
                    {
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageAction.ListGraphs, e), state);
                    }
                }, state);
        }

        private delegate Object AsyncQueryDelegate(IQueryableStorage storage, String query);

        private static Object Query(IQueryableStorage storage, String query)
        {
            return storage.Query(query);
        }

        internal static void AsyncQuery(this IQueryableStorage storage, String query, AsyncStorageCallback callback, Object state)
        {
            AsyncQueryDelegate d = new AsyncQueryDelegate(Query);
            d.BeginInvoke(storage, query, r =>
                {
                    try
                    {
                        Object results = d.EndInvoke(r);
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageAction.SparqlQuery, query, results), state);
                    }
                    catch (Exception e)
                    {
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageAction.SparqlQuery, query, e), state);
                    }
                }, state);
        }

        private delegate void AsyncQueryHandlersDelegate(IQueryableStorage storage, String query, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler);

        private static void QueryHandlers(IQueryableStorage storage, String query, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler)
        {
            storage.Query(rdfHandler, resultsHandler, query);
        }

        internal static void AsyncQueryHandlers(this IQueryableStorage storage, String query, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, AsyncStorageCallback callback, Object state)
        {
            AsyncQueryHandlersDelegate d = new AsyncQueryHandlersDelegate(QueryHandlers);
            d.BeginInvoke(storage, query, rdfHandler, resultsHandler, r =>
                {
                    try
                    {
                        d.EndInvoke(r);
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageAction.SparqlQueryWithHandler, query, rdfHandler, resultsHandler), state);
                    }
                    catch (Exception e)
                    {
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageAction.SparqlQueryWithHandler, query, rdfHandler, resultsHandler, e), state);
                    }
                }, state);
        }

        private delegate void AsyncUpdateDelegate(IUpdateableStorage storage, String updates);

        private static void Update(IUpdateableStorage storage, String updates)
        {
            storage.Update(updates);
        }

        internal static void AsyncUpdate(this IUpdateableStorage storage, String updates, AsyncStorageCallback callback, Object state)
        {
            AsyncUpdateDelegate d = new AsyncUpdateDelegate(Update);
            d.BeginInvoke(storage, updates, r =>
                {
                    try
                    {
                        d.EndInvoke(r);
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageAction.SparqlUpdate, updates), state);
                    }
                    catch (Exception e)
                    {
                        callback(storage, new AsyncStorageCallbackArgs(AsyncStorageAction.SparqlUpdate, updates, e), state);
                    }
                }, state);
        }
    }
}
