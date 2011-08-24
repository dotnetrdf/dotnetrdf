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
        private List<TripleStorePersistenceAction> _actions = new List<TripleStorePersistenceAction>();
        private HashSet<String> _removedGraphs = new HashSet<string>();
        private bool _persisting = false;

        public PersistentGraphCollection(IGenericIOManager manager)
        {
            if (manager == null) throw new ArgumentNullException("manager", "Must use a non-null IGenericIOManager instance with a PersistentGraphCollection");
            this._manager = manager;

            this.TripleAddedHandler = new TripleEventHandler(this.OnTripleAsserted);
            this.TripleRemovedHandler = new TripleEventHandler(this.OnTripleRetracted);
        }

        protected override void RaiseGraphAdded(IGraph g)
        {
            if (!this._persisting)
            {
                if (this._manager.UpdateSupported)
                {
                    this.AttachHandlers(g);
                    if (!this.ContainsInternal(g.BaseUri))
                    {
                        //When a new graph is introduced that does not exist in the underlying store
                        //be sure to persist the initial triples
                        this._actions.Add(new TripleStorePersistenceAction(new GraphPersistenceAction(g, GraphPersistenceActionType.Added)));
                        foreach (Triple t in g.Triples)
                        {
                            this._actions.Add(new TripleStorePersistenceAction(new TriplePersistenceAction(t)));
                        }
                    }
                }
                else
                {
                    this._actions.Add(new TripleStorePersistenceAction(new GraphPersistenceAction(g, GraphPersistenceActionType.Added)));
                }
            }
            base.RaiseGraphAdded(g);
        }

        protected override void RaiseGraphRemoved(IGraph g)
        {
            if (!this._persisting)
            {
                this._removedGraphs.Add(g.BaseUri.ToSafeString());
                if (this._manager.UpdateSupported)
                {
                    this.DetachHandlers(g);
                    this._actions.Add(new TripleStorePersistenceAction(new GraphPersistenceAction(g, GraphPersistenceActionType.Deleted)));
                }
                else
                {
                    this._actions.Add(new TripleStorePersistenceAction(new GraphPersistenceAction(g, GraphPersistenceActionType.Deleted)));
                }
            }
            base.RaiseGraphRemoved(g);
        }

        public override bool Contains(Uri graphUri)
        {
            if (base.Contains(graphUri))
            {
                return true;
            }
            else if (!this._removedGraphs.Contains(graphUri.ToSafeString()))
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
            else
            {
                return false;
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
            if (!this._persisting)
            {
                this._actions.Add(new TripleStorePersistenceAction(new TriplePersistenceAction(args.Triple)));
            }
        }

        private void OnTripleRetracted(Object sender, TripleEventArgs args)
        {
            if (!this._persisting)
            {
                this._actions.Add(new TripleStorePersistenceAction(new TriplePersistenceAction(args.Triple, true)));
            }
        }

        internal void Flush()
        {
            try
            {
                this._persisting = true;
                this._removedGraphs.Clear();

                //Read-Only managers have no persistence
                if (this._manager.IsReadOnly) return;

                //No actions means no persistence necessary
                if (this._actions.Count == 0) return;

                if (this._manager.UpdateSupported)
                {
                    //Persist based on Triple level actions
                    //First group Triple together based on Graph URI
                    while (this._actions.Count > 0)
                    {
                        TripleStorePersistenceAction action = this._actions[0];

                        if (action.IsTripleAction)
                        {
                            Queue<TriplePersistenceAction> actions = new Queue<TriplePersistenceAction>();
                            Uri currUri = action.TripleAction.Triple.GraphUri;
                            actions.Enqueue(this._actions[0].TripleAction);
                            this._actions.RemoveAt(0);

                            //Find all the Triple actions related to this Graph up to the next non-Triple action
                            for (int i = 0; i < this._actions.Count && this._actions[i].IsTripleAction; i++)
                            {
                                if (EqualityHelper.AreUrisEqual(currUri, this._actions[i].TripleAction.Triple.GraphUri))
                                {
                                    actions.Enqueue(this._actions[i].TripleAction);
                                    this._actions.RemoveAt(i);
                                    i--;
                                }
                            }

                            //Split the Triple Actions for this Graph into batches of adds and deletes to ensure
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
                        else
                        {
                            switch (action.GraphAction.Action)
                            {
                                case GraphPersistenceActionType.Added:
                                    //No need to do anything in-memory as will be in the graph collection
                                    //Call SaveGraph() with an empty graph to create the relevant graph
                                    //If Triples were added these will be persisted separately with
                                    //TriplePersistenceActions
                                    Graph g = new Graph();
                                    g.BaseUri = action.GraphAction.Graph.BaseUri;
                                    this._manager.SaveGraph(g);
                                    break;

                                case GraphPersistenceActionType.Deleted:
                                    //No need to do anything in-memory as won't be in the graph collection
                                    //If DeleteGraph() is supported call it to delete the relevant graph
                                    if (this._manager.DeleteSupported)
                                    {
                                        this._manager.DeleteGraph(action.GraphAction.Graph.BaseUri);
                                    }
                                    break;
                            }
                            this._actions.RemoveAt(0);
                        }
                    }
                }
                else
                {
                    //Persist based on Graph level actions
                    foreach (TripleStorePersistenceAction action in this._actions)
                    {
                        if (action.IsGraphAction)
                        {
                            if (action.GraphAction.Action == GraphPersistenceActionType.Added)
                            {
                                this._manager.SaveGraph(action.GraphAction.Graph);
                            }
                            else if (action.GraphAction.Action == GraphPersistenceActionType.Deleted && this._manager.DeleteSupported)
                            {
                                //Can only delete graphs if deletion is supported
                                this._manager.DeleteGraph(action.GraphAction.Graph.BaseUri);
                            }
                        }
                    }
                }
            }
            finally
            {
                this._persisting = false;
            }
        }

        internal void Discard()
        {
            try 
            {
                this._persisting = true;
                this._removedGraphs.Clear();

                //Read-Only managers have no persistence
                if (this._manager.IsReadOnly) return;

                //No actions mean no persistence necessary
                if (this._actions.Count == 0) return;

                //Important - For discard we reverse the list of actions so that we
                //rollback the actions in appropriate order
                this._actions.Reverse();

                if (this._manager.UpdateSupported)
                {
                    //Persist based on Triple level actions
                    //First group Triple together based on Graph URI
                    while (this._actions.Count > 0)
                    {
                        TripleStorePersistenceAction action = this._actions[0];

                        if (action.IsTripleAction)
                        {
                            Queue<TriplePersistenceAction> actions = new Queue<TriplePersistenceAction>();
                            Uri currUri = this._actions[0].TripleAction.Triple.GraphUri;
                            actions.Enqueue(this._actions[0].TripleAction);
                            this._actions.RemoveAt(0);

                            //Find all the Triple actions related to this Graph up to the next non-Triple action
                            for (int i = 0; i < this._actions.Count && this._actions[i].IsTripleAction; i++)
                            {
                                if (EqualityHelper.AreUrisEqual(currUri, this._actions[i].TripleAction.Triple.GraphUri))
                                {
                                    actions.Enqueue(this._actions[i].TripleAction);
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
                                        //Also for discard we only need to alter the in-memory state not actually
                                        //do any persistence since the actions will never have been persisted
                                        if (toDelete)
                                        {
                                            this[currUri].Assert(batch);
                                        }
                                        else
                                        {
                                            this[currUri].Retract(batch);
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
                                //Also for discard we only need to alter the in-memory state not actually
                                //do any persistence since the actions will never have been persisted
                                if (toDelete)
                                {
                                    this[currUri].Assert(batch);
                                }
                                else
                                {
                                    this[currUri].Retract(batch);
                                }
                            }
                        }
                        else
                        {
                            switch (action.GraphAction.Action)
                            {
                                case GraphPersistenceActionType.Added:
                                    //Need to remove from being in-memory
                                    this.Remove(action.GraphAction.Graph.BaseUri);
                                    break;

                                case GraphPersistenceActionType.Deleted:
                                    //Need to add back into memory
                                    this.Add(action.GraphAction.Graph, false);
                                    break;
                            }
                            this._actions.RemoveAt(0);
                        }
                    }
                }
                else
                {
                    //Persist based on Graph level actions
                    foreach (TripleStorePersistenceAction action in this._actions)
                    {
                        //Important - For discard we flip the actions in order to reverse them
                        //i.e. additions become removals and vice versa

                        if (action.IsGraphAction)
                        {
                            if (action.GraphAction.Action == GraphPersistenceActionType.Added)
                            {
                                this.Remove(action.GraphAction.Graph.BaseUri);
                            }
                            else if (action.GraphAction.Action == GraphPersistenceActionType.Deleted)
                            {
                                this.Add(action.GraphAction.Graph, false);
                            }
                        }
                    }
                }
            } 
            finally 
            {
                this._persisting = false;
            }
        }
    }
}

#endif