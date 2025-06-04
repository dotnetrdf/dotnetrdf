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
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Ordering;
using VDS.RDF.Query.Patterns;
using Xunit;

namespace VDS.RDF.Query.Builder;


public class QueryBuilderTests
{
    [Fact]
    public void CanCreateSelectStarQuery()
    {
        SparqlQuery q = QueryBuilder
            .SelectAll()
            .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
            .BuildQuery();
        Assert.Equal(SparqlQueryType.SelectAll, q.QueryType);
        Assert.NotNull(q.RootGraphPattern);
    }

    [Fact]
    public void CanCreateSelectDistinctStarQuery()
    {
        SparqlQuery q = QueryBuilder
            .SelectAll()
            .Distinct()
            .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
            .BuildQuery();
        Assert.Equal(SparqlQueryType.SelectAllDistinct, q.QueryType);
        Assert.True(q.HasDistinctModifier);
        Assert.NotNull(q.RootGraphPattern);
    }

    [Fact]
    public void CanAddTriplePatternsWithTriplePatternBuilder()
    {
        var builder = QueryBuilder.SelectAll();
        builder.Prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));

        var q = builder.Where(tpb => tpb.Subject("s").Predicate("p").Object("o")
                                        .Subject("s").PredicateUri(UriFactory.Root.Create(RdfSpecsHelper.RdfType)).Object<IUriNode>("foaf:Person")
                                        .Subject("s").PredicateUri("foaf:Name").Object("Tomasz Pluskiewicz")
                                        .Subject<IBlankNode>("bnode_id").Predicate("p").Object("o"))
            .BuildQuery();
        Assert.NotNull(q.RootGraphPattern);
        Assert.Equal(4, q.RootGraphPattern.TriplePatterns.Count());
    }

    [Fact]
    public void CanAddTriplePatternsAsObjects()
    {
        // given
        var p1 = new TriplePattern(new VariablePattern("s"), new VariablePattern("p"), new VariablePattern("o"));
        var p2 = new TriplePattern(new VariablePattern("s"), new VariablePattern("p"), new VariablePattern("o"));

        // when
        var q = QueryBuilder.SelectAll().Where(p1, p2).BuildQuery();

        // then
        Assert.NotNull(q.RootGraphPattern);
        Assert.Equal(2, q.RootGraphPattern.TriplePatterns.Count());
        Assert.Contains(p1, q.RootGraphPattern.TriplePatterns);
        Assert.Contains(p2, q.RootGraphPattern.TriplePatterns);
    }

    [Fact]
    public void AddingTriplePatternsCallDelegateOnlyOnce()
    {
        // given
        var callCount = 0;

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
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void CanAddOptionalTriplePatterns()
    {
        // given
        var builder = QueryBuilder.SelectAll()
                                  .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                                  .Optional(gpb => gpb.Where(tpb => tpb.Subject("s")
                                                                       .PredicateUri(UriFactory.Root.Create(RdfSpecsHelper.RdfType))
                                                                       .Object("type")));

        // when
        var q = builder.BuildQuery();

        // then
        Assert.NotNull(q.RootGraphPattern);
        Assert.Single(q.RootGraphPattern.TriplePatterns);
        Assert.Single(q.RootGraphPattern.ChildGraphPatterns);
        Assert.True(q.RootGraphPattern.ChildGraphPatterns[0].IsOptional);
    }

    [Fact]
    public void CanAddMultipleChildGraphPatterns()
    {
        // given
        var builder = QueryBuilder.SelectAll()
                                  .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                                  .Optional(gpb => gpb.Where(tpb => tpb.Subject("s")
                                                                       .PredicateUri(UriFactory.Root.Create(RdfSpecsHelper.RdfType))
                                                                       .Object("type")))
                                  .Where(tpb => tpb.Subject("x").Predicate("y").Object("z"));

        // when
        var q = builder.BuildQuery();

        // then
        Assert.NotNull(q.RootGraphPattern);
        Assert.Equal(2, q.RootGraphPattern.TriplePatterns.Count());
        Assert.Single(q.RootGraphPattern.ChildGraphPatterns);
        Assert.True(q.RootGraphPattern.ChildGraphPatterns[0].IsOptional);
        Assert.Single(q.RootGraphPattern.ChildGraphPatterns[0].TriplePatterns);
    }

    [Fact]
    public void GetExectuableQueryReturnsNewInstance()
    {
        // given
        IQueryBuilder builder = QueryBuilder.SelectAll().Where(tpb => tpb.Subject("s").Predicate("p").Object("o"));

        // when
        SparqlQuery query1 = builder.BuildQuery();
        SparqlQuery query2 = builder.BuildQuery();

        // then
        Assert.NotSame(query1, query2);
    }

    [Fact]
    public void CanStartQueryWithGivenVariablesStrings()
    {
        // given
        IQueryBuilder queryBuilder = QueryBuilder.Select("s", "p", "o")
                                                 .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"));

        // when
        SparqlQuery query = queryBuilder.BuildQuery();

        // then
        Assert.Equal(3, query.Variables.Count());
        Assert.Equal(1, query.Variables.Count(v => v.Name == "s"));
        Assert.Equal(1, query.Variables.Count(v => v.Name == "p"));
        Assert.Equal(1, query.Variables.Count(v => v.Name == "o"));
        Assert.True(query.Variables.All(var => var.IsResultVariable));
    }

    [Fact]
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
        Assert.Equal(3, query.Variables.Count());
        Assert.Contains(s, query.Variables);
        Assert.Contains(p, query.Variables);
        Assert.Contains(o, query.Variables);
    }

    [Fact]
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
        Assert.NotNull(query.RootGraphPattern);
        Assert.Single(query.RootGraphPattern.TriplePatterns);
        Assert.Single(query.RootGraphPattern.ChildGraphPatterns);
        Assert.Equal(2, query.RootGraphPattern.ChildGraphPatterns.Single().TriplePatterns.Count);
    }

    [Fact]
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
        Assert.NotNull(query.RootGraphPattern);
        Assert.Single(query.RootGraphPattern.TriplePatterns);
        Assert.Single(query.RootGraphPattern.ChildGraphPatterns);
        Assert.Equal(2, query.RootGraphPattern.ChildGraphPatterns.Single().TriplePatterns.Count);
    }

    [Fact]
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
        Assert.NotNull(query.RootGraphPattern);
        Assert.Equal(4, query.RootGraphPattern.TriplePatterns.Count);
        Assert.Empty(query.RootGraphPattern.ChildGraphPatterns);
    }

    [Fact]
    public void CanAddMultipleSelectVariablesOneByOne()
    {
        // given
        var b = QueryBuilder.Select("s").And("p").And("o")
                            .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"));

        // when
        var q = b.BuildQuery();

        // then
        Assert.NotNull(q.RootGraphPattern);
        Assert.Equal(3, q.Variables.Count(v => v.IsResultVariable));
        Assert.Single(q.RootGraphPattern.TriplePatterns);
    }

    [Fact]
    public void ShouldEnsureSparqlVariablesAreReturnVariables()
    {
        // when
        var q = QueryBuilder.Select(new SparqlVariable("s"), new SparqlVariable("p"), new SparqlVariable("o"))
                            .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                            .BuildQuery();

        // then
        Assert.Equal(3, q.Variables.Count(v => v.IsResultVariable));
    }

    [Fact]
    public void ShouldBeCreatedWithEmptyNamespaceMap()
    {
        // when
        IQueryBuilder builder = QueryBuilder.SelectAll().Where();

        // then
        Assert.Empty(builder.Prefixes.Prefixes);
    }

    [Fact]
    public void CanCreateSelectQueryWithExpressionFirst()
    {
        // when
        SparqlQuery q = QueryBuilder
            .Select(eb => eb.IsIRI(eb.Variable("o"))).As("isIri").And("o")
            .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
            .BuildQuery();

        // then
        Assert.Equal(SparqlQueryType.Select, q.QueryType);
        Assert.Equal(2, q.Variables.Count());
        Assert.Equal(1, q.Variables.Count(v => v.IsProjection && v.IsResultVariable && v.Name == "isIri"));
        Assert.Equal(1, q.Variables.Count(v => !v.IsProjection && v.IsResultVariable && v.Name == "o"));
    }

    [Fact]
    public void ShouldAllowUsingISparqlExpressionForFilter()
    {
        // given
        ISparqlExpression expression = new IsIriFunction(new VariableTerm("x"));
        var b = QueryBuilder.SelectAll().Filter(expression);

        // when
        var q = b.BuildQuery();

        // then
        Assert.True(q.RootGraphPattern.IsFiltered);
        Assert.Same(expression, q.RootGraphPattern.Filter.Expression);
    }

    [Fact]
    public void ShouldAllowAddingLimit()
    {
        // when
        SparqlQuery q = QueryBuilder.SelectAll()
            .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
            .Limit(5)
            .BuildQuery();

        // then
        Assert.Equal(5, q.Limit);
        Assert.Equal(0, q.Offset);
    }

    [Fact]
    public void ShouldAllowAddingNegativeLimit()
    {
        // when
        SparqlQuery q = QueryBuilder.SelectAll()
            .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
            .Limit(5).Limit(-10)
            .BuildQuery();

        // then
        Assert.Equal(-1, q.Limit);
        Assert.Equal(0, q.Offset);
    }

    [Fact]
    public void ShouldAllowAddingOffset()
    {
        // when
        SparqlQuery q = QueryBuilder.SelectAll()
            .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
            .Offset(10)
            .BuildQuery();

        // then
        Assert.Equal(-1, q.Limit);
        Assert.Equal(10, q.Offset);
    }

    [Fact]
    public void ShouldAllowAddingLimitMultipleTimes()
    {
        // given
        var limit = 5;

        // when
        SparqlQuery q = QueryBuilder.SelectAll()
            .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
            .Limit(limit++).Limit(limit++).Limit(limit++).Limit(limit++).Limit(limit)
            .BuildQuery();

        // then
        Assert.Equal(limit, q.Limit);
        Assert.Equal(0, q.Offset);
    }

    [Fact]
    public void ShouldAllowAddingOffsetMultipleTimes()
    {
        // given
        var offset = 5;

        // when
        SparqlQuery q = QueryBuilder.SelectAll()
            .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
            .Offset(offset++).Offset(offset++).Offset(offset++).Offset(offset++).Offset(offset)
            .BuildQuery();

        // then
        Assert.Equal(-1, q.Limit);
        Assert.Equal(offset, q.Offset);
    }

    [Fact]
    public void ShouldAllowAddingLimitAndOffset()
    {
        // when
        SparqlQuery q = QueryBuilder.SelectAll()
            .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
            .Limit(5).Offset(10)
            .BuildQuery();

        // then
        Assert.Equal(5, q.Limit);
        Assert.Equal(10, q.Offset);
    }

    [Fact]
    public void ShouldAllowBuildingAskQueries()
    {
        // when
        SparqlQuery q = QueryBuilder.Ask()
                                    .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                                    .BuildQuery();

        // then
        Assert.Equal(SparqlQueryType.Ask, q.QueryType);
    }

    [Fact]
    public void ShouldAllowBuildingConstructQueries()
    {
        // when
        SparqlQuery q = QueryBuilder.Construct(gpb => gpb.Where(tpb => tpb.Subject("o").Predicate("p").Object("s")))
                                    .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                                    .BuildQuery();

        // then
        Assert.Equal(SparqlQueryType.Construct, q.QueryType);
        Assert.NotNull(q.ConstructTemplate);
        Assert.Single(q.ConstructTemplate.TriplePatterns);
    }

    [Fact]
    public void ShouldAllowBuildingConstructQueriesWithNullBuilderFunction()
    {
        // when
        SparqlQuery q = QueryBuilder.Construct(null)
                                    .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
                                    .BuildQuery();

        // then
        Assert.Equal(SparqlQueryType.Construct, q.QueryType);
        Assert.Null(q.ConstructTemplate);
        Assert.Single(q.RootGraphPattern.TriplePatterns);
    }

    [Fact]
    public void ShouldAllowBuildingConstructQueriesWithoutBuilderFunction()
    {
        // when
        SparqlQuery q = QueryBuilder.Construct()
                                    .Where(BuildSPOPattern)
                                    .BuildQuery();

        // then
        Assert.Equal(SparqlQueryType.Construct, q.QueryType);
        Assert.Null(q.ConstructTemplate);
        Assert.Single(q.RootGraphPattern.TriplePatterns);
    }

    private static void BuildSPOPattern(ITriplePatternBuilder tpb)
    {
        tpb.Subject("s").Predicate("p").Object("o");
    }

    [Fact]
    public void ShouldAllowOrderingQueryByVariableAscending()
    {
        // when
        SparqlQuery sparqlQuery = QueryBuilder.SelectAll()
                                              .Where(BuildSPOPattern)
                                              .OrderBy("s")
                                              .BuildQuery();

        // then
        Assert.NotNull(sparqlQuery.OrderBy);
        Assert.Null(sparqlQuery.OrderBy.Child);
        Assert.True(sparqlQuery.OrderBy is OrderByVariable);
        Assert.False(sparqlQuery.OrderBy.Descending);
        Assert.Equal("s", (sparqlQuery.OrderBy as OrderByVariable).Variables.Single());
    }

    [Fact]
    public void ShouldAllowOrderingQueryByVariableDescending()
    {
        // when
        SparqlQuery sparqlQuery = QueryBuilder.SelectAll()
                                              .Where(BuildSPOPattern)
                                              .OrderByDescending("s")
                                              .BuildQuery();

        // then
        Assert.NotNull(sparqlQuery.OrderBy);
        Assert.Null(sparqlQuery.OrderBy.Child);
        Assert.True(sparqlQuery.OrderBy is OrderByVariable);
        Assert.True(sparqlQuery.OrderBy.Descending);
        Assert.Equal("s", (sparqlQuery.OrderBy as OrderByVariable).Variables.Single());
    }

    [Fact]
    public void ShouldAllowOrderingQueryByExpressionAscending()
    {
        // when
        SparqlQuery sparqlQuery = QueryBuilder.SelectAll()
                                              .Where(BuildSPOPattern)
                                              .OrderBy(expr => expr.Str(expr.Variable("s"))) 
                                              .BuildQuery();

        // then
        Assert.NotNull(sparqlQuery.OrderBy);
        Assert.Null(sparqlQuery.OrderBy.Child);
        Assert.True(sparqlQuery.OrderBy is OrderByExpression);
        Assert.False(sparqlQuery.OrderBy.Descending);
        Assert.True((sparqlQuery.OrderBy as OrderByExpression).Expression is StrFunction);
    }

    [Fact]
    public void ShouldAllowOrderingQueryByExpressionDescending()
    {
        // when
        SparqlQuery sparqlQuery = QueryBuilder.SelectAll()
                                              .Where(BuildSPOPattern)
                                              .OrderByDescending(expr => expr.Str(expr.Variable("s")))
                                              .BuildQuery();

        // then
        Assert.NotNull(sparqlQuery.OrderBy);
        Assert.Null(sparqlQuery.OrderBy.Child);
        Assert.True(sparqlQuery.OrderBy is OrderByExpression);
        Assert.True(sparqlQuery.OrderBy.Descending);
        Assert.True((sparqlQuery.OrderBy as OrderByExpression).Expression is StrFunction);
    }

    [Fact]
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

        for (var i = 0; i < 6; i++)
        {
            Assert.NotNull(currentOrdering);
            currentOrdering = currentOrdering.Child;
        }
        Assert.Null(currentOrdering);
    }

    [Fact]
    public void CanBuildUnionOfChildPattern()
    {
        // when
        var query = QueryBuilder.SelectAll()
            .Union(first => first.Where(BuildSPOPattern),
                   second => second.Where(BuildSPOPattern),
                   third => third.Where(BuildSPOPattern))
            .BuildQuery();

        // then
        var union = query.RootGraphPattern.ChildGraphPatterns.Single();
        Assert.True(union.IsUnion);
        Assert.Equal(3, union.ChildGraphPatterns.Count);
    }

    [Fact]
    public void CanBuildUnionOfMixedPatterns()
    {
        // when
        var query = QueryBuilder.SelectAll()
            .Union(
                first => first.Service(new Uri("http://example.com"), service => service.Where(BuildSPOPattern)),
                second => second.Graph("s", graph => graph.Where(BuildSPOPattern)))
            .BuildQuery();

        // then
        var union = query.RootGraphPattern.ChildGraphPatterns.Single();
        Assert.True(union.IsUnion);
        Assert.Equal(2, union.ChildGraphPatterns.Count);
        Assert.True(union.ChildGraphPatterns[0].ChildGraphPatterns[0].IsService);
        Assert.True(union.ChildGraphPatterns[1].ChildGraphPatterns[0].IsGraph);
    }

    [Fact]
    public void SingleGraphPatternInUnionBehavesLikeChildPattern()
    {
        // when
        var query = QueryBuilder.SelectAll()
            .Union(graph => graph.Where(BuildSPOPattern))
            .BuildQuery();

        // then
        var union = query.RootGraphPattern.ChildGraphPatterns.Single();
        Assert.False(union.IsUnion);
        Assert.Single(union.TriplePatterns);
    }

    [Fact]
    public void CanBuildSelectSumWithGrouping()
    {
        // given
        var o = new VariableTerm("o");

        // when
        var query = QueryBuilder.Select(ex => ex.Sum(o)).As("sum")
            .Where(BuildSPOPattern)
            .GroupBy("s")
            .BuildQuery();

        // then
        Assert.Equal("sum", query.Variables.Single().Name);
        Assert.Equal("s", query.GroupBy.Variables.Single());
    }

    [Fact]
    public void CanBuildGroupingWithExpression()
    {
        // when
        var query = QueryBuilder.SelectAll()
            .Where(BuildSPOPattern)
            .GroupBy(eb => eb.Datatype(eb.Variable("s")))
            .BuildQuery();

        // then
        Assert.Equal("DATATYPE(?s)", query.GroupBy.Expression.ToString());
    }

    [Fact]
    public void CanBuildSelectSumWithHaving()
    {
        // given
        var o = new VariableTerm("o");

        // when
        var query = QueryBuilder.Select(ex => ex.Sum(o)).As("sum")
            .Where(BuildSPOPattern)
            .Having(ex => ex.Variable("sum") > 10)
            .BuildQuery();

        // then
        Assert.Equal("?sum > 10", query.Having.Expression.ToString().Trim());
    }

    [Fact]
    public void QueryWithHavingShouldRoundtripThroughParser()
    {
        IQueryBuilder queryBuilder = QueryBuilder.Select(b => b.Sum("z")).As("sum")
            .Where(p => p.Subject("x").Predicate("y").Object("z"))
            .GroupBy("y")
            .Having(b => b.Variable("sum") > 10);
        SparqlQuery query = queryBuilder.BuildQuery();
        var queryString = query.ToString();
        SparqlQuery parsedQuery = new SparqlQueryParser().ParseFromString(queryString);
        parsedQuery.Having.Should().NotBeNull();
        parsedQuery.Having.Expression.ToString().Trim().Should().Be("?sum > 10");
    }

    [Fact]
    public void WithoutCallingHavingItIsNullOnBuiltQuery()
    {
        // when
        var query = QueryBuilder.Select("o")
            .Where(BuildSPOPattern)
            .BuildQuery();

        // then
        Assert.Null(query.Having);
    }

    /// <summary>
    /// Tests if queries that contain sub-query can be build.
    /// </summary>
    [Fact]
    public void CanBuildSubQueries()
    {
        var hasSomeValue = new Uri("http://example.org/hasSomeValue");

        var s = new SparqlVariable("s");
        var x = new SparqlVariable("x");

        // Valid: SELECT query with one SELECT sub-query.
        IQueryBuilder subSelectBuilder = QueryBuilder
            .Select(s)
            .And(v => v.Count(x)).As("c")
            .Where(BuildSPOPattern)
            .Optional(o => o.Where(p => p.Subject(s).PredicateUri(hasSomeValue).Object(x)))
            .GroupBy(s);

        IQueryBuilder queryBuilder0 = QueryBuilder
            .Select(s)
            .Where(BuildSPOPattern)
            .Child(subSelectBuilder)
            .Filter(f => f.Variable("c") == 0);

        SparqlQuery q0 = queryBuilder0.BuildQuery();

        Assert.Equal(1, q0.RootGraphPattern.TriplePatterns.Count(p => p.PatternType == TriplePatternType.SubQuery));

        // Invalid: SELECT query with one ASK sub-query.
        IQueryBuilder subAskBuilder = QueryBuilder
            .Ask()
            .Where(BuildSPOPattern);

        Assert.Throws<ArgumentException>(() =>
        {
            IQueryBuilder queryBuilder1 = QueryBuilder
                .Select(s)
                .Where(BuildSPOPattern)
                .Child(subAskBuilder);
        });

        // Invalid: SELECT query with one CONSTRUCT sub-query.
        IQueryBuilder subConstructBuilder = QueryBuilder
            .Construct()
            .Where(BuildSPOPattern);

        Assert.Throws<ArgumentException>(() =>
        {
            IQueryBuilder queryBuilder1 = QueryBuilder
                .Select(s)
                .Where(BuildSPOPattern)
                .Child(subConstructBuilder);
        });

        // Invalid: SELECT query with one DESCRIBE sub-query.
        IQueryBuilder subDescribeBuilder = QueryBuilder
            .Construct()
            .Where(BuildSPOPattern);

        Assert.Throws<ArgumentException>(() =>
        {
            IQueryBuilder queryBuilder1 = QueryBuilder
                .Select(s)
                .Where(BuildSPOPattern)
                .Child(subDescribeBuilder);
        });
    }

    /// <summary>
    /// Tests if queries can be build using a visitor pattern where the expressions are 
    /// visited top-down, building UNION before the actual content.
    /// </summary>
    [Fact]
    public void CanBuildUnionWithGraphPatternBuilders()
    {
        // Define two pattern builders which will be used to generate triple patterns for a UNION expression.
        var patternBuilder0 = new GraphPatternBuilder();
        var patternBuilder1 = new GraphPatternBuilder();

        IQueryBuilder queryBuilder0 = QueryBuilder
            .Ask()
            .Union(patternBuilder0, patternBuilder1);

        // Add some triple patterns after the UNION was added to the query.
        patternBuilder0.Where(p => p.Subject("s0").Predicate("p0").ObjectLiteral(0));
        patternBuilder1.Where(p => p.Subject("s1").Predicate("p1").ObjectLiteral(1));

        SparqlQuery q0 = queryBuilder0.BuildQuery();

        Assert.Single(q0.RootGraphPattern.ChildGraphPatterns);
        Assert.True(q0.RootGraphPattern.ChildGraphPatterns.First().IsUnion);
        Assert.Equal(2, q0.RootGraphPattern.ChildGraphPatterns.First().ChildGraphPatterns.Count());

        // Now add other triples to the query after the query was already build.
        patternBuilder1.Where(p => p.Subject("s1").Predicate("p2").ObjectLiteral(2));

        SparqlQuery q1 = queryBuilder0.BuildQuery();

        Assert.Single(q1.RootGraphPattern.ChildGraphPatterns);
        Assert.True(q1.RootGraphPattern.ChildGraphPatterns.First().IsUnion);
        Assert.Equal(2, q1.RootGraphPattern.ChildGraphPatterns.First().ChildGraphPatterns.Count());
        Assert.Contains(q1.RootGraphPattern.ChildGraphPatterns.First().ChildGraphPatterns, p => p.ToString().Contains("p2"));
    }

    [Fact]
    public void CanBuildQueryWithGroupBySeveralVariables()
    {
        SparqlQuery query = QueryBuilder.Select(b => b.Sum("c")).As("sum")
            .Where(p => p
                .Subject("x").PredicateUri(new Uri("http://example.org/a")).Object("a")
                .Subject("x").PredicateUri(new Uri("http://example.org/b")).Object("b")
                .Subject("x").PredicateUri(new Uri("http://example.org/c")).Object("c"))
            .GroupBy("x")
            .GroupBy("a")
            .GroupBy("b")
            .BuildQuery();
        query.GroupBy.Variables.Count().Should().Be(3);
        query.GroupBy.ToString().Should().Be("?x ?a ?b");
    }

    [Fact]
    public void ItBuildsAnOptimisedQueryByDefault()
    {
        SparqlQuery query = QueryBuilder.Select("s", "p", "o")
            .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
            .BuildQuery();
        query.IsOptimised.Should().BeTrue();
    }
    
    [Fact]
    public void QueryOptimisationCanBeDisabled()
    {
        SparqlQuery query = QueryBuilder.Select("s", "p", "o")
            .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
            .BuildQuery(false);
        query.IsOptimised.Should().BeFalse();
    }

    [Fact]
    public void ItCanUseALocallyDefinedQueryOptimiser()
    {
        var optimiserMock = new Mock<IQueryOptimiser>();
        SparqlQuery query = QueryBuilder.Select("s", "p", "o")
            .Where(tpb => tpb.Subject("s").Predicate("p").Object("o"))
            .BuildQuery(true, optimiserMock.Object);
        query.IsOptimised.Should().BeTrue();
        optimiserMock.Verify(optim => optim.Optimise(It.IsAny<GraphPattern>(), It.IsAny<IEnumerable<string>>()), Times.Once);
    }
}
