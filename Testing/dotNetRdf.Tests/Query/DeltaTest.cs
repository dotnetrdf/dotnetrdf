/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

namespace VDS.RDF.Query;


public class DeltaTest
{
    private readonly TurtleParser _parser = new TurtleParser();
    private readonly SparqlQueryParser _sparqlParser = new SparqlQueryParser();

    private const string TestData = @"
<http://r1> <http://r1> <http://r1> .
<http://r2> <http://r2> <http://r2> .
";

    private const string TestData2 = @"
<http://r1> <http://r1> <http://r1> , <http://r2> .
<http://r2> <http://r2> <http://r2> .
";

    private const string TestData3 = @"
<http://r1> <http://r1> <http://r1> , ""value"" .
<http://r2> <http://r2> <http://r2> , 1234 .
";

    private const string TestData4 = @"
<http://r1> <http://r1> <http://r1> , ""value"" , 1234, 123e4, 123.4, true, false .
<http://r2> <http://r2> <http://r2> .
";

    private const string TestData5 = @"
<http://r2> <http://r2> <http://r2> .
";

    private const string MinusQuery = @"
SELECT *
WHERE
{
    GRAPH <http://a>
    {
        ?s ?p ?o .
    }
    MINUS
    {
        GRAPH <http://b> { ?s ?p ?o }
    }
}
";

    private const string OptionalSameTermQuery1 = @"
SELECT *
WHERE
{
    GRAPH <http://a>
    {
        ?s ?p ?o .
    }
    OPTIONAL
    {
        GRAPH <http://b> { ?s0 ?p0 ?o0 . }
        FILTER (SAMETERM(?s, ?s0) && SAMETERM(?p, ?p0) && SAMETERM(?o, ?o0))
    }
    FILTER(!BOUND(?s0))
}
";

    private const string OptionalSameTermQuery2 = @"
SELECT *
WHERE
{
    GRAPH <http://a>
    {
        ?s ?p ?o .
    }
    OPTIONAL
    {
        GRAPH <http://b> { ?s ?p ?o0 . }
        FILTER (SAMETERM(?o, ?o0))
    }
    FILTER(!BOUND(?o0))
}
";

    private const string NotExistsQuery = @"
SELECT *
WHERE
{
    GRAPH <http://a>
    {
        ?s ?p ?o .
    }
    FILTER NOT EXISTS { GRAPH <http://b> { ?s ?p ?o } }
}
";

    private void TestQuery(IInMemoryQueryableStore store, string query, int differences)
    {
        SparqlQuery q = _sparqlParser.ParseFromString(query);
        var processor = new LeviathanQueryProcessor(store);
        using (var resultSet = processor.ProcessQuery(q) as SparqlResultSet)
        {
            Assert.NotNull(resultSet);
            Assert.Equal(differences, resultSet.Count);
        }
    }

    private void TestDeltas(IGraph a, IGraph b, int differences)
    {
        a.BaseUri = new Uri("http://a");
        b.BaseUri = new Uri("http://b");

        IInMemoryQueryableStore store = new TripleStore();
        store.Add(a);
        store.Add(b);

        TestQuery(store, MinusQuery, differences);
        TestQuery(store, OptionalSameTermQuery1, differences);
        TestQuery(store, OptionalSameTermQuery2, differences);
        TestQuery(store, NotExistsQuery, differences);
    }

    [Theory]
    [InlineData(TestData, TestData, 0)]
    [InlineData(TestData, TestData2, 0)]
    [InlineData(TestData2, TestData, 1)]
    [InlineData(TestData, TestData5, 1)]
    [InlineData(TestData5, TestData, 0)]
    [InlineData(TestData, null, 2)]
    [InlineData(null, TestData, 0)]
    [InlineData(TestData3, TestData, 2)]
    [InlineData(TestData, TestData3, 0)]
    [InlineData(TestData4, TestData, 6)]
    [InlineData(TestData, TestData4, 0)]
    public void SparqlGraphDeltas(string graphAData, string graphBData, int difference)
    {
        IGraph a = new Graph(new UriNode(new Uri("http://a")));
        IGraph b = new Graph(new UriNode(new Uri("http://b")));
        if (graphAData != null) _parser.Load(a, new StringReader(graphAData));
        if (graphBData != null) _parser.Load(b, new StringReader(graphBData));
        TestDeltas(a, b, difference);
    }

}