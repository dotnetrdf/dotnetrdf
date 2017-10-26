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
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;
using VDS.RDF.XunitExtensions;

namespace VDS.RDF.Web
{
    /// <summary>
    /// Summary description for ServiceDescription
    /// </summary>

    public class ServiceDescription
    {
        private void EnsureIIS()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseIIS))
            {
                throw new SkipTestException("Test Config marks IIS as unavailable, cannot run this test");
            }
        }

        [SkippableFact]
        public void ServiceDescriptionOptionsRequestOnSparqlServer()
        {
            EnsureIIS();
            String server = TestConfigManager.GetSetting(TestConfigManager.LocalGraphStoreUri);

            Console.WriteLine("Making an OPTIONS request to the web demos SPARQL Server at " + server);
            Console.WriteLine();

            NTriplesFormatter formatter = new NTriplesFormatter();

            HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(server);
            request.Method = "OPTIONS";
            request.Accept = MimeTypesHelper.HttpAcceptHeader;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                Graph g = new Graph();
                parser.Load(g, new StreamReader(response.GetResponseStream()));

                TestTools.ShowGraph(g);

                Assert.False(g.IsEmpty, "A non-empty Service Description Graph should have been returned");

                response.Close();
            }
        }

        [SkippableFact]
        public void ServiceDescriptionOptionsRequestOnSparqlServer2()
        {
            EnsureIIS();
            String path = TestConfigManager.GetSetting(TestConfigManager.LocalGraphStoreUri) + "some/long/path/elsewhere";

            Console.WriteLine("Making an OPTIONS request to the web demos SPARQL Server at " + path);
            Console.WriteLine("This Test tries to ensure that the URI resolution works correctly");
            Console.WriteLine();

            NTriplesFormatter formatter = new NTriplesFormatter();

            HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(path);
            request.Method = "OPTIONS";
            request.Accept = MimeTypesHelper.HttpAcceptHeader;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                Graph g = new Graph();
                parser.Load(g, new StreamReader(response.GetResponseStream()));

                TestTools.ShowGraph(g);

                Assert.False(g.IsEmpty, "A non-empty Service Description Graph should have been returned");

                response.Close();
            }
        }

        [SkippableFact]
        public void ServiceDescriptionDescriptionUriSparqlServer()
        {
            EnsureIIS();
            String path = TestConfigManager.GetSetting(TestConfigManager.LocalGraphStoreUri) + "description";

            Console.WriteLine("Making an request for the Service Description from the web demos SPARQL Server at " + path);
            Console.WriteLine();

            NTriplesFormatter formatter = new NTriplesFormatter();
            Graph g = new Graph();
            UriLoader.Load(g, new Uri(path));
            TestTools.ShowGraph(g);

            Assert.False(g.IsEmpty, "A non-empty Service Description Graph should have been returned");
        }

        [SkippableFact]
        public void ServiceDescriptionOptionsRequestOnQueryHandler()
        {
            EnsureIIS();
            String server = TestConfigManager.GetSetting(TestConfigManager.LocalQueryUri);

            Console.WriteLine("Making an OPTIONS request to the web demos Query Handler at " + server);
            Console.WriteLine();

            NTriplesFormatter formatter = new NTriplesFormatter();

            HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(server);
            request.Method = "OPTIONS";
            request.Accept = MimeTypesHelper.HttpAcceptHeader;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                Graph g = new Graph();
                parser.Load(g, new StreamReader(response.GetResponseStream()));

                TestTools.ShowGraph(g);

                Assert.False(g.IsEmpty, "A non-empty Service Description Graph should have been returned");

                response.Close();
            }
        }

        [SkippableFact]
        public void ServiceDescriptionOptionsRequestOnSparqlServer3()
        {
            EnsureIIS();
            String server = TestConfigManager.GetSetting(TestConfigManager.LocalGraphStoreQueryUri);

            Console.WriteLine("Making an OPTIONS request to the web demos SPARQL Server Query Endpoint at " + server);
            Console.WriteLine();

            NTriplesFormatter formatter = new NTriplesFormatter();

            HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(server);
            request.Method = "OPTIONS";
            request.Accept = MimeTypesHelper.HttpAcceptHeader;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                Graph g = new Graph();
                parser.Load(g, new StreamReader(response.GetResponseStream()));

                TestTools.ShowGraph(g);

                Assert.False(g.IsEmpty, "A non-empty Service Description Graph should have been returned");

                response.Close();
            }
        }

        [SkippableFact]
        public void ServiceDescriptionOptionsRequestOnSparqlServer4()
        {
            EnsureIIS();
            String server = TestConfigManager.GetSetting(TestConfigManager.LocalGraphStoreUpdateUri);

            Console.WriteLine("Making an OPTIONS request to the web demos SPARQL Server Update Endpoint at " + server);
            Console.WriteLine();

            NTriplesFormatter formatter = new NTriplesFormatter();

            HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(server);
            request.Method = "OPTIONS";
            request.Accept = MimeTypesHelper.HttpAcceptHeader;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                Graph g = new Graph();
                parser.Load(g, new StreamReader(response.GetResponseStream()));

                TestTools.ShowGraph(g);

                Assert.False(g.IsEmpty, "A non-empty Service Description Graph should have been returned");

                response.Close();
            }
        }
    }

}
