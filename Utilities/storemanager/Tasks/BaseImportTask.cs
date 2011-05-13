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

        public BaseImportTask(String name, IGenericIOManager manager)
            : base(name)
        {
            this._manager = manager;
        }

        protected sealed override TaskResult RunTaskInternal()
        {
            if (this._manager.UpdateSupported)
            {
                this.ImportUsingHandler(new WriteToStoreHandler(this._manager));
            }
            else
            {
                IGraph g = this.ImportUsingGraph();
                this._manager.SaveGraph(g);
            }
            return new TaskResult(true);
        }

        protected abstract void ImportUsingHandler(IRdfHandler handler);

        protected abstract IGraph ImportUsingGraph();
    }
}
