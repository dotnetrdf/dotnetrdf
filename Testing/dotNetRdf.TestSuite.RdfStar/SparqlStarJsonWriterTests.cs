using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using VDS.RDF.Query;
using VDS.RDF.Writing;
using Xunit;

namespace VDS.RDF.TestSuite.RdfStar;

public class SparqlStarJsonWriterTests
{
    [Fact]
    public void ItHandlesFormattingATripleNode()
    {
        var results = new List<ISparqlResult>
        {
            new SparqlResult(new[]
            {
                new KeyValuePair<string, INode>("x",
                    new TripleNode(new Triple(new UriNode(new Uri("http://example.org/s")),
                        new UriNode(new Uri("http://example.org/p")), new LiteralNode("o", (Uri)null, false))))
            })
        };
        var resultSet = new SparqlResultSet(results);
        ISparqlResultsWriter writer = new SparqlJsonWriter();
        var output = StringWriter.Write(resultSet, writer);
        var jsonNode = JObject.Parse(output);
        var expectJson =
            @"{ type: 'triple', value: { subject: { type: 'uri', value: 'http://example.org/s' }, predicate: { type: 'uri', value: 'http://example.org/p' }, object: { type: 'literal', value: 'o' } } }";
        var expectJsonNode = JObject.Parse(expectJson);
        JToken tripleNodeResult = jsonNode["results"]?["bindings"]?[0]?["x"];
        Assert.NotNull(tripleNodeResult);
        Assert.True(JToken.DeepEquals(expectJsonNode, tripleNodeResult));
    }
}
