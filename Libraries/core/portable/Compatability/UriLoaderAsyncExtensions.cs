using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VDS.RDF.Parsing.Handlers;

namespace VDS.RDF.Parsing
{

    /// <summary>
    /// Provides extension methods that enable the async version of the UriLoader class to support
    /// a subset of the synchronous UriLoader methods
    /// </summary>
    public static partial class UriLoader
    {
        public static void Load(IGraph g, Uri u)
        {
            var wait = new ManualResetEvent(false);
            Load(g, u, null, (graph, state) => ((ManualResetEvent) state).Set(), wait);
            wait.WaitOne();
        }

        public static void Load(IGraph g, Uri u, IRdfReader parser)
        {
            var wait = new ManualResetEvent(false);
            Load(g,u,parser, (graph, state) => ((ManualResetEvent) state).Set(), wait );
            wait.WaitOne();
        }

        public static void Load(IRdfHandler handler, Uri u)
        {
            var wait = new ManualResetEvent(false);
            Load(handler, u, (rdfHandler, state) => ((ManualResetEvent)state).Set(), wait);
            wait.WaitOne();
        }
    }
}
