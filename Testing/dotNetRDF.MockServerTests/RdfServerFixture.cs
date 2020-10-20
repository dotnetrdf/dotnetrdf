using System;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace dotNetRDF.MockServerTests
{
    public class RdfServerFixture : IDisposable
    {
        public WireMockServer Server { get; }

        public RdfServerFixture()
        {
            Server = WireMockServer.Start();
            RegisterResource("/one.ttl", "text/turtle", "<#s> a <#t> .");
            RegisterResource("/one.nt", "application/n-triples", 
                "<http://example.org/s> <http://example.org/p> <http://example.org/o> .");
            RegisterResource("/one.trig", "application/x-trig",
                @"
{
    <http://example.org/s> <http://example.org/p> <http://example.org/o> .
}
<http://example.org/graph>
{
    <http://example.org/s> <http://example.org/p2> <http://example.org/o2> .
}");
            RegisterResource("/resource", "text/turtle", "<#s> <#p> <#o> .");

            // Endpoint for testing task cancellation
            Server.Given(Request.Create().WithPath("/wait").UsingGet())
                .RespondWith(Response.Create().WithDelay(1000).WithNotFound());
        }

        private void RegisterResource(string path, string contentType, string content)
        {
            Server.Given(Request.Create()
                    .WithPath(path)
                    .UsingGet()
                    .WithHeader("Accept", new WildcardMatcher("*" + contentType + "*"))
                    )
                .RespondWith(Response.Create()
                    .WithBody(content)
                    .WithHeader("Content-Type", contentType));
        }

        public void Dispose()
        {
            Server.Stop();
        }
    }
}