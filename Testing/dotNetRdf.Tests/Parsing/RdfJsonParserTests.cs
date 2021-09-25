using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace VDS.RDF.Parsing
{
    public class RdfJsonParserTests
    {
        [Fact]
        public void ItShouldParseAnIso8601DateTimeLiteral()
        {
            var json = @"
 {
  ""http://example.com"": {
    ""http://purl.org/dc/terms/issued"": [{
        ""datatype"": ""http://www.w3.org/2001/XMLSchema#date"",
        ""type"": ""literal"",
        ""value"": ""2017-10-24T15:01:53+02:00""
      }]
    }
  }";

            IGraph g = new Graph();
            g.LoadFromString(json, new RdfJsonParser());
            var matches = g.GetTriplesWithSubjectPredicate(
                g.CreateUriNode(new Uri("http://example.com")),
                g.CreateUriNode(new Uri("http://purl.org/dc/terms/issued"))).ToList();
            matches.Should().HaveCount(1);
            matches[0].Object.Should().BeAssignableTo<ILiteralNode>().Which.Value.Should().Be("2017-10-24T15:01:53+02:00");
        }
    }
}
