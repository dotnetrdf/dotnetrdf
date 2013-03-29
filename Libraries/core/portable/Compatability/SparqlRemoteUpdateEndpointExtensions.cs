using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VDS.RDF.Update
{
    public static class SparqlRemoteUpdateEndpointExtensions
    {
        public static void Update(this SparqlRemoteUpdateEndpoint endpoint, string update)
        {
            var wait = new AutoResetEvent(false);
            endpoint.Update(update, state => { (state as AutoResetEvent).Set(); }, wait);
            wait.WaitOne();
        }
    }
}
