/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace VDS.RDF.LDF.Client;

[Collection("QpfServer")]
public class E2EMockTests(MockQpfServer qpfServer, ITestOutputHelper output) : E2EBaseTests(output)
{
    private readonly MockQpfServer qpfServer = qpfServer;

    protected override TpfLiveGraph Graph => new(new(qpfServer.BaseUri, "2016-04/en"));
}

[CollectionDefinition("QpfServer")]
public class QpfServerCollection : ICollectionFixture<MockQpfServer> { }

public sealed class MockQpfServer : IDisposable
{
    private readonly WireMockServer server;

    public MockQpfServer()
    {
        server = WireMockServer.Start();

        static IResponseBuilder file(string q) => Response.Create()
            .WithHeader("Content-Type", "text/turtle")
            .WithTransformer(true)
            .WithBodyFromFile(Path.Combine("resources", "dbpedia", $"{q}.ttl"));

        static IRequestBuilder root() => Request.Create().WithPath("/2016-04/en");
        server.Given(root()).RespondWith(file("root"));
        server.Given(root().WithParam("page")).RespondWith(file("root{{request.query.page}}"));

        var containsTriple = root().WithParam("subject", "http://dbpedia.org/ontology/extinctionDate").WithParam("predicate", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type").WithParam("object", "http://www.w3.org/1999/02/22-rdf-syntax-ns#Property");
        server.Given(containsTriple).RespondWith(file("containsTriple"));

        static IRequestBuilder triplesWithObject() => root().WithParam("object", "\"1997-02-04\"^^http://www.w3.org/2001/XMLSchema#date");
        server.Given(triplesWithObject()).RespondWith(file("getTriplesWithObject"));
        server.Given(triplesWithObject().WithParam("page")).RespondWith(file("getTriplesWithObject{{request.query.page}}"));

        static IRequestBuilder triplesWithPredicate() => root().WithParam("predicate", "http://dbpedia.org/ontology/extinctionDate");
        server.Given(triplesWithPredicate()).RespondWith(file("getTriplesWithPredicate"));
        server.Given(triplesWithPredicate().WithParam("page")).RespondWith(file("getTriplesWithPredicate{{request.query.page}}"));

        static IRequestBuilder triplesWithPredicateObject() => root().WithParam("predicate", "http://dbpedia.org/ontology/extinctionDate").WithParam("object", "\"2011-10-05\"^^http://www.w3.org/2001/XMLSchema#date");
        server.Given(triplesWithPredicateObject()).RespondWith(file("getTriplesWithPredicateObject"));

        static IRequestBuilder triplesWithSubject() => root().WithParam("subject", "http://0-access.newspaperarchive.com.topcat.switchinc.org/Viewer.aspx?img=7578853");
        server.Given(triplesWithSubject()).RespondWith(file("getTriplesWithSubject"));

        static IRequestBuilder triplesWithSubjectObject() => root().WithParam("subject", "http://dbpedia.org/resource/123_Democratic_Alliance").WithParam("object", "\"707366241\"^^http://www.w3.org/2001/XMLSchema#integer");
        server.Given(triplesWithSubjectObject()).RespondWith(file("getTriplesWithSubjectObject"));

        static IRequestBuilder triplesWithSubjectPredicate() => root().WithParam("subject", "http://dbpedia.org/resource/123_Democratic_Alliance").WithParam("predicate", "http://dbpedia.org/ontology/extinctionDate");
        server.Given(triplesWithSubjectPredicate()).RespondWith(file("getTriplesWithSubjectPredicate"));
    }

    public Uri BaseUri => new(server.Url);

    void IDisposable.Dispose() => server.Stop();
}
