using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
{

    [TestClass]
    public class ConstructWithOptionalTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlUpdateParser _updateParser = new SparqlUpdateParser();

        private void TestConstruct(IGraph data, IGraph expected, String query)
        {
            TripleStore store = new TripleStore();
            store.Add(data);

            this.TestConstruct(store, expected, query);
        }

        private void TestConstruct(IInMemoryQueryableStore store, IGraph expected, String query)
        {
            SparqlQuery q = this._parser.ParseFromString(query);

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(store);
            Object results = processor.ProcessQuery(q);
            if (results is IGraph)
            {
                IGraph result = (IGraph)results;

                NTriplesFormatter formatter = new NTriplesFormatter();
                Console.WriteLine("Result Data");
                foreach (Triple t in result.Triples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }
                Console.WriteLine();

                Console.WriteLine("Expected Data");
                foreach (Triple t in expected.Triples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }
                Console.WriteLine();

                Assert.AreEqual(expected, result, "Graphs should be equal");
            }
            else
            {
                Assert.Fail("Did not get a Graph as expected");
            }
        }

        private void TestUpdate(IGraph data, IGraph expected, String update)
        {
            TripleStore store = new TripleStore();
            store.Add(data);

            this.TestUpdate(store, expected, update);
        }

        private void TestUpdate(IInMemoryQueryableStore store, IGraph expected, String update)
        {
            SparqlUpdateCommandSet cmds = this._updateParser.ParseFromString(update);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(store);
            processor.ProcessCommandSet(cmds);

            Assert.IsTrue(store.HasGraph(null), "Store should have a default unnamed Graph");
            IGraph result = store.Graph(null);
            
            NTriplesFormatter formatter = new NTriplesFormatter();
            Console.WriteLine("Result Data");
            foreach (Triple t in result.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
            Console.WriteLine();

            Console.WriteLine("Expected Data");
            foreach (Triple t in expected.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
            Console.WriteLine();

            Assert.AreEqual(expected, result, "Graphs should be equal");
        }

        [TestMethod]
        public void SparqlConstructWithOptional()
        {
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");

            Graph expected = new Graph();
            expected.Assert(g.GetTriplesWithPredicate(g.CreateUriNode("rdf:type")));
            expected.Assert(g.GetTriplesWithPredicate(g.CreateUriNode("eg:Speed")));

            String query = "PREFIX ex: <http://example.org/vehicles/> CONSTRUCT { ?s a ?type . ?s ex:Speed ?speed } WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } }";

            this.TestConstruct(g, expected, query);
        }

        [TestMethod]
        public void SparqlUpdateInsertWithOptional()
        {
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/vehicles/");

            Graph expected = new Graph();
            expected.Assert(g.GetTriplesWithPredicate(g.CreateUriNode("rdf:type")));
            expected.Assert(g.GetTriplesWithPredicate(g.CreateUriNode("eg:Speed")));

            String update = "PREFIX ex: <http://example.org/vehicles/> INSERT { ?s a ?type . ?s ex:Speed ?speed } USING <http://example.org/vehicles/> WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } }";

            this.TestUpdate(g, expected, update);
        }

        [TestMethod]
        public void SparqlUpdateDeleteWithOptional()
        {
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/vehicles/");

            Graph def = new Graph();
            def.Merge(g);

            TripleStore store = new TripleStore();
            store.Add(g);
            store.Add(def);

            Graph expected = new Graph();
            expected.NamespaceMap.Import(g.NamespaceMap);
            expected.Merge(g);
            expected.Retract(expected.GetTriplesWithPredicate(expected.CreateUriNode("rdf:type")));
            expected.Retract(expected.GetTriplesWithPredicate(expected.CreateUriNode("eg:Speed")));

            String update = "PREFIX ex: <http://example.org/vehicles/> DELETE { ?s a ?type . ?s ex:Speed ?speed } USING <http://example.org/vehicles/> WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } }";

            this.TestUpdate(store, expected, update);
        }

        [TestMethod]
        public void SparqlUpdateModifyWithOptional()
        {
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/vehicles/");

            Graph expected = new Graph();
            expected.Assert(g.GetTriplesWithPredicate(g.CreateUriNode("rdf:type")));
            expected.Assert(g.GetTriplesWithPredicate(g.CreateUriNode("eg:Speed")));

            String update = "PREFIX ex: <http://example.org/vehicles/> DELETE { } INSERT { ?s a ?type . ?s ex:Speed ?speed } USING <http://example.org/vehicles/> WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } }";

            this.TestUpdate(g, expected, update);
        }

        [TestMethod]
        public void SparqlUpdateModifyWithOptional2()
        {
            Graph g = new Graph();
            g.LoadFromFile("InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/vehicles/");

            Graph def = new Graph();
            def.Merge(g);

            TripleStore store = new TripleStore();
            store.Add(g);
            store.Add(def);

            Graph expected = new Graph();
            expected.NamespaceMap.Import(g.NamespaceMap);
            expected.Merge(g);
            expected.Retract(expected.GetTriplesWithPredicate(expected.CreateUriNode("rdf:type")));
            expected.Retract(expected.GetTriplesWithPredicate(expected.CreateUriNode("eg:Speed")));

            String update = "PREFIX ex: <http://example.org/vehicles/> DELETE { ?s a ?type . ?s ex:Speed ?speed } INSERT { } USING <http://example.org/vehicles/> WHERE { ?s a ?type . OPTIONAL { ?s ex:Speed ?speed } }";

            this.TestUpdate(store, expected, update);
        }
    }
}
