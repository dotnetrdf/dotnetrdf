using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace VDS.RDF.Query;

public class SparqlRemoteTestsBase : IDisposable
{
    protected const string ConstructQuery = "CONSTRUCT {?s ?p ?o} WHERE {?s ?p ?o}";
    protected const string SelectQuery = "SELECT * WHERE {?s ?p ?o}";

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

    protected WireMockServer _server;

    public SparqlRemoteTestsBase()
    {
        _server = WireMockServer.Start();
    }

    public void Dispose()
    {
        _server.Stop();
    }


    protected void RegisterSelectQueryGetHandler(string query = SelectQuery)
    {
        _server.Given(Request.Create()
                .WithPath("/sparql")
                .UsingGet()
                .WithParam(queryParams=>
                    queryParams.ContainsKey("query") && 
                    queryParams["query"].Any(q => HttpUtility.UrlDecode(q).StartsWith(query))))
            .RespondWith(Response.Create()
                .WithBody(SparqlResultsXml, encoding:Encoding.UTF8)
                .WithHeader("Content-Type", MimeTypesHelper.SparqlResultsXml[0])
                .WithStatusCode(HttpStatusCode.OK));
    }

    protected void RegisterConstructQueryGetHandler()
    {
        _server.Given(Request.Create()
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
        _server.Given(Request.Create()
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
        _server.Given(Request.Create()
                .WithPath("/sparql")
                .UsingPost()
                .WithBody(x=>x.Contains("query=" + HttpUtility.UrlEncode(SelectQuery))))
            .RespondWith(Response.Create()
                .WithBody(SparqlResultsXml, encoding: Encoding.UTF8)
                .WithHeader("Content-Type", MimeTypesHelper.SparqlResultsXml[0])
                .WithStatusCode(HttpStatusCode.OK));
    }

    protected void RegisterErrorHandler()
    {
        _server.Given(Request.Create()
                .WithPath("/sparql")
                .UsingAnyMethod())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.ServiceUnavailable));
    }
}