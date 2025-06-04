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
using Xunit;
using VDS.RDF.Parsing;

namespace VDS.RDF.Storage;


/// <summary>
/// Summary description for FourStoreTest
/// </summary>

public class FourStoreTest
{
    public static FourStoreConnector GetConnection()
    {
        Assert.SkipUnless(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseFourStore), "Test Config marks 4store as unavailable, test cannot be run");
        return new FourStoreConnector(TestConfigManager.GetSetting(TestConfigManager.FourStoreServer));
    }

    [Fact]
    public void StorageFourStoreSaveGraph()
    {
        var g = new Graph(new Uri("http://example.org/4storeTest"));
        FileLoader.Load(g, "resources\\InferenceTest.ttl");

        FourStoreConnector fourstore = GetConnection();
        fourstore.SaveGraph(g);

        var h = new Graph();
        fourstore.LoadGraph(h, "http://example.org/4storeTest");

        Assert.Equal(g, h);
    }

    [Fact]
    public void StorageFourStoreLoadGraph()
    {
        StorageFourStoreSaveGraph();

        var g = new Graph(new Uri("http://example.org/4storeTest"));
        FileLoader.Load(g, "resources\\InferenceTest.ttl");

        FourStoreConnector fourstore = GetConnection();

        var h = new Graph();
        fourstore.LoadGraph(h, "http://example.org/4storeTest");

        Assert.Equal(g, h);
    }

    [Fact]
    public void StorageFourStoreDeleteGraph()
    {
        StorageFourStoreSaveGraph();

        FourStoreConnector fourstore = GetConnection();
        fourstore.DeleteGraph("http://example.org/4storeTest");

        var g = new Graph();
        fourstore.LoadGraph(g, "http://example.org/4storeTest");

        Assert.True(g.IsEmpty, "Graph should be empty as it was deleted from 4store");
    }

    [Fact]
    public void StorageFourStoreAddTriples()
    {
        StorageFourStoreDeleteGraph();
        StorageFourStoreSaveGraph();

        var g = new Graph();
        var ts = new List<Triple>();
        ts.Add(new Triple(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object"))));

        FourStoreConnector fourstore = GetConnection();
        fourstore.UpdateGraph("http://example.org/4storeTest", ts, null);

        fourstore.LoadGraph(g, "http://example.org/4storeTest");

        Assert.True(ts.All(t => g.ContainsTriple(t)), "Added Triple should be in the Graph");
    }

    [Fact]
    public void StorageFourStoreRemoveTriples()
    {
        //StorageFourStoreAddTriples();
        var nodeFactory = new NodeFactory(new NodeFactoryOptions());
        var g = new Graph(nodeFactory.CreateUriNode(new Uri("http://example.org/4storeRemoveTriples")));

        FourStoreConnector fourstore = GetConnection();
        fourstore.DeleteGraph("http://example.org/4storeRemoveTriples");

        g.Assert(new[]
        {
            new Triple(g.CreateUriNode(new Uri("http://example.org/s1")),
                g.CreateUriNode(new Uri("http://example.org/p")),
                g.CreateUriNode(new Uri("http://example.org/o"))),
            new Triple(g.CreateUriNode(new Uri("http://example.org/s2")),
                g.CreateUriNode(new Uri("http://example.org/p")),
                g.CreateUriNode(new Uri("http://example.org/o")))
        });
        fourstore.SaveGraph(g);
        List<Triple> toRemove = g.GetTriplesWithSubject(new Uri("http://example.org/s1")).ToList();

        fourstore.UpdateGraph("http://example.org/4storeRemoveTriples", null, toRemove);
        g.Clear();
        fourstore.LoadGraph(g, "http://example.org/4storeRemoveTriples");
        Assert.NotEmpty(toRemove);
        Assert.True(toRemove.All(t => !g.ContainsTriple(t)), "Removed Triple should not have been in the Graph");
    }

    [Fact]
    public void StorageFourStoreUpdate()
    {
        FourStoreConnector fourstore = GetConnection();
        fourstore.Update("CREATE SILENT GRAPH <http://example.org/update>; INSERT DATA { GRAPH <http://example.org/update> { <http://example.org/subject> <http://example.org/predicate> <http://example.org/object> } }");

        var g = new Graph();
        fourstore.LoadGraph(g, "http://example.org/update");

        Assert.Equal(1, g.Triples.Count);

        fourstore.Update("DROP SILENT GRAPH <http://example.org/update>");
        var h = new Graph();
        fourstore.LoadGraph(h, "http://example.org/update");

        Assert.True(h.IsEmpty, "Graph should be empty after the DROP GRAPH update was issued");
    }
}
