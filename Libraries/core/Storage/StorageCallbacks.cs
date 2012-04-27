using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Possible Async Storage API Actions
    /// </summary>
    public enum AsyncStorageOperation
    {
        /// <summary>
        /// Loaded a Graph
        /// </summary>
        LoadGraph,
        /// <summary>
        /// Loaded data with a RDF Handler
        /// </summary>
        LoadWithHandler,
        /// <summary>
        /// Saved a Graph
        /// </summary>
        SaveGraph,
        /// <summary>
        /// Updates a Graph
        /// </summary>
        UpdateGraph,
        /// <summary>
        /// Deleted a Graph
        /// </summary>
        DeleteGraph,
        /// <summary>
        /// Listed Graphs
        /// </summary>
        ListGraphs,
        /// <summary>
        /// Made a SPARQL Query
        /// </summary>
        SparqlQuery,
        /// <summary>
        /// Made a SPARQL Query with a handler
        /// </summary>
        SparqlQueryWithHandler,
        /// <summary>
        /// Made a SPARQL Update
        /// </summary>
        SparqlUpdate,
        /// <summary>
        /// Began a Transaction
        /// </summary>
        TransactionBegin,
        /// <summary>
        /// Committed a Transaction
        /// </summary>
        TransactionCommit,
        /// <summary>
        /// Rolled back a Transaction
        /// </summary>
        TransactionRollback,
        /// <summary>
        /// Created a Store
        /// </summary>
        CreateStore,
        /// <summary>
        /// Deleted a Store
        /// </summary>
        DeleteStore,
        /// <summary>
        /// Retrieved a reference to a Store
        /// </summary>
        GetStore,
        /// <summary>
        /// Got the list of Stores
        /// </summary>
        ListStores
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
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation)
        {
            this.Operation = operation;
        }

        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, Exception ex)
            : this(operation)
        {
            this.Error = ex;
        }

        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, IGraph g)
            : this(operation, g, null) { }

        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, IGraph g, Exception e)
            : this(operation, e)
        {
            this.Graph = g;
        }

        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, Uri graphUri)
            : this(operation, graphUri, null) { }

        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, Uri graphUri, Exception e)
            : this(operation, e)
        {
            this.GraphUri = graphUri;
        }

        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, IEnumerable<Uri> graphUris)
            : this(operation)
        {
            this.GraphUris = graphUris;
        }

        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, IRdfHandler handler)
            : this(operation, handler, null) { }

        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, IRdfHandler handler, Exception e)
            : this(operation, e)
        {
            this.RdfHandler = handler;
        }

        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, String query, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler)
            : this(operation, query, rdfHandler, resultsHandler, null) { }

        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, String query, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, Exception e)
            : this(operation, e)
        {
            this.SetData(query);
            this.RdfHandler = rdfHandler;
            this.ResultsHandler = resultsHandler;
        }

        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, String query, Object queryResults)
            : this(operation, queryResults as IGraph)
        {
            this.SetData(query);
            this.QueryResults = queryResults;
        }

        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, String query, Object queryResults, Exception e)
            : this(operation, e)
        {
            this.SetData(query);
            this.QueryResults = queryResults;
        }

        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, String data)
            : this(operation, data, null) { }

        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, String data, Exception e)
            : this(operation, e)
        {
            this.SetData(data);
        }

        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, IEnumerable<String> stores)
            : this(operation, stores, null) { }

        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, IEnumerable<String> stores, Exception e)
            : this(operation, e)
        {
            this.StoreIDs = stores;
        }

        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, String storeID, IAsyncStorageProvider provider, Exception e)
            : this(operation, storeID, e)
        {
            this.StorageProvider = provider;
        }

        private void SetData(String data)
        {
            switch (this.Operation)
            {
                case AsyncStorageOperation.SparqlUpdate:
                    this.Updates = data;
                    break;
                case AsyncStorageOperation.SparqlQuery:
                case AsyncStorageOperation.SparqlQueryWithHandler:
                    this.Query = data;
                    break;

                case AsyncStorageOperation.CreateStore:
                case AsyncStorageOperation.DeleteStore:
                case AsyncStorageOperation.GetStore:
                    this.StoreID = data;
                    break;
            }
        }

        /// <summary>
        /// Gets whether the async operation succeeded (no error occurred)
        /// </summary>
        public bool WasSuccessful
        {
            get
            {
                return this.Error == null;
            }
        }

        /// <summary>
        /// Gets the Graph that was saved/loaded (if applicable)
        /// </summary>
        public IGraph Graph { get; private set; }

        /// <summary>
        /// Gets the error that occurred (for failed operations)
        /// </summary>
        public Exception Error { get; private set; }

        /// <summary>
        /// Gets the URI of the Graph affected by the operation
        /// </summary>
        public Uri GraphUri { get; private set; }

        /// <summary>
        /// Gets the list of Graph URIs (if applicable)
        /// </summary>
        public IEnumerable<Uri> GraphUris { get; private set; }

        /// <summary>
        /// Gets the RDF Handler used (if applicable)
        /// </summary>
        public IRdfHandler RdfHandler { get; private set; }

        /// <summary>
        /// Gets the Results Handler used (if applicable)
        /// </summary>
        public ISparqlResultsHandler ResultsHandler { get; private set; }

        /// <summary>
        /// Gets the Query Results (if applicable)
        /// </summary>
        public Object QueryResults { get; private set; }

        /// <summary>
        /// Gets the SPARQL Query (if applicable)
        /// </summary>
        public String Query { get; private set; }

        /// <summary>
        /// Gets the SPARQL Update (if applicable)
        /// </summary>
        public String Updates { get; private set; }

        /// <summary>
        /// Gets the Store ID (if applicable)
        /// </summary>
        public String StoreID { get; private set; }

        /// <summary>
        /// Gets the list of Store IDs (if applicable)
        /// </summary>
        public IEnumerable<String> StoreIDs { get; private set; }

        /// <summary>
        /// Gets the Storage Provider (if applicable)
        /// </summary>
        /// <remarks>
        /// <para>
        /// For the <see cref="AsyncStorageOperation.GetStore"/> operation this will be the reference to the newly returned store instance
        /// </para>
        /// </remarks>
        public IAsyncStorageProvider StorageProvider { get; private set; }

        /// <summary>
        /// Gets the operation that was performed
        /// </summary>
        public AsyncStorageOperation Operation { get; private set; }
    }

    /// <summary>
    /// Generic callback for async storage API operations
    /// </summary>
    /// <param name="sender">Originator of the callback</param>
    /// <param name="args">Callback Arguments</param>
    /// <param name="state">State object originally passed to the async call</param>
    public delegate void AsyncStorageCallback(Object sender, AsyncStorageCallbackArgs args, Object state);
}
