using System;
using FluentAssertions;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Builder.Assertions;
using Xunit;

namespace VDS.RDF.Query.Builder
{
    public class ValuesTests
    {
        private readonly SparqlQueryParser _parser = new SparqlQueryParser();

        [Fact]
        public void ShorthandMethod_SingleVariable_SingleValue_InRootGraphPattern_AddedSuccessfully()
        {
            // equivalent to
            var expected = _parser.ParseFromString(@"SELECT ?o
            {
                VALUES ?o { 5 }
            }");

            // given
            var select = QueryBuilder.Select("o");
            select.InlineData("o").Values(5);
            var query = select.BuildQuery();

            // then
            query.RootGraphPattern.HasInlineData.Should().BeTrue();
            query.RootGraphPattern.InlineData.Should()
                .BeEquivalentTo(expected.RootGraphPattern.InlineData);
        }

        [Fact]
        public void ShorthandMethod_SingleVariable_MultipleValues_InRootGraphPattern_AddedSuccesfully()
        {
            // equivalent to
            var expected = _parser.ParseFromString(@"SELECT ?o
            {
                VALUES ?o { 5 10 15 }
            }");

            // given
            var select = QueryBuilder.Select("o");
            select.InlineData("o")
                .Values(5)
                .Values(10)
                .Values(15);
            var query = select.BuildQuery();

            // then
            query.RootGraphPattern.HasInlineData.Should().BeTrue();
            query.RootGraphPattern.InlineData.Should()
                .BeEquivalentTo(expected.RootGraphPattern.InlineData);
        }

        [Fact]
        public void ShorthandMethod_MultipleVariables_SingleTuple_InRootGraphPattern_AddedSuccesfully()
        {
            // equivalent to
            var expected = _parser.ParseFromString(@"SELECT *
            {
                VALUES ( ?x ?y ?z )
                {
                   ( ""a"" ""b"" ""c"" )
                }
            }");

            // given
            var select = QueryBuilder.Select("o");
            select.InlineData("x", "y", "z")
                .Values("a", "b", "c");
            var query = select.BuildQuery();

            // then
            query.RootGraphPattern.HasInlineData.Should().BeTrue();
            query.RootGraphPattern.InlineData.Should()
                .BeEquivalentTo(expected.RootGraphPattern.InlineData);
        }

        [Fact]
        public void ShorthandMethod_MultipleVariables_MultipleTuples_InRootGraphPattern_AddedSuccesfully()
        {
            // equivalent to
            var expected = _parser.ParseFromString(@"SELECT *
            {
                VALUES ( ?x ?y ?z )
                {
                   ( ""a"" ""b"" ""c"" )
                   ( 123 124 125 )
                }
            }");

            // given
            var select = QueryBuilder.Select("o");
            select.InlineData("x", "y", "z")
                .Values("a", "b", "c")
                .Values(123, 124, 125);
            var query = select.BuildQuery();

            // then
            query.RootGraphPattern.HasInlineData.Should().BeTrue();
            query.RootGraphPattern.InlineData.Should()
                .BeEquivalentTo(expected.RootGraphPattern.InlineData);
        }

        [Fact]
        public void ShorthandMethod_MultipleVariables_UriValues_InRootGraphPattern_AddedSuccesfully()
        {
            // equivalent to
            var expected = _parser.ParseFromString(@"SELECT *
            {
                VALUES ( ?x ?y ?z )
                {
                   ( <http://example.com/x> <http://example.com/y> <http://example.com/z> )
                }
            }");

            // given
            var select = QueryBuilder.SelectAll();
            select.InlineData("x", "y", "z")
                .Values(new Uri("http://example.com/x"), new Uri("http://example.com/y"), new Uri("http://example.com/z"));
            var query = select.BuildQuery();

            // then
            query.RootGraphPattern.HasInlineData.Should().BeTrue();
            query.RootGraphPattern.InlineData.Should()
                .BeEquivalentTo(expected.RootGraphPattern.InlineData);
        }

        [Fact]
        public void ShorthandMethod_MultipleVariables_MixedValuesWithUndef_InRootGraphPattern_AddedSuccesfully()
        {
            // equivalent to
            var expected = _parser.ParseFromString(@"SELECT *
            {
                VALUES ( ?x ?y ?z )
                {
                   ( ""Tomasz"" <http://example.com/y> UNDEF )
                   ( ""Rob"" UNDEF 6 )
                }
            }");

            // given
            var select = QueryBuilder.SelectAll();
            select.InlineData("x", "y", "z")
                .Values("Tomasz", new Uri("http://example.com/y"), null)
                .Values("Rob", null, 6);
            var query = select.BuildQuery();

            // then
            query.RootGraphPattern.HasInlineData.Should().BeTrue();
            query.RootGraphPattern.InlineData.Should()
                .BeEquivalentTo(expected.RootGraphPattern.InlineData);
        }

        [Fact]
        public void VerboseMethod_SingleVariable_MultipleValues_InRootGraphPattern_AddedSuccesfully()
        {
            // equivalent to
            var expected = _parser.ParseFromString(@"SELECT *
            {
                VALUES ( ?x )
                {
                   ( ""Tomasz"" )
                   ( 20 )
                   ( <http://example.com> )
                }
             }");

            // given
            var select = QueryBuilder.SelectAll();
            select.InlineData("x")
                .Values(vb => vb.Value("Tomasz"))
                .Values(vb => vb.Value(20))
                .Values(vb => vb.Value(new Uri("http://example.com")));
            var query = select.BuildQuery();

            // then
            query.RootGraphPattern.HasInlineData.Should().BeTrue();
            query.RootGraphPattern.InlineData.Should()
                .BeEquivalentTo(expected.RootGraphPattern.InlineData);
        }

        [Fact]
        public void VerboseMethod_UndefValues_InRootGraphPattern_AddedSuccesfully()
        {
            // equivalent to
            var expected = _parser.ParseFromString(@"SELECT *
             {
                VALUES ( ?x ?y ?z )
                {
                    ( UNDEF UNDEF UNDEF )
                }
            }");

            // given
            var select = QueryBuilder.SelectAll();
            select.InlineData("x", "y", "z")
                .Values(vb => vb.Undef().Undef().Undef());
            var query = select.BuildQuery();

            // then
            query.RootGraphPattern.HasInlineData.Should().BeTrue();
            query.RootGraphPattern.InlineData.Should()
                .BeEquivalentTo(expected.RootGraphPattern.InlineData);
        }

        [Fact]
        public void VerboseMethod_TypedLiteral_InRootGraphPattern_AddedSuccesfully()
        {
            // equivalent to
            var expected = _parser.ParseFromString(@"SELECT *
             {
                VALUES ?name
                {
                    ""Tomasz""^^<https://schema.org/givenName>
                }
            }");

            // given
            var select = QueryBuilder.SelectAll();
            select.InlineData("name")
                .Values(vb => vb.Value("Tomasz", new Uri("https://schema.org/givenName")));
            var query = select.BuildQuery();

            // then
            query.RootGraphPattern.HasInlineData.Should().BeTrue();
            query.RootGraphPattern.InlineData.Should()
                .BeEquivalentTo(expected.RootGraphPattern.InlineData);
        }

        [Fact]
        public void VerboseMethod_TaggedLiteral_InRootGraphPattern_AddedSuccesfully()
        {
            // equivalent to
            var expected = _parser.ParseFromString(@"SELECT *
            {
                VALUES ( ?name )
                {
                    ( ""Tomasz""@pl )
                    ( ""Thomas""@en )
                }
            }");

            // given
            var select = QueryBuilder.SelectAll();
            select.InlineData("name")
                .Values(vb => vb.Value("Tomasz", "pl"))
                .Values(vb => vb.Value("Thomas", "en"));
            var query = select.BuildQuery();

            // then
            query.RootGraphPattern.HasInlineData.Should().BeTrue();
            query.RootGraphPattern.InlineData.Should()
                .BeEquivalentTo(expected.RootGraphPattern.InlineData);
        }

        [Fact]
        public void ShorthandMethod_OverEntireQuery_AddedSuccesfully()
        {
            // equivalent to
            var expected = _parser.ParseFromString(@"SELECT *
            {
            }
            VALUES ( ?x ?y ?z )
            {
                ( 10 ""Hello"" <http://some.url> )
            }");

            // given
            var select = QueryBuilder.SelectAll();
            select.InlineDataOverQuery("x", "y", "z")
                .Values(10, "Hello", new Uri("http://some.url"));
            var query = select.BuildQuery();

            // then
            query.Bindings.Should()
                .BeEquivalentTo(expected.Bindings);
        }

        [Fact]
        public void ShorthandMethod_BooleanValue_AddedSuccesfully()
        {
            // equivalent to
            var expected = _parser.ParseFromString(@"SELECT *
            {
                VALUES ?bools { true false }
            }");

            // given
            var select = QueryBuilder.SelectAll();
            select.InlineData("bools")
                .Values(true)
                .Values(false);
            var query = select.BuildQuery();

            // then
            query.RootGraphPattern.InlineData.Should()
                .BeEquivalentTo(expected.RootGraphPattern.InlineData);
        }

        [Fact]
        public void ShorthandMethod_TooManyValues_ThrowsException()
        {
            // given
            var select = QueryBuilder.SelectAll();
            var inlineData = select.InlineData("a", "b");

            // when
            var ex = Assert.Throws<InvalidOperationException>(() => inlineData.Values(1, 2, 3));

            // then
            ex.Should().NotBeNull();
        }

        [Fact]
        public void ShorthandMethod_TooFewValues_ThrowsException()
        {
            // given
            var select = QueryBuilder.SelectAll();
            var inlineData = select.InlineData("a", "b");

            // when
            var ex = Assert.Throws<InvalidOperationException>(() => inlineData.Values(1));

            // then
            ex.Should().NotBeNull();
        }

        [Fact]
        public void VerboseMethod_TooManyValues_ThrowsException()
        {
            // given
            var select = QueryBuilder.SelectAll();
            var inlineData = select.InlineData("a", "b");

            // when
            var ex = Assert.Throws<InvalidOperationException>(() =>
                inlineData.Values(builder => builder.Value(1).Value(2).Value(3)));

            // then
            ex.Should().NotBeNull();
        }

        [Fact]
        public void VerboseMethod_TooFewValues_ThrowsException()
        {
            // given
            var select = QueryBuilder.SelectAll();
            var inlineData = select.InlineData("a", "b");

            // when
            var ex = Assert.Throws<InvalidOperationException>(() =>
                inlineData.Values(builder => builder.Value(1)));

            // then
            ex.Should().NotBeNull();
        }
    }
}