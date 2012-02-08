using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.Alexandria.Documents;

namespace VDS.Alexandria.Datasets
{
    public abstract class AlexandriaDocumentDataset<TReader,TWriter> : BaseAlexandriaDataset
    {
        private AlexandriaDocumentStoreManager<TReader, TWriter> _docManager;

        public AlexandriaDocumentDataset(AlexandriaDocumentStoreManager<TReader, TWriter> manager)
            : base(manager)
        {
            this._docManager = manager;
        }

        protected sealed override bool HasGraphInternal(Uri graphUri)
        {
            return this._docManager.DocumentManager.HasDocument(this._docManager.DocumentManager.GraphRegistry.GetDocumentName(graphUri.ToSafeString()));
        }

        public override IEnumerable<IGraph> Graphs
        {
            get 
            {
                return (from uri in this._docManager.DocumentManager.GraphRegistry.GraphUris
                        select this[new Uri(uri)]);
            }
        }

        public override IEnumerable<Uri> GraphUris
        {
            get 
            {
                return this._docManager.DocumentManager.GraphRegistry.GraphUris.Select(u => (u == null || u.Equals(String.Empty)) ? null : new Uri(u)); 
            }
        }

        protected override IGraph GetGraphInternal(Uri graphUri)
        {
            //TODO: Cache retrieved Graphs in-memory?
            Graph g = new Graph();
            this._docManager.LoadGraph(g, graphUri);
            return g;
        }

        protected override void FlushInternal()
        {
            base.FlushInternal();
            this._docManager.DocumentManager.Flush();
        }
    }
}
