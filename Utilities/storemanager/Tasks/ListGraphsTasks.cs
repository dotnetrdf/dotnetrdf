using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    public class ListGraphsTask 
        : NonCancellableTask<IEnumerable<Uri>>
    {
        private IGenericIOManager _manager;

        public ListGraphsTask(IGenericIOManager manager)
            : base("List Graphs")
        {
            this._manager = manager;
        }

        protected override IEnumerable<Uri> RunTaskInternal()
        {
            if (!this._manager.IsReady)
            {
                this.Information = "Waiting for Store to become ready...";
                this.RaiseStateChanged();
                while (!this._manager.IsReady)
                {
                    Thread.Sleep(250);
                }
            }

            if (this._manager.ListGraphsSupported)
            {
                return this._manager.ListGraphs();
            }
            else if (this._manager is IQueryableGenericIOManager)
            {
                List<Uri> uris = new List<Uri>();
                Object results = ((IQueryableGenericIOManager)this._manager).Query("SELECT DISTINCT ?g WHERE {GRAPH ?g {?s ?p ?o}}");
                if (results is SparqlResultSet)
                {
                    SparqlResultSet rset = (SparqlResultSet)results;
                    foreach (SparqlResult res in rset)
                    {
                        if (res["g"] != null && res["g"].NodeType == NodeType.Uri)
                        {
                            uris.Add(((IUriNode)res["g"]).Uri);
                        }
                    }
                    return uris;
                }
                else
                {
                    throw new RdfStorageException("Store failed to list graphs");
                }
            }
            else
            {
                throw new RdfStorageException("Store does not provide a means to list graphs");
            }
        }
    }
}
