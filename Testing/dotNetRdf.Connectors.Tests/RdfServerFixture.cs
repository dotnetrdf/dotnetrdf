using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace VDS.RDF.Storage;

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
