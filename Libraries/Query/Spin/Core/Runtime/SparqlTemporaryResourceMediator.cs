using System;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Spin.Utility;
using VDS.RDF.Storage;

namespace VDS.RDF.Query.Spin.Core.Runtime
{
    #region Events handlers

    internal delegate void ReleasedEventHandler(Object sender);

    #endregion Events handlers

    /// <summary>
    /// A base class for objects that require usage of temporary graphs in the underlying storage
    /// </summary>
    /// TODO provide an API to simplify temporary resources declaration, dependencies and runtime management
    /// TODO allow to create direct instances for potential garbage collection (mainly in case of crash recovery or whenever timed-out resources are not automatically disposed of)
    public abstract class SparqlTemporaryResourceMediator
        : IDisposable
    {
        internal event ReleasedEventHandler Released;

        /// <summary>
        /// The base namespace prefix for temporary resources and vocabulary
        /// </summary>
        public const String BASE_URI = "tmp:dotnetrdf.org";

        /// <summary>
        /// The Uri prefix for temporary resources
        /// </summary>
        public const String NS_URI = BASE_URI + ":";

        public const String RES_URI = BASE_URI + "#";

        private readonly String _id = Guid.NewGuid().ToString().Replace("-", "");

        internal String ID
        {
            get
            {
                return _id;
            }
        }

        public Uri Uri
        {
            get
            {
                return UriFactory.Create(NS_URI + _id);
            }
        }

        internal abstract IUpdateableStorage UnderlyingStorage { get; }

        internal abstract SparqlTemporaryResourceMediator ParentContext { get; }

        internal void RaiseReleased()
        {
            // first notify all listeners
            ReleasedEventHandler handler = Released;
            try
            {
                if (handler != null) Released.Invoke(this);
            }
            finally
            {
                // then clean the StorageRuntimeMonitorGraph

                // Get any reference to graphs owned solely by the mediator
                SparqlParameterizedString command = new SparqlParameterizedString(@"
                    SELECT DISTINCT ?tempGraph
                    FROM @monitorLog
                    WHERE {
                        ?tempGraph @requiredBy @consumerUri .
                        # to clear any reference to temp graphs
                        # the graph must not be referenced by another consumer
                        FILTER (NOT EXISTS {
                            ?tempGraph @requiredBy ?concurrentConsumer .
                            FILTER (!sameTerm(@consumerUri, ?concurrentConsumer))
                        })
                    }");
                command.SetParameter("monitorLog", RDFHelper.CreateUriNode(StorageRuntimeMonitor.RUNTIME_MONITOR_GRAPH_URI));
                command.SetParameter("requiredBy", RuntimeHelper.PropertyRequiredBy);
                //command.SetParameter("hasScope", PropertyHasScope);
                command.SetParameter("consumerUri", RDFHelper.CreateUriNode(Uri));
                command.SetParameter("RdfNull", RuntimeHelper.BLACKHOLE);

                SparqlResultSet metas = (SparqlResultSet)UnderlyingStorage.Query(command.ToString());
                // Build the list resources to garbage and graphs to clear
                StringBuilder dropGraphsSB = new StringBuilder();
                StringBuilder releasableResourcesFilter = new StringBuilder();
                releasableResourcesFilter.AppendLine("@consumerUri");
                foreach (Uri graphUri in metas.Results.Select(r => ((IUriNode)r.Value("tempGraph")).Uri))
                {
                    releasableResourcesFilter.AppendLine(", <" + graphUri.ToString() + ">");
                    dropGraphsSB.AppendLine("DROP SILENT GRAPH <" + graphUri.ToString() + ">;");
                }
                // Then clean up the UnderlyingStorage
                command.CommandText = @"
                    DELETE {
                        GRAPH @monitorLog {
                            ?garbagedResource ?p ?o .
                        }
                    }
                    USING NAMED @monitorLog
                    WHERE {
                        GRAPH @monitorLog {
                            ?garbagedResource ?p ?o .
                            FILTER (?garbagedResource IN(" + releasableResourcesFilter.ToString() + @"))
                        }
                    };

                    " + dropGraphsSB.ToString() + @"
                    #DROP SILENT GRAPH @RdfNull;
                    ";
                UnderlyingStorage.Update(command.ToString());
            }
        }

        public virtual void Dispose()
        {
            RaiseReleased();
        }
    }
}