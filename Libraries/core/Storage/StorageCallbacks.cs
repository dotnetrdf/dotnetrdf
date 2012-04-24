using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Possible Async Storage API Actions
    /// </summary>
    public enum AsyncStorageAction
    {
        LoadGraph,
        LoadWithHandler,
        SaveGraph,
        UpdateGraph,
        DeleteGraph,
        ListGraphs,
        SparqlQuery,
        SparqlQueryWithHandler,
        SparqlUpdate,
        TransactionBegin,
        TransactionCommit,
        TransactionRollback
    }

    /// <summary>
    /// Represents arguments passed to callbacks on success/failure of a async storage API call
    /// </summary>
    /// <remarks>
    /// <para>
    /// Primarily used to provide simple method signatures on the async storage API callbacks
    /// </para>
    /// </remarks>
    public sealed class AsyncStorageCallbackArgs
    {
        public AsyncStorageCallbackArgs(AsyncStorageAction action)
        {
            this.Action = action;
        }

        public AsyncStorageCallbackArgs(AsyncStorageAction action, Exception ex)
            : this(action)
        {
            this.Error = ex;
        }

        public AsyncStorageCallbackArgs(AsyncStorageAction action, IGraph g)
            : this(action)
        {
            this.Graph = g;
        }

        public AsyncStorageCallbackArgs(AsyncStorageAction action, IGraph g, Exception e)
            : this(action, e)
        {
            this.Graph = g;
        }

        public AsyncStorageCallbackArgs(AsyncStorageAction action, Uri graphUri)
            : this(action)
        {
            this.GraphUri = graphUri;
        }

        public AsyncStorageCallbackArgs(AsyncStorageAction action, Uri graphUri, Exception e)
            : this(action, e)
        {
            this.GraphUri = graphUri;
        }

        public AsyncStorageCallbackArgs(AsyncStorageAction action, IEnumerable<Uri> graphUris)
            : this(action)
        {
            this.GraphUris = graphUris;
        }

        public AsyncStorageCallbackArgs(AsyncStorageAction action, IRdfHandler handler)
            : this(action)
        {
            this.RdfHandler = handler;
        }

        public AsyncStorageCallbackArgs(AsyncStorageAction action, IRdfHandler handler, Exception e)
            : this(action, e)
        {
            this.RdfHandler = handler;
        }

        public AsyncStorageCallbackArgs(AsyncStorageAction action, String query, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler)
            : this(action)
        {
            this.Query = query;
            this.RdfHandler = rdfHandler;
            this.ResultsHandler = resultsHandler;
        }

        public AsyncStorageCallbackArgs(AsyncStorageAction action, String query, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, Exception e)
            : this(action, e)
        {
            this.Query = query;
            this.RdfHandler = rdfHandler;
            this.ResultsHandler = resultsHandler;
        }

        public AsyncStorageCallbackArgs(AsyncStorageAction action, ISparqlResultsHandler handler, Exception e)
            : this(action, e)
        {
            this.ResultsHandler = handler;
        }

        public AsyncStorageCallbackArgs(AsyncStorageAction action, String query, Object queryResults)
            : this(action, queryResults as IGraph)
        {
            this.Query = query;
            this.QueryResults = queryResults;
        }

        public AsyncStorageCallbackArgs(AsyncStorageAction action, String query, Object queryResults, Exception e)
            : this(action, e)
        {
            this.Query = query;
            this.QueryResults = queryResults;
        }

        public AsyncStorageCallbackArgs(AsyncStorageAction action, String updates)
            : this(action)
        {
            this.Updates = updates;
        }

        public AsyncStorageCallbackArgs(AsyncStorageAction action, String updates, Exception e)
            : this(action, e)
        {
            this.Updates = updates;
        }

        public bool WasSuccessful
        {
            get
            {
                return this.Error == null;
            }
        }

        public IGraph Graph { get; private set; }

        public Exception Error { get; private set; }

        public Uri GraphUri { get; private set; }

        public IEnumerable<Uri> GraphUris { get; private set; }

        public IRdfHandler RdfHandler { get; private set; }

        public ISparqlResultsHandler ResultsHandler { get; private set; }

        public Object QueryResults { get; private set; }

        public String Query { get; private set; }

        public String Updates { get; private set; }

        public AsyncStorageAction Action { get; private set; }
    }

    /// <summary>
    /// Generic callback for async storage API operations
    /// </summary>
    /// <param name="sender">Originator of the callback</param>
    /// <param name="args">Callback Arguments</param>
    /// <param name="state">State object originally passed to the async call</param>
    public delegate void AsyncStorageCallback(Object sender, AsyncStorageCallbackArgs args, Object state);
}
