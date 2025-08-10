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
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Writing;


public class SparqlTsvTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private InMemoryDataset _dataset;
    private LeviathanQueryProcessor _processor;
    private readonly SparqlQueryParser _parser = new SparqlQueryParser();
    private readonly SparqlTsvParser _tsvParser = new SparqlTsvParser();
    private readonly SparqlTsvWriter _tsvWriter = new SparqlTsvWriter();

    public SparqlTsvTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private void EnsureTestData()
    {
        if (_dataset == null)
        {
            var store = new TripleStore();
            var g = new Graph();
            g.LoadFromFile(Path.Combine("resources", "InferenceTest.ttl"));
            g.BaseUri = new Uri("http://example.org/graph");
            store.Add(g);

            _dataset = new InMemoryDataset(store);
            _processor = new LeviathanQueryProcessor(_dataset);
        }
    }

    [Theory]
    [InlineData("SELECT * WHERE { ?s a ?type }")]
    [InlineData("SELECT * WHERE { ?s a ?type . ?s ex:Speed ?speed }")]
    [InlineData("SELECT * WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } }")]
    [InlineData("SELECT * WHERE { ?s <http://example.org/noSuchThing> ?o }")]
    [InlineData("SELECT * WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } ?s ?p ?o }")]
    [InlineData("SELECT ?s (ISLITERAL(?o) AS ?LiteralObject) WHERE { ?s ?p ?o }")]
    public void TestTsvRoundTrip(string query)
    {
        EnsureTestData();

        var queryString = new SparqlParameterizedString(query);
        queryString.Namespaces.AddNamespace("ex", new Uri("http://example.org/vehicles/"));
        SparqlQuery q = _parser.ParseFromString(queryString);

        var original = _processor.ProcessQuery(q) as SparqlResultSet;
        Assert.NotNull(original);

        _testOutputHelper.WriteLine("Original Results:");
        TestTools.ShowResults(original, _testOutputHelper);

        var writer = new System.IO.StringWriter();
        _tsvWriter.Save(original, writer);
        _testOutputHelper.WriteLine("Serialized TSV Results:");
        _testOutputHelper.WriteLine(writer.ToString());
        _testOutputHelper.WriteLine();

        var results = new SparqlResultSet();
        _tsvParser.Load(results, new StringReader(writer.ToString()));
        _testOutputHelper.WriteLine("Parsed Results:");
        TestTools.ShowResults(results, _testOutputHelper);

        Assert.True(original.Equals(results));
        Assert.Equal(original, results);
        
    }
}
