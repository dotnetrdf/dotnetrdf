/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Linq;
using NUnit.Framework;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Ordering;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    [TestFixture]
    public class QueryBuilderTests
    {
        [Test]
        public void CanCreateSelectStarQuery()
        {
            SparqlQuery q = QueryBuilder
                .SelectAll()
                .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                .BuildQuery();
            Assert.AreEqual(SparqlQueryType.SelectAll, q.QueryType);
            Assert.IsNotNull(q.RootGraphPattern);
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void ShouldEnsureSparqlVariablesAreReturnVariables()
        {
            // when
            var q = QueryBuilder.Select(new SparqlVariable("s"), new SparqlVariable("p"), new SparqlVariable("o"))
                                .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                                .BuildQuery();

            // then
            Assert.AreEqual(3, q.Variables.Count(v => v.IsResultVariable));
        }

        [Test]
        public void ShouldBeCreatedWithEmptyNamespaceMap()
        {
            // when
            IQueryBuilder builder = QueryBuilder.SelectAll().Where();

            // then
            Assert.AreEqual(0, builder.Prefixes.Prefixes.Count());
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void ShouldAllowBuildingAskQueries()
        {
            // when
            SparqlQuery q = QueryBuilder.Ask()
                                        .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                                        .BuildQuery();

            // then
            Assert.AreEqual(SparqlQueryType.Ask, q.QueryType);
        }

        [Test]
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

        [Test]
        public void ShouldAllowBuildingConstructQueriesWithNullBuilderFunction()
        {
            // when
            SparqlQuery q = QueryBuilder.Construct(null)
                                        .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                                        .BuildQuery();

            // then
            Assert.AreEqual(SparqlQueryType.Construct, q.QueryType);
            Assert.IsNull(q.ConstructTemplate);
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns.Count);
        }

        [Test]
        public void ShouldAllowBuildingConstructQueriesWithoutBuilderFunction()
        {
            // when
            SparqlQuery q = QueryBuilder.Construct()
                                        .Where(BuildSPOPattern)
                                        .BuildQuery();

            // then
            Assert.AreEqual(SparqlQueryType.Construct, q.QueryType);
            Assert.IsNull(q.ConstructTemplate);
            Assert.AreEqual(1, q.RootGraphPattern.TriplePatterns.Count);
        }

        private static void BuildSPOPattern(ITriplePatternBuilder tpb)
        {
            tpb.Subject("s").Predicate("p").Object("o");
        }

        [Test]
        public void ShouldAllowOrderingQueryByVariableAscending()
        {
            // when
            SparqlQuery sparqlQuery = QueryBuilder.SelectAll()
                                                  .Where(BuildSPOPattern)
                                                  .OrderBy("s")
                                                  .BuildQuery();

            // then
            Assert.IsNotNull(sparqlQuery.OrderBy);
            Assert.IsNull(sparqlQuery.OrderBy.Child);
            Assert.IsTrue(sparqlQuery.OrderBy is OrderByVariable);
            Assert.IsFalse(sparqlQuery.OrderBy.Descending);
            Assert.AreEqual("s", (sparqlQuery.OrderBy as OrderByVariable).Variables.Single());
        }

        [Test]
        public void ShouldAllowOrderingQueryByVariableDescending()
        {
            // when
            SparqlQuery sparqlQuery = QueryBuilder.SelectAll()
                                                  .Where(BuildSPOPattern)
                                                  .OrderByDescending("s")
                                                  .BuildQuery();

            // then
            Assert.IsNotNull(sparqlQuery.OrderBy);
            Assert.IsNull(sparqlQuery.OrderBy.Child);
            Assert.IsTrue(sparqlQuery.OrderBy is OrderByVariable);
            Assert.IsTrue(sparqlQuery.OrderBy.Descending);
            Assert.AreEqual("s", (sparqlQuery.OrderBy as OrderByVariable).Variables.Single());
        }

        [Test]
        public void ShouldAllowOrderingQueryByExpressionAscending()
        {
            // when
            SparqlQuery sparqlQuery = QueryBuilder.SelectAll()
                                                  .Where(BuildSPOPattern)
                                                  .OrderBy(expr => expr.Str(expr.Variable("s"))) 
                                                  .BuildQuery();

            // then
            Assert.IsNotNull(sparqlQuery.OrderBy);
            Assert.IsNull(sparqlQuery.OrderBy.Child);
            Assert.IsTrue(sparqlQuery.OrderBy is OrderByExpression);
            Assert.IsFalse(sparqlQuery.OrderBy.Descending);
            Assert.IsTrue((sparqlQuery.OrderBy as OrderByExpression).Expression is StrFunction);
        }

        [Test]
        public void ShouldAllowOrderingQueryByExpressionDescending()
        {
            // when
            SparqlQuery sparqlQuery = QueryBuilder.SelectAll()
                                                  .Where(BuildSPOPattern)
                                                  .OrderByDescending(expr => expr.Str(expr.Variable("s")))
                                                  .BuildQuery();

            // then
            Assert.IsNotNull(sparqlQuery.OrderBy);
            Assert.IsNull(sparqlQuery.OrderBy.Child);
            Assert.IsTrue(sparqlQuery.OrderBy is OrderByExpression);
            Assert.IsTrue(sparqlQuery.OrderBy.Descending);
            Assert.IsTrue((sparqlQuery.OrderBy as OrderByExpression).Expression is StrFunction);
        }

        [Test]
        public void ShouldAllowChainingMultipleVariableAndExpressionOrderings()
        {
            // when
            SparqlQuery sparqlQuery = QueryBuilder.SelectAll()
                                                  .Where(BuildSPOPattern)
                                                  .OrderBy("s")
                                                  .OrderByDescending("p")
                                                  .OrderBy("o")
                                                  .OrderByDescending(expr => expr.Str(expr.Variable("s")))
                                                  .OrderBy(expr => expr.Str(expr.Variable("p")))
                                                  .OrderByDescending(expr => expr.Str(expr.Variable("o")))
                                                  .BuildQuery();

            // then
            ISparqlOrderBy currentOrdering = sparqlQuery.OrderBy;

            for (int i = 0; i < 6; i++)
            {
                Assert.IsNotNull(currentOrdering);
                currentOrdering = currentOrdering.Child;
            }
            Assert.IsNull(currentOrdering);
        }

        [Test]
        public void CanBuildUnionChildPattern()
        {
            // when
            var query = QueryBuilder.SelectAll()
                .Child(root =>
                    root.Where(BuildSPOPattern)
                        .Union(second => second.Where(BuildSPOPattern))
                        .Union(third => third.Where(BuildSPOPattern)))
                .BuildQuery();

            // then
            var union = query.RootGraphPattern.ChildGraphPatterns.Single();
            Assert.That(union.IsUnion);
            Assert.That(union.ChildGraphPatterns, Has.Count.EqualTo(3));
        }

        [Test]
        public void CanBuildUnionOfMixedPatterns()
        {
            // when
            var query = QueryBuilder.SelectAll()
                .Child(root =>
                    root.Service(new Uri("http://example.com"), service => service.Where(BuildSPOPattern))
                        .Union(second => second.Graph("s", graph => graph.Where(BuildSPOPattern))))
                .BuildQuery();

            // then
            var union = query.RootGraphPattern.ChildGraphPatterns.Single();
            Assert.That(union.IsUnion);
            Assert.That(union.ChildGraphPatterns, Has.Count.EqualTo(2));
            Assert.That(union.ChildGraphPatterns[0].IsService);
            Assert.That(union.ChildGraphPatterns[1].IsGraph);
        }

        [Test]
        public void ShouldRemoveUnionedTriplePatternsFromRootGraphPattern()
        {
            // when
            var query = QueryBuilder.SelectAll()
                .Child(root =>
                    root.Where(BuildSPOPattern)
                        .Union(second => second.Where(BuildSPOPattern)))
                .BuildQuery();

            // then
            Assert.That(query.RootGraphPattern.TriplePatterns, Is.Empty);
        }
    }
}
