using System;
using System.Collections.Generic;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using Xunit;

namespace dotNetRDF.Query.Behemoth.Tests
{
    public class BehemothQueryProcessorTests : IClassFixture<FoafDatasetFixture>
    {
        private BehemothQueryProcessor _queryProcessor;

        public BehemothQueryProcessorTests(FoafDatasetFixture fixture)
        {
            _queryProcessor = new BehemothQueryProcessor(fixture.EvaluationContext.Data);
        }

        [Fact]
        public void TestSimpleSelect()
        {
            var queryParser = new SparqlQueryParser();
            var query = queryParser.ParseFromString("SELECT * WHERE { <http://example.org/test/alice> ?p ?o }");
            var result = _queryProcessor.ProcessQuery(query) as SparqlResultSet;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void TestSimpleAsk()
        {
            var queryParser = new SparqlQueryParser();
            var query = queryParser.ParseFromString("ASK { <http://example.org/test/alice> a ?t }");
            var result = _queryProcessor.ProcessQuery(query) as SparqlResultSet;
            Assert.NotNull(result);
            Assert.Equal(SparqlResultsType.Boolean, result.ResultsType);
            Assert.True(result.Result);
        }

        [Fact]
        public void TestSimpleBind()
        {
            var queryParser = new SparqlQueryParser();
            var query = queryParser.ParseFromString("SELECT ?x ?y WHERE { VALUES(?x ?y) { (<http://example.org/foo> UNDEF) (UNDEF <http://example.bar>) } }");
            var result = _queryProcessor.ProcessQuery(query) as SparqlResultSet;
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }
    }
}
