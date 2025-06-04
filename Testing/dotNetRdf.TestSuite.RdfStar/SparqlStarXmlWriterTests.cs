using System;
using System.Collections.Generic;
using System.IO;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;
using Xunit;
using StringWriter = VDS.RDF.Writing.StringWriter;

namespace VDS.RDF.TestSuite.RdfStar;

public class SparqlStarXmlWriterTests
{
    [Fact]
    public void ItSupportsWritingATripleNodeBinding()
    {
        var tn = new TripleNode(new Triple(new UriNode(new Uri("http://example.org/s")),
            new UriNode(new Uri("http://example.org/p")), new LiteralNode("o",  false)));
        var results = new List<ISparqlResult>
        {
            new SparqlResult(new[]
            {
                new KeyValuePair<string, INode>("x", tn)
            })
        };
        var resultSet = new SparqlResultSet(results);
        ISparqlResultsWriter writer = new SparqlXmlWriter();
        var output = StringWriter.Write(resultSet, writer);

        var parser = new SparqlXmlParser();
        var roundTrippedResultSet = new SparqlResultSet();
        using (var input = new StringReader(output))
        {
            parser.Load(roundTrippedResultSet, input);
        }

        Assert.Single(roundTrippedResultSet);
        Assert.Equal(tn, roundTrippedResultSet[0]["x"]);
    }
}
