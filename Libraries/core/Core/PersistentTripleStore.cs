using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;

namespace VDS.RDF
{
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
 	        throw new NotImplementedException();
        }

        public void  Discard()
        {
 	        throw new NotImplementedException();
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

        public PersistentGraphCollection(IGenericIOManager manager)
        {
            if (manager == null) throw new ArgumentNullException("manager", "Must use a non-null IGenericIOManager instance with a PersistentGraphCollection");
            this._manager = manager;

            this.TripleAddedHandler = new TripleEventHandler(this.OnTripleAsserted);
            this.TripleRemovedHandler = new TripleEventHandler(this.OnTripleRetracted);

            this.GraphAdded += new GraphEventHandler((sender, args) =>
            {
                this.AttachHandlers(args.Graph);
            });
            this.GraphRemoved += new GraphEventHandler((sender, args) =>
            {
                this.DetachHandlers(args.Graph);
            });
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

        }

        internal void Discard()
        {

        }
    }
}
