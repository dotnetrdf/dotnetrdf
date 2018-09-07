using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using Xunit;
using StringWriter = VDS.RDF.Writing.StringWriter;

namespace VDS.RDF.JsonLd
{
    public class JsonLdParserTests
    {
        /// <summary>
        /// Test to replicate the StackOverflowException condition reported in GitHub issue #122
        /// when loading a remote context document.
        /// </summary>
        [Fact]
        public void TestIssue122()
        {
            var jsonLdParser = new JsonLdParser();
            ITripleStore tStore = new TripleStore();
            using (var reader = new System.IO.StringReader(@"{
            ""@context"": ""http://json-ld.org/contexts/person.jsonld"",
            ""@id"": ""http://dbpedia.org/resource/John_Lennon"",
            ""name"": ""John Lennon"",
            ""born"": ""1940-10-09"",
            ""spouse"": ""http://dbpedia.org/resource/Cynthia_Lennon""
            }"))
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

        public static IEnumerable<object[]> DateTimeValues
        {
            get
            {
                return new[] {
                    new object[] { "not_a_date", "http://www.w3.org/2001/XMLSchema#dateTime" },
                    new object[] { "2000-01-01T00:00:00", "http://www.w3.org/2001/XMLSchema#dateTime" },
                    new object[] { "2000-01-01T00:00:00-00:00", "http://www.w3.org/2001/XMLSchema#dateTime" },
                    new object[] { "2000-01-01T00:00:00+00:00", "http://www.w3.org/2001/XMLSchema#dateTime" },
                    new object[] { "2000-01-01T00:00:00-01:00", "http://www.w3.org/2001/XMLSchema#dateTime" },
                    new object[] { "2000-01-01T00:00:00+01:00", "http://www.w3.org/2001/XMLSchema#dateTime" },
                    new object[] { "not_a_date", "http://example.com/datatype" },
                    new object[] { "2000-01-01T00:00:00", "http://example.com/datatype" },
                    new object[] { "2000-01-01T00:00:00-00:00", "http://example.com/datatype" },
                    new object[] { "2000-01-01T00:00:00+00:00", "http://example.com/datatype" },
                    new object[] { "2000-01-01T00:00:00-01:00", "http://example.com/datatype" },
                    new object[] { "2000-01-01T00:00:00+01:00", "http://example.com/datatype" },
                };
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
            var jsonLd = @"
{
  '@context': [
    { '@base': 'http://example.com/' },
    'https://www.w3.org/ns/hydra/core'
  ],
  '@id': 'foo',
  'rdf:type': 'hydra:Class'
}";
            var jsonLdParser = new JsonLdParser();
            ITripleStore tStore = new TripleStore();
            using (var reader = new StringReader(jsonLd))
            {
                jsonLdParser.Load(tStore, reader);
            }
            foreach (var t in tStore.Triples)
            {
                Console.WriteLine(t.Subject);
            }
            Assert.Contains(tStore.Triples, t=>t.Subject.As<IUriNode>().Uri.ToString().Equals("http://example.com/foo"));
            
        }
    }
}
