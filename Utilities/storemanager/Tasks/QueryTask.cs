using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    public class QueryTask : NonCancellableTask<Object>
    {
        private IQueryableGenericIOManager _manager;
        private String _query;

        public QueryTask(IQueryableGenericIOManager manager, String query)
            : base("SPARQL Query")
        {
            this._manager = manager;
            this._query = query;
        }

        protected override object RunTaskInternal()
        {
            Object results = this._manager.Query(this._query);
            return results;
        }

        public override bool IsCancellable
        {
            get 
            {
                return false; 
            }
        }
    }
}
