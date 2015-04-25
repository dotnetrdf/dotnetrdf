/*

dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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