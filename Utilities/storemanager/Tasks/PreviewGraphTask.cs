using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    class PreviewGraphTask : CancellableTask<IGraph>
    {
        private IGenericIOManager _manager;
        private String _graphUri;
        private int _previewSize = 100;
        private CancellableHandler _canceller;

        public PreviewGraphTask(IGenericIOManager manager, String graphUri, int previewSize)
            : base("Preview Graph")
        {
            this._manager = manager;
            this._graphUri = graphUri;
            this._previewSize = previewSize;
        }

        protected override IGraph RunTaskInternal()
        {
            if (this._graphUri != null && !this._graphUri.Equals(String.Empty))
            {
                this.Information = "Previewing Graph " + this._graphUri.ToString() + "...";
            }
            else
            {
                this.Information = "Previewing Default Graph...";
            }

            Graph g = new Graph();
            this._canceller = new CancellableHandler(new PagingHandler(new GraphHandler(g), this._previewSize));
            this._manager.LoadGraph(this._canceller, this._graphUri);
            this.Information = "Previewed Graph previews first " + g.Triples.Count + " Triple(s)";
            return g;
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
