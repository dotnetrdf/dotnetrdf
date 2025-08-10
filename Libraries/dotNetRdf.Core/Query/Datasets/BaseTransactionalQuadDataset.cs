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
/// Abstract Base class for quad datasets that support transactions.
/// </summary>
/// <remarks>
/// <para>
/// The Transaction implementation of dotNetRDF is based upon a MRSW concurrency model, since only one writer may be active changes are immediately pushed to the dataset and visible within the transaction and they are committed or rolled back when <see cref="BaseTransactionalDataset.Flush()">Flush()</see> or <see cref="BaseTransactionalDataset.Discard()">Discard()</see> are called.
/// </para>
/// <para>
/// So in practical terms it is perfectly OK for the storage to be updated during a transaction because if the transaction fails the changes will be rolled back because all changes are stored in-memory until the end of the transaction.  This may not be an ideal transaction model for all scenarios so you may wish to implement your own version of transactions or code your implementations of the abstract methods accordingly to limit actual persistence to the end of a transaction.
/// </para>
/// </remarks>
public abstract class BaseTransactionalQuadDataset
    : BaseQuadDataset
{
    private List<GraphPersistenceAction> _actions = new List<GraphPersistenceAction>();
    private TripleStore _modifiableGraphs = new TripleStore();

    /// <summary>
    /// Creates a Transactional Quad Dataset.
    /// </summary>
    public BaseTransactionalQuadDataset() { }

    /// <summary>
    /// Creates a Transactional Quad Dataset.
    /// </summary>
    /// <param name="unionDefaultGraph">Sets whether the default graph should be the union of all graphs.</param>
    public BaseTransactionalQuadDataset(bool unionDefaultGraph)
        : base(unionDefaultGraph) { }

    /// <summary>
    /// Creates a Transactional Quad Dataset.
    /// </summary>
    /// <param name="defaultGraphUri">Default Graph URI.</param>
    public BaseTransactionalQuadDataset(Uri defaultGraphUri)
        : base(defaultGraphUri) { }

    /// <summary>
    /// Creates a transactional quad dataset.
    /// </summary>
    /// <param name="defaultGraphName">Default graph name.</param>
    public BaseTransactionalQuadDataset(IRefNode defaultGraphName)
        : base(defaultGraphName){ }

    /// <summary>
    /// Adds a Graph to the Dataset.
    /// </summary>
    /// <param name="g">Graph to add.</param>
    public sealed override bool AddGraph(IGraph g)
    {
        if (HasGraph(g.Name))
        {
            var existing = (ITransactionalGraph)GetModifiableGraph(g.Name);
            _actions.Add(new GraphPersistenceAction(existing, GraphPersistenceActionType.Modified));
        }
        else
        {
            _actions.Add(new GraphPersistenceAction(g, GraphPersistenceActionType.Added));
        }
        return AddGraphInternal(g);
    }

    /// <summary>
    /// Adds a Graph to the Dataset.
    /// </summary>
    /// <param name="g">Graph to add.</param>
    protected abstract bool AddGraphInternal(IGraph g);

    /// <summary>
    /// Removes a Graph from the Dataset.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    [Obsolete("Replaced by RemoveGraph(IRefNode)")]
    public sealed override bool RemoveGraph(Uri graphUri)
    {
        return RemoveGraph(graphUri == null ? null : new UriNode(graphUri));
    }

    /// <summary>
    /// Removes a Graph from the Dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <exception cref="NotSupportedException">May be thrown if the Dataset is immutable i.e. Updates not supported.</exception>
    public override bool RemoveGraph(IRefNode graphName)
    {
        if (graphName == null)
        {
            if (DefaultGraphNames.Any())
            {
                foreach (IRefNode u in DefaultGraphNames)
                {
                    if (IsDefaultGraph(u))
                    {
                        // Default Graph gets cleared
                        var wrapper = new GraphPersistenceWrapper(this[u]);
                        wrapper.Clear();
                        _actions.Add(new GraphPersistenceAction(wrapper, GraphPersistenceActionType.Modified));
                        return true;
                    }
                    else
                    {
                        // Other Graphs get actually deleted
                        _actions.Add(new GraphPersistenceAction(this[u], GraphPersistenceActionType.Deleted));
                        return true;
                    }
                }
            }
            else if (HasGraph((IRefNode) null))
            {
                _actions.Add(new GraphPersistenceAction(this[(IRefNode) null], GraphPersistenceActionType.Deleted));
                return RemoveGraphInternal(null);
            }
        }
        else if (HasGraph(graphName))
        {
            _actions.Add(new GraphPersistenceAction(this[graphName], GraphPersistenceActionType.Deleted));
            return RemoveGraphInternal(graphName);
        }
        return false;
    }
    
    /// <summary>
    /// Removes a graph from the dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <returns></returns>
    protected abstract bool RemoveGraphInternal(IRefNode graphName);

    /// <summary>
    /// Gets a Graph from the dataset.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    [Obsolete("Replaced by this[IRefNode]")]
    public override IGraph this[Uri graphUri]
    {
        get
        {
            return this[new UriNode(graphUri)];
        }
    }


    /// <summary>
    /// Gets the graph with the given name from the dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>
    /// This property need only return a read-only view of the Graph, code which wishes to modify Graphs should use the <see cref="ISparqlDataset.GetModifiableGraph(IRefNode)">GetModifiableGraph()</see> method to guarantee a Graph they can modify and will be persisted to the underlying storage.
    /// </para>
    /// </remarks>
    public override IGraph this[IRefNode graphName]
    {
        get
        {
            if (graphName == null)
            {
                if (DefaultGraphNames.Any())
                {
                    if (DefaultGraphNames.Count() == 1)
                    {
                        return new Graph(new QuadDatasetTripleCollection(this, DefaultGraphNames.First()));
                    }
                    else
                    {
                        IEnumerable<IGraph> gs = (from u in DefaultGraphNames
                            select new Graph(new QuadDatasetTripleCollection(this, u))).OfType<IGraph>().ToList();
                        return new UnionGraph(gs.First(), gs.Skip(1));
                    }
                }
                else if (_modifiableGraphs.HasGraph((IRefNode) null))
                {
                    return _modifiableGraphs[(IRefNode) null];
                }
                else
                {
                    return GetGraphInternal(null);
                }
            }
            else if (_modifiableGraphs.HasGraph(graphName))
            {
                return _modifiableGraphs[graphName];
            }
            else
            {
                return GetGraphInternal(graphName);
            }
        }
    }

    /// <summary>
    /// Gets a Graph from the Dataset that can be modified.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    [Obsolete("Replaced by GetModifiableGraph(IRefNode)")]
    public sealed override IGraph GetModifiableGraph(Uri graphUri)
    {
        return GetModifiableGraph(new UriNode(graphUri));
    }

    /// <summary>
    /// Gets the Graph with the given name from the Dataset.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">May be thrown if the Dataset is immutable i.e. Updates not supported.</exception>
    /// <remarks>
    /// <para>
    /// Graphs returned from this method must be modifiable and the Dataset must guarantee that when it is Flushed or Disposed of that any changes to the Graph are persisted.
    /// </para>
    /// </remarks>
    public override IGraph GetModifiableGraph(IRefNode graphName)
    {
        if (!_modifiableGraphs.HasGraph(graphName))
        {
            IGraph current = GetModifiableGraphInternal(graphName);
            if (!_modifiableGraphs.HasGraph(current.Name))
            {
                _modifiableGraphs.Add(current);
            }
            graphName = current.Name;
        }
        var existing = (ITransactionalGraph)_modifiableGraphs[graphName];
        _actions.Add(new GraphPersistenceAction(existing, GraphPersistenceActionType.Modified));
        return existing;
    }

    /// <summary>
    /// Gets a Graph from the Dataset that can be modified transactionally.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <returns></returns>
    protected abstract ITransactionalGraph GetModifiableGraphInternal(IRefNode graphUri);

    /// <summary>
    /// Ensures that any changes to the Dataset (if any) are flushed to the underlying Storage.
    /// </summary>
    /// <remarks>
    /// Commits the Active Transaction.
    /// </remarks>
    public sealed override void Flush()
    {
        var i = 0;
        while (i < _actions.Count)
        {
            GraphPersistenceAction action = _actions[i];
            switch (action.Action)
            {
                case GraphPersistenceActionType.Added:
                    // If Graph was added ensure any changes were flushed
                    action.Graph.Flush();
                    break;
                case GraphPersistenceActionType.Deleted:
                    // If Graph was deleted can discard any changes
                    action.Graph.Discard();
                    break;
                case GraphPersistenceActionType.Modified:
                    // If Graph was modified ensure any changes were flushed
                    action.Graph.Flush();
                    break;
            }
            i++;
        }
        _actions.Clear();
        // Ensure any Modifiable Graphs we've looked at have been Flushed()
        foreach (ITransactionalGraph g in _modifiableGraphs.Graphs.OfType<ITransactionalGraph>())
        {
            g.Flush();
        }
        _modifiableGraphs = new TripleStore();

        FlushInternal();
    }

    /// <summary>
    /// Ensures that any changes to the Dataset (if any) are discarded.
    /// </summary>
    /// <remarks>
    /// Rollsback the Active Transaction.
    /// </remarks>
    public sealed override void Discard()
    {
        var i = _actions.Count - 1;
        var total = _actions.Count;
        while (i >= 0)
        {
            GraphPersistenceAction action = _actions[i];
            switch (action.Action)
            {
                case GraphPersistenceActionType.Added:
                    // If a Graph was added we must now remove it
                    if (HasGraphInternal(action.Graph.Name))
                    {
                        RemoveGraphInternal(action.Graph.Name);
                    }
                    break;
                case GraphPersistenceActionType.Deleted:
                    // If a Graph was deleted we must now add it back again
                    // Don't add the full Graph only an empty Graph with the given URI
                    var g = new Graph(action.Graph.Name);
                    AddGraphInternal(g);
                    break;
                case GraphPersistenceActionType.Modified:
                    // If a Graph was modified we must discard the changes
                    action.Graph.Discard();
                    break;
            }
            i--;
        }
        if (total == _actions.Count)
        {
            _actions.Clear();
        }
        else
        {
            _actions.RemoveRange(0, total);
        }
        // Ensure any modifiable Graphs we've looked at have been Discarded
        foreach (ITransactionalGraph g in _modifiableGraphs.Graphs.OfType<ITransactionalGraph>())
        {
            g.Discard();
        }
        _modifiableGraphs = new TripleStore();

        DiscardInternal();
    }

    /// <summary>
    /// Allows the derived dataset to take any post-Flush() actions required.
    /// </summary>
    protected virtual void FlushInternal()
    {
        // No actions by default
    }

    /// <summary>
    /// Allows the derived dataset to take any post-Discard() actions required.
    /// </summary>
    protected virtual void DiscardInternal()
    {
        // No actions by default
    }
}