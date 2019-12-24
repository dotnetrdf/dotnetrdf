using System;
using System.Net;
using System.Text;
using VDS.RDF;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace dotNetRDF.MockServerTests
{
    public class FederatedEndpointFixture : IDisposable
    {
        public FluentMockServer Server1 { get; }
        public FluentMockServer Server2 { get; }

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
            <binding name='p'><uri>http://example.org/s</uri></binding>
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
            <binding name='p'><uri>http://contoso.com/s</uri></binding>
            <binding name='o'><literal>o</literal></binding>
        </result>
    </results>
</sparql>";

        public FederatedEndpointFixture()
        {
            Server1 = FluentMockServer.Start();
            Server2 = FluentMockServer.Start();

            Server1.Given(Request.Create()
                    .WithPath("/query")
                    .UsingGet()
                    .WithParam("query", MatchBehaviour.AcceptOnMatch))
                .RespondWith(Response.Create()
                    .WithBody(Server1ResultsXml, encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", MimeTypesHelper.SparqlResultsXml[0])
                    .WithStatusCode(HttpStatusCode.OK));

            Server2
                .Given(Request.Create().WithPath("/query").UsingGet().WithParam("query"))
                .RespondWith(Response.Create().WithBody(Server2ResultsXml, encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", MimeTypesHelper.SparqlResultsXml[0]).WithStatusCode(HttpStatusCode.OK));
            Server2.Given(Request.Create().WithPath("/fail"))
                .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.Forbidden));
            Server2.Given(Request.Create().WithPath("/timeout"))
                .RespondWith(Response.Create().WithDelay(4000).WithBody(Server2ResultsXml, encoding: Encoding.UTF8)
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
}