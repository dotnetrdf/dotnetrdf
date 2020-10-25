using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace VDS.RDF
{
    public class RdfServerFixture : IDisposable
    {
        public WireMockServer Server { get; }

        public HttpClient Client { get; }

        public RdfServerFixture()
        {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");

            Server = WireMockServer.Start();

            RegisterResource("/resource/Southampton", "application/rdf+xml", Path.Combine("resources", "rdfserver", "southampton.rdf"));
            RegisterResource("/resource/czech-royals", "text/turtle", Path.Combine("resources", "czech-royals.ttl"));
            RegisterResource("/doap", "text/n3", Path.Combine("resources", "rdfserver", "doap.n3"));
            RegisterResource("/doap", "application/n-triples", Path.Combine("resources", "rdfserver", "doap.nt"));
            RegisterResource("/doap", "application/rdf+xml", Path.Combine("resources", "rdfserver", "doap.rdf"));
            RegisterResource("/doap", "application/json", Path.Combine("resources", "rdfserver", "doap.json"));
            RegisterResource("/doap", "application/ld+json", Path.Combine("resources", "rdfserver", "doap.jsonld"));
            RegisterResource("/doap", "text/turtle", Path.Combine("resources", "rdfserver", "doap.ttl"));

            RegisterResourceWithStringContent("/one.ttl", "text/turtle", "<#s> a <#t> .");
            RegisterResourceWithStringContent("/one.nt", "application/n-triples",
                "<http://example.org/s> <http://example.org/p> <http://example.org/o> .");
            RegisterResourceWithStringContent("/one.trig", "application/x-trig",
                @"
{
    <http://example.org/s> <http://example.org/p> <http://example.org/o> .
}
<http://example.org/graph>
{
    <http://example.org/s> <http://example.org/p2> <http://example.org/o2> .
}");
            RegisterResourceWithStringContent("/resource", "text/turtle", "<#s> <#p> <#o> .");

            // Endpoint for testing task cancellation
            Server.Given(Request.Create().WithPath("/wait").UsingGet())
                .RespondWith(Response.Create().WithDelay(1000).WithNotFound());
        }

        public Uri UriFor(string path)
        {
            return new Uri(Server.Urls[0] + path);
        }

        private void RegisterResource(string path, string contentType, string filePath)
        {
            Server.Given(Request.Create()
                    .WithPath(path)
                    .UsingGet()
                    .WithHeader("Accept", new WildcardMatcher("*" + contentType + "*"))
                )
                .RespondWith(Response.Create()
                    .WithBodyFromFile(filePath)
                    .WithHeader("Content-Type", contentType));
        }

        private void RegisterResourceWithStringContent(string path, string contentType, string content)
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

    [CollectionDefinition("RdfServer")]
    public class RdfServerCollection : ICollectionFixture<RdfServerFixture>
    {
    }
}
