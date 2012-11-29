using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Test.Builder
{
    [TestClass]
    public class GraphPatternBuilderTests
    {
        private GraphPatternBuilder _builder;

        [TestInitialize]
        public void Setup()
        {
            _builder = new GraphPatternBuilder(new NamespaceMapper());
        }

        [TestMethod]
        public void ShouldReturnNullIfThereAreNoFiltersTriplePatternsOrChildGraphPatterns()
        {
            // when
            var graphPattern = _builder.BuildGraphPattern();

            // then
            Assert.IsNull(graphPattern);
        }

        [TestMethod]
        public void ShouldAllowUsingISparqlExpressionForFilter()
        {
            // given
            ISparqlExpression expression = new IsIriFunction(new VariableTerm("x"));
            _builder.Filter(expression);

            // when
            GraphPattern graphPattern = _builder.BuildGraphPattern();

            // then
            Assert.IsTrue(graphPattern.IsFiltered);
            Assert.AreSame(expression, graphPattern.Filter.Expression);
        }

        [TestMethod]
        public void ShouldAllowCreatingUnionOfTwoGraphPatterns()
        {
            // given
            _builder.Where(t => t.Subject("s").Predicate("p").Object("o"));

            // when
            var unionBuilder =
                _builder.Union(union => union.Where(t => t.Subject("x").Predicate("y").Object("z")));
            var graphPattern = ((GraphPatternBuilder)unionBuilder).BuildGraphPattern();

            // then
            Assert.IsTrue(graphPattern.IsUnion);
            Assert.AreEqual(2, graphPattern.ChildGraphPatterns.Count);
            Assert.AreEqual(1, graphPattern.ChildGraphPatterns[0].TriplePatterns.Count);
            Assert.AreEqual(1, graphPattern.ChildGraphPatterns[1].TriplePatterns.Count);
            Assert.AreEqual(3, graphPattern.ChildGraphPatterns[0].Variables.Count());
            Assert.AreEqual(3, graphPattern.ChildGraphPatterns[1].Variables.Count());
        }

        [TestMethod]
        public void ShouldAllowCreatingUnionOfMultipleGraphPatterns()
        {
            // given
            Action<ITriplePatternBuilder> buildTriplePattern = t => t.Subject("s").Predicate("p").Object("o");
            _builder.Where(buildTriplePattern);

            // when
            var unionBuilder =
                _builder.Union(union => union.Where(buildTriplePattern))
                        .Union(union => union.Where(buildTriplePattern))
                        .Union(union => union.Where(buildTriplePattern))
                        .Union(union => union.Where(buildTriplePattern))
                        .Union(union => union.Where(buildTriplePattern));
            var graphPattern = ((GraphPatternBuilder)unionBuilder).BuildGraphPattern();

            // then
            Assert.IsTrue(graphPattern.IsUnion);
            Assert.AreEqual(6, graphPattern.ChildGraphPatterns.Count);
            foreach (var childGraphPattern in graphPattern.ChildGraphPatterns)
            {
                Assert.AreEqual(1, childGraphPattern.TriplePatterns.Count);
            }
        }
    }
}