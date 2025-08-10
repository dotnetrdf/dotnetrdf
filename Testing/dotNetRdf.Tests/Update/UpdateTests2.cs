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
using System.Linq;
using FluentAssertions;
using System.IO;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Update.Commands;

namespace VDS.RDF.Update;

[Collection("RdfServer")]
public class UpdateTests2
{
    private readonly RdfServerFixture _serverFixture;

    public UpdateTests2(RdfServerFixture serverFixture)
    {
        _serverFixture = serverFixture;
    }

    [Fact]
    public void SparqlUpdateCreateDrop()
    {
        var store = new TripleStore();

        Console.WriteLine("Store has " + store.Graphs.Count + " Graphs");

        //Create a couple of Graphs using Create Commands
        var create1 = new CreateCommand(new Uri("http://example.org/1"));
        var create2 = new CreateCommand(new Uri("http://example.org/2"));
        var nodeFactory = new NodeFactory();
        IUriNode g1 = nodeFactory.CreateUriNode(new Uri("http://example.org/1"));
        IUriNode g2 = nodeFactory.CreateUriNode(new Uri("http://example.org/2"));

        store.ExecuteUpdate(create1);
        store.ExecuteUpdate(create2);

        Assert.Equal(3, store.Graphs.Count);
        Assert.Empty(store.Triples);

        //Trying the same Create again should cause an error
        try
        {
            store.ExecuteUpdate(create1);
            Assert.Fail("Executing a CREATE command twice without the SILENT modifier should error");
        }
        catch (SparqlUpdateException)
        {
            Console.WriteLine("Executing a CREATE command twice without the SILENT modifier errored as expected");
        }

        //Equivalent Create with SILENT should not error
        var create3 = new CreateCommand(new Uri("http://example.org/1"), true);
        try
        {
            store.ExecuteUpdate(create3);
            Console.WriteLine("Executing a CREATE for an existing Graph with the SILENT modifier suppressed the error as expected");
        }
        catch (SparqlUpdateException)
        {
            Assert.Fail("Executing a CREATE for an existing Graph with the SILENT modifier should not error");
        }

        var drop1 = new DropCommand(g1);
        store.ExecuteUpdate(drop1);
        Assert.Equal(2, store.Graphs.Count);

        try
        {
            store.ExecuteUpdate(drop1);
            Assert.Fail("Trying to DROP a non-existent Graph should error");
        }
        catch (SparqlUpdateException)
        {
            Console.WriteLine("Trying to DROP a non-existent Graph produced an error as expected");
        }

        var drop2 = new DropCommand(g1, ClearMode.Graph, true);
        try
        {
            store.ExecuteUpdate(drop2);
            Console.WriteLine("Trying to DROP a non-existent Graph with the SILENT modifier suppressed the error as expected");
        }
        catch (SparqlUpdateException)
        {
            Assert.Fail("Trying to DROP a non-existent Graph with the SILENT modifier should suppress the error");
        }
    }

    [Fact]
    public void SparqlUpdateLoad()
    {
        var store = new TripleStore();
        var nodeFactory = new NodeFactory();
        var loadLondon = new LoadCommand(_serverFixture.UriFor("/doap"), (IRefNode)null);
        var loadSouthampton = new LoadCommand(_serverFixture.UriFor("/resource/Southampton"), nodeFactory.CreateUriNode(new Uri("http://example.org")));

        store.ExecuteUpdate(loadLondon);
        store.ExecuteUpdate(loadSouthampton);

        Assert.Equal(2, store.Graphs.Count);
        Assert.NotEmpty(store.Triples);
    }

    [Fact]
    public void SparqlUpdateModify()
    {
        var store = new TripleStore();
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        g.BaseUri = null;
        store.Add(g);

        var rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));

        Assert.NotEmpty(store.GetTriplesWithPredicate(rdfType));

        var update = "DELETE {?s a ?type} WHERE {?s a ?type}";
        var parser = new SparqlUpdateParser();
        var cmds = parser.ParseFromString(update);
        store.ExecuteUpdate(cmds);

        Assert.Empty(store.GetTriplesWithPredicate(rdfType));
    }

    [Fact]
    public void SparqlUpdateWithDefaultQueryProcessor()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        g.BaseUri = null;
        var store = new TripleStore();
        store.Add(g);
        var dataset = new InMemoryDataset(store);

        var parser = new SparqlUpdateParser();
        var cmds = parser.ParseFromString("DELETE { ?s a ?type } WHERE { ?s a ?type }");

        ISparqlUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(cmds);
    }

    [Fact]
    public void SparqlUpdateWithCustomQueryProcessor()
    {
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        g.BaseUri = null;
        var store = new TripleStore();
        store.Add(g);
        var dataset = new InMemoryDataset(store);

        var parser = new SparqlUpdateParser();
        var cmds = parser.ParseFromString("DELETE { ?s a ?type } WHERE { ?s a ?type }");

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
        var cmds = new SparqlUpdateParser().ParseFromString(command);
        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(cmds);

        Assert.Single(dataset.Graphs);

        var g = dataset.Graphs.First();
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

        var store = new TripleStore();
        store.Add(sourceGraph);
        ISparqlDataset dataset = new InMemoryDataset(store, sourceGraph.Name);

        // when
        var command = new SparqlParameterizedString
        {
            CommandText = @"PREFIX ex: <http://www.example.com/>
DELETE { ex:Subject ex:hasObject ex:Object . }
INSERT { ex:Subject ex:hasBlank [ ex:hasObject ex:Object ] . }
WHERE { ?s ?p ?o . }"
        };
        var cmds = new SparqlUpdateParser().ParseFromString(command);
        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(cmds);

        Assert.Single(dataset.Graphs);

        var g = dataset.Graphs.First();
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
        var initData = @"@prefix ex: <http://www.example.com/>.
@prefix rr: <http://www.w3.org/ns/r2rml#>.

ex:triplesMap rr:predicateObjectMap _:blank .
_:blank rr:object ex:Employee, ex:Worker .";

        var update = @"PREFIX rr: <http://www.w3.org/ns/r2rml#>

DELETE { ?map rr:object ?value . }
INSERT { ?map rr:objectMap [ rr:constant ?value ] . }
WHERE { ?map rr:object ?value }";

        var query = @"prefix ex: <http://www.example.com/>
prefix rr: <http://www.w3.org/ns/r2rml#>

select *
where
{
ex:triplesMap rr:predicateObjectMap ?predObjMap .
?predObjMap rr:objectMap ?objMap .
}";

        var expectedData = @"@prefix ex: <http://www.example.com/>.
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
        var store = new TripleStore();
        store.Add(graph);

        var dataset = new InMemoryDataset(store, graph.Name);
        ISparqlUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
        var updateParser = new SparqlUpdateParser();
        processor.ProcessCommandSet(updateParser.ParseFromString(update));

        Console.WriteLine("Resulting Graph:");
        TestTools.ShowGraph(graph);
        Console.WriteLine();

        var x = graph.GetTriplesWithPredicate(graph.CreateUriNode("rr:predicateObjectMap")).FirstOrDefault();
        var origBNode = x.Object;
        Assert.True(graph.GetTriples(origBNode).Count() > 1, "Should be more than one Triple using the BNode");
        var ys = graph.GetTriplesWithSubject(origBNode);
        foreach (var y in ys)
        {
            Assert.Equal(origBNode, y.Subject);
        }

        //Graphs should be equal
        var diff = graph.Difference(expectedGraph);
        if (!diff.AreEqual) TestTools.ShowDifferences(diff);
        Assert.Equal(expectedGraph, graph);

        //Test the Query
        var results = graph.ExecuteQuery(query) as SparqlResultSet;
        TestTools.ShowResults(results);
        Assert.False(results.IsEmpty, "Should be some results");
    }

    [Fact]
    public void SparqlUpdateInsertBNodesComplex1()
    {
        var update = @"PREFIX : <http://test/>
INSERT { :s :p _:b } WHERE { };
INSERT { ?o ?p ?s } WHERE { ?s ?p ?o }";

        var store = new TripleStore();
        store.ExecuteUpdate(update);

        Assert.Equal(1, store.Graphs.Count);
        Assert.Equal(2, store.Triples.Count());

        var def = store[(IRefNode)null];
        var a = def.Triples.Where(t => t.Subject.NodeType == NodeType.Blank).FirstOrDefault();
        Assert.NotNull(a);
        var b = def.Triples.Where(t => t.Object.NodeType == NodeType.Blank).FirstOrDefault();
        Assert.NotNull(b);

        Assert.Equal(a.Subject, b.Object);
    }

    [Fact]
    public void SparqlUpdateInsertBNodesComplex2()
    {
        var update = @"PREFIX : <http://test/>
INSERT { GRAPH :a { :s :p _:b } } WHERE { };
INSERT { GRAPH :b { :s :p _:b } } WHERE { };
INSERT { GRAPH :a { ?s ?p ?o } } WHERE { GRAPH :b { ?s ?p ?o } }";

        var store = new TripleStore();
        var nodeFactory = new NodeFactory();
        store.ExecuteUpdate(update);

        Assert.Equal(3, store.Graphs.Count);
        Assert.Equal(2, store[nodeFactory.CreateUriNode(new Uri("http://test/a"))].Triples.Count);

    }

    [Fact]
    public void SparqlUpdateInsertBNodesComplex3()
    {
        var update = @"PREFIX : <http://test/>
INSERT DATA { GRAPH :a { :s :p _:b } GRAPH :b { :s :p _:b } };
INSERT { GRAPH :a { ?s ?p ?o } } WHERE { GRAPH :b { ?s ?p ?o } }";

        var store = new TripleStore();
        var nodeFactory = new NodeFactory();
        store.ExecuteUpdate(update);

        Assert.Equal(3, store.Graphs.Count);
        Assert.Equal(1, store[nodeFactory.CreateUriNode(new Uri("http://test/a"))].Triples.Count);

    }

    [Fact]
    public void SparqlUpdateInsertWithGraphClause1()
    {
        var g = new Graph();
        g.Assert(g.CreateUriNode(UriFactory.Root.Create("http://subject")), g.CreateUriNode(UriFactory.Root.Create("http://predicate")), g.CreateUriNode(UriFactory.Root.Create("http://object")));

        var dataset = new InMemoryDataset(g);

        var updates = "INSERT { GRAPH ?s { ?s ?p ?o } } WHERE { ?s ?p ?o }";
        var commands = new SparqlUpdateParser().ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(commands);

        Assert.Equal(2, dataset.GraphNames.Count());
        Assert.True(dataset.HasGraph(g.CreateUriNode(UriFactory.Root.Create("http://subject"))));
    }

    [Fact]
    public void SparqlUpdateDeleteWithGraphClause1()
    {
        var g = new Graph();
        g.Assert(g.CreateUriNode(UriFactory.Root.Create("http://subject")), g.CreateUriNode(UriFactory.Root.Create("http://predicate")), g.CreateUriNode(UriFactory.Root.Create("http://object")));
        var h = new Graph(new UriNode(UriFactory.Root.Create("http://subject")));
        h.Merge(g);

        var dataset = new InMemoryDataset(g);
        dataset.AddGraph(h);
        dataset.Flush();

        Assert.Equal(2, dataset.GraphNames.Count());

        var updates = "DELETE { GRAPH ?s { ?s ?p ?o } } WHERE { ?s ?p ?o }";
        SparqlUpdateCommandSet commands = new SparqlUpdateParser().ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(commands);

        Assert.Equal(2, dataset.GraphNames.Count());
        Assert.True(dataset.HasGraph(new UriNode(UriFactory.Root.Create("http://subject"))));
        Assert.Equal(0, dataset[new UriNode(UriFactory.Root.Create("http://subject"))].Triples.Count);
    }

    [Fact]
    public void SparqlUpdateDeleteWithGraphClause2()
    {
        var g = new Graph();
        g.Assert(g.CreateUriNode(UriFactory.Root.Create("http://subject")), g.CreateUriNode(UriFactory.Root.Create("http://predicate")), g.CreateUriNode(UriFactory.Root.Create("http://object")));
        var h = new Graph(new UriNode(UriFactory.Root.Create("http://subject")));
        h.Merge(g);

        var dataset = new InMemoryDataset(g);
        dataset.AddGraph(h);
        dataset.Flush();

        Assert.Equal(2, dataset.GraphNames.Count());

        var updates = "DELETE { GRAPH ?s { ?s ?p ?o } } INSERT { GRAPH ?o { ?s ?p ?o } } WHERE { ?s ?p ?o }";
        var commands = new SparqlUpdateParser().ParseFromString(updates);

        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(commands);

        Assert.Equal(3, dataset.GraphNames.Count());
        Assert.True(dataset.HasGraph(new UriNode(UriFactory.Root.Create("http://subject"))));
        Assert.True(dataset.HasGraph(new UriNode(UriFactory.Root.Create("http://object"))));
        Assert.Equal(0, dataset[new UriNode(UriFactory.Root.Create("http://subject"))].Triples.Count);
    }

    [Fact]
    public void SparqlUpdateModifyCommandWithNoMatchingGraph()
    {
        var dataset = new InMemoryDataset();
        var nodeFactory = new NodeFactory();
        IUriNode egGraph = nodeFactory.CreateUriNode(new Uri("http://example.org/graph"));
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
        var nodeFactory = new NodeFactory();
        IUriNode egGraph = nodeFactory.CreateUriNode(new Uri("http://example.org/graph"));
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
        var nodeFactory = new NodeFactory();
        IUriNode egGraph = nodeFactory.CreateUriNode(new Uri("http://example.org/graph"));
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
        var updateGraphUri = new UriNode(new Uri("http://example.org/g2"));
        var updateGraph = new Graph(updateGraphUri);
        updateGraph.Assert(new Triple(updateGraph.CreateUriNode(new Uri("http://example.org/s")),
            updateGraph.CreateUriNode(new Uri("http://example.org/p")),
            updateGraph.CreateUriNode(new Uri("http://example.org/o"))));
        dataset.AddGraph(updateGraph);

        var egGraph = new UriNode(new Uri("http://example.org/graph"));
        dataset.HasGraph(egGraph).Should().BeFalse();
        var processor = new LeviathanUpdateProcessor(dataset);
        SparqlUpdateCommandSet cmdSet = new SparqlUpdateParser().ParseFromString(
            "WITH <" + egGraph +
            "> DELETE { GRAPH <http://example.org/g2> { <http://example.org/s> <http://example.org/p> <http://example.org/o> }} WHERE {}");
        processor.ProcessCommandSet(cmdSet);

        dataset.HasGraph(egGraph).Should().BeFalse(because:"Update command should not create an empty graph for a DELETE operation");
        dataset[updateGraphUri].IsEmpty.Should()
            .BeTrue("Triples should be deleted by child graph pattern");

    }

}
