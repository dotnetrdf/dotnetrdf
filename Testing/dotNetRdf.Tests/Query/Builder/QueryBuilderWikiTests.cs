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
using Xunit;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder;

/// <summary>
/// Test class for queries used on the <a href="https://bitbucket.org/romanticweb/dotnetrdf/wiki">wiki</a>, so that it stays updated
/// </summary>

public class QueryBuilderWikiTests
{
    [Fact]
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
        Assert.NotNull(q.RootGraphPattern);
        Assert.Equal(2, q.Variables.Count());
        Assert.True(q.Variables.All(v => v.IsResultVariable));
        Assert.Equal(2, q.RootGraphPattern.TriplePatterns.Count);
    }

    [Fact]
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
        Assert.NotNull(q.RootGraphPattern);
        Assert.Equal(2, q.Variables.Count());
        Assert.True(q.Variables.All(v => v.IsResultVariable));
        Assert.Equal(2, q.RootGraphPattern.TriplePatterns.Count);
    }

    [Fact]
    public void DescribeWithoutGraphPattern()
    {
        // given
        const string uriString = "http://example.org/";
        var b = QueryBuilder.Describe(new Uri(uriString));

        // when
        var q = b.BuildQuery();

        // then
        Assert.Equal(SparqlQueryType.Describe, q.QueryType);
        Assert.Single(q.DescribeVariables);
        Assert.Equal(uriString, q.DescribeVariables.Single().Value);
    }

    [Fact]
    public void DescribeWithGraphPattern()
    {
        // given
        var b = QueryBuilder.Describe("x")
                            .Where(tpb => tpb.Subject("x").PredicateUri("foaf:mbox").Object(new Uri("mailto:alice@org")));
        b.Prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));

        // when
        var q = b.BuildQuery();

        // then
        Assert.NotNull(q.RootGraphPattern);
        Assert.Equal("x", q.DescribeVariables.Single().Value);
        Assert.Single(q.RootGraphPattern.TriplePatterns);
    }

    [Fact]
    public void DescribeWithMixedVariablesAndUrIs()
    {
        // given
        var b = QueryBuilder.Describe("x").And("y").And(new Uri("http://example.org/"))
                                          .Where(tpb => tpb.Subject("x").PredicateUri("foaf:knows").Object("y"));
        b.Prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));

        // when
        var q = b.BuildQuery();

        // then
        Assert.NotNull(q.RootGraphPattern);
        Assert.Equal("x", q.DescribeVariables.ElementAt(0).Value);
        Assert.Equal("y", q.DescribeVariables.ElementAt(1).Value);
        Assert.Equal("http://example.org/", q.DescribeVariables.ElementAt(2).Value);
        Assert.Single(q.RootGraphPattern.TriplePatterns);
    }

    [Fact]
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
        Assert.NotNull(q.RootGraphPattern);
        Assert.Single(q.RootGraphPattern.TriplePatterns);
        Assert.Single(q.RootGraphPattern.ChildGraphPatterns);
        Assert.True(q.RootGraphPattern.ChildGraphPatterns.Single().IsOptional);
        Assert.Single(q.RootGraphPattern.ChildGraphPatterns.Single().TriplePatterns);
    }

    [Fact]
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
        Assert.Single(q.RootGraphPattern.TriplePatterns);
        Assert.Equal(2, q.RootGraphPattern.ChildGraphPatterns.Count);
        Assert.Single(q.RootGraphPattern.ChildGraphPatterns.ElementAt(0).TriplePatterns);
        Assert.Single(q.RootGraphPattern.ChildGraphPatterns.ElementAt(1).TriplePatterns);
    }

    [Fact]
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
        Assert.Single(q.RootGraphPattern.TriplePatterns);
        Assert.Single(q.RootGraphPattern.ChildGraphPatterns);
        Assert.Equal(2, q.RootGraphPattern.ChildGraphPatterns.Single().TriplePatterns.Count);
    }

    [Fact]
    public void FilterInRootGraphPattern()
    {
        // given
        var b = QueryBuilder.Select("name", "mbox")
                            .Where(tpb => tpb.Subject("x").PredicateUri("dc:title").Object("title"))
                            .Filter(fb => fb.Regex(fb.Variable("title"), "^SPARQL"));
        b.Prefixes.AddNamespace("dc", new Uri("http://purl.org/dc/elements/1.1/"));

        // when
        var q = b.BuildQuery(false);

        // then
        Assert.NotNull(q.RootGraphPattern.Filter);
        Assert.True(q.RootGraphPattern.Filter.Expression is RegexFunction);
        var regex = (RegexFunction)q.RootGraphPattern.Filter.Expression;
        Assert.True(regex.Arguments.ElementAt(0) is VariableTerm);
        Assert.True(regex.Arguments.ElementAt(1) is ConstantTerm);
    }

    [Fact]
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
        var q = b.BuildQuery(false);

        // then
        Assert.Single(q.RootGraphPattern.ChildGraphPatterns);
        Assert.True(q.RootGraphPattern.ChildGraphPatterns.Single().Filter.Expression is LessThanExpression);
    }

    [Fact]
    public void NotExistsFilter()
    {
        // given
        var b = QueryBuilder.Select("person")
            .Where(tpb => tpb.Subject("person").PredicateUri("rdf:type").Object<IUriNode>("foaf:Person"))
            .Filter(
                fb =>
                    !fb.Exists(ex => ex.Where(tpb => tpb.Subject("person").PredicateUri("foaf:name").Object("name"))));
        b.Prefixes.AddNamespace("rdf", new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#"));
        b.Prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));

        // when
        var q = b.BuildQuery();

        // then
        Assert.True(q.RootGraphPattern.IsFiltered);
        Assert.True(q.RootGraphPattern.Filter.Expression is NotExpression);
        Assert.True(q.RootGraphPattern.Filter.Expression.Arguments.First() is ExistsFunction);
    }

    [Fact]
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
        Assert.NotNull(q.RootGraphPattern);
        Assert.Single(q.RootGraphPattern.TriplePatterns);
        Assert.Single(q.RootGraphPattern.ChildGraphPatterns);
        Assert.True(q.RootGraphPattern.ChildGraphPatterns.Single().IsMinus);
        Assert.Single(q.RootGraphPattern.ChildGraphPatterns.Single().TriplePatterns);
    }

    [Fact]
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
        var exists = (ExistsFunction)q.RootGraphPattern.Filter.Expression.Arguments.First();
        Assert.True(exists.Arguments.Single() is GraphPatternTerm);
        Assert.True(((GraphPatternTerm)exists.Arguments.Single()).Pattern.IsFiltered);
    }

    [Fact]
    public void SomewhatComplexFilterSample()
    {
        // given
        var b = QueryBuilder.SelectAll()
            .Filter(eb =>
                {
                    var title = eb.Variable("title");
                    return !eb.IsBlank("book") &&
                           eb.Regex(eb.Str(title), "Twilight", "i") &&
                           eb.Lang(title) == "cn";
                });

        // when
        var q = b.BuildQuery();

        // then
        Assert.Equal(SparqlQueryType.SelectAll, q.QueryType);
        Assert.True(q.RootGraphPattern.IsFiltered);
        Assert.True(q.RootGraphPattern.Filter.Expression is AndExpression);
        Assert.True(q.RootGraphPattern.Filter.Expression.Arguments.ElementAt(0) is AndExpression);
        Assert.True(q.RootGraphPattern.Filter.Expression.Arguments.ElementAt(1) is EqualsExpression);
        Assert.True(q.RootGraphPattern.Filter.Expression.Arguments.ElementAt(0).Arguments.ElementAt(0) is NotExpression);
        Assert.True(q.RootGraphPattern.Filter.Expression.Arguments.ElementAt(0).Arguments.ElementAt(1) is RegexFunction);
        Assert.True(q.RootGraphPattern.Filter.Expression.Arguments.ElementAt(0).Arguments.ElementAt(0).Arguments.ElementAt(0) is IsBlankFunction);
        Assert.True(q.RootGraphPattern.Filter.Expression.Arguments.ElementAt(1).Arguments.ElementAt(0) is LangFunction);
        Assert.True(q.RootGraphPattern.Filter.Expression.Arguments.ElementAt(1).Arguments.ElementAt(1) is ConstantTerm);
    }

    [Fact]
    public void BindAssignment()
    {
        // given
        var b = QueryBuilder.Select("title", "price")
            .Where(tp => tp.Subject("x").PredicateUri("ns:price").Object("p")
                           .Subject("x").PredicateUri("ns:discount").Object("discount"))
            .Bind(ex => ex.Variable("p") * (1 - ex.Variable("discount"))).As("price")
            .Filter(ex => ex.Variable("price") < 20)
            .Where(tp => tp.Subject("x").PredicateUri("dc:title").Object("title"));
        b.Prefixes.AddNamespace("dc", new Uri("http://purl.org/dc/elements/1.1/"));
        b.Prefixes.AddNamespace("ns", new Uri("http://example.com/ns#"));

        // when
        var q = b.BuildQuery(false);

        // then
        Assert.False(q.RootGraphPattern.HasChildGraphPatterns);
        Assert.Equal(3, q.RootGraphPattern.TriplePatterns.Count);
        Assert.True(q.RootGraphPattern.IsFiltered);
        Assert.True(q.RootGraphPattern.Filter.Expression is LessThanExpression);
        Assert.Single(q.RootGraphPattern.UnplacedAssignments);
    }

    [Fact]
    public void SelectExpression()
    {
        // given
        var b = QueryBuilder.Select("title")
            .And(ex => ex.Variable("p")*(1 - ex.Variable("discount"))).As("price")
            .Where(tp => tp.Subject("x").PredicateUri("ns:price").Object("p")
                           .Subject("x").PredicateUri("dc:title").Object("title")
                           .Subject("x").PredicateUri("ns:discount").Object("discount"));
        b.Prefixes.AddNamespace("dc", new Uri("http://purl.org/dc/elements/1.1/"));
        b.Prefixes.AddNamespace("ns", new Uri("http://example.com/ns#"));

        // when
        var q = b.BuildQuery();

        // then
        Assert.False(q.RootGraphPattern.HasChildGraphPatterns);
        Assert.Equal(3, q.RootGraphPattern.TriplePatterns.Count);
        Assert.False(q.RootGraphPattern.IsFiltered);
        Assert.Empty(q.RootGraphPattern.UnplacedAssignments);
        Assert.Equal(1, q.Variables.Count(v => v.IsProjection && v.IsResultVariable));
    }

    [Fact]
    public void BasicConstruct()
    {
        // given
        var b = QueryBuilder.Construct(c => c.Where(t => t.Subject(new Uri("http://example.org/person#Alice"))
                                                          .PredicateUri("vcard:FN")
                                                          .Object<ILiteralNode>("Alice")))
            .Where(tp => tp.Subject("x").PredicateUri("foaf:name").Object("name"));
        b.Prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));
        b.Prefixes.AddNamespace("vcard", new Uri("http://www.w3.org/2001/vcard-rdf/3.0#"));

        // when
        var q = b.BuildQuery();

        // then
        Assert.Equal(SparqlQueryType.Construct, q.QueryType);
        Assert.Single(q.ConstructTemplate.TriplePatterns);
        Assert.Single(q.RootGraphPattern.TriplePatterns);
    }

    [Fact]
    public void ConstructWhere()
    {
        // given
        var b = QueryBuilder.Construct()
                            .Where(tp => tp.Subject("x").PredicateUri("foaf:name").Object("name"));
        b.Prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));

        // when
        var q = b.BuildQuery();

        // then
        Assert.Equal(SparqlQueryType.Construct, q.QueryType);
        Assert.Null(q.ConstructTemplate);
        Assert.Single(q.RootGraphPattern.TriplePatterns);
    }

    [Fact]
    public void Ask()
    {
        // given
        var b = QueryBuilder.Ask()
                            .Where(t => t.Subject("x").PredicateUri("foaf:name").ObjectLiteral("Alice"));
        b.Prefixes.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));

        // when
        var q = b.BuildQuery();

        // then
        Assert.Equal(SparqlQueryType.Ask, q.QueryType);
        Assert.Single(q.RootGraphPattern.TriplePatterns);
    }
}