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
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;
using Xunit.Abstractions;

namespace VDS.RDF.Storage
{
    public class FusekiTest
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly NTriplesFormatter _formatter = new NTriplesFormatter();

        public FusekiTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public static FusekiConnector GetConnection(string uploadMimeType = null)
        {
            Skip.IfNot(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseFuseki),
                "Test Configuration marks Fuseki as unavailable, test cannot be run");
            var mimeTypeDescription =
                uploadMimeType == null ? null : MimeTypesHelper.GetDefinitions(uploadMimeType).First();
            return new FusekiConnector(TestConfigManager.GetSetting(TestConfigManager.FusekiServer), mimeTypeDescription);
        }

        [SkippableTheory]
        [InlineData("application/rdf+xml")]
        [InlineData("application/n-triples")]
        public void StorageFusekiSaveGraph(string mimeType = null)
        {
            try
            {
                UriLoader.CacheEnabled = false;
                var g = new Graph();
                FileLoader.Load(g, "resources\\InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/fusekiTest");

                //Save Graph to Fuseki
                FusekiConnector fuseki = FusekiTest.GetConnection(mimeType);
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
            finally
            {
                UriLoader.CacheEnabled = true;
            }
        }

        [SkippableFact]
        public void StorageFusekiSaveGraph2()
        {
            try
            {
                UriLoader.CacheEnabled = false;
                var g = new Graph();
                FileLoader.Load(g, "resources\\InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/fuseki#test");

                //Save Graph to Fuseki
                FusekiConnector fuseki = FusekiTest.GetConnection();
                fuseki.SaveGraph(g);
                Console.WriteLine("Graph saved to Fuseki OK");

                //Now retrieve Graph from Fuseki
                var h = new Graph();
                fuseki.LoadGraph(h, "http://example.org/fuseki#test");

                Console.WriteLine();
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(_formatter));
                }

                Assert.Equal(g, h);
            }
            finally
            {
                UriLoader.CacheEnabled = true;
            }
        }

        [SkippableFact]
        public void StorageFusekiSaveDefaultGraph()
        {
            try
            {
                UriLoader.CacheEnabled = false;
                var g = new Graph();
                FileLoader.Load(g, "resources\\InferenceTest.ttl");
                g.BaseUri = null;

                //Save Graph to Fuseki
                FusekiConnector fuseki = FusekiTest.GetConnection();
                fuseki.SaveGraph(g);
                Console.WriteLine("Graph saved to Fuseki OK");

                //Now retrieve Graph from Fuseki
                var h = new Graph();
                fuseki.LoadGraph(h, (Uri)null);

                Console.WriteLine();
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(_formatter));
                }

                Assert.Equal(g, h);
                Assert.Null(h.BaseUri);
            }
            finally
            {
                UriLoader.CacheEnabled = true;
            }
        }

        [SkippableFact]
        public void StorageFusekiSaveDefaultGraph2()
        {
            try
            {
                UriLoader.CacheEnabled = false;
                var g = new Graph();
                FileLoader.Load(g, "resources\\InferenceTest.ttl");
                g.BaseUri = null;

                //Save Graph to Fuseki
                FusekiConnector fuseki = FusekiTest.GetConnection();
                fuseki.SaveGraph(g);
                Console.WriteLine("Graph saved to Fuseki OK");

                //Now retrieve Graph from Fuseki
                var h = new Graph();
                fuseki.LoadGraph(h, (String)null);

                Console.WriteLine();
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(_formatter));
                }

                Assert.Equal(g, h);
                Assert.Null(h.BaseUri);
            }
            finally
            {
                UriLoader.CacheEnabled = true;
            }
        }

        [SkippableFact]
        public void StorageFusekiLoadGraph()
        {
            try
            {
                UriLoader.CacheEnabled = false;
                //Ensure that the Graph will be there using the SaveGraph() test
                StorageFusekiSaveGraph();

                var g = new Graph();
                FileLoader.Load(g, "resources\\InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/fusekiTest");

                //Try to load the relevant Graph back from the Store
                FusekiConnector fuseki = FusekiTest.GetConnection();

                var h = new Graph();
                fuseki.LoadGraph(h, "http://example.org/fusekiTest");

                Console.WriteLine();
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(_formatter));
                }

                Assert.Equal(g, h);
            }
            finally
            {
                UriLoader.CacheEnabled = true;
            }
        }

        [SkippableFact]
        public void StorageFusekiDeleteGraph()
        {
            try
            {
                UriLoader.CacheEnabled = false;
                StorageFusekiSaveGraph();

                FusekiConnector fuseki = FusekiTest.GetConnection();
                fuseki.DeleteGraph("http://example.org/fusekiTest");

                var g = new Graph();
                try
                {
                    fuseki.LoadGraph(g, "http://example.org/fusekiTest");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Errored as expected since the Graph was deleted");
                    TestTools.ReportError("Error", ex);
                }
                Console.WriteLine();

                //If we do get here without erroring then the Graph should be empty
                Assert.True(g.IsEmpty, "Graph should be empty even if an error wasn't thrown as the data should have been deleted from the Store");
            }
            finally
            {
                UriLoader.CacheEnabled = true;
            }
        }

        [SkippableFact]
        public void StorageFusekiDeleteDefaultGraph()
        {
            try
            {
                UriLoader.CacheEnabled = false;
                StorageFusekiSaveDefaultGraph();

                FusekiConnector fuseki = FusekiTest.GetConnection();
                fuseki.DeleteGraph((Uri)null);

                var g = new Graph();
                try
                {
                    fuseki.LoadGraph(g, (Uri)null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Errored as expected since the Graph was deleted");
                    TestTools.ReportError("Error", ex);
                }
                Console.WriteLine();

                //If we do get here without erroring then the Graph should be empty
                Assert.True(g.IsEmpty, "Graph should be empty even if an error wasn't thrown as the data should have been deleted from the Store");
            }
            finally
            {
                UriLoader.CacheEnabled = true;
            }
        }

        [SkippableFact]
        public void StorageFusekiDeleteDefaultGraph2()
        {
            try
            {
                UriLoader.CacheEnabled = false;
                StorageFusekiSaveDefaultGraph();

                FusekiConnector fuseki = FusekiTest.GetConnection();
                fuseki.DeleteGraph((String)null);

                var g = new Graph();
                try
                {
                    fuseki.LoadGraph(g, (Uri)null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Errored as expected since the Graph was deleted");
                    TestTools.ReportError("Error", ex);
                }
                Console.WriteLine();

                //If we do get here without erroring then the Graph should be empty
                Assert.True(g.IsEmpty, "Graph should be empty even if an error wasn't thrown as the data should have been deleted from the Store");
            }
            finally
            {
                UriLoader.CacheEnabled = true;
            }
        }

        [SkippableFact]
        public void StorageFusekiAddTriples()
        {
            try
            {
                UriLoader.CacheEnabled = false;
                StorageFusekiSaveGraph();

                var g = new Graph();
                var ts = new List<Triple>();
                ts.Add(new Triple(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object"))));

                FusekiConnector fuseki = FusekiTest.GetConnection();
                fuseki.UpdateGraph("http://example.org/fusekiTest", ts, null);

                fuseki.LoadGraph(g, "http://example.org/fusekiTest");
                Assert.True(ts.All(t => g.ContainsTriple(t)), "Added Triple should have been in the Graph");
            }
            finally
            {
                UriLoader.CacheEnabled = true;
            }
        }

        [SkippableFact]
        public void StorageFusekiRemoveTriples()
        {
            try
            {
                UriLoader.CacheEnabled = false;
                StorageFusekiSaveGraph();

                var g = new Graph();
                var ts = new List<Triple>();
                ts.Add(new Triple(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object"))));

                FusekiConnector fuseki = FusekiTest.GetConnection();
                fuseki.UpdateGraph("http://example.org/fusekiTest", null, ts);

                fuseki.LoadGraph(g, "http://example.org/fusekiTest");
                Assert.True(ts.All(t => !g.ContainsTriple(t)), "Removed Triple should not have been in the Graph");
            }
            finally
            {
                UriLoader.CacheEnabled = true;
            }
        }

        [SkippableFact]
        public void StorageFusekiQuery()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();

            var results = fuseki.Query("SELECT * WHERE { {?s ?p ?o} UNION { GRAPH ?g {?s ?p ?o} } }");
            if (results is SparqlResultSet)
            {
                TestTools.ShowResults(results);
            }
            else
            {
                Assert.True(false, "Did not get a SPARQL Result Set as expected");
            }
        }

        [SkippableFact]
        public void StorageFusekiUpdate()
        {
            Skip.IfNot(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing),
                "Test Config marks Remote Parsing as unavailable, test cannot be run");

            FusekiConnector fuseki = FusekiTest.GetConnection();

            //Try doing a SPARQL Update LOAD command
            var command = "LOAD <http://dbpedia.org/resource/Ilkeston> INTO GRAPH <http://example.org/Ilson>";
            fuseki.Update(command);

            //Then see if we can retrieve the newly loaded graph
            IGraph g = new Graph();
            fuseki.LoadGraph(g, "http://example.org/Ilson");
            Assert.False(g.IsEmpty, "Graph should be non-empty");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(_formatter));
            }

            Console.WriteLine();

            //Try a DROP Graph to see if that works
            command = "DROP GRAPH <http://example.org/Ilson>";
            fuseki.Update(command);

            g = new Graph();
            fuseki.LoadGraph(g, "http://example.org/Ilson");
            Assert.True(g.IsEmpty, "Graph should be empty as it should have been DROPped by Fuseki");
        }

        [SkippableFact]
        public void StorageFusekiDescribe()
        {
            FusekiConnector fuseki = FusekiTest.GetConnection();

            var results = fuseki.Query("DESCRIBE <http://example.org/vehicles/FordFiesta>");
            if (results is IGraph)
            {
                TestTools.ShowGraph((IGraph) results);
            }
            else
            {
                Assert.True(false, "Did not return a Graph as expected");
            }
        }
    }
}
