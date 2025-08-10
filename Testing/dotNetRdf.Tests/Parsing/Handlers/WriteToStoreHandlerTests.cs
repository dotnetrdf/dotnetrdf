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
using System.IO;
using System.Linq;
using Xunit;
using VDS.RDF.Storage;

namespace VDS.RDF.Parsing.Handlers;

/// <summary>
/// Summary description for WriteToStoreHandlerTests
/// </summary>
public class WriteToStoreHandlerTests
{
    private readonly Uri TestGraphUri = new Uri("http://example.org/WriteToStoreHandlerTest");
    private readonly Uri TestBNodeUri = new Uri("http://example.org/WriteToStoreHandlerTest/BNodes");

    private void EnsureTestData()
    {
        if (!System.IO.File.Exists("write_to_store_handler_tests_temp.ttl"))
        {
            var g = new Graph();
            EmbeddedResourceLoader.Load(g, "VDS.RDF.Configuration.configuration.ttl");
            g.SaveToFile("write_to_store_handler_tests_temp.ttl");
        }
    }

    private void TestWriteToStoreHandler(IStorageProvider manager)
    {
        //First ensure that our test file exists
        EnsureTestData();

        //Try to ensure that the target Graph does not exist
        if (manager.DeleteSupported)
        {
            manager.DeleteGraph(TestGraphUri);
        }
        else
        {
            var g = new Graph
            {
                BaseUri = TestGraphUri
            };
            manager.SaveGraph(g);
        }

        var temp = new Graph();
        try
        {
            manager.LoadGraph(temp, TestGraphUri);
            Assert.True(temp.IsEmpty, "Unable to ensure that Target Graph in Store is empty prior to running Test");
        }
        catch
        {
            //An Error Loading the Graph is OK
        }

        var handler = new WriteToStoreHandler(manager, TestGraphUri, 100);
        var parser = new TurtleParser();
        parser.Load(handler, "write_to_store_handler_tests_temp.ttl");

        manager.LoadGraph(temp, TestGraphUri);
        Assert.False(temp.IsEmpty, "Graph should not be empty");

        var orig = new Graph();
        orig.LoadFromFile("write_to_store_handler_tests_temp.ttl");

        Assert.Equal(orig, temp);
    }

    private void TestWriteToStoreDatasetsHandler(IStorageProvider manager)
    {
        var factory = new NodeFactory();
        INode a = factory.CreateUriNode(new Uri("http://example.org/a"));
        INode b = factory.CreateUriNode(new Uri("http://example.org/b"));
        INode c = factory.CreateUriNode(new Uri("http://example.org/c"));
        INode d = factory.CreateUriNode(new Uri("http://example.org/d"));

        var graphB = new Uri("http://example.org/graphs/b");
        var graphD = new Uri("http://example.org/graphs/d");

        //Try to ensure that the target Graphs do not exist
        if (manager.DeleteSupported)
        {
            manager.DeleteGraph(TestGraphUri);
            manager.DeleteGraph(graphB);
            manager.DeleteGraph(graphD);
        }
        else
        {
            var g = new Graph
            {
                BaseUri = TestGraphUri
            };
            manager.SaveGraph(g);
            g.BaseUri = graphB;
            manager.SaveGraph(g);
            g.BaseUri = graphD;
            manager.SaveGraph(g);
        }

        //Do the parsing and thus the loading
        var handler = new WriteToStoreHandler(manager, TestGraphUri);
        var parser = new NQuadsParser();
        parser.Load(handler, File.OpenText(Path.Combine("resources", "writetostore.nq")));

        //Load the expected Graphs
        var def = new Graph();
        manager.LoadGraph(def, TestGraphUri);
        var gB = new Graph();
        manager.LoadGraph(gB, graphB);
        var gD = new Graph();
        manager.LoadGraph(gD, graphD);

        Assert.Equal(2, def.Triples.Count);
        Assert.True(def.ContainsTriple(new Triple(a, a, a)), "Default Graph should have the a triple");
        Assert.Equal(1, gB.Triples.Count);
        Assert.True(gB.ContainsTriple(new Triple(b, b, b)), "b Graph should have the b triple");
        Assert.True(def.ContainsTriple(new Triple(c, c, c)), "Default Graph should have the c triple");
        Assert.Equal(1, gD.Triples.Count);
        Assert.True(gD.ContainsTriple(new Triple(d, d, d)), "d Graph should have the d triple");
    }

    private void TestWriteToStoreHandlerWithBNodes(IStorageProvider manager)
    {
        var fragment = "@prefix : <http://example.org/>. :subj :has [ a :BNode ; :with \"value\" ] .";

        //Try to ensure that the target Graph does not exist
        if (manager.DeleteSupported)
        {
            manager.DeleteGraph(TestBNodeUri);
        } 
        else 
        {
            var temp = new Graph
            {
                BaseUri = TestBNodeUri
            };
            manager.SaveGraph(temp);
        }

        //Then write to the store
        var parser = new TurtleParser();
        var handler = new WriteToStoreHandler(manager, TestBNodeUri, 1);
        parser.Load(handler, new StringReader(fragment));

        //Then load back the data and check it
        var g = new Graph();
        manager.LoadGraph(g, TestBNodeUri);

        Assert.Equal(3, g.Triples.Count);
        var nodes = g.Nodes.BlankNodes().ToList();
        for (var i = 0; i < nodes.Count; i++)
        {
            for (var j = 0; j < nodes.Count; j++)
            {
                if (i == j) continue;
                Assert.Equal(nodes[i], nodes[j]);
            }
        }
    }

    [Fact]
    public void ParsingWriteToStoreHandlerBadInstantiation()
    {
        Assert.Throws<ArgumentNullException>(() => new WriteToStoreHandler(null, null));
    }

    [Fact]
    public void ParsingWriteToStoreHandlerBadInstantiation2()
    {
        Assert.Throws<ArgumentException>(() => new WriteToStoreHandler(new ReadOnlyConnector(new InMemoryManager()), null));
    }

    [Fact]
    public void ParsingWriteToStoreHandlerBadInstantiation4()
    {
        Assert.Throws<ArgumentException>(() => new WriteToStoreHandler(new InMemoryManager(), null, 0));
    }

    [Fact]
    public void ParsingWriteToStoreHandlerInMemory()
    {
        var mem = new InMemoryManager();
        TestWriteToStoreHandler(mem);
    }

    [Fact]
    public void ParsingWriteToStoreHandlerDatasetsInMemory()
    {
        var manager = new InMemoryManager();
        TestWriteToStoreDatasetsHandler(manager);
    }

    [Fact]
    public void ParsingWriteToStoreHandlerBNodesAcrossBatchesInMemory()
    {
        var manager = new InMemoryManager();
        TestWriteToStoreHandlerWithBNodes(manager);
    }

}
