using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class QueryBuilderTests
    {
        [TestMethod]
        public void CanCreateSelectStarQuery()
        {
            SparqlQuery q = QueryBuilder
                .SelectAll()
                .GetExecutableQuery();
            Assert.AreEqual(SparqlQueryType.SelectAll, q.QueryType);
            Assert.IsNull(q.RootGraphPattern);
        }

        [TestMethod]
        public void CanCreateSelectDistinctStarQuery()
        {
            SparqlQuery q = QueryBuilder
                .SelectAll()
                .Distinct().GetExecutableQuery();
            Assert.AreEqual(SparqlQueryType.SelectAllDistinct, q.QueryType);
            Assert.IsTrue(q.HasDistinctModifier);
            Assert.IsNull(q.RootGraphPattern);
        }

        [TestMethod]
        public void CanAddTriplePatternsWithTriplePatternBuilder()
        {
            SparqlQuery q = QueryBuilder
                .SelectAll()
                .Where(tpb => tpb.Subject("s").Predicate("p").Object("o")
                                 .Subject("s").Predicate(UriFactory.Create(RdfSpecsHelper.RdfType)).Object<IUriNode>("o")
                                 .Subject("s").PredicateUri("foaf:Name").Object("Tomasz Pluskiewicz")
                                 .Subject<IBlankNode>("bnode_id").Predicate("p").Object("o"))
                .GetExecutableQuery();
            Assert.IsNotNull(q.RootGraphPattern);
            Assert.AreEqual(4, q.RootGraphPattern.TriplePatterns.Count());
        }

        [TestMethod]
        public void CanAddTriplePatternsAsObjects()
        {
            // given
            TriplePattern p1 = new TriplePattern(new VariablePattern("s"), new VariablePattern("p"), new VariablePattern("o"));
            TriplePattern p2 = new TriplePattern(new VariablePattern("s"), new VariablePattern("p"), new VariablePattern("o"));

            // when
            var q = QueryBuilder.SelectAll().Where(p1, p2).GetExecutableQuery();

            // then
            Assert.IsNotNull(q.RootGraphPattern);
            Assert.AreEqual(2, q.RootGraphPattern.TriplePatterns.Count());
            Assert.IsTrue(q.RootGraphPattern.TriplePatterns.Contains(p1));
            Assert.IsTrue(q.RootGraphPattern.TriplePatterns.Contains(p2));
        }

        [TestMethod]
        public void AddingTriplePatternsCallDelegateOnlyOnce()
        {
            // given
            int callCount = 0;

            // when
            QueryBuilder.SelectAll()
                        .Where(tpb =>
                            {
                                callCount++;
                                tpb.Subject("s").Predicate("p").Object("o")
                                   .Subject("s").Predicate("p").Object("o");
                            });

            // then
            Assert.AreEqual(1, callCount);
        }

        [TestMethod]
        public void CanAddOptionalTriplePatterns()
        {
            // given
            IQueryBuilder builder = QueryBuilder.SelectAll();
            builder.Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                   .Optional(tpb => tpb.Subject("s").Predicate(UriFactory.Create(RdfSpecsHelper.RdfType)).Object("type"));

            // when
            var q = builder.GetExecutableQuery();

            // then
            Assert.IsNotNull(q.RootGraphPattern);
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns.Count());
            Assert.AreEqual(1, q.RootGraphPattern.ChildGraphPatterns.Count);
            Assert.IsTrue(q.RootGraphPattern.ChildGraphPatterns[0].IsOptional);
        }

        [TestMethod]
        public void CanAddOptionalTriplePatternsAsObjects()
        {
            // given
            TriplePattern p1 = new TriplePattern(new VariablePattern("s"), new VariablePattern("p"), new VariablePattern("o"));
            TriplePattern p2 = new TriplePattern(new VariablePattern("s"), new VariablePattern("p"), new VariablePattern("o"));

            // when
            var q = QueryBuilder.SelectAll().Where(p1).Optional(p2).GetExecutableQuery();

            // then
            Assert.IsNotNull(q.RootGraphPattern);
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns.Count());
            Assert.AreEqual(1, q.RootGraphPattern.ChildGraphPatterns.Count);
            Assert.IsTrue(q.RootGraphPattern.TriplePatterns.Contains(p1));
            Assert.IsTrue(q.RootGraphPattern.ChildGraphPatterns.Single().TriplePatterns.Contains(p2));
        }

        [TestMethod]
        public void CanAddMultipleChildGraphPatterns()
        {
            // given
            IQueryBuilder builder = QueryBuilder.SelectAll();
            builder.Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                   .Optional(tpb => tpb.Subject("s").Predicate(UriFactory.Create(RdfSpecsHelper.RdfType)).Object("type"))
                   .Where(tpb => tpb.Subject("x").Predicate("y").Object("z"));

            // when
            var q = builder.GetExecutableQuery();

            // then
            Assert.IsNotNull(q.RootGraphPattern);
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns.Count());
            Assert.AreEqual(2, q.RootGraphPattern.ChildGraphPatterns.Count);
            Assert.IsTrue(q.RootGraphPattern.ChildGraphPatterns[0].IsOptional);
            Assert.AreEqual(1, q.RootGraphPattern.ChildGraphPatterns[1].TriplePatterns.Count());
        }

        [TestMethod]
        public void GetExectuableQueryReturnsNewInstance()
        {
            // given
            IQueryBuilder builder = QueryBuilder.SelectAll().Where(tpb => tpb.Subject("s").Predicate("p").Object("o"));

            // when
            SparqlQuery query1 = builder.GetExecutableQuery();
            SparqlQuery query2 = builder.GetExecutableQuery();

            // then
            Assert.AreNotSame(query1, query2);
        }

        [TestMethod]
        public void CanStartQueryWithGivenVariablesStrings()
        {
            // given
            IQueryBuilder queryBuilder = QueryBuilder.Select("s", "p", "o");

            // when
            SparqlQuery query = queryBuilder.GetExecutableQuery();

            // then
            Assert.AreEqual(3, query.Variables.Count());
            Assert.AreEqual(1, query.Variables.Count(v => v.Name == "s"));
            Assert.AreEqual(1, query.Variables.Count(v => v.Name == "p"));
            Assert.AreEqual(1, query.Variables.Count(v => v.Name == "o"));
            Assert.IsTrue(query.Variables.All(var => var.IsResultVariable));
        }

        [TestMethod]
        public void CanStartQueryWithGivenVariables()
        {
            // given
            var s = new SparqlVariable("s", true);
            var p = new SparqlVariable("p", true);
            var o = new SparqlVariable("o", true);
            IQueryBuilder queryBuilder = QueryBuilder.Select(s, p, o);

            // when
            SparqlQuery query = queryBuilder.GetExecutableQuery();

            // then
            Assert.AreEqual(3, query.Variables.Count());
            Assert.IsTrue(query.Variables.Contains(s));
            Assert.IsTrue(query.Variables.Contains(p));
            Assert.IsTrue(query.Variables.Contains(o));
        }
    }
}
