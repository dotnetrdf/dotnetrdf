using FluentAssertions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;
using StringWriter = VDS.RDF.Writing.StringWriter;

namespace VDS.RDF.JsonLd;

public class JsonLdParserTests : IDisposable
{
    private readonly ITestOutputHelper _output;
    private WireMockServer _server;

    public JsonLdParserTests(ITestOutputHelper output)
    {
        _output = output;
        _server = WireMockServer.Start();
    }


    /// <summary>
    /// Test to replicate the StackOverflowException condition reported in GitHub issue #122
    /// when loading a remote context document.
    /// </summary>
    [Fact]
    public void TestIssue122()
    {
        var jsonLdParser = new JsonLdParser();
        ITripleStore tStore = new TripleStore();
        _server.Given(Request.Create().WithPath("/contexts/person.jsonld"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/ld+json")
                .WithBody($$"""
                    {
                      '@context': { 
                        'name': 'http://xmlns.com/foaf/0.1/name',
                        'born': { 
                          '@id': 'http://schema.org/birthDate',
                          '@type': 'http://www.w3.org/2001/XMLSchema#date'
                        },
                        'spouse': {
                          '@id': 'http://schema.org/spouse',
                          '@type': '@id'
                        }
                      }
                    }
                """));

        using (var reader = new StringReader($$"""
             {
                 "@context": "{{_server.Urls[0]}}/contexts/person.jsonld",
                 "@id": "http://dbpedia.org/resource/John_Lennon",
                 "name": "John Lennon",
                 "born": "1940-10-09",
                 "spouse": "http://dbpedia.org/resource/Cynthia_Lennon"
            }
        """))
        {
            jsonLdParser.Load(tStore, reader);
        }
        Assert.Equal(3, tStore.Triples.Count());
        Assert.Contains(tStore.Triples, x =>
            x.Subject.As<IUriNode>().Uri.ToString().Equals("http://dbpedia.org/resource/John_Lennon") &&
            x.Predicate.As<IUriNode>().Uri.ToString().Equals("http://xmlns.com/foaf/0.1/name") &&
            x.Object.As<ILiteralNode>().Value.Equals("John Lennon"));
        Assert.Contains(tStore.Triples, x =>
                x.Subject.As<IUriNode>().Uri.ToString().Equals("http://dbpedia.org/resource/John_Lennon") &&
                x.Predicate.As<IUriNode>().Uri.ToString().Equals("http://schema.org/birthDate") &&
                x.Object.As<ILiteralNode>().Value.Equals("1940-10-09") &&
                x.Object.As<ILiteralNode>().DataType.ToString().Equals("http://www.w3.org/2001/XMLSchema#date"));
        Assert.Contains(tStore.Triples, x =>
                x.Subject.As<IUriNode>().Uri.ToString().Equals("http://dbpedia.org/resource/John_Lennon") &&
                x.Predicate.As<IUriNode>().Uri.ToString().Equals("http://schema.org/spouse") &&
                x.Object.As<IUriNode>().Uri.ToString().Equals("http://dbpedia.org/resource/Cynthia_Lennon"));
    }

    [Fact]
    public void ItAddsListTriplesToTheGraphContainingTheList()
    {
        var jsonLdParser = new JsonLdParser();
        ITripleStore tripleStore = new TripleStore();
        using (var reader = new StringReader(@"{
            ""@id"": ""urn:graph:1"",
            ""@graph"": [
            {
                ""@id"": ""urn:subject:1"",
                ""urn:predicate:1"": { ""@list"": [ ""foo"", ""bar"" ]
                }
            }
            ]}"))
        {
            jsonLdParser.Load(tripleStore, reader);
        }

        IGraph defaultGraph = tripleStore.Graphs.FirstOrDefault(g=>g.Name == null);
        Assert.True(defaultGraph == null || defaultGraph.IsEmpty);
        IGraph contentGraph = tripleStore.Graphs[new UriNode(new Uri("urn:graph:1"))];
        Assert.NotNull(contentGraph);
        Assert.Equal(5, contentGraph.Triples.Count);
    }


    public static IEnumerable<TheoryDataRow<string, string>> DateTimeValues
    {
        get
        {
            return [
                new("not_a_date", "http://www.w3.org/2001/XMLSchema#dateTime"),
                new("2000-01-01T00:00:00", "http://www.w3.org/2001/XMLSchema#dateTime"),
                new("2000-01-01T00:00:00-00:00", "http://www.w3.org/2001/XMLSchema#dateTime"),
                new("2000-01-01T00:00:00+00:00", "http://www.w3.org/2001/XMLSchema#dateTime"),
                new("2000-01-01T00:00:00-01:00", "http://www.w3.org/2001/XMLSchema#dateTime"),
                new("2000-01-01T00:00:00+01:00", "http://www.w3.org/2001/XMLSchema#dateTime"),
                new("not_a_date", "http://example.com/datatype"),
                new("2000-01-01T00:00:00", "http://example.com/datatype"),
                new("2000-01-01T00:00:00-00:00", "http://example.com/datatype"),
                new("2000-01-01T00:00:00+00:00", "http://example.com/datatype"),
                new("2000-01-01T00:00:00-01:00", "http://example.com/datatype"),
                new("2000-01-01T00:00:00+01:00", "http://example.com/datatype"),
            ];
        }
    }

    [Theory]
    [MemberData(nameof(DateTimeValues))]
    public void RoundtripsDatetimeLiteralsInDiff(string dateTimeValue, string datatype)
    {
        using (var original = new TripleStore())
        {
            original.LoadFromString($@"<http://example.com/1> <http://example.com/1> ""{dateTimeValue}""^^<{datatype}>.");

            using (var target = new TripleStore())
            {
                target.LoadFromString(StringWriter.Write(original, new JsonLdWriter()), new JsonLdParser());

                Assert.True(original.Graphs.Single().Difference(target.Graphs.Single()).AreEqual);
            }
        }
    }

    [Theory]
    [MemberData(nameof(DateTimeValues))]
    public void RoundtripsDatetimeLiterals(string dateTimeValue, string datatype)
    {
        using (var store = new TripleStore())
        {
            var jsonLd = $@"
{{
    ""http://example.com/1"": {{
        ""@type"": ""{datatype}"",
        ""@value"": ""{dateTimeValue}""
    }}
}}
";
            store.LoadFromString(jsonLd, new JsonLdParser());

            var result = store.Graphs.Single().Triples.Single().Object.As<ILiteralNode>();

            Assert.Equal(dateTimeValue, result.Value);
            Assert.Equal(datatype, result.DataType.AbsoluteUri);
        }
    }

    [Theory]
    [MemberData(nameof(DateTimeValues))]
    public void RoundtripsDatetimeTypedLiterals(string dateTimeValue, string datatype)
    {
        var isValidDateTime = DateTime.TryParse(dateTimeValue, null, DateTimeStyles.AdjustToUniversal, out DateTime parsedDateTime);
        var isDateTimeDatatype = datatype == XmlSpecsHelper.XmlSchemaDataTypeDateTime;

        if (isValidDateTime && isDateTimeDatatype)
        {
            using (var store = new TripleStore())
            {
                var jsonLd = $@"
{{
    ""http://example.com/1"": {{
        ""@type"": ""{datatype}"",
        ""@value"": ""{dateTimeValue}""
    }}
}}
";
                store.LoadFromString(jsonLd, new JsonLdParser());

                var result = store.Graphs.Single().Triples.Single().Object.AsValuedNode().AsDateTime();

                Assert.Equal(parsedDateTime, result);
            }
        }
    }

    [Fact]
    public void ItRetainsLocalContextWhenProcessingARemoteContext()
    {
        _server.Given(Request.Create().WithPath("/context.jsonld"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/ld+json")
                .WithBody("{'@context': { 'foo': 'http://example.org/foo'} }"));
        var contextPath = _server.Urls[0] + "/context.jsonld";
        var jsonLd = @"
{
  '@context': [
    { '@base': 'http://example.com/' },
    '" + contextPath + @"'
  ],
  '@id': 'foo',
  'rdf:type': 'foo:Item'
}";
        var jsonLdParser = new JsonLdParser();
        ITripleStore tStore = new TripleStore();
        using (var reader = new StringReader(jsonLd))
        {
            jsonLdParser.Load(tStore, reader);
        }
        foreach (var t in tStore.Triples)
        {
            _output.WriteLine(t.Subject.ToString());
        }
        Assert.Contains(tStore.Triples, t=>t.Subject.As<IUriNode>().Uri.ToString().Equals("http://example.com/foo"));
        
    }

    [Fact]
    public void ItTreatsUrnsAsAbsoluteIris()
    {
        var jsonLd = @"
{
    '@id': 'urn:uuid:f81d4fae-7dec-11d0-a765-00a0c91e6bf6',
    'http://example.org/p': {
        '@value': 'o',
        '@type': 'urn:uuid:f81d4fae-7dec-11d0-a765-00a0c91e6bf6'
    }
}";
        var jsonLdParser = new JsonLdParser();
        ITripleStore tStore = new TripleStore();
        using (var reader = new StringReader(jsonLd))
        {
            jsonLdParser.Load(tStore, reader);
        }
        foreach (var t in tStore.Triples)
        {
            _output.WriteLine(t.ToString());
        }
        Assert.Contains(tStore.Triples, t => t.Subject is IUriNode node &&  node.Uri.ToString().Equals("urn:uuid:f81d4fae-7dec-11d0-a765-00a0c91e6bf6"));
        Assert.Contains(tStore.Triples,
            t => t.Object is ILiteralNode node && node.DataType.ToString().Equals("urn:uuid:f81d4fae-7dec-11d0-a765-00a0c91e6bf6"));
    }

    [Fact]
    public void ItShouldRaiseAWarningIfAnIdCannotBeResolveToAnIri()
    {
        var jsonLd = @"{
              '@graph': [
            {
                '@id': 'Row1',
                '@type': 'MelRow',
                'rdfs:label': 'An empty MEL Row'
            }
            ],
            '@context': {
                'rdfs': 'http://www.w3.org/2000/01/rdf-schema#',
                '@vocab': 'http://example.com/ontology/mel#',
                'sor': 'http://example.com/ontology/sor#',
                '@version': '1.1'
            }
        }";
        var warnings = new List<string>();
        var jsonLdParser = new JsonLdParser();
        jsonLdParser.Warning += message => warnings.Add(message);
        ITripleStore tStore = new TripleStore();
        using (var reader = new StringReader(jsonLd))
        {
            jsonLdParser.Load(tStore, reader);
        }

        warnings.Should().NotBeEmpty();
    }

    [Fact]
    public void FramingSupportsUrisContainingEncodedUris()
    {
        var inputJson = @"[
  {
    ""@id"": ""http://localhost:8080/outline/http%3A%2F%2Flocalhost%3A8080%2Fknowledge-graph%2Fengine_01"",
    ""@type"": [
      ""http://www.w3.org/ns/hydra/core#Resource"",
      ""http://example.org/vocab#Engine""
    ],
    ""http://example.org/vocab#id"": [
      {
        ""@value"": ""engine_01""
      }
    ]
  }
]";

        var frameJson = @"{
  ""@context"": {
    ""ex"": ""http://example.org/vocab#""
  },
  ""@id"": ""http://localhost:8080/outline/http%3A%2F%2Flocalhost%3A8080%2Fknowledge-graph%2Fengine_01""
}";

        var input = JToken.Parse(inputJson);
        var frame = JToken.Parse(frameJson);
        JObject frameResult = JsonLdProcessor.Frame(input, frame, new JsonLdProcessorOptions());
        frameResult["@id"]?.ToString().Should()
            .Be("http://localhost:8080/outline/http%3A%2F%2Flocalhost%3A8080%2Fknowledge-graph%2Fengine_01");
        frameResult["@type"]?.Children().Count().Should().Be(2);
        frameResult["ex:id"]?.ToString().Should().Be("engine_01");
        _output.WriteLine(frameResult.ToString());
    }
    /// <inheritdoc />
    public void Dispose()
    {
        _server.Stop();
    }
}
