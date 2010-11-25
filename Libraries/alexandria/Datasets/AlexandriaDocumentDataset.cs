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

        public override bool HasGraph(Uri graphUri)
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

        public override IGraph this[Uri graphUri]
        {
            get 
            {
                if (this.HasGraph(graphUri))
                {
                    //TODO: Cache retrieved Graphs in-memory?
                    Graph g = new Graph();
                    this._docManager.LoadGraph(g, graphUri);
                    return g;
                }
                else
                {
                    throw new AlexandriaException("The requested Graph '" + graphUri.ToSafeString() + "' does not exist in this Store");
                }
            }
        }

        public override void Flush()
        {
            base.Flush();
            this._docManager.DocumentManager.Flush();
        }
    }
}
