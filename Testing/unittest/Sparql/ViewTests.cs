using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Inference;
using VDS.RDF.Storage;
using VDS.RDF.Test.Storage;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class ViewTests
    {
        [TestMethod]
        public void SparqlViewConstruct()
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
                FileLoader.Load(g, "InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/data");
                store.Add(g);

                Thread.Sleep(200);
                if (view.Triples.Count == 0) view.UpdateView();

                Console.WriteLine("SPARQL View Populated");
                TestTools.ShowGraph(view);

                Assert.IsTrue(view.Triples.Count > 0, "View should have updated to contain some Triples");
        }

        [TestMethod]
        public void SparqlViewDescribe()
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
                FileLoader.Load(g, "InferenceTest.ttl");
                g.BaseUri = null;
                store.Add(g, true);

                Thread.Sleep(200);
                if (view.Triples.Count == 0) view.UpdateView();

                Console.WriteLine("SPARQL View Populated");
                TestTools.ShowGraph(view);

                Assert.IsTrue(view.Triples.Count > 0, "View should have updated to contain some Triples");
        }

        [TestMethod]
        public void SparqlViewSelect()
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
                FileLoader.Load(g, "InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/data");
                store.Add(g);

                Thread.Sleep(200);
                if (view.Triples.Count == 0) view.UpdateView();

                Console.WriteLine("SPARQL View Populated");
                TestTools.ShowGraph(view);

                Assert.IsTrue(view.Triples.Count > 0, "View should have updated to contain some Triples");
        }

        [TestMethod]
        public void SparqlViewAndReasonerInteraction()
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
                FileLoader.Load(g, "InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/data");
                store.Add(g);

                Thread.Sleep(200);
                if (view.Triples.Count == 0) view.UpdateView();

                Console.WriteLine("SPARQL View Populated");
                TestTools.ShowGraph(view);
                Console.WriteLine();

                Assert.IsTrue(view.Triples.Count > 0, "View should have updated to contain some Triples");
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

        [TestMethod]
        public void SparqlViewNativeAllegroGraph()
        {
                AllegroGraphConnector agraph = AllegroGraphTests.GetConnection();
                PersistentTripleStore store = new PersistentTripleStore(agraph);

                //Load a Graph into the Store to ensure there is some data for the view to retrieve
                Graph g = new Graph();
                FileLoader.Load(g, "InferenceTest.ttl");
                agraph.SaveGraph(g);

                //Create the SPARQL View
                NativeSparqlView view = new NativeSparqlView("CONSTRUCT { ?s ?p ?o } WHERE { GRAPH ?g { ?s ?p ?o . FILTER(IsLiteral(?o)) } }", store);

                Console.WriteLine("SPARQL View Populated");
                TestTools.ShowGraph(view);
                Console.WriteLine();
        }

        [TestMethod]
        public void SparqlViewGraphScope()
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
                FileLoader.Load(g, "InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/data");
                store.Add(g);

                Thread.Sleep(200);
                if (view.Triples.Count == 0) view.UpdateView();
                int lastCount = view.Triples.Count;

                Console.WriteLine("SPARQL View Populated");
                TestTools.ShowGraph(view);

                Assert.IsTrue(view.Triples.Count > 0, "View should have updated to contain some Triples");

                //Load another Graph with a different URI into the Store
                Graph h = new Graph();
                h.BaseUri = new Uri("http://example.org/2");
                FileLoader.Load(h, "Turtle.ttl");
                store.Add(h);

                Thread.Sleep(200);
                view.UpdateView();

                Assert.IsTrue(view.Triples.Count == lastCount, "View should not have changed since the added Graph is not in the set of Graphs over which the query operates");

                //Remove this Graph and check the View still doesn't change
                store.Remove(h.BaseUri);

                Thread.Sleep(200);
                view.UpdateView();

                Assert.IsTrue(view.Triples.Count == lastCount, "View should not have changed since the removed Graph is not in the set of Graphs over which the query operates");
        }
    }
}
