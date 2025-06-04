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
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Update.Commands;
using FluentAssertions;
using System.IO;

namespace VDS.RDF.Update;


public class UpdateTests1
{
    private readonly ITestOutputHelper _testOutputHelper;

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

    public readonly IUriNode SourceGraph = new UriNode(new Uri("http://example.org/source"));
    public readonly IUriNode DestinationGraph = new UriNode(new Uri("http://example.org/destination"));

    public UpdateTests1(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void SparqlUpdateInsertDataWithBNodes()
    {
        var store = new TripleStore();
        var g = new Graph();
        store.Add(g);

        var prefixes = "PREFIX rdf: <" + NamespaceMapper.RDF + ">\n PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA + ">\n PREFIX ex: <http://example.org/>\n PREFIX rdl: <http://example.org/roles>\n PREFIX tpl: <http://example.org/template/>\n";
        var insert = prefixes + "INSERT DATA { " + InsertPatterns1 + "}";
        _testOutputHelper.WriteLine(insert);
        _testOutputHelper.WriteLine();
        store.ExecuteUpdate(insert);
        insert = prefixes + "INSERT DATA {" + InsertPatterns2 + "}";
        _testOutputHelper.WriteLine(insert);
        _testOutputHelper.WriteLine();
        store.ExecuteUpdate(insert);

        foreach (Triple t in g.Triples)
        {
            _testOutputHelper.WriteLine(t.ToString());
        }
    }

    [Fact]
    public void SparqlUpdateInsertDataWithSkolemBNodes()
    {
        var store = new TripleStore();
        var g = new Graph();
        store.Add(g);

        var prefixes = "PREFIX rdf: <" + NamespaceMapper.RDF + ">\n PREFIX xsd: <" + NamespaceMapper.XMLSCHEMA + ">\n PREFIX ex: <http://example.org/>\n PREFIX rdl: <http://example.org/roles>\n PREFIX tpl: <http://example.org/template/>\n";
        var insert = prefixes + "INSERT DATA { " + InsertPatterns1 + "}";
        _testOutputHelper.WriteLine(insert.Replace("_:template","<_:template>"));
        _testOutputHelper.WriteLine();
        store.ExecuteUpdate(insert.Replace("_:template", "<_:template>"));
        insert = prefixes + "INSERT DATA {" + InsertPatterns2 + "}";
        _testOutputHelper.WriteLine(insert);
        _testOutputHelper.WriteLine();
        store.ExecuteUpdate(insert);

        foreach (Triple t in g.Triples)
        {
            _testOutputHelper.WriteLine(t.ToString());
        }
    }

    [Fact]
    public void SparqlUpdateAddCommand()
    {
        IGraph g = new Graph(SourceGraph);
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        IGraph h = new Graph(DestinationGraph);
        FileLoader.Load(h, Path.Combine("resources", "Turtle.ttl"));

        var store = new TripleStore();
        store.Add(g);
        store.Add(h);

        Assert.NotEqual(g, h);

        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet commands = parser.ParseFromString("ADD GRAPH <http://example.org/source> TO GRAPH <http://example.org/destination>");

        var processor = new LeviathanUpdateProcessor(store);
        processor.ProcessCommandSet(commands);

        g = store[SourceGraph];
        h = store[DestinationGraph];
        Assert.False(g.IsEmpty, "Source Graph should not be empty");
        Assert.False(h.IsEmpty, "Destination Graph should not be empty");
        Assert.True(h.HasSubGraph(g), "Destination Graph should have Source Graph as a subgraph");
    }

    [Fact]
    public void SparqlUpdateAddCommand2()
    {
        IGraph g = new Graph(SourceGraph);
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        IGraph h = new Graph();
        FileLoader.Load(h, Path.Combine("resources", "Turtle.ttl"));
        h.BaseUri = null;

        var store = new TripleStore();
        store.Add(g);
        store.Add(h);

        Assert.NotEqual(g, h);

        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet commands = parser.ParseFromString("ADD GRAPH <http://example.org/source> TO DEFAULT");

        var processor = new LeviathanUpdateProcessor(store);
        processor.ProcessCommandSet(commands);

        g = store[SourceGraph];
        h = store[(IRefNode)null];
        Assert.False(g.IsEmpty, "Source Graph should not be empty");
        Assert.False(h.IsEmpty, "Destination Graph should not be empty");
        Assert.True(h.HasSubGraph(g), "Destination Graph should have Source Graph as a subgraph");
    }

    [Fact]
    public void SparqlUpdateAddCommand3()
    {
        IGraph g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        IGraph h = new Graph(DestinationGraph);
        FileLoader.Load(h, Path.Combine("resources", "Turtle.ttl"));

        var store = new TripleStore();
        store.Add(g);
        store.Add(h);

        Assert.NotEqual(g, h);

        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet commands = parser.ParseFromString("ADD DEFAULT TO GRAPH <http://example.org/destination>");

        var processor = new LeviathanUpdateProcessor(store);
        processor.ProcessCommandSet(commands);

        g = store[(IRefNode)null];
        h = store[DestinationGraph];
        Assert.False(g.IsEmpty, "Source Graph should not be empty");
        Assert.False(h.IsEmpty, "Destination Graph should not be empty");
        Assert.True(h.HasSubGraph(g), "Destination Graph should have Source Graph as a subgraph");
    }

    [Fact]
    public void SparqlUpdateCopyCommand()
    {
        IGraph g = new Graph(SourceGraph);
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        IGraph h = new Graph(DestinationGraph);
        FileLoader.Load(h, Path.Combine("resources", "Turtle.ttl"));

        var store = new TripleStore();
        store.Add(g);
        store.Add(h);

        Assert.NotEqual(g, h);

        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet commands = parser.ParseFromString("COPY GRAPH <http://example.org/source> TO GRAPH <http://example.org/destination>");

        var processor = new LeviathanUpdateProcessor(store);
        processor.ProcessCommandSet(commands);

        g = store[SourceGraph];
        h = store[DestinationGraph];
        Assert.False(g.IsEmpty, "Source Graph should not be empty");
        Assert.False(h.IsEmpty, "Destination Graph should not be empty");
        Assert.Equal(g, h);
    }

    [Fact]
    public void SparqlUpdateCopyCommand2()
    {
        IGraph g = new Graph(SourceGraph);
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        IGraph h = new Graph();
        FileLoader.Load(h, Path.Combine("resources", "Turtle.ttl"));
        h.BaseUri = null;

        var store = new TripleStore();
        store.Add(g);
        store.Add(h);

        Assert.NotEqual(g, h);

        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet commands = parser.ParseFromString("COPY GRAPH <http://example.org/source> TO DEFAULT");

        var processor = new LeviathanUpdateProcessor(store);
        processor.ProcessCommandSet(commands);

        g = store[SourceGraph];
        h = store[(IRefNode)null];
        Assert.False(g.IsEmpty, "Source Graph should not be empty");
        Assert.False(h.IsEmpty, "Destination Graph should not be empty");
        Assert.Equal(g, h);
    }

    [Fact]
    public void SparqlUpdateCopyCommand3()
    {
        IGraph g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        IGraph h = new Graph(DestinationGraph);
        FileLoader.Load(h, Path.Combine("resources", "Turtle.ttl"));

        var store = new TripleStore();
        store.Add(g);
        store.Add(h);

        Assert.NotEqual(g, h);

        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet commands = parser.ParseFromString("COPY DEFAULT TO GRAPH <http://example.org/destination>");

        var processor = new LeviathanUpdateProcessor(store);
        processor.ProcessCommandSet(commands);

        g = store[(IRefNode)null];
        h = store[DestinationGraph];
        Assert.False(g.IsEmpty, "Source Graph should not be empty");
        Assert.False(h.IsEmpty, "Destination Graph should not be empty");
        Assert.Equal(g, h);
    }

    [Fact]
    public void SparqlUpdateMoveCommand()
    {
        IGraph g = new Graph(SourceGraph);
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        IGraph h = new Graph(DestinationGraph);
        FileLoader.Load(h, Path.Combine("resources", "Turtle.ttl"));

        var store = new TripleStore();
        store.Add(g);
        store.Add(h);

        Assert.NotEqual(g, h);

        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet commands = parser.ParseFromString("MOVE GRAPH <http://example.org/source> TO GRAPH <http://example.org/destination>");

        var processor = new LeviathanUpdateProcessor(store);
        processor.ProcessCommandSet(commands);

        g = store.HasGraph(SourceGraph) ? store[SourceGraph] : null;
        h = store[DestinationGraph];
        Assert.False(h.IsEmpty, "Destination Graph should not be empty");
        Assert.True(g == null || g.IsEmpty, "Source Graph should be Deleted/Empty");

        var orig = new Graph();
        FileLoader.Load(orig, Path.Combine("resources", "InferenceTest.ttl"));
        Assert.Equal(orig, h);
    }

    [Fact]
    public void SparqlUpdateMoveCommand2()
    {
        IGraph g = new Graph(SourceGraph);
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        IGraph h = new Graph();
        FileLoader.Load(h, Path.Combine("resources", "Turtle.ttl"));

        var store = new TripleStore();
        store.Add(g);
        store.Add(h);

        Assert.NotEqual(g, h);

        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet commands = parser.ParseFromString("MOVE GRAPH <http://example.org/source> TO DEFAULT");

        var processor = new LeviathanUpdateProcessor(store);
        processor.ProcessCommandSet(commands);

        g = store.HasGraph(SourceGraph) ? store[SourceGraph] : null;
        h = store[(IRefNode)null];
        Assert.False(h.IsEmpty, "Destination Graph should not be empty");
        Assert.True(g == null || g.IsEmpty, "Source Graph should be Deleted/Empty");

        var orig = new Graph();
        FileLoader.Load(orig, Path.Combine("resources", "InferenceTest.ttl"));
        Assert.Equal(orig, h);
    }

    [Fact]
    public void SparqlUpdateMoveCommand3()
    {
        IGraph g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        g.BaseUri = null;

        IGraph h = new Graph(DestinationGraph);
        FileLoader.Load(h, Path.Combine("resources", "Turtle.ttl"));

        var store = new TripleStore();
        store.Add(g);
        store.Add(h);

        Assert.NotEqual(g, h);

        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet commands = parser.ParseFromString("MOVE DEFAULT TO GRAPH <http://example.org/destination>");

        var processor = new LeviathanUpdateProcessor(store);
        processor.ProcessCommandSet(commands);

        g = store.HasGraph((IRefNode)null) ? store[(IRefNode)null] : null;
        h = store[DestinationGraph];
        Assert.False(h.IsEmpty, "Destination Graph should not be empty");
        Assert.False(g == null, "Default Graph should still exist");
        Assert.True(g.IsEmpty, "Source Graph (the Default Graph) should be Empty");

        var orig = new Graph();
        FileLoader.Load(orig, Path.Combine("resources", "InferenceTest.ttl"));
        Assert.Equal(orig, h);
    }

    [Theory]
    [InlineData("insert-default-graph")]
    [InlineData("insert-using")]
    [InlineData("insert-using-named")]
    public void SparqlUpdateInsert(string test)
    {
        var testPath = Path.Combine("resources", "sparql-update", test);

        var p = new SparqlUpdateParser();
        var cmds = p.ParseFromFile(Path.Combine(testPath, "update.rq"));
        var store = new TripleStore();
        var inputFiles = Directory.GetFiles(testPath, "input*.*");
        foreach (var inputFile in inputFiles) {
            store.LoadFromFile(inputFile);
        }
        var updateProcessor = new LeviathanUpdateProcessor(store);
        updateProcessor.ProcessCommandSet(cmds);

        var expectStore = new TripleStore();
        var expectFiles = Directory.GetFiles(testPath, "output*.*");
        foreach(var expectFile in expectFiles)
        {
            expectStore.LoadFromFile(expectFile);
        }

        foreach(var h in expectStore.Graphs)
        {
            Assert.True(store.HasGraph(h.Name), "Did not find expected graph " + h.Name + " in updated store.");
            var g = store.Graphs[h.Name];
            GraphDiffReport diff = h.Difference(g);
            if (!diff.AreEqual)
            {
                TestTools.ShowDifferences(diff, _testOutputHelper);
            }

            Assert.Equal(h, g);
        }
    }


    [Fact]
    public void SparqlUpdateInsertCommand3()
    {
        var command = new SparqlParameterizedString();
        command.Namespaces.AddNamespace("rdf", new Uri(NamespaceMapper.RDF));
        command.Namespaces.AddNamespace("rdfs", new Uri(NamespaceMapper.RDFS));
        command.CommandText = "INSERT { ?s rdf:type ?class } USING NAMED <http://example.org/temp> WHERE { ?s a ?type . ?type rdfs:subClassOf+ ?class };";
        command.CommandText += "INSERT { ?s ?property ?value } USING NAMED <http://example.org/temp> WHERE {?s ?p ?value . ?p rdfs:subPropertyOf+ ?property };";
        command.CommandText += "INSERT { ?s rdf:type rdfs:Class } USING NAMED <http://example.org/temp> WHERE { ?s rdfs:subClassOf ?class };";
        command.CommandText += "INSERT { ?s rdf:type rdf:Property } USING NAMED <http://example.org/temp> WHERE { ?s rdfs:subPropertyOf ?property };";

        var store = new TripleStore();
        var g = new Graph(new UriNode(new Uri("http://example.org/temp")));
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        store.Add(g);

        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet cmds = parser.ParseFromString(command);
        var dataset = new InMemoryDataset(store);
        var processor = new LeviathanUpdateProcessor(dataset);
        dataset.SetDefaultGraph((IRefNode)null);
        processor.ProcessCommandSet(cmds);

        IGraph def = store[(IRefNode)null];
        TestTools.ShowGraph(def);
        Assert.True(def.IsEmpty, "Graph should be empty as the commands only used USING NAMED (so shouldn't have changed the dataset) and the Active Graph for the dataset was empty so there should have been nothing matched to generate insertions from");
    }

    [Fact]
    public void SparqlUpdateDeleteWithCommand()
    {
        var command = "WITH <http://example.org/> DELETE { ?s ?p ?o } WHERE {?s ?p ?o}";
        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet cmds = parser.ParseFromString(command);

        var delete = (DeleteCommand)cmds[0];

        Assert.Equal(new UriNode(new Uri("http://example.org/")), delete.TargetGraph);
    }

    [Fact]
    public void SparqlUpdateInsertDataCombination()
    {
        var command = new SparqlParameterizedString();
        command.Namespaces.AddNamespace("ex", new Uri("http://example.org/"));
        command.CommandText = "INSERT DATA { ex:a ex:b ex:c GRAPH <http://example.org/graph> { ex:a ex:b ex:c } }";

        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet cmds = parser.ParseFromString(command);

        Assert.False(cmds.Commands.All(cmd => cmd.AffectsSingleGraph), "Commands should report that they do not affect a single Graph");
        Assert.True(cmds.Commands.All(cmd => cmd.AffectsGraph((IRefNode)null)), "Commands should report that they affect the Default Graph");
        Assert.True(cmds.Commands.All(cmd => cmd.AffectsGraph(new UriNode(new Uri("http://example.org/graph")))), "Commands should report that they affect the named Graph");

        var dataset = new InMemoryDataset();
        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(cmds);

        var g = new Graph();
        g.NamespaceMap.Import(command.Namespaces);
        var t = new Triple(g.CreateUriNode("ex:a"), g.CreateUriNode("ex:b"), g.CreateUriNode("ex:c"));

        IGraph def = dataset[(IRefNode)null];
        Assert.Equal(1, def.Triples.Count);
        Assert.True(def.ContainsTriple(t), "Should have the inserted Triple in the Default Graph");

        IGraph ex = dataset[new UriNode(new Uri("http://example.org/graph"))];
        Assert.Equal(1, ex.Triples.Count);
        Assert.True(ex.ContainsTriple(t), "Should have the inserted Triple in the Named Graph");

    }

    [Fact]
    public void SparqlUpdateDeleteDataCombination()
    {
        var command = new SparqlParameterizedString();
        command.Namespaces.AddNamespace("ex", new Uri("http://example.org/"));
        command.CommandText = "DELETE DATA { ex:a ex:b ex:c GRAPH <http://example.org/graph> { ex:a ex:b ex:c } }";

        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet cmds = parser.ParseFromString(command);

        Assert.False(cmds.Commands.All(cmd => cmd.AffectsSingleGraph), "Commands should report that they do not affect a single Graph");
        Assert.True(cmds.Commands.All(cmd => cmd.AffectsGraph((IRefNode)null)), "Commands should report that they affect the Default Graph");
        Assert.True(cmds.Commands.All(cmd => cmd.AffectsGraph(new UriNode(new Uri("http://example.org/graph")))), "Commands should report that they affect the named Graph");

        var dataset = new InMemoryDataset();
        IGraph def = new Graph();
        def.NamespaceMap.Import(command.Namespaces);
        def.Assert(new Triple(def.CreateUriNode("ex:a"), def.CreateUriNode("ex:b"), def.CreateUriNode("ex:c")));
        dataset.AddGraph(def);
        IGraph ex = new Graph(new UriNode(new Uri("http://example.org/graph")));
        ex.NamespaceMap.Import(command.Namespaces);
        ex.Assert(new Triple(ex.CreateUriNode("ex:a"), ex.CreateUriNode("ex:b"), ex.CreateUriNode("ex:c")));
        dataset.AddGraph(ex);

        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(cmds);

        var g = new Graph();
        g.NamespaceMap.Import(command.Namespaces);
        var t = new Triple(g.CreateUriNode("ex:a"), g.CreateUriNode("ex:b"), g.CreateUriNode("ex:c"));

        def = dataset[(IRefNode)null];
        Assert.Equal(0, def.Triples.Count);
        Assert.False(def.ContainsTriple(t), "Should not have the deleted Triple in the Default Graph");

        ex = dataset[new UriNode(new Uri("http://example.org/graph"))];
        Assert.Equal(0, ex.Triples.Count);
        Assert.False(ex.ContainsTriple(t), "Should not have the deleted Triple in the Named Graph");

    }

    [Fact]
    public void SparqlUpdateInsertWithBind()
    {
        var store = new TripleStore();
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "rvesse.ttl"));
        store.Add(g);

        var origTriples = g.Triples.Count;
        var command = new SparqlParameterizedString();
        command.Namespaces.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));
        command.CommandText = "INSERT { ?x foaf:mbox_sha1sum ?hash } WHERE { ?x foaf:mbox ?email . BIND(SHA1(STR(?email)) AS ?hash) }";

        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet cmds = parser.ParseFromString(command);

        var dataset = new InMemoryDataset(store, g.Name);
        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(cmds);

        Assert.NotEqual(origTriples, g.Triples.Count);
        Assert.True(g.GetTriplesWithPredicate(g.CreateUriNode(new Uri("http://xmlns.com/foaf/0.1/mbox_sha1sum"))).Any(), "Expected new triples to have been added");
    }

    [Fact]
    public void SparqlUpdateInsertWithFilter()
    {
        var store = new TripleStore();
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "rvesse.ttl"));
        store.Add(g);

        var origTriples = g.Triples.Count;

        var command = new SparqlParameterizedString();
        command.Namespaces.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));
        command.CommandText = @"INSERT { ?x foaf:mbox_sha1sum ?email } WHERE { ?x foaf:mbox ?email . FILTER(REGEX(STR(?email), 'dotnetrdf.org')) }";

        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet cmds = parser.ParseFromString(command);

        var dataset = new InMemoryDataset(store, g.Name);
        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(cmds);

        Assert.NotEqual(origTriples, g.Triples.Count);
        Assert.True(g.GetTriplesWithPredicate(g.CreateUriNode(new Uri("http://xmlns.com/foaf/0.1/mbox_sha1sum"))).Any(), "Expected new triples to have been added");
    }

    [Fact]
    public void SparqlUpdateDeleteWithBind()
    {
        var store = new TripleStore();
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "rvesse.ttl"));
        store.Add(g);

        var origTriples = g.Triples.Count;

        var command = new SparqlParameterizedString();
        command.Namespaces.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));
        command.CommandText = "DELETE { ?x foaf:mbox ?y } WHERE { ?x foaf:mbox ?email . BIND(?email AS ?y) }";

        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet cmds = parser.ParseFromString(command);

        var dataset = new InMemoryDataset(store, g.Name);
        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(cmds);

        Assert.NotEqual(origTriples, g.Triples.Count);
        Assert.False(g.GetTriplesWithPredicate(g.CreateUriNode(new Uri("http://xmlns.com/foaf/0.1/mbox"))).Any(), "Expected triples to have been deleted");
    }

    [Fact]
    public void SparqlUpdateDeleteWithFilter()
    {
        var store = new TripleStore();
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "rvesse.ttl"));
        store.Add(g);

        var origTriples = g.Triples.Count;

        var command = new SparqlParameterizedString();
        command.Namespaces.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));
        command.CommandText = @"DELETE { ?x foaf:mbox ?email } WHERE { ?x foaf:mbox ?email . FILTER(REGEX(STR(?email), 'dotnetrdf\\.org')) }";

        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet cmds = parser.ParseFromString(command);

        var dataset = new InMemoryDataset(store, g.Name);
        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(cmds);

        Assert.NotEqual(origTriples, g.Triples.Count);
        Assert.False(g.GetTriplesWithPredicate(g.CreateUriNode(new Uri("http://xmlns.com/foaf/0.1/mbox"))).Any(t => t.Object.ToString().Contains("dotnetrdf.org")), "Expected triples to have been deleted");
    }

    [Fact]
    public void ExecutionTimeIsNullBeforeExecutionAndNotNullAfter()
    {
        var store = new TripleStore();
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "rvesse.ttl"));
        store.Add(g);

        var origTriples = g.Triples.Count;

        var command = new SparqlParameterizedString();
        command.Namespaces.AddNamespace("foaf", new Uri("http://xmlns.com/foaf/0.1/"));
        command.CommandText = @"DELETE { ?x foaf:mbox ?email } WHERE { ?x foaf:mbox ?email }";

        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet cmds = parser.ParseFromString(command);
        cmds.UpdateExecutionTime.Should().BeNull();

        var dataset = new InMemoryDataset(store, g.Name);
        var processor = new LeviathanUpdateProcessor(dataset);
        processor.ProcessCommandSet(cmds);

        cmds.UpdateExecutionTime.Should().NotBeNull();
    }
}
