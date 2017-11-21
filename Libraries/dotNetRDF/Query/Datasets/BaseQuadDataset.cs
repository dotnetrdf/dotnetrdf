/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.Common.References;

namespace VDS.RDF.Query.Datasets
{
    /// <summary>
    /// Abstract Base class of dataset designed around out of memory datasets where you rarely wish to load data into memory but simply wish to know which graph to look in for data
    /// </summary>
    public abstract class BaseQuadDataset
        : ISparqlDataset
    {
        private readonly ThreadIsolatedReference<Stack<IEnumerable<Uri>>> _defaultGraphs;
        private readonly ThreadIsolatedReference<Stack<IEnumerable<Uri>>> _activeGraphs;
        private bool _unionDefaultGraph = true;
        private Uri _defaultGraphUri;

        /// <summary>
        /// Creates a new Quad Dataset
        /// </summary>
        public BaseQuadDataset()
        {
            _defaultGraphs = new ThreadIsolatedReference<Stack<IEnumerable<Uri>>>(InitDefaultGraphStack);
            _activeGraphs = new ThreadIsolatedReference<Stack<IEnumerable<Uri>>>(InitActiveGraphStack);
        }

        /// <summary>
        /// Creates a new Quad Dataset
        /// </summary>
        /// <param name="unionDefaultGraph">Whether to make the default graph the union of all graphs</param>
        public BaseQuadDataset(bool unionDefaultGraph)
            : this()
        {
            _unionDefaultGraph = unionDefaultGraph;
        }

        /// <summary>
        /// Creates a new Quad Dataset
        /// </summary>
        /// <param name="defaultGraphUri">URI of the Default Graph</param>
        public BaseQuadDataset(Uri defaultGraphUri)
            : this(false)
        {
            _defaultGraphUri = defaultGraphUri;
        }

        private Stack<IEnumerable<Uri>> InitDefaultGraphStack()
        {
            Stack<IEnumerable<Uri>> s = new Stack<IEnumerable<Uri>>();
            if (!_unionDefaultGraph)
            {
                s.Push(new Uri[] { _defaultGraphUri });
            }
            return s;
        }

        private Stack<IEnumerable<Uri>> InitActiveGraphStack()
        {
            return new Stack<IEnumerable<Uri>>();
        }

        #region Active and Default Graph management

        /// <summary>
        /// Sets the Active Graph
        /// </summary>
        /// <param name="graphUris">Graph URIs</param>
        public void SetActiveGraph(IEnumerable<Uri> graphUris)
        {
            _activeGraphs.Value.Push(graphUris.ToList());
        }

        /// <summary>
        /// Sets the Active Graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public void SetActiveGraph(Uri graphUri)
        {
            if (graphUri == null)
            {
                _activeGraphs.Value.Push(DefaultGraphUris);
            }
            else
            {
                _activeGraphs.Value.Push(new Uri[] { graphUri });
            }
        }

        /// <summary>
        /// Sets the Default Graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public void SetDefaultGraph(Uri graphUri)
        {
            _defaultGraphs.Value.Push(new Uri[] { graphUri });
        }

        /// <summary>
        /// Sets the Default Graph
        /// </summary>
        /// <param name="graphUris">Graph URIs</param>
        public void SetDefaultGraph(IEnumerable<Uri> graphUris)
        {
            _defaultGraphs.Value.Push(graphUris.ToList());
        }

        /// <summary>
        /// Resets the Active Graph
        /// </summary>
        public void ResetActiveGraph()
        {
            if (_activeGraphs.Value.Count > 0)
            {
                _activeGraphs.Value.Pop();
            }
            else
            {
                throw new RdfQueryException("Unable to reset the Active Graph since no previous Active Graphs exist");
            }
        }

        /// <summary>
        /// Resets the Default Graph
        /// </summary>
        public void ResetDefaultGraph()
        {
            if (_defaultGraphs.Value.Count > 0)
            {
                _defaultGraphs.Value.Pop();
            }
            else
            {
                throw new RdfQueryException("Unable to reset the Default Graph since no previous Default Graphs exist");
            }
        }

        /// <summary>
        /// Gets the Default Graph URIs
        /// </summary>
        public IEnumerable<Uri> DefaultGraphUris
        {
            get 
            {
                if (_defaultGraphs.Value.Count > 0)
                {
                    return _defaultGraphs.Value.Peek();
                }
                else if (_unionDefaultGraph)
                {
                    return GraphUris;
                }
                else
                {
                    return Enumerable.Empty<Uri>();
                }
            }
        }

        /// <summary>
        /// Gets the Active Graph URIs
        /// </summary>
        public IEnumerable<Uri> ActiveGraphUris
        {
            get 
            {
                if (_activeGraphs.Value.Count > 0)
                {
                    return _activeGraphs.Value.Peek();
                }
                else
                {
                    return DefaultGraphUris;
                }
            }
        }

        /// <summary>
        /// Gets whether this dataset uses a union default graph
        /// </summary>
        public bool UsesUnionDefaultGraph
        {
            get 
            {
                return _unionDefaultGraph;
            }
        }

        /// <summary>
        /// Gets whether the given URI represents the default graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected bool IsDefaultGraph(Uri graphUri)
        {
            return EqualityHelper.AreUrisEqual(graphUri, _defaultGraphUri);
        }

        #endregion

        /// <summary>
        /// Adds a Graph to the dataset
        /// </summary>
        /// <param name="g">Graph</param>
        public virtual bool AddGraph(IGraph g)
        {
            bool added = false;
            foreach (Triple t in g.Triples)
            {
                added = AddQuad(g.BaseUri, t) || added;
            }
            return added;
        }

        /// <summary>
        /// Adds a Quad to the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="t">Triple</param>
        protected internal abstract bool AddQuad(Uri graphUri, Triple t);

        /// <summary>
        /// Removes a Graph from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public abstract bool RemoveGraph(Uri graphUri);

        /// <summary>
        /// Removes a Quad from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="t">Triple</param>
        protected internal abstract bool RemoveQuad(Uri graphUri, Triple t);

        /// <summary>
        /// Gets whether a Graph with the given URI is the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public bool HasGraph(Uri graphUri)
        {
            if (graphUri == null)
            {
                if (DefaultGraphUris.Any())
                {
                    return true;
                }
                else
                {
                    return HasGraphInternal(null);
                }
            }
            else
            {
                return HasGraphInternal(graphUri);
            }
        }

        /// <summary>
        /// Determines whether a given Graph exists in the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected abstract bool HasGraphInternal(Uri graphUri);

        /// <summary>
        /// Gets the Graphs in the dataset
        /// </summary>
        public virtual IEnumerable<IGraph> Graphs
        {
            get 
            {
                return (from u in GraphUris
                        select this[u]);
            }
        }

        /// <summary>
        /// Gets the URIs of the graphs in the dataset
        /// </summary>
        public abstract IEnumerable<Uri> GraphUris
        {
            get;
        }

        /// <summary>
        /// Gets the Graph with the given URI from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// This property need only return a read-only view of the Graph, code which wishes to modify Graphs should use the <see cref="ISparqlDataset.GetModifiableGraph">GetModifiableGraph()</see> method to guarantee a Graph they can modify and will be persisted to the underlying storage
        /// </para>
        /// </remarks>
        public virtual IGraph this[Uri graphUri]
        {
            get
            {
                if (graphUri == null)
                {
                    if (DefaultGraphUris.Any())
                    {
                        if (DefaultGraphUris.Count() == 1)
                        {
                            return new Graph(new QuadDatasetTripleCollection(this, DefaultGraphUris.First()));
                        }
                        else
                        {
                            IEnumerable<IGraph> gs = (from u in DefaultGraphUris
                                                      select new Graph(new QuadDatasetTripleCollection(this, u))).OfType<IGraph>();
                            return new UnionGraph(gs.First(), gs.Skip(1));
                        }
                    }
                    else
                    {
                        return GetGraphInternal(null);
                    }
                }
                else
                {
                    return GetGraphInternal(graphUri);
                }
            }
        }

        /// <summary>
        /// Gets a Graph from the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected abstract IGraph GetGraphInternal(Uri graphUri);

        /// <summary>
        /// Gets a modifiable graph from the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public virtual IGraph GetModifiableGraph(Uri graphUri)
        {
            throw new NotSupportedException("This dataset is immutable");
        }

        /// <summary>
        /// Gets whether the dataset has any triples
        /// </summary>
        public virtual bool HasTriples
        {
            get 
            {
                return Triples.Any();
            }
        }

        /// <summary>
        /// Gets whether the dataset contains a triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public bool ContainsTriple(Triple t)
        {
            return ActiveGraphUris.Any(u => ContainsQuad(u, t));
        }

        /// <summary>
        /// Gets whether a Triple exists in a specific Graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected abstract internal bool ContainsQuad(Uri graphUri, Triple t);

        /// <summary>
        /// Gets all triples from the dataset
        /// </summary>
        public IEnumerable<Triple> Triples
        {
            get 
            { 
                return (from u in ActiveGraphUris
                        from t in GetQuads(u)
                        select t);
            }
        }

        /// <summary>
        /// Gets all the Triples for a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected abstract internal IEnumerable<Triple> GetQuads(Uri graphUri);

        /// <summary>
        /// Gets all the Triples with a given subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            return (from u in ActiveGraphUris
                    from t in GetQuadsWithSubject(u, subj)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given subject from a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        protected abstract internal IEnumerable<Triple> GetQuadsWithSubject(Uri graphUri, INode subj);

        /// <summary>
        /// Gets all the Triples with a given predicate
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            return (from u in ActiveGraphUris
                    from t in GetQuadsWithPredicate(u, pred)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given predicate from a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        protected abstract internal IEnumerable<Triple> GetQuadsWithPredicate(Uri graphUri, INode pred);

        /// <summary>
        /// Gets all the Triples with a given object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            return (from u in ActiveGraphUris
                    from t in GetQuadsWithObject(u, obj)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given object from a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected abstract internal IEnumerable<Triple> GetQuadsWithObject(Uri graphUri, INode obj);

        /// <summary>
        /// Gets all the Triples with a given subject and predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            return (from u in ActiveGraphUris
                    from t in GetQuadsWithSubjectPredicate(u, subj, pred)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given subject and predicate from a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        protected abstract internal IEnumerable<Triple> GetQuadsWithSubjectPredicate(Uri graphUri, INode subj, INode pred);

        /// <summary>
        /// Gets all the Triples with a given subject and object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            return (from u in ActiveGraphUris
                    from t in GetQuadsWithSubjectObject(u, subj, obj)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given subject and object from a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected abstract internal IEnumerable<Triple> GetQuadsWithSubjectObject(Uri graphUri, INode subj, INode obj);

        /// <summary>
        /// Gets all the Triples with a given predicate and object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            return (from u in ActiveGraphUris
                    from t in GetQuadsWithPredicateObject(u, pred, obj)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given predicate and object from a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected abstract internal IEnumerable<Triple> GetQuadsWithPredicateObject(Uri graphUri, INode pred, INode obj);

        /// <summary>
        /// Flushes any changes to the dataset
        /// </summary>
        public virtual void Flush()
        {
            // Nothing to do
        }

        /// <summary>
        /// Discards any changes to the dataset
        /// </summary>
        public virtual void Discard()
        {
            // Nothing to do
        }
    }

    /// <summary>
    /// Abstract Base class for immutable quad datasets
    /// </summary>
    public abstract class BaseImmutableQuadDataset
        : BaseQuadDataset
    {
        /// <summary>
        /// Throws an error as this dataset is immutable
        /// </summary>
        /// <param name="g">Graph</param>
        public sealed override bool AddGraph(IGraph g)
        {
            throw new NotSupportedException("This dataset is immutable");
        }

        /// <summary>
        /// Throws an error as this dataset is immutable
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="t">Triple</param>
        protected internal override bool AddQuad(Uri graphUri, Triple t)
        {
            throw new NotSupportedException("This dataset is immutable");
        }

        /// <summary>
        /// Throws an error as this dataset is immutable
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public sealed override bool RemoveGraph(Uri graphUri)
        {
            throw new NotSupportedException("This dataset is immutable");
        }

        /// <summary>
        /// Throws an error as this dataset is immutable
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="t">Triple</param>
        protected internal override bool RemoveQuad(Uri graphUri, Triple t)
        {
            throw new NotSupportedException("This dataset is immutable");
        }

        /// <summary>
        /// Throws an error as this dataset is immutable
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public sealed override IGraph GetModifiableGraph(Uri graphUri)
        {
            throw new NotSupportedException("This dataset is immutable");
        }
    }

    /// <summary>
    /// Abstract Base class for quad datasets that support transactions
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
        /// Creates a Transactional Quad Dataset
        /// </summary>
        public BaseTransactionalQuadDataset()
            : base() { }

        /// <summary>
        /// Creates a Transactional Quad Dataset
        /// </summary>
        /// <param name="unionDefaultGraph">Sets whether the default graph should be the union of all graphs</param>
        public BaseTransactionalQuadDataset(bool unionDefaultGraph)
            : base(unionDefaultGraph) { }

        /// <summary>
        /// Creates a Transactional Quad Dataset
        /// </summary>
        /// <param name="defaultGraphUri">Default Graph URI</param>
        public BaseTransactionalQuadDataset(Uri defaultGraphUri)
            : base(defaultGraphUri) { }

        /// <summary>
        /// Adds a Graph to the Dataset
        /// </summary>
        /// <param name="g">Graph to add</param>
        public sealed override bool AddGraph(IGraph g)
        {
            if (HasGraph(g.BaseUri))
            {
                ITransactionalGraph existing = (ITransactionalGraph)GetModifiableGraph(g.BaseUri);
                _actions.Add(new GraphPersistenceAction(existing, GraphPersistenceActionType.Modified));
            }
            else
            {
                _actions.Add(new GraphPersistenceAction(g, GraphPersistenceActionType.Added));
            }
            return AddGraphInternal(g);
        }

        /// <summary>
        /// Adds a Graph to the Dataset
        /// </summary>
        /// <param name="g">Graph to add</param>
        protected abstract bool AddGraphInternal(IGraph g);

        /// <summary>
        /// Removes a Graph from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public sealed override bool RemoveGraph(Uri graphUri)
        {
            if (graphUri == null)
            {
                if (DefaultGraphUris.Any())
                {
                    foreach (Uri u in DefaultGraphUris)
                    {
                        if (IsDefaultGraph(u))
                        {
                            // Default Graph gets cleared
                            GraphPersistenceWrapper wrapper = new GraphPersistenceWrapper(this[u]);
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
                else if (HasGraph(graphUri))
                {
                    _actions.Add(new GraphPersistenceAction(this[graphUri], GraphPersistenceActionType.Deleted));
                    return RemoveGraphInternal(graphUri);
                }
            }
            else if (HasGraph(graphUri))
            {
                _actions.Add(new GraphPersistenceAction(this[graphUri], GraphPersistenceActionType.Deleted));
                return RemoveGraphInternal(graphUri);
            }
            return false;
        }

        /// <summary>
        /// Removes a Graph from the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        protected abstract bool RemoveGraphInternal(Uri graphUri);

        /// <summary>
        /// Gets a Graph from the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public override IGraph this[Uri graphUri]
        {
            get
            {
                if (graphUri == null)
                {
                    if (DefaultGraphUris.Any())
                    {
                        if (DefaultGraphUris.Count() == 1)
                        {
                            return new Graph(new QuadDatasetTripleCollection(this, DefaultGraphUris.First()));
                        }
                        else
                        {
                            IEnumerable<IGraph> gs = (from u in DefaultGraphUris
                                                      select new Graph(new QuadDatasetTripleCollection(this, u))).OfType<IGraph>();
                            return new UnionGraph(gs.First(), gs.Skip(1));
                        }
                    }
                    else if (_modifiableGraphs.HasGraph(graphUri))
                    {
                        return _modifiableGraphs[graphUri];
                    }
                    else
                    {
                        return GetGraphInternal(null);
                    }
                }
                else if (_modifiableGraphs.HasGraph(graphUri))
                {
                    return _modifiableGraphs[graphUri];
                }
                else
                {
                    return GetGraphInternal(graphUri);
                }
            }
        }

        /// <summary>
        /// Gets a Graph from the Dataset that can be modified
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public sealed override IGraph GetModifiableGraph(Uri graphUri)
        {
            if (!_modifiableGraphs.HasGraph(graphUri))
            {
                IGraph current = GetModifiableGraphInternal(graphUri);
                if (!_modifiableGraphs.HasGraph(current.BaseUri))
                {
                    _modifiableGraphs.Add(current);
                }
                graphUri = current.BaseUri;
            }
            ITransactionalGraph existing = (ITransactionalGraph)_modifiableGraphs[graphUri];
            _actions.Add(new GraphPersistenceAction(existing, GraphPersistenceActionType.Modified));
            return existing;
        }

        /// <summary>
        /// Gets a Graph from the Dataset that can be modified transactionally
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected abstract ITransactionalGraph GetModifiableGraphInternal(Uri graphUri);

        /// <summary>
        /// Ensures that any changes to the Dataset (if any) are flushed to the underlying Storage
        /// </summary>
        /// <remarks>
        /// Commits the Active Transaction
        /// </remarks>
        public sealed override void Flush()
        {
            int i = 0;
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
        /// Ensures that any changes to the Dataset (if any) are discarded
        /// </summary>
        /// <remarks>
        /// Rollsback the Active Transaction
        /// </remarks>
        public sealed override void Discard()
        {
            int i = _actions.Count - 1;
            int total = _actions.Count;
            while (i >= 0)
            {
                GraphPersistenceAction action = _actions[i];
                switch (action.Action)
                {
                    case GraphPersistenceActionType.Added:
                        // If a Graph was added we must now remove it
                        if (HasGraphInternal(action.Graph.BaseUri))
                        {
                            RemoveGraphInternal(action.Graph.BaseUri);
                        }
                        break;
                    case GraphPersistenceActionType.Deleted:
                        // If a Graph was deleted we must now add it back again
                        // Don't add the full Graph only an empty Graph with the given URI
                        Graph g = new Graph();
                        g.BaseUri = action.Graph.BaseUri;
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
        /// Allows the derived dataset to take any post-Flush() actions required
        /// </summary>
        protected virtual void FlushInternal()
        {
            // No actions by default
        }

        /// <summary>
        /// Allows the derived dataset to take any post-Discard() actions required
        /// </summary>
        protected virtual void DiscardInternal()
        {
            // No actions by default
        }
    }
}
