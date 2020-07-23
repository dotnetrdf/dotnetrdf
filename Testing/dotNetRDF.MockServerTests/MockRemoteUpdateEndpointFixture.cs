using System;
using System.Net;
using System.Web;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace dotNetRDF.MockServerTests
{
    public class MockRemoteUpdateEndpointFixture : IDisposable
    {
        public readonly WireMockServer Server;

        public MockRemoteUpdateEndpointFixture()
        {
            Server = WireMockServer.Start();
            Server.Given(Request.Create()
                    .WithPath("/update")
                    .UsingPost()
                    .WithBody(body=>HttpUtility.UrlDecode(body).StartsWith("update=LOAD <http://dbpedia.org/resource/Ilkeston>")))
                .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.OK));
        }

        public void Dispose()
        {
            Server.Stop();
        }

        
    }
}