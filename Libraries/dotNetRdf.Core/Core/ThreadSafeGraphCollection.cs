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
using System.Threading;

namespace VDS.RDF;

/// <summary>
/// Thread Safe decorator around a Graph collection.
/// </summary>
/// <remarks>
/// Provides concurrency via a <see cref="ReaderWriterLockSlim" />.
/// </remarks>
public class ThreadSafeGraphCollection 
    : WrapperGraphCollection
{
    private readonly ReaderWriterLockSlim _lockManager = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    /// <summary>
    /// Creates a new Thread Safe decorator around the default <see cref="GraphCollection"/>.
    /// </summary>
    public ThreadSafeGraphCollection()
        : base(new GraphCollection()) { }

    /// <summary>
    /// Creates a new Thread Safe decorator around the supplied graph collection.
    /// </summary>
    /// <param name="graphCollection">Graph Collection.</param>
    public ThreadSafeGraphCollection(BaseGraphCollection graphCollection)
        : base(graphCollection) { }

    /// <summary>
    /// Enters the write lock.
    /// </summary>
    protected void EnterWriteLock()
    {
        _lockManager.EnterWriteLock();
    }

    /// <summary>
    /// Exits the write lock.
    /// </summary>
    protected void ExitWriteLock()
    {
        _lockManager.ExitWriteLock();
    }

    /// <summary>
    /// Enters the read lock.
    /// </summary>
    protected void EnterReadLock()
    {
        _lockManager.EnterReadLock();
    }

    /// <summary>
    /// Exits the read lock.
    /// </summary>
    protected void ExitReadLock()
    {
        _lockManager.ExitReadLock();
    }

    /// <summary>
    /// Checks whether the Graph with the given Uri exists in this Graph Collection.
    /// </summary>
    /// <param name="graphUri">Graph Uri to test.</param>
    /// <returns></returns>
    [Obsolete]
    public override bool Contains(Uri graphUri)
    {
        try
        {
            EnterReadLock();
            return _graphs.Contains(graphUri);
        }
        finally
        {
            ExitReadLock();
        }
    }

    /// <summary>
    /// Checks whether the graph with the given name exists in this graph collection.
    /// </summary>
    /// <param name="graphName">Graph name to test for.</param>
    /// <returns>True if a graph with the specified name is in the collection, false otherwise.</returns>
    /// <remarks>The null value is used to reference the default (unnamed) graph.</remarks>
    public override bool Contains(IRefNode graphName)
    {
        try
        {
            EnterReadLock();
            return _graphs.Contains(graphName);
        }
        finally
        {
            ExitReadLock();
        }
    }

    /// <summary>
    /// Adds a Graph to the Collection.
    /// </summary>
    /// <param name="g">Graph to add.</param>
    /// <param name="mergeIfExists">Sets whether the Graph should be merged with an existing Graph of the same Uri if present.</param>
    /// <exception cref="RdfException">Throws an RDF Exception if the Graph has no Base Uri or if the Graph already exists in the Collection and the <paramref name="mergeIfExists"/> parameter was not set to true.</exception>
    public override bool Add(IGraph g, bool mergeIfExists)
    {
        try
        {
            EnterWriteLock();
            return _graphs.Add(g, mergeIfExists);
        }
        finally
        {
            ExitWriteLock();
        }
    }

    /// <summary>
    /// Removes a Graph from the Collection.
    /// </summary>
    /// <param name="graphUri">Uri of the Graph to remove.</param>
    [Obsolete]
    public override bool Remove(Uri graphUri)
    {
        try
        {
            EnterWriteLock();
            return _graphs.Remove(graphUri);
        }
        finally
        {
            ExitWriteLock();
        }
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
        try
        {
            EnterWriteLock();
            return _graphs.Remove(graphName);
        }
        finally
        {
            ExitWriteLock();
        }
    }


    /// <summary>
    /// Gets the number of Graphs in the Collection.
    /// </summary>
    public override int Count
    {
        get
        {
            try
            {
                EnterReadLock();
                return _graphs.Count;
            }
            finally
            {
                ExitReadLock();
            }
        }
    }

    /// <summary>
    /// Gets the Enumerator for the Collection.
    /// </summary>
    /// <returns></returns>
    public override IEnumerator<IGraph> GetEnumerator()
    {
        IEnumerable<IGraph> graphs;
        try
        {
            EnterReadLock();
            graphs = _graphs.ToList();
        }
        finally
        {
            ExitReadLock();
        }
        return graphs.GetEnumerator();
    }

    /// <summary>
    /// Provides access to the Graph URIs of Graphs in the Collection.
    /// </summary>
    [Obsolete]
    public override IEnumerable<Uri> GraphUris
    {
        get
        {
            try
            {
                EnterReadLock();
                return _graphs.GraphUris.ToList();
            }
            finally
            {
                ExitReadLock();
            }
        }
    }

    /// <summary>
    /// Provides an enumeration of the names of all of teh graphs in the collection.
    /// </summary>
    public override IEnumerable<IRefNode> GraphNames
    {
        get
        {
            try
            {
                EnterReadLock();
                return _graphs.GraphNames.ToList();
            }
            finally
            {
                ExitReadLock();
            }
        }
    }

    /// <summary>
    /// Gets a Graph from the Collection.
    /// </summary>
    /// <param name="graphUri">Graph Uri.</param>
    /// <returns></returns>
    [Obsolete]
    public override IGraph this[Uri graphUri]
    {
        get
        {
            try
            {
                EnterReadLock();
                return _graphs[graphUri];
            }
            finally
            {
                ExitReadLock();
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
            try
            {
                EnterReadLock();
                return _graphs[graphName];
            }
            finally
            {
                ExitReadLock();
            }
        }
    }

    /// <summary>
    /// Disposes of the Graph Collection.
    /// </summary>
    /// <remarks>Invokes the <strong>Dispose()</strong> method of all Graphs contained in the Collection.</remarks>
    public override void Dispose()
    {
        try
        {
            EnterWriteLock();
            _graphs.Dispose();
        }
        finally
        {
            ExitWriteLock();
        }
    }
}