using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    public abstract class BaseImportTask : CancellableTask<TaskResult>
    {
        private IGenericIOManager _manager;
        private CancellableHandler _canceller;

        public BaseImportTask(String name, IGenericIOManager manager)
            : base(name)
        {
            this._manager = manager;
        }

        protected sealed override TaskResult RunTaskInternal()
        {
            if (this._manager.UpdateSupported)
            {
                this._canceller = new CancellableHandler(new WriteToStoreHandler(this._manager));
                if (this.HasBeenCancelled) this._canceller.Cancel();
                this.ImportUsingHandler(this._canceller);
            }
            else
            {
                Graph g = new Graph();
                GraphHandler h = new GraphHandler(g);
                this._canceller = new CancellableHandler(h);
                if (this.HasBeenCancelled) this._canceller.Cancel();
                this.ImportUsingHandler(this._canceller);
                this._manager.SaveGraph(g);
            }
            return new TaskResult(true);
        }

        protected abstract void ImportUsingHandler(IRdfHandler handler);

        protected override void CancelInternal()
        {
            if (this._canceller != null)
            {
                this._canceller.Cancel();
            }
        }
    }
}
