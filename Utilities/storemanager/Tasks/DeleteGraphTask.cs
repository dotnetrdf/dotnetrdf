using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    class DeleteGraphTask : NonCancellableTask<TaskResult>
    {
        private IGenericIOManager _manager;
        private String _graphUri;

        public DeleteGraphTask(IGenericIOManager manager, String graphUri)
            : base("Delete Graph")
        {
            this._manager = manager;
            this._graphUri = graphUri;
        }

        protected override TaskResult RunTaskInternal()
        {
            if (this._graphUri != null && !this._graphUri.Equals(String.Empty))
            {
                this.Information = "Deleting Graph " + this._graphUri.ToString() + "...";
            }
            else
            {
                this.Information = "Deleting Default Graph...";
            }
            this._manager.DeleteGraph(this._graphUri);
            if (this._graphUri != null && !this._graphUri.Equals(String.Empty))
            {
                this.Information = "Deleted Graph " + this._graphUri.ToString() + " OK";
            }
            else
            {
                this.Information = "Deleted Default Graph OK";
            }

            return new TaskResult(true);
        }
    }
}
