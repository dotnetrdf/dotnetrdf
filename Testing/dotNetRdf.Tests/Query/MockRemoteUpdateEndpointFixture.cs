using System;
using System.Net;
using System.Web;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace VDS.RDF.Query;

public class MockRemoteUpdateEndpointFixture : IDisposable
{
    public WireMockServer Server { get; }

    public MockRemoteUpdateEndpointFixture()
    {
        Server = WireMockServer.Start();
        Server.Given(Request.Create()
                .WithPath("/update")
                .UsingPost()
                .WithBody(body => HttpUtility.UrlDecode(body).StartsWith("update=LOAD <http://dbpedia.org/resource/Ilkeston>")))
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.OK));
        Server.Given(Request.Create()
                .WithPath("/update")
                .UsingPost()
                .WithBody(body =>
                    HttpUtility.UrlDecode(body).StartsWith("update=DELETE <http://dbpedia.org/resource/Ilkeston>")))
            .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.Forbidden));
    }


    public void Dispose()
    {
        Server.Stop();
    }

    
}