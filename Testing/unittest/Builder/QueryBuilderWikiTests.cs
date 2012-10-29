using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;

namespace VDS.RDF.Test.Builder
{
    /// <summary>
    /// Test class for queries used on the <a href="https://bitbucket.org/romanticweb/dotnetrdf/wiki">wiki</a>, so that it stays updated
    /// </summary>
    [TestClass]
    public class QueryBuilderWikiTests
    {
        [TestMethod]
        public void SimpleSelectWithOneGraphPatternAndReturnVariables()
        {
            // given
            var b = QueryBuilder.Select("name", "mbox")
                                .Where(tpb => tpb.Subject("x").PredicateUri("foaf:name").Object("name")
                                                 .Subject("x").PredicateUri("foaf:mbox").Object("mbox"));
            b.Prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));

            // when
            var q = b.GetExecutableQuery();

            // then
            Assert.IsNotNull(q.RootGraphPattern);
            Assert.AreEqual(2, q.Variables.Count());
            Assert.IsTrue(q.Variables.All(v => v.IsResultVariable));
            Assert.AreEqual(2, q.RootGraphPattern.TriplePatterns.Count);
        }

        [TestMethod]
        public void SimpleSelectWithOneGraphPatternAndReturnVariablesAlternative()
        {
            // given
            var b = QueryBuilder.Select("name", "mbox")
                                .Where(tpb => tpb.Subject("x").PredicateUri("foaf:name").Object("name"))
                                .Where(tpb => tpb.Subject("x").PredicateUri("foaf:mbox").Object("mbox"));
            b.Prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));

            // when
            var q = b.GetExecutableQuery();

            // then
            Assert.IsNotNull(q.RootGraphPattern);
            Assert.AreEqual(2, q.Variables.Count());
            Assert.IsTrue(q.Variables.All(v => v.IsResultVariable));
            Assert.AreEqual(2, q.RootGraphPattern.TriplePatterns.Count);
        }

        [TestMethod]
        public void DescribeWithoutGraphPattern()
        {
            // given
            const string uriString = "http://example.org/";
            var b = QueryBuilder.Describe(new Uri(uriString));

            // when
            var q = b.GetExecutableQuery();

            // then
            Assert.IsNull(q.RootGraphPattern);
            Assert.IsTrue(q.QueryType == SparqlQueryType.Describe);
            Assert.AreEqual(1, q.DescribeVariables.Count());
            Assert.AreEqual(uriString, q.DescribeVariables.Single().Value);
        }

        [TestMethod]
        public void DescribeWithGraphPattern()
        {
            // given
            var b = QueryBuilder.Describe("x")
                                .Where(tpb => tpb.Subject("x").PredicateUri("foaf:mbox").Object(new Uri("mailto:alice@org")));
            b.Prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));

            // when
            var q = b.GetExecutableQuery();

            // then
            Assert.IsNotNull(q.RootGraphPattern);
            Assert.AreEqual("x", q.DescribeVariables.Single().Value);
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns.Count);
        }

        [TestMethod]
        public void DescribeWithMixedVariablesAndUrIs()
        {
            // given
            var b = QueryBuilder.Describe("x").And("y").And(new Uri("http://example.org/"))
                                              .Where(tpb => tpb.Subject("x").PredicateUri("foaf:knows").Object("y"));
            b.Prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));

            // when
            var q = b.GetExecutableQuery();

            // then
            Assert.IsNotNull(q.RootGraphPattern);
            Assert.AreEqual("x", q.DescribeVariables.ElementAt(0).Value);
            Assert.AreEqual("y", q.DescribeVariables.ElementAt(1).Value);
            Assert.AreEqual("http://example.org/", q.DescribeVariables.ElementAt(2).Value);
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns.Count);
        }

        [TestMethod]
        public void SimpleOptional()
        {
            // given
            var b = QueryBuilder.Select("name", "mbox")
                                .Where(tpb => tpb.Subject("x").PredicateUri("foaf:name").Object("name"))
                                .Optional(opt => opt.Where(tbp => tbp.Subject("x").PredicateUri("foaf:mbox").Object("mbox")));
            b.Prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));

            // when
            var q = b.GetExecutableQuery();

            // then
            Assert.IsNotNull(q.RootGraphPattern);
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns.Count);
            Assert.AreEqual(1, q.RootGraphPattern.ChildGraphPatterns.Count);
            Assert.AreEqual(1, q.RootGraphPattern.ChildGraphPatterns.Single().TriplePatterns.Count);
        }

        [TestMethod]
        public void MultipleOptionals()
        {
            // given
            var b = QueryBuilder.Select("name", "mbox", "hpage")
                .Where(tpb => tpb.Subject("x").PredicateUri("foaf:name").Object("name"))
                .Optional(opt => opt.Where(tbp => tbp.Subject("x").PredicateUri("foaf:mbox").Object("mbox")))
                .Optional(opt => opt.Where(tbp => tbp.Subject("x").PredicateUri("foaf:homepage").Object("hpage")));
            b.Prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));

            // when
            var q = b.GetExecutableQuery();

            // then
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns.Count);
            Assert.AreEqual(2, q.RootGraphPattern.ChildGraphPatterns.Count);
            Assert.AreEqual(1, q.RootGraphPattern.ChildGraphPatterns.ElementAt(0).TriplePatterns.Count);
            Assert.AreEqual(1, q.RootGraphPattern.ChildGraphPatterns.ElementAt(1).TriplePatterns.Count);
        }

        [TestMethod]
        public void MultipleWheresInOptional()
        {
            // given
            var b = QueryBuilder.Select("name", "mbox", "hpage")
                .Where(tpb => tpb.Subject("x").PredicateUri("foaf:name").Object("name"))
                .Optional(opt => opt.Where(tbp => tbp.Subject("x").PredicateUri("foaf:mbox").Object("mbox"))
                                    .Where(tbp => tbp.Subject("x").PredicateUri("foaf:homepage").Object("hpage")));
            b.Prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));

            // when
            var q = b.GetExecutableQuery();

            // then
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns.Count);
            Assert.AreEqual(1, q.RootGraphPattern.ChildGraphPatterns.Count);
            Assert.AreEqual(2, q.RootGraphPattern.ChildGraphPatterns.Single().TriplePatterns.Count);
        }

        [TestMethod]
        public void FilterInRootGraphPattern()
        {
            // given
            var b = QueryBuilder.Select("name", "mbox")
                                .Where(tpb => tpb.Subject("x").PredicateUri("dc:title").Object("title"))
                                .Filter(fb => fb.Regex("title", "^SPARQL"));

            // when
            var q = b.GetExecutableQuery();

            // then
            Assert.IsNotNull(q.RootGraphPattern.Filter);
        }
    }
}