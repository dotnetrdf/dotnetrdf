using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SemWeb;
using SemWeb.Query;
using VDS.RDF.Interop;
using VDS.RDF.Interop.SemWeb;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;

namespace VDS.RDF.Test.Interop
{
    [TestClass]
    public class SemWebInteropTests
    {
        [TestMethod]
        public void InteropSemWebReadVia()
        {
            MemoryStore mem = new MemoryStore();
            mem.Import(new N3Reader("InferenceTest.ttl"));

            Console.WriteLine("SemWeb got the following Statements:");
            foreach (Statement stmt in mem)
            {
                Console.WriteLine(stmt.ToString());
            }
            Console.WriteLine();

            Graph g = new Graph();
            SemWebConverter.FromSemWeb(mem, g);

            Console.WriteLine("dotNetRDF got the following Triples via Conversion:");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }
            Console.WriteLine();

            Graph h = new Graph();
            FileLoader.Load(h, "InferenceTest.ttl");

            Console.WriteLine("dotNetRDF got the following Triples directly from the File:");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }
            Console.WriteLine();

            Assert.AreEqual(g, h, "Graphs should have ben equal");
        }

        [TestMethod]
        public void InteropSemWebWriteVia()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            MemoryStore mem = new MemoryStore();
            SemWebConverter.ToSemWeb(g, mem);

            RdfWriter writer = new RdfXmlWriter("semweb.rdf");
            writer.Write(mem);
            writer.Close();

            //Read the output graph back in to check for equality
            Graph h = new Graph();
            FileLoader.Load(h, "semweb.rdf");

            Assert.AreEqual(g, h, "Graphs should have been equal");
        }

        [TestMethod]
        public void InteropSemWebGraphConversion()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "Turtle.ttl");

            MemoryStore mem = new MemoryStore();
            SemWebConverter.ToSemWeb(g, mem);

            Graph h = new Graph();
            SemWebConverter.FromSemWeb(mem, h);

            Assert.AreEqual(g, h, "1 - Graphs should have been equal");

            MemoryStore mem2 = new MemoryStore();
            SemWebConverter.ToSemWeb(h, mem2);

            Graph i = new Graph();
            SemWebConverter.FromSemWeb(mem2, i);

            Assert.AreEqual(h, i, "2 - Graphs should have been equal");
        }

        [TestMethod]
        public void InteropSemWebGraphSource()
        {
            Graph g = new Graph();
            //FileLoader.Load(g, "InferenceTest.ttl");
            GraphSource source = new GraphSource(g);

            Console.WriteLine("Reading the input using SemWeb");
            N3Reader reader = new N3Reader("InferenceTest.ttl");
            reader.Select(source);
            Console.WriteLine();

            Console.WriteLine("Outputting all Triples using N3Writer");
            N3Writer writer = new N3Writer(Console.Out);
            source.Select(writer);
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Outputting all Triples of the form ?s rdf:type ?type");
            Statement template = new Statement(new Variable(), new Entity(RdfSpecsHelper.RdfType), new Variable());
            source.Select(template, writer);
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Outputting all Triples of the form ?s rdf:type ?car");
            template = new Statement(new Variable(), new Entity(RdfSpecsHelper.RdfType), new Entity("http://example.org/vehicles/Car"));
            source.Select(template, writer);
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Outputting all Triples for Cars and Planes");
            SelectFilter filter = new SelectFilter();
            filter.Predicates = new Entity[] { new Entity(RdfSpecsHelper.RdfType) };
            filter.Objects = new Entity[] { new Entity("http://example.org/vehicles/Car"), new Entity("http://example.org/vehicles/Plane") };
            source.Select(filter, writer);
            Console.WriteLine();
            Console.WriteLine();

            writer.Close();
        }

        [TestMethod]
        public void InteropSemWebInMemoryStoreConversion()
        {
            //Set up a Store and load 3 Graphs into it
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            store.Add(g);
            g = new Graph();
            FileLoader.Load(g, "Turtle.ttl");
            store.Add(g);
            g = new Graph();
            g.Assert(new Triple(g.CreateUriNode(new Uri("http://example.org/#this")), g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), g.CreateUriNode(new Uri("http://example.org/Graph"))));
            store.Add(g);

            InMemoryStoreSource source = new InMemoryStoreSource(store);

            //Put this all into a SemWeb MemoryStore
            MemoryStore mem = new MemoryStore();
            source.Select(mem);

            SemWebConsolePrinter printer = new SemWebConsolePrinter();
            mem.Select(printer);

            //Read it back into a dotNetRDF TripleStore
            InMemoryStoreSource source2 = new InMemoryStoreSource(new TripleStore());
            mem.Select(source2);

            //Check the two stores are equivalent
            IInMemoryQueryableStore store2 = source2.Store;
            foreach (IGraph graph in store.Graphs)
            {
                String baseUri = (graph.BaseUri == null) ? String.Empty : graph.BaseUri.ToString();
                Assert.IsTrue(store2.HasGraph(graph.BaseUri), "Second Store has Graph '" + baseUri + "' missing");

                Assert.AreEqual(graph, store2.Graph(graph.BaseUri), "Graph '" + baseUri + "' was not the same Graph in both Stores");
            }
            
        }

        [TestMethod]
        public void InteropSemWebGraphQuery() 
        {
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            GraphSource source = new GraphSource(g);

            Console.WriteLine("Query for Cars and their Speeds");
            Console.WriteLine();
            SemWeb.Query.SparqlXmlQuerySink sink = new SemWeb.Query.SparqlXmlQuerySink(Console.Out);
            Variable car = new Variable("car");
            Statement[] graphPattern = new Statement[] 
            {
                new Statement(car, new Entity(RdfSpecsHelper.RdfType), new Entity("http://example.org/vehicles/Car")),
                new Statement(car, new Entity("http://example.org/vehicles/Speed"), new Variable("speed"))
            };

            source.Query(graphPattern, new SemWeb.Query.QueryOptions(), sink);

            Console.WriteLine("Query for the 1st Car and it's Speed");
            Console.WriteLine();
            sink = new SemWeb.Query.SparqlXmlQuerySink(Console.Out);
            graphPattern = new Statement[] 
            {
                new Statement(car, new Entity(RdfSpecsHelper.RdfType), new Entity("http://example.org/vehicles/Car")),
                new Statement(car, new Entity("http://example.org/vehicles/Speed"), new Variable("speed"))
            };
            SemWeb.Query.QueryOptions options = new SemWeb.Query.QueryOptions();
            options.Limit = 1;
            source.Query(graphPattern, options, sink);
        }

        [TestMethod]
        public void InteropSemWebGraph()
        {
            Console.WriteLine("Testing using SemWeb as the underlying Storage for a Graph");
            MemoryStore mem = new MemoryStore();

            SemWebGraph g = new SemWebGraph(mem, mem);

            Console.WriteLine("Attempting to parse Triples into the Graph");
            FileLoader.Load(g, "InferenceTest.ttl");

            Console.WriteLine("Parsing finished OK");
            Console.WriteLine("Retrieved " + g.Triples.Count + " Triples");
            Console.WriteLine();

            Console.WriteLine("Attempting to enumerate Triples");

            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }

            Console.WriteLine();
            Console.WriteLine("And again for good measure:");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }

            Console.WriteLine();
            Console.WriteLine("And now things which are cars");
            foreach (Triple t in g.GetTriplesWithPredicateObject(g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType)), g.CreateUriNode(new Uri("http://example.org/vehicles/Car"))))
            {
                Console.WriteLine(t.ToString());
            }
        }
    }

    class SemWebConsolePrinter : StatementSink
    {
        public bool Add(Statement statement)
        {
            Console.WriteLine(statement.ToString());
            return true;
        }
    }

    class SemWebResultsConsolePrinter : QueryResultSink
    {

        public override bool Add(VariableBindings result)
        {
            StringBuilder output = new StringBuilder();
            foreach (Variable var in result.Variables)
            {
                output.Append("?" + var.LocalName + " = ");
                output.Append(result[var]);
                output.Append(" , ");
            }
            Console.WriteLine(output.ToString());
            return true;
        }
    }
}
