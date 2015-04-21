using System;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Storage;

namespace VDS.RDF.Query.Spin.Core.Runtime
{
    // TODO refactor this into a StorageMonitor or something ?
    /// <summary>
    /// The monitorLog class is responsible to log and track transactional events in a centralized distributed graph on a specific storage
    /// </summary>
    internal class StorageRuntimeMonitor
    {
        /// <summary>
        /// The distributed monitorLog's graph Uri
        /// </summary>
        internal static Uri RUNTIME_MONITOR_GRAPH_URI = UriFactory.Create(RuntimeHelper.BASE_URI + ":storageMonitorLog");

        // TODO ? make this threaded while the connection's State is either Fetching or Executing ?
        internal static void Ping(Connection connection)
        {
            if (!(connection.UnderlyingStorage is IUpdateableStorage)) return;
            IUpdateableStorage storage = (IUpdateableStorage)connection.UnderlyingStorage;
            SparqlParameterizedString pingCommand = new SparqlParameterizedString(@"
                WITH @monitorLog
                INSERT {
                    ?s @startedAtUri ?startdDate .
                }
                WHERE {
                    FILTER NOT EXISTS { ?s @startedAtUri ?anyStartDate . }
                    BIND (NOW() as ?startdDate)
                };

                WITH @monitorLog
                DELETE {
                    ?s @lastAccessUri ?lastAccessDate .
                }
                INSERT {
                    ?s @lastAccessUri ?now .
                }
                WHERE {
                    BIND (NOW() as ?now)
                    OPTIONAL {
                        ?s @lastAccessUri ?lastAccessDate .
                    }
                };
                ");
            pingCommand.SetParameter("monitorLog", RDFHelper.CreateUriNode(RUNTIME_MONITOR_GRAPH_URI));
            pingCommand.SetParameter("lastAccessUri", RuntimeHelper.PropertyLastAccess);
            pingCommand.SetParameter("startedAtUri", RuntimeHelper.PropertyStartedAt);
            pingCommand.SetVariable("s", RDFHelper.CreateUriNode(connection.Uri));
            storage.Update(pingCommand.ToString());
        }
    }
}