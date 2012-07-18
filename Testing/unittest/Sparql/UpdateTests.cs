using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Inference;
using VDS.RDF.Update;
using VDS.RDF.Update.Commands;

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
        public void SparqlUpdateInsertDataWithSkolemBNodes()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            store.Add(g);

            String prefixes = "PREFIX rdf: <" + NamespaceMapper.RDF + ">\n PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA + ">\n PREFIX ex: <http://example.org/>\n PREFIX rdl: <http://example.org/roles>\n PREFIX tpl: <http://example.org/template/>\n";
            String insert = prefixes + "INSERT DATA { " + InsertPatterns1 + "}";
            Console.WriteLine(insert.Replace("_:template","<_:template>"));
            Console.WriteLine();
            store.ExecuteUpdate(insert.Replace("_:template", "<_:template>"));
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

        [TestMethod]
        public void SparqlUpdateInsertCommand()
        {
            SparqlParameterizedString command = new SparqlParameterizedString();
            command.Namespaces.AddNamespace("rdf", new Uri(NamespaceMapper.RDF));
            command.Namespaces.AddNamespace("rdfs", new Uri(NamespaceMapper.RDFS));
            command.CommandText = "INSERT { ?s rdf:type ?class } WHERE { ?s a ?type . ?type rdfs:subClassOf+ ?class };";
            command.CommandText += "INSERT { ?s ?property ?value } WHERE {?s ?p ?value . ?p rdfs:subPropertyOf+ ?property };";
            command.CommandText += "INSERT { ?s rdf:type rdfs:Class } WHERE { ?s rdfs:subClassOf ?class };";
            command.CommandText += "INSERT { ?s rdf:type rdf:Property } WHERE { ?s rdfs:subPropertyOf ?property };";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.Retract(g.Triples.Where(t => !t.IsGroundTriple).ToList());
            g.BaseUri = null;
            store.Add(g);

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString(command);
            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(store);
            processor.ProcessCommandSet(cmds);

            TestTools.ShowGraph(g);
            Console.WriteLine();

            //Now reload the test data and apply an RDFS reasoner over it
            //This should give us a Graph equivalent to the one created by the previous INSERT commands
            Graph h = new Graph();
            FileLoader.Load(h, "InferenceTest.ttl");
            h.Retract(h.Triples.Where(t => !t.IsGroundTriple));
            RdfsReasoner reasoner = new RdfsReasoner();
            reasoner.Apply(h);

            GraphDiffReport diff = h.Difference(g);
            if (!diff.AreEqual)
            {
                TestTools.ShowDifferences(diff);
            }

            Assert.AreEqual(h, g, "Graphs should be equal");            
        }

        [TestMethod]
        public void SparqlUpdateInsertCommand2()
        {
            SparqlParameterizedString command = new SparqlParameterizedString();
            command.Namespaces.AddNamespace("rdf", new Uri(NamespaceMapper.RDF));
            command.Namespaces.AddNamespace("rdfs", new Uri(NamespaceMapper.RDFS));
            command.CommandText = "INSERT { ?s rdf:type ?class } USING <http://example.org/temp> WHERE { ?s a ?type . ?type rdfs:subClassOf+ ?class };";
            command.CommandText += "INSERT { ?s ?property ?value } USING <http://example.org/temp> WHERE {?s ?p ?value . ?p rdfs:subPropertyOf+ ?property };";
            command.CommandText += "INSERT { ?s rdf:type rdfs:Class } USING <http://example.org/temp> WHERE { ?s rdfs:subClassOf ?class };";
            command.CommandText += "INSERT { ?s rdf:type rdf:Property } USING <http://example.org/temp> WHERE { ?s rdfs:subPropertyOf ?property };";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/temp");
            store.Add(g);
            int origTriples = g.Triples.Count;

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString(command);
            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(store);
            processor.ProcessCommandSet(cmds);

            Assert.AreEqual(origTriples, g.Triples.Count, "Triples in input Graph shouldn't have changed as INSERTs should have gone to the default graph");

            IGraph def = store.Graph(null);

            TestTools.ShowGraph(def);
            Console.WriteLine();

            //Apply a RDFS reasoner over the original input and output it into another graph
            //Should be equivalent to the default Graph
            Graph h = new Graph();
            RdfsReasoner reasoner = new RdfsReasoner();
            reasoner.Apply(g, h);

            TestTools.ShowGraph(h);

            GraphDiffReport report = h.Difference(def);
            if (!report.AreEqual)
            {
                TestTools.ShowDifferences(report);

                Assert.IsTrue(report.RemovedTriples.Count() == 1, "Should have only 1 missing Triple (due to rdfs:domain inference which is hard to encode in an INSERT command)");
            }
        }

        [TestMethod]
        public void SparqlUpdateInsertCommand3()
        {
            SparqlParameterizedString command = new SparqlParameterizedString();
            command.Namespaces.AddNamespace("rdf", new Uri(NamespaceMapper.RDF));
            command.Namespaces.AddNamespace("rdfs", new Uri(NamespaceMapper.RDFS));
            command.CommandText = "INSERT { ?s rdf:type ?class } USING NAMED <http://example.org/temp> WHERE { ?s a ?type . ?type rdfs:subClassOf+ ?class };";
            command.CommandText += "INSERT { ?s ?property ?value } USING NAMED <http://example.org/temp> WHERE {?s ?p ?value . ?p rdfs:subPropertyOf+ ?property };";
            command.CommandText += "INSERT { ?s rdf:type rdfs:Class } USING NAMED <http://example.org/temp> WHERE { ?s rdfs:subClassOf ?class };";
            command.CommandText += "INSERT { ?s rdf:type rdf:Property } USING NAMED <http://example.org/temp> WHERE { ?s rdfs:subPropertyOf ?property };";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/temp");
            store.Add(g);

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString(command);
            InMemoryDataset dataset = new InMemoryDataset(store);
            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            dataset.SetDefaultGraph((Uri)null);
            processor.ProcessCommandSet(cmds);

            IGraph def = store.Graph(null);
            TestTools.ShowGraph(def);
            Assert.IsTrue(def.IsEmpty, "Graph should be empty as the commands only used USING NAMED (so shouldn't have changed the dataset) and the Active Graph for the dataset was empty so there should have been nothing matched to generate insertions from");
        }

        [TestMethod]
        public void SparqlUpdateInsertCommand4()
        {
            SparqlParameterizedString command = new SparqlParameterizedString();
            command.Namespaces.AddNamespace("rdf", new Uri(NamespaceMapper.RDF));
            command.Namespaces.AddNamespace("rdfs", new Uri(NamespaceMapper.RDFS));
            command.CommandText = "INSERT { ?s rdf:type ?class } USING NAMED <http://example.org/temp> WHERE { GRAPH ?g { ?s a ?type . ?type rdfs:subClassOf+ ?class } };";
            command.CommandText += "INSERT { ?s ?property ?value } USING NAMED <http://example.org/temp> WHERE { GRAPH ?g { ?s ?p ?value . ?p rdfs:subPropertyOf+ ?property } };";
            command.CommandText += "INSERT { ?s rdf:type rdfs:Class } USING NAMED <http://example.org/temp> WHERE { GRAPH ?g { ?s rdfs:subClassOf ?class } };";
            command.CommandText += "INSERT { ?s rdf:type rdf:Property } USING NAMED <http://example.org/temp> WHERE { GRAPH ?g { ?s rdfs:subPropertyOf ?property } };";

            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = new Uri("http://example.org/temp");
            store.Add(g);

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString(command);
            InMemoryDataset dataset = new InMemoryDataset(store);
            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            dataset.SetDefaultGraph((Uri)null);
            processor.ProcessCommandSet(cmds);

            IGraph def = store.Graph(null);
            TestTools.ShowGraph(def);

            //Apply a RDFS reasoner over the original input and output it into another graph
            //Should be equivalent to the default Graph
            Graph h = new Graph();
            RdfsReasoner reasoner = new RdfsReasoner();
            reasoner.Apply(g, h);

            TestTools.ShowGraph(h);

            GraphDiffReport report = h.Difference(def);
            if (!report.AreEqual)
            {
                TestTools.ShowDifferences(report);

                Assert.IsTrue(report.RemovedTriples.Count() == 1, "Should have only 1 missing Triple (due to rdfs:domain inference which is hard to encode in an INSERT command)");
            }
        }

        [TestMethod]
        public void SparqlUpdateDeleteWithCommand()
        {
            String command = "WITH <http://example.org/> DELETE { ?s ?p ?o } WHERE {?s ?p ?o}";
            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString(command);

            DeleteCommand delete = (DeleteCommand)cmds[0];

            Assert.AreEqual(new Uri("http://example.org/"), delete.GraphUri, "Graph URI of the Command should be equal to http://example.org/");
        }

        [TestMethod]
        public void SparqlUpdateInsertDataCombination()
        {
            SparqlParameterizedString command = new SparqlParameterizedString();
            command.Namespaces.AddNamespace("ex", new Uri("http://example.org/"));
            command.CommandText = "INSERT DATA { ex:a ex:b ex:c GRAPH <http://example.org/graph> { ex:a ex:b ex:c } }";

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString(command);

            Assert.IsFalse(cmds.Commands.All(cmd => cmd.AffectsSingleGraph), "Commands should report that they do not affect a single Graph");
            Assert.IsTrue(cmds.Commands.All(cmd => cmd.AffectsGraph(null)), "Commands should report that they affect the Default Graph");
            Assert.IsTrue(cmds.Commands.All(cmd => cmd.AffectsGraph(new Uri("http://example.org/graph"))), "Commands should report that they affect the named Graph");

            InMemoryDataset dataset = new InMemoryDataset();
            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(cmds);

            Graph g = new Graph();
            g.NamespaceMap.Import(command.Namespaces);
            Triple t = new Triple(g.CreateUriNode("ex:a"), g.CreateUriNode("ex:b"), g.CreateUriNode("ex:c"));

            IGraph def = dataset[null];
            Assert.AreEqual(1, def.Triples.Count, "Should be 1 Triple in the Default Graph");
            Assert.IsTrue(def.ContainsTriple(t), "Should have the inserted Triple in the Default Graph");

            IGraph ex = dataset[new Uri("http://example.org/graph")];
            Assert.AreEqual(1, ex.Triples.Count, "Should be 1 Triple in the Named Graph");
            Assert.IsTrue(ex.ContainsTriple(t), "Should have the inserted Triple in the Named Graph");

        }

        [TestMethod]
        public void SparqlUpdateDeleteDataCombination()
        {
            SparqlParameterizedString command = new SparqlParameterizedString();
            command.Namespaces.AddNamespace("ex", new Uri("http://example.org/"));
            command.CommandText = "DELETE DATA { ex:a ex:b ex:c GRAPH <http://example.org/graph> { ex:a ex:b ex:c } }";

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString(command);

            Assert.IsFalse(cmds.Commands.All(cmd => cmd.AffectsSingleGraph), "Commands should report that they do not affect a single Graph");
            Assert.IsTrue(cmds.Commands.All(cmd => cmd.AffectsGraph(null)), "Commands should report that they affect the Default Graph");
            Assert.IsTrue(cmds.Commands.All(cmd => cmd.AffectsGraph(new Uri("http://example.org/graph"))), "Commands should report that they affect the named Graph");

            InMemoryDataset dataset = new InMemoryDataset();
            IGraph def = new Graph();
            def.NamespaceMap.Import(command.Namespaces);
            def.Assert(new Triple(def.CreateUriNode("ex:a"), def.CreateUriNode("ex:b"), def.CreateUriNode("ex:c")));
            dataset.AddGraph(def);
            IGraph ex = new Graph();
            ex.BaseUri = new Uri("http://example.org/graph");
            ex.NamespaceMap.Import(command.Namespaces);
            ex.Assert(new Triple(ex.CreateUriNode("ex:a"), ex.CreateUriNode("ex:b"), ex.CreateUriNode("ex:c")));
            dataset.AddGraph(ex);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(cmds);

            Graph g = new Graph();
            g.NamespaceMap.Import(command.Namespaces);
            Triple t = new Triple(g.CreateUriNode("ex:a"), g.CreateUriNode("ex:b"), g.CreateUriNode("ex:c"));

            def = dataset[null];
            Assert.AreEqual(0, def.Triples.Count, "Should be 0 Triples in the Default Graph");
            Assert.IsFalse(def.ContainsTriple(t), "Should not have the deleted Triple in the Default Graph");

            ex = dataset[new Uri("http://example.org/graph")];
            Assert.AreEqual(0, ex.Triples.Count, "Should be 0 Triples in the Named Graph");
            Assert.IsFalse(ex.ContainsTriple(t), "Should not have the deleted Triple in the Named Graph");

        }

        [TestMethod]
        public void SparqlUpdateInsertWithBind()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromFile("rvesse.ttl");
            store.Add(g);

            int origTriples = g.Triples.Count;

            SparqlParameterizedString command = new SparqlParameterizedString();
            command.Namespaces.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));
            command.CommandText = "INSERT { ?x foaf:mbox_sha1sum ?hash } WHERE { ?x foaf:mbox ?email . BIND(SHA1(STR(?email)) AS ?hash) }";

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString(command);

            InMemoryDataset dataset = new InMemoryDataset(store, g.BaseUri);
            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(cmds);

            Assert.AreNotEqual(origTriples, g.Triples.Count, "Expected new triples to have been added");
            Assert.IsTrue(g.GetTriplesWithPredicate(g.CreateUriNode(new Uri("http://xmlns.com/foaf/0.1/mbox_sha1sum"))).Any(), "Expected new triples to have been added");
        }

        [TestMethod]
        public void SparqlUpdateInsertWithFilter()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromFile("rvesse.ttl");
            store.Add(g);

            int origTriples = g.Triples.Count;

            SparqlParameterizedString command = new SparqlParameterizedString();
            command.Namespaces.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));
            command.CommandText = "INSERT { ?x foaf:mbox_sha1sum ?email } WHERE { ?x foaf:mbox ?email . FILTER(REGEX(STR(?email), 'dotnetrdf\\.org')) }";

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString(command);

            InMemoryDataset dataset = new InMemoryDataset(store, g.BaseUri);
            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(cmds);

            Assert.AreNotEqual(origTriples, g.Triples.Count, "Expected new triples to have been added");
            Assert.IsTrue(g.GetTriplesWithPredicate(g.CreateUriNode(new Uri("http://xmlns.com/foaf/0.1/mbox_sha1sum"))).Any(), "Expected new triples to have been added");
        }

        [TestMethod]
        public void SparqlUpdateDeleteWithBind()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromFile("rvesse.ttl");
            store.Add(g);

            int origTriples = g.Triples.Count;

            SparqlParameterizedString command = new SparqlParameterizedString();
            command.Namespaces.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));
            command.CommandText = "DELETE { ?x foaf:mbox ?y } WHERE { ?x foaf:mbox ?email . BIND(?email AS ?y) }";

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString(command);

            InMemoryDataset dataset = new InMemoryDataset(store, g.BaseUri);
            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(cmds);

            Assert.AreNotEqual(origTriples, g.Triples.Count, "Expected triples to have been deleted");
            Assert.IsFalse(g.GetTriplesWithPredicate(g.CreateUriNode(new Uri("http://xmlns.com/foaf/0.1/mbox"))).Any(), "Expected triples to have been deleted");
        }

        [TestMethod]
        public void SparqlUpdateDeleteWithFilter()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.LoadFromFile("rvesse.ttl");
            store.Add(g);

            int origTriples = g.Triples.Count;

            SparqlParameterizedString command = new SparqlParameterizedString();
            command.Namespaces.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));
            command.CommandText = "DELETE { ?x foaf:mbox ?email } WHERE { ?x foaf:mbox ?email . FILTER(REGEX(STR(?email), 'dotnetrdf\\.org')) }";

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString(command);

            InMemoryDataset dataset = new InMemoryDataset(store, g.BaseUri);
            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(cmds);

            Assert.AreNotEqual(origTriples, g.Triples.Count, "Expected triples to have been deleted");
            Assert.IsFalse(g.GetTriplesWithPredicate(g.CreateUriNode(new Uri("http://xmlns.com/foaf/0.1/mbox"))).Where(t => t.Object.ToString().Contains("dotnetrdf.org")).Any(), "Expected triples to have been deleted");
        }
    }
}
