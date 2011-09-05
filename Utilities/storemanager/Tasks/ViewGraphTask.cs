using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    class ViewGraphTask 
        : NonCancellableTask<IGraph>
    {
        private IGenericIOManager _manager;
        private String _graphUri;

        public ViewGraphTask(IGenericIOManager manager, String graphUri)
            : base("View Graph")
        {
            this._manager = manager;
            this._graphUri = graphUri;
        }

        protected override IGraph RunTaskInternal()
        {
            if (this._graphUri != null && !this._graphUri.Equals(String.Empty))
            {
                this.Information = "Loading Graph " + this._graphUri.ToString() + "...";
            }
            else
            {
                this.Information = "Loading Default Graph...";
            }

            Graph g = new Graph();
            this._manager.LoadGraph(g, this._graphUri);
            this.Information = "Loaded Graph contains " + g.Triples.Count + " Triple(s)";
            return g;
        }
    }
}
