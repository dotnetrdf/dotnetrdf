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
using System.Text;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage.Management;

namespace VDS.RDF.Storage;


public class SesameTests
{
    public static SesameHttpProtocolConnector GetConnection()
    {
        Assert.SkipUnless(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseSesame), "Test Config marks Sesame as unavailable, cannot run test");
        return new SesameHttpProtocolConnector(TestConfigManager.GetSetting(TestConfigManager.SesameServer), TestConfigManager.GetSetting(TestConfigManager.SesameRepository));
    }

    [Fact]
    public void StorageSesameSaveLoad()
    {
        var g = new Graph();
        FileLoader.Load(g, "resources\\InferenceTest.ttl");
        g.BaseUri = new Uri("http://example.org/SesameTest");

        SesameHttpProtocolConnector sesame = SesameTests.GetConnection();
        sesame.SaveGraph(g);

        //Options.HttpDebugging = true;
        //Options.HttpFullDebugging = true;

        var h = new Graph();
        sesame.LoadGraph(h, "http://example.org/SesameTest");
        Assert.False(h.IsEmpty, "Graph should not be empty after loading");

        Assert.Equal(g, h);
    }

    [Fact]
    public void StorageSesameSaveEmptyGraph1()
    {
        var g = new Graph
        {
            BaseUri = new Uri("http://example.org/Sesame/empty")
        };

        SesameHttpProtocolConnector sesame = SesameTests.GetConnection();
        sesame.SaveGraph(g);

        var h = new Graph();
        sesame.LoadGraph(h, "http://example.org/Sesame/empty");
        Assert.True(h.IsEmpty, "Graph should be empty after loading");

        Assert.Equal(g, h);
    }

    [Fact]
    public void StorageSesameSaveEmptyGraph2()
    {
        SesameHttpProtocolConnector sesame = SesameTests.GetConnection();
        var graphUri = new Uri("http://example.org/Sesame/empty2");
        Console.WriteLine("Deleting any existing graph");
        sesame.DeleteGraph(graphUri);
        Console.WriteLine("Existing graph deleted");

        // First create a non-empty graph
        var g = new Graph
        {
            BaseUri = graphUri
        };
        g.Assert(g.CreateBlankNode(), g.CreateUriNode("rdf:type"), g.CreateUriNode(new Uri("http://example.org/BNode")));
        Console.WriteLine("Saving non-empty graph");
        sesame.SaveGraph(g);
        Console.WriteLine("Non-empty graph saved");

        var h = new Graph();
        sesame.LoadGraph(h, graphUri);
        Assert.False(h.IsEmpty, "Graph should not be empty after loading");

        Assert.Equal(g, h);

        // Now attempt to save an empty graph as well
        g = new Graph
        {
            BaseUri = graphUri
        };
        Console.WriteLine("Attempting to save empty graph with same name");
        sesame.SaveGraph(g);
        Console.WriteLine("Empty graph saved");

        h = new Graph();
        sesame.LoadGraph(h, graphUri);
        Assert.True(h.IsEmpty, "Graph should be empty after loading");

        Assert.Equal(g, h);
    }

    [Fact]
    public void StorageSesameSaveEmptyGraph3()
    {
        SesameHttpProtocolConnector sesame = SesameTests.GetConnection();
        Uri graphUri = null;
        Console.WriteLine("Deleting any existing graph");
        sesame.DeleteGraph(graphUri);
        Console.WriteLine("Existing graph deleted");

        // First create a non-empty graph
        var g = new Graph
        {
            BaseUri = graphUri
        };
        g.Assert(g.CreateBlankNode(), g.CreateUriNode("rdf:type"), g.CreateUriNode(new Uri("http://example.org/BNode")));
        Console.WriteLine("Saving non-empty graph");
        sesame.SaveGraph(g);
        Console.WriteLine("Non-empty graph saved");

        var h = new Graph();
        sesame.LoadGraph(h, graphUri);
        Assert.False(h.IsEmpty, "Graph should not be empty after loading");

        Assert.Equal(g, h);

        // Now attempt to overwrite with an empty graph
        g = new Graph
        {
            BaseUri = graphUri
        };
        Console.WriteLine("Attempting to save empty graph with same name");
        sesame.SaveGraph(g);
        Console.WriteLine("Empty graph saved");

        h = new Graph();
        sesame.LoadGraph(h, graphUri);

        // Since saving to default graph does not overwrite the graph we've just retrieved must contain the empty graph as a sub-graph
        Assert.True(h.HasSubGraph(g));
    }

    [Fact]
    public void StorageSesameDeleteTriples1()
    {
        var g = new Graph();
        FileLoader.Load(g, "resources\\InferenceTest.ttl");
        g.BaseUri = new Uri("http://example.org/SesameTest");

        SesameHttpProtocolConnector sesame = SesameTests.GetConnection();
        sesame.SaveGraph(g);

        //Delete all Triples about the Ford Fiesta
        sesame.UpdateGraph(g.BaseUri, null, g.GetTriplesWithSubject(new Uri("http://example.org/vehicles/FordFiesta")));

        var results = sesame.Query("ASK WHERE { GRAPH <http://example.org/SesameTest> { <http://example.org/vehicles/FordFiesta> ?p ?o } }");
        if (results is SparqlResultSet)
        {
            Assert.False(((SparqlResultSet) results).Result, "There should no longer be any triples about the Ford Fiesta present");
        }

        var h = new Graph();
        sesame.LoadGraph(h, g.BaseUri);

        Assert.False(h.IsEmpty, "Graph should not be completely empty");
        Assert.True(g.HasSubGraph(h), "Graph retrieved with missing Triples should be a sub-graph of the original Graph");
        Assert.False(g.Equals(h), "Graph retrieved should not be equal to original Graph");
    }

    [Fact]
    public void StorageSesameDeleteTriples2()
    {
        var g = new Graph
        {
            BaseUri = new Uri("http://example.org/SesameTest/Delete2")
        };
        g.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/ns#"));
        g.Assert(g.CreateUriNode("ex:subj"), g.CreateUriNode("ex:pred"), g.CreateUriNode("ex:obj"));

        SesameHttpProtocolConnector sesame = SesameTests.GetConnection();
        sesame.SaveGraph(g);

        //Delete the single triple
        sesame.UpdateGraph(g.BaseUri, null, g.Triples);

        var results = sesame.Query("ASK WHERE { GRAPH <http://example.org/SesameTest/Delete2> { <http://example.org/ns#subj> ?p ?o } }");
        if (results is SparqlResultSet)
        {
            Assert.False(((SparqlResultSet) results).Result, "There should no longer be any triples present in the graph");
        }

        var h = new Graph();
        sesame.LoadGraph(h, g.BaseUri);

        Assert.True(h.IsEmpty, "Graph should not be completely empty");
        Assert.False(g.Equals(h), "Graph retrieved should not be equal to original Graph");
    }

    [Fact]
    public void StorageSesameDeleteTriples3()
    {
        SesameHttpProtocolConnector sesame = SesameTests.GetConnection();
        var g = new Graph
        {
            BaseUri = new Uri("http://example.org/sesame/chinese")
        };
        FileLoader.Load(g, @"resources\chinese.ttl");
        sesame.SaveGraph(g);

        var ask = "ASK WHERE { GRAPH <http://example.org/sesame/chinese> { ?s ?p '例子' } }";

        var results = sesame.Query(ask);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet)
        {
            Assert.True(((SparqlResultSet) results).Result);
        }

        // Now delete the triple in question
        sesame.UpdateGraph(g.BaseUri, null, g.Triples);

        // Re-issue ASK to check deletion
        results = sesame.Query(ask);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet)
        {
            Assert.False(((SparqlResultSet) results).Result);
        }
    }

    [Fact]
    public void StorageSesameDeleteGraph1()
    {
        SesameHttpProtocolConnector sesame = SesameTests.GetConnection();
        var graphUri = new Uri("http://example.org/Sesame/delete");

        var g = new Graph();
        FileLoader.Load(g, "resources\\InferenceTest.ttl");
        g.BaseUri = graphUri;

        sesame.SaveGraph(g);

        var h = new Graph();
        sesame.LoadGraph(h, graphUri);
        Assert.False(h.IsEmpty, "Graph should not be empty after loading");

        Assert.Equal(g, h);

        sesame.DeleteGraph(graphUri);
        h = new Graph();
        sesame.LoadGraph(h, graphUri);
        Assert.True(h.IsEmpty, "Graph should be equal after deletion");
        Assert.NotEqual(g, h);
    }

    [Fact]
    public void StorageSesameDeleteGraph2()
    {
        SesameHttpProtocolConnector sesame = SesameTests.GetConnection();
        Uri graphUri = null;
        sesame.DeleteGraph(graphUri);

        var g = new Graph();
        FileLoader.Load(g, "resources\\InferenceTest.ttl");
        g.BaseUri = graphUri;

        sesame.SaveGraph(g);

        var h = new Graph();
        sesame.LoadGraph(h, graphUri);
        Assert.False(h.IsEmpty, "Graph should not be empty after loading");

        Assert.Equal(g, h);

        sesame.DeleteGraph(graphUri);
        h = new Graph();
        sesame.LoadGraph(h, graphUri);
        Assert.True(h.IsEmpty, "Graph should be equal after deletion");
        Assert.NotEqual(g, h);
    }

    [Fact]
    public void StorageSesameCyrillic()
    {
        SesameHttpProtocolConnector sesame = SesameTests.GetConnection();
        var g = new Graph
        {
            BaseUri = new Uri("http://example.org/sesame/cyrillic")
        };
        FileLoader.Load(g, @"resources\cyrillic.rdf");
        sesame.SaveGraph(g);

        var ask = "ASK WHERE { GRAPH <http://example.org/sesame/cyrillic> { ?s ?p 'литерал' } }";

        var results = sesame.Query(ask);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet)
        {
            Assert.True(((SparqlResultSet) results).Result);
        }
    }

    [Fact]
    public void StorageSesameChinese()
    {
        SesameHttpProtocolConnector sesame = SesameTests.GetConnection();
        var g = new Graph
        {
            BaseUri = new Uri("http://example.org/sesame/chinese")
        };
        FileLoader.Load(g, @"resources\chinese.ttl");
        sesame.SaveGraph(g);

        var ask = "ASK WHERE { GRAPH <http://example.org/sesame/chinese> { ?s ?p '例子' } }";

        var results = sesame.Query(ask);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet)
        {
            Assert.True(((SparqlResultSet) results).Result);
        }
    }

    [Fact]
    public void StorageSesameAsk()
    {
        SesameHttpProtocolConnector sesame = SesameTests.GetConnection();

        var ask = "ASK WHERE { ?s ?p ?o }";

        var results = sesame.Query(ask);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
    }

    [Fact]
    public void StorageSesameDescribe()
    {
        SesameHttpProtocolConnector sesame = SesameTests.GetConnection();

        var describe = "DESCRIBE <http://example.org/vehicles/FordFiesta>";

        var results = sesame.Query(describe);
        Assert.IsType<IGraph>(results, exactMatch: false);
    }

    [Fact]
    public void StorageSesameSparqlUpdate1()
    {
        Assert.SkipUnless(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing), "Test Config marks Remote Parsing as unavailable, test cannot be run");

        SesameHttpProtocolConnector sesame = SesameTests.GetConnection();
        sesame.Update(@"DROP GRAPH <http://example.org/sparqlUpdateLoad>;
LOAD <http://dbpedia.org/resource/Ilkeston> INTO GRAPH <http://example.org/sparqlUpdateLoad>;
DELETE WHERE 
{ 
  GRAPH <http://example.org/sparqlUpdateLoad> 
  { ?s <http://www.w3.org/2003/01/geo/wgs84_pos#long> ?long ; <http://www.w3.org/2003/01/geo/wgs84_pos#lat> ?lat }
}");

        var orig = new Graph();
        orig.LoadFromUri(new Uri("http://dbpedia.org/resource/Ilkeston"));
        orig.Retract(orig.GetTriplesWithPredicate(new Uri("http://www.w3.org/2003/01/geo/wgs84_pos#long")).ToList());
        orig.Retract(orig.GetTriplesWithPredicate(new Uri("http://www.w3.org/2003/01/geo/wgs84_pos#lat")).ToList());

        var actual = new Graph();
        sesame.LoadGraph(actual, "http://example.org/sparqlUpdateLoad");

        GraphDiffReport diff = orig.Difference(actual);
        Assert.True(diff.AreEqual);

        Assert.Equal(orig, actual);
    }

    [Fact]
    public void StorageSesameSparqlUpdate2()
    {
        SesameHttpProtocolConnector sesame = SesameTests.GetConnection();
        var g = new Graph();
        g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        g.BaseUri = new Uri("http://example.org/sparqlUpdateDeleteWhere");
        sesame.SaveGraph(g);

        sesame.Update("WITH <http://example.org/sparqlUpdateDeleteWhere> DELETE { ?s a ?type } WHERE { ?s a ?type }");

        var h = new Graph();
        sesame.LoadGraph(h, "http://example.org/sparqlUpdateDeleteWhere");
        INode rdfType = h.CreateUriNode("rdf:type");
        Assert.False(h.GetTriplesWithPredicate(rdfType).Any(), "Should not be any rdf:type triples after SPARQL Update operation");
    }

    [Fact]
    public void StorageSesameSparqlUpdate3()
    {
        SesameHttpProtocolConnector sesame = SesameTests.GetConnection();

        // Ensure required graph is present
        var g = new Graph
        {
            BaseUri = new Uri("http://example.org/sesame/chinese")
        };
        FileLoader.Load(g, @"resources\chinese.ttl");
        sesame.SaveGraph(g);

        var ask = "ASK WHERE { GRAPH <http://example.org/sesame/chinese> { ?s ?p '例子' } }";

        // Issue query to validate data was added
        var results = sesame.Query(ask);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet)
        {
            Assert.True(((SparqlResultSet) results).Result);
        }

        // Issue a DELETE for the Chinese literal
        var update = "DELETE WHERE { GRAPH <http://example.org/sesame/chinese> { ?s ?p '例子' } }";
        sesame.Update(update);

        // Re-issue query to validate triple was deleted
        results = sesame.Query(ask);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet)
        {
            Assert.False(((SparqlResultSet) results).Result);
        }
    }

    [Fact]
    public void StorageSesameSparqlUpdate4()
    {
        // Test case adapted from CORE-374 sample update
        SesameHttpProtocolConnector sesame = SesameTests.GetConnection();

        // Insert the Data
        var updates = new StringBuilder();
        using (StreamReader reader = File.OpenText(@"resources\core-374.ru"))
        {
            updates.Append(reader.ReadToEnd());
            reader.Close();
        }
        sesame.Update(updates.ToString());

        var ask = "ASK WHERE { GRAPH <http://example.org/sesame/core-374> { ?s ?p 'République du Niger'@fr } }";

        // Issue query to validate data was added
        var results = sesame.Query(ask);
        Assert.IsType<SparqlResultSet>(results, exactMatch: false);
        if (results is SparqlResultSet)
        {
            Assert.True(((SparqlResultSet) results).Result);
        }
    }

    [Fact]
    public void SesameServerDispose()
    {
        // Reproduction of issue #507 - NRE on dispose
        var sesame = new SesameServer("http://localhost:8080/sesame");
        sesame.Dispose();
    }
}
