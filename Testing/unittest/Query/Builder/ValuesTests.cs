using System;
using FluentAssertions;
using VDS.RDF.Query.Builder.Assertions;
using Xunit;

namespace VDS.RDF.Query.Builder
{
    public class ValuesTests
    {
        [Fact]
        public void InlineDataCanBeAddedToRootGraphPatternWithSingleValue()
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
    }
}