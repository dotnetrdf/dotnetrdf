﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;
using Xunit;
using Xunit.Abstractions;

namespace VDS.RDF.Query
{
    public class MathEvaluationTests
    {
        private readonly SparqlQueryParser _parser = new SparqlQueryParser();
        private readonly LeviathanQueryProcessor _processor = new LeviathanQueryProcessor(new InMemoryDataset());
        private readonly ITestOutputHelper _output;

        public MathEvaluationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private void TestQuery(string query, string expectedValue)
        {
            var q = _parser.ParseFromString(query);

            _output.WriteLine(q.ToString());
            _output.WriteLine("");
            _output.WriteLine(q.ToAlgebra().ToString());

            var resultsSet = _processor.ProcessQuery(q) as SparqlResultSet;
            resultsSet.Should().NotBeNull().And.HaveCount(1);
            resultsSet[0].Variables.Should().Contain("f");
            resultsSet[0]["f"].Should().BeAssignableTo<ILiteralNode>().Which.Value.Should().Be(expectedValue);
        }

        [Fact]
        public void TestDivisionBeforeAddition()
        {
            // 10/5+5 Should be interpreted as (10/5) + 5 = 7, not 10/(5+5) = 1
            TestQuery("SELECT ?f WHERE {BIND (10/5+5 as ?f)}", "7");
        }

        [Fact]
        public void TestDivisionBeforeAddition2()
        {
            // 6+10/2 Should be interpreted as 6 + (10/2) = 11, not (6 + 10)/2 = 8
            TestQuery("SELECT ?f WHERE {BIND (6+10/2 as ?f)}", "11");
        }

        [Fact]
        public void TestDivisionBeforeSubtraction()
        {
            // 10-4/2 should be interpreted as 10 - (4/2) = 8, not (10 - 4)/2 = 3
            // TODO: SparqlParser throws an error if the space around the - is missed out, suggesting that the use of the MINUS token is ambiguous
            TestQuery("SELECT ?f WHERE {BIND (10 - 4/2 as ?f)}", "8");
        }

        [Fact]
        public void TestDivisionEvaluatedLeftToRight()
        {
            // 10/5/2 should be interpreted as (10/5)/2 = 1, not 10/(5/2) = 4
            TestQuery("SELECT ?f WHERE {BIND (10/5/2 as ?f)}", "1");
        }
    }
}
