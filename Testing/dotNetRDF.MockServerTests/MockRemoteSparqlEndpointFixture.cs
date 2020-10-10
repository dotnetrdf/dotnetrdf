using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using VDS.RDF;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace dotNetRDF.MockServerTests
{
    public class MockRemoteSparqlEndpointFixture : IDisposable
    {
        public WireMockServer Server { get; }

        public MockRemoteSparqlEndpointFixture()
        {
            Server = WireMockServer.Start();
            RegisterSelectQueryGetHandler();
            RegisterSelectQueryPostHandler();
            RegisterConstructQueryGetHandler();
            RegisterConstructQueryPostHandler();
            RegisterErrorHandler();
        }

        public void Dispose()
        {
            Server.Stop();
        }

        public readonly string ConstructQuery = "CONSTRUCT {?s ?p ?o} WHERE {?s ?p ?o}";
        public readonly string ErrorConstructQuery = "CONSTRUCT {?s ?p ?err} WHERE {?s ?p ?o}";
        public readonly string SelectQuery = "SELECT * WHERE {?s ?p ?o}";
        public readonly string ErrorSelectQuery = "SELECT ?err WHERE {?s ?p ?o}";

        private const string SparqlResultsXml = @"<?xml version=""1.0""?>
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

        private const string ConstructResults = "<http://example.org/s> <http://example.org/p> \"o\" .";

        protected void RegisterSelectQueryGetHandler()
        {
            RegisterSelectQueryGetHandler(SelectQuery);
        }

        public void RegisterSelectQueryGetHandler(string query)
        {
            Server.Given(Request.Create()
                    .WithPath("/sparql")
                    .UsingGet()
                    .WithParam(queryParams =>
                        queryParams.ContainsKey("query") &&
                        queryParams["query"].Any(q => HttpUtility.UrlDecode(q).StartsWith(query))))
                .RespondWith(Response.Create()
                    .WithBody(SparqlResultsXml, encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", MimeTypesHelper.SparqlResultsXml[0])
                    .WithStatusCode(HttpStatusCode.OK));
        }

        protected void RegisterConstructQueryGetHandler()
        {
            Server.Given(Request.Create()
                    .WithPath("/sparql")
                    .UsingGet()
                    .WithParam(queryParams =>
                        queryParams.ContainsKey("query") &&
                        queryParams["query"].Any(q => HttpUtility.UrlDecode(q).StartsWith(ConstructQuery))))
                .RespondWith(Response.Create()
                    .WithBody(ConstructResults, encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", "application/n-triples")
                    .WithStatusCode(HttpStatusCode.OK));
        }

        protected void RegisterConstructQueryPostHandler()
        {
            Server.Given(Request.Create()
                    .WithPath("/sparql")
                    .UsingPost()
                    .WithBody(x => x.Contains("query=" + HttpUtility.UrlEncode(ConstructQuery))))
                .RespondWith(Response.Create()
                    .WithBody(ConstructResults, encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", "application/n-triples")
                    .WithStatusCode(HttpStatusCode.OK));
        }

        protected void RegisterSelectQueryPostHandler()
        {
            Server.Given(Request.Create()
                    .WithPath("/sparql")
                    .UsingPost()
                    .WithBody(x => x.Contains("query=" + HttpUtility.UrlEncode(SelectQuery))))
                .RespondWith(Response.Create()
                    .WithBody(SparqlResultsXml, encoding: Encoding.UTF8)
                    .WithHeader("Content-Type", MimeTypesHelper.SparqlResultsXml[0])
                    .WithStatusCode(HttpStatusCode.OK));
        }

        protected void RegisterErrorHandler()
        {
            Server.Given(Request.Create()
                    .WithPath("/sparql")
                    .UsingGet()
                    .WithParam(queryParams =>
                        queryParams.ContainsKey("query") &&
                        queryParams["query"].Any(q => HttpUtility.UrlDecode(q).Contains("?err"))))
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.BadRequest));
        }
    }
}