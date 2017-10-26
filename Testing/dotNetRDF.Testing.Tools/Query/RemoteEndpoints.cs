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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;
using VDS.RDF.XunitExtensions;

namespace VDS.RDF.Query
{
    public partial class RemoteEndpoints
    {
        const int AsyncTimeout = 45000;

        public static SparqlRemoteEndpoint GetQueryEndpoint()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseIIS))
            {
                throw new SkipTestException("Test Config marks IIS as unavailable, cannot run test");
            }
            return new SparqlRemoteEndpoint(new Uri(TestConfigManager.GetSetting(TestConfigManager.LocalGraphStoreQueryUri)));
        }

        public static SparqlRemoteUpdateEndpoint GetUpdateEndpoint()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseIIS))
            {
                throw new SkipTestException("Test Config marks IIS as unavailable, cannot run test");
            }
            return new SparqlRemoteUpdateEndpoint(new Uri(TestConfigManager.GetSetting(TestConfigManager.LocalGraphStoreUpdateUri)));
        }

        [SkippableFact]
        public void SparqlRemoteEndpointLongQuery()
        {
            try
            {
                Options.HttpDebugging = true;

                StringBuilder input = new StringBuilder();
                input.AppendLine("SELECT * WHERE {?s ?p ?o}");
                input.AppendLine(new String('#', 2048));

                SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
                Object results = endpoint.QueryWithResultSet(input.ToString());
                Assert.IsAssignableFrom<SparqlResultSet>(results);
                if (results is SparqlResultSet)
                {
                    TestTools.ShowResults(results);
                }
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        [SkippableFact]
        public void SparqlRemoteEndpointLongUpdate()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing))
            {
                throw new SkipTestException("Test Config marks Remote Parsing as unavailable, test cannot be run");
            }

            try
            {
                Options.HttpDebugging = true;

                StringBuilder input = new StringBuilder();
                input.AppendLine("LOAD <http://dbpedia.org/resource/Ilkeston>");
                input.AppendLine(new String('#', 2048));

                SparqlRemoteUpdateEndpoint endpoint = RemoteEndpoints.GetUpdateEndpoint();
                endpoint.Update(input.ToString());
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        [SkippableFact]
        public void SparqlRemoteEndpointCountHandler()
        {
            SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
            CountHandler handler = new CountHandler();
            endpoint.QueryWithResultGraph(handler, "CONSTRUCT { ?s ?p ?o } WHERE { { ?s ?p ?o } UNION { GRAPH ?g { ?s ?p ?o } } }");

            Console.WriteLine("Triple Count: " + handler.Count);
            Assert.NotEqual(0, handler.Count);
        }

        [SkippableFact]
        public void SparqlRemoteEndpointResultCountHandler()
        {
            SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
            ResultCountHandler handler = new ResultCountHandler();
            endpoint.QueryWithResultSet(handler, "SELECT * WHERE { { ?s ?p ?o } UNION { GRAPH ?g { ?s ?p ?o } } }");

            Console.WriteLine("Result Count: " + handler.Count);
            Assert.NotEqual(0, handler.Count);
        }

        [SkippableFact]
        public void SparqlRemoteEndpointMemoryLeak1()
        {
            /*
            Dim endpoint = New SparqlRemoteEndpoint(New Uri("http://localhost:8080/sesame/repositories/my_repo"))
Dim queryString As SparqlParameterizedString = New SparqlParameterizedString()
queryString.Namespaces.AddNamespace("annot", New Uri(oAppSettingsReader.GetValue("BaseUriSite", GetType(System.String)) & "/annotations.owl#"))
queryString.CommandText = "SELECT DISTINCT ?text WHERE {?annotation annot:onContent <" & _uriDocument & "> ; annot:onContentPart """ & ContentPart & """ ; annot:text ?text ; annot:isValid ""false""^^xsd:boolean . }"
Dim results As SparqlResultSet = endpoint.QueryWithResultSet(queryString.ToString)
For Each result As SparqlResult In results
    Console.WriteLine(DirectCast(result.Value("text"), LiteralNode).Value)
Next
results.Dispose()
             */

            //Do a GC before attempting the test
            GC.GetTotalMemory(true);

            //First off make sure to load some data into the some
            SparqlRemoteUpdateEndpoint updateEndpoint = RemoteEndpoints.GetUpdateEndpoint();
            updateEndpoint.Update("DROP ALL; INSERT DATA { <http://subject> <http://predicate> <http://object> . }");

            int totalRuns = 10000;

            //Loop over making queries to try and reproduce the memory leak
            for (int i = 1; i <= totalRuns; i++)
            {
                SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
                SparqlParameterizedString queryString = new SparqlParameterizedString();
                queryString.CommandText = "SELECT * WHERE { ?s ?p ?o }";

                SparqlResultSet results = endpoint.QueryWithResultSet(queryString.ToString());
                Assert.Equal(1, results.Count);
                foreach (SparqlResult result in results)
                {
                    //We're just iterating to make sure we touch the whole of the results
                }
                results.Dispose();

                if (i % 500 == 0)
                {
                    Debug.WriteLine("Memory Usage after " + i + " Iterations: " + Process.GetCurrentProcess().PrivateMemorySize64);
                }
            }
            Debug.WriteLine("Memory Usage after " + totalRuns + " Iterations: " + Process.GetCurrentProcess().PrivateMemorySize64);
        }

        [SkippableFact]
        public void SparqlRemoteEndpointMemoryLeak2()
        {
            //Do a GC before attempting the test
            GC.GetTotalMemory(true);

            //First off make sure to load some data into the some
            SparqlRemoteUpdateEndpoint updateEndpoint = RemoteEndpoints.GetUpdateEndpoint();
            updateEndpoint.Update("DROP ALL");

            int totalRuns = 10000;
            int subjects = 1000;
            int predicates = 10;

            //Loop over making queries to try and reproduce the memory leak
            for (int i = 1; i <= totalRuns; i++)
            {
                //Add new data each time around
                updateEndpoint.Update("INSERT DATA { <http://subject/" + (i % subjects) + "> <http://predicate/" + (i % predicates) + "> <http://object/" + i + "> . }");

                SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
                SparqlParameterizedString queryString = new SparqlParameterizedString();
                queryString.CommandText = "SELECT * WHERE { <http://subject/" + (i % 1000) + "> ?p ?o }";

                ResultCountHandler handler = new ResultCountHandler();
                endpoint.QueryWithResultSet(handler, queryString.ToString());
                Assert.True(handler.Count >= 1 && handler.Count <= subjects, "Result Count " + handler.Count + " is not in expected range 1 <= x < " + (i % 1000));

                if (i % 500 == 0)
                {
                    Debug.WriteLine("Memory Usage after " + i + " Iterations: " + Process.GetCurrentProcess().PrivateMemorySize64);
                }
            }
            Debug.WriteLine("Memory Usage after " + totalRuns + " Iterations: " + Process.GetCurrentProcess().PrivateMemorySize64);
        }

        [SkippableFact]
        public void SparqlRemoteEndpointWriteThroughHandler()
        {
            SparqlRemoteEndpoint endpoint = RemoteEndpoints.GetQueryEndpoint();
            WriteThroughHandler handler = new WriteThroughHandler(typeof(NTriplesFormatter), Console.Out, false);
            endpoint.QueryWithResultGraph(handler, "CONSTRUCT WHERE { ?s ?p ?o }");
        }
    }
}
