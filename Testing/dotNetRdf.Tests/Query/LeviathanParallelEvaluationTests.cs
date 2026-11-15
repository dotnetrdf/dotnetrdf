/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2026 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using Xunit;

namespace VDS.RDF.Query;

public class LeviathanParallelEvaluationTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IntermediateJoinAndProductResultsRespectParallelEvaluationOption(bool usePLinqEvaluation)
    {
        using var graph = new Graph();
        StringParser.Parse(graph, """
            @prefix ex: <https://example.org/> .
            ex:a ex:name "first" ; ex:category "tool" ; ex:label "A" .
            ex:b ex:name "second" ; ex:category "tool" .
            """, new TurtleParser());
        var processor = new JoinTrackingProcessor(new InMemoryDataset(graph), usePLinqEvaluation);
        SparqlQuery query = new SparqlQueryParser().ParseFromString("""
            PREFIX ex: <https://example.org/>
            SELECT ?name ?label ?value WHERE {
                { ?s ex:name ?name }
                { ?s ex:category "tool" }
                OPTIONAL { ?s ex:label ?label }
                { VALUES ?value { 1 2 } }
            }
            """);

        var results = Assert.IsType<SparqlResultSet>(processor.ProcessQuery(query));

        Assert.Equal(4, results.Count);
        Assert.Equal(2, results.Count(result => ((ILiteralNode)result["name"]).Value == "first" && ((ILiteralNode)result["label"]).Value == "A"));
        Assert.Equal(2, results.Count(result => ((ILiteralNode)result["name"]).Value == "second" && !result.TryGetBoundValue("label", out _)));
        Assert.Equal(new[] { "1", "2" }, results.Select(result => ((ILiteralNode)result["value"]).Value).Distinct().OrderBy(value => value));
        Assert.Contains(processor.JoinResults, result => result.Count == 2);
        BaseMultiset product = Assert.Single(processor.JoinResults, result => result.Count == 4);
        Assert.Equal(usePLinqEvaluation, product is PartitionedMultiset);
        Assert.Single(processor.LeftJoinResults);
        Assert.All(processor.JoinResults.Concat(processor.LeftJoinResults), result => Assert.Equal(usePLinqEvaluation, result.UsePLinqEvaluation));
    }

    [Fact]
    public void CachedBindingsRespectTheEvaluationContext()
    {
        using var graph = new Graph();
        var dataset = new InMemoryDataset(graph);
        var processor = new LeviathanQueryProcessor(dataset);
        SparqlQuery query = new SparqlQueryParser().ParseFromString("SELECT ?value WHERE { } VALUES ?value { 1 }");
        var bindings = new Bindings(query.Bindings);
        var parallelContext = new SparqlEvaluationContext(query, dataset, new LeviathanQueryOptions { UsePLinqEvaluation = true });
        var serialContext = new SparqlEvaluationContext(query, dataset, new LeviathanQueryOptions { UsePLinqEvaluation = false });

        BaseMultiset parallel = processor.ProcessBindings(bindings, parallelContext);
        BaseMultiset serial = processor.ProcessBindings(bindings, serialContext);

        Assert.True(parallel.UsePLinqEvaluation);
        Assert.False(serial.UsePLinqEvaluation);
        Assert.Equal(parallel.Sets.Single()["value"], serial.Sets.Single()["value"]);
        Assert.Same(parallel, processor.ProcessBindings(bindings, parallelContext));
    }

    private sealed class JoinTrackingProcessor(ISparqlDataset dataset, bool usePLinqEvaluation)
        : LeviathanQueryProcessor(dataset, options =>
        {
            options.UsePLinqEvaluation = usePLinqEvaluation;
            options.AlgebraOptimisation = false;
        })
    {
        public List<BaseMultiset> JoinResults { get; } = [];
        public List<BaseMultiset> LeftJoinResults { get; } = [];

        public override BaseMultiset ProcessJoin(IJoin join, SparqlEvaluationContext context)
        {
            BaseMultiset result = base.ProcessJoin(join, context);
            JoinResults.Add(result);
            return result;
        }

        public override BaseMultiset ProcessLeftJoin(ILeftJoin leftJoin, SparqlEvaluationContext context)
        {
            BaseMultiset result = base.ProcessLeftJoin(leftJoin, context);
            LeftJoinResults.Add(result);
            return result;
        }
    }
}
