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
using FluentAssertions;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;
using VDS.RDF.Update;
using VDS.RDF.Update.Commands;

namespace VDS.RDF.Update
{

    public class UpdateTests2
    {
        [Fact]
        public void SparqlUpdateCreateDrop()
        {
            TripleStore store = new TripleStore();

            Console.WriteLine("Store has " + store.Graphs.Count + " Graphs");

            //Create a couple of Graphs using Create Commands
            CreateCommand create1 = new CreateCommand(new Uri("http://example.org/1"));
            CreateCommand create2 = new CreateCommand(new Uri("http://example.org/2"));

            store.ExecuteUpdate(create1);
            store.ExecuteUpdate(create2);

            Assert.Equal(3, store.Graphs.Count);
            Assert.Empty(store.Triples);

            //Trying the same Create again should cause an error
            try
            {
                store.ExecuteUpdate(create1);
                Assert.True(false, "Executing a CREATE command twice without the SILENT modifier should error");
            }
            catch (SparqlUpdateException)
            {
                Console.WriteLine("Executing a CREATE command twice without the SILENT modifier errored as expected");
            }

            //Equivalent Create with SILENT should not error
            CreateCommand create3 = new CreateCommand(new Uri("http://example.org/1"), true);
            try
            {
                store.ExecuteUpdate(create3);
                Console.WriteLine("Executing a CREATE for an existing Graph with the SILENT modifier suppressed the error as expected");
            }
            catch (SparqlUpdateException)
            {
                Assert.True(false, "Executing a CREATE for an existing Graph with the SILENT modifier should not error");
            }

            DropCommand drop1 = new DropCommand(new Uri("http://example.org/1"));
            store.ExecuteUpdate(drop1);
            Assert.Equal(2, store.Graphs.Count);

            try
            {
                store.ExecuteUpdate(drop1);
                Assert.True(false, "Trying to DROP a non-existent Graph should error");
            }
            catch (SparqlUpdateException)
            {
                Console.WriteLine("Trying to DROP a non-existent Graph produced an error as expected");
            }

            DropCommand drop2 = new DropCommand(new Uri("http://example.org/1"), ClearMode.Graph, true);
            try
            {
                store.ExecuteUpdate(drop2);
                Console.WriteLine("Trying to DROP a non-existent Graph with the SILENT modifier suppressed the error as expected");
            }
            catch (SparqlUpdateException)
            {
                Assert.True(false, "Trying to DROP a non-existent Graph with the SILENT modifier should suppress the error");
            }
        }

        [SkippableFact]
        public void SparqlUpdateLoad()
        {
            Skip.IfNot(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing), "Test Config marks Remote Parsing as unavailable, test cannot be run");

            TripleStore store = new TripleStore();

            LoadCommand loadLondon = new LoadCommand(new Uri("http://dbpedia.org/resource/London"));
            LoadCommand loadSouthampton = new LoadCommand(new Uri("http://dbpedia.org/resource/Southampton"), new Uri("http://example.org"));

            store.ExecuteUpdate(loadLondon);
            store.ExecuteUpdate(loadSouthampton);

            Assert.Equal(2, store.Graphs.Count);
            Assert.NotEmpty(store.Triples);

            foreach (IGraph g in store.Graphs)
            {
                foreach (Triple t in g.Triples)
                {
                    Console.Write(t.ToString());
                    if (g.BaseUri != null)
                    {
                        Console.WriteLine(" from " + g.BaseUri.ToString());
                    }
                    else
                    {
                        Console.WriteLine();
                    }
                }
            }
        }

        [Fact]
        public void SparqlUpdateModify()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");
            g.BaseUri = null;
            store.Add(g);

            IUriNode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));

            Assert.NotEmpty(store.GetTriplesWithPredicate(rdfType));

            String update = "DELETE {?s a ?type} WHERE {?s a ?type}";
            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString(update);
            store.ExecuteUpdate(cmds);

            Assert.Empty(store.GetTriplesWithPredicate(rdfType));
        }

        [Fact]
        public void SparqlUpdateWithDefaultQueryProcessor()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = null;
            TripleStore store = new TripleStore();
            store.Add(g);
            InMemoryDataset dataset = new InMemoryDataset(store);

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString("DELETE { ?s a ?type } WHERE { ?s a ?type }");

            ISparqlUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(cmds);
        }

        [Fact]
        public void SparqlUpdateWithCustomQueryProcessor()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = null;
            TripleStore store = new TripleStore();
            store.Add(g);
            InMemoryDataset dataset = new InMemoryDataset(store);

            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString("DELETE { ?s a ?type } WHERE { ?s a ?type }");

            ISparqlUpdateProcessor processor = new ExplainUpdateProcessor(dataset, ExplanationLevel.Full);
            processor.ProcessCommandSet(cmds);
        }

        [Fact]
        public void SparqlUpdateChangesNotReflectedInOriginalGraph1()
        {
            //Test Case originally submitted by Tomasz Pluskiewicz

            // given
            IGraph sourceGraph = new Graph();
            sourceGraph.LoadFromString(@"@prefix ex: <http://www.example.com/> .
ex:Subject ex:hasObject ex:Object .");

            IGraph expectedGraph = new Graph();
            expectedGraph.LoadFromString(@"@prefix ex: <http://www.example.com/> .
ex:Subject ex:hasBlank [ ex:hasObject ex:Object ] .");


            //IMPORTANT - Because we create the dataset without an existing store it
            //creates a new triple store internally which has a single unnamed graph
            //Then when we add another unnamed graph it merges into that existing graph,
            //this is why the update does not apply to the added graph but rather to
            //the merged graph that is internal to the dataset
            ISparqlDataset dataset = new InMemoryDataset(true);
            dataset.AddGraph(sourceGraph);

            // when
            var command = new SparqlParameterizedString
            {
                CommandText = @"PREFIX ex: <http://www.example.com/>
DELETE { ex:Subject ex:hasObject ex:Object . }
INSERT { ex:Subject ex:hasBlank [ ex:hasObject ex:Object ] . }
WHERE { ?s ?p ?o . }"
            };
            SparqlUpdateCommandSet cmds = new SparqlUpdateParser().ParseFromString(command);
            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(cmds);

            Assert.Single(dataset.Graphs);

            IGraph g = dataset.Graphs.First();
            Assert.Equal(2, g.Triples.Count);
            Assert.False(ReferenceEquals(g, sourceGraph), "Result Graph should not be the Source Graph");
            Assert.Equal(1, sourceGraph.Triples.Count);
            Assert.NotEqual(expectedGraph, sourceGraph);
        }

        [Fact]
        public void SparqlUpdateChangesNotReflectedInOriginalGraph2()
        {
            //Test Case originally submitted by Tomasz Pluskiewicz

            // given
            IGraph sourceGraph = new Graph();
            sourceGraph.LoadFromString(@"@prefix ex: <http://www.example.com/> .
ex:Subject ex:hasObject ex:Object .");

            IGraph expectedGraph = new Graph();
            expectedGraph.LoadFromString(@"@prefix ex: <http://www.example.com/> .
ex:Subject ex:hasBlank [ ex:hasObject ex:Object ] .");

            TripleStore store = new TripleStore();
            store.Add(sourceGraph);
            ISparqlDataset dataset = new InMemoryDataset(store, sourceGraph.BaseUri);

            // when
            var command = new SparqlParameterizedString
            {
                CommandText = @"PREFIX ex: <http://www.example.com/>
DELETE { ex:Subject ex:hasObject ex:Object . }
INSERT { ex:Subject ex:hasBlank [ ex:hasObject ex:Object ] . }
WHERE { ?s ?p ?o . }"
            };
            SparqlUpdateCommandSet cmds = new SparqlUpdateParser().ParseFromString(command);
            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(cmds);

            Assert.Single(dataset.Graphs);

            IGraph g = dataset.Graphs.First();
            Assert.Equal(2, g.Triples.Count);
            Assert.True(ReferenceEquals(g, sourceGraph), "Result Graph should be the Source Graph");
            Assert.Equal(2, sourceGraph.Triples.Count);
            Assert.Equal(expectedGraph, sourceGraph);
        }

        [Fact]
        public void SparqlUpdateInsertDeleteWithBlankNodes()
        {
            //This test adapted from a contribution by Tomasz Pluskiewicz
            //It demonstrates an issue in SPARQL Update caused by incorrect Graph
            //references that can result when using GraphPersistenceWrapper in a SPARQL Update
            String initData = @"@prefix ex: <http://www.example.com/>.
@prefix rr: <http://www.w3.org/ns/r2rml#>.

ex:triplesMap rr:predicateObjectMap _:blank .
_:blank rr:object ex:Employee, ex:Worker .";

            String update = @"PREFIX rr: <http://www.w3.org/ns/r2rml#>

DELETE { ?map rr:object ?value . }
INSERT { ?map rr:objectMap [ rr:constant ?value ] . }
WHERE { ?map rr:object ?value }";

            String query = @"prefix ex: <http://www.example.com/>
prefix rr: <http://www.w3.org/ns/r2rml#>

select *
where
{
ex:triplesMap rr:predicateObjectMap ?predObjMap .
?predObjMap rr:objectMap ?objMap .
}";

            String expectedData = @"@prefix ex: <http://www.example.com/>.
@prefix rr: <http://www.w3.org/ns/r2rml#>.

ex:triplesMap rr:predicateObjectMap _:blank.
_:blank rr:objectMap _:autos1.
_:autos1 rr:constant ex:Employee.
_:autos2 rr:constant ex:Worker.
_:blank rr:objectMap _:autos2.";

            // given
            IGraph graph = new Graph();
            graph.LoadFromString(initData);
            IGraph expectedGraph = new Graph();
            expectedGraph.LoadFromString(expectedData);

            Console.WriteLine("Initial Graph:");
            TestTools.ShowGraph(graph);
            Console.WriteLine();

            // when
            TripleStore store = new TripleStore();
            store.Add(graph);

            var dataset = new InMemoryDataset(store, graph.BaseUri);
            ISparqlUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            var updateParser = new SparqlUpdateParser();
            processor.ProcessCommandSet(updateParser.ParseFromString(update));

            Console.WriteLine("Resulting Graph:");
            TestTools.ShowGraph(graph);
            Console.WriteLine();

            Triple x = graph.GetTriplesWithPredicate(graph.CreateUriNode("rr:predicateObjectMap")).FirstOrDefault();
            INode origBNode = x.Object;
            Assert.True(graph.GetTriples(origBNode).Count() > 1, "Should be more than one Triple using the BNode");
            IEnumerable<Triple> ys = graph.GetTriplesWithSubject(origBNode);
            foreach (Triple y in ys)
            {
                Assert.Equal(origBNode, y.Subject);
            }

            //Graphs should be equal
            GraphDiffReport diff = graph.Difference(expectedGraph);
            if (!diff.AreEqual) TestTools.ShowDifferences(diff);
            Assert.Equal(expectedGraph, graph);

            //Test the Query
            SparqlResultSet results = graph.ExecuteQuery(query) as SparqlResultSet;
            TestTools.ShowResults(results);
            Assert.False(results.IsEmpty, "Should be some results");
        }

        [Fact]
        public void SparqlUpdateInsertBNodesComplex1()
        {
            String update = @"PREFIX : <http://test/>
INSERT { :s :p _:b } WHERE { };
INSERT { ?o ?p ?s } WHERE { ?s ?p ?o }";

            TripleStore store = new TripleStore();
            store.ExecuteUpdate(update);

            Assert.Equal(1, store.Graphs.Count);
            Assert.Equal(2, store.Triples.Count());

            IGraph def = store[null];
            Triple a = def.Triples.Where(t => t.Subject.NodeType == NodeType.Blank).FirstOrDefault();
            Assert.NotNull(a);
            Triple b = def.Triples.Where(t => t.Object.NodeType == NodeType.Blank).FirstOrDefault();
            Assert.NotNull(b);

            Assert.Equal(a.Subject, b.Object);
        }

        [Fact]
        public void SparqlUpdateInsertBNodesComplex2()
        {
            String update = @"PREFIX : <http://test/>
INSERT { GRAPH :a { :s :p _:b } } WHERE { };
INSERT { GRAPH :b { :s :p _:b } } WHERE { };
INSERT { GRAPH :a { ?s ?p ?o } } WHERE { GRAPH :b { ?s ?p ?o } }";

            TripleStore store = new TripleStore();
            store.ExecuteUpdate(update);

            Assert.Equal(3, store.Graphs.Count);
            Assert.Equal(2, store[new Uri("http://test/a")].Triples.Count);

        }

        [Fact]
        public void SparqlUpdateInsertBNodesComplex3()
        {
            String update = @"PREFIX : <http://test/>
INSERT DATA { GRAPH :a { :s :p _:b } GRAPH :b { :s :p _:b } };
INSERT { GRAPH :a { ?s ?p ?o } } WHERE { GRAPH :b { ?s ?p ?o } }";

            TripleStore store = new TripleStore();
            store.ExecuteUpdate(update);

            Assert.Equal(3, store.Graphs.Count);
            Assert.Equal(1, store[new Uri("http://test/a")].Triples.Count);

        }

        [Fact]
        public void SparqlUpdateInsertWithGraphClause1()
        {
            Graph g = new Graph();
            g.Assert(g.CreateUriNode(UriFactory.Create("http://subject")), g.CreateUriNode(UriFactory.Create("http://predicate")), g.CreateUriNode(UriFactory.Create("http://object")));

            InMemoryDataset dataset = new InMemoryDataset(g);

            String updates = "INSERT { GRAPH ?s { ?s ?p ?o } } WHERE { ?s ?p ?o }";
            SparqlUpdateCommandSet commands = new SparqlUpdateParser().ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(commands);

            Assert.Equal(2, dataset.GraphUris.Count());
            Assert.True(dataset.HasGraph(UriFactory.Create("http://subject")));
        }

        [Fact]
        public void SparqlUpdateDeleteWithGraphClause1()
        {
            Graph g = new Graph();
            g.Assert(g.CreateUriNode(UriFactory.Create("http://subject")), g.CreateUriNode(UriFactory.Create("http://predicate")), g.CreateUriNode(UriFactory.Create("http://object")));
            Graph h = new Graph();
            h.Merge(g);
            h.BaseUri = UriFactory.Create("http://subject");

            InMemoryDataset dataset = new InMemoryDataset(g);
            dataset.AddGraph(h);
            dataset.Flush();

            Assert.Equal(2, dataset.GraphUris.Count());

            String updates = "DELETE { GRAPH ?s { ?s ?p ?o } } WHERE { ?s ?p ?o }";
            SparqlUpdateCommandSet commands = new SparqlUpdateParser().ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(commands);

            Assert.Equal(2, dataset.GraphUris.Count());
            Assert.True(dataset.HasGraph(UriFactory.Create("http://subject")));
            Assert.Equal(0, dataset[UriFactory.Create("http://subject")].Triples.Count);
        }

        [Fact]
        public void SparqlUpdateDeleteWithGraphClause2()
        {
            Graph g = new Graph();
            g.Assert(g.CreateUriNode(UriFactory.Create("http://subject")), g.CreateUriNode(UriFactory.Create("http://predicate")), g.CreateUriNode(UriFactory.Create("http://object")));
            Graph h = new Graph();
            h.Merge(g);
            h.BaseUri = UriFactory.Create("http://subject");

            InMemoryDataset dataset = new InMemoryDataset(g);
            dataset.AddGraph(h);
            dataset.Flush();

            Assert.Equal(2, dataset.GraphUris.Count());

            String updates = "DELETE { GRAPH ?s { ?s ?p ?o } } INSERT { GRAPH ?o { ?s ?p ?o } } WHERE { ?s ?p ?o }";
            SparqlUpdateCommandSet commands = new SparqlUpdateParser().ParseFromString(updates);

            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            processor.ProcessCommandSet(commands);

            Assert.Equal(3, dataset.GraphUris.Count());
            Assert.True(dataset.HasGraph(UriFactory.Create("http://subject")));
            Assert.True(dataset.HasGraph(UriFactory.Create("http://object")));
            Assert.Equal(0, dataset[UriFactory.Create("http://subject")].Triples.Count);
        }

        [Fact]
        public void SparqlUpdateModifyCommandWithNoMatchingGraph()
        {
            var dataset = new InMemoryDataset();
            var egGraph = new Uri("http://example.org/graph");
            dataset.HasGraph(egGraph).Should().BeFalse();
            var processor = new LeviathanUpdateProcessor(dataset);
            var cmdSet = new SparqlUpdateParser().ParseFromString(
                "WITH <" + egGraph +"> DELETE {} INSERT {} WHERE {}");
            processor.ProcessCommandSet(cmdSet);
            dataset.HasGraph(egGraph).Should().BeFalse(); // No triples got added

            cmdSet = new SparqlUpdateParser().ParseFromString(
                "WITH <" + egGraph + "> DELETE {} INSERT { <http://example.org/s> <http://example.org/p> <http://example.org/o> } WHERE {}");
            processor.ProcessCommandSet(cmdSet);
            dataset.HasGraph(egGraph).Should().BeTrue(); // Triples got added
        }

        [Fact]
        public void SparqlUpdateInsertCommandWithNoMatchingGraph()
        {
            var dataset = new InMemoryDataset();
            var egGraph = new Uri("http://example.org/graph");
            dataset.HasGraph(egGraph).Should().BeFalse();
            var processor = new LeviathanUpdateProcessor(dataset);
            var cmdSet = new SparqlUpdateParser().ParseFromString(
                "WITH <" + egGraph + "> INSERT {} WHERE {}");
            processor.ProcessCommandSet(cmdSet);
            dataset.HasGraph(egGraph).Should().BeFalse(); // No triples got added

            cmdSet = new SparqlUpdateParser().ParseFromString(
                "WITH <" + egGraph + "> INSERT { <http://example.org/s> <http://example.org/p> <http://example.org/o> } WHERE {}");
            processor.ProcessCommandSet(cmdSet);
            dataset.HasGraph(egGraph).Should().BeTrue(); // Triples got added
        }

        [Fact]
        public void SparqlUpdateDeleteCommandWithNoMatchingGraph()
        {
            var dataset = new InMemoryDataset();
            var egGraph = new Uri("http://example.org/graph");
            dataset.HasGraph(egGraph).Should().BeFalse();
            var processor = new LeviathanUpdateProcessor(dataset);
            var cmdSet = new SparqlUpdateParser().ParseFromString(
                "WITH <" + egGraph + "> DELETE {} WHERE {}");
            processor.ProcessCommandSet(cmdSet);
            dataset.HasGraph(egGraph).Should().BeFalse();
        }

        [Fact]
        public void SparqlUpdateDeleteCommandWithNoMatchingGraphAndChildGraphPatterns()
        {
            var dataset = new InMemoryDataset();
            var updateGraphUri = new Uri("http://example.org/g2");
            var updateGraph = new Graph{BaseUri = updateGraphUri };
            updateGraph.Assert(new Triple(updateGraph.CreateUriNode(new Uri("http://example.org/s")),
                updateGraph.CreateUriNode(new Uri("http://example.org/p")),
                updateGraph.CreateUriNode(new Uri("http://example.org/o"))));
            dataset.AddGraph(updateGraph);

            var egGraph = new Uri("http://example.org/graph");
            dataset.HasGraph(egGraph).Should().BeFalse();
            var processor = new LeviathanUpdateProcessor(dataset);
            var cmdSet = new SparqlUpdateParser().ParseFromString(
                "WITH <" + egGraph +
                "> DELETE { GRAPH <http://example.org/g2> { <http://example.org/s> <http://example.org/p> <http://example.org/o> }} WHERE {}");
            processor.ProcessCommandSet(cmdSet);

            dataset.HasGraph(egGraph).Should().BeFalse(because:"Update command should not create an empty graph for a DELETE operation");
            dataset[updateGraphUri].IsEmpty.Should()
                .BeTrue("Triples should be deleted by child graph pattern");

        }

    }
}
