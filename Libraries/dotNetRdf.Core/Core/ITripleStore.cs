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

#nullable enable

using System;
using System.Collections.Generic;
using VDS.RDF.Parsing;

namespace VDS.RDF;

/// <summary>
/// Interface for Triple Stores.
/// </summary>
/// <remarks>A Triple Store may be a representation of some storage backed actual store or just a temporary collection of Graphs created for working with.  Note that an implementation is not required to provide a definitive view of a Triple Store and may only provide a limited/partial snapshot of the underlying store.  Check the documentation for the various implementations to see what type of view of a Triple Store they actually provide.</remarks>
public interface ITripleStore 
    : IDisposable
{
    #region Properties

    /// <summary>
    /// Gets whether a TripleStore is Empty.
    /// </summary>
    bool IsEmpty
    {
        get;
    }

    /// <summary>
    /// Gets the Graph Collection of Graphs in this Triple Store.
    /// </summary>
    BaseGraphCollection Graphs
    {
        get;
    }

    /// <summary>
    /// Gets all the Triples in the Triple Store which are currently loaded in memory (see remarks).
    /// </summary>
    /// <remarks>Since a Triple Store object may represent only a snapshot of the underlying Store evaluating this enumerator may only return some of the Triples in the Store and may depending on specific Triple Store return nothing.</remarks>
    IEnumerable<Triple> Triples
    {
        get;
    }
    
    /// <summary>
    /// Gets all the Quads in the Triple Store which are currently loaded in memory.
    /// </summary>
    /// <remarks>Since a Triple Store object may represent only a snapshot of the underlying Store evaluating this enumerator may only return some of the Quads in the Store and may depending on specific Triple Store return nothing.</remarks>
    IEnumerable<Quad> Quads { get; }

    /// <summary>
    /// Get the preferred URI factory to use when creating URIs in this store.
    /// </summary>
    IUriFactory UriFactory { get; }
    #endregion

    #region Assert & Retract Quads

    /// <summary>
    /// Assert a quad in the triple store.
    /// </summary>
    /// <param name="quad">The quad to be added.</param>
    /// <remarks>If the quad's graph is not currently in the triple store, a new graph will be added.</remarks>
    public void Assert(Quad quad);

    /// <summary>
    /// Remove a quad from the triple store.
    /// </summary>
    /// <param name="quad">The quad to be removed.</param>
    /// <remarks>If the quad's graph is not currently in the triple store, this operation will make no modification to the triple store. Otherwise it will invoke the Retract method on the graph.</remarks>
    public void Retract(Quad quad);

    #endregion
    #region Loading & Unloading Graphs

    /// <summary>
    /// Adds an empty graph with the specified name to the triple store.
    /// </summary>
    /// <param name="graphName"></param>
    /// <returns>True if a new graph was added, false otherwise.</returns>
    bool Add(IRefNode? graphName);

    /// <summary>
    /// Adds a Graph into the Triple Store.
    /// </summary>
    /// <param name="g">Graph to add.</param>
    bool Add(IGraph g);

    /// <summary>
    /// Adds a Graph into the Triple Store.
    /// </summary>
    /// <param name="g">Graph to add.</param>
    /// <param name="mergeIfExists">Controls whether the Graph should be merged with an existing Graph of the same Uri if it already exists in the Triple Store.</param>
    bool Add(IGraph g, bool mergeIfExists);

    /// <summary>
    /// Adds a Graph into the Triple Store by dereferencing the Graph Uri to get the RDF and then load the resulting Graph into the Triple Store.
    /// </summary>
    /// <param name="graphUri">Uri of the Graph to be added.</param>
    bool AddFromUri(Uri graphUri);

    /// <summary>
    /// Adds a Graph into the Triple Store by dereferencing the Graph Uri to get the RDF and then load the resulting Graph into the Triple Store.
    /// </summary>
    /// <param name="graphUri">Uri of the Graph to be added.</param>
    /// <param name="mergeIfExists">Controls whether the Graph should be merged with an existing Graph of the same Uri if it already exists in the Triple Store.</param>
    bool AddFromUri(Uri graphUri, bool mergeIfExists);

    /// <summary>
    /// Adds a Graph into the Triple Store by dereferencing the Graph Uri to get the RDF and then load the resulting Graph into the Triple Store.
    /// </summary>
    /// <param name="graphUri">Uri of the Graph to be added.</param>
    /// <param name="mergeIfExists">Controls whether the Graph should be merged with an existing Graph of the same Uri if it already exists in the Triple Store.</param>
    /// <param name="loader">The loader to use for retrieving and parsing the graph data.</param>
    bool AddFromUri(Uri graphUri, bool mergeIfExists, Loader loader);

    /// <summary>
    /// Removes a Graph from the Triple Store.
    /// </summary>
    /// <param name="graphUri">Graph Uri of the Graph to remove.</param>
    [Obsolete("Replaced by Remove(IRefNode)")]
    bool Remove(Uri? graphUri);

    /// <summary>
    /// Removes a graph from the triple store.
    /// </summary>
    /// <param name="graphName">The name of the graph to remove.</param>
    /// <returns>True if the operation removed a graph, false if no matching graph was found to remove.</returns>
    bool Remove(IRefNode? graphName);

    #endregion

    #region Graph Retrieval

    /// <summary>
    /// Checks whether the Graph with the given Uri is in this Triple Store.
    /// </summary>
    /// <param name="graphUri">Graph Uri.</param>
    /// <returns></returns>
    [Obsolete("Replaced by HasGraph(IRefNode)")]
    bool HasGraph(Uri graphUri);

    /// <summary>
    /// Checks whether the graph with the given name is in this triple store.
    /// </summary>
    /// <param name="graphName">The name of the graph to check for.</param>
    /// <returns>True if this store contains a graph with the specified name, false otherwise.</returns>
    /// <remarks>Pass null for<paramref name="graphName"/> to check for the default (unnamed) graph.</remarks>
    bool HasGraph(IRefNode? graphName);

    /// <summary>
    /// Gets a Graph from the Triple Store;.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    [Obsolete("Replaced by this[IRefNode]")]
    IGraph this[Uri graphUri]
    {
        get;
    }

    /// <summary>
    /// Gets a graph from the triple store.
    /// </summary>
    /// <param name="graphName">The name of the graph to be retrieved. May be null to retrieve the default (unnamed) graph.</param>
    /// <returns></returns>
    IGraph this[IRefNode? graphName] { get; }

    #endregion
    
    #region Quad Retrieval

    /// <summary>
    /// Return an enumeration of all quads in the store that match the specified subject, predicate, object and/or graph.
    /// </summary>
    /// <param name="s">The subject node to match. Null matches all subject nodes.</param>
    /// <param name="p">The predicate node to match. Null matches all predicate nodes.</param>
    /// <param name="o">The object node to match. Null matches all object nodes.</param>
    /// <param name="g">The graph to match. Null matches all graphs if <paramref name="allGraphs"/> is true, or only the unnamed graph is <paramref name="allGraphs"/> is false.</param>
    /// <param name="allGraphs"></param>
    /// <returns></returns>
    IEnumerable<Quad> GetQuads(INode? s = null, INode? p = null, INode? o = null, IRefNode? g = null, bool allGraphs = true);
    #endregion

    #region Events

    /// <summary>
    /// Event which is raised when a Graph is added
    /// </summary>
    event TripleStoreEventHandler? GraphAdded;

    /// <summary>
    /// Event which is raised when a Graph is removed
    /// </summary>
    event TripleStoreEventHandler? GraphRemoved;

    /// <summary>
    /// Event which is raised when a Graphs contents changes
    /// </summary>
    event TripleStoreEventHandler? GraphChanged;

    /// <summary>
    /// Event which is raised when a Graph is cleared
    /// </summary>
    event TripleStoreEventHandler? GraphCleared;

    /// <summary>
    /// Event which is raised when a Graph has a merge operation performed on it
    /// </summary>
    event TripleStoreEventHandler? GraphMerged;

    #endregion

}
