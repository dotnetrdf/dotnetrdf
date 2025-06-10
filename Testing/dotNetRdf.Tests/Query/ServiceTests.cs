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

using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Update;

namespace VDS.RDF.Query;

public class ServiceTests : IClassFixture<MockRemoteSparqlEndpointFixture>
{
    private readonly MockRemoteSparqlEndpointFixture _serverFixture;

    public ServiceTests(MockRemoteSparqlEndpointFixture serverFixture)
    {
        _serverFixture = serverFixture;
    }

    [Fact]
    public void SparqlServiceQuery()
    {
        _serverFixture.RegisterSelectQueryGetHandler("SELECT * WHERE { ?s ?p ?o . } LIMIT 10 ");
        var query = $"SELECT * WHERE {{ SERVICE <{_serverFixture.Server.Urls[0] + "/sparql"}> {{ ?s ?p ?o }} }} LIMIT 10";
        var parser = new SparqlQueryParser();
        var q = parser.ParseFromString(query);

        var processor = new LeviathanQueryProcessor(new TripleStore(), options => options.QueryExecutionTimeout = 5000);
        var results = processor.ProcessQuery(q);
        Assert.IsType<SparqlResultSet>(results);
        var resultSet = results as SparqlResultSet;
        Assert.NotEmpty(resultSet.Results);
    }

    [Fact]
    public void SparqlServiceUpdate()
    {
        _serverFixture.RegisterSelectQueryGetHandler("SELECT * WHERE \r\n{ ?s ?p ?o . }\r\n");
        // _serverFixture.RegisterSelectQueryPostHandler();
        var query = $"INSERT {{ ?s ?p ?o }} WHERE {{ SERVICE <{_serverFixture.Server.Urls[0] + "/sparql"}> {{ ?s ?p ?o . }} }}";
        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet q = parser.ParseFromString(query);

        var processor =
            new LeviathanUpdateProcessor(new TripleStore(), options => options.UpdateExecutionTimeout = 15000);
        processor.ProcessCommandSet(q);
    }
    
    [Fact]
    [Trait("Category", "explicit")]
    public void SparqlServiceUsingBindings()
    {
        _serverFixture.RegisterSelectQueryGetHandler("SELECT * WHERE { <http://dbpedia.org/resource/Southampton> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> ? type . }");
        _serverFixture.RegisterSelectQueryGetHandler("SELECT * WHERE { <http://dbpedia.org/resource/Ilkeston> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> ? type . }");
        var query = "SELECT * WHERE { SERVICE <http://dbpedia.org/sparql> { ?s a ?type } } VALUES ?s { <http://dbpedia.org/resource/Southampton> <http://dbpedia.org/resource/Ilkeston> }";
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        var processor = new LeviathanQueryProcessor(new TripleStore(), options => options.QueryExecutionTimeout = 5000);
        var results = processor.ProcessQuery(q);
        if (results is SparqlResultSet)
        {
            TestTools.ShowResults(results);
        }
        else
        {
            Assert.Fail("Should have returned a SPARQL Result Set");
        }
    }

    [Fact]
    public void SparqlServiceWithNonExistentService()
    {
        var query = "SELECT * WHERE { SERVICE <http://www.dotnetrdf.org/noSuchService> { ?s a ?type } } LIMIT 10";
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        var processor = new LeviathanQueryProcessor(new TripleStore(), options => options.QueryExecutionTimeout = 5000);
        Assert.Throws<RdfQueryException>(() => processor.ProcessQuery(q));
    }

    [Fact]
    public void SparqlServiceWithSilentNonExistentService()
    {
        var query =
            "SELECT * WHERE { SERVICE SILENT <http://www.dotnetrdf.org/noSuchService> { ?s a ?type } } LIMIT 10";
        var parser = new SparqlQueryParser();
        SparqlQuery q = parser.ParseFromString(query);

        var processor = new LeviathanQueryProcessor(new TripleStore(), options=>options.QueryExecutionTimeout = 5000);
        object results = processor.ProcessQuery(q);
        TestTools.ShowResults(results);
    }
}
