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
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;

namespace VDS.RDF;

/// <summary>
/// Internal implementation of a Graph Collection for use by the <see cref="PersistentTripleStore">PersistentTripleStore</see>.
/// </summary>
class PersistentGraphCollection
    : GraphCollection
{
    private readonly IStorageProvider _manager;
    private readonly TripleEventHandler _tripleAddedHandler;
    private readonly TripleEventHandler _tripleRemovedHandler;
    private readonly List<TripleStorePersistenceAction> _actions = new List<TripleStorePersistenceAction>();
    private readonly HashSet<string> _removedGraphs = new HashSet<string>();
    private bool _persisting;

    public PersistentGraphCollection(IStorageProvider manager)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager), "Must use a non-null IStorageProvider instance with a PersistentGraphCollection");

        _tripleAddedHandler = OnTripleAsserted;
        _tripleRemovedHandler = OnTripleRetracted;
    }

    protected override void RaiseGraphAdded(IGraph g)
    {
        if (!_persisting)
        {
            if (_manager.UpdateSupported)
            {
                AttachHandlers(g);
                if (_removedGraphs.Contains(g.Name.ToSafeString()) || !ContainsInternal(g.Name))
                {
                    // When a new graph is introduced that does not exist in the underlying store
                    // be sure to persist the initial triples
                    _actions.Add(new TripleStorePersistenceAction(new GraphPersistenceAction(g, GraphPersistenceActionType.Added)));
                    foreach (Triple t in g.Triples)
                    {
                        _actions.Add(new TripleStorePersistenceAction(new TriplePersistenceAction(t, g.Name)));
                    }
                }
            }
            else
            {
                _actions.Add(new TripleStorePersistenceAction(new GraphPersistenceAction(g, GraphPersistenceActionType.Added)));
            }
        }
        base.RaiseGraphAdded(g);
    }

    protected override void RaiseGraphRemoved(IGraph g)
    {
        if (!_persisting)
        {
            var uri = g.Name.ToSafeString();
            _removedGraphs.Add(uri);
            if (_manager.UpdateSupported)
            {
                DetachHandlers(g);
                _actions.Add(new TripleStorePersistenceAction(new GraphPersistenceAction(g, GraphPersistenceActionType.Deleted)));
            }
            else
            {
                _actions.Add(new TripleStorePersistenceAction(new GraphPersistenceAction(g, GraphPersistenceActionType.Deleted)));
            }
        }
        base.RaiseGraphRemoved(g);
    }


    /// <summary>
    /// Checks whether the Graph with the given Uri exists in this Graph Collection.
    /// </summary>
    /// <param name="graphUri">Graph Uri to test.</param>
    /// <returns></returns>
    [Obsolete("Replaced by Contains(IRefNode)")]
    public override bool Contains(Uri graphUri)
    {
        return Contains(graphUri == null ? null : new UriNode(graphUri));
    }

    /// <summary>
    /// Checks whether the Graph with the given name exists in this Graph Collection.
    /// </summary>
    /// <param name="graphName">Graph name to test.</param>
    /// <returns></returns>
    public override bool Contains(IRefNode graphName)
    {
        var uri = graphName.ToSafeString();
        if (base.Contains(graphName))
        {
            return true;
        }
        else if (!_removedGraphs.Contains(uri))
        {
            // Try and load the Graph and return true if anything is returned
            var g = new Graph(graphName);
            try
            {
                _manager.LoadGraph(g, graphName.ToSafeString());
                if (g.Triples.Count > 0)
                {
                    // If we're going to return true we must also store the Graph in the collection
                    // for later use
                    Add(g, true);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                // If trying to load the Graph errors then it doesn't exist so return false
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    [Obsolete("Replaced by Remove(IRefNode)")]
    public override bool Remove(Uri graphUri)
    {
        if (Contains(graphUri))
        {
            return base.Remove(graphUri);
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
        return Contains(graphName) && base.Remove(graphName);
    }

    /// <summary>
    /// Provides access to the Graph URIs of Graphs in the Collection.
    /// </summary>
    [Obsolete("Replaced by GraphNames")]
    public override IEnumerable<Uri> GraphUris
    {
        get
        {
            if (_manager.ListGraphsSupported)
            {
                return _manager.ListGraphs().Concat(base.GraphUris).Distinct();
            }
            else
            {
                return base.GraphUris;
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
            if (_manager.ListGraphsSupported)
            {
                foreach (var graphName in _manager.ListGraphNames())
                {
                    if (string.IsNullOrEmpty(graphName)) yield return null;
                    else if (Uri.TryCreate(graphName, UriKind.Absolute, out Uri graphUri)) yield return new UriNode(graphUri);
                    else if (graphName.StartsWith("_:")) yield return new BlankNode(graphName.Substring(2));
                    else yield return new BlankNode(graphName);
                }
            }
            foreach(IRefNode name in base.GraphNames) yield return name;
        }
    }

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
            if (Contains(graphUri))
            {
                return base[graphUri];
            }
            else
            {
                throw new RdfException("The Graph with the given URI does not exist in the Graph Collection");
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
            if (Contains(graphName))
            {
                return base[graphName];
            }
            throw new RdfException("The Graph with the given URI does not exist in the Graph Collection");
        }
    }

    private bool ContainsInternal(IRefNode graphName)
    {
        var handler = new AnyHandler();
        try
        {
            _manager.LoadGraph(handler, graphName.ToSafeString());
            return handler.Any;
        }
        catch
        {
            return false;
        }
    }


    private void AttachHandlers(IGraph g)
    {
        g.TripleAsserted += _tripleAddedHandler;
        g.TripleRetracted += _tripleRemovedHandler;
    }

    private void DetachHandlers(IGraph g)
    {
        g.TripleAsserted -= _tripleAddedHandler;
        g.TripleRetracted -= _tripleRemovedHandler;
    }

    private void OnTripleAsserted(object sender, TripleEventArgs args)
    {
        if (!_persisting)
        {
            _actions.Add(new TripleStorePersistenceAction(new TriplePersistenceAction(args.Triple, args.Graph.Name)));
        }
    }

    private void OnTripleRetracted(object sender, TripleEventArgs args)
    {
        if (!_persisting)
        {
            _actions.Add(new TripleStorePersistenceAction(new TriplePersistenceAction(args.Triple, args.Graph.Name, true)));
        }
    }

    internal bool IsSynced
    {
        get
        {
            return _actions.Count == 0;
        }
    }

    internal void Flush()
    {
        try
        {
            _persisting = true;
            _removedGraphs.Clear();

            // Read-Only managers have no persistence
            if (_manager.IsReadOnly) return;

            // No actions means no persistence necessary
            if (_actions.Count == 0) return;

            if (_manager.UpdateSupported)
            {
                // Persist based on Triple level actions
                // First group Triple together based on Graph URI
                while (_actions.Count > 0)
                {
                    TripleStorePersistenceAction action = _actions[0];

                    if (action.IsTripleAction)
                    {
                        var actions = new Queue<TriplePersistenceAction>();
                        IRefNode currentGraph = action.TripleAction.Graph;
                        actions.Enqueue(_actions[0].TripleAction);
                        _actions.RemoveAt(0);

                        // Find all the Triple actions related to this Graph up to the next non-Triple action
                        for (var i = 0; i < _actions.Count && _actions[i].IsTripleAction; i++)
                        {
                            if (EqualityHelper.AreRefNodesEqual(currentGraph, _actions[i].TripleAction.Graph))
                            {
                                actions.Enqueue(_actions[i].TripleAction);
                                _actions.RemoveAt(i);
                                i--;
                            }
                        }

                        // Split the Triple Actions for this Graph into batches of adds and deletes to ensure
                        // accurate persistence of the actions
                        var toDelete = false;
                        var batch = new List<Triple>();
                        while (actions.Count > 0)
                        {
                            TriplePersistenceAction next = actions.Dequeue();
                            if (next.IsDelete != toDelete)
                            {
                                if (batch.Count > 0)
                                {
                                    // Process a batch whenever we find a switch between additions and removals
                                    // This ensures that regardless of the logic in UpdateGraph() we force
                                    // additions and removals to happen in the order we care about
                                    if (toDelete)
                                    {
                                        _manager.UpdateGraph(currentGraph, null, batch);
                                    }
                                    else
                                    {
                                        _manager.UpdateGraph(currentGraph, batch, null);
                                    }
                                    batch.Clear();
                                }
                                toDelete = next.IsDelete;
                            }
                            batch.Add(next.Triple);
                        }
                        // Ensure the final batch (if any) gets processed
                        if (batch.Count > 0)
                        {
                            if (toDelete)
                            {
                                _manager.UpdateGraph(currentGraph, null, batch);
                            }
                            else
                            {
                                _manager.UpdateGraph(currentGraph, batch, null);
                            }
                        }
                    }
                    else
                    {
                        switch (action.GraphAction.Action)
                        {
                            case GraphPersistenceActionType.Added:
                                // No need to do anything in-memory as will be in the graph collection
                                // Call SaveGraph() with an empty graph to create the relevant graph
                                // If Triples were added these will be persisted separately with
                                // TriplePersistenceActions
                                var g = new Graph(action.GraphAction.Graph.Name);
                                g.BaseUri = action.GraphAction.Graph.BaseUri;
                                _manager.SaveGraph(g);
                                break;

                            case GraphPersistenceActionType.Deleted:
                                // No need to do anything in-memory as won't be in the graph collection
                                // If DeleteGraph() is supported call it to delete the relevant graph
                                if (_manager.DeleteSupported)
                                {
                                    _manager.DeleteGraph(action.GraphAction.Graph.Name.ToString());
                                }
                                break;
                        }
                        _actions.RemoveAt(0);
                    }
                }
            }
            else
            {
                // Persist based on Graph level actions
                foreach (TripleStorePersistenceAction action in _actions)
                {
                    if (action.IsGraphAction)
                    {
                        if (action.GraphAction.Action == GraphPersistenceActionType.Added)
                        {
                            _manager.SaveGraph(action.GraphAction.Graph);
                        }
                        else if (action.GraphAction.Action == GraphPersistenceActionType.Deleted && _manager.DeleteSupported)
                        {
                            // Can only delete graphs if deletion is supported
                            _manager.DeleteGraph(action.GraphAction.Graph.BaseUri);
                        }
                    }
                }
            }
        }
        finally
        {
            _persisting = false;
        }
    }

    internal void Discard()
    {
        try 
        {
            _persisting = true;
            _removedGraphs.Clear();

            // Read-Only managers have no persistence
            if (_manager.IsReadOnly) return;

            // No actions mean no persistence necessary
            if (_actions.Count == 0) return;

            // Important - For discard we reverse the list of actions so that we
            // rollback the actions in appropriate order
            _actions.Reverse();

            if (_manager.UpdateSupported)
            {
                // Persist based on Triple level actions
                // First group Triple together based on Graph URI
                while (_actions.Count > 0)
                {
                    TripleStorePersistenceAction action = _actions[0];

                    if (action.IsTripleAction)
                    {
                        var actions = new Queue<TriplePersistenceAction>();
                        IRefNode currentGraph = _actions[0].TripleAction.Graph;
                        actions.Enqueue(_actions[0].TripleAction);
                        _actions.RemoveAt(0);

                        // Find all the Triple actions related to this Graph up to the next non-Triple action
                        for (var i = 0; i < _actions.Count && _actions[i].IsTripleAction; i++)
                        {
                            if (EqualityHelper.AreRefNodesEqual(currentGraph, _actions[i].TripleAction.Graph))
                            {
                                actions.Enqueue(_actions[i].TripleAction);
                                _actions.RemoveAt(i);
                                i--;
                            }
                        }

                        // Split the Triples for this Graph into batches of adds and deletes to ensure
                        // accurate persistence of the actions
                        var toDelete = false;
                        var batch = new List<Triple>();
                        while (actions.Count > 0)
                        {
                            TriplePersistenceAction next = actions.Dequeue();
                            if (next.IsDelete != toDelete)
                            {
                                if (batch.Count > 0)
                                {
                                    // Process a batch whenever we find a switch between additions and removals
                                    // This ensures that regardless of the logic in UpdateGraph() we force
                                    // additions and removals to happen in the order we care about

                                    // Important - For discard we flip the actions in order to reverse them
                                    // i.e. additions become removals and vice versa
                                    // Also for discard we only need to alter the in-memory state not actually
                                    // do any persistence since the actions will never have been persisted
                                    if (toDelete)
                                    {
                                        this[currentGraph].Assert(batch);
                                    }
                                    else
                                    {
                                        this[currentGraph].Retract(batch);
                                    }
                                    batch.Clear();
                                }
                                toDelete = next.IsDelete;
                            }
                            batch.Add(next.Triple);
                        }
                        // Ensure the final batch (if any) gets processed
                        if (batch.Count > 0)
                        {
                            // Important - For discard we flip the actions in order to reverse them
                            // i.e. additions become removals and vice versa
                            // Also for discard we only need to alter the in-memory state not actually
                            // do any persistence since the actions will never have been persisted
                            if (toDelete)
                            {
                                this[currentGraph].Assert(batch);
                            }
                            else
                            {
                                this[currentGraph].Retract(batch);
                            }
                        }
                    }
                    else
                    {
                        switch (action.GraphAction.Action)
                        {
                            case GraphPersistenceActionType.Added:
                                // Need to remove from being in-memory
                                Remove(action.GraphAction.Graph.Name);
                                break;

                            case GraphPersistenceActionType.Deleted:
                                // Need to add back into memory
                                Add(action.GraphAction.Graph, false);
                                break;
                        }
                        _actions.RemoveAt(0);
                    }
                }
            }
            else
            {
                // Persist based on Graph level actions
                foreach (TripleStorePersistenceAction action in _actions)
                {
                    // Important - For discard we flip the actions in order to reverse them
                    // i.e. additions become removals and vice versa

                    if (action.IsGraphAction)
                    {
                        if (action.GraphAction.Action == GraphPersistenceActionType.Added)
                        {
                            Remove(action.GraphAction.Graph.Name);
                        }
                        else if (action.GraphAction.Action == GraphPersistenceActionType.Deleted)
                        {
                            Add(action.GraphAction.Graph, false);
                        }
                    }
                }
            }
        } 
        finally 
        {
            _persisting = false;
        }
    }
}