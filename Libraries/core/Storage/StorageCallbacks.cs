/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage.Management.Provisioning;

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
        /// Gettting a new store template
        /// </summary>
        NewTemplate,
        /// <summary>
        /// Getting all available templates
        /// </summary>
        AvailableTemplates,
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
        ListStores,
        /// <summary>
        /// Unknown Action
        /// </summary>
        Unknown
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
        /// <summary>
        /// Creates new callback arguments
        /// </summary>
        /// <param name="operation">Operation</param>
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation)
        {
            this.Operation = operation;
        }

        /// <summary>
        /// Creates new callback arguments
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="ex">Error that occurred</param>
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, Exception ex)
            : this(operation)
        {
            this.Error = ex;
        }

        /// <summary>
        /// Creates new callback arguments
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="g">Graph to return</param>
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, IGraph g)
            : this(operation, g, null) { }

        /// <summary>
        /// Creates new callback arguments
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="g">Graph to return</param>
        /// <param name="e">Error that occurred</param>
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, IGraph g, Exception e)
            : this(operation, e)
        {
            this.Graph = g;
        }

        /// <summary>
        /// Creates new callback arguments
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="graphUri">URI of the affected Graph</param>
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, Uri graphUri)
            : this(operation, graphUri, null) { }

        /// <summary>
        /// Creates new callback arguments
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="graphUri">URI of the affected Graph</param>
        /// <param name="e">Error that occurred</param>
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, Uri graphUri, Exception e)
            : this(operation, e)
        {
            this.GraphUri = graphUri;
        }

        /// <summary>
        /// Creates new callback arguments
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="graphUris">Enumeration of Graph URIs</param>
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, IEnumerable<Uri> graphUris)
            : this(operation)
        {
            this.GraphUris = graphUris;
        }

        /// <summary>
        /// Creates new callback arguments
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="handler">Handler to return</param>
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, IRdfHandler handler)
            : this(operation, handler, null) { }

        /// <summary>
        /// Creates new callback arguments
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="handler">Handler to return</param>
        /// <param name="e">Error that occurred</param>
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, IRdfHandler handler, Exception e)
            : this(operation, e)
        {
            this.RdfHandler = handler;
        }

        /// <summary>
        /// Creates new callback arguments
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="query">SPARQL Query</param>
        /// <param name="rdfHandler">RDF Handler to return</param>
        /// <param name="resultsHandler">Results Handler to return</param>
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, String query, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler)
            : this(operation, query, rdfHandler, resultsHandler, null) { }

        /// <summary>
        /// Creates new callback arguments
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="query">SPARQL Query</param>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="e">Error that occurred</param>
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, String query, IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, Exception e)
            : this(operation, e)
        {
            this.SetData(query);
            this.RdfHandler = rdfHandler;
            this.ResultsHandler = resultsHandler;
        }

        /// <summary>
        /// Creates new callback arguments
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="query">SPARQL Query</param>
        /// <param name="queryResults">Results to return</param>
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, String query, Object queryResults)
            : this(operation, queryResults as IGraph)
        {
            this.SetData(query);
            this.QueryResults = queryResults;
        }

        /// <summary>
        /// Creates new callback arguments
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="query">SPARQL Query</param>
        /// <param name="queryResults">Results to return</param>
        /// <param name="e">Error that occurred</param>
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, String query, Object queryResults, Exception e)
            : this(operation, e)
        {
            this.SetData(query);
            this.QueryResults = queryResults;
        }

        /// <summary>
        /// Creates new callback arguments
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="data">Data to return</param>
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, String data)
            : this(operation, data, (Exception)null) { }

        /// <summary>
        /// Creates new callback arguments
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="data">Data to return</param>
        /// <param name="e">Error that occurred</param>
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, String data, Exception e)
            : this(operation, e)
        {
            this.SetData(data);
        }

        /// <summary>
        /// Creates new callback arguments
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="stores">Enumeration of Store IDs</param>
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, IEnumerable<String> stores)
            : this(operation, stores, null) { }

        /// <summary>
        /// Creates new callback arguments
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="stores">Enumeration of Store IDs</param>
        /// <param name="e">Error that occurred</param>
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, IEnumerable<String> stores, Exception e)
            : this(operation, e)
        {
            this.StoreIDs = stores;
        }

        /// <summary>
        /// Creates new callback arguments
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="provider">Storage Provider</param>
        /// <param name="e">Error that occurred</param>
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, String storeID, IAsyncStorageProvider provider, Exception e)
            : this(operation, storeID, e)
        {
            this.StorageProvider = provider;
        }

        /// <summary>
        /// Creates new callback arguments
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="template">Template</param>
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, String storeID, IStoreTemplate template)
            : this(operation, storeID)
        {
            this.Template = template;
        }

        /// <summary>
        /// Creates new callback arguments
        /// </summary>
        /// <param name="operation">Operation</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="templates">Templates</param>
        public AsyncStorageCallbackArgs(AsyncStorageOperation operation, String storeID, IEnumerable<IStoreTemplate> templates)
            : this(operation, storeID)
        {
            this.AvailableTemplates = templates;
        }

        /// <summary>
        /// Sets the Data to the appropriate property based on the operation type
        /// </summary>
        /// <param name="data">Data</param>
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
                case AsyncStorageOperation.NewTemplate:
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

        /// <summary>
        /// Gets the template that was created (if any)
        /// </summary>
        public IStoreTemplate Template { get; private set; }

        /// <summary>
        /// Gets the templates that were created (if any)
        /// </summary>
        public IEnumerable<IStoreTemplate> AvailableTemplates { get; private set; }
    }

    /// <summary>
    /// Generic callback for async storage API operations
    /// </summary>
    /// <param name="sender">Originator of the callback</param>
    /// <param name="args">Callback Arguments</param>
    /// <param name="state">State object originally passed to the async call</param>
    public delegate void AsyncStorageCallback(Object sender, AsyncStorageCallbackArgs args, Object state);
}
