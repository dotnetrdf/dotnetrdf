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

namespace VDS.RDF.Query.Datasets;

/// <summary>
/// A Graph Collection which wraps an <see cref="ISparqlDataset">ISparqlDataset</see> implementation so it can be used as if it was a Graph Collection.
/// </summary>
public class DatasetGraphCollection
    : BaseGraphCollection
{
    private ISparqlDataset _dataset;

    /// <summary>
    /// Creates a new Dataset Graph collection.
    /// </summary>
    /// <param name="dataset">SPARQL Dataset.</param>
    public DatasetGraphCollection(ISparqlDataset dataset)
    {
        _dataset = dataset;
    }

    /// <summary>
    /// Gets whether the Collection contains a Graph with the given URI.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    [Obsolete("Replaced by Contains(IRefNode)")]
    public override bool Contains(Uri graphUri)
    {
        return _dataset.HasGraph(graphUri);
    }

    /// <summary>
    /// Checks whether the graph with the given name exists in this graph collection.
    /// </summary>
    /// <param name="graphName">Graph name to test for.</param>
    /// <returns>True if a graph with the specified name is in the collection, false otherwise.</returns>
    /// <remarks>The null value is used to reference the default (unnamed) graph.</remarks>
    public override bool Contains(IRefNode graphName)
    {
        return _dataset.HasGraph(graphName);
    }

    /// <summary>
    /// Adds a Graph to the Collection.
    /// </summary>
    /// <param name="g">Graph to add.</param>
    /// <param name="mergeIfExists">Whether to merge the given Graph with any existing Graph with the same URI.</param>
    /// <exception cref="RdfException">Thrown if a Graph with the given URI already exists and the <paramref name="mergeIfExists">mergeIfExists</paramref> is set to false.</exception>
    public override bool Add(IGraph g, bool mergeIfExists)
    {
        if (Contains(g.Name))
        {
            if (mergeIfExists)
            {
                IGraph temp = _dataset.GetModifiableGraph(g.Name);
                temp.Merge(g);
                temp.Dispose();
                _dataset.Flush();
                return true;
            }
            else
            {
                throw new RdfException("Cannot add this Graph as a Graph with the URI '" + g.BaseUri.ToSafeString() + "' already exists in the Collection and mergeIfExists was set to false");
            }
        }
        else
        {
            // Safe to add a new Graph
            if (_dataset.AddGraph(g))
            {
                _dataset.Flush();
                RaiseGraphAdded(g);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Removes a Graph from the Collection.
    /// </summary>
    /// <param name="graphUri">URI of the Graph to removed.</param>
    [Obsolete("Replaced by Remove(IRefNode)")]
    public override bool Remove(Uri graphUri)
    {
        if (Contains(graphUri))
        {
            IGraph temp = _dataset[graphUri];
            var removed = _dataset.RemoveGraph(graphUri);
            _dataset.Flush();
            RaiseGraphRemoved(temp);
            temp.Dispose();
            return removed;
        }
        return false;
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
        if (!_dataset.HasGraph(graphName))
        {
            return false;
        }

        IGraph temp = _dataset[graphName];
        var removed = _dataset.RemoveGraph(graphName);
        _dataset.Flush();
        RaiseGraphRemoved(temp);
        temp.Dispose();
        return removed;

    }

    /// <summary>
    /// Gets the number of Graphs in the Collection.
    /// </summary>
    public override int Count
    {
        get 
        {
            return _dataset.GraphNames.Count(); 
        }
    }

    /// <summary>
    /// Gets the URIs of Graphs in the Collection.
    /// </summary>
    [Obsolete("Replaced by GraphNames")]
    public override IEnumerable<Uri> GraphUris => _dataset.GraphUris;

    /// <summary>
    /// Provides an enumeration of the names of all of teh graphs in the collection.
    /// </summary>
    public override IEnumerable<IRefNode> GraphNames => _dataset.GraphNames;

    /// <summary>
    /// Gets the Graph with the given URI.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    [Obsolete("Replaced by this[IRefNode]")]
    public override IGraph this[Uri graphUri]
    {
        get 
        {
            if (_dataset.HasGraph(graphUri))
            {
                return _dataset[graphUri];
            }
            else
            {
                throw new RdfException("The Graph with the given URI does not exist in this Graph Collection");
            }
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
            if (_dataset.HasGraph(graphName))
            {
                return _dataset[graphName];
            }
            throw new RdfException("The graph with the given name does not exist in this graph collection");
        }
    }
    /// <summary>
    /// Disposes of the Graph Collection.
    /// </summary>
    public override void Dispose()
    {
        _dataset.Flush();
    }

    /// <summary>
    /// Gets the enumeration of Graphs in this Collection.
    /// </summary>
    /// <returns></returns>
    public override IEnumerator<IGraph> GetEnumerator()
    {
        return _dataset.Graphs.GetEnumerator();
    }
}
