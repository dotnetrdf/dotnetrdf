using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VDS.RDF.Compatability;
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
            var state = new AsyncOperationState();
            Load(g, u, null, (graph, aos) => ((AsyncOperationState) aos).OperationCompleted(), state);
            state.WaitForCompletion();
        }

        public static void Load(IGraph g, Uri u, IRdfReader parser)
        {
            var state = new AsyncOperationState();
            Load(g,u,parser, (graph, aos) => ((AsyncOperationState) aos).OperationCompleted(), state );
            state.WaitForCompletion();
        }

        public static void Load(IRdfHandler handler, Uri u)
        {
            var state = new AsyncOperationState();
            Load(handler, u, (rdfHandler, aos) => ((AsyncOperationState)aos).OperationCompleted(), state);
            state.WaitForCompletion();
        }
    }
}
