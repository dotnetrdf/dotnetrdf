using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;

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
            var q = b.BuildQuery();

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
            var q = b.BuildQuery();

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
            var q = b.BuildQuery();

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
            var q = b.BuildQuery();

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
            var q = b.BuildQuery();

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
            var q = b.BuildQuery();

            // then
            Assert.IsNotNull(q.RootGraphPattern);
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns.Count);
            Assert.AreEqual(1, q.RootGraphPattern.ChildGraphPatterns.Count);
            Assert.IsTrue(q.RootGraphPattern.ChildGraphPatterns.Single().IsOptional);
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
            var q = b.BuildQuery();

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
            var q = b.BuildQuery();

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
                                .Filter(fb => fb.Regex(fb.Variable("title"), "^SPARQL"));
            b.Prefixes.AddNamespace("dc", new Uri("http://purl.org/dc/elements/1.1/"));

            // when
            var q = b.BuildQuery();

            // then
            Assert.IsNotNull(q.RootGraphPattern.Filter);
            Assert.IsTrue(q.RootGraphPattern.Filter.Expression is RegexFunction);
            var regex = (RegexFunction)q.RootGraphPattern.Filter.Expression;
            Assert.IsTrue(regex.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(regex.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void FilterInOptionalPattern()
        {
            // given
            var b = QueryBuilder.Select("title", "price")
                .Where(tpb => tpb.Subject("x").PredicateUri("dc:title").Object("title"))
                .Optional(opt =>
                    {
                        opt.Where(tpb => tpb.Subject("x").PredicateUri("ns:price").Object("price"));
                        opt.Filter(eb => eb.Variable("price") < eb.Constant(30));
                    });
            b.Prefixes.AddNamespace("dc", new Uri("http://purl.org/dc/elements/1.1/"));
            b.Prefixes.AddNamespace("ns", new Uri("http://example.org/ns#"));

            // when
            var q = b.BuildQuery();

            // then
            Assert.AreEqual(1, q.RootGraphPattern.ChildGraphPatterns.Count);
            Assert.IsTrue(q.RootGraphPattern.ChildGraphPatterns.Single().Filter.Expression is LessThanExpression);
        }

        [TestMethod]
        public void NotExistsFilter()
        {
            // given
            var b = QueryBuilder.Select("person")
                .Where(tpb => tpb.Subject("person").PredicateUri("rdf:type").Object<IUriNode>("foaf:Person"))
                .Filter(
                    fb =>
                        !fb.Exists(ex => ex.Where(tpb => tpb.Subject("person").PredicateUri("foaf:name").Object("name"))));
            b.Prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));

            // when
            var q = b.BuildQuery();

            // then
            Assert.IsTrue(q.RootGraphPattern.IsFiltered);
            Assert.IsTrue(q.RootGraphPattern.Filter.Expression is NotExpression);
            Assert.IsTrue(q.RootGraphPattern.Filter.Expression.Arguments.First() is ExistsFunction);
        }

        [TestMethod]
        public void MinusGraphPattern()
        {
            // given
            var b = QueryBuilder.Select("s").Distinct()
                .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                .Minus(min => min.Where(tpb => tpb.Subject("s").PredicateUri("foaf:givenName").Object<ILiteralNode>("Bob")));
            b.Prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));

            // when
            var q = b.BuildQuery();

            // then
            Assert.IsNotNull(q.RootGraphPattern);
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns.Count);
            Assert.AreEqual(1, q.RootGraphPattern.ChildGraphPatterns.Count);
            Assert.IsTrue(q.RootGraphPattern.ChildGraphPatterns.Single().IsMinus);
            Assert.AreEqual(1, q.RootGraphPattern.ChildGraphPatterns.Single().TriplePatterns.Count);
        }

        [TestMethod]
        public void FilterInsideNotExists()
        {
            // given
            var b = QueryBuilder.SelectAll()
                .Where(tpb => tpb.Subject("x").PredicateUri(":p").Object("n"))
                .Filter(fb => !fb.Exists(ex => ex.Where(tpb => tpb.Subject("x").PredicateUri(":q").Object("m"))
                                                 .Filter(inner => inner.Variable("n") != inner.Variable("m"))));
            b.Prefixes.AddNamespace("", new Uri("http://example.com/"));

            // when
            var q = b.BuildQuery();

            // then
            ExistsFunction exists = (ExistsFunction) q.RootGraphPattern.Filter.Expression.Arguments.First();
            Assert.IsTrue(exists.Arguments.Single() is GraphPatternTerm);
            Assert.IsTrue(((GraphPatternTerm)exists.Arguments.Single()).Pattern.IsFiltered);
        }
    }
}