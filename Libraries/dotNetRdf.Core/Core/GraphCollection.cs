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
using VDS.Common.Collections;

namespace VDS.RDF;

/// <summary>
/// Wrapper class for Graph Collections.
/// </summary>
public class GraphCollection 
    : BaseGraphCollection, IEnumerable<IGraph>
{
    /// <summary>
    /// Internal Constant used as the Hash Code for the default graph.
    /// </summary>
    protected const int DefaultGraphId = 0;

    /// <summary>
    /// Dictionary of Graph Uri Enhanced Hash Codes to Graphs.
    /// </summary>
    /// <remarks>See <see cref="Extensions.GetEnhancedHashCode">GetEnhancedHashCode()</see>.</remarks>
    protected MultiDictionary<IRefNode, IGraph> _graphs;

    /// <summary>
    /// Creates a new Graph Collection.
    /// </summary>
    public GraphCollection()
    {
        _graphs = new MultiDictionary<IRefNode, IGraph>(u => u?.GetHashCode() ?? DefaultGraphId, true, new FastNodeComparer(), MultiDictionaryMode.Avl);
    }

    /// <summary>
    /// Checks whether the Graph with the given Uri exists in this Graph Collection.
    /// </summary>
    /// <param name="graphUri">Graph Uri to test.</param>
    /// <returns></returns>
    [Obsolete("Replaced by Contains(IRefNode)")]
    public override bool Contains(Uri graphUri)
    {
        if (graphUri == null) return _graphs.Keys.Any(k => k == null);
        return _graphs.Keys.Any(k => k is IUriNode uriNode && uriNode.Uri.Equals(graphUri));
    }

    /// <summary>
    /// Checks whether the graph with the given name exists in this graph collection.
    /// </summary>
    /// <param name="graphName">Graph name to test for.</param>
    /// <returns>True if a graph with the specified name is in the collection, false otherwise.</returns>
    /// <remarks>The null value is used to reference the default (unnamed) graph.</remarks>
    public override bool Contains(IRefNode graphName)
    {
        return _graphs.ContainsKey(graphName);
    }

    /// <summary>
    /// Adds a Graph to the Collection.
    /// </summary>
    /// <param name="g">Graph to add.</param>
    /// <param name="mergeIfExists">Sets whether the Graph should be merged with an existing Graph of the same Uri if present.</param>
    /// <exception cref="RdfException">Throws an RDF Exception if the Graph has no Base Uri or if the Graph already exists in the Collection and the <paramref name="mergeIfExists"/> parameter was not set to true.</exception>
    public override bool Add(IGraph g, bool mergeIfExists)
    {
        if (_graphs.ContainsKey(g.Name))
        {
            // Already exists in the Graph Collection
            if (mergeIfExists)
            {
                // Merge into the existing Graph
                _graphs[g.Name].Merge(g);
                return true;
            }

            // Not allowed
            throw new RdfException("The Graph you tried to add already exists in the Graph Collection and the mergeIfExists parameter was set to false");
        }
        else
        {
            // Safe to add a new Graph
            _graphs.Add(g.Name, g);
            RaiseGraphAdded(g);
            return true;
        }
    }

    /// <summary>
    /// Removes a Graph from the Collection.
    /// </summary>
    /// <param name="graphUri">Uri of the Graph to remove.</param>
    [Obsolete("Replaced by Remove(IRefNode)")]
    public override bool Remove(Uri graphUri)
    {
        IRefNode graphName = _graphs.Keys.FirstOrDefault(k => k is UriNode uriNode && uriNode.Uri.Equals(graphUri));
        if (graphName == null && !_graphs.ContainsKey(null!)) return false;
        return Remove(graphName);
    }

    /// <summary>
    /// Removes a graph from the collection.
    /// </summary>
    /// <param name="graphName">Name of the Graph to remove.</param>
    /// <remarks>
    /// The null value is used to reference the Default Graph.
    /// </remarks>
    public override bool Remove(IRefNode graphName) 
    {
        if (_graphs.TryGetValue(graphName, out IGraph g) &&
            _graphs.Remove(graphName))
        {
            RaiseGraphRemoved(g);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the number of Graphs in the Collection.
    /// </summary>
    public override int Count
    {
        get
        {
            return _graphs.Count;
        }
    }

    /// <summary>
    /// Provides access to the Graph URIs of Graphs in the Collection.
    /// </summary>
    [Obsolete("Replaced by GraphNames")]
    public override IEnumerable<Uri> GraphUris
    {
        get
        {
            return _graphs.Keys.Where(k=>k == null || k is IUriNode).Select(k=>(k as UriNode)?.Uri);
        }
    }

    /// <summary>
    /// Provides an enumeration of the names of all of teh graphs in the collection.
    /// </summary>
    public override IEnumerable<IRefNode> GraphNames => _graphs.Keys;

    /// <summary>
    /// Gets a Graph from the Collection.
    /// </summary>
    /// <param name="graphUri">Graph Uri.</param>
    /// <returns></returns>
    [Obsolete("Replaced by this[IRefNode]")]
    public override IGraph this[Uri graphUri]
    {
        get 
        {
            if (graphUri == null && _graphs.TryGetValue(null!, out IGraph defaultGraph))
            {
                return defaultGraph;
            }

            IGraph g = _graphs.Values.FirstOrDefault(
                graph => graph.Name is IUriNode uriNode && uriNode.Uri.Equals(graphUri));
            if (g == null)
            {
                throw new RdfException("The Graph with the given URI does not exist in this Graph Collection");
            }

            return g;
        }
    }

    /// <summary>
    /// Gets a graph from the collection.
    /// </summary>
    /// <param name="graphName">The name of the graph to retrieve.</param>
    /// <returns></returns>
    /// <remarks>The null value is used to reference the default graph.</remarks>
    public override IGraph this[IRefNode graphName]
    {
        get
        {
            if (_graphs.TryGetValue(graphName, out IGraph g)) return g;
            throw new RdfException("The graph with the given name does not exist in this graph collection.");
        }
    }

    /// <summary>
    /// Gets the Enumerator for the Collection.
    /// </summary>
    /// <returns></returns>
    public override IEnumerator<IGraph> GetEnumerator()
    {
        return _graphs.Values.GetEnumerator();
    }

    /// <summary>
    /// Gets the Enumerator for this Collection.
    /// </summary>
    /// <returns></returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Disposes of the Graph Collection.
    /// </summary>
    /// <remarks>Invokes the <strong>Dispose()</strong> method of all Graphs contained in the Collection.</remarks>
    public override void Dispose()
    {
        _graphs.Clear();
    }
}
