using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace VDS.RDF;

public class RdfServerFixture : IDisposable
{
    public WireMockServer Server { get; }

    public HttpClient Client { get; }

    public HttpClient NoRedirectClient { get; }

    public RdfServerFixture()
    {
        Client = new HttpClient();
        Client.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");
        NoRedirectClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
        NoRedirectClient.DefaultRequestHeaders.CacheControl = CacheControlHeaderValue.Parse("no-cache");

        Server = WireMockServer.Start();

        RegisterResource("/resource/Southampton", "application/rdf+xml", Path.Combine("resources", "rdfserver", "southampton.rdf"));
        RegisterResource("/resource/czech-royals", "text/turtle", Path.Combine("resources", "czech-royals.ttl"));
        RegisterResource("/doap", "text/n3", Path.Combine("resources", "rdfserver", "doap.n3"));
        RegisterResource("/doap", "application/n-triples", Path.Combine("resources", "rdfserver", "doap.nt"));
        RegisterResource("/doap", "application/rdf+xml", Path.Combine("resources", "rdfserver", "doap.rdf"));
        RegisterResource("/doap", "application/json", Path.Combine("resources", "rdfserver", "doap.json"));
        RegisterResource("/doap", "application/ld+json", Path.Combine("resources", "rdfserver", "doap.jsonld"));
        RegisterResource("/doap", "text/turtle", Path.Combine("resources", "rdfserver", "doap.ttl"));
        RegisterResource("/dbpedia_ldf.html", "text/html", Path.Combine("resources", "rdfserver", "dbpedia_ldf.html"));

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
            .RespondWith(Response.Create().WithDelay(2500).WithNotFound());

        // Endpoint for testing SPARQL Update timeouts
        Server.Given(Request.Create().WithPath("/slow/doap")
            .UsingGet()
            .WithHeader("Accept", new WildcardMatcher("*text/turtle*"))
        ).RespondWith(Response.Create()
            .WithBodyFromFile(Path.Combine("resources", "rdfserver", "doap.ttl"))
            .WithDelay(2500)
            .WithHeader("Content-Type", "text/turtle"));

        // Endpoints for testing a redirect with See Other
        Server.Given(Request.Create().WithPath("/redirectRelative").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(303).WithHeader("Location", "/resource/Southampton"));
        Server.Given(Request.Create().WithPath("/redirectAbsolute").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(303).WithHeader("Location", Server.Urls[0] + "/resource/Southampton"));
        Server.Given(Request.Create().WithPath("/redirectQuadsRelative").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(303).WithHeader("Location", "/one.trig"));
        Server.Given(Request.Create().WithPath("/redirectQuadsAbsolute").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(303).WithHeader("Location", Server.Urls[0] + "/one.trig"));
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
