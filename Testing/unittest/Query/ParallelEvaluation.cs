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

namespace VDS.RDF.Query
{
    public class ParallelEvaluation
    {
        private InMemoryDataset _dataset;
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlFormatter _formatter = new SparqlFormatter();
        private LeviathanQueryProcessor _processor;
        private const int TripleLimit = 150;
        private NodeFactory _factory = new NodeFactory();

        private void EnsureTestData()
        {
            if (this._dataset == null)
            {
                this._dataset = new InMemoryDataset();
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.Retract(g.Triples.Where(t => !t.IsGroundTriple).ToList());
                if (g.Triples.Count > TripleLimit) g.Retract(g.Triples.Skip(TripleLimit).ToList());
                this._dataset.AddGraph(g);

                this._processor = new LeviathanQueryProcessor(this._dataset);
            }
        }

        private void TestQuery(String query)
        {
            this.EnsureTestData();

            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine("Query:");
            Console.WriteLine(this._formatter.Format(q));
            Console.WriteLine();

            Console.WriteLine("Normal Algebra:");
            Console.WriteLine(q.ToAlgebra().ToString());
            Console.WriteLine();

            Stopwatch timer = new Stopwatch();

            //Evaluate normally
            timer.Start();
            Object normResults = this._processor.ProcessQuery(q);
            timer.Stop();
            Console.WriteLine("Normal Evaluation took " + timer.Elapsed);
            timer.Reset();

            if (normResults is SparqlResultSet)
            {
                SparqlResultSet rsetNorm = (SparqlResultSet)normResults;
                Console.WriteLine("Normal Evaluation returned " + rsetNorm.Count + " Result(s)");
                Console.WriteLine();

                //Evaluate parallelised
                q.AlgebraOptimisers = new IAlgebraOptimiser[] { new ParallelEvaluationOptimiser() };
                Console.WriteLine("Parallel Algebra:");
                Console.WriteLine(q.ToAlgebra().ToString());
                Console.WriteLine();

                timer.Start();
                Object parResults = this._processor.ProcessQuery(q);
                timer.Stop();
                Console.WriteLine("Parallel Evaluation took " + timer.Elapsed);

                if (parResults is SparqlResultSet)
                {
                    SparqlResultSet rsetPar = (SparqlResultSet)parResults;
                    Console.WriteLine("Parallel Evaluation returned " + rsetPar.Count + " Result(s)");
                    Assert.Equal(rsetNorm.Count, rsetPar.Count);
                    Assert.Equal(rsetNorm, rsetPar);
                }
                else
                {
                    Assert.True(false, "Query did not return a SPARQL Result Set as expected");
                }
            }
            else
            {
                Assert.True(false, "Query did not return a SPARQL Result Set for normal evaluation as expected");
            }
        }

        [Fact]
        public void SparqlParallelEvaluationDivision1()
        {
            INode zero = (0).ToLiteral(this._factory);
            INode one = (0).ToLiteral(this._factory);

            List<INode[]> data = new List<INode[]>()
            {
                new INode[] { zero, zero, zero },
                new INode[] { zero, one, zero },
                new INode[] { one, zero, null },
                new INode[] { one, one, one }
            };

            BaseMultiset multiset = new Multiset();
            foreach (INode[] row in data)
            {
                Set s = new Set();
                s.Add("x", row[0]);
                s.Add("y", row[1]);
                s.Add("expected", row[2]);
            }

            ISparqlExpression expr = new DivisionExpression(new VariableTerm("x"), new VariableTerm("y"));

            for (int i = 1; i <= 10000; i++)
            {
                Console.WriteLine("Iteration #" + i);
                SparqlEvaluationContext context = new SparqlEvaluationContext(null, null);
                context.InputMultiset = multiset;
                context.OutputMultiset = new Multiset();

                context.InputMultiset.SetIDs.AsParallel().ForAll(id => this.EvalExtend(context, context.InputMultiset, expr, "actual", id));

                foreach (ISet s in context.OutputMultiset.Sets)
                {
                    Assert.Equal(s["expected"], s["actual"]);
                }
                Console.WriteLine("Iteration #" + i + " Completed OK");
            }
        }

        private void EvalExtend(SparqlEvaluationContext context, BaseMultiset results, ISparqlExpression expr, String var, int id)
        {
            ISet s = results[id].Copy();
            try
            {
                //Make a new assignment
                INode temp = expr.Evaluate(context, id);
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
            String data = @"<http://a> <http://p> <http://x> .
<http://b> <http://p> <http://y> .
<http://c> <http://p> <http://z> .
<http://x> <http://value> ""X"" .
<http://z> <http://value> ""Z"" .";

            String query = "SELECT * WHERE { ?s <http://p> ?o . OPTIONAL { ?o <http://value> ?value } }";
            SparqlQuery q = this._parser.ParseFromString(query);

            TripleStore store = new TripleStore();
            StringParser.ParseDataset(store, data, new NQuadsParser());
            InMemoryDataset dataset = new InMemoryDataset(store);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);

            Stopwatch timer = new Stopwatch();
            timer.Start();
            int i;
            for (i = 1; i <= 100000; i++)
            {
                SparqlResultSet results = processor.ProcessQuery(q) as SparqlResultSet;
                Assert.NotNull(results);
                if (results.Count != 3) TestTools.ShowResults(results);
                Assert.Equal(3, results.Count);
            }
            timer.Stop();

            Console.WriteLine("Completed " + i + " Iterations OK");
            Console.WriteLine("Took " + timer.Elapsed);

        }

#if !NETCOREAPP2_0 // Not currently supported for .NET Standard. See issue #137
        [Fact]
        public void SparqlParallelEvaluationJoin1()
        {
            this.TestQuery("SELECT * WHERE { ?s ?p ?o { ?x ?y ?z } }");
        }

        [Fact]
        public void SparqlParallelEvaluationJoin2()
        {
            try
            {
                this.TestQuery("SELECT * WHERE { ?s ?p ?o { ?x ?y ?z } { ?a ?b ?c } }");
            }
            catch (OutOfMemoryException outEx)
            {
                TestTools.ReportError("Out of Memory", outEx);
            }
        }
#endif
    }
}
