using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace VDS.RDF.Query
{
    public static class SparqlRemoteEndpointExtensions
    {
        public static IGraph QueryWithResultGraph(this SparqlRemoteEndpoint endpoint, string query)
        {
            IGraph result = null;
            var wait = new AutoResetEvent(false);
            endpoint.QueryWithResultGraph(query, (graph, state) =>
                {
                    result = graph;
                    (state as AutoResetEvent).Set();
                }, wait);
            return result;
        }

        public static void QueryWithResultGraph(this SparqlRemoteEndpoint endpoint, IRdfHandler handler, string query)
        {
            var wait = new AutoResetEvent(false);
            endpoint.QueryWithResultGraph(handler, query,
                                          (rdfHandler, resultsHandler, state) => { (state as AutoResetEvent).Set(); },
                                          wait);
        }

        public static SparqlResultSet QueryWithResultSet(this SparqlRemoteEndpoint endpoint, string query)
        {
            SparqlResultSet resultSet = null;
            var wait = new AutoResetEvent(false);
            endpoint.QueryWithResultSet(query, (results, state) =>
                {
                    resultSet = results;
                    (state as AutoResetEvent).Set();
                }, wait);
            return resultSet;
        }

        public static void QueryWithResultSet(this SparqlRemoteEndpoint endpoint, ISparqlResultsHandler handler,
                                                          string query)
        {
            var wait = new AutoResetEvent(false);
            endpoint.QueryWithResultSet(handler, query, (rdfHandler, resultsHandler, state) =>
                { (state as AutoResetEvent).Set(); }, wait);
        }

    }
}
