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
using VDS.RDF.Configuration;
using VDS.RDF.Query;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage
{
    public class ReadWriteSparqlTests
    {
        private readonly NTriplesFormatter _formatter = new NTriplesFormatter();

        public static ReadWriteSparqlConnector GetConnection()
        {
            return new ReadWriteSparqlConnector(RemoteEndpoints.GetQueryEndpoint(), RemoteEndpoints.GetUpdateEndpoint());
        }

        [SkippableFact]
        public void StorageReadWriteSparqlSaveGraph()
        {
            try
            {
                var g = new Graph();
                g.LoadFromFile("resources\\InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/readWriteTest");

                //Save Graph to ReadWriteSparql
                ReadWriteSparqlConnector readWrite = GetConnection();
                readWrite.SaveGraph(g);
                Console.WriteLine("Graph saved to ReadWriteSparql OK");

                //Now retrieve Graph from ReadWriteSparql
                var h = new Graph();
                readWrite.LoadGraph(h, "http://example.org/readWriteTest");

                Console.WriteLine();
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(_formatter));
                }

                Assert.Equal(g, h);
            }
            finally
            {
            }
        }

        [SkippableFact]
        public void StorageReadWriteSparqlSaveDefaultGraph()
        {
            try
            {
                var g = new Graph();
                g.LoadFromFile("resources\\InferenceTest.ttl");
                g.BaseUri = null;

                //Save Graph to ReadWriteSparql
                ReadWriteSparqlConnector readWrite = GetConnection();
                readWrite.SaveGraph(g);
                Console.WriteLine("Graph saved to ReadWriteSparql OK");

                //Now retrieve Graph from ReadWriteSparql
                var h = new Graph();
                readWrite.LoadGraph(h, (Uri)null);

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
            }
        }

        [SkippableFact]
        public void StorageReadWriteSparqlSaveDefaultGraph2()
        {
            try
            {
                var g = new Graph();
                g.LoadFromFile("resources\\InferenceTest.ttl");
                g.BaseUri = null;

                //Save Graph to ReadWriteSparql
                ReadWriteSparqlConnector readWrite = GetConnection();
                readWrite.SaveGraph(g);
                Console.WriteLine("Graph saved to ReadWriteSparql OK");

                //Now retrieve Graph from ReadWriteSparql
                var h = new Graph();
                readWrite.LoadGraph(h, (String)null);

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
            }
        }

        [SkippableFact]
        public void StorageReadWriteSparqlLoadGraph()
        {
            try
            {
                //Ensure that the Graph will be there using the SaveGraph() test
                StorageReadWriteSparqlSaveGraph();

                var g = new Graph();
                g.LoadFromFile("resources\\InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/readWriteTest");

                //Try to load the relevant Graph back from the Store
                ReadWriteSparqlConnector readWrite = GetConnection();

                var h = new Graph();
                readWrite.LoadGraph(h, "http://example.org/readWriteTest");

                Console.WriteLine();
                foreach (Triple t in h.Triples)
                {
                    Console.WriteLine(t.ToString(_formatter));
                }

                Assert.Equal(g, h);
            }
            finally
            {
            }
        }

        [SkippableFact]
        public void StorageReadWriteSparqlDeleteGraph()
        {
            try
            {
                StorageReadWriteSparqlSaveGraph();

                ReadWriteSparqlConnector readWrite = GetConnection();
                readWrite.DeleteGraph("http://example.org/readWriteTest");

                var g = new Graph();
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
            }
        }

        [SkippableFact]
        public void StorageReadWriteSparqlDeleteDefaultGraph()
        {
            try
            {
                StorageReadWriteSparqlSaveDefaultGraph();

                ReadWriteSparqlConnector readWrite = GetConnection();
                readWrite.DeleteGraph((Uri)null);

                var g = new Graph();
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
            }
        }

        [SkippableFact]
        public void StorageReadWriteSparqlDeleteDefaultGraph2()
        {
            try
            {
                StorageReadWriteSparqlSaveDefaultGraph();

                ReadWriteSparqlConnector readWrite = GetConnection();
                readWrite.DeleteGraph((String)null);

                var g = new Graph();
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
            }
        }

        [SkippableFact]
        public void StorageReadWriteSparqlAddTriples()
        {
            try
            {
                StorageReadWriteSparqlSaveGraph();

                var g = new Graph();
                var ts = new List<Triple>
                {
                    new Triple(g.CreateUriNode(new Uri("http://example.org/subject")),
                        g.CreateUriNode(new Uri("http://example.org/predicate")),
                        g.CreateUriNode(new Uri("http://example.org/object")))
                };

                ReadWriteSparqlConnector readWrite = GetConnection();
                readWrite.UpdateGraph("http://example.org/readWriteTest", ts, null);

                readWrite.LoadGraph(g, "http://example.org/readWriteTest");
                Assert.True(ts.All(t => g.ContainsTriple(t)), "Added Triple should have been in the Graph");
            }
            finally
            {
            }
        }

        [SkippableFact]
        public void StorageReadWriteSparqlRemoveTriples()
        {
            try
            {
                StorageReadWriteSparqlSaveGraph();

                var g = new Graph();
                var ts = new List<Triple>
                {
                    new Triple(g.CreateUriNode(new Uri("http://example.org/subject")),
                        g.CreateUriNode(new Uri("http://example.org/predicate")),
                        g.CreateUriNode(new Uri("http://example.org/object")))
                };

                ReadWriteSparqlConnector readWrite = GetConnection();
                readWrite.UpdateGraph("http://example.org/readWriteTest", null, ts);

                readWrite.LoadGraph(g, "http://example.org/readWriteTest");
                Assert.True(ts.All(t => !g.ContainsTriple(t)), "Removed Triple should not have been in the Graph");
            }
            finally
            {
            }
        }

        [SkippableFact]
        public void StorageReadWriteSparqlQuery()
        {
            ReadWriteSparqlConnector readWrite = GetConnection();

            var results = readWrite.Query("SELECT * WHERE { {?s ?p ?o} UNION { GRAPH ?g {?s ?p ?o} } }");
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
            Skip.IfNot(TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing),
                "Test Config marks Remote Parsing as unavailable, test cannot be run");

            ReadWriteSparqlConnector readWrite = GetConnection();

            //Try doing a SPARQL Update LOAD command
            var command = "LOAD <http://dbpedia.org/resource/Ilkeston> INTO GRAPH <http://example.org/Ilson>";
            readWrite.Update(command);

            //Then see if we can retrieve the newly loaded graph
            IGraph g = new Graph();
            readWrite.LoadGraph(g, "http://example.org/Ilson");
            Assert.False(g.IsEmpty, "Graph should be non-empty");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(_formatter));
            }
            Console.WriteLine();

            //Try a DROP Graph to see if that works
            command = "DROP GRAPH <http://example.org/Ilson>";
            readWrite.Update(command);

            g = new Graph();
            readWrite.LoadGraph(g, "http://example.org/Ilson");
            Assert.True(g.IsEmpty, "Graph should be empty as it should have been DROPped by ReadWriteSparql");
            
        }

        [SkippableFact]
        public void StorageReadWriteSparqlDescribe()
        {
            ReadWriteSparqlConnector readWrite = GetConnection();

            var results = readWrite.Query("DESCRIBE <http://example.org/vehicles/FordFiesta>");
            if (results is IGraph graph)
            {
                TestTools.ShowGraph(graph);
            }
            else
            {
                Assert.True(false, "Did not return a Graph as expected");
            }
        }

        [SkippableFact]
        public void StorageReadWriteSparqlConfigSerialization1()
        {
            ReadWriteSparqlConnector connector = GetConnection();
            var g = new Graph();
            INode n = g.CreateBlankNode();
            var context = new ConfigurationSerializationContext(g)
            {
                NextSubject = n
            };
            connector.SerializeConfiguration(context);

            TestTools.ShowGraph(g);

            var temp = ConfigurationLoader.LoadObject(g, n);
            Assert.IsType<ReadWriteSparqlConnector>(temp);
            var connector2 = (ReadWriteSparqlConnector)temp;
            Assert.True(EqualityHelper.AreUrisEqual(connector.Endpoint.Uri, connector2.Endpoint.Uri));
            Assert.True(EqualityHelper.AreUrisEqual(connector.UpdateEndpoint.Uri, connector2.UpdateEndpoint.Uri));
        }
    }
}
