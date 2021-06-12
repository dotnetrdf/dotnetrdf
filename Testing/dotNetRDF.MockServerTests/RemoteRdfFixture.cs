using System;
using System.IO;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace dotNetRDF.MockServerTests
{
    public class RemoteRdfFixture : IDisposable
    {
        public WireMockServer Server { get; }

        public RemoteRdfFixture()
        {
            Server = WireMockServer.Start();
            Server.Given(Request.Create()
                .WithPath("/rvesse.ttl")
                .UsingGet())
                .RespondWith(Response.Create()
                    .WithBodyFromFile(Path.Combine("resources", "rvesse.ttl"))
                    .WithHeader("Content-Type", "text/turtle")
                );
        }
        public void Dispose()
        {
            Server.Stop();
            Server.Dispose();
        }
    }
}
