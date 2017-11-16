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
#if !NETCOREAPP2_0 // View invalidation currently not supported in .NET Standard 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Inference;
using VDS.RDF.Storage;
using VDS.RDF.XunitExtensions;

namespace VDS.RDF.Query
{
    public partial class ViewTests
    {
        [Fact]
        public void SparqlViewConstruct1()
        {
                TripleStore store = new TripleStore();
                SparqlView view = new SparqlView("CONSTRUCT { ?s ?p ?o } WHERE { GRAPH ?g { ?s ?p ?o . FILTER(IsLiteral(?o)) } }", store);
                view.BaseUri = new Uri("http://example.org/view");
                store.Add(view);

                Console.WriteLine("SPARQL View Empty");
                TestTools.ShowGraph(view);
                Console.WriteLine();

                //Load a Graph into the Store to cause the SPARQL View to update
                Graph g = new Graph();
                FileLoader.Load(g, "resources\\InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/data");
                store.Add(g);

                Thread.Sleep(1000);
                if (view.Triples.Count == 0) view.UpdateView();

                Console.WriteLine("SPARQL View Populated");
                TestTools.ShowGraph(view);

                Assert.True(view.Triples.Count > 0, "View should have updated to contain some Triples");
        }

        [Fact(Skip = "If the test is not stable, fix the test")]
        public void SparqlViewConstruct2()
        {
            //Since the test has failed intermittently in the past run it a whole bunch of times to be on the safe side
            for (int i = 1; i <= 50; i++)
            {
                this.SparqlViewConstruct1();
            }
        }

        [Fact]
        public void SparqlViewDescribe1()
        {
                TripleStore store = new TripleStore();
                SparqlView view = new SparqlView("DESCRIBE <http://example.org/vehicles/FordFiesta>", store);
                view.BaseUri = new Uri("http://example.org/view");
                store.Add(view);

                Console.WriteLine("SPARQL View Empty");
                TestTools.ShowGraph(view);
                Console.WriteLine();

                //Load a Graph into the Store to cause the SPARQL View to update
                Graph g = new Graph();
                FileLoader.Load(g, "resources\\InferenceTest.ttl");
                g.BaseUri = null;
                store.Add(g, true);

                Thread.Sleep(500);
                if (view.Triples.Count == 0) view.UpdateView();

                Console.WriteLine("SPARQL View Populated");
                TestTools.ShowGraph(view);

                Assert.True(view.Triples.Count > 0, "View should have updated to contain some Triples");
        }

        [Fact(Skip = "If the test is not stable, fix the test")]
        public void SparqlViewDescribe2()
        {
            //Since the test has failed intermittently in the past run it a whole bunch of times to be on the safe side
            for (int i = 1; i <= 50; i++)
            {
                this.SparqlViewDescribe1();
            }
        }

        [Fact]
        public void SparqlViewSelect1()
        {
                TripleStore store = new TripleStore();
                SparqlView view = new SparqlView("SELECT ?s (<http://example.org/vehicles/TurbochargedSpeed>) AS ?p (?speed * 1.25) AS ?o  WHERE { GRAPH ?g { ?s <http://example.org/vehicles/Speed> ?speed } }", store);
                view.BaseUri = new Uri("http://example.org/view");
                store.Add(view);

                Console.WriteLine("SPARQL View Empty");
                TestTools.ShowGraph(view);
                Console.WriteLine();

                //Load a Graph into the Store to cause the SPARQL View to update
                Graph g = new Graph();
                FileLoader.Load(g, "resources\\InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/data");
                store.Add(g);

                Thread.Sleep(500);
                if (view.Triples.Count == 0) view.UpdateView();

                Console.WriteLine("SPARQL View Populated");
                TestTools.ShowGraph(view);

                Assert.True(view.Triples.Count > 0, "View should have updated to contain some Triples");
        }

        [Fact(Skip = "If the test is not stable, fix the test")]
        public void SparqlViewSelect2()
        {
            //Since the test has failed intermittently in the past run it a whole bunch of times to be on the safe side
            for (int i = 1; i <= 50; i++)
            {
                this.SparqlViewSelect1();
            }
        }

        [Fact]
        public void SparqlViewAndReasonerInteraction1()
        {
                TripleStore store = new TripleStore();
                SparqlView view = new SparqlView("CONSTRUCT { ?s a ?type } WHERE { GRAPH ?g { ?s a ?type } }", store);
                view.BaseUri = new Uri("http://example.org/view");
                store.Add(view);

                Console.WriteLine("SPARQL View Empty");
                TestTools.ShowGraph(view);
                Console.WriteLine();

                //Load a Graph into the Store to cause the SPARQL View to update
                Graph g = new Graph();
                FileLoader.Load(g, "resources\\InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/data");
                store.Add(g);

                Thread.Sleep(500);
                if (view.Triples.Count == 0) view.UpdateView();

                Console.WriteLine("SPARQL View Populated");
                TestTools.ShowGraph(view);
                Console.WriteLine();

                Assert.True(view.Triples.Count > 0, "View should have updated to contain some Triples");
                int lastCount = view.Triples.Count;

                //Apply an RDFS reasoner
                StaticRdfsReasoner reasoner = new StaticRdfsReasoner();
                reasoner.Initialise(g);
                store.AddInferenceEngine(reasoner);

                Thread.Sleep(200);
                if (view.Triples.Count == lastCount) view.UpdateView();
                Console.WriteLine("SPARQL View Populated after Reasoner added");
                TestTools.ShowGraph(view);
        }

        [Fact(Skip = "If the test is not stable, fix the test")]
        public void SparqlViewAndReasonerInteraction2()
        {
            //Since the test has failed intermittently in the past run it a whole bunch of times to be on the safe side
            for (int i = 1; i <= 50; i++)
            {
                this.SparqlViewAndReasonerInteraction1();
            }
        }

        [Fact]
        public void SparqlViewGraphScope1()
        {
                TripleStore store = new TripleStore();
                SparqlView view = new SparqlView("CONSTRUCT { ?s ?p ?o } FROM <http://example.org/data> WHERE { ?s ?p ?o . FILTER(IsLiteral(?o)) }", store);
                view.BaseUri = new Uri("http://example.org/view");
                store.Add(view);

                Console.WriteLine("SPARQL View Empty");
                TestTools.ShowGraph(view);
                Console.WriteLine();

                //Load a Graph into the Store to cause the SPARQL View to update
                Graph g = new Graph();
                FileLoader.Load(g, "resources\\InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/data");
                store.Add(g);

                Thread.Sleep(500);
                if (view.Triples.Count == 0) view.UpdateView();
                int lastCount = view.Triples.Count;

                Console.WriteLine("SPARQL View Populated");
                TestTools.ShowGraph(view);

                Assert.True(view.Triples.Count > 0, "View should have updated to contain some Triples");

                //Load another Graph with a different URI into the Store
                Graph h = new Graph();
                h.BaseUri = new Uri("http://example.org/2");
                FileLoader.Load(h, "resources\\Turtle.ttl");
                store.Add(h);

                Thread.Sleep(500);
                view.UpdateView();

                Assert.True(view.Triples.Count == lastCount, "View should not have changed since the added Graph is not in the set of Graphs over which the query operates");

                //Remove this Graph and check the View still doesn't change
                store.Remove(h.BaseUri);

                Thread.Sleep(500);
                view.UpdateView();

                Assert.True(view.Triples.Count == lastCount, "View should not have changed since the removed Graph is not in the set of Graphs over which the query operates");
        }


        [Fact(Skip = "If the test is not stable, we should fix the test")]
        public void SparqlViewGraphScope2()
        {
            //Since the test has failed intermittently in the past run it a whole bunch of times to be on the safe side
            for (int i = 1; i <= 50; i++)
            {
                this.SparqlViewGraphScope1();
            }
        }
    }
}
#endif