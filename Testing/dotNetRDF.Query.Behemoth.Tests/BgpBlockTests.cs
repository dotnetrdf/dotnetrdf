// unset

using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;
using Xunit;

namespace dotNetRDF.Query.Behemoth.Tests
{
    public class BgpBlockTests : IClassFixture<FoafDatasetFixture>
    {
        private readonly FoafDatasetFixture _fixture;

        public BgpBlockTests(FoafDatasetFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void TestSinglePatternBgp()
        {
            var rdfType = new UriNode(new Uri(NamespaceMapper.RDF + "type"));
            var foafPerson = new UriNode(new Uri("http://xmlns.com/foaf/0.1/Person"));

            var bgp = new Bgp(new TriplePattern(new VariablePattern("person"),
                new NodeMatchPattern(rdfType),
                new NodeMatchPattern(foafPerson)));

            var block = new BgpBlock(bgp, _fixture.EvaluationContext);
            var results = block.Evaluate(Bindings.Empty).ToList();
            Assert.Equal(2, results.Count);
            Assert.Contains(results, x =>x["person"].Equals(new UriNode(new Uri("http://example.org/test/alice"))));
            Assert.Contains(results, x => x["person"].Equals(new UriNode(new Uri("http://example.org/test/bob"))));
        }

        [Fact]
        public void TestJoinedPatternsBgp()
        {
            var rdfType = new UriNode(new Uri(NamespaceMapper.RDF + "type"));
            var foafPerson = new UriNode(new Uri("http://xmlns.com/foaf/0.1/Person"));
            var foafKnows = new UriNode(new Uri("http://xmlns.com/foaf/0.1/knows"));

            var bgp = new Bgp(new List<ITriplePattern>
            {
                new TriplePattern(new VariablePattern("person"),
                    new NodeMatchPattern(rdfType),
                    new NodeMatchPattern(foafPerson)),
                new TriplePattern(new VariablePattern("person"),
                new NodeMatchPattern(foafKnows),
                new VariablePattern("friend"))
            });
            var block = new BgpBlock(bgp, _fixture.EvaluationContext);
            var results = block.Evaluate(Bindings.Empty).ToList();
            Assert.Single(results);
            Assert.Contains(results, x => x["person"].Equals(new UriNode(new Uri("http://example.org/test/alice"))) &&
                                          x["friend"].Equals(new UriNode(new Uri("http://example.org/test/bob"))));
        }

        [Fact]
        public void TestCrossJoinedPatternsBgp()
        {
            var rdfType = new UriNode(new Uri(NamespaceMapper.RDF + "type"));
            var foafPerson = new UriNode(new Uri("http://xmlns.com/foaf/0.1/Person"));
            var foafGroup = new UriNode(new Uri("http://xmlns.com/foaf/0.1/Group"));
            var alice = new UriNode(new Uri("http://example.org/test/alice"));
            var bob = new UriNode(new Uri("http://example.org/test/bob"));
            var managers = new UriNode(new Uri("http://example.org/test/managers"));
            var developers = new UriNode(new Uri("http://example.org/test/developers"));

            var bgp = new Bgp(new List<ITriplePattern>
            {
                new TriplePattern(new VariablePattern("person"),
                    new NodeMatchPattern(rdfType),
                    new NodeMatchPattern(foafPerson)),
                new TriplePattern(new VariablePattern("group"),
                    new NodeMatchPattern(rdfType),
                    new NodeMatchPattern(foafGroup))
            });
            var block = new BgpBlock(bgp, _fixture.EvaluationContext);
            var results = block.Evaluate(Bindings.Empty).ToList();
            Assert.Equal(4, results.Count);
            Assert.True(results.All(x=>x["person"].Equals(alice) || x["person"].Equals(bob)));
            Assert.True(results.All(x=>x["group"].Equals(managers) || x["group"].Equals(developers)));
        }

        [Fact]
        public void TestPatternEvaluationBailsEarly()
        {
            var pattern1 = new Mock<IEvaluationBlock>();
            var pattern2 = new Mock<IEvaluationBlock>();
            IEnumerable<Bindings> pattern1Bindings = Enumerable.Range(1, 5)
                .Select(x => new Bindings(new KeyValuePair<string, INode>("a", new LongNode(x))));
            
            pattern1.Setup(x => x.Evaluate(It.IsAny<Bindings>())).Returns(pattern1Bindings);
            pattern2.SetupSequence(x => x.Evaluate(It.IsAny<Bindings>()))
                .Returns(Enumerable.Range(1, 3).Select(x =>
                    new Bindings(new KeyValuePair<string, INode>("a", new LongNode(1)),
                        new KeyValuePair<string, INode>("b", new LongNode(x)))))
                .Returns(Enumerable.Range(1, 3).Select(x =>
                    new Bindings(new KeyValuePair<string, INode>("a", new LongNode(2)),
                        new KeyValuePair<string, INode>("b", new LongNode(x)))))
                .Returns(Enumerable.Range(1, 3).Select(x=>
                    new Bindings(new KeyValuePair<string, INode>("a", new LongNode(3)),
                        new KeyValuePair<string, INode>("b", new LongNode(x)))))
                .Throws(new InvalidOperationException());

            var bgp = new BgpBlock(new List<IEvaluationBlock>() {pattern1.Object, pattern2.Object});
            var results = bgp.Evaluate(new Bindings()).Take(5).ToList();

            // Taking 5 elements should only invoke pattern2 twice before enough solutions have been generated
            pattern1.Verify(x=>x.Evaluate(It.IsAny<Bindings>()), Times.Once);
            pattern2.Verify(x=>x.Evaluate(It.Is<Bindings>(x=>x["a"].AsValuedNode().AsInteger().Equals(1))), Times.Once);
            pattern2.Verify(x => x.Evaluate(It.Is<Bindings>(x => x["a"].AsValuedNode().AsInteger().Equals(2))), Times.Once);
            pattern2.VerifyNoOtherCalls();
        }
    }
}