using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query;

namespace VDS.RDF.LinkedData.Kasabi.Apis
{
    public class SparqlApi
        : KasabiApi
    {

        public SparqlApi(String datasetID, String authKey)
            : base(datasetID, "sparql", authKey) { }

        public IGraph QueryWithResultGraph(String query)
        {
            Dictionary<String, String> postParams = new Dictionary<string, string>();
            postParams.Add("query", query);
            return this.GetGraphResponse(KasabiClient.EmptyParams, postParams);
        }

        public void QueryWithResultGraph(IRdfHandler handler, String query)
        {
            Dictionary<String, String> postParams = new Dictionary<string, string>();
            postParams.Add("query", query);
            this.GetGraphResponse(KasabiClient.EmptyParams, postParams, handler);
        }

        public SparqlResultSet QueryWithResultSet(String query)
        {
            Dictionary<String, String> postParams = new Dictionary<string, string>();
            postParams.Add("query", query);
            return this.GetSparqlResultsResponse(KasabiClient.EmptyParams, postParams);
        }

        public void QueryWithResultSet(ISparqlResultsHandler handler, String query)
        {
            Dictionary<String, String> postParams = new Dictionary<string, string>();
            postParams.Add("query", query);
            this.GetSparqlResultsResponse(KasabiClient.EmptyParams, postParams, handler);
        }
    }
}
