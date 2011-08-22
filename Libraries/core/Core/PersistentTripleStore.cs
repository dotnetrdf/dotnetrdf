#if !NO_STORAGE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;

namespace VDS.RDF
{
    /// <summary>
    /// Represents an in-memory view of a triple store provided by an <see cref="IGenericIOManager">IGenericIOManager</see> instance where changes to the in-memory view get reflected in the persisted view.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Note:</strong> This is a transactional implementation - this means that changes made are not persisted until you either call <see cref="PersistentTripleStore.Flush()">Flush()</see> or you dispose of the instance.
    /// </para>
    /// <para>
    /// The actual level of persistence provided will vary according to the <see cref="IGenericIOManager">IGenericIOManager</see> instance you use.  For example if the <see cref="IGenericIOManager.DeleteGraph()">DeleteGraph()</see> method is not supported then Graph removals won't persist in the underlying store.  Similarily an instance which is read-only will allow you to pull out existing graphs from the store but won't persist any changes.
    /// </para>
    /// <para>
    /// The Contains() method of the underlying <see cref="BaseGraphCollection">BaseGraphCollection</see> has been overridden so that invoking Contains causes the Graph from the underlying store to be loaded if it exists, this means that operations like <see cref="PersistentTripleStore.HasGraph()">HasGraph()</see> may be slower than expected or cause applications to stop while they wait to load data from the store.
    /// </para>
    /// </remarks>
    public sealed class PersistentTripleStore
        : BaseTripleStore, INativelyQueryableStore, IUpdateableTripleStore, ITransactionalStore
    {
        private IGenericIOManager _manager;

        public PersistentTripleStore(IGenericIOManager manager)
            : base(new PersistentGraphCollection(manager))
        {
            this._manager = manager;
        }

        ~PersistentTripleStore()
        {
            this.Dispose(false);
        }

        public override void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);
            this.Flush();
        }

        public void  Flush()
        {
            ((PersistentGraphCollection)this._graphs).Flush();
        }

        public void  Discard()
        {
            ((PersistentGraphCollection)this._graphs).Discard();   
        }

        #region INativelyQueryableStore Members

        public object ExecuteQuery(string query)
        {
            throw new NotImplementedException();
        }

        public void ExecuteQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string query)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IUpdateableTripleStore Members

        public void ExecuteUpdate(string update)
        {
            throw new NotImplementedException();
        }

        public void ExecuteUpdate(VDS.RDF.Update.SparqlUpdateCommand update)
        {
            throw new NotImplementedException();
        }

        public void ExecuteUpdate(VDS.RDF.Update.SparqlUpdateCommandSet updates)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public sealed class PersistentGraphCollection
        : GraphCollection
    {
        private IGenericIOManager _manager;
        private TripleEventHandler TripleAddedHandler, TripleRemovedHandler;
        private List<TriplePersistenceAction> _actions = new List<TriplePersistenceAction>();
        private List<GraphPersistenceAction> _graphActions = new List<GraphPersistenceAction>();

        public PersistentGraphCollection(IGenericIOManager manager)
        {
            if (manager == null) throw new ArgumentNullException("manager", "Must use a non-null IGenericIOManager instance with a PersistentGraphCollection");
            this._manager = manager;

            this.TripleAddedHandler = new TripleEventHandler(this.OnTripleAsserted);
            this.TripleRemovedHandler = new TripleEventHandler(this.OnTripleRetracted);

            ////If the Manager supports Triple Level updates we'll watch all updates to the 
            ////Graph and Flush()/Discard() them when appropriate
            ////Note that for Managers that don't support this we just watch graph level
            ////actions instead and
            //if (this._manager.UpdateSupported)
            //{
            //    this.GraphAdded += new GraphEventHandler((sender, args) =>
            //    {
            //        this.AttachHandlers(args.Graph);
            //        if (!this.ContainsInternal(args.Graph.BaseUri))
            //        {
            //            //When a new graph is introduced that does not exist in the underlying store
            //            //be sure to persist the initial triples
            //            foreach (Triple t in args.Graph.Triples)
            //            {
            //                this._actions.Add(new TriplePersistenceAction(t));
            //            }
            //        }
            //    });
            //    this.GraphRemoved += new GraphEventHandler((sender, args) =>
            //    {
            //        this.DetachHandlers(args.Graph);
            //    });
            //}
            //else
            //{
            //    this.GraphAdded += new GraphEventHandler((sender, args) =>
            //        {
            //            this._graphActions.Add(new GraphPersistenceAction(args.Graph, GraphPersistenceActionType.Added));
            //        });
            //    this.GraphRemoved += new GraphEventHandler((sender, args) =>
            //        {
            //            this._graphActions.Add(new GraphPersistenceAction(args.Graph, GraphPersistenceActionType.Deleted));
            //        });
            //}
        }

        protected override void RaiseGraphAdded(IGraph g)
        {
            if (this._manager.UpdateSupported)
            {
                this.AttachHandlers(g);
                if (!this.ContainsInternal(g.BaseUri))
                {
                    //When a new graph is introduced that does not exist in the underlying store
                    //be sure to persist the initial triples
                    foreach (Triple t in g.Triples)
                    {
                        this._actions.Add(new TriplePersistenceAction(t));
                    }
                }
            }
            else
            {
                this._graphActions.Add(new GraphPersistenceAction(g, GraphPersistenceActionType.Added));
            }
            base.RaiseGraphAdded(g);
        }

        protected override void RaiseGraphRemoved(IGraph g)
        {
            if (this._manager.UpdateSupported)
            {
                this.DetachHandlers(g);
            }
            else
            {
                this._graphActions.Add(new GraphPersistenceAction(g, GraphPersistenceActionType.Deleted));
            }
            base.RaiseGraphRemoved(g);
        }

        public override bool Contains(Uri graphUri)
        {
            if (base.Contains(graphUri))
            {
                return true;
            }
            else
            {
                //Try and load the Graph and return true if anything is returned
                Graph g = new Graph();
                this._manager.LoadGraph(g, graphUri);
                if (g.Triples.Count > 0)
                {
                    //If we're going to return true we must also store the Graph in the collection
                    //for later use
                    this.Add(g, true);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override IEnumerable<Uri> GraphUris
        {
            get
            {
                if (this._manager.ListGraphsSupported)
                {
                    return this._manager.ListGraphs().Concat(base.GraphUris).Distinct();
                }
                else
                {
                    return base.GraphUris;
                }
            }
        }

        public override IGraph this[Uri graphUri]
        {
            get
            {
                if (this.Contains(graphUri))
                {
                    return base[graphUri];
                }
                else
                {
                    throw new RdfException("The Graph with the given URI does not exist in the Graph Collection");
                }
            }
        }

        private bool ContainsInternal(Uri graphUri)
        {
            AnyHandler handler = new AnyHandler();
            this._manager.LoadGraph(handler, graphUri);
            return handler.Any;
        }

        private void AttachHandlers(IGraph g)
        {
            g.TripleAsserted += this.TripleAddedHandler;
            g.TripleRetracted += this.TripleRemovedHandler;
        }

        private void DetachHandlers(IGraph g)
        {
            g.TripleAsserted -= this.TripleAddedHandler;
            g.TripleRetracted -= this.TripleRemovedHandler;
        }

        private void OnTripleAsserted(Object sender, TripleEventArgs args)
        {
            this._actions.Add(new TriplePersistenceAction(args.Triple));
        }

        private void OnTripleRetracted(Object sender, TripleEventArgs args)
        {
            this._actions.Add(new TriplePersistenceAction(args.Triple, true));
        }

        internal void Flush()
        {
            //Read-Only managers have no persistence
            if (this._manager.IsReadOnly) return;

            if (this._manager.UpdateSupported)
            {
                if (this._actions.Count == 0) return;

                //Persist based on Triple level actions
                //First group Triple together based on Graph URI
                while (this._actions.Count > 0)
                {
                    Queue<TriplePersistenceAction> actions = new Queue<TriplePersistenceAction>();
                    Uri currUri = this._actions[0].Triple.GraphUri;
                    actions.Enqueue(this._actions[0]);
                    this._actions.RemoveAt(0);

                    for (int i = 0; i < this._actions.Count; i++)
                    {
                        if (EqualityHelper.AreUrisEqual(currUri, this._actions[i].Triple.GraphUri))
                        {
                            actions.Enqueue(this._actions[i]);
                            this._actions.RemoveAt(i);
                            i--;
                        }
                    }

                    //Split the Triples for this Graph into batches of adds and deletes to ensure
                    //accurate persistence of the actions
                    bool toDelete = false;
                    List<Triple> batch = new List<Triple>();
                    while (actions.Count > 0)
                    {
                        TriplePersistenceAction next = actions.Dequeue();
                        if (next.IsDelete != toDelete)
                        {
                            if (batch.Count > 0)
                            {
                                //Process a batch whenever we find a switch between additions and removals
                                //This ensures that regardless of the logic in UpdateGraph() we force
                                //additions and removals to happen in the order we care about
                                if (toDelete)
                                {
                                    this._manager.UpdateGraph(currUri, null, batch);
                                }
                                else
                                {
                                    this._manager.UpdateGraph(currUri, batch, null);
                                }
                                batch.Clear();
                            }
                            toDelete = next.IsDelete;
                        }
                        batch.Add(next.Triple);
                    }
                    //Ensure the final batch (if any) gets processed
                    if (batch.Count > 0)
                    {
                        if (toDelete)
                        {
                            this._manager.UpdateGraph(currUri, null, batch);
                        }
                        else
                        {
                            this._manager.UpdateGraph(currUri, batch, null);
                        }
                    }
                }
            }
            else
            {
                if (this._graphActions.Count == 0) return;

                //Persist based on Graph level actions
                foreach (GraphPersistenceAction action in this._graphActions)
                {
                    if (action.Action == GraphPersistenceActionType.Added)
                    {
                        this._manager.SaveGraph(action.Graph);
                    }
                    else if (action.Action == GraphPersistenceActionType.Deleted && this._manager.DeleteSupported)
                    {
                        //Can only delete graphs if deletion is supported
                        this._manager.DeleteGraph(action.Graph.BaseUri);
                    }
                }
            }
        }

        internal void Discard()
        {
            //Read-Only managers have no persistence
            if (this._manager.IsReadOnly) return;

            if (this._manager.UpdateSupported)
            {
                if (this._actions.Count == 0) return;

                //Persist based on Triple level actions
                //First group Triple together based on Graph URI

                //Important - For discard we reverse the list of actions 
                this._actions.Reverse();

                while (this._actions.Count > 0)
                {
                    Queue<TriplePersistenceAction> actions = new Queue<TriplePersistenceAction>();
                    Uri currUri = this._actions[0].Triple.GraphUri;
                    actions.Enqueue(this._actions[0]);
                    this._actions.RemoveAt(0);

                    for (int i = 0; i < this._actions.Count; i++)
                    {
                        if (EqualityHelper.AreUrisEqual(currUri, this._actions[i].Triple.GraphUri))
                        {
                            actions.Enqueue(this._actions[i]);
                            this._actions.RemoveAt(i);
                            i--;
                        }
                    }

                    //Split the Triples for this Graph into batches of adds and deletes to ensure
                    //accurate persistence of the actions
                    bool toDelete = false;
                    List<Triple> batch = new List<Triple>();
                    while (actions.Count > 0)
                    {
                        TriplePersistenceAction next = actions.Dequeue();
                        if (next.IsDelete != toDelete)
                        {
                            if (batch.Count > 0)
                            {
                                //Process a batch whenever we find a switch between additions and removals
                                //This ensures that regardless of the logic in UpdateGraph() we force
                                //additions and removals to happen in the order we care about

                                //Important - For discard we flip the actions in order to reverse them
                                //i.e. additions become removals and vice versa
                                if (toDelete)
                                {
                                    this._manager.UpdateGraph(currUri, batch, null);
                                }
                                else
                                {
                                    this._manager.UpdateGraph(currUri, null, batch);
                                }
                                batch.Clear();
                            }
                            toDelete = next.IsDelete;
                        }
                        batch.Add(next.Triple);
                    }
                    //Ensure the final batch (if any) gets processed
                    if (batch.Count > 0)
                    {
                        //Important - For discard we flip the actions in order to reverse them
                        //i.e. additions become removals and vice versa
                        if (toDelete)
                        {
                            this._manager.UpdateGraph(currUri, batch, null);
                        }
                        else
                        {
                            this._manager.UpdateGraph(currUri, null, batch);
                        }
                    }
                }
            }
            else
            {
                if (this._graphActions.Count == 0) return;

                //Persist based on Graph level actions

                //Important - For discard we reverse the list of actions 
                this._graphActions.Reverse();

                foreach (GraphPersistenceAction action in this._graphActions)
                {
                    //Important - For discard we flip the actions in order to reverse them
                    //i.e. additions become removals and vice versa

                    if (action.Action == GraphPersistenceActionType.Added && this._manager.DeleteSupported)
                    {
                        //Can only delete graphs if deletion is supported
                        this._manager.DeleteGraph(action.Graph.BaseUri);
                    }
                    else if (action.Action == GraphPersistenceActionType.Deleted)
                    {
                        this._manager.SaveGraph(action.Graph);
                    }
                }
            }
        }
    }
}

#endif