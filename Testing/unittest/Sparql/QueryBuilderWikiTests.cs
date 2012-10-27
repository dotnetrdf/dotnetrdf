using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;

namespace VDS.RDF.Test.Sparql
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
    }
}