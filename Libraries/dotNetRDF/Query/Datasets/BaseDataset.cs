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
    /// Abstract Base Class for Datasets which provides implementation of Active and Default Graph management
    /// </summary>
    public abstract class BaseDataset
        : ISparqlDataset
    {
        /// <summary>
        /// Reference to the Active Graph being used for executing a SPARQL Query
        /// </summary>
        private readonly ThreadIsolatedReference<IGraph> _activeGraph;
        /// <summary>
        /// Default Graph for executing SPARQL Queries against
        /// </summary>
        private readonly ThreadIsolatedReference<IGraph> _defaultGraph;
        /// <summary>
        /// Stack of Default Graph References used for executing a SPARQL Query when a Query may choose to change the Default Graph from the Dataset defined one
        /// </summary>
        private readonly ThreadIsolatedReference<Stack<IGraph>> _defaultGraphs;
        /// <summary>
        /// Stack of Active Graph References used for executing a SPARQL Query when there are nested GRAPH Clauses
        /// </summary>
        private readonly ThreadIsolatedReference<Stack<IGraph>> _activeGraphs;

        private readonly ThreadIsolatedReference<Stack<IEnumerable<Uri>>> _defaultGraphUris;

        private readonly ThreadIsolatedReference<Stack<IEnumerable<Uri>>> _activeGraphUris;

        private readonly bool _unionDefaultGraph = true;
        private readonly Uri _defaultGraphUri;

        /// <summary>
        /// Creates a new Dataset
        /// </summary>
        public BaseDataset()
        {
            _activeGraph = new ThreadIsolatedReference<IGraph>();
            _defaultGraph = new ThreadIsolatedReference<IGraph>(InitDefaultGraph);
            _defaultGraphs = new ThreadIsolatedReference<Stack<IGraph>>(InitGraphStack);
            _activeGraphs = new ThreadIsolatedReference<Stack<IGraph>>(InitGraphStack);
            _defaultGraphUris = new ThreadIsolatedReference<Stack<IEnumerable<Uri>>>(InitDefaultGraphUriStack);
            _activeGraphUris = new ThreadIsolatedReference<Stack<IEnumerable<Uri>>>(InitGraphUriStack);
        }

        /// <summary>
        /// Creates a new Dataset with the given Union Default Graph setting
        /// </summary>
        /// <param name="unionDefaultGraph">Whether to use a Union Default Graph</param>
        public BaseDataset(bool unionDefaultGraph)
            : this()
        {
            _unionDefaultGraph = unionDefaultGraph;
        }

        /// <summary>
        /// Creates a new Dataset with a fixed Default Graph and without a Union Default Graph
        /// </summary>
        /// <param name="defaultGraphUri"></param>
        public BaseDataset(Uri defaultGraphUri)
            : this()
        {
            _unionDefaultGraph = false;
            _defaultGraphUri = defaultGraphUri;
        }

        private IGraph InitDefaultGraph()
        {
            if (_unionDefaultGraph)
            {
                return null;
            }
            else
            {
                return GetGraphInternal(_defaultGraphUri);
            }
        }

        private Stack<IGraph> InitGraphStack()
        {
            return new Stack<IGraph>();
        }

        private Stack<IEnumerable<Uri>> InitDefaultGraphUriStack()
        {
            Stack<IEnumerable<Uri>> s = new Stack<IEnumerable<Uri>>();
            if (!_unionDefaultGraph)
            {
                s.Push(new Uri[] { _defaultGraphUri });
            }
            return s;
        }

        private Stack<IEnumerable<Uri>> InitGraphUriStack()
        {
            return new Stack<IEnumerable<Uri>>();
        }

        #region Active and Default Graph Management

        /// <summary>
        /// Gets a reference to the actual <see cref="IGraph">IGraph</see> that is currently treated as the default graph
        /// </summary>
        protected IGraph InternalDefaultGraph
        {
            get
            {
                return _defaultGraph.Value;
            }
        }

        /// <summary>
        /// Sets the Default Graph for the SPARQL Query
        /// </summary>
        /// <param name="g"></param>
        private void SetDefaultGraphInternal(IGraph g)
        {
            _defaultGraphs.Value.Push(_defaultGraph.Value);
            _defaultGraph.Value = g;
        }

        /// <summary>
        /// Sets the Default Graph
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public void SetDefaultGraph(Uri graphUri)
        {
            if (HasGraph(graphUri))
            {
                SetDefaultGraphInternal(this[graphUri]);
                _defaultGraphUris.Value.Push(new Uri[] { graphUri });
            }
            else
            {
                SetDefaultGraphInternal(new Graph());
                _defaultGraphUris.Value.Push(Enumerable.Empty<Uri>());
            }
        }

        /// <summary>
        /// Sets the Default Graph
        /// </summary>
        /// <param name="graphUris">Graph URIs</param>
        public void SetDefaultGraph(IEnumerable<Uri> graphUris)
        {
            if (!graphUris.Any())
            {
                SetDefaultGraphInternal(new Graph());
                _defaultGraphUris.Value.Push(Enumerable.Empty<Uri>());
            }
            else if (graphUris.Count() == 1)
            {
                SetDefaultGraph(graphUris.First());
            }
            else
            {
                // Multiple Graph URIs
                // Build a merged Graph of all the Graph URIs
                Graph g = new Graph();
                foreach (Uri u in graphUris)
                {
                    if (HasGraph(u))
                    {
                        g.Merge(this[u], true);
                    }
                }
                SetDefaultGraphInternal(g);
                _defaultGraphUris.Value.Push(graphUris.ToList());
            }
        }

        /// <summary>
        /// Sets the Active Graph for the SPARQL Query
        /// </summary>
        /// <param name="g">Active Graph</param>
        private void SetActiveGraphInternal(IGraph g)
        {
            _activeGraphs.Value.Push(_activeGraph.Value);
            _activeGraph.Value = g;
        }

        /// <summary>
        /// Sets the Active Graph for the SPARQL query
        /// </summary>
        /// <param name="graphUri">Uri of the Active Graph</param>
        /// <remarks>
        /// Helper function used primarily in the execution of GRAPH Clauses
        /// </remarks>
        public void SetActiveGraph(Uri graphUri)
        {
            if (graphUri == null)
            {
                // Change the Active Graph so that the query operates over the default graph
                // If the default graph is null then it operates over the entire dataset
                _activeGraphs.Value.Push(_activeGraph.Value);
                _activeGraph.Value = _defaultGraph.Value;
                _activeGraphUris.Value.Push(_defaultGraphUris.Value.Count > 0 ? _defaultGraphUris.Value.Peek() : Enumerable.Empty<Uri>());
            }
            else if (HasGraph(graphUri))
            {
                SetActiveGraphInternal(this[graphUri]);
                _activeGraphUris.Value.Push(new Uri[] { graphUri });
            }
            else
            {
                // Active Graph is an empty Graph in the case where the Graph is not present in the Dataset
                SetActiveGraphInternal(new Graph());
                _activeGraphUris.Value.Push(Enumerable.Empty<Uri>());
            }
        }

        /// <summary>
        /// Sets the Active Graph for the SPARQL query
        /// </summary>
        /// <param name="graphUris">URIs of the Graphs which form the Active Graph</param>
        /// <remarks>Helper function used primarily in the execution of GRAPH Clauses</remarks>
        public void SetActiveGraph(IEnumerable<Uri> graphUris)
        {
            if (!graphUris.Any())
            {
                SetActiveGraph((Uri)null);
            }
            else if (graphUris.Count() == 1)
            {
                // If only 1 Graph Uri call the simpler SetActiveGraph method which will be quicker
                SetActiveGraph(graphUris.First());
            }
            else
            {
                // Multiple Graph URIs
                // Build a merged Graph of all the Graph URIs
                Graph g = new Graph();
                foreach (Uri u in graphUris)
                {
                    if (HasGraph(u))
                    {
                        g.Merge(this[u], true);
                    }
                }
                SetActiveGraphInternal(g);
                _activeGraphUris.Value.Push(graphUris.ToList());
            }
        }

        /// <summary>
        /// Sets the Active Graph for the SPARQL query to be the previous Active Graph
        /// </summary>
        public void ResetActiveGraph()
        {
            if (_activeGraphs.Value.Count > 0)
            {
                _activeGraph.Value = _activeGraphs.Value.Pop();
                _activeGraphUris.Value.Pop();
            }
            else
            {
                throw new RdfQueryException("Unable to reset the Active Graph since no previous Active Graphs exist");
            }
        }

        /// <summary>
        /// Sets the Default Graph for the SPARQL Query to be the previous Default Graph
        /// </summary>
        public void ResetDefaultGraph()
        {
            if (_defaultGraphs.Value.Count > 0)
            {
                _defaultGraph.Value = _defaultGraphs.Value.Pop();
                _defaultGraphUris.Value.Pop();
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
                if (_defaultGraphUris.Value.Count > 0)
                {
                    return _defaultGraphUris.Value.Peek();
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
                if (_activeGraphUris.Value.Count > 0)
                {
                    return _activeGraphUris.Value.Peek();
                }
                else
                {
                    return Enumerable.Empty<Uri>();
                }
            }
        }

        /// <summary>
        /// Gets whether the Default Graph is treated as being the union of all Graphs in the dataset when no Default Graph is otherwise set
        /// </summary>
        public bool UsesUnionDefaultGraph
        {
            get
            {
                return _unionDefaultGraph;
            }
        }

        #endregion

        #region General Implementation

        /// <summary>
        /// Adds a Graph to the Dataset
        /// </summary>
        /// <param name="g">Graph</param>
        public abstract bool AddGraph(IGraph g);

        /// <summary>
        /// Removes a Graph from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public virtual bool RemoveGraph(Uri graphUri)
        {
            if (graphUri == null)
            {
                if (_defaultGraph != null)
                {
                    _defaultGraph.Value.Clear();
                    return true;
                }
                else if (HasGraph(graphUri))
                {
                    return RemoveGraphInternal(graphUri);
                }
            }
            else if (HasGraph(graphUri))
            {
                return RemoveGraphInternal(graphUri);
            }
            return false;
        }

        /// <summary>
        /// Removes a Graph from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        protected abstract bool RemoveGraphInternal(Uri graphUri);

        /// <summary>
        /// Gets whether a Graph with the given URI is the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public bool HasGraph(Uri graphUri)
        {
            if (graphUri == null)
            {
                if (_defaultGraph != null)
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
        /// Gets all the Graphs in the Dataset
        /// </summary>
        public abstract IEnumerable<IGraph> Graphs
        {
            get;
        }

        /// <summary>
        /// Gets all the URIs of Graphs in the Dataset
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
                    if (_defaultGraph != null)
                    {
                        return _defaultGraph.Value;
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
        /// Gets the given Graph from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected abstract IGraph GetGraphInternal(Uri graphUri);

        /// <summary>
        /// Gets the Graph with the given URI from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Graphs returned from this method must be modifiable and the Dataset must guarantee that when it is Flushed or Disposed of that any changes to the Graph are persisted
        /// </para>
        /// </remarks>
        public abstract IGraph GetModifiableGraph(Uri graphUri);

        /// <summary>
        /// Gets whether the Dataset has any Triples
        /// </summary>
        public virtual bool HasTriples
        {
            get 
            { 
                return Triples.Any(); 
            }
        }

        /// <summary>
        /// Gets whether the Dataset contains a specific Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public bool ContainsTriple(Triple t)
        {
            if (_activeGraph.Value == null)
            {
                if (_defaultGraph.Value == null)
                {
                    return ContainsTripleInternal(t);
                }
                else
                {
                    return _defaultGraph.Value.ContainsTriple(t);
                }
            }
            else
            {
                return _activeGraph.Value.ContainsTriple(t);
            }
        }

        /// <summary>
        /// Determines whether the Dataset contains a specific Triple
        /// </summary>
        /// <param name="t">Triple to search for</param>
        /// <returns></returns>
        protected abstract bool ContainsTripleInternal(Triple t);

        /// <summary>
        /// Gets all the Triples in the Dataset
        /// </summary>
        public IEnumerable<Triple> Triples
        {
            get
            {
                if (_activeGraph.Value == null)
                {
                    if (_defaultGraph.Value == null)
                    {
                        // No specific Active Graph which implies that the Default Graph is the entire Triple Store
                        return GetAllTriples();
                    }
                    else
                    {
                        // Specific Default Graph so return that
                        return _defaultGraph.Value.Triples;
                    }
                }
                else
                {
                    // Active Graph is used (which may happen to be the Default Graph)
                    return _activeGraph.Value.Triples;
                }
            }
        }

        /// <summary>
        /// Abstract method that concrete implementations must implement to return an enumerable of all the Triples in the Dataset
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<Triple> GetAllTriples();

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            if (_activeGraph.Value == null)
            {
                if (_defaultGraph.Value == null)
                {
                    return GetTriplesWithSubjectInternal(subj);
                }
                else
                {
                    return _defaultGraph.Value.GetTriplesWithSubject(subj);
                }
            }
            else
            {
                return _activeGraph.Value.GetTriplesWithSubject(subj);
            }
        }

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Subject
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        protected abstract IEnumerable<Triple> GetTriplesWithSubjectInternal(INode subj);

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Predicate
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            if (_activeGraph.Value == null)
            {
                if (_defaultGraph.Value == null)
                {
                    return GetTriplesWithPredicateInternal(pred);
                }
                else
                {
                    return _defaultGraph.Value.GetTriplesWithPredicate(pred);
                }
            }
            else
            {
                return _activeGraph.Value.GetTriplesWithPredicate(pred);
            }
        }

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Predicate
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        protected abstract IEnumerable<Triple> GetTriplesWithPredicateInternal(INode pred);

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            if (_activeGraph.Value == null)
            {
                if (_defaultGraph.Value == null)
                {
                    return GetTriplesWithObjectInternal(obj);
                }
                else
                {
                    return _defaultGraph.Value.GetTriplesWithObject(obj);
                }
            }
            else
            {
                return _activeGraph.Value.GetTriplesWithObject(obj);
            }
        }

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Object
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected abstract IEnumerable<Triple> GetTriplesWithObjectInternal(INode obj);

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Subject and Predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            if (_activeGraph.Value == null)
            {
                if (_defaultGraph.Value == null)
                {
                    return GetTriplesWithSubjectPredicateInternal(subj, pred);
                }
                else
                {
                    return _defaultGraph.Value.GetTriplesWithSubjectPredicate(subj, pred);
                }
            }
            else
            {
                return _activeGraph.Value.GetTriplesWithSubjectPredicate(subj, pred);
            }
        }

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Subject and Predicate
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        protected abstract IEnumerable<Triple> GetTriplesWithSubjectPredicateInternal(INode subj, INode pred);

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Subject and Object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            if (_activeGraph.Value == null)
            {
                if (_defaultGraph.Value == null)
                {
                    return GetTriplesWithSubjectObjectInternal(subj, obj);
                }
                else
                {
                    return _defaultGraph.Value.GetTriplesWithSubjectObject(subj, obj);
                }
            }
            else
            {
                return _activeGraph.Value.GetTriplesWithSubjectObject(subj, obj);
            }
        }

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Subject and Object
        /// </summary>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected abstract IEnumerable<Triple> GetTriplesWithSubjectObjectInternal(INode subj, INode obj);

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Predicate and Object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            if (_activeGraph.Value == null)
            {
                if (_defaultGraph.Value == null)
                {
                    return GetTriplesWithPredicateObjectInternal(pred, obj);
                }
                else
                {
                    return _defaultGraph.Value.GetTriplesWithPredicateObject(pred, obj);
                }
            }
            else
            {
                return _activeGraph.Value.GetTriplesWithPredicateObject(pred, obj);
            }
        }

        /// <summary>
        /// Gets all the Triples in the Dataset with the given Predicate and Object
        /// </summary>
        /// <param name="pred">Predicate</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected abstract IEnumerable<Triple> GetTriplesWithPredicateObjectInternal(INode pred, INode obj);

        #endregion

        /// <summary>
        /// Ensures that any changes to the Dataset (if any) are flushed to the underlying Storage
        /// </summary>
        public abstract void Flush();

        /// <summary>
        /// Ensures that any changes to the Dataset (if any) are discarded
        /// </summary>
        public abstract void Discard();
    }

    /// <summary>
    /// Abstract Base Class for Immutable Datasets
    /// </summary>
    public abstract class BaseImmutableDataset
        : BaseDataset
    {
        /// <summary>
        /// Throws an exception since Immutable Datasets cannot be altered
        /// </summary>
        /// <param name="g">Graph to add</param>
        public override bool AddGraph(IGraph g)
        {
            throw new NotSupportedException("Cannot add a Graph to an immutable Dataset");
        }

        /// <summary>
        /// Throws an exception since Immutable Datasets cannot be altered
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public override bool RemoveGraph(Uri graphUri)
        {
            throw new NotSupportedException("Cannot remove a Graph from an immutable Dataset");
        }

        /// <summary>
        /// Throws an exception since Immutable Datasets cannot be altered
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public override IGraph GetModifiableGraph(Uri graphUri)
        {
            throw new NotSupportedException("Cannot retrieve a Modifiable Graph from an immutable Dataset");
        }

        /// <summary>
        /// Ensures that any changes to the Dataset (if any) are flushed to the underlying Storage
        /// </summary>
        public sealed override void Flush()
        {
            // Does Nothing
        }

        /// <summary>
        /// Ensures that any changes to the Dataset (if any) are discarded
        /// </summary>
        public sealed override void Discard()
        {
            // Does Nothing
        }
    }

    /// <summary>
    /// Abstract Base Class for Mutable Datasets that support Transactions
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Transaction implementation of dotNetRDF is based upon a MRSW concurrency model, since only one writer may be active changes are immediately pushed to the dataset and visible within the transaction and they are committed or rolled back when <see cref="BaseTransactionalDataset.Flush()">Flush()</see> or <see cref="BaseTransactionalDataset.Discard()">Discard()</see> are called.
    /// </para>
    /// <para>
    /// So in practical terms it is perfectly OK for the storage to be updated during a transaction because if the transaction fails the changes will be rolled back because all changes are stored in-memory until the end of the transaction.  This may not be an ideal transaction model for all scenarios so you may wish to implement your own version of transactions or code your implementations of the abstract methods accordingly to limit actual persistence to the end of a transaction.
    /// </para>
    /// </remarks>
    public abstract class BaseTransactionalDataset
        : BaseDataset
    {
        private List<GraphPersistenceAction> _actions = new List<GraphPersistenceAction>();
        private TripleStore _modifiableGraphs = new TripleStore();

        /// <summary>
        /// Creates a new Transactional Dataset
        /// </summary>
        public BaseTransactionalDataset()
            : base() { }

        /// <summary>
        /// Creates a new Transactional Dataset with the given Union Default Graph setting
        /// </summary>
        /// <param name="unionDefaultGraph">Whether to use a Union Default Graph</param>
        public BaseTransactionalDataset(bool unionDefaultGraph)
            : base(unionDefaultGraph) { }

        /// <summary>
        /// Creates a new Transactional Dataset with a fixed Default Graph and no Union Default Graph
        /// </summary>
        /// <param name="defaultGraphUri">Default Graph URI</param>
        public BaseTransactionalDataset(Uri defaultGraphUri)
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
                if (InternalDefaultGraph != null)
                {
                    GraphPersistenceWrapper wrapper = new GraphPersistenceWrapper(InternalDefaultGraph);
                    wrapper.Clear();
                    _actions.Add(new GraphPersistenceAction(wrapper, GraphPersistenceActionType.Modified));
                    return true;
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
        /// Gets a Graph from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        /// <remarks>
        /// If the Graph has been modified during the active Transaction the modified version is returned rather than the original version
        /// </remarks>
        public sealed override IGraph this[Uri graphUri]
        {
            get
            {
                if (graphUri == null)
                {
                    if (InternalDefaultGraph != null)
                    {
                        return InternalDefaultGraph;
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
