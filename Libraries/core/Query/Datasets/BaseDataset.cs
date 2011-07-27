/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Datasets
{
    /// <summary>
    /// Abstract Base Class for Datasets which provides implementation of Active and Default Graph management
    /// </summary>
    public abstract class BaseDataset : ISparqlDataset
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

        private readonly bool _unionDefaultGraph = true;
        private readonly Uri _defaultGraphUri;

        /// <summary>
        /// Creates a new Dataset
        /// </summary>
        public BaseDataset()
        {
            this._activeGraph = new ThreadIsolatedReference<IGraph>();
            this._defaultGraph = new ThreadIsolatedReference<IGraph>(this.InitDefaultGraph);
            this._defaultGraphs = new ThreadIsolatedReference<Stack<IGraph>>(this.InitGraphStack);
            this._activeGraphs = new ThreadIsolatedReference<Stack<IGraph>>(this.InitGraphStack);
        }

        /// <summary>
        /// Creates a new Dataset with the given Union Default Graph setting
        /// </summary>
        /// <param name="unionDefaultGraph">Whether to use a Union Default Graph</param>
        public BaseDataset(bool unionDefaultGraph)
            : this()
        {
            this._unionDefaultGraph = unionDefaultGraph;
        }

        /// <summary>
        /// Creates a new Dataset with a fixed Default Graph and without a Union Default Graph
        /// </summary>
        /// <param name="defaultGraphUri"></param>
        public BaseDataset(Uri defaultGraphUri)
            : this()
        {
            this._unionDefaultGraph = false;
            this._defaultGraphUri = defaultGraphUri;
        }

        private IGraph InitDefaultGraph()
        {
            if (this._unionDefaultGraph)
            {
                return null;
            }
            else
            {
                return this.GetGraphInternal(this._defaultGraphUri);
            }
        }

        private Stack<IGraph> InitGraphStack()
        {
            return new Stack<IGraph>();
        }

        #region Active and Default Graph Management

        /// <summary>
        /// Sets the Default Graph for the SPARQL Query
        /// </summary>
        /// <param name="g"></param>
        public void SetDefaultGraph(IGraph g)
        {
            this._defaultGraphs.Value.Push(this._defaultGraph.Value);
            this._defaultGraph.Value = g;
        }

        /// <summary>
        /// Sets the Active Graph for the SPARQL Query
        /// </summary>
        /// <param name="g">Active Graph</param>
        public void SetActiveGraph(IGraph g)
        {
            this._activeGraphs.Value.Push(this._activeGraph.Value);
            this._activeGraph.Value = g;
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
                //Change the Active Graph so that the query operates over the default graph
                //If the default graph is null then it operates over the entire dataset
                this._activeGraphs.Value.Push(this._activeGraph.Value);
                this._activeGraph.Value = this._defaultGraph.Value;
            }
            else if (this.HasGraph(graphUri))
            {
                //Push current Active Graph on the Stack
                this._activeGraphs.Value.Push(this._activeGraph.Value);

                //Set the new Active Graph
                this._activeGraph.Value = this[graphUri];
            }
            else
            {
                //Active Graph is an empty Graph in the case where the Graph is not present in the Dataset
                this._activeGraphs.Value.Push(this._activeGraph.Value);
                this._activeGraph.Value = new Graph();
            }
        }

        /// <summary>
        /// Sets the Active Graph for the SPARQL query
        /// </summary>
        /// <param name="graphUris">URIs of the Graphs which form the Active Graph</param>
        /// <remarks>Helper function used primarily in the execution of GRAPH Clauses</remarks>
        public void SetActiveGraph(IEnumerable<Uri> graphUris)
        {
            if (graphUris.Count() == 1)
            {
                //If only 1 Graph Uri call the simpler SetActiveGraph method which will be quicker
                this.SetActiveGraph(graphUris.First());
            }
            else
            {
                //Multiple Graph URIs
                //Build a merged Graph of all the Graph URIs
                Graph g = new Graph();
                foreach (Uri u in graphUris)
                {
                    if (this.HasGraph(u))
                    {
                        g.Merge(this[u], true);
                    }
                    //else
                    //{
                    //    throw new RdfQueryException("A Graph with URI '" + u.ToString() + "' does not exist in this Triple Store, a GRAPH Clause cannot be used to change the Active Graph to a Graph that doesn't exist");
                    //}
                }

                //Push current Active Graph on the Stack
                this._activeGraphs.Value.Push(this._activeGraph.Value);

                //Set the new Active Graph
                this._activeGraph.Value = g;
            }
        }

        /// <summary>
        /// Sets the Active Graph for the SPARQL query to be the previous Active Graph
        /// </summary>
        public void ResetActiveGraph()
        {
            if (this._activeGraphs.Value.Count > 0)
            {
                this._activeGraph.Value = this._activeGraphs.Value.Pop();
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
            if (this._defaultGraphs.Value.Count > 0)
            {
                this._defaultGraph.Value = this._defaultGraphs.Value.Pop();
            }
            else
            {
                throw new RdfQueryException("Unable to reset the Default Graph since no previous Default Graphs exist");
            }
        }

        /// <summary>
        /// Gets the current Default Graph (null if none)
        /// </summary>
        public IGraph DefaultGraph
        {
            get
            {
                return this._defaultGraph.Value;
            }
        }

        /// <summary>
        /// Gets the current Active Graph (null if none)
        /// </summary>
        public IGraph ActiveGraph
        {
            get
            {
                return this._activeGraph.Value;
            }
        }

        /// <summary>
        /// Gets whether the Default Graph is treated as being the union of all Graphs in the dataset when no Default Graph is otherwise set
        /// </summary>
        public bool UsesUnionDefaultGraph
        {
            get
            {
                return this._unionDefaultGraph;
            }
        }

        #endregion

        #region General Implementation

        /// <summary>
        /// Adds a Graph to the Dataset
        /// </summary>
        /// <param name="g">Graph</param>
        public abstract void AddGraph(IGraph g);

        /// <summary>
        /// Removes a Graph from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public virtual void RemoveGraph(Uri graphUri)
        {
            if (graphUri == null || graphUri.ToSafeString().Equals(GraphCollection.DefaultGraphUri))
            {
                if (this._defaultGraph != null)
                {
                    this._defaultGraph.Value.Clear();
                }
                else if (this.HasGraph(graphUri))
                {
                    this.RemoveGraphInternal(graphUri);
                }
            }
            else if (this.HasGraph(graphUri))
            {
                this.RemoveGraphInternal(graphUri);
            }
        }

        /// <summary>
        /// Removes a Graph from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        protected abstract void RemoveGraphInternal(Uri graphUri);

        /// <summary>
        /// Gets whether a Graph with the given URI is the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public bool HasGraph(Uri graphUri)
        {
            if (graphUri == null || graphUri.ToSafeString().Equals(GraphCollection.DefaultGraphUri))
            {
                if (this._defaultGraph != null)
                {
                    return true;
                }
                else
                {
                    return this.HasGraphInternal(null);
                }
            }
            else
            {
                return this.HasGraphInternal(graphUri);
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
                if (graphUri == null || graphUri.ToSafeString().Equals(GraphCollection.DefaultGraphUri))
                {
                    if (this._defaultGraph != null)
                    {
                        return this._defaultGraph.Value;
                    }
                    else
                    {
                        return this.GetGraphInternal(null);
                    }
                }
                else
                {
                    return this.GetGraphInternal(graphUri);
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
                return this.Triples.Any(); 
            }
        }

        /// <summary>
        /// Gets whether the Dataset contains a specific Triple
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public bool ContainsTriple(Triple t)
        {
            if (this._activeGraph.Value == null)
            {
                if (this._defaultGraph.Value == null)
                {
                    return this.ContainsTripleInternal(t);
                }
                else
                {
                    return this._defaultGraph.Value.ContainsTriple(t);
                }
            }
            else
            {
                return this._activeGraph.Value.ContainsTriple(t);
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
                if (this._activeGraph.Value == null)
                {
                    if (this._defaultGraph.Value == null)
                    {
                        //No specific Active Graph which implies that the Default Graph is the entire Triple Store
                        return this.GetAllTriples();
                    }
                    else
                    {
                        //Specific Default Graph so return that
                        return this._defaultGraph.Value.Triples;
                    }
                }
                else
                {
                    //Active Graph is used (which may happen to be the Default Graph)
                    return this._activeGraph.Value.Triples;
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
            if (this._activeGraph.Value == null)
            {
                if (this._defaultGraph.Value == null)
                {
                    return this.GetTriplesWithSubjectInternal(subj);
                }
                else
                {
                    return this._defaultGraph.Value.GetTriplesWithSubject(subj);
                }
            }
            else
            {
                return this._activeGraph.Value.GetTriplesWithSubject(subj);
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
            if (this._activeGraph.Value == null)
            {
                if (this._defaultGraph.Value == null)
                {
                    return this.GetTriplesWithPredicateInternal(pred);
                }
                else
                {
                    return this._defaultGraph.Value.GetTriplesWithPredicate(pred);
                }
            }
            else
            {
                return this._activeGraph.Value.GetTriplesWithPredicate(pred);
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
            if (this._activeGraph.Value == null)
            {
                if (this._defaultGraph.Value == null)
                {
                    return this.GetTriplesWithObjectInternal(obj);
                }
                else
                {
                    return this._defaultGraph.Value.GetTriplesWithObject(obj);
                }
            }
            else
            {
                return this._activeGraph.Value.GetTriplesWithObject(obj);
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
            if (this._activeGraph.Value == null)
            {
                if (this._defaultGraph.Value == null)
                {
                    return this.GetTriplesWithSubjectPredicateInternal(subj, pred);
                }
                else
                {
                    return this._defaultGraph.Value.GetTriplesWithSubjectPredicate(subj, pred);
                }
            }
            else
            {
                return this._activeGraph.Value.GetTriplesWithSubjectPredicate(subj, pred);
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
            if (this._activeGraph.Value == null)
            {
                if (this._defaultGraph.Value == null)
                {
                    return this.GetTriplesWithSubjectObjectInternal(subj, obj);
                }
                else
                {
                    return this._defaultGraph.Value.GetTriplesWithSubjectObject(subj, obj);
                }
            }
            else
            {
                return this._activeGraph.Value.GetTriplesWithSubjectObject(subj, obj);
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
            if (this._activeGraph.Value == null)
            {
                if (this._defaultGraph.Value == null)
                {
                    return this.GetTriplesWithPredicateObjectInternal(pred, obj);
                }
                else
                {
                    return this._defaultGraph.Value.GetTriplesWithPredicateObject(pred, obj);
                }
            }
            else
            {
                return this._activeGraph.Value.GetTriplesWithPredicateObject(pred, obj);
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
    public abstract class BaseImmutableDataset : BaseDataset
    {
        /// <summary>
        /// Throws an exception since Immutable Datasets cannot be altered
        /// </summary>
        /// <param name="g">Graph to add</param>
        public override void AddGraph(IGraph g)
        {
            throw new NotSupportedException("Cannot add a Graph to an immutable Dataset");
        }

        /// <summary>
        /// Throws an exception since Immutable Datasets cannot be altered
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public override void RemoveGraph(Uri graphUri)
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
            //Does Nothing
        }

        /// <summary>
        /// Ensures that any changes to the Dataset (if any) are discarded
        /// </summary>
        public sealed override void Discard()
        {
            //Does Nothing
        }
    }

    /// <summary>
    /// Abstract Base Class for Mutable Datasets that support Transactions
    /// </summary>
    public abstract class BaseTransactionalDataset : BaseDataset
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
        public sealed override void AddGraph(IGraph g)
        {
            if (this.HasGraph(g.BaseUri))
            {
                ITransactionalGraph existing = (ITransactionalGraph)this.GetModifiableGraph(g.BaseUri);
                this._actions.Add(new GraphPersistenceAction(existing, GraphPersistenceActionType.Modified));
            }
            else
            {
                this._actions.Add(new GraphPersistenceAction(g, GraphPersistenceActionType.Added));
            }
            this.AddGraphInternal(g);
        }

        /// <summary>
        /// Adds a Graph to the Dataset
        /// </summary>
        /// <param name="g">Graph to add</param>
        protected abstract void AddGraphInternal(IGraph g);

        /// <summary>
        /// Removes a Graph from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public sealed override void RemoveGraph(Uri graphUri)
        {
            if (graphUri == null || graphUri.ToSafeString().Equals(GraphCollection.DefaultGraphUri))
            {
                if (this.DefaultGraph != null)
                {
                    GraphPersistenceWrapper wrapper = new GraphPersistenceWrapper(DefaultGraph);
                    wrapper.Clear();
                    this._actions.Add(new GraphPersistenceAction(wrapper, GraphPersistenceActionType.Modified));
                }
                else if (this.HasGraph(graphUri))
                {
                    this._actions.Add(new GraphPersistenceAction(this[graphUri], GraphPersistenceActionType.Deleted));
                    this.RemoveGraphInternal(graphUri);
                }
            }
            else if (this.HasGraph(graphUri))
            {
                this._actions.Add(new GraphPersistenceAction(this[graphUri], GraphPersistenceActionType.Deleted));
                this.RemoveGraphInternal(graphUri);
            }
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
                if (graphUri == null || graphUri.ToSafeString().Equals(GraphCollection.DefaultGraphUri))
                {
                    if (this.DefaultGraph != null)
                    {
                        return this.DefaultGraph;
                    }
                    else if (this._modifiableGraphs.HasGraph(graphUri))
                    {
                        return this._modifiableGraphs.Graph(graphUri);
                    }
                    else
                    {
                        return this.GetGraphInternal(null);
                    }
                }
                else if (this._modifiableGraphs.HasGraph(graphUri))
                {
                    return this._modifiableGraphs.Graph(graphUri);
                }
                else
                {
                    return this.GetGraphInternal(graphUri);
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
            if (!this._modifiableGraphs.HasGraph(graphUri))
            {
                IGraph current = this.GetModifiableGraphInternal(graphUri);
                if (!this._modifiableGraphs.HasGraph(current.BaseUri))
                {
                    this._modifiableGraphs.Add(current);
                }
                graphUri = current.BaseUri;
            }
            ITransactionalGraph existing = (ITransactionalGraph)this._modifiableGraphs.Graph(graphUri);
            this._actions.Add(new GraphPersistenceAction(existing, GraphPersistenceActionType.Modified));
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
            while (i < this._actions.Count)
            {
                GraphPersistenceAction action = this._actions[i];
                switch (action.Action)
                {
                    case GraphPersistenceActionType.Added:
                        //If Graph was added ensure any changes were flushed
                        action.Graph.Flush();
                        break;
                    case GraphPersistenceActionType.Deleted:
                        //If Graph was deleted can discard any changes
                        action.Graph.Discard();
                        break;
                    case GraphPersistenceActionType.Modified:
                        //If Graph was modified ensure any changes were flushed
                        action.Graph.Flush();
                        break;
                }
                i++;
            }
            this._actions.Clear();
            //Ensure any Modifiable Graphs we've looked at have been Flushed()
            foreach (ITransactionalGraph g in this._modifiableGraphs.Graphs.OfType<ITransactionalGraph>())
            {
                g.Flush();
            }
            this._modifiableGraphs = new TripleStore();

            this.FlushInternal();
        }

        /// <summary>
        /// Ensures that any changes to the Dataset (if any) are discarded
        /// </summary>
        /// <remarks>
        /// Rollsback the Active Transaction
        /// </remarks>
        public sealed override void Discard()
        {
            int i = this._actions.Count - 1;
            int total = this._actions.Count;
            while (i >= 0)
            {
                GraphPersistenceAction action = this._actions[i];
                switch (action.Action)
                {
                    case GraphPersistenceActionType.Added:
                        //If a Graph was added we must now remove it
                        if (this.HasGraphInternal(action.Graph.BaseUri))
                        {
                            this.RemoveGraphInternal(action.Graph.BaseUri);
                        }
                        break;
                    case GraphPersistenceActionType.Deleted:
                        //If a Graph was deleted we must now add it back again
                        //Don't add the full Graph only an empty Graph with the given URI
                        Graph g = new Graph();
                        g.BaseUri = action.Graph.BaseUri;
                        this.AddGraphInternal(g);
                        break;
                    case GraphPersistenceActionType.Modified:
                        //If a Graph was modified we must discard the changes
                        action.Graph.Discard();
                        break;
                }
                i--;
            }
            if (total == this._actions.Count)
            {
                this._actions.Clear();
            }
            else
            {
                this._actions.RemoveRange(0, total);
            }
            //Ensure any modifiable Graphs we've looked at have been Discarded
            foreach (ITransactionalGraph g in this._modifiableGraphs.Graphs.OfType<ITransactionalGraph>())
            {
                g.Discard();
            }
            this._modifiableGraphs = new TripleStore();

            this.DiscardInternal();
        }

        /// <summary>
        /// Allows the derived dataset to take and post-Flush() actions required
        /// </summary>
        protected virtual void FlushInternal()
        {
            //No actions by default
        }

        /// <summary>
        /// Allows the derived dataset to take any post-Discard() actions required
        /// </summary>
        protected virtual void DiscardInternal()
        {
            //No actions by default
        }
    }
}
