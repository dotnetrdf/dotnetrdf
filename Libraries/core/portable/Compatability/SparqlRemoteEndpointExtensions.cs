using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using VDS.RDF.Compatability;

namespace VDS.RDF.Query
{
    public static class SparqlRemoteEndpointExtensions
    {
        public static IGraph QueryWithResultGraph(this SparqlRemoteEndpoint endpoint, string query)
        {
            IGraph result = null;
            var wait = new AsyncOperationState();
            endpoint.QueryWithResultGraph(query, (graph, state) =>
                {
                    result = graph;
                    (state as AsyncOperationState).OperationCompleted();
                }, wait);
            wait.WaitForCompletion();
            return result;
        }

        public static void QueryWithResultGraph(this SparqlRemoteEndpoint endpoint, IRdfHandler handler, string query)
        {
            var wait = new AsyncOperationState();
            endpoint.QueryWithResultGraph(handler, query,
                                          (rdfHandler, resultsHandler, state) =>
                                              {
                                                  (state as AsyncOperationState).OperationCompleted();
                                              },
                                          wait);
            wait.WaitForCompletion();
        }

        public static SparqlResultSet QueryWithResultSet(this SparqlRemoteEndpoint endpoint, string query)
        {
            SparqlResultSet resultSet = null;
            var wait = new AsyncOperationState();
            endpoint.QueryWithResultSet(query, (results, state) =>
                {
                    resultSet = results;
                    (state as AsyncOperationState).OperationCompleted();
                }, wait);
            wait.WaitForCompletion();
            return resultSet;
        }

        public static void QueryWithResultSet(this SparqlRemoteEndpoint endpoint, ISparqlResultsHandler handler,
                                                          string query)
        {
            var wait = new AsyncOperationState();
            endpoint.QueryWithResultSet(handler, query, (rdfHandler, resultsHandler, state) =>
                { (state as AsyncOperationState).OperationCompleted(); }, wait);
            wait.WaitForCompletion();
        }

    }
}
