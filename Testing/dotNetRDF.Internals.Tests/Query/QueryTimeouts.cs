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
using System.Linq;
using System.Text;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query
{

    public class QueryTimeouts
    {
        private long[] _timeouts = new long[] { 50, 100, 250, 500, 1000 };
        private SparqlQueryParser _parser = new SparqlQueryParser();

        private ISparqlDataset AsDataset(IInMemoryQueryableStore store)
        {
            if (store.Graphs.Count == 1)
            {
                return new InMemoryDataset(store, store.Graphs.First().BaseUri);
            }
            else
            {
                return new InMemoryDataset(store);
            }
        }

        private void TestProductTimeout(IGraph data, String query, bool useGlobal, int expectedResults)
        {
            Console.WriteLine("Maximum Expected Results: " + expectedResults);
            Console.WriteLine("Initial Global Timeout: " + Options.QueryExecutionTimeout);
            Console.WriteLine();

            long globalOrig = Options.QueryExecutionTimeout;
            try
            {
                if (useGlobal)
                {
                    Console.WriteLine("Global Timeout setting in use");
                }
                else
                {
                    Console.WriteLine("Per Query Timeout setting in use");
                }
                Console.WriteLine();

                TripleStore store = new TripleStore();
                store.Add(data);

                SparqlQuery q = this._parser.ParseFromString(query);
                LeviathanQueryProcessor processor = new LeviathanQueryProcessor(AsDataset(store));

                SparqlFormatter formatter = new SparqlFormatter();
                Console.WriteLine("Query:");
                Console.WriteLine(formatter.Format(q));

                //Evaluate for each Timeout
                foreach (long t in this._timeouts)
                {
                    //Set the Timeout and ask for Partial Results
                    if (useGlobal)
                    {
                        Options.QueryExecutionTimeout = t;
                    }
                    else
                    {
                        q.Timeout = t;
                    }
                    q.PartialResultsOnTimeout = true;

                    //Check that the reported Timeout matches the expected
                    SparqlEvaluationContext context = new SparqlEvaluationContext(q, null);
                    long expected;
                    if (useGlobal)
                    {
                        expected = t;
                    }
                    else
                    {
                        if (Options.QueryExecutionTimeout > 0 && t <= Options.QueryExecutionTimeout)
                        {
                            expected = t;
                        }
                        else if (Options.QueryExecutionTimeout == 0)
                        {
                            expected = t;
                        }
                        else
                        {
                            expected = Options.QueryExecutionTimeout;
                        }
                    }
                    Assert.Equal(expected, context.QueryTimeout);

                    //Run the Query
                    Object results = processor.ProcessQuery(q);
                    Assert.IsAssignableFrom<SparqlResultSet>(results);
                    if (results is SparqlResultSet)
                    {
                        SparqlResultSet rset = (SparqlResultSet)results;

                        Console.WriteLine("Requested Timeout: " + t + " - Actual Timeout: " + expected + "ms - Results: " + rset.Count + " - Query Time: " + q.QueryExecutionTime);
                        Assert.True(rset.Count <= expectedResults, "Results should be <= expected");
                    }
                }
            }
            finally
            {
                Options.QueryExecutionTimeout = globalOrig;
            }
        }

        private void TestProductTimeoutGlobalOverride(IGraph data, String query, long globalTimeout, int expectedResults)
        {
            long origGlobal = Options.QueryExecutionTimeout;
            try
            {
                Options.QueryExecutionTimeout = globalTimeout;
                this.TestProductTimeout(data, query, false, expectedResults);
            }
            finally
            {
                Options.QueryExecutionTimeout = origGlobal;
            }
        }

        [Fact]
        public void SparqlQueryTimeout()
        {
            String query = "SELECT * WHERE { ?s ?p ?o . ?s ?p2 ?o2 . ?a ?b ?c }";
            SparqlQuery q = this._parser.ParseFromString(query);
            q.Timeout = 1;

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            store.Add(g);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(AsDataset(store));
            Assert.Throws<RdfQueryTimeoutException>(() =>
            {
                    //Try multiple times because sometimes machine load may mean we don't timeout
                    for (int i = 0; i < 10; i++)
                {
                    processor.ProcessQuery(q);
                }
            });
        }

        [Fact]
        public void SparqlQueryTimeoutDuringProduct()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            String query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z }";
            this.TestProductTimeout(g, query, false, g.Triples.Count * g.Triples.Count);
        }

        [Fact]
        public void SparqlQueryTimeoutDuringProduct2()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            String query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z . ?a ?b ?c }";
            this.TestProductTimeout(g, query, false, g.Triples.Count * g.Triples.Count * g.Triples.Count);
        }

        [Fact]
        public void SparqlQueryTimeoutGlobalDuringProduct()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            String query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z }";
            this.TestProductTimeout(g, query, true, g.Triples.Count * g.Triples.Count);
        }

        [Fact]
        public void SparqlQueryTimeoutGlobalDuringProduct2()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            String query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z . ?a ?b ?c }";
            this.TestProductTimeout(g, query, true, g.Triples.Count * g.Triples.Count * g.Triples.Count);
        }

        [Fact]
        public void SparqlQueryTimeoutDuringProductOverriddenByGlobal()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            String query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z }";
            this.TestProductTimeoutGlobalOverride(g, query, 1000, g.Triples.Count * g.Triples.Count);
        }

        [Fact]
        public void SparqlQueryTimeoutDuringProductOverriddenByGlobal2()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            String query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z . ?a ?b ?c }";
            this.TestProductTimeoutGlobalOverride(g, query, 1000, g.Triples.Count * g.Triples.Count * g.Triples.Count);
        }

        [Fact]
        public void SparqlQueryTimeoutDuringProductNotOverriddenByGlobal()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            String query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z }";
            this.TestProductTimeoutGlobalOverride(g, query, 0, g.Triples.Count * g.Triples.Count);
        }

        [Fact]
        public void SparqlQueryTimeoutDuringProductNotOverriddenByGlobal2()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            String query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z . ?a ?b ?c }";
            this.TestProductTimeoutGlobalOverride(g, query, 0, g.Triples.Count * g.Triples.Count * g.Triples.Count);
        }

        [Fact]
        public void SparqlQueryTimeoutDuringProductLazy()
        {
            String query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z . ?a ?b ?c }";
            SparqlQuery q = this._parser.ParseFromString(query);
            q.Timeout = 1;

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            store.Add(g);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(AsDataset(store));
            Assert.Throws<RdfQueryTimeoutException>(() =>
            {
                processor.ProcessQuery(q);
            });
        }

        [Fact(Skip = "in practise it is suprisingly easy to compute this in under a millisecond given a reasonable machine since it only needs to compute one value")]
        public void SparqlQueryTimeoutDuringProductLazy2()
        {
            String query = "ASK WHERE { ?s ?p ?o . ?x ?y ?z }";
            SparqlQuery q = this._parser.ParseFromString(query);
            q.Timeout = 1;
            Console.WriteLine(q.ToAlgebra().ToString());

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            store.Add(g);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(AsDataset(store));
            Assert.Throws<RdfQueryTimeoutException>(() =>
            {
                //Try multiple times because sometimes machine load may mean we don't timeout
                for (int i = 0; i < 100; i++)
                {
                    processor.ProcessQuery(q);
                }
            });
        }

        [Fact]
        public void SparqlQueryTimeoutNone()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            String query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z }";

            long currTimeout = Options.QueryExecutionTimeout;
            try
            {
                Options.QueryExecutionTimeout = 0;

                SparqlQuery q = this._parser.ParseFromString(query);
                TripleStore store = new TripleStore();
                store.Add(g);

                LeviathanQueryProcessor processor = new LeviathanQueryProcessor(AsDataset(store));
                Object results = processor.ProcessQuery(q);
                Assert.IsAssignableFrom<SparqlResultSet>(results);
                if (results is SparqlResultSet)
                {
                    SparqlResultSet rset = (SparqlResultSet)results;
                    Console.WriteLine("Results: " + rset.Count + " - Query Time: " + q.QueryExecutionTime);
                    Assert.Equal(g.Triples.Count * g.Triples.Count, rset.Count);
                }
            }
            finally
            {
                Options.QueryExecutionTimeout = currTimeout;
            }
        }

        [Fact]
        public void SparqlQueryTimeoutMinimal()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            String query = "SELECT * WHERE { ?s ?p ?o . ?x ?y ?z . ?a ?b ?c }";

            long currTimeout = Options.QueryExecutionTimeout;
            try
            {
                Options.QueryExecutionTimeout = 1;

                SparqlQuery q = this._parser.ParseFromString(query);
                q.PartialResultsOnTimeout = true;
                TripleStore store = new TripleStore();
                store.Add(g);

                LeviathanQueryProcessor processor = new LeviathanQueryProcessor(AsDataset(store));
                Object results = processor.ProcessQuery(q);
                Assert.IsAssignableFrom<SparqlResultSet>(results);
                if (results is SparqlResultSet)
                {
                    SparqlResultSet rset = (SparqlResultSet)results;
                    Console.WriteLine("Results: " + rset.Count + " - Query Time: " + q.QueryExecutionTime);
                    Assert.True(rset.Count < (g.Triples.Count * g.Triples.Count * g.Triples.Count));
                }
            }
            finally
            {
                Options.QueryExecutionTimeout = currTimeout;
            }
        }
    }
}
