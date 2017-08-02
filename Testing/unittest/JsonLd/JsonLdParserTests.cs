using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using VDS.RDF.Parsing;
using Xunit;

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
            Assert.True(tStore.Triples.Any(
                x=>
                x.Subject.As<IUriNode>().Uri.ToString().Equals("http://dbpedia.org/resource/John_Lennon") &&
                x.Predicate.As<IUriNode>().Uri.ToString().Equals("http://xmlns.com/foaf/0.1/name") &&
                x.Object.As<ILiteralNode>().Value.Equals("John Lennon")));
            Assert.True(tStore.Triples.Any(
                x =>
                    x.Subject.As<IUriNode>().Uri.ToString().Equals("http://dbpedia.org/resource/John_Lennon") &&
                    x.Predicate.As<IUriNode>().Uri.ToString().Equals("http://schema.org/birthDate") &&
                    x.Object.As<ILiteralNode>().Value.Equals("1940-10-09") &&
                    x.Object.As<ILiteralNode>().DataType.ToString().Equals("http://www.w3.org/2001/XMLSchema#dateTime")));
            Assert.True(tStore.Triples.Any(
                x =>
                    x.Subject.As<IUriNode>().Uri.ToString().Equals("http://dbpedia.org/resource/John_Lennon") &&
                    x.Predicate.As<IUriNode>().Uri.ToString().Equals("http://schema.org/spouse") &&
                    x.Object.As<IUriNode>().Uri.ToString().Equals("http://dbpedia.org/resource/Cynthia_Lennon")));
        }
    }
}
