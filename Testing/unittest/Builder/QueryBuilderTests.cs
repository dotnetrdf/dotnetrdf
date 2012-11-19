using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Test.Builder
{
    [TestClass]
    public class QueryBuilderTests
    {
        [TestMethod]
        public void CanCreateSelectStarQuery()
        {
            SparqlQuery q = QueryBuilder
                .SelectAll()
                .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                .BuildQuery();
            Assert.AreEqual(SparqlQueryType.SelectAll, q.QueryType);
            Assert.IsNotNull(q.RootGraphPattern);
        }

        [TestMethod]
        public void CanCreateSelectDistinctStarQuery()
        {
            SparqlQuery q = QueryBuilder
                .SelectAll()
                .Distinct()
                .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                .BuildQuery();
            Assert.AreEqual(SparqlQueryType.SelectAllDistinct, q.QueryType);
            Assert.IsTrue(q.HasDistinctModifier);
            Assert.IsNotNull(q.RootGraphPattern);
        }

        [TestMethod]
        public void CanAddTriplePatternsWithTriplePatternBuilder()
        {
            var builder = QueryBuilder.SelectAll();
            builder.Prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));

            var q = builder.Where(tpb => tpb.Subject("s").Predicate("p").Object("o")
                                            .Subject("s").PredicateUri(UriFactory.Create(RdfSpecsHelper.RdfType)).Object<IUriNode>("foaf:Person")
                                            .Subject("s").PredicateUri("foaf:Name").Object("Tomasz Pluskiewicz")
                                            .Subject<IBlankNode>("bnode_id").Predicate("p").Object("o"))
                .BuildQuery();
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
            var q = QueryBuilder.SelectAll().Where(p1, p2).BuildQuery();

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
                            })
                        .BuildQuery();

            // then
            Assert.AreEqual(1, callCount);
        }

        [TestMethod]
        public void CanAddOptionalTriplePatterns()
        {
            // given
            var builder = QueryBuilder.SelectAll()
                                      .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                                      .Optional(gpb => gpb.Where(tpb => tpb.Subject("s")
                                                                           .PredicateUri(UriFactory.Create(RdfSpecsHelper.RdfType))
                                                                           .Object("type")));

            // when
            var q = builder.BuildQuery();

            // then
            Assert.IsNotNull(q.RootGraphPattern);
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns.Count());
            Assert.AreEqual(1, q.RootGraphPattern.ChildGraphPatterns.Count);
            Assert.IsTrue(q.RootGraphPattern.ChildGraphPatterns[0].IsOptional);
        }

        [TestMethod]
        public void CanAddMultipleChildGraphPatterns()
        {
            // given
            var builder = QueryBuilder.SelectAll()
                                      .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                                      .Optional(gpb => gpb.Where(tpb => tpb.Subject("s")
                                                                           .PredicateUri(UriFactory.Create(RdfSpecsHelper.RdfType))
                                                                           .Object("type")))
                                      .Where(tpb => tpb.Subject("x").Predicate("y").Object("z"));

            // when
            var q = builder.BuildQuery();

            // then
            Assert.IsNotNull(q.RootGraphPattern);
            Assert.AreEqual(2, q.RootGraphPattern.TriplePatterns.Count());
            Assert.AreEqual(1, q.RootGraphPattern.ChildGraphPatterns.Count);
            Assert.IsTrue(q.RootGraphPattern.ChildGraphPatterns[0].IsOptional);
            Assert.AreEqual(1, q.RootGraphPattern.ChildGraphPatterns[0].TriplePatterns.Count());
        }

        [TestMethod]
        public void GetExectuableQueryReturnsNewInstance()
        {
            // given
            IQueryBuilder builder = QueryBuilder.SelectAll().Where(tpb => tpb.Subject("s").Predicate("p").Object("o"));

            // when
            SparqlQuery query1 = builder.BuildQuery();
            SparqlQuery query2 = builder.BuildQuery();

            // then
            Assert.AreNotSame(query1, query2);
        }

        [TestMethod]
        public void CanStartQueryWithGivenVariablesStrings()
        {
            // given
            IQueryBuilder queryBuilder = QueryBuilder.Select("s", "p", "o")
                                                     .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"));

            // when
            SparqlQuery query = queryBuilder.BuildQuery();

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
            IQueryBuilder queryBuilder = QueryBuilder.Select(s, p, o)
                                                     .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"));

            // when
            SparqlQuery query = queryBuilder.BuildQuery();

            // then
            Assert.AreEqual(3, query.Variables.Count());
            Assert.IsTrue(query.Variables.Contains(s));
            Assert.IsTrue(query.Variables.Contains(p));
            Assert.IsTrue(query.Variables.Contains(o));
        }

        [TestMethod]
        public void CanAddOptionalGraphPatternsAfterRegular()
        {
            // given
            var builder = QueryBuilder.SelectAll()
                .Where(tpb => tpb.Subject(new Uri("http://example.com/Resource"))
                                 .PredicateUri(new Uri(RdfSpecsHelper.RdfType))
                                 .Object(new Uri("http://example.com/Class")))
                .Optional(gpb => gpb.Where(tpb => tpb.Subject("s").Predicate("p").Object(new Uri("http://example.com/Resource"))
                                                     .Subject(new Uri("http://example.com/Resource")).Predicate("p").Object("o")));

            // when
            var query = builder.BuildQuery();

            // then
            Assert.IsNotNull(query.RootGraphPattern);
            Assert.AreEqual(1, query.RootGraphPattern.TriplePatterns.Count);
            Assert.AreEqual(1, query.RootGraphPattern.ChildGraphPatterns.Count);
            Assert.AreEqual(2, query.RootGraphPattern.ChildGraphPatterns.Single().TriplePatterns.Count);
        }

        [TestMethod]
        public void CanAddOptionalGraphPatternsBeforeRegular()
        {
            // given
            var builder = QueryBuilder.SelectAll()
                .Optional(gpb => gpb.Where(tpb => tpb.Subject("s").Predicate("p").Object(new Uri("http://example.com/Resource"))
                                                     .Subject(new Uri("http://example.com/Resource")).Predicate("p").Object("o")))
                .Where(tpb => tpb.Subject(new Uri("http://example.com/Resource"))
                                 .PredicateUri(new Uri(RdfSpecsHelper.RdfType))
                                 .Object(new Uri("http://example.com/Class")));

            // when
            var query = builder.BuildQuery();

            // then
            Assert.IsNotNull(query.RootGraphPattern);
            Assert.AreEqual(1, query.RootGraphPattern.TriplePatterns.Count);
            Assert.AreEqual(1, query.RootGraphPattern.ChildGraphPatterns.Count);
            Assert.AreEqual(2, query.RootGraphPattern.ChildGraphPatterns.Single().TriplePatterns.Count);
        }

        [TestMethod]
        public void SubsequentWhereCallsShouldAddToRootGraphPattern()
        {
            // given
            var builder = QueryBuilder.SelectAll()
                .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"));

            // when
            var query = builder.BuildQuery();
            // then
            Assert.IsNotNull(query.RootGraphPattern);
            Assert.AreEqual(4, query.RootGraphPattern.TriplePatterns.Count);
            Assert.AreEqual(0, query.RootGraphPattern.ChildGraphPatterns.Count);
        }

        [TestMethod]
        public void CanAddMultipleSelectVariablesOneByOne()
        {
            // given
            var b = QueryBuilder.Select("s").And("p").And("o")
                                .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"));

            // when
            var q = b.BuildQuery();

            // then
            Assert.IsNotNull(q.RootGraphPattern);
            Assert.AreEqual(3, q.Variables.Count(v => v.IsResultVariable));
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns.Count());
        }

        [TestMethod]
        public void ShouldEnsureSparqlVariablesAreReturnVariables()
        {
            // when
            var q = QueryBuilder.Select(new SparqlVariable("s"), new SparqlVariable("p"), new SparqlVariable("o"))
                                .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                                .BuildQuery();

            // then
            Assert.AreEqual(3, q.Variables.Count(v => v.IsResultVariable));
        }

        [TestMethod]
        public void ShouldBeCreatedWithEmptyNamespaceMap()
        {
            // when
            ISelectQueryBuilder builder = QueryBuilder.SelectAll();

            // then
            Assert.AreEqual(0, builder.Prefixes.Prefixes.Count());
        }

        [TestMethod]
        public void CanCreateSelectQueryWithExpressionFirst()
        {
            // when
            SparqlQuery q = QueryBuilder
                .Select(eb => eb.IsIRI(eb.Variable("o"))).As("isIri").And("o")
                .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                .BuildQuery();

            // then
            Assert.AreEqual(SparqlQueryType.Select, q.QueryType);
            Assert.AreEqual(2, q.Variables.Count());
            Assert.AreEqual(1, q.Variables.Count(v => v.IsProjection && v.IsResultVariable && v.Name == "isIri"));
            Assert.AreEqual(1, q.Variables.Count(v => !v.IsProjection && v.IsResultVariable && v.Name == "o"));
        }

        [TestMethod]
        public void ShouldAllowUsingISparqlExpressionForFilter()
        {
            // given
            ISparqlExpression expression = new IsIriFunction(new VariableTerm("x"));
            var b = QueryBuilder.SelectAll().Filter(expression);

            // when
            var q = b.BuildQuery();

            // then
            Assert.IsTrue(q.RootGraphPattern.IsFiltered);
            Assert.AreSame(expression, q.RootGraphPattern.Filter.Expression);
        }

        [TestMethod]
        public void ShouldAllowAddingLimit()
        {
            // when
            SparqlQuery q = QueryBuilder.SelectAll()
                .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                .Limit(5)
                .BuildQuery();

            // then
            Assert.AreEqual(5, q.Limit);
            Assert.AreEqual(0, q.Offset);
        }

        [TestMethod]
        public void ShouldAllowAddingNegativeLimit()
        {
            // when
            SparqlQuery q = QueryBuilder.SelectAll()
                .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                .Limit(5).Limit(-10)
                .BuildQuery();

            // then
            Assert.AreEqual(-1, q.Limit);
            Assert.AreEqual(0, q.Offset);
        }

        [TestMethod]
        public void ShouldAllowAddingOffset()
        {
            // when
            SparqlQuery q = QueryBuilder.SelectAll()
                .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                .Offset(10)
                .BuildQuery();

            // then
            Assert.AreEqual(-1, q.Limit);
            Assert.AreEqual(10, q.Offset);
        }

        [TestMethod]
        public void ShouldAllowAddingLimitMultipleTimes()
        {
            // given
            int limit = 5;

            // when
            SparqlQuery q = QueryBuilder.SelectAll()
                .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                .Limit(limit++).Limit(limit++).Limit(limit++).Limit(limit++).Limit(limit)
                .BuildQuery();

            // then
            Assert.AreEqual(limit, q.Limit);
            Assert.AreEqual(0, q.Offset);
        }

        [TestMethod]
        public void ShouldAllowAddingOffsetMultipleTimes()
        {
            // given
            int offset = 5;

            // when
            SparqlQuery q = QueryBuilder.SelectAll()
                .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                .Offset(offset++).Offset(offset++).Offset(offset++).Offset(offset++).Offset(offset)
                .BuildQuery();

            // then
            Assert.AreEqual(-1, q.Limit);
            Assert.AreEqual(offset, q.Offset);
        }

        [TestMethod]
        public void ShouldAllowAddingLimitAndOffset()
        {
            // when
            SparqlQuery q = QueryBuilder.SelectAll()
                .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                .Limit(5).Offset(10)
                .BuildQuery();

            // then
            Assert.AreEqual(5, q.Limit);
            Assert.AreEqual(10, q.Offset);
        }

        [TestMethod]
        public void ShouldAllowBuildingAskQueries()
        {
            // when
            SparqlQuery q = QueryBuilder.Ask()
                                        .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                                        .BuildQuery();

            // then
            Assert.AreEqual(SparqlQueryType.Ask, q.QueryType);
        }

        [TestMethod]
        public void ShouldAllowBuildingConstructQueries()
        {
            // when
            SparqlQuery q = QueryBuilder.Construct(gpb => gpb.Where(tpb => tpb.Subject("o").Predicate("p").Object("s")))
                                        .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                                        .BuildQuery();

            // then
            Assert.AreEqual(SparqlQueryType.Construct, q.QueryType);
            Assert.IsNotNull(q.ConstructTemplate);
            Assert.AreEqual(1, q.ConstructTemplate.TriplePatterns.Count);
        }
    }
}
