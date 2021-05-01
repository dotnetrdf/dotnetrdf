using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Nodes;
using Xunit;

namespace dotNetRDF.Query.Behemoth.Tests
{
    public class JoinBlockTests
    {
        [Fact]
        public void ItMinimizesEnumeration()
        {
            var lhs = new Mock<IEvaluationBlock>();
            lhs.Setup(x => x.Evaluate(It.IsAny<Bindings>()))
                .Returns(
                    Enumerable.Range(1, 10000).Select(x =>
                        x > 1000
                            ? throw new InvalidOperationException("Enumeration of lhs values exceeded 1000")
                            : new Bindings(new KeyValuePair<string, INode>("x", new LongNode(x * 3)))));
            var rhs = new Mock<IEvaluationBlock>();
            rhs.Setup(x => x.Evaluate(It.IsAny<Bindings>()))
                .Returns(
                    Enumerable.Range(1, 10000).Select(x =>
                        x > 1000
                            ? throw new InvalidOperationException("Enumeration of rhs values exceeded 1000")
                            : new Bindings(new KeyValuePair<string, INode>("x", new LongNode(x * 5)))));

            var joinBlock = new JoinBlock(lhs.Object, rhs.Object, new[] {"x"});
            var results = joinBlock.Evaluate(Bindings.Empty).Take(10).ToList();
            Assert.Equal(10, results.Count);
            var integerValues = results.Select(x => x["x"]).OfType<IValuedNode>().Select(v => v.AsInteger()).ToList();
            Assert.Equal(10, integerValues.Count);
            Assert.True(integerValues.All(x=>x > 0 && (x%3 == 0) && (x%5 == 0)));
        }

        [Fact]
        public void ItJoinsWhenLhsEmptiesOut()
        {
            var lhs = new Mock<IEvaluationBlock>();
            lhs.Setup(x => x.Evaluate(It.IsAny<Bindings>()))
                .Returns(
                    Enumerable.Range(1, 10).Select(x=> new Bindings(new KeyValuePair<string, INode>("x", new LongNode(x * 3)))));
            var rhs = new Mock<IEvaluationBlock>();
            rhs.Setup(x => x.Evaluate(It.IsAny<Bindings>()))
                .Returns(
                    Enumerable.Range(1, 10000).Select(x =>
                        new Bindings(new KeyValuePair<string, INode>("x", new LongNode(x * 5)))));

            var joinBlock = new JoinBlock(lhs.Object, rhs.Object, new[] { "x" });
            var results = joinBlock.Evaluate(Bindings.Empty).Take(10).ToList();
            Assert.Equal(2, results.Count); // Only joins on 15 and 30
            var integerValues = results.Select(x => x["x"]).OfType<IValuedNode>().Select(v => v.AsInteger()).ToList();
            Assert.Equal(2, integerValues.Count);
            Assert.True(integerValues.All(x => x > 0 && (x % 3 == 0) && (x % 5 == 0)));
        }

        [Fact]
        public void ItJoinsWhenRhsEmptiesOut()
        {
            var lhs = new Mock<IEvaluationBlock>();
            lhs.Setup(x => x.Evaluate(It.IsAny<Bindings>()))
                .Returns(
                    Enumerable.Range(1, 10000).Select(x => new Bindings(new KeyValuePair<string, INode>("x", new LongNode(x * 3)))));
            var rhs = new Mock<IEvaluationBlock>();
            rhs.Setup(x => x.Evaluate(It.IsAny<Bindings>()))
                .Returns(
                    Enumerable.Range(1, 10).Select(x =>
                        new Bindings(new KeyValuePair<string, INode>("x", new LongNode(x * 5)))));

            var joinBlock = new JoinBlock(lhs.Object, rhs.Object, new[] { "x" });
            var results = joinBlock.Evaluate(Bindings.Empty).Take(10).ToList();
            Assert.Equal(3, results.Count); // Only joins on 15, 30 and 45
            var integerValues = results.Select(x => x["x"]).OfType<IValuedNode>().Select(v => v.AsInteger()).ToList();
            Assert.Equal(3, integerValues.Count);
            Assert.True(integerValues.All(x => x > 0 && (x % 3 == 0) && (x % 5 == 0)));
        }

        [Fact]
        public void LeftJoinBlockReturnsAllFromLhs()
        {
            var lhs = new Mock<IEvaluationBlock>();
            lhs.Setup(x => x.Evaluate(It.IsAny<Bindings>()))
                .Returns(
                    Enumerable.Range(1, 10000).Select(x => new Bindings(new KeyValuePair<string, INode>("x", new LongNode(x * 3)))));
            var rhs = new Mock<IEvaluationBlock>();
            rhs.Setup(x => x.Evaluate(It.IsAny<Bindings>()))
                .Returns(
                    Enumerable.Range(1, 10).Select(x =>
                        new Bindings(new KeyValuePair<string, INode>("x", new LongNode(x * 5)),
                            new KeyValuePair<string, INode>("y", new LongNode(x)))));

            var joinBlock = new LeftJoinBlock(lhs.Object, rhs.Object, new[] { "x" });
            var results = joinBlock.Evaluate(Bindings.Empty).Take(10).ToList();
            Assert.Equal(10, results.Count); // Should include all bindings from LHS
            var integerValues = results.Select(x => x["x"]).OfType<IValuedNode>().Select(v => v.AsInteger()).ToList();
            Assert.Equal(10, integerValues.Count);
            Assert.True(integerValues.All(x => x > 0 && (x % 3 == 0)));
            // Bec
            Assert.DoesNotContain(results, x =>x.ContainsVariable("y"));
        }

        [Fact]
        public void TestSingleVarJoin()
        {
            var left = new Bindings[]
            {
                new Bindings(new KeyValuePair<string, INode>("x", new LongNode(1))),
                new Bindings(new KeyValuePair<string, INode>("x", new LongNode(2))),
                new Bindings(new KeyValuePair<string, INode>("x", new LongNode(3)))
            };
            var right = new Bindings[]
            {
                new Bindings(new KeyValuePair<string, INode>("x", new LongNode(4))),
                new Bindings(new KeyValuePair<string, INode>("x", new LongNode(3))),
                new Bindings(new KeyValuePair<string, INode>("x", new LongNode(2)))
            };
            var expect = new List<Bindings>
            {
                new Bindings(new KeyValuePair<string, INode>("x", new LongNode(3))),
                new Bindings(new KeyValuePair<string, INode>("x", new LongNode(2)))
            };
            TestJoin(left, right, new []{"x"}, expect);
        }

        [Fact]
        public void TestTwoVarJoin()
        {
            var left = new Bindings[]
            {
                new Bindings(
                    new KeyValuePair<string, INode>("x", new LongNode(1)),
                    new KeyValuePair<string, INode>("y", new LongNode(2))),
                new Bindings(
                    new KeyValuePair<string, INode>("x", new LongNode(2)),
                    new KeyValuePair<string, INode>("y", new LongNode(4))),
                new Bindings(
                    new KeyValuePair<string, INode>("x", new LongNode(3)),
                    new KeyValuePair<string, INode>("y", new LongNode(6)))
            };
            var right = new Bindings[]
            {
                new Bindings(
                    new KeyValuePair<string, INode>("x", new LongNode(4)),
                    new KeyValuePair<string, INode>("y", new LongNode(4))),
                new Bindings(new KeyValuePair<string, INode>("x", new LongNode(3)),
                    new KeyValuePair<string, INode>("y", new LongNode(4))),
                new Bindings(new KeyValuePair<string, INode>("x", new LongNode(2)),
                    new KeyValuePair<string, INode>("y", new LongNode(4)))
            };
            var expect = new List<Bindings>
            {
                new Bindings(new KeyValuePair<string, INode>("x", new LongNode(2)),
                    new KeyValuePair<string, INode>("y", new LongNode(4)))
            };
            TestJoin(left, right, new[] { "x", "y" }, expect);
        }

        [Fact]
        public void TestMultipleJoins()
        {
            var left = new Bindings[]
            {
                new Bindings(
                    new KeyValuePair<string, INode>("x", new LongNode(1)),
                    new KeyValuePair<string, INode>("y", new LongNode(2))),
                new Bindings(
                    new KeyValuePair<string, INode>("x", new LongNode(2)),
                    new KeyValuePair<string, INode>("y", new LongNode(4))),
                new Bindings(
                    new KeyValuePair<string, INode>("x", new LongNode(3)),
                    new KeyValuePair<string, INode>("y", new LongNode(6)))
            };
            var right = new Bindings[]
            {
                new Bindings(
                    new KeyValuePair<string, INode>("x", new LongNode(4)),
                    new KeyValuePair<string, INode>("y", new LongNode(4)),
                    new KeyValuePair<string, INode>("z", new LongNode(1))),
                new Bindings(new KeyValuePair<string, INode>("x", new LongNode(3)),
                    new KeyValuePair<string, INode>("y", new LongNode(4)),
                    new KeyValuePair<string, INode>("z", new LongNode(2))),
                new Bindings(new KeyValuePair<string, INode>("x", new LongNode(2)),
                    new KeyValuePair<string, INode>("y", new LongNode(4)),
                    new KeyValuePair<string, INode>("z", new LongNode(3))),
                new Bindings(
                    new KeyValuePair<string, INode>("x", new LongNode(4)),
                    new KeyValuePair<string, INode>("y", new LongNode(4)),
                    new KeyValuePair<string, INode>("z", new LongNode(4))),
                new Bindings(new KeyValuePair<string, INode>("x", new LongNode(3)),
                    new KeyValuePair<string, INode>("y", new LongNode(4)),
                    new KeyValuePair<string, INode>("z", new LongNode(5))),
                new Bindings(new KeyValuePair<string, INode>("x", new LongNode(2)),
                    new KeyValuePair<string, INode>("y", new LongNode(4)),
                    new KeyValuePair<string, INode>("z", new LongNode(6)))
            };
            var expect = new List<Bindings>
            {
                new Bindings(new KeyValuePair<string, INode>("x", new LongNode(2)),
                    new KeyValuePair<string, INode>("y", new LongNode(4)),
                    new KeyValuePair<string, INode>("z", new LongNode(3))),
                new Bindings(new KeyValuePair<string, INode>("x", new LongNode(2)),
                new KeyValuePair<string, INode>("y", new LongNode(4)),
                new KeyValuePair<string, INode>("z", new LongNode(6)))
            };
            TestJoin(left, right, new[] { "x", "y" }, expect);
        }


        private void TestJoin(IReadOnlyCollection<Bindings> left, IEnumerable<Bindings> right, string[] joinVars, List<Bindings> expected, int windowSize=100)
        {
            var lhs = new Mock<IEvaluationBlock>();
            lhs.Setup(x => x.Evaluate(It.IsAny<Bindings>())).Returns(left);
            var rhs = new Mock<IEvaluationBlock>();
            rhs.Setup(x => x.Evaluate(It.IsAny<Bindings>())).Returns(right);

            var join = new JoinBlock(lhs.Object, rhs.Object, joinVars, windowSize, windowSize);
            var actual = join.Evaluate(Bindings.Empty).ToList();
            Assert.Equal(expected.Count, actual.Count);
            foreach (var expectedSolution in expected)
            {
                var actualIx = actual.IndexOf(expectedSolution);
                Assert.True(actualIx >= 0, $"Could not find expected solution {expectedSolution} in actual solutions.");
                actual.RemoveAt(actualIx);
            }

            if (actual.Count > 0)
            {
                // These are unexpected solutions
                Assert.True(false, $"Found one or more unexpected solutions in the result: {actual}");
            }
        }
    }
}
