using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    class CountTriplesTask : CancellableTask<TaskValueResult<int>>
    {
        private IGenericIOManager _manager;
        private String _graphUri;
        private CancellableHandler _canceller;
        private CountHandler _counter;

        public CountTriplesTask(IGenericIOManager manager, String graphUri)
            : base("Count Triples")
        {
            this._manager = manager;
            this._graphUri = graphUri;
        }

        protected override TaskValueResult<int> RunTaskInternal()
        {
            if (this._graphUri != null && !this._graphUri.Equals(String.Empty))
            {
                this.Information = "Counting Triples for Graph " + this._graphUri.ToString() + "...";
            }
            else
            {
                this.Information = "Counting Triples for Default Graph...";
            }

            this._counter = new CountHandler();
            this._canceller = new CancellableHandler(this._counter);
            this._manager.LoadGraph(this._canceller, this._graphUri);
            this.Information = "Graph contains " + this._counter.Count + " Triple(s)";
            return new TaskValueResult<int>(this._counter.Count);
        }

        protected override void CancelInternal()
        {
            if (this._canceller != null)
            {
                this._canceller.Cancel();
            }
        }
    }
}
