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
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;
using System.IO;

namespace VDS.RDF.Storage;

[Collection("Fuseki Test Collection")]
public class FusekiTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly NTriplesFormatter _formatter = new NTriplesFormatter();

    public FusekiTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static FusekiConnector GetConnection(string uploadMimeType = null)
    {
        MimeTypeDefinition mimeTypeDescription =
            uploadMimeType == null ? null : MimeTypesHelper.GetDefinitions(uploadMimeType).First();
        return new FusekiConnector(TestConfigManager.GetSetting(TestConfigManager.FusekiServer),
            mimeTypeDescription);
    }

    [Theory]
    [InlineData("application/rdf+xml")]
    [InlineData("application/n-triples")]
    public void StorageFusekiSaveGraph(string mimeType = null)
    {
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        g.BaseUri = new Uri("http://example.org/fusekiTest");

        //Save Graph to Fuseki
        FusekiConnector fuseki = FusekiTests.GetConnection(mimeType);
        fuseki.SaveGraph(g);
        _testOutputHelper.WriteLine("Graph saved to Fuseki OK");

        //Now retrieve Graph from Fuseki
        var h = new Graph();
        fuseki.LoadGraph(h, "http://example.org/fusekiTest");

        _testOutputHelper.WriteLine("");
        foreach (Triple t in h.Triples)
        {
            _testOutputHelper.WriteLine(t.ToString(_formatter));
        }

        Assert.Equal(g, h);
    }

    [Fact]
    public void StorageFusekiSaveGraph2()
    {
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        g.BaseUri = new Uri("http://example.org/fuseki#test");

        //Save Graph to Fuseki
        FusekiConnector fuseki = FusekiTests.GetConnection();
        fuseki.SaveGraph(g);

        //Now retrieve Graph from Fuseki
        var h = new Graph();
        fuseki.LoadGraph(h, "http://example.org/fuseki#test");

        Assert.Equal(g, h);
    }

    [Fact]
    public void StorageFusekiSaveDefaultGraph()
    {
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        g.BaseUri = null;

        //Save Graph to Fuseki
        FusekiConnector fuseki = FusekiTests.GetConnection();
        fuseki.SaveGraph(g);

        //Now retrieve Graph from Fuseki
        var h = new Graph();
        fuseki.LoadGraph(h, (Uri)null);

        Assert.Equal(g, h);
        Assert.Null(h.BaseUri);
    }

    [Fact]
    public void StorageFusekiSaveDefaultGraph2()
    {
        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        g.BaseUri = null;

        //Save Graph to Fuseki
        FusekiConnector fuseki = FusekiTests.GetConnection();
        fuseki.SaveGraph(g);

        //Now retrieve Graph from Fuseki
        var h = new Graph();
        fuseki.LoadGraph(h, (String)null);

        Assert.Equal(g, h);
        Assert.Null(h.BaseUri);
    }

    [Fact]
    public void StorageFusekiLoadGraph()
    {
        //Ensure that the Graph will be there using the SaveGraph() test
        StorageFusekiSaveGraph();

        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));
        g.BaseUri = new Uri("http://example.org/fusekiTest");

        //Try to load the relevant Graph back from the Store
        FusekiConnector fuseki = FusekiTests.GetConnection();

        var h = new Graph();
        fuseki.LoadGraph(h, "http://example.org/fusekiTest");

        Assert.Equal(g, h);
    }

    [Fact]
    public void StorageFusekiDeleteGraph()
    {
        StorageFusekiSaveGraph();

        FusekiConnector fuseki = FusekiTests.GetConnection();
        fuseki.DeleteGraph("http://example.org/fusekiTest");

        var g = new Graph();
        try
        {
            fuseki.LoadGraph(g, "http://example.org/fusekiTest");
        }
        catch (Exception)
        {
            // Expect exception or empty graph
        }

        //If we do get here without erroring then the Graph should be empty
        Assert.True(g.IsEmpty,
            "Graph should be empty even if an error wasn't thrown as the data should have been deleted from the Store");
    }

    [Fact]
    public void StorageFusekiDeleteDefaultGraph()
    {
        StorageFusekiSaveDefaultGraph();

        FusekiConnector fuseki = FusekiTests.GetConnection();
        fuseki.DeleteGraph((Uri)null);

        var g = new Graph();
        try
        {
            fuseki.LoadGraph(g, (Uri)null);
        }
        catch (Exception)
        {
            // Expect exception or empty graph
        }


        //If we do get here without erroring then the Graph should be empty
        Assert.True(g.IsEmpty,
            "Graph should be empty even if an error wasn't thrown as the data should have been deleted from the Store");
    }

    [Fact]
    public void StorageFusekiDeleteDefaultGraph2()
    {
        StorageFusekiSaveDefaultGraph();

        FusekiConnector fuseki = FusekiTests.GetConnection();
        fuseki.DeleteGraph((String)null);

        var g = new Graph();
        try
        {
            fuseki.LoadGraph(g, (Uri)null);
        }
        catch (Exception)
        {
            // Expect exception or empty graph
        }

        //If we do get here without erroring then the Graph should be empty
        Assert.True(g.IsEmpty,
            "Graph should be empty even if an error wasn't thrown as the data should have been deleted from the Store");
    }

    [Fact]
    public void StorageFusekiAddTriples()
    {
        StorageFusekiSaveGraph();

        var g = new Graph();
        var ts = new List<Triple>
        {
            new Triple(g.CreateUriNode(new Uri("http://example.org/subject")),
                g.CreateUriNode(new Uri("http://example.org/predicate")),
                g.CreateUriNode(new Uri("http://example.org/object")))
        };

        FusekiConnector fuseki = FusekiTests.GetConnection();
        fuseki.UpdateGraph("http://example.org/fusekiTest", ts, null);

        fuseki.LoadGraph(g, "http://example.org/fusekiTest");
        Assert.True(ts.All(t => g.ContainsTriple(t)), "Added Triple should have been in the Graph");
    }

    [Fact]
    public void StorageFusekiRemoveTriples()
    {
        StorageFusekiSaveGraph();

        var g = new Graph();
        var ts = new List<Triple>
        {
            new Triple(g.CreateUriNode(new Uri("http://example.org/subject")),
                g.CreateUriNode(new Uri("http://example.org/predicate")),
                g.CreateUriNode(new Uri("http://example.org/object")))
        };

        FusekiConnector fuseki = FusekiTests.GetConnection();
        fuseki.UpdateGraph("http://example.org/fusekiTest", null, ts);

        fuseki.LoadGraph(g, "http://example.org/fusekiTest");
        Assert.True(ts.All(t => !g.ContainsTriple(t)), "Removed Triple should not have been in the Graph");
    }

    [Fact]
    public void StorageFusekiQuery()
    {
        FusekiConnector fuseki = FusekiTests.GetConnection();

        object results = fuseki.Query("SELECT * WHERE { {?s ?p ?o} UNION { GRAPH ?g {?s ?p ?o} } }");
        if (results is SparqlResultSet)
        {
            //TestTools.ShowResults(results);
        }
        else
        {
            Assert.Fail("Did not get a SPARQL Result Set as expected");
        }
    }

    [Fact]
    public void StorageFusekiUpdate()
    {
        FusekiConnector fuseki = FusekiTests.GetConnection();

        //Try doing a SPARQL Update LOAD command
        var command = "LOAD <http://dbpedia.org/resource/Ilkeston> INTO GRAPH <http://example.org/Ilson>";
        fuseki.Update(command);

        //Then see if we can retrieve the newly loaded graph
        IGraph g = new Graph();
        fuseki.LoadGraph(g, "http://example.org/Ilson");
        Assert.False(g.IsEmpty, "Graph should be non-empty");

        //Try a DROP Graph to see if that works
        command = "DROP GRAPH <http://example.org/Ilson>";
        fuseki.Update(command);

        g = new Graph();
        fuseki.LoadGraph(g, "http://example.org/Ilson");
        Assert.True(g.IsEmpty, "Graph should be empty as it should have been DROPped by Fuseki");
    }

    [Fact]
    public void StorageFusekiDescribe()
    {
        FusekiConnector fuseki = FusekiTests.GetConnection();

        object results = fuseki.Query("DESCRIBE <http://example.org/vehicles/FordFiesta>");
        if (results is IGraph)
        {
            //TestTools.ShowGraph((IGraph) results);
        }
        else
        {
            Assert.Fail("Did not return a Graph as expected");
        }
    }
}
