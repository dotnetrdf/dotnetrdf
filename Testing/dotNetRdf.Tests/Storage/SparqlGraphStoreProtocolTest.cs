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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage;


public class SparqlGraphStoreProtocolTest
{
    private NTriplesFormatter _formatter = new NTriplesFormatter();

    public static SparqlHttpProtocolConnector GetConnection()
    {
        Assert.SkipUnless(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseIIS), "Test Config marks IIS as unavailable, cannot run test");
        return new SparqlHttpProtocolConnector(TestConfigManager.GetSetting(TestConfigManager.LocalGraphStoreUri));
    }

    [Fact]
    public void StorageSparqlUniformHttpProtocolSaveGraph()
    {
        try
        {
            SetUriLoaderCaching(false);

            var g = new Graph();
            FileLoader.Load(g, Path.Combine("resources", "Turtle.ttl"));
            g.BaseUri = new Uri("http://example.org/sparqlTest");

            //Save Graph to SPARQL Uniform Protocol
            SparqlHttpProtocolConnector sparql = SparqlGraphStoreProtocolTest.GetConnection();
            sparql.SaveGraph(g);

            //Now retrieve Graph from SPARQL Uniform Protocol
            var h = new Graph();
            sparql.LoadGraph(h, "http://example.org/sparqlTest");

            GraphDiffReport diff = g.Difference(h);
            if (!diff.AreEqual)
            {
                TestTools.ShowDifferences(diff);
                Assert.True(diff.AddedTriples.Count() == 1, "Should only be 1 Triple difference due to New Line normalization");
                Assert.True(diff.RemovedTriples.Count() == 1, "Should only be 1 Triple difference due to New Line normalization");
                Assert.False(diff.AddedMSGs.Any(), "Should not be any MSG differences");
                Assert.False(diff.RemovedMSGs.Any(), "Should not be any MSG differences");
            }
        }
        finally
        {
            SetUriLoaderCaching(true);
        }
    }

    [Fact]
    public void StorageSparqlUniformHttpProtocolSaveGraph2()
    {
        try
        {
            SetUriLoaderCaching(false);

            var g = new Graph();
            FileLoader.Load(g, Path.Combine("resources", "Turtle.ttl"));
            g.BaseUri = new Uri("http://example.org/sparql#test");

            //Save Graph to SPARQL Uniform Protocol
            SparqlHttpProtocolConnector sparql = SparqlGraphStoreProtocolTest.GetConnection();
            sparql.SaveGraph(g);

            //Now retrieve Graph from SPARQL Uniform Protocol
            var h = new Graph();
            sparql.LoadGraph(h, "http://example.org/sparql#test");

            GraphDiffReport diff = g.Difference(h);
            if (!diff.AreEqual)
            {
                TestTools.ShowDifferences(diff);
                Assert.True(diff.AddedTriples.Count() == 1, "Should only be 1 Triple difference due to New Line normalization");
                Assert.True(diff.RemovedTriples.Count() == 1, "Should only be 1 Triple difference due to New Line normalization");
                Assert.False(diff.AddedMSGs.Any(), "Should not be any MSG differences");
                Assert.False(diff.RemovedMSGs.Any(), "Should not be any MSG differences");
            }
        }
        finally
        {
            SetUriLoaderCaching(true);
        }
    }

    [Fact]
    public void StorageSparqlUniformHttpProtocolLoadGraph()
    {
        try
        {
            SetUriLoaderCaching(false);
            //Ensure that the Graph will be there using the SaveGraph() test
            StorageSparqlUniformHttpProtocolSaveGraph();

            var g = new Graph();
            FileLoader.Load(g, Path.Combine("resources", "Turtle.ttl"));
            g.BaseUri = new Uri("http://example.org/sparqlTest");

            //Try to load the relevant Graph back from the Store
            SparqlHttpProtocolConnector sparql = SparqlGraphStoreProtocolTest.GetConnection();

            var h = new Graph();
            sparql.LoadGraph(h, "http://example.org/sparqlTest");

            GraphDiffReport diff = g.Difference(h);
            if (!diff.AreEqual)
            {
                TestTools.ShowDifferences(diff);
                Assert.True(diff.AddedTriples.Count() == 1, "Should only be 1 Triple difference due to New Line normalization (added)");
                Assert.True(diff.RemovedTriples.Count() == 1, "Should only be 1 Triple difference due to New Line normalization (removed)");
                Assert.False(diff.AddedMSGs.Any(), "Should not be any MSG differences");
                Assert.False(diff.RemovedMSGs.Any(), "Should not be any MSG differences");
            }
        }
        finally
        {
            SetUriLoaderCaching(true);
        }
    }

    [Fact]
    public void StorageSparqlUniformHttpProtocolGraphExists()
    {
        try
        {
            SetUriLoaderCaching(false);
            //Ensure that the Graph will be there using the SaveGraph() test
            StorageSparqlUniformHttpProtocolSaveGraph();

            //Check the Graph exists in the Store
            SparqlHttpProtocolConnector sparql = SparqlGraphStoreProtocolTest.GetConnection();
            Assert.True(sparql.HasGraph("http://example.org/sparqlTest"));
        }
        finally
        {
            SetUriLoaderCaching(true);
        }
    }

    [Fact]
    public void StorageSparqlUniformHttpProtocolDeleteGraph()
    {
        try
        {
            SetUriLoaderCaching(false);
            StorageSparqlUniformHttpProtocolSaveGraph();

            SparqlHttpProtocolConnector sparql = SparqlGraphStoreProtocolTest.GetConnection();
            sparql.DeleteGraph("http://example.org/sparqlTest");

            //Give SPARQL Uniform Protocol time to delete stuff
            Thread.Sleep(1000);

            try
            {
                var g = new Graph();
                sparql.LoadGraph(g, "http://example.org/sparqlTest");

                //If we do get here without erroring then the Graph should be empty
                Assert.True(g.IsEmpty, "If the Graph loaded without error then it should have been empty as we deleted it from the store");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errored as expected since the Graph was deleted");
                TestTools.ReportError("Error", ex);
            }
        }
        finally
        {
            SetUriLoaderCaching(true);
        }
    }

    [Fact]
    public void StorageSparqlUniformHttpProtocolAddTriples()
    {
        try
        {
            SetUriLoaderCaching(false);

            StorageSparqlUniformHttpProtocolSaveGraph();

            var g = new Graph();
            g.Retract(g.Triples.Where(t => !t.IsGroundTriple));
            FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

            SparqlHttpProtocolConnector sparql = SparqlGraphStoreProtocolTest.GetConnection();
            sparql.UpdateGraph("http://example.org/sparqlTest", g.Triples, null);

            var h = new Graph();
            sparql.LoadGraph(h, "http://example.org/sparqlTest");

            foreach (Triple t in h.Triples)
            {
                Console.WriteLine(t.ToString(_formatter));
            }

            Assert.True(g.IsSubGraphOf(h), "Retrieved Graph should have the added Triples as a Sub Graph");
        }
        finally
        {
            SetUriLoaderCaching(true);
        }
    }

    [Fact]
    public void StorageSparqlUniformHttpProtocolRemoveTriples()
    {
        try
        {
            SetUriLoaderCaching(false);
            var g = new Graph();
            FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

            try
            {
                SparqlHttpProtocolConnector sparql = SparqlGraphStoreProtocolTest.GetConnection();
                sparql.UpdateGraph("http://example.org/sparqlTest", null, g.Triples);

                Assert.Fail("SPARQL Uniform HTTP Protocol does not support removing Triples");
            }
            catch (RdfStorageException storeEx)
            {
                Console.WriteLine("Got an error as expected");
                TestTools.ReportError("Storage Error", storeEx);
            }
            catch (NotSupportedException ex)
            {
                Console.WriteLine("Got a Not Supported error as expected");
                TestTools.ReportError("Not Supported", ex);
            }
        }
        finally
        {
            SetUriLoaderCaching(true);
        }
    }

    [Fact]
    public void StorageSparqlUniformHttpProtocolPostCreate()
    {
        SparqlHttpProtocolConnector connector = SparqlGraphStoreProtocolTest.GetConnection();

        var request = (HttpWebRequest)WebRequest.Create(TestConfigManager.GetSetting(TestConfigManager.LocalGraphStoreUri));
        request.Method = "POST";
        request.ContentType = "application/rdf+xml";

        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        using (var writer = new StreamWriter(request.GetRequestStream()))
        {
            var rdfxmlwriter = new RdfXmlWriter();
            rdfxmlwriter.Save(g, writer);
            writer.Close();
        }

        using (var response = (HttpWebResponse)request.GetResponse())
        {
            //Should get a 201 Created response
            if (response.StatusCode == HttpStatusCode.Created)
            {
                if (response.Headers["Location"] == null) Assert.Fail("A Location: Header containing the URI of the newly created Graph should have been returned");
                var graphUri = new Uri(response.Headers["Location"]);

                Console.WriteLine("New Graph URI is " + graphUri.ToString());

                Console.WriteLine("Now attempting to retrieve this Graph from the Store");
                var h = new Graph();
                connector.LoadGraph(h, graphUri);

                TestTools.ShowGraph(h);

                Assert.Equal(g, h);
            }
            else
            {
                Assert.Fail("A 201 Created response should have been received but got a " + (int)response.StatusCode + " response");
            }
            response.Close();
        }
    }

    [Fact]
    public void StorageSparqlUniformHttpProtocolPostCreateMultiple()
    {
        SparqlHttpProtocolConnector connector = SparqlGraphStoreProtocolTest.GetConnection();

        var g = new Graph();
        FileLoader.Load(g, Path.Combine("resources", "InferenceTest.ttl"));

        var uris = new List<Uri>();
        for (var i = 0; i < 10; i++)
        {
            var request = (HttpWebRequest)WebRequest.Create(TestConfigManager.GetSetting(TestConfigManager.LocalGraphStoreUri));
            request.Method = "POST";
            request.ContentType = "application/rdf+xml";

            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                var rdfxmlwriter = new RdfXmlWriter();
                rdfxmlwriter.Save(g, writer);
                writer.Close();
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                //Should get a 201 Created response
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    if (response.Headers["Location"] == null) Assert.Fail("A Location: Header containing the URI of the newly created Graph should have been returned");
                    var graphUri = new Uri(response.Headers["Location"]);
                    uris.Add(graphUri);

                    Console.WriteLine("New Graph URI is " + graphUri.ToString());

                    Console.WriteLine("Now attempting to retrieve this Graph from the Store");
                    var h = new Graph();
                    connector.LoadGraph(h, graphUri);

                    Assert.Equal(g, h);
                    Console.WriteLine("Graphs were equal as expected");
                }
                else
                {
                    Assert.Fail("A 201 Created response should have been received but got a " + (int)response.StatusCode + " response");
                }
                response.Close();
            }
            Console.WriteLine();
        }

        Assert.True(uris.Distinct().Count() == 10, "Should have generated 10 distinct URIs");
    }

    /// <summary>
    /// Provides a wrapper around the UriLoader.CacheEnabled option
    /// that allows it to be conditionally excluded when building
    /// tests for platforms that don't support it.
    /// </summary>
    /// <param name="cacheEnabled"></param>
    private void SetUriLoaderCaching(bool cacheEnabled)
    {
        // Caching is no longer controlled this way.
        //UriLoader.CacheEnabled = cacheEnabled;
    }
}
