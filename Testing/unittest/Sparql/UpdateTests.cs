using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Update;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class UpdateTests
    {
        public const String InsertPatterns1 = @"ex:IndividualA rdf:type          tpl:MyIndividualClass .
_:template        rdf:type          tpl:MyTemplate .
_:template        tpl:ObjectRole    rdl:MyTypeClass .
_:template        tpl:PossessorRole ex:IndividualA .
_:template        tpl:PropertyRole  'ValueA'^^xsd:String .";

        public const String InsertPatterns2 = @"ex:IndividualB rdf:type          tpl:MyIndividualClass .
_:template        rdf:type          tpl:MyTemplate .
_:template        tpl:ObjectRole    rdl:MyTypeClass .
_:template        tpl:PossessorRole ex:IndividualB .
_:template        tpl:PropertyRole  'ValueB'^^xsd:String .";

        [TestMethod]
        public void SparqlUpdateInsertDataWithBNodes()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            store.Add(g);

            String prefixes = "PREFIX rdf: <" + NamespaceMapper.RDF + ">\n PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA + ">\n PREFIX ex: <http://example.org/>\n PREFIX rdl: <http://example.org/roles>\n PREFIX tpl: <http://example.org/template/>\n";
            String insert = prefixes + "INSERT DATA { " + InsertPatterns1 + "}";
            Console.WriteLine(insert);
            Console.WriteLine();
            store.ExecuteUpdate(insert);
            insert = prefixes + "INSERT DATA {" + InsertPatterns2 + "}";
            Console.WriteLine(insert);
            Console.WriteLine();
            store.ExecuteUpdate(insert);

            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }
        }

        [TestMethod]
        public void SparqlUpdateAddCommand()
        {
            IGraph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/source");

            IGraph h = new Graph();
            FileLoader.Load(h, "Turtle.ttl");
            h.BaseUri = new Uri("http://example.org/destination");

            TripleStore store = new TripleStore();
            store.Add(g);
            store.Add(h);

            Assert.AreNotEqual(g, h, "Graphs should not be equal");

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet commands = parser.ParseFromString("ADD GRAPH <http://example.org/source> TO GRAPH <http://example.org/destination>");

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(store);
            processor.ProcessCommandSet(commands);

            g = store.Graph(new Uri("http://example.org/source"));
            h = store.Graph(new Uri("http://example.org/destination"));
            Assert.IsFalse(g.IsEmpty, "Source Graph should not be empty");
            Assert.IsFalse(h.IsEmpty, "Destination Graph should not be empty");
            Assert.IsTrue(h.HasSubGraph(g), "Destination Graph should have Source Graph as a subgraph");
        }

        [TestMethod]
        public void SparqlUpdateAddCommand2()
        {
            IGraph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/source");

            IGraph h = new Graph();
            FileLoader.Load(h, "Turtle.ttl");
            h.BaseUri = null;

            TripleStore store = new TripleStore();
            store.Add(g);
            store.Add(h);

            Assert.AreNotEqual(g, h, "Graphs should not be equal");

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet commands = parser.ParseFromString("ADD GRAPH <http://example.org/source> TO DEFAULT");

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(store);
            processor.ProcessCommandSet(commands);

            g = store.Graph(new Uri("http://example.org/source"));
            h = store.Graph(null);
            Assert.IsFalse(g.IsEmpty, "Source Graph should not be empty");
            Assert.IsFalse(h.IsEmpty, "Destination Graph should not be empty");
            Assert.IsTrue(h.HasSubGraph(g), "Destination Graph should have Source Graph as a subgraph");
        }

        [TestMethod]
        public void SparqlUpdateAddCommand3()
        {
            IGraph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            IGraph h = new Graph();
            FileLoader.Load(h, "Turtle.ttl");
            h.BaseUri = new Uri("http://example.org/destination");

            TripleStore store = new TripleStore();
            store.Add(g);
            store.Add(h);

            Assert.AreNotEqual(g, h, "Graphs should not be equal");

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet commands = parser.ParseFromString("ADD DEFAULT TO GRAPH <http://example.org/destination>");

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(store);
            processor.ProcessCommandSet(commands);

            g = store.Graph(null);
            h = store.Graph(new Uri("http://example.org/destination"));
            Assert.IsFalse(g.IsEmpty, "Source Graph should not be empty");
            Assert.IsFalse(h.IsEmpty, "Destination Graph should not be empty");
            Assert.IsTrue(h.HasSubGraph(g), "Destination Graph should have Source Graph as a subgraph");
        }

        [TestMethod]
        public void SparqlUpdateCopyCommand()
        {
            IGraph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/source");

            IGraph h = new Graph();
            FileLoader.Load(h, "Turtle.ttl");
            h.BaseUri = new Uri("http://example.org/destination");

            TripleStore store = new TripleStore();
            store.Add(g);
            store.Add(h);

            Assert.AreNotEqual(g, h, "Graphs should not be equal");

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet commands = parser.ParseFromString("COPY GRAPH <http://example.org/source> TO GRAPH <http://example.org/destination>");

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(store);
            processor.ProcessCommandSet(commands);

            g = store.Graph(new Uri("http://example.org/source"));
            h = store.Graph(new Uri("http://example.org/destination"));
            Assert.IsFalse(g.IsEmpty, "Source Graph should not be empty");
            Assert.IsFalse(h.IsEmpty, "Destination Graph should not be empty");
            Assert.AreEqual(g, h, "Graphs should now be equal");
        }

        [TestMethod]
        public void SparqlUpdateCopyCommand2()
        {
            IGraph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/source");

            IGraph h = new Graph();
            FileLoader.Load(h, "Turtle.ttl");
            h.BaseUri = null;

            TripleStore store = new TripleStore();
            store.Add(g);
            store.Add(h);

            Assert.AreNotEqual(g, h, "Graphs should not be equal");

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet commands = parser.ParseFromString("COPY GRAPH <http://example.org/source> TO DEFAULT");

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(store);
            processor.ProcessCommandSet(commands);

            g = store.Graph(new Uri("http://example.org/source"));
            h = store.Graph(null);
            Assert.IsFalse(g.IsEmpty, "Source Graph should not be empty");
            Assert.IsFalse(h.IsEmpty, "Destination Graph should not be empty");
            Assert.AreEqual(g, h, "Graphs should now be equal");
        }

        [TestMethod]
        public void SparqlUpdateCopyCommand3()
        {
            IGraph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            IGraph h = new Graph();
            FileLoader.Load(h, "Turtle.ttl");
            h.BaseUri = new Uri("http://example.org/destination");

            TripleStore store = new TripleStore();
            store.Add(g);
            store.Add(h);

            Assert.AreNotEqual(g, h, "Graphs should not be equal");

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet commands = parser.ParseFromString("COPY DEFAULT TO GRAPH <http://example.org/destination>");

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(store);
            processor.ProcessCommandSet(commands);

            g = store.Graph(null);
            h = store.Graph(new Uri("http://example.org/destination"));
            Assert.IsFalse(g.IsEmpty, "Source Graph should not be empty");
            Assert.IsFalse(h.IsEmpty, "Destination Graph should not be empty");
            Assert.AreEqual(g, h, "Graphs should now be equal");
        }

        [TestMethod]
        public void SparqlUpdateMoveCommand()
        {
            IGraph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/source");

            IGraph h = new Graph();
            FileLoader.Load(h, "Turtle.ttl");
            h.BaseUri = new Uri("http://example.org/destination");

            TripleStore store = new TripleStore();
            store.Add(g);
            store.Add(h);

            Assert.AreNotEqual(g, h, "Graphs should not be equal");

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet commands = parser.ParseFromString("MOVE GRAPH <http://example.org/source> TO GRAPH <http://example.org/destination>");

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(store);
            processor.ProcessCommandSet(commands);

            g = store.HasGraph(new Uri("http://example.org/source")) ? store.Graph(new Uri("http://example.org/source")) : null;
            h = store.Graph(new Uri("http://example.org/destination"));
            Assert.IsFalse(h.IsEmpty, "Destination Graph should not be empty");
            Assert.IsTrue(g == null || g.IsEmpty, "Source Graph should be Deleted/Empty");

            Graph orig = new Graph();
            FileLoader.Load(orig, "InferenceTest.ttl");
            Assert.AreEqual(orig, h, "Destination Graph should be equal to the original contents of the Source Graph");
        }

        [TestMethod]
        public void SparqlUpdateMoveCommand2()
        {
            IGraph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/source");

            IGraph h = new Graph();
            FileLoader.Load(h, "Turtle.ttl");
            h.BaseUri = null;

            TripleStore store = new TripleStore();
            store.Add(g);
            store.Add(h);

            Assert.AreNotEqual(g, h, "Graphs should not be equal");

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet commands = parser.ParseFromString("MOVE GRAPH <http://example.org/source> TO DEFAULT");

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(store);
            processor.ProcessCommandSet(commands);

            g = store.HasGraph(new Uri("http://example.org/source")) ? store.Graph(new Uri("http://example.org/source")) : null;
            h = store.Graph(null);
            Assert.IsFalse(h.IsEmpty, "Destination Graph should not be empty");
            Assert.IsTrue(g == null || g.IsEmpty, "Source Graph should be Deleted/Empty");

            Graph orig = new Graph();
            FileLoader.Load(orig, "InferenceTest.ttl");
            Assert.AreEqual(orig, h, "Destination Graph should be equal to the original contents of the Source Graph");
        }

        [TestMethod]
        public void SparqlUpdateMoveCommand3()
        {
            IGraph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            IGraph h = new Graph();
            FileLoader.Load(h, "Turtle.ttl");
            h.BaseUri = new Uri("http://example.org/destination");

            TripleStore store = new TripleStore();
            store.Add(g);
            store.Add(h);

            Assert.AreNotEqual(g, h, "Graphs should not be equal");

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet commands = parser.ParseFromString("MOVE DEFAULT TO GRAPH <http://example.org/destination>");

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(store);
            processor.ProcessCommandSet(commands);

            g = store.HasGraph(null) ? store.Graph(null) : null;
            h = store.Graph(new Uri("http://example.org/destination"));
            Assert.IsFalse(h.IsEmpty, "Destination Graph should not be empty");
            Assert.IsFalse(g == null, "Default Graph should still exist");
            Assert.IsTrue(g.IsEmpty, "Source Graph (the Default Graph) should be Empty");

            Graph orig = new Graph();
            FileLoader.Load(orig, "InferenceTest.ttl");
            Assert.AreEqual(orig, h, "Destination Graph should be equal to the original contents of the Source Graph");
        }
    }
}
