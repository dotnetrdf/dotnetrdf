#if !NO_STORAGE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Update;

namespace VDS.RDF
{
    /// <summary>
    /// Represents an in-memory view of a triple store provided by an <see cref="IGenericIOManager">IGenericIOManager</see> instance where changes to the in-memory view get reflected in the persisted view.
    /// </summary>
    /// <remarks>
    /// <h3>Persistence Behaviour</h3>
    /// <para>
    /// <strong>Note:</strong> This is a transactional implementation - this means that changes made are not persisted until you either call <see cref="PersistentTripleStore.Flush()">Flush()</see> or you dispose of the instance.  Alternatively you may invoke the <see cref="PersistentTripleStore.Discard()">Discard()</see> method to throw away changes made to the in-memory state.
    /// </para>
    /// <para>
    /// The actual level of persistence provided will vary according to the <see cref="IGenericIOManager">IGenericIOManager</see> instance you use.  For example if the <see cref="IGenericIOManager.DeleteGraph()">DeleteGraph()</see> method is not supported then Graph removals won't persist in the underlying store.  Similarily an instance which is read-only will allow you to pull out existing graphs from the store but won't persist any changes.
    /// </para>
    /// <para>
    /// The Contains() method of the underlying <see cref="BaseGraphCollection">BaseGraphCollection</see> has been overridden so that invoking Contains causes the Graph from the underlying store to be loaded if it exists, this means that operations like <see cref="PersistentTripleStore.HasGraph()">HasGraph()</see> may be slower than expected or cause applications to stop while they wait to load data from the store.
    /// </para>
    /// <h3>SPARQL Query Behaviour</h3>
    /// <para>
    /// The exact SPARQL Query behaviour will depend on the capabilities of the underlying <see cref="IGenericIOManager">IGenericIOManager</see> instance.  If it also implements the <see cref="IQueryableGenericIOManager">IQueryableGenericIOManager</see> interface then its own SPARQL implementation will be used, note that if you try and make a SPARQL query but the in-memory view has not been synced (via a <see cref="PersistentTripleStore.Flush()">Flush()</see> or <see cref="PersistentTripleStore.Discard()">Discard()</see> call) prior to the query then an <see cref="RdfQueryException">RdfQueryException</see> will be thrown.  If you want to make the query regardless you can do so by invoking the query method on the underlying store directly by accessing it via the <see cref="PersistentTripleStore.UnderlyingStore">UnderlyingStore</see> property.
    /// </para>
    /// <para>
    /// If the underlying store does not support SPARQL itself then SPARQL queries cannot be applied and a <see cref="NotSupportedException">NotSupportedException</see> will be thrown.
    /// </para>
    /// <h3>SPARQL Update Behaviour</h3>
    /// <para>
    /// Similarly to SPARQL Query support the SPARQL Update behaviour depends on whether the underlying <see cref="IGenericIOManager">IGenericIOManager</see> instance also implements the <see cref="IUpdateableGenericIOManager">IUpdateableGenericIOManager</see> interface.  If it does then its own SPARQL implementation is used, otherwise a <see cref="GenericUpdateProcessor">GenericUpdateProcessor</see> will be used to approximate the SPARQL Update.
    /// </para>
    /// <para>
    /// Please be aware that as with SPARQL Query if the in-memory view is not synced with the underlying store a <see cref="SparqlUpdateException">SparqlUpdateException</see> will be thrown.
    /// </para>
    /// <h3>Other Notes</h3>
    /// <para>
    /// It is possible for the in-memory view of the triple store to get out of sync with the underlying store if that store is being modified by other processes or other code not utilising the <see cref="PersistentTripleStore">PersistentTripleStore</see> instance that you have created.  Currently there is no means to resync the in-memory view with the underlying view so you should be careful of using this class in scenarios where your underlying store may be modified.
    /// </para>
    /// </remarks>
    public sealed class PersistentTripleStore
        : BaseTripleStore, INativelyQueryableStore, IUpdateableTripleStore, ITransactionalStore
    {
        private IGenericIOManager _manager;
        private SparqlUpdateParser _updateParser;
        private GenericUpdateProcessor _updateProcessor;

        /// <summary>
        /// Creates a new in-memory view of some underlying store represented by the <see cref="IGenericIOManager">IGenericIOManager</see> instance
        /// </summary>
        /// <param name="manager">IO Manager</param>
        /// <remarks>
        /// Please see the remarks for this class for notes on exact behaviour of this class
        /// </remarks>
        public PersistentTripleStore(IGenericIOManager manager)
            : base(new PersistentGraphCollection(manager))
        {
            this._manager = manager;
        }

        /// <summary>
        /// Finalizer which ensures that the instance is properly disposed of thereby persisting any outstanding changes to the underlying store
        /// </summary>
        /// <remarks>
        /// If you do not wish to persist your changes you must call <see cref="PersistentTripleStore.Discard()">Discard()</see> prior to disposing of this instance or allowing it to go out of scope such that the finalizer gets called
        /// </remarks>
        ~PersistentTripleStore()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the underlying store
        /// </summary>
        public IGenericIOManager UnderlyingStore
        {
            get
            {
                return _manager;
            }
        }

        /// <summary>
        /// Disposes of the Triple Store flushing any outstanding changes to the underlying store
        /// </summary>
        /// <remarks>
        /// If you do not want to persist changes you have please ensure you call <see cref="PersistentTripleStore.Discard()">Discard()</see> prior to disposing of the instance
        /// </remarks>
        public override void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);
            this.Flush();
        }

        /// <summary>
        /// Flushes any outstanding changes to the underlying store
        /// </summary>
        public void  Flush()
        {
            if (this._graphs != null)
            {
                ((PersistentGraphCollection)this._graphs).Flush();
            }
        }

        /// <summary>
        /// Discards any outstanding changes returning the in-memory view of the store to the state it was in after the last Flush/Discard operation
        /// </summary>
        public void  Discard()
        {
            if (this._graphs != null)
            {
                ((PersistentGraphCollection)this._graphs).Discard();
            }
        }

        #region INativelyQueryableStore Members

        /// <summary>
        /// Executes a SPARQL Query on the Triple Store
        /// </summary>
        /// <param name="query">Sparql Query as unparsed String</param>
        /// <returns></returns>
        public object ExecuteQuery(string query)
        {
            Graph g = new Graph();
            SparqlResultSet results = new SparqlResultSet();
            this.ExecuteQuery(new GraphHandler(g), new ResultSetHandler(results), query);
            if (results.ResultsType != SparqlResultsType.Unknown)
            {
                return results;
            }
            else
            {
                return g;
            }
        }

        /// <summary>
        /// Executes a SPARQL Query on the Triple Store processing the results using an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="query">SPARQL Query as unparsed String</param>
        public void ExecuteQuery(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string query)
        {
            if (this._manager is IQueryableGenericIOManager)
            {
                if (!((PersistentGraphCollection)this._graphs).IsSynced)
                {
                    throw new RdfQueryException("Unable to execute a SPARQL Query as the in-memory view of the store is not synced with the underlying store, please invoked Flush() or Discard() and try again.  Alternatively if you do not want to see in-memory changes reflected in query results you can invoke the Query() method directly on the underlying store by accessing it through the UnderlyingStore property.");
                }

                ((IQueryableGenericIOManager)this._manager).Query(rdfHandler, resultsHandler, query);
            }
            else
            {
                throw new NotSupportedException("SPARQL Query is not supported as the underlying store does not support it");
            }
        }

        #endregion

        #region IUpdateableTripleStore Members

        /// <summary>
        /// Executes an Update against the Triple Store
        /// </summary>
        /// <param name="update">SPARQL Update Command(s)</param>
        /// <remarks>
        /// As per the SPARQL 1.1 Update specification the command string may be a sequence of commands
        /// </remarks>
        public void ExecuteUpdate(string update)
        {
            if (this._manager is IUpdateableGenericIOManager)
            {
                if (!((PersistentGraphCollection)this._graphs).IsSynced)
                {
                    throw new SparqlUpdateException("Unable to execute a SPARQL Update as the in-memory view of the store is not synced with the underlying store, please invoked Flush() or Discard() and try again.  Alternatively if you do not want to see in-memory changes reflected in update results you can invoke the Update() method directly on the underlying store by accessing it through the UnderlyingStore property.");
                }

                ((IUpdateableGenericIOManager)this._manager).Update(update);
            }
            else
            {
                if (this._updateProcessor == null) this._updateProcessor = new GenericUpdateProcessor(this._manager);
                if (this._updateParser == null) this._updateParser = new SparqlUpdateParser();
                SparqlUpdateCommandSet cmds = this._updateParser.ParseFromString(update);
                this._updateProcessor.ProcessCommandSet(cmds);
            }
        }

        /// <summary>
        /// Executes a single Update Command against the Triple Store
        /// </summary>
        /// <param name="update">SPARQL Update Command</param>
        public void ExecuteUpdate(SparqlUpdateCommand update)
        {
            this.ExecuteUpdate(update.ToString());
        }

        /// <summary>
        /// Executes a set of Update Commands against the Triple Store
        /// </summary>
        /// <param name="updates">SPARQL Update Command Set</param>
        public void ExecuteUpdate(SparqlUpdateCommandSet updates)
        {
            this.ExecuteUpdate(updates.ToString());
        }

        #endregion
    }

    /// <summary>
    /// Internal implementation of a Graph Collection for use by the <see cref="PersistentTripleStore">PersistentTripleStore</see>
    /// </summary>
    class PersistentGraphCollection
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
                    if (this._removedGraphs.Contains(g.BaseUri.ToSafeString()) || !this.ContainsInternal(g.BaseUri))
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
                try
                {
                    this._manager.LoadGraph(g, graphUri);
                    if (g.Triples.Count > 0)
                    {
                        //If we're going to return true we must also store the Graph in the collection
                        //for later use
                        g.BaseUri = graphUri;
                        this.Add(g, true);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    //If trying to load the Graph errors then it doesn't exist so return false
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        protected internal override void Remove(Uri graphUri)
        {
            if (this.Contains(graphUri))
            {
                base.Remove(graphUri);
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
            try
            {
                this._manager.LoadGraph(handler, graphUri);
                return handler.Any;
            }
            catch
            {
                return false;
            }
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

        internal bool IsSynced
        {
            get
            {
                return this._actions.Count == 0;
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