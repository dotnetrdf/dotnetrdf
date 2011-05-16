using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace VDS.RDF.Utilities.StoreManager.Tasks
{
    public class QueryTask : NonCancellableTask<Object>
    {
        private String _query;
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlQuery _q;
        private GenericQueryProcessor _processor;

        public QueryTask(IQueryableGenericIOManager manager, String query)
            : base("SPARQL Query")
        {
            this._processor = new GenericQueryProcessor(manager);
            this._query = query;
        }

        protected override object RunTaskInternal()
        {
            //Firstly try and parse the Query
            this._q = this._parser.ParseFromString(this._query);

            //Then apply it to the Manager using the GenericQueryProcessor
            try
            {
                Object result = this._processor.ProcessQuery(this._q);
                this.Information = "Query Completed OK (Took " + this._q.QueryExecutionTime.Value.ToString() + ")";
                this.RaiseStateChanged();
                return result;
            }
            catch
            {
                this.Information = "Query Failed (Took " + this._q.QueryExecutionTime.Value.ToString() + ")";
                this.RaiseStateChanged();
                throw;
            }
        }

        public override bool IsCancellable
        {
            get 
            {
                return false; 
            }
        }

        public SparqlQuery Query
        {
            get
            {
                return this._q;
            }
        }
    }
}
