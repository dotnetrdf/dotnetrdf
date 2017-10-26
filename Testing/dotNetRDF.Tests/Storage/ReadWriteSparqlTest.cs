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
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;
using VDS.RDF.XunitExtensions;

namespace VDS.RDF.Storage
{
    public class ReadWriteSparqlTests
    {
        private NTriplesFormatter _formatter = new NTriplesFormatter();

        public static ReadWriteSparqlConnector GetConnection()
        {
            return new ReadWriteSparqlConnector(RemoteEndpoints.GetQueryEndpoint(), RemoteEndpoints.GetUpdateEndpoint());
        }

        private void SetUriLoaderCaching(bool newValue)
        {
            Options.UriLoaderCaching = newValue;
        }

        [SkippableFact]
        public void StorageReadWriteSparqlSaveGraph()
        {
            try
            {
                SetUriLoaderCaching(false);

                Graph g = new Graph();
                g.LoadFromFile("resources\\InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/readWriteTest");

                //Save Graph to ReadWriteSparql
                ReadWriteSparqlConnector readWrite = ReadWriteSparqlTests.GetConnection();
                readWrite.SaveGraph(g);
                Console.WriteLine("Graph saved to ReadWriteSparql OK");

                //Now retrieve Graph from ReadWriteSparql
                Graph h = new Graph();
                readWrite.LoadGraph(h, "http://example.org/readWriteTest");

                Console.WriteLine();
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(this._formatter));
                }

                Assert.Equal(g, h);
            }
            finally
            {
                SetUriLoaderCaching(true);
            }
        }

        [SkippableFact]
        public void StorageReadWriteSparqlSaveDefaultGraph()
        {
            try
            {
                SetUriLoaderCaching(false);

                Graph g = new Graph();
                g.LoadFromFile("resources\\InferenceTest.ttl");
                g.BaseUri = null;

                //Save Graph to ReadWriteSparql
                ReadWriteSparqlConnector readWrite = ReadWriteSparqlTests.GetConnection();
                readWrite.SaveGraph(g);
                Console.WriteLine("Graph saved to ReadWriteSparql OK");

                //Now retrieve Graph from ReadWriteSparql
                Graph h = new Graph();
                readWrite.LoadGraph(h, (Uri)null);

                Console.WriteLine();
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(this._formatter));
                }

                Assert.Equal(g, h);
                Assert.Null(h.BaseUri);
            }
            finally
            {
                SetUriLoaderCaching(true);
            }
        }

        [SkippableFact]
        public void StorageReadWriteSparqlSaveDefaultGraph2()
        {
            try
            {
                SetUriLoaderCaching(false);

                Graph g = new Graph();
                g.LoadFromFile("resources\\InferenceTest.ttl");
                g.BaseUri = null;

                //Save Graph to ReadWriteSparql
                ReadWriteSparqlConnector readWrite = ReadWriteSparqlTests.GetConnection();
                readWrite.SaveGraph(g);
                Console.WriteLine("Graph saved to ReadWriteSparql OK");

                //Now retrieve Graph from ReadWriteSparql
                Graph h = new Graph();
                readWrite.LoadGraph(h, (String)null);

                Console.WriteLine();
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(this._formatter));
                }

                Assert.Equal(g, h);
                Assert.Null(h.BaseUri);
            }
            finally
            {
                SetUriLoaderCaching(true);
            }
        }

        [SkippableFact]
        public void StorageReadWriteSparqlLoadGraph()
        {
            try
            {
                SetUriLoaderCaching(false);

                //Ensure that the Graph will be there using the SaveGraph() test
                StorageReadWriteSparqlSaveGraph();

                Graph g = new Graph();
                g.LoadFromFile("resources\\InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/readWriteTest");

                //Try to load the relevant Graph back from the Store
                ReadWriteSparqlConnector readWrite = ReadWriteSparqlTests.GetConnection();

                Graph h = new Graph();
                readWrite.LoadGraph(h, "http://example.org/readWriteTest");

                Console.WriteLine();
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(this._formatter));
                }

                Assert.Equal(g, h);
            }
            finally
            {
                SetUriLoaderCaching(true);
            }
        }

        [SkippableFact]
        public void StorageReadWriteSparqlDeleteGraph()
        {
            try
            {
                SetUriLoaderCaching(false);

                StorageReadWriteSparqlSaveGraph();

                ReadWriteSparqlConnector readWrite = ReadWriteSparqlTests.GetConnection();
                readWrite.DeleteGraph("http://example.org/readWriteTest");

                Graph g = new Graph();
                try
                {
                    readWrite.LoadGraph(g, "http://example.org/readWriteTest");
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
                SetUriLoaderCaching(true);
            }
        }

        [SkippableFact]
        public void StorageReadWriteSparqlDeleteDefaultGraph()
        {
            try
            {
                SetUriLoaderCaching(false);

                StorageReadWriteSparqlSaveDefaultGraph();

                ReadWriteSparqlConnector readWrite = ReadWriteSparqlTests.GetConnection();
                readWrite.DeleteGraph((Uri)null);

                Graph g = new Graph();
                try
                {
                    readWrite.LoadGraph(g, (Uri)null);
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
                SetUriLoaderCaching(true);
            }
        }

        [SkippableFact]
        public void StorageReadWriteSparqlDeleteDefaultGraph2()
        {
            try
            {
                SetUriLoaderCaching(false);

                StorageReadWriteSparqlSaveDefaultGraph();

                ReadWriteSparqlConnector readWrite = ReadWriteSparqlTests.GetConnection();
                readWrite.DeleteGraph((String)null);

                Graph g = new Graph();
                try
                {
                    readWrite.LoadGraph(g, (Uri)null);
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
                SetUriLoaderCaching(true);
            }
        }

        [SkippableFact]
        public void StorageReadWriteSparqlAddTriples()
        {
            try
            {
                SetUriLoaderCaching(false);

                StorageReadWriteSparqlSaveGraph();

                Graph g = new Graph();
                List<Triple> ts = new List<Triple>();
                ts.Add(new Triple(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object"))));

                ReadWriteSparqlConnector readWrite = ReadWriteSparqlTests.GetConnection();
                readWrite.UpdateGraph("http://example.org/readWriteTest", ts, null);

                readWrite.LoadGraph(g, "http://example.org/readWriteTest");
                Assert.True(ts.All(t => g.ContainsTriple(t)), "Added Triple should have been in the Graph");
            }
            finally
            {
                SetUriLoaderCaching(true);
            }
        }

        [SkippableFact]
        public void StorageReadWriteSparqlRemoveTriples()
        {
            try
            {
                SetUriLoaderCaching(false);

                StorageReadWriteSparqlSaveGraph();

                Graph g = new Graph();
                List<Triple> ts = new List<Triple>();
                ts.Add(new Triple(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object"))));

                ReadWriteSparqlConnector readWrite = ReadWriteSparqlTests.GetConnection();
                readWrite.UpdateGraph("http://example.org/readWriteTest", null, ts);

                readWrite.LoadGraph(g, "http://example.org/readWriteTest");
                Assert.True(ts.All(t => !g.ContainsTriple(t)), "Removed Triple should not have been in the Graph");
            }
            finally
            {
                SetUriLoaderCaching(true);
            }
        }

        [SkippableFact]
        public void StorageReadWriteSparqlQuery()
        {
            ReadWriteSparqlConnector readWrite = ReadWriteSparqlTests.GetConnection();

            Object results = readWrite.Query("SELECT * WHERE { {?s ?p ?o} UNION { GRAPH ?g {?s ?p ?o} } }");
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
        public void StorageReadWriteSparqlUpdate()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing))
            {
                throw new SkipTestException("Test Config marks Remote Parsing as unavailable, test cannot be run");
            }

            try
            {
                Options.HttpDebugging = true;

                ReadWriteSparqlConnector readWrite = ReadWriteSparqlTests.GetConnection();

                //Try doing a SPARQL Update LOAD command
                String command = "LOAD <http://dbpedia.org/resource/Ilkeston> INTO GRAPH <http://example.org/Ilson>";
                readWrite.Update(command);

                //Then see if we can retrieve the newly loaded graph
                IGraph g = new Graph();
                readWrite.LoadGraph(g, "http://example.org/Ilson");
                Assert.False(g.IsEmpty, "Graph should be non-empty");
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString(this._formatter));
                }
                Console.WriteLine();

                //Try a DROP Graph to see if that works
                command = "DROP GRAPH <http://example.org/Ilson>";
                readWrite.Update(command);

                g = new Graph();
                readWrite.LoadGraph(g, "http://example.org/Ilson");
                Assert.True(g.IsEmpty, "Graph should be empty as it should have been DROPped by ReadWriteSparql");
            }
            finally
            {
                Options.HttpDebugging = false;
            }
            
        }

        [SkippableFact]
        public void StorageReadWriteSparqlDescribe()
        {
            try
            {
                Options.HttpDebugging = true;

                ReadWriteSparqlConnector readWrite = ReadWriteSparqlTests.GetConnection();

                Object results = readWrite.Query("DESCRIBE <http://example.org/vehicles/FordFiesta>");
                if (results is IGraph)
                {
                    TestTools.ShowGraph((IGraph)results);
                }
                else
                {
                    Assert.True(false, "Did not return a Graph as expected");
                }
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        [SkippableFact]
        public void StorageReadWriteSparqlConfigSerialization1()
        {
            ReadWriteSparqlConnector connector = ReadWriteSparqlTests.GetConnection();
            Graph g = new Graph();
            INode n = g.CreateBlankNode();
            ConfigurationSerializationContext context = new ConfigurationSerializationContext(g);
            context.NextSubject = n;
            connector.SerializeConfiguration(context);

            TestTools.ShowGraph(g);

            Object temp = ConfigurationLoader.LoadObject(g, n);
            Assert.IsType<ReadWriteSparqlConnector>(temp);
            ReadWriteSparqlConnector connector2 = (ReadWriteSparqlConnector)temp;
            Assert.True(EqualityHelper.AreUrisEqual(connector.Endpoint.Uri, connector2.Endpoint.Uri));
            Assert.True(EqualityHelper.AreUrisEqual(connector.UpdateEndpoint.Uri, connector2.UpdateEndpoint.Uri));
        }
    }
}
