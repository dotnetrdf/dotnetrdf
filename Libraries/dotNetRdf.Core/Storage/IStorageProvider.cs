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
using VDS.RDF.Storage.Management;

namespace VDS.RDF.Storage;

/// <summary>
/// Interface for storage providers which provide the read/write functionality to some arbitrary storage layer.
/// </summary>
/// <remarks>
/// Designed to allow for arbitrary Triple Stores to be plugged into the library as required by the end user.
/// </remarks>
public interface IStorageProvider
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
    IStorageServer ParentServer
    {
        get;
    }

    /// <summary>
    /// Loads a Graph from the Store.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="graphUri">Uri of the Graph to load.</param>
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
    /// Loads a Graph from the Store.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    /// <remarks>
    /// <para>
    /// If the Graph being loaded into is Empty then it's Base Uri should become the Uri of the Graph being loaded, otherwise it should be merged into the existing non-empty Graph whose Base Uri should be unaffected.
    /// </para>
    /// <para>
    /// Behaviour of this method with regards to non-existent Graphs is up to the implementor, an empty Graph may be returned or an error thrown.  Implementors <strong>should</strong> state in the XML comments for their implementation what behaviour is implemented.
    /// </para>
    /// </remarks>
    void LoadGraph(IGraph g, string graphUri);

    /// <summary>
    /// Loads a Graph from the Store using the RDF Handler.
    /// </summary>
    /// <param name="handler">RDF Handler.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    /// <remarks>
    /// <para>
    /// Behaviour of this method with regards to non-existent Graphs is up to the implementor, an empty Graph may be returned or an error thrown.  Implementors <strong>should</strong> state in the XML comments for their implementation what behaviour is implemented.
    /// </para>
    /// </remarks>
    void LoadGraph(IRdfHandler handler, Uri graphUri);

    /// <summary>
    /// Loads a Graph from the Store using the RDF Handler.
    /// </summary>
    /// <param name="handler">RDF Handler.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    /// <remarks>
    /// <para>
    /// Behaviour of this method with regards to non-existent Graphs is up to the implementor, an empty Graph may be returned or an error thrown.  Implementors <strong>should</strong> state in the XML comments for their implementation what behaviour is implemented.
    /// </para>
    /// </remarks>
    void LoadGraph(IRdfHandler handler, string graphUri);

    /// <summary>
    /// Saves a Graph to the Store.
    /// </summary>
    /// <param name="g">Graph to Save.</param>
    /// <remarks>
    /// Graph name should be taken from the <see cref="IGraph.Name"/> property
    /// <br /><br />
    /// Behaviour of this method with regards to whether it overwrites/updates/merges with existing Graphs of the same name is up to the implementor and may be dependent on the underlying store.
    /// Implementors <strong>should</strong> state in the XML comments for their implementations what behaviour is implemented.
    /// </remarks>
    void SaveGraph(IGraph g);

    /// <summary>
    /// Updates a Graph in the Store.
    /// </summary>
    /// <param name="graphName">Name of the Graph to update.</param>
    /// <param name="additions">Triples to add to the Graph.</param>
    /// <param name="removals">Triples to remove from the Graph.</param>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> Not all Stores are capable of supporting update at the individual Triple level and as such it is acceptable for such a Store to throw a <see cref="NotSupportedException">NotSupportedException</see> or an <see cref="RdfStorageException">RdfStorageException</see> if the Store cannot provide this functionality.
    /// </para>
    /// <para>
    /// Behaviour of this method with regards to non-existent Graph is up to the implementor, it may create a new empty Graph and apply the updates to that or it may throw an error.  Implementors <strong>should</strong> state in the XML comments for their implementation what behaviour is implemented.
    /// </para>
    /// <para>
    /// Implementers <strong>MUST</strong> allow for either the additions or removals argument to be null.
    /// </para>
    /// </remarks>
    /// <exception cref="NotSupportedException">May be thrown if the underlying Store is not capable of doing Updates at the Triple level.</exception>
    /// <exception cref="RdfStorageException">May be thrown if the underlying Store is not capable of doing Updates at the Triple level or if some error occurs while attempting the Update.</exception>
    void UpdateGraph(IRefNode graphName, IEnumerable<Triple> additions, IEnumerable<Triple> removals);

    /// <summary>
    /// Updates a Graph in the Store.
    /// </summary>
    /// <param name="graphUri">Uri of the Graph to update.</param>
    /// <param name="additions">Triples to add to the Graph.</param>
    /// <param name="removals">Triples to remove from the Graph.</param>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> Not all Stores are capable of supporting update at the individual Triple level and as such it is acceptable for such a Store to throw a <see cref="NotSupportedException">NotSupportedException</see> if the Store cannot provide this functionality.
    /// </para>
    /// <para>
    /// Behaviour of this method with regards to non-existent Graph is up to the implementor, it may create a new empty Graph and apply the updates to that or it may throw an error.  Implementors <strong>should</strong> state in the XML comments for their implementation what behaviour is implemented.
    /// </para>
    /// <para>
    /// Implementers <strong>MUST</strong> allow for either the additions or removals argument to be null.
    /// </para>
    /// </remarks>
    /// <exception cref="NotSupportedException">May be thrown if the underlying Store is not capable of doing Updates at the Triple level.</exception>
    /// <exception cref="RdfStorageException">May be thrown if the underlying Store is not capable of doing Updates at the Triple level or if some error occurs while attempting the Update.</exception>
    [Obsolete("Replaced by UpdateGraph(IRefNode, IEnumerable<Triple>, IEnumerable<Triple>)")]
    void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals);

    /// <summary>
    /// Updates a Graph in the Store.
    /// </summary>
    /// <param name="graphUri">Uri of the Graph to update.</param>
    /// <param name="additions">Triples to add to the Graph.</param>
    /// <param name="removals">Triples to remove from the Graph.</param>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> Not all Stores are capable of supporting update at the individual Triple level and as such it is acceptable for such a Store to throw a <see cref="NotSupportedException">NotSupportedException</see> or an <see cref="RdfStorageException">RdfStorageException</see> if the Store cannot provide this functionality.
    /// </para>
    /// <para>
    /// Behaviour of this method with regards to non-existent Graph is up to the implementor, it may create a new empty Graph and apply the updates to that or it may throw an error.  Implementors <strong>should</strong> state in the XML comments for their implementation what behaviour is implemented.
    /// </para>
    /// <para>
    /// Implementers <strong>MUST</strong> allow for either the additions or removals argument to be null.
    /// </para>
    /// </remarks>
    /// <exception cref="NotSupportedException">May be thrown if the underlying Store is not capable of doing Updates at the Triple level.</exception>
    /// <exception cref="RdfStorageException">May be thrown if the underlying Store is not capable of doing Updates at the Triple level or if some error occurs while attempting the Update.</exception>
    [Obsolete("Replaced by UpdateGraph(IRefNode, IEnumerable<Triple>, IEnumerable<Triple>)")] 
    void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals);

    /// <summary>
    /// Deletes a Graph from the Store.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to be deleted.</param>
    /// <exception cref="NotSupportedException">May be thrown if the underlying Store is not capable of doing Deleting a Graph.</exception>
    /// <exception cref="RdfStorageException">May be thrown if the underlying Store is not capable of Deleting a Graph or an error occurs while performing the delete.</exception>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> Not all Stores are capable of Deleting a Graph so it is acceptable for such a Store to throw a <see cref="NotSupportedException">NotSupportedException</see> or an <see cref="RdfStorageException">RdfStorageException</see> if the Store cannot provide this functionality.
    /// </para>
    /// </remarks>
    void DeleteGraph(Uri graphUri);

    /// <summary>
    /// Deletes a Graph from the Store.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to be deleted.</param>
    /// <exception cref="NotSupportedException">May be thrown if the underlying Store is not capable of doing Deleting a Graph.</exception>
    /// <exception cref="RdfStorageException">May be thrown if the underlying Store is not capable of Deleting a Graph or an error occurs while performing the delete.</exception>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> Not all Stores are capable of Deleting a Graph so it is acceptable for such a Store to throw a <see cref="NotSupportedException">NotSupportedException</see> or an <see cref="RdfStorageException">RdfStorageException</see> if the Store cannot provide this functionality.
    /// </para>
    /// </remarks>
    void DeleteGraph(string graphUri);


    /// <summary>
    /// Gets a List of Graph URIs for the graphs in the store.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// Implementations should implement this method only if they need to provide a custom way of listing Graphs.  If the Store for which you are providing a manager can efficiently return the Graphs using a SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } } query then there should be no need to implement this function.
    /// </para>
    /// </remarks>
    [Obsolete("Replaced by ListGraphNames")]
    IEnumerable<Uri> ListGraphs();

    /// <summary>
    /// Gets an enumeration of the names of the graphs in the store.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// Implementations should implement this method only if they need to provide a custom way of listing Graphs.  If the Store for which you are providing a manager can efficiently return the Graphs using a SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } } query then there should be no need to implement this function.
    /// </para>
    /// </remarks>
    IEnumerable<string> ListGraphNames();
}
