using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;
using Xunit;

namespace dotNetRDF.Query.Behemoth.Tests
{
    public class ScalingTests : IClassFixture<VirtualDatasetFixture>
    {
        private readonly VirtualDatasetFixture _fixture;

        public ScalingTests(VirtualDatasetFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void TestSinglePatternWithManyResults()
        {
            var rdfType = _fixture.NodeFactory.CreateUriNode("rdf:type");
            var mathInteger = _fixture.NodeFactory.CreateUriNode("math:Integer");
            var bgp = new Bgp(new TriplePattern(new VariablePattern("s"),
                new NodeMatchPattern(rdfType),
                new NodeMatchPattern(mathInteger)));
            var block = new BgpBlock(bgp, _fixture.EvaluationContext);
            var resultCount = block.Evaluate(Bindings.Empty).Count();
            Assert.Equal(1_000_001, resultCount);
        }

        [Fact]
        public void TestSimpleTwoPatternBgpWithManyResults()
        {
            var rdfType = _fixture.NodeFactory.CreateUriNode("rdf:type");
            var mathInteger = _fixture.NodeFactory.CreateUriNode("math:Integer");
            var rdfValue = _fixture.NodeFactory.CreateUriNode("rdf:value");

            var bgp = new Bgp(new[]
                {
                    new TriplePattern(
                        new VariablePattern("s"),
                        new NodeMatchPattern(rdfType),
                        new NodeMatchPattern(mathInteger)),
                    new TriplePattern(
                        new VariablePattern("s"),
                        new NodeMatchPattern(rdfValue),
                        new VariablePattern("v"))
                }
            );
            var block = new BgpBlock(bgp, _fixture.EvaluationContext);
            var resultCount = block.Evaluate(Bindings.Empty).Count();
            Assert.Equal(1_000_001, resultCount);
        }

        [Fact]
        public void TestLeviathanEvaluateSinglePattern()
        {
            var nf = new NodeFactory();
            var ds = new VirtualDataset();
            ds.AddTripleProvider(new IntegerTripleProvider(1_000_000, nf));
            var rdfType = _fixture.NodeFactory.CreateUriNode("rdf:type");
            var mathInteger = _fixture.NodeFactory.CreateUriNode("math:Integer");

            var bgp = new Bgp(new TriplePattern(new VariablePattern("s"),
                new NodeMatchPattern(rdfType),
                new NodeMatchPattern(mathInteger)));
            var resultMultiset = bgp.Evaluate(new SparqlEvaluationContext(null, ds, new LeviathanQueryOptions()));
            Assert.Equal(1_000_001, resultMultiset.Count);
        }


        [Fact]
        public void TestLeviathanSimpleTwoPatternBgpWithManyResults()
        {
            var nf = new NodeFactory();
            var ds = new VirtualDataset();
            ds.AddTripleProvider(new IntegerTripleProvider(1_000_000, nf));
            var rdfType = nf.CreateUriNode("rdf:type");
            var mathInteger = nf.CreateUriNode("math:Integer");
            var rdfValue = nf.CreateUriNode("rdf:value");

            var bgp = new Bgp(new[]
                {
                    new TriplePattern(
                        new VariablePattern("s"),
                        new NodeMatchPattern(rdfType),
                        new NodeMatchPattern(mathInteger)),
                    new TriplePattern(
                        new VariablePattern("s"),
                        new NodeMatchPattern(rdfValue),
                        new VariablePattern("v"))
                }
            );
            var resultMultiset = bgp.Evaluate(new SparqlEvaluationContext(null, ds, new LeviathanQueryOptions()));
            Assert.Equal(1_000_001, resultMultiset.Count);
        }
    }
}
