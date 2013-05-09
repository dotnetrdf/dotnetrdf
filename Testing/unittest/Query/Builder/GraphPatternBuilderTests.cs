using System;
using System.Linq;
using NUnit.Framework;
using Moq;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    [TestFixture]
    public class GraphPatternBuilderTests
    {
        private GraphPatternBuilder _builder;
        private Mock<INamespaceMapper> _namespaceMapper;

        [SetUp]
        public void Setup()
        {
            _namespaceMapper = new Mock<INamespaceMapper>(MockBehavior.Strict);
            _builder = new GraphPatternBuilder();
        }

        [Test]
        public void ShouldAllowUsingISparqlExpressionForFilter()
        {
            // given
            ISparqlExpression expression = new IsIriFunction(new VariableTerm("x"));
            _builder.Filter(expression);

            // when
            GraphPattern graphPattern = _builder.BuildGraphPattern(_namespaceMapper.Object);

            // then
            Assert.IsTrue(graphPattern.IsFiltered);
            Assert.AreSame(expression, graphPattern.Filter.Expression);
        }

        [Test]
        public void ShouldAllowCreatingUnionOfTwoGraphPatterns()
        {
            // given
            _builder.Where(t => t.Subject("s").Predicate("p").Object("o"));

            // when
            var unionBuilder =
                _builder.Union(union => union.Where(t => t.Subject("x").Predicate("y").Object("z")));
            var graphPattern = ((GraphPatternBuilder)unionBuilder).BuildGraphPattern(_namespaceMapper.Object);

            // then
            Assert.IsTrue(graphPattern.IsUnion);
            Assert.AreEqual(2, graphPattern.ChildGraphPatterns.Count);
            Assert.AreEqual(1, graphPattern.ChildGraphPatterns[0].TriplePatterns.Count);
            Assert.AreEqual(1, graphPattern.ChildGraphPatterns[1].TriplePatterns.Count);
            Assert.AreEqual(3, graphPattern.ChildGraphPatterns[0].Variables.Count());
            Assert.AreEqual(3, graphPattern.ChildGraphPatterns[1].Variables.Count());
        }

        [Test]
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
            var graphPattern = ((GraphPatternBuilder)unionBuilder).BuildGraphPattern(_namespaceMapper.Object);

            // then
            Assert.IsTrue(graphPattern.IsUnion);
            Assert.AreEqual(6, graphPattern.ChildGraphPatterns.Count);
            foreach (var childGraphPattern in graphPattern.ChildGraphPatterns)
            {
                Assert.AreEqual(1, childGraphPattern.TriplePatterns.Count);
            }
        }

        [Test]
        public void ShouldAllowAddingSimpleChildGraphPatterns()
        {
            // given
            _builder.Child(cp => cp.Child(cp2 => cp2.Child(cp3 => cp3.Child(cp4 => cp4.Child(last => { })))));

            // when
            var graphPattern = _builder.BuildGraphPattern(_namespaceMapper.Object);

            // then
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(1, graphPattern.ChildGraphPatterns.Count);
                graphPattern = graphPattern.ChildGraphPatterns.Single();
            }
        }
    }
}