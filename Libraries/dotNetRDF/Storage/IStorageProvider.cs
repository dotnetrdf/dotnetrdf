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
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage.Management;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Interface which describes the capabilities of some storage provider
    /// </summary>
    public interface IStorageCapabilities
    {
        /// <summary>
        /// Gets whether the connection with the underlying Store is ready for use
        /// </summary>
        bool IsReady
        {
            get;
        }

        /// <summary>
        /// Gets whether the connection with the underlying Store is read-only
        /// </summary>
        /// <remarks>
        /// Any Manager which indicates it is read-only should also return false for the <see cref="IStorageCapabilities.UpdateSupported">UpdatedSupported</see> property and should throw a <see cref="RdfStorageException">RdfStorageException</see> if the <strong>SaveGraph()</strong> or <strong>UpdateGraph()</strong> methods are called
        /// </remarks>
        bool IsReadOnly
        {
            get;
        }

        /// <summary>
        /// Gets the Save Behaviour the Store uses
        /// </summary>
        IOBehaviour IOBehaviour
        {
            get;
        }


        /// <summary>
        /// Gets whether the triple level updates are supported
        /// </summary>
        /// <remarks>
        /// Some Stores do not support updates at the Triple level and may as designated in the interface defintion throw a <see cref="NotSupportedException">NotSupportedException</see> if the <strong>UpdateGraph()</strong> method is called.  This property allows for calling code to check in advance whether Updates are supported
        /// </remarks>
        bool UpdateSupported
        {
            get;
        }

        /// <summary>
        /// Gets whether the deletion of graphs is supported
        /// </summary>
        /// <remarks>
        /// Some Stores do not support the deletion of Graphs and may as designated in the interface definition throw a <see cref="NotSupportedException">NotSupportedException</see> if the <strong>DeleteGraph()</strong> method is called.  This property allows for calling code to check in advance whether Deletion of Graphs is supported.
        /// </remarks>
        bool DeleteSupported
        {
            get;
        }

        /// <summary>
        /// Gets whether the Store supports Listing Graphs
        /// </summary>
        bool ListGraphsSupported
        {
            get;
        }
    }

    /// <summary>
    /// Interface for storage providers which provide the read/write functionality to some arbitrary storage layer
    /// </summary>
    /// <remarks>
    /// Designed to allow for arbitrary Triple Stores to be plugged into the library as required by the end user
    /// </remarks>
    public interface IStorageProvider
        : IStorageCapabilities, IDisposable
    {
        /// <summary>
        /// Gets the Parent Server on which this store is hosted (if any)
        /// </summary>
        /// <remarks>
        /// <para>
        /// For storage backends which support multiple stores this is useful because it provides a way to access all the stores on that backend.  For stores which are standalone they should simply return null
        /// </para>
        /// </remarks>
        IStorageServer ParentServer
        {
            get;
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Uri of the Graph to load</param>
        /// <remarks>
        /// <para>
        /// If the Graph being loaded into is Empty then it's Base Uri should become the Uri of the Graph being loaded, otherwise it should be merged into the existing non-empty Graph whose Base Uri should be unaffected.
        /// </para>
        /// <para>
        /// Behaviour of this method with regards to non-existent Graphs is up to the implementor, an empty Graph may be returned or an error thrown.  Implementors <strong>should</strong> state in the XML comments for their implementation what behaviour is implemented.
        /// </para>
        /// </remarks>
        void LoadGraph(IGraph g, Uri graphUri);

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <remarks>
        /// <para>
        /// If the Graph being loaded into is Empty then it's Base Uri should become the Uri of the Graph being loaded, otherwise it should be merged into the existing non-empty Graph whose Base Uri should be unaffected.
        /// </para>
        /// <para>
        /// Behaviour of this method with regards to non-existent Graphs is up to the implementor, an empty Graph may be returned or an error thrown.  Implementors <strong>should</strong> state in the XML comments for their implementation what behaviour is implemented.
        /// </para>
        /// </remarks>
        void LoadGraph(IGraph g, String graphUri);

        /// <summary>
        /// Loads a Graph from the Store using the RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <remarks>
        /// <para>
        /// Behaviour of this method with regards to non-existent Graphs is up to the implementor, an empty Graph may be returned or an error thrown.  Implementors <strong>should</strong> state in the XML comments for their implementation what behaviour is implemented.
        /// </para>
        /// </remarks>
        void LoadGraph(IRdfHandler handler, Uri graphUri);

        /// <summary>
        /// Loads a Graph from the Store using the RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <remarks>
        /// <para>
        /// Behaviour of this method with regards to non-existent Graphs is up to the implementor, an empty Graph may be returned or an error thrown.  Implementors <strong>should</strong> state in the XML comments for their implementation what behaviour is implemented.
        /// </para>
        /// </remarks>
        void LoadGraph(IRdfHandler handler, String graphUri);

        /// <summary>
        /// Saves a Graph to the Store
        /// </summary>
        /// <param name="g">Graph to Save</param>
        /// <remarks>
        /// Uri of the Graph should be taken from the <see cref="IGraph.BaseUri">BaseUri</see> property
        /// <br /><br />
        /// Behaviour of this method with regards to whether it overwrites/updates/merges with existing Graphs of the same Uri is up to the implementor and may be dependent on the underlying store.  Implementors <strong>should</strong> state in the XML comments for their implementations what behaviour is implemented.
        /// </remarks>
        void SaveGraph(IGraph g);

        /// <summary>
        /// Updates a Graph in the Store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to update</param>
        /// <param name="additions">Triples to add to the Graph</param>
        /// <param name="removals">Triples to remove from the Graph</param>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> Not all Stores are capable of supporting update at the individual Triple level and as such it is acceptable for such a Store to throw a <see cref="NotSupportedException">NotSupportedException</see> if the Store cannot provide this functionality
        /// </para>
        /// <para>
        /// Behaviour of this method with regards to non-existent Graph is up to the implementor, it may create a new empty Graph and apply the updates to that or it may throw an error.  Implementors <strong>should</strong> state in the XML comments for their implementation what behaviour is implemented.
        /// </para>
        /// <para>
        /// Implementers <strong>MUST</strong> allow for either the additions or removals argument to be null
        /// </para>
        /// </remarks>
        /// <exception cref="NotSupportedException">May be thrown if the underlying Store is not capable of doing Updates at the Triple level</exception>
        /// <exception cref="RdfStorageException">May be thrown if the underlying Store is not capable of doing Updates at the Triple level or if some error occurs while attempting the Update</exception>
        void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals);

        /// <summary>
        /// Updates a Graph in the Store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to update</param>
        /// <param name="additions">Triples to add to the Graph</param>
        /// <param name="removals">Triples to remove from the Graph</param>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> Not all Stores are capable of supporting update at the individual Triple level and as such it is acceptable for such a Store to throw a <see cref="NotSupportedException">NotSupportedException</see> or an <see cref="RdfStorageException">RdfStorageException</see> if the Store cannot provide this functionality
        /// </para>
        /// <para>
        /// Behaviour of this method with regards to non-existent Graph is up to the implementor, it may create a new empty Graph and apply the updates to that or it may throw an error.  Implementors <strong>should</strong> state in the XML comments for their implementation what behaviour is implemented.
        /// </para>
        /// <para>
        /// Implementers <strong>MUST</strong> allow for either the additions or removals argument to be null
        /// </para>
        /// </remarks>
        /// <exception cref="NotSupportedException">May be thrown if the underlying Store is not capable of doing Updates at the Triple level</exception>
        /// <exception cref="RdfStorageException">May be thrown if the underlying Store is not capable of doing Updates at the Triple level or if some error occurs while attempting the Update</exception>
        void UpdateGraph(String graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals);

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to be deleted</param>
        /// <exception cref="NotSupportedException">May be thrown if the underlying Store is not capable of doing Deleting a Graph</exception>
        /// <exception cref="RdfStorageException">May be thrown if the underlying Store is not capable of Deleting a Graph or an error occurs while performing the delete</exception>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> Not all Stores are capable of Deleting a Graph so it is acceptable for such a Store to throw a <see cref="NotSupportedException">NotSupportedException</see> or an <see cref="RdfStorageException">RdfStorageException</see> if the Store cannot provide this functionality
        /// </para>
        /// </remarks>
        void DeleteGraph(Uri graphUri);

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to be deleted</param>
        /// <exception cref="NotSupportedException">May be thrown if the underlying Store is not capable of doing Deleting a Graph</exception>
        /// <exception cref="RdfStorageException">May be thrown if the underlying Store is not capable of Deleting a Graph or an error occurs while performing the delete</exception>
        /// <remarks>
        /// <para>
        /// <strong>Note:</strong> Not all Stores are capable of Deleting a Graph so it is acceptable for such a Store to throw a <see cref="NotSupportedException">NotSupportedException</see> or an <see cref="RdfStorageException">RdfStorageException</see> if the Store cannot provide this functionality
        /// </para>
        /// </remarks>
        void DeleteGraph(String graphUri);

        /// <summary>
        /// Gets a List of Graph URIs for the graphs in the store
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Implementations should implement this method only if they need to provide a custom way of listing Graphs.  If the Store for which you are providing a manager can efficiently return the Graphs using a SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } } query then there should be no need to implement this function.
        /// </para>
        /// </remarks>
        IEnumerable<Uri> ListGraphs();
    }

    /// <summary>
    /// Interface for storage providers which allow SPARQL Queries to be made against them
    /// </summary>
    public interface IQueryableStorage
        : IStorageProvider
    {
        /// <summary>
        /// Makes a SPARQL Query against the underlying store
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns><see cref="SparqlResultSet">SparqlResultSet</see> or a <see cref="Graph">Graph</see> depending on the Sparql Query</returns>
        /// <exception cref="RdfQueryException">Thrown if an error occurs performing the query</exception>
        /// <exception cref="RdfStorageException">Thrown if an error occurs performing the query</exception>
        /// <exception cref="RdfParseException">Thrown if the query is invalid when validated by dotNetRDF prior to passing the query request to the store or if the request succeeds but the store returns malformed results</exception>
        /// <exception cref="RdfParserSelectionException">Thrown if the store returns results in a format dotNetRDF does not understand</exception>
        Object Query(String sparqlQuery);

        /// <summary>
        /// Makes a SPARQL Query against the underlying store processing the resulting Graph/Result Set with a handler of your choice
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">SPARQL Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <exception cref="RdfQueryException">Thrown if an error occurs performing the query</exception>
        /// <exception cref="RdfStorageException">Thrown if an error occurs performing the query</exception>
        /// <exception cref="RdfParseException">Thrown if the query is invalid when validated by dotNetRDF prior to passing the query request to the store or if the request succeeds but the store returns malformed results</exception>
        /// <exception cref="RdfParserSelectionException">Thrown if the store returns results in a format dotNetRDF does not understand</exception>
        void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery);
    }

    /// <summary>
    /// Interface for storage providers which allow SPARQL Queries to be made against them with reasoning set by query
    /// </summary>
    public interface IReasoningQueryableStorage
        : IStorageProvider
    {
        /// <summary>
        /// Makes a SPARQL Query against the underlying store
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="reasoning">rReasoning On demand by query</param>
        /// <returns><see cref="SparqlResultSet">SparqlResultSet</see> or a <see cref="Graph">Graph</see> depending on the Sparql Query</returns>
        /// <exception cref="RdfQueryException">Thrown if an error occurs performing the query</exception>
        /// <exception cref="RdfStorageException">Thrown if an error occurs performing the query</exception>
        /// <exception cref="RdfParseException">Thrown if the query is invalid when validated by dotNetRDF prior to passing the query request to the store or if the request succeeds but the store returns malformed results</exception>
        /// <exception cref="RdfParserSelectionException">Thrown if the store returns results in a format dotNetRDF does not understand</exception>
        Object Query(String sparqlQuery, bool reasoning);

        /// <summary>
        /// Makes a SPARQL Query against the underlying store processing the resulting Graph/Result Set with a handler of your choice
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">SPARQL Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="reasoning">rReasoning On demand by query</param>
        /// <exception cref="RdfQueryException">Thrown if an error occurs performing the query</exception>
        /// <exception cref="RdfStorageException">Thrown if an error occurs performing the query</exception>
        /// <exception cref="RdfParseException">Thrown if the query is invalid when validated by dotNetRDF prior to passing the query request to the store or if the request succeeds but the store returns malformed results</exception>
        /// <exception cref="RdfParserSelectionException">Thrown if the store returns results in a format dotNetRDF does not understand</exception>
        void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery, bool reasoning
            );
    }


    /// <summary>
    /// Interface for storage providers which allow SPARQL Updates to be made against them
    /// </summary>
    public interface IUpdateableStorage
        : IQueryableStorage
    {
        /// <summary>
        /// Processes a SPARQL Update command against the underlying Store
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update</param>
        void Update(String sparqlUpdate);
    }

    /// <summary>
    /// Interface for storage providers which provide asynchronous read/write functionality to some arbitrary storage layer
    /// </summary>
    /// <remarks>
    /// Designed to allow for arbitrary Triple Stores to be plugged into the library as required by the end user
    /// </remarks>
    public interface IAsyncStorageProvider
        : IStorageCapabilities, IDisposable
    {
        /// <summary>
        /// Gets the Parent Server on which this store is hosted (if any)
        /// </summary>
        /// <remarks>
        /// <para>
        /// For storage backends which support multiple stores this is useful because it provides a way to access all the stores on that backend.  For stores which are standalone they should simply return null
        /// </para>
        /// </remarks>
        IAsyncStorageServer AsyncParentServer
        {
            get;
        }

        /// <summary>
        /// Loads a Graph from the Store asynchronously
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        void LoadGraph(IGraph g, Uri graphUri, AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Loads a Graph from the Store asynchronously
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        void LoadGraph(IGraph g, String graphUri, AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Loads a Graph from the Store asynchronously
        /// </summary>
        /// <param name="handler">Handler to load with</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        void LoadGraph(IRdfHandler handler, Uri graphUri, AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Loads a Graph from the Store asynchronously
        /// </summary>
        /// <param name="handler">Handler to load with</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        void LoadGraph(IRdfHandler handler, String graphUri, AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Saves a Graph to the Store asynchronously
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        void SaveGraph(IGraph g, AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Updates a Graph in the Store asychronously
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Updates a Graph in the Store asychronously
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        void UpdateGraph(String graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        void DeleteGraph(Uri graphUri, AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        void DeleteGraph(String graphUri, AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Lists the Graphs in the Store asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        void ListGraphs(AsyncStorageCallback callback, Object state);
    }

    /// <summary>
    /// Interface for storage providers which allow SPARQL Queries to be made against them asynchronously
    /// </summary>
    public interface IAsyncQueryableStorage
        : IAsyncStorageProvider
    {
        /// <summary>
        /// Queries the store asynchronously
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <exception cref="RdfQueryException">Thrown if an error occurs performing the query</exception>
        /// <exception cref="RdfStorageException">Thrown if an error occurs performing the query</exception>
        /// <exception cref="RdfParseException">Thrown if the query is invalid when validated by dotNetRDF prior to passing the query request to the store or if the request succeeds but the store returns malformed results</exception>
        /// <exception cref="RdfParserSelectionException">Thrown if the store returns results in a format dotNetRDF does not understand</exception>
        void Query(String sparqlQuery, AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Queries the store asynchronously
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <exception cref="RdfQueryException">Thrown if an error occurs performing the query</exception>
        /// <exception cref="RdfStorageException">Thrown if an error occurs performing the query</exception>
        /// <exception cref="RdfParseException">Thrown if the query is invalid when validated by dotNetRDF prior to passing the query request to the store or if the request succeeds but the store returns malformed results</exception>
        /// <exception cref="RdfParserSelectionException">Thrown if the store returns results in a format dotNetRDF does not understand</exception>
        void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery, AsyncStorageCallback callback, Object state);
    }

    /// <summary>
    /// Interface for storage providers which allow SPARQL Updates to be made against them asynchronously
    /// </summary>
    public interface IAsyncUpdateableStorage
        : IAsyncQueryableStorage
    {
        /// <summary>
        /// Updates the store asynchronously
        /// </summary>
        /// <param name="sparqlUpdates">SPARQL Update</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        void Update(String sparqlUpdates, AsyncStorageCallback callback, Object state);
    }

    /// <summary>
    /// Interface for storage providers which have controllable transactions
    /// </summary>
    /// <remarks>
    /// <para>
    /// It is up to the implementation whether transactions are per-thread or global and how transactions interact with operations performed on the storage provider.  Please see individual implementations for notes on how transactions are implemented.
    /// </para>
    /// </remarks>
    public interface ITransactionalStorage
    {
        /// <summary>
        /// Begins a transaction
        /// </summary>
        void Begin();

        /// <summary>
        /// Commits a transaction
        /// </summary>
        void Commit();

        /// <summary>
        /// Rolls back a transaction
        /// </summary>
        void Rollback();
    }

    /// <summary>
    /// Interface for storage providers which have controllable transactions which can be managed asynchronously
    /// </summary>
    public interface IAsyncTransactionalStorage
    {
        /// <summary>
        /// Begins a transaction asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        void Begin(AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Commits a transaction asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        void Commit(AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Rolls back a transaction asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        void Rollback(AsyncStorageCallback callback, Object state);
    }
}
