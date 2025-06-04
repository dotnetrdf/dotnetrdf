using System;
using System.Net;
using System.Text;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace VDS.RDF.Query;

public class FederatedEndpointFixture : IDisposable
{
    public WireMockServer Server1 { get; }
    public WireMockServer Server2 { get; }

    private const string Server1ResultsXml = @"<?xml version=""1.0""?>
<sparql xmlns='http://www.w3.org/2005/sparql-results#'>
    <head>
        <variable name='s'/>
        <variable name='p'/>
        <variable name='o'/>
    </head>
    <results>
        <result>
            <binding name='s'><uri>http://example.org/s</uri></binding>
            <binding name='p'><uri>http://example.org/p</uri></binding>
            <binding name='o'><literal>o</literal></binding>
        </result>
    </results>
</sparql>";

    private const string Server2ResultsXml = @"<?xml version=""1.0""?>
<sparql xmlns='http://www.w3.org/2005/sparql-results#'>
    <head>
        <variable name='s'/>
        <variable name='p'/>
        <variable name='o'/>
    </head>
    <results>
        <result>
            <binding name='s'><uri>http://contoso.com/s</uri></binding>
            <binding name='p'><uri>http://contoso.com/p</uri></binding>
            <binding name='o'><literal>o</literal></binding>
        </result>
    </results>
</sparql>";

    private const string Server1ConstructResults = "<http://example.org/s> <http://example.org/p> \"o\" .";
    private const string Server2ConstructResults = "<http://contoso.com/s> <http://contoso.com/p> \"o\" .";

    public FederatedEndpointFixture()
    {
        Server1 = WireMockServer.Start();
        Server2 = WireMockServer.Start();

        Server1.Given(Request.Create()
                .WithPath("/query")
                .UsingGet()
                .WithParam("query", MatchBehaviour.AcceptOnMatch))
            .RespondWith(Response.Create()
                .WithBody(Server1ResultsXml, encoding: Encoding.UTF8)
                .WithHeader("Content-Type", MimeTypesHelper.SparqlResultsXml[0])
                .WithStatusCode(HttpStatusCode.OK));
        Server1.Given(Request.Create().WithPath("/query2").UsingGet().WithParam("query"))
            .RespondWith(Response.Create().WithBody(Server1ConstructResults, encoding: Encoding.UTF8)
                .WithHeader("Content-Type", "application/n-triples").WithStatusCode(HttpStatusCode.OK));

        Server2
            .Given(Request.Create().WithPath("/query").UsingGet().WithParam("query"))
            .RespondWith(Response.Create().WithBody(Server2ResultsXml, encoding: Encoding.UTF8)
                .WithHeader("Content-Type", MimeTypesHelper.SparqlResultsXml[0]).WithStatusCode(HttpStatusCode.OK));
        Server2.Given(Request.Create().WithPath("/query2").UsingGet().WithParam("query"))
            .RespondWith(Response.Create().WithBody(Server2ConstructResults, encoding: Encoding.UTF8)
                .WithHeader("Content-Type", "application/n-triples").WithStatusCode(HttpStatusCode.OK));
        Server2.Given(Request.Create().WithPath("/fail"))
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.Forbidden));
        Server2.Given(Request.Create().WithPath("/timeout"))
            .RespondWith(Response.Create().WithDelay(6000).WithBody(Server2ResultsXml, encoding: Encoding.UTF8)
                .WithHeader("Content-Type", MimeTypesHelper.SparqlResultsXml[0]).WithStatusCode(HttpStatusCode.OK));

    }

    public void Dispose()
    {
        Server1.Stop();
        Server1.Dispose();
        Server2.Stop();
        Server2.Dispose();
    }
}