using System;
using FluentAssertions;
using VDS.RDF.Query.Builder.Assertions;
using Xunit;

namespace VDS.RDF.Query.Builder
{
    public class ValuesTests
    {
        [Fact]
        public void ShorthandMethod_SingleVariable_SingleValue_InRootGraphPattern_AddedSuccessfully()
        {
            // equivalent to
            // SELECT ?o
            // {
            //    VALUES ?o { 5 }
            // }

            // given
            var select = QueryBuilder.Select("o").GetQueryBuilder();
            select.InlineData("o").Values(5);
            var query = select.BuildQuery();

            // then
            Console.Write(query);
            query.RootGraphPattern.HasInlineData.Should().BeTrue();
            query.RootGraphPattern.InlineData.Should()
                .DeclareVariables("o")
                .And.HasTuples(1)
                .And.ContainTuple(new
                {
                    o = 5
                });
        }

        [Fact]
        public void ShorthandMethod_SingleVariable_MultipleValues_InRootGraphPattern_AddedSuccesfully()
        {
            // equivalent to
            // SELECT ?o
            // {
            //    VALUES ?o { 5, 10, 15 }
            // }

            // given
            var select = QueryBuilder.Select("o").GetQueryBuilder();
            select.InlineData("o")
                .Values(5)
                .Values(10)
                .Values(15);
            var query = select.BuildQuery();

            // then
            Console.Write(query);
            query.RootGraphPattern.HasInlineData.Should().BeTrue();
            query.RootGraphPattern.InlineData.Should()
                .DeclareVariables("o")
                .And.HasTuples(3)
                .And.ContainTuple(new { o = 5 })
                .And.ContainTuple(new { o = 10 })
                .And.ContainTuple(new { o = 15 });
        }

        [Fact]
        public void ShorthandMethod_MultipleVariables_SingleTuple_InRootGraphPattern_AddedSuccesfully()
        {
            // equivalent to
            // SELECT *
            // {
            //    VALUES ( ?x, ?y, ?z )
            //    {
            //       ( "a", "b", "c" )
            //    }
            // }

            // given
            var select = QueryBuilder.Select("o").GetQueryBuilder();
            select.InlineData("x", "y", "z")
                .Values("a", "b", "c");
            var query = select.BuildQuery();

            // then
            Console.Write(query);
            query.RootGraphPattern.HasInlineData.Should().BeTrue();
            query.RootGraphPattern.InlineData.Should()
                .DeclareVariables("x", "y", "z")
                .And.HasTuples(1)
                .And.ContainTuple(new { x = "a", y = "b", z = "c" });
        }
    }
}