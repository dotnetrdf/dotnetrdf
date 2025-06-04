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
using VDS.RDF.Storage;

namespace VDS.RDF;

/// <summary>
/// The Store Graph Persistence Wrapper is a wrapper around another Graph that will be persisted to an underlying store via a provided <see cref="IStorageProvider">IStorageProvider</see> implementation.
/// </summary>
public class StoreGraphPersistenceWrapper
    : GraphPersistenceWrapper
{
    private readonly IStorageProvider _manager;

    /// <summary>
    /// Creates a new Store Graph Persistence Wrapper.
    /// </summary>
    /// <param name="manager">Generic IO Manager.</param>
    /// <param name="g">Graph to wrap.</param>
    /// <param name="graphUri">Graph URI (the URI the Graph will be persisted as).</param>
    /// <param name="writeOnly">Whether to operate in write-only mode.</param>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> In order to operate in write-only mode the <see cref="IStorageProvider">IStorageProvider</see> must support triple level updates indicated by it returning true to its <see cref="IStorageCapabilities.UpdateSupported">UpdateSupported</see> property and the Graph to be wrapped must be an empty Graph.
    /// </para>
    /// </remarks>
    [Obsolete("Replaced by StoreGraphPersistenceWrapper(IStorageProvider, IGraph, bool)")]
    public StoreGraphPersistenceWrapper(IStorageProvider manager, IGraph g, Uri graphUri, bool writeOnly)
        : base(g, writeOnly)
    {
        if (manager == null) throw new ArgumentNullException(nameof(manager),"Cannot persist to a null Generic IO Manager");
        if (manager.IsReadOnly) throw new ArgumentException("Cannot persist to a read-only Generic IO Manager", nameof(manager));
        if (writeOnly && !manager.UpdateSupported) throw new ArgumentException("If writeOnly is set to true then the Generic IO Manager must support triple level updates", nameof(writeOnly));
        if (writeOnly && !g.IsEmpty) throw new ArgumentException("If writeOnly is set to true then the input graph must be empty", nameof(writeOnly));

        _manager = manager;
        BaseUri = graphUri;
    }

    /// <summary>
    /// Creates a new Store Graph Persistence Wrapper.
    /// </summary>
    /// <param name="manager">Generic IO Manager.</param>
    /// <param name="g">Graph to wrap.</param>
    /// <param name="writeOnly">Whether to operate in write-only mode.</param>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> In order to operate in write-only mode the <see cref="IStorageProvider">IStorageProvider</see> must support triple level updates indicated by it returning true to its <see cref="IStorageCapabilities.UpdateSupported">UpdateSupported</see> property and the Graph to be wrapped must be an empty Graph.
    /// </para>
    /// </remarks>
    public StoreGraphPersistenceWrapper(IStorageProvider manager, IGraph g, bool writeOnly)
        : base(g, writeOnly)
    {
        _manager = manager ??
                   throw new ArgumentNullException(nameof(manager), "Cannot persist to a null generic IO manager");
        if (manager.IsReadOnly) throw new ArgumentException("Cannot persist to a read-only Generic IO Manager", nameof(manager));
        if (writeOnly && !manager.UpdateSupported) throw new ArgumentException("If writeOnly is set to true then the Generic IO Manager must support triple level updates", nameof(writeOnly));
        if (writeOnly && !g.IsEmpty) throw new ArgumentException("If writeOnly is set to true then the input graph must be empty", nameof(writeOnly));
    }

    /// <summary>
    /// Creates a new Store Graph Persistence Wrapper.
    /// </summary>
    /// <param name="manager">Generic IO Manager.</param>
    /// <param name="g">Graph to wrap.</param>
    public StoreGraphPersistenceWrapper(IStorageProvider manager, IGraph g)
        : this(manager, g, false) { }

    /// <summary>
    /// Creates a new Store Graph Persistence Wrapper around a new empty Graph.
    /// </summary>
    /// <param name="manager">Generic IO Manager.</param>
    /// <param name="graphUri">Graph URI (the URI the Graph will be persisted as).</param>
    /// <param name="writeOnly">Whether to operate in write-only mode.</param>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> In order to operate in write-only mode the <see cref="IStorageProvider">IStorageProvider</see> must support triple level updates indicated by it returning true to its <see cref="IStorageCapabilities.UpdateSupported">UpdateSupported</see> property.
    /// </para>
    /// <para>
    /// When not operating in write-only mode the existing Graph will be loaded from the underlying store.
    /// </para>
    /// </remarks>
    [Obsolete("Replaced by StoreGraphPersistenceWrapper(IStorageProvider, IRefNode, bool)")]
    public StoreGraphPersistenceWrapper(IStorageProvider manager, Uri graphUri, bool writeOnly)
        : base(new Graph(graphUri == null ? null : new UriNode(graphUri)), writeOnly)
    {
        if (manager == null) throw new ArgumentNullException(nameof(manager), "Cannot persist to a null Generic IO Manager");
        if (manager.IsReadOnly) throw new ArgumentException("Cannot persist to a read-only Generic IO Manager", nameof(manager));
        if (writeOnly && !manager.UpdateSupported) throw new ArgumentException("If writeOnly is set to true then the Generic IO Manager must support triple level updates", nameof(writeOnly));
        _manager = manager;
        if (!writeOnly)
        {
            // Load in the existing data
            _manager.LoadGraph(_g, graphUri);
        }
    }


    /// <summary>
    /// Creates a new Store Graph Persistence Wrapper around a new empty Graph.
    /// </summary>
    /// <param name="manager">Generic IO Manager.</param>
    /// <param name="graphName">Graph name (the name the Graph will be persisted as).</param>
    /// <param name="writeOnly">Whether to operate in write-only mode.</param>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> In order to operate in write-only mode the <see cref="IStorageProvider">IStorageProvider</see> must support triple level updates indicated by it returning true to its <see cref="IStorageCapabilities.UpdateSupported">UpdateSupported</see> property.
    /// </para>
    /// <para>
    /// When not operating in write-only mode the existing Graph will be loaded from the underlying store.
    /// </para>
    /// </remarks>
    public StoreGraphPersistenceWrapper(IStorageProvider manager, IRefNode graphName, bool writeOnly)
        : base(new Graph(graphName), writeOnly)
    {
        if (manager == null) throw new ArgumentNullException(nameof(manager), "Cannot persist to a null Generic IO Manager");
        if (manager.IsReadOnly) throw new ArgumentException("Cannot persist to a read-only Generic IO Manager", nameof(manager));
        if (writeOnly && !manager.UpdateSupported) throw new ArgumentException("If writeOnly is set to true then the Generic IO Manager must support triple level updates", nameof(writeOnly));

        _manager = manager;

        if (!writeOnly)
        {
            // Load in the existing data
            _manager.LoadGraph(_g, graphName?.ToString());
        }
    }

    /// <summary>
    /// Creates a new Store Graph Persistence Wrapper around a new empty Graph.
    /// </summary>
    /// <param name="manager">Generic IO Manager.</param>
    /// <param name="graphUri">Graph URI (the URI the Graph will be persisted as).</param>
    [Obsolete("Replaced by StoreGraphPersistenceWrapper(IStorageProvider, IRefNode)")]
    public StoreGraphPersistenceWrapper(IStorageProvider manager, Uri graphUri)
        : this(manager, graphUri, false) { }

    /// <summary>
    /// Creates a new Store Graph Persistence Wrapper around a new empty Graph.
    /// </summary>
    /// <param name="manager">Generic IO Manager.</param>
    /// <param name="graphName">The name the graph will be persisted as.</param>
    public StoreGraphPersistenceWrapper(IStorageProvider manager, IRefNode graphName)
        : this(manager, graphName, false)
    {
    }

    /// <summary>
    /// Gets whether the in-use <see cref="IStorageProvider">IStorageProvider</see> supports triple level updates.
    /// </summary>
    protected override bool SupportsTriplePersistence => _manager.UpdateSupported;

    /// <summary>
    /// Persists the deleted Triples to the in-use <see cref="IStorageProvider">IStorageProvider</see>.
    /// </summary>
    /// <param name="ts">Triples.</param>
    protected override void PersistDeletedTriples(IEnumerable<Triple> ts)
    {
        if (_manager.UpdateSupported)
        {
            _manager.UpdateGraph(Name, null, ts);
        }
        else
        {
            throw new NotSupportedException("The underlying Generic IO Manager does not support Triple Level persistence");
        }
    }

    /// <summary>
    /// Persists the inserted Triples to the in-use <see cref="IStorageProvider">IStorageProvider</see>.
    /// </summary>
    /// <param name="ts">Triples.</param>
    protected override void PersistInsertedTriples(IEnumerable<Triple> ts)
    {
        if (_manager.UpdateSupported)
        {
            _manager.UpdateGraph(Name, ts, null);
        }
        else
        {
            throw new NotSupportedException("The underlying Generic IO Manager does not support Triple Level persistence");
        }
    }

    /// <summary>
    /// Persists the entire Graph to the in-use <see cref="IStorageProvider">IStorageProvider</see>.
    /// </summary>
    protected override void PersistGraph()
    {
        _manager.SaveGraph(this);
    }
}