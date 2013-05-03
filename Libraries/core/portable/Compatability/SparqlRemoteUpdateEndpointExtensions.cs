using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VDS.RDF.Compatability;

namespace VDS.RDF.Update
{
    public static class SparqlRemoteUpdateEndpointExtensions
    {
        public static void Update(this SparqlRemoteUpdateEndpoint endpoint, string update)
        {
            var wait = new AsyncOperationState();
            endpoint.Update(update, state => { (state as AsyncOperationState).OperationCompleted(); }, wait);
            wait.WaitForCompletion();
        }
    }
}
