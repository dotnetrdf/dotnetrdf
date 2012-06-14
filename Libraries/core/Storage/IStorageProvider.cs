/*

Copyright Robert Vesse 2009-12
rvesse@vdesign-studios.com

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
        /// Any Manager which indicates it is read-only should also return false for the <see cref="IStorageCapbilities.UpdateSupported">UpdatedSupported</see> property and should throw a <see cref="RdfStorageException">RdfStorageException</see> if the <strong>SaveGraph()</strong> or <strong>UpdateGraph()</strong> methods are called
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
    /// Interface for storage providers which provide the Read/Write functionality to some arbitrary storage layer
    /// </summary>
    /// <remarks>
    /// Designed to allow for arbitrary Triple Stores to be plugged into the library as required by the end user
    /// </remarks>
    public interface IStorageProvider
        : IStorageCapabilities, IDisposable
    {
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
        Object Query(String sparqlQuery);

        /// <summary>
        /// Makes a SPARQL Query against the underlying store processing the resulting Graph/Result Set with a handler of your choice
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">SPARQL Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery);
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

    public interface IAsyncStorageProvider
        : IStorageCapabilities, IDisposable
    {
        void LoadGraph(IGraph g, Uri graphUri, AsyncStorageCallback callback, Object state);

        void LoadGraph(IGraph g, String graphUri, AsyncStorageCallback callback, Object state);

        void LoadGraph(IRdfHandler handler, Uri graphUri, AsyncStorageCallback callback, Object state);

        void LoadGraph(IRdfHandler handler, String graphUri, AsyncStorageCallback callback, Object state);

        void SaveGraph(IGraph g, AsyncStorageCallback callback, Object state);

        void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, Object state);

        void UpdateGraph(String graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, Object state);

        void DeleteGraph(Uri graphUri, AsyncStorageCallback callback, Object state);

        void DeleteGraph(String graphUri, AsyncStorageCallback callback, Object state);

        void ListGraphs(AsyncStorageCallback callback, Object state);
    }

    public interface IAsyncQueryableStorage
        : IAsyncStorageProvider
    {
        void Query(String sparqlQuery, AsyncStorageCallback callback, Object state);

        void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery, AsyncStorageCallback callback, Object state);
    }

    public interface IAsyncUpdateableStorage
        : IAsyncQueryableStorage
    {
        void Update(String sparqlUpdates, AsyncStorageCallback callback, Object state);
    }

    /// <summary>
    /// Interface for storage providers which are capable of managing multiple stores
    /// </summary>
    public interface IStorageServer
    {
        /// <summary>
        /// Gets the list of available stores
        /// </summary>
        /// <returns></returns>
        IEnumerable<String> ListStores();

        /// <summary>
        /// Creates a new Store with the given ID
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <returns>Whether creation succeeded</returns>
        bool CreateStore(string storeID);

        /// <summary>
        /// Deletes the Store with the given ID
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <remarks>
        /// Whether attempting to delete the Store that you are accessing is permissible is up to the implementation
        /// </remarks>
        void DeleteStore(string storeID);

        /// <summary>
        /// Gets the Store with the given ID
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <returns></returns>
        /// <remarks>
        /// If the implementation is also an instance of <see cref="IStorageProvider">IStorageProvider</see> and the requested Store ID represents the current instance then it is acceptable for an implementation to return itself.  Consumers of this method should be aware of this and if necessary use other means to create a connection to a store if they want a unique instance of the provider.
        /// </remarks>
        IStorageProvider GetStore(string storeID);
    }

    public interface IAsyncStorageServer
    {
        void ListStores(AsyncStorageCallback callback, Object state);

        void CreateStore(String storeID, AsyncStorageCallback callback, Object state);

        void DeleteStore(String storeID, AsyncStorageCallback callback, Object state);

        void GetStore(String storeID, AsyncStorageCallback callback, Object state);
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

    public interface IAsyncTransactionalStorage
    {
        void Begin(AsyncStorageCallback callback, Object state);

        void Commit(AsyncStorageCallback callback, Object state);

        void Rollback(AsyncStorageCallback callback, Object state);
    }
}
