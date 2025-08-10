/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Arithmetic;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Writing.Formatting;
using Xunit;

namespace VDS.RDF.Query;

public class ParallelEvaluation
{
    private InMemoryDataset _dataset;
    private SparqlQueryParser _parser = new SparqlQueryParser();
    private SparqlFormatter _formatter = new SparqlFormatter();
    private LeviathanQueryProcessor _processor;
    private const int TripleLimit = 100;
    private NodeFactory _factory = new NodeFactory();
    private readonly ITestOutputHelper _output;

    public ParallelEvaluation(ITestOutputHelper output)
    {
        _output = output;
    }

    private void EnsureTestData()
    {
        if (_dataset == null)
        {
            _dataset = new InMemoryDataset();
            var g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.Retract(g.Triples.Where(t => !t.IsGroundTriple).ToList());
            if (g.Triples.Count > TripleLimit) g.Retract(g.Triples.Skip(TripleLimit).ToList());
            _dataset.AddGraph(g);
            _dataset.SetDefaultGraph(g.Name);

            _processor = new LeviathanQueryProcessor(_dataset);
        }
    }

    private void TestQuery(string query, bool checkGraphEquality)
    {
        EnsureTestData();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var q = _parser.ParseFromString(query);

        _output.WriteLine("Query:");
        _output.WriteLine(_formatter.Format(q));
        _output.WriteLine(string.Empty);

        _output.WriteLine("Normal Algebra:");
        _output.WriteLine(q.ToAlgebra().ToString());
        _output.WriteLine(string.Empty);

        var timer = new Stopwatch();

        //Evaluate normally
        timer.Start();
        var normResults = _processor.ProcessQuery(q);
        timer.Stop();
        _output.WriteLine("Normal Evaluation took " + timer.Elapsed);
        timer.Reset();

        if (normResults is SparqlResultSet)
        {
            var rsetNorm = (SparqlResultSet)normResults;
            _output.WriteLine("Normal Evaluation returned " + rsetNorm.Count + " Result(s)");
            _output.WriteLine(string.Empty);

            //Evaluate parallelised
            q.AlgebraOptimisers = new IAlgebraOptimiser[] { new ParallelEvaluationOptimiser() };
            _output.WriteLine("Parallel Algebra:");
            _output.WriteLine(q.ToAlgebra().ToString());
            _output.WriteLine(string.Empty);

            timer.Start();
            var parResults = _processor.ProcessQuery(q);
            timer.Stop();
            _output.WriteLine("Parallel Evaluation took " + timer.Elapsed);

            if (parResults is SparqlResultSet rsetPar)
            {
                _output.WriteLine("Parallel Evaluation returned " + rsetPar.Count + " Result(s)");
                Assert.Equal(rsetNorm.Count, rsetPar.Count);
                if (checkGraphEquality)
                {
                    Assert.StrictEqual(rsetNorm, rsetPar);
                }
            }
            else
            {
                Assert.Fail("Query did not return a SPARQL Result Set as expected");
            }
        }
        else
        {
            Assert.Fail("Query did not return a SPARQL Result Set for normal evaluation as expected");
        }
    }

    [Fact]
    public void SparqlParallelEvaluationDivision1()
    {
        INode zero = (0).ToLiteral(_factory);
        INode one = (0).ToLiteral(_factory);

        var data = new List<INode[]>()
        {
            new INode[] { zero, zero, zero },
            new INode[] { zero, one, zero },
            new INode[] { one, zero, null },
            new INode[] { one, one, one }
        };

        BaseMultiset multiset = new Multiset();
        foreach (INode[] row in data)
        {
            var s = new Set();
            s.Add("x", row[0]);
            s.Add("y", row[1]);
            s.Add("expected", row[2]);
        }

        ISparqlExpression expr = new DivisionExpression(new VariableTerm("x"), new VariableTerm("y"));

        var processor = new LeviathanExpressionProcessor(new LeviathanQueryOptions(), null);

        for (var i = 1; i <= 10000; i++)
        {
            var context = new SparqlEvaluationContext(null, new LeviathanQueryOptions())
            {
                InputMultiset = multiset,
                OutputMultiset = new Multiset()
            };

            context.InputMultiset.SetIDs.AsParallel().ForAll(id => EvalExtend(processor, context, context.InputMultiset, expr, "actual", id));

            foreach (ISet s in context.OutputMultiset.Sets)
            {
                Assert.Equal(s["expected"], s["actual"]);
            }
        }
    }

    private void EvalExtend(LeviathanExpressionProcessor processor, SparqlEvaluationContext context, BaseMultiset results, ISparqlExpression expr, String var, int id)
    {
        ISet s = results[id].Copy();
        try
        {
            //Make a new assignment
            INode temp = expr.Accept(processor, context, id);
            s.Add(var, temp);
        }
        catch
        {
            //No assignment if there's an error but the solution is preserved
        }
        context.OutputMultiset.Add(s);
    }

    [Fact]
    [Trait("Coverage", "Skip")]
    public void SparqlParallelEvaluationOptional1()
    {
        var data = @"<http://a> <http://p> <http://x> .
<http://b> <http://p> <http://y> .
<http://c> <http://p> <http://z> .
<http://x> <http://value> ""X"" .
<http://z> <http://value> ""Z"" .";

        var query = "SELECT * WHERE { ?s <http://p> ?o . OPTIONAL { ?o <http://value> ?value } }";
        var q = _parser.ParseFromString(query);

        var store = new TripleStore();
        StringParser.ParseDataset(store, data, new NQuadsParser());
        var dataset = new InMemoryDataset(store);
        var processor = new LeviathanQueryProcessor(dataset);

        var timer = new Stopwatch();
        timer.Start();
        int i;
        for (i = 1; i <= 10000; i++)
        {
            var results = processor.ProcessQuery(q) as SparqlResultSet;
            Assert.NotNull(results);
            if (results.Count != 3) TestTools.ShowResults(results);
            Assert.Equal(3, results.Count);
        }
        timer.Stop();

        _output.WriteLine("Completed " + i + " Iterations OK");
        _output.WriteLine("Took " + timer.Elapsed);

    }

    [Fact]
    public void SparqlParallelEvaluationJoin1()
    {
        TestQuery("SELECT * WHERE { ?s ?p ?o { ?x ?y ?z } }", true);
    }

    [Fact]
    public void SparqlParallelEvaluationJoin2()
    {
        try
        {
            TestQuery("SELECT * WHERE { ?s ?p ?o { ?x ?y ?z } { ?a ?b ?c } }", false);
        }
        catch (OutOfMemoryException outEx)
        {
            TestTools.ReportError("Out of Memory", outEx);
        }
    }

    [Fact]
    public void SparqlParallelUnionEvaluation()
    {
        TestQuery("SELECT * WHERE { { ?s ?p ?o } UNION { ?a ?b ?c } }", true);
    }
}
