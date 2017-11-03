using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using VDS.RDF.Update;
using Xunit;

namespace dotNetRDF.MockServerTests
{
    public partial class SparqlRemoteUpdateEndpointTests
    {
        [Fact]
        public void SparqlRemoteEndpointAsyncApiUpdate()
        {
           
            SparqlRemoteUpdateEndpoint endpoint = GetUpdateEndpoint();
            ManualResetEvent signal = new ManualResetEvent(false);
            endpoint.Update("LOAD <http://dbpedia.org/resource/Ilkeston> INTO GRAPH <http://example.org/async/graph>", s =>
            {
                signal.Set();
                signal.Close();
            }, null);

            Thread.Sleep(1000);
            Assert.True(signal.SafeWaitHandle.IsClosed, "Wait Handle should be closed");
        }
    }
}
