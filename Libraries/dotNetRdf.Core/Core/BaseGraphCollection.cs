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
using System.Linq;

namespace VDS.RDF;

/// <summary>
/// Abstract Base Class for Graph Collections.
/// </summary>
/// <remarks>Designed to allow the underlying storage of a Graph Collection to be changed at a later date without affecting classes that use it.</remarks>
public abstract class BaseGraphCollection 
    : IEnumerable<IGraph>, IDisposable
{
    /// <summary>
    /// Checks whether the Graph with the given Uri exists in this Graph Collection.
    /// </summary>
    /// <param name="graphUri">Graph Uri to test.</param>
    /// <returns></returns>
    /// <remarks>
    /// The null URI is used to reference the Default Graph.
    /// </remarks>
    [Obsolete("Replaced by Contains(IRefNode)")]
    public abstract bool Contains(Uri graphUri);

    /// <summary>
    /// Checks whether the graph with the given name exists in this graph collection.
    /// </summary>
    /// <param name="graphName">Graph name to test for.</param>
    /// <returns>True if a graph with the specified name is in the collection, false otherwise.</returns>
    /// <remarks>The null value is used to reference the default (unnamed) graph.</remarks>
    public abstract bool Contains(IRefNode graphName);

    /// <summary>
    /// Adds a Graph to the Collection.
    /// </summary>
    /// <param name="g">Graph to add.</param>
    /// <param name="mergeIfExists">Sets whether the Graph should be merged with an existing Graph of the same Uri if present.</param>
    public abstract bool Add(IGraph g, bool mergeIfExists);

    /// <summary>
    /// Removes a Graph from the Collection.
    /// </summary>
    /// <param name="graphUri">Uri of the Graph to remove.</param>
    /// <remarks>
    /// The null URI is used to reference the Default Graph.
    /// </remarks>
    [Obsolete("Replaced by Remove(IRefNode)")]
    public abstract bool Remove(Uri graphUri);

    /// <summary>
    /// Removes a graph from the collection.
    /// </summary>
    /// <param name="graphName">Name of the Graph to remove.</param>
    /// <remarks>
    /// The null value is used to reference the Default Graph.
    /// </remarks>
    public abstract bool Remove(IRefNode graphName);

    /// <summary>
    /// Gets the number of Graphs in the Collection.
    /// </summary>
    public abstract int Count
    {
        get;
    }

    /// <summary>
    /// Provides access to the Graph URIs of Graphs in the Collection.
    /// </summary>
    [Obsolete("Replaced by GraphNames")]
    public abstract IEnumerable<Uri> GraphUris
    {
        get;
    }

    /// <summary>
    /// Provides an enumeration of the names of all of teh graphs in the collection.
    /// </summary>
    public abstract IEnumerable<IRefNode> GraphNames { get; }

    /// <summary>
    /// Gets a Graph from the Collection.
    /// </summary>
    /// <param name="graphUri">Graph Uri.</param>
    /// <returns></returns>
    /// <remarks>
    /// The null URI is used to reference the Default Graph.
    /// </remarks>
    [Obsolete("Replaced by this[IRefNode]")]
    public abstract IGraph this[Uri graphUri] 
    {
        get;
    }

    /// <summary>
    /// Gets a graph from the collection.
    /// </summary>
    /// <param name="graphName">The name of the graph to retrieve.</param>
    /// <returns></returns>
    /// <remarks>The null value is used to reference the default graph.</remarks>
    public abstract IGraph this[IRefNode graphName] { get; }

    /// <summary>
    /// Disposes of the Graph Collection.
    /// </summary>
    /// <remarks>Invokes the <see cref="IDisposable.Dispose()">Dispose()</see> method of all Graphs contained in the Collection.</remarks>
    public abstract void Dispose();

    /// <summary>
    /// Gets the Enumerator for the Collection.
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerator<IGraph> GetEnumerator();

    /// <summary>
    /// Gets the Enumerator for this Collection.
    /// </summary>
    /// <returns></returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return Enumerable.Empty<IGraph>().GetEnumerator();
    }

    /// <summary>
    /// Event which is raised when a Graph is added to the Collection
    /// </summary>
    public event GraphEventHandler GraphAdded;

    /// <summary>
    /// Event which is raised when a Graph is removed from the Collection
    /// </summary>
    public event GraphEventHandler GraphRemoved;

    /// <summary>
    /// Helper method which raises the <see cref="GraphAdded">Graph Added</see> event manually.
    /// </summary>
    /// <param name="g">Graph.</param>
    protected virtual void RaiseGraphAdded(IGraph g)
    {
        GraphAdded?.Invoke(this, new GraphEventArgs(g));
    }

    /// <summary>
    /// Helper method which raises the <see cref="GraphRemoved">Graph Removed</see> event manually.
    /// </summary>
    /// <param name="g">Graph.</param>
    protected virtual void RaiseGraphRemoved(IGraph g)
    {
        GraphRemoved?.Invoke(this, new GraphEventArgs(g));
    }
}
