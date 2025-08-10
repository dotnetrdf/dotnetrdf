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

namespace VDS.RDF.Parsing.Handlers;

/// <summary>
/// A RDF Handler which writes the Triples being parsed directly to a <see cref="IStorageProvider">IStorageProvider</see> in batches provided the manager supports the <see cref="IStorageProvider.UpdateGraph(Uri,System.Collections.Generic.IEnumerable{VDS.RDF.Triple},IEnumerable{Triple})">UpdateGraph()</see> method.
/// </summary>
public class WriteToStoreHandler
    : BaseRdfHandler
{
    /// <summary>
    /// Default Batch Size for writes.
    /// </summary>
    public const int DefaultBatchSize = 1000;

    private IStorageProvider _manager;
    private List<Triple> _actions;
    private List<Tuple<Triple, IRefNode>> _bnodeActions;
    private HashSet<IRefNode> _bnodeUris;
    private IRefNode _defaultGraphUri, _currGraphUri;
    private int _batchSize;

    /// <summary>
    /// Creates a new Write to Store Handler.
    /// </summary>
    /// <param name="manager">Manager to write to.</param>
    /// <param name="defaultGraphUri">Graph URI to write Triples from the default graph to.</param>
    /// <param name="batchSize">Batch Size.</param>
    public WriteToStoreHandler(IStorageProvider manager, Uri defaultGraphUri, int batchSize)
    {
        if (manager == null) throw new ArgumentNullException("manager", "Cannot write to a null Generic IO Manager");
        if (manager.IsReadOnly) throw new ArgumentException("manager", "Cannot write to a Read-Only Generic IO Manager");
        if (!manager.UpdateSupported) throw new ArgumentException("manager", "Generic IO Manager must support Triple Level updates to be used with this Handler");
        if (batchSize <= 0) throw new ArgumentException("batchSize", "Batch Size must be >= 1");

        _manager = manager;
        _defaultGraphUri = defaultGraphUri == null ? null : new UriNode(defaultGraphUri);
        _batchSize = batchSize;

        // Make the Actions Queue one larger than the Batch Size
        _actions = new List<Triple>(_batchSize + 1);
        _bnodeActions = new List<Tuple<Triple, IRefNode>>();
        _bnodeUris = new HashSet<IRefNode>();
    }

    /// <summary>
    /// Creates a new Write to Store Handler.
    /// </summary>
    /// <param name="manager">Manager to write to.</param>
    /// <param name="defaultGraphUri">Graph URI to write Triples from the default graph to.</param>
    public WriteToStoreHandler(IStorageProvider manager, Uri defaultGraphUri)
        : this(manager, defaultGraphUri, DefaultBatchSize) { }

    /// <summary>
    /// Creates a new Write to Store Handler.
    /// </summary>
    /// <param name="manager">Manager to write to.</param>
    /// <param name="batchSize">Batch Size.</param>
    public WriteToStoreHandler(IStorageProvider manager, int batchSize)
        : this(manager, null, batchSize) { }

    /// <summary>
    /// Creates a new Write to Store Handler.
    /// </summary>
    /// <param name="manager">Manager to write to.</param>
    public WriteToStoreHandler(IStorageProvider manager)
        : this(manager, null, DefaultBatchSize) { }

    /// <summary>
    /// Starts RDF Handling by ensuring the queue of Triples to write is empty.
    /// </summary>
    protected override void StartRdfInternal()
    {
        _actions.Clear();
        _bnodeActions.Clear();
        _currGraphUri = _defaultGraphUri;
    }

    /// <summary>
    /// Ends RDF Handling by ensuring the queue of Triples to write has been processed.
    /// </summary>
    /// <param name="ok">Indicates whether parsing completed without error.</param>
    protected override void EndRdfInternal(bool ok)
    {
        // First process the last batch of ground triples (if any)
        if (_actions.Count > 0)
        {
            ProcessBatch();
        }
        // Then process each batch of non-ground triples
        foreach (IRefNode g in _bnodeUris)
        {
            var batch = new List<Triple>();
            for (var i = 0; i < _bnodeActions.Count; i++)
            {
                if (EqualityHelper.AreRefNodesEqual(g, _bnodeActions[i].Item2))
                {
                    batch.Add(_bnodeActions[i].Item1);
                    _bnodeActions.RemoveAt(i);
                    i--;
                }
            }
            if (g == null)
            {
                _manager.UpdateGraph(_defaultGraphUri, batch, null);
            }
            else
            {
                _manager.UpdateGraph(g, batch, null);
            }
        }
    }

    /// <summary>
    /// Handles triples by passing them to <see cref="HandleQuadInternal"/> for queuing, passing null as the target graph name.
    /// </summary>
    /// <param name="t">Triple to handle.</param>
    /// <returns></returns>
    protected override bool HandleTripleInternal(Triple t)
    {
        return HandleQuadInternal(t, _defaultGraphUri);
    }

    /// <summary>
    /// Handles quads by queuing them for writing and enacting the writing if the Batch Size has been reached/exceeded.
    /// </summary>
    /// <param name="t">Triple.</param>
    /// <param name="graph">The graph containing the triple.</param>
    /// <returns></returns>
    protected override bool HandleQuadInternal(Triple t, IRefNode graph)
    {
        if (t.IsGroundTriple)
        {
            // Ground Triples are processed in Batches as we handle the Triples
            if (graph != null && !EqualityHelper.AreRefNodesEqual(graph, _currGraphUri))
            {
                // The Triple has a Graph URI and it is not the same as the Current Graph URI
                // so we process the existing Batch and then set the Current Graph URI to the new Graph URI
                ProcessBatch();
                _currGraphUri = graph;
            }
            else if (graph == null && !EqualityHelper.AreRefNodesEqual(_defaultGraphUri, _currGraphUri))
            {
                // The Triple has no Graph URI and the Current Graph URI is not the Default Graph URI so
                // we process the existing Batch and reset the Current Graph URI to the Default Graph URI
                ProcessBatch();
                _currGraphUri = _defaultGraphUri;
            }

            _actions.Add(t);

            // Whenever we hit the Batch Size process it
            if (_actions.Count >= _batchSize)
            {
                ProcessBatch();
            }
        }
        else
        {
            // Non-Ground Triples (i.e. those with Blank Nodes) are saved up until the end to ensure that Blank
            // Node are persisted properly
            _bnodeActions.Add(new Tuple<Triple, IRefNode>(t, graph));
            _bnodeUris.Add(graph);
        }
        return true;
    }

    private void ProcessBatch()
    {
        if (_actions.Count > 0)
        {
            _manager.UpdateGraph(_currGraphUri, _actions, null);
            _actions.Clear();
        }
    }

    /// <summary>
    /// Gets that the Handler accepts all Triples.
    /// </summary>
    public override bool AcceptsAll
    {
        get 
        {
            return true;
        }
    }
}