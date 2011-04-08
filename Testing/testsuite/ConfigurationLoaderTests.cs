using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Writing;

namespace dotNetRDFTest
{
    public class ConfigurationLoaderTests
    {
        public static void Main(string[] args)
        {
            StreamWriter output = new StreamWriter("ConfigurationLoaderTests.txt");
            Console.SetOut(output);
            Console.WriteLine("## Configuration Loader Tests");
            Console.WriteLine();

            try
            {
                Console.WriteLine("Attempting to load our sample configuration graph");
                Graph g = new Graph();
                FileLoader.Load(g, "sample-config.ttl");
                Console.WriteLine("Sample graph loaded OK");
                Console.WriteLine();  
      
                //Test AppSettings resolution
                Console.WriteLine("# Testing <appSetting:Key> URI resolution");

                //Get the Actual Values
                String actualStr = ConfigurationManager.AppSettings["TestString"];
                bool actualTrue = Boolean.Parse(ConfigurationManager.AppSettings["TestTrue"]);
                bool actualFalse = Boolean.Parse(ConfigurationManager.AppSettings["TestFalse"]);
                int actualInt = Int32.Parse(ConfigurationManager.AppSettings["TestInt32"]);

                Console.WriteLine("Actual String: " + actualStr);
                Console.WriteLine("Actual True: " + actualTrue);
                Console.WriteLine("Actual False: " + actualFalse);
                Console.WriteLine("Actual Int: " + actualInt);

                //Now load the resolved values
                IUriNode objNode = g.GetUriNode(new Uri("dotnetrdf:test"));
                String resolvedStr = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, "dnr:stylesheet"));
                String resolvedStr2 = ConfigurationLoader.GetConfigurationValue(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, "dnr:stylesheet"));
                bool resolvedTrue = ConfigurationLoader.GetConfigurationBoolean(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, "dnr:cacheSliding"), false);
                bool resolvedFalse = ConfigurationLoader.GetConfigurationBoolean(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, "dnr:showErrors"), true);
                int resolvedInt = ConfigurationLoader.GetConfigurationInt32(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, "dnr:cacheDuration"), -1);

                Console.WriteLine("Resolved String: " + resolvedStr);
                Console.WriteLine("Resolved True: " + resolvedTrue);
                Console.WriteLine("Resolved False: " + resolvedFalse);
                Console.WriteLine("Resolved Int: " + resolvedInt);

                if (actualStr != resolvedStr) Console.WriteLine("AppSetting resolution failed");
                if (actualStr != resolvedStr2) Console.WriteLine("AppSetting resolution failed");
                if (resolvedStr != resolvedStr2) Console.WriteLine("AppSetting resolution failed");
                if (!resolvedTrue) Console.WriteLine("AppSetting resolution failed");
                if (actualTrue != resolvedTrue) Console.WriteLine("AppSetting resolution failed");
                if (resolvedFalse) Console.WriteLine("AppSetting resolution failed");
                if (actualFalse != resolvedFalse) Console.WriteLine("AppSetting resolution failed");
                if (actualInt != resolvedInt) Console.WriteLine("AppSetting resolution failed");

                //Attempt to load our MicrosoftSqlStoreManager from the Graph
                Console.WriteLine("# Attempting to load an object based on the Configuration Graph");
                INode testObj = g.GetUriNode(new Uri("dotnetrdf:sparqlDB"));
                if (testObj == null) throw new RdfException("Couldn't find the expected Test Object Node in the Graph");

                Object loaded = ConfigurationLoader.LoadObject(g, testObj);
                Console.WriteLine("Loaded an object, now need to check if it's of the type we expected...");
                if (loaded is ISqlIOManager)
                {
                    Console.WriteLine("It's an ISqlIOManager, is it for Microsoft SQL Server...");
                    if (loaded is MicrosoftSqlStoreManager)
                    {
                        Console.WriteLine("Success - Object loaded to correct type OK");
                    }
                    else
                    {
                        Console.WriteLine("Failure - Object loaded as " + loaded.GetType().ToString());
                    }
                }
                else
                {
                    Console.WriteLine("Failure - Object loaded as " + loaded.GetType().ToString());
                }
                Console.WriteLine();

                //Attempt to load it as a type for which there aren't any loaders defined
                ConfigurationLoader.ClearCache();
                Console.WriteLine("# Attempting to load the same object as a type for which there is no loader");
                try
                {
                    loaded = ConfigurationLoader.LoadObject(g, testObj, typeof(Triple));

                    Console.WriteLine("Failure - Loaded even though a bad type was provided");
                }
                catch (DotNetRdfConfigurationException configEx)
                {
                    Console.WriteLine("Success - Produced an error since there was no loader for the type provided:");
                    Console.WriteLine(configEx.Message);
                }
                Console.WriteLine();
                ConfigurationLoader.ClearCache();

                //Attempt to load a Graph from the Configuration
                Console.WriteLine("# Attempting to load a Graph which is defined in our configuration");
                testObj = g.GetBlankNode("a");
                ConfigurationLoaderTests.LoadGraphTest(g, testObj);

                Console.WriteLine("# Attempting to load another Graph which is defined in our configuration");
                testObj = g.GetBlankNode("b");
                ConfigurationLoaderTests.LoadGraphTest(g, testObj);

                Console.WriteLine("# Attempting to load another Graph whose contents are defined to be those of the first Graph");
                testObj = g.GetBlankNode("c");
                ConfigurationLoaderTests.LoadGraphTest(g, testObj);

                Console.WriteLine("# Attempting to load another Graph which makes an indirect circular reference to itself and thus an error should occur");
                testObj = g.GetBlankNode("d");
                try
                {
                    ConfigurationLoaderTests.LoadGraphTest(g, testObj);
                    Console.WriteLine("ERROR - Expected error did not occur");
                }
                catch (DotNetRdfConfigurationException ex)
                {
                    Console.WriteLine("Error occurred as expected: " + ex.Message);
                }

                Console.WriteLine("# Attempting to load a Graph which has a reasoner applied to it");
                testObj = g.GetBlankNode("f");
                ConfigurationLoaderTests.LoadGraphTest(g, testObj);

                Console.WriteLine("# Attempting to load a Graph which has an OWL reasoner applied to it");
                testObj = g.GetBlankNode("g");
                try
                {
                    ConfigurationLoaderTests.LoadGraphTest(g, testObj);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error occurred: " + ex.Message);
                    while (ex.InnerException != null)
                    {
                        Console.WriteLine(ex.InnerException.Message);
                        ex = ex.InnerException;
                    }
                }

                //Attempt to load a Triple Store from Configuration
                Console.WriteLine();
                Console.WriteLine("# Attempting to load a Triple Store from the Configuration");
                testObj = g.GetBlankNode("store1");
                ConfigurationLoaderTests.LoadStoreTest(g, testObj);

                Stopwatch t = new Stopwatch();
                t.Start();
                Console.WriteLine("# Attempting to load a SQL Triple Store from the Configuration");
                testObj = g.GetBlankNode("store2");
                ConfigurationLoaderTests.LoadStoreTest(g, testObj);
                t.Stop();
                Console.WriteLine("Took " + t.Elapsed + " time to load");
                t.Reset();
                if (ConfigurationLoader.IsCached(g, testObj))
                {
                    Console.WriteLine("Cached as expected, subsequent loads should be effectively instantaneous");
                    t.Start();
                    ConfigurationLoaderTests.LoadStoreTest(g, testObj);
                    t.Stop();
                    Console.WriteLine("Took " + t.Elapsed + " time to load");
                }

                Console.WriteLine("# Attempting to load a Native Triple Store form the Configuration");
                testObj = g.GetBlankNode("store3");
                ConfigurationLoaderTests.LoadStoreTest(g, testObj);

                //Attempt to load a SPARQL Endpoint which has a proxy server attached
                Console.WriteLine();
                Console.WriteLine("# Attempting to load a SPARQL Endpoint with user and Proxy Credentials");
                testObj = g.GetBlankNode("proxyTest");
                Object ep = ConfigurationLoader.LoadObject(g, testObj);
                if (ep is SparqlRemoteEndpoint)
                {
                    SparqlRemoteEndpoint endpoint = (SparqlRemoteEndpoint)ep;
                    Console.WriteLine("Loaded OK");
                    Console.WriteLine("URI: " + endpoint.Uri.ToString());
                    Console.WriteLine("Username: " + endpoint.Credentials.UserName);
                    Console.WriteLine("Password: " + endpoint.Credentials.Password);
                    Console.WriteLine("Use above Credentials for Proxy? " + endpoint.UseCredentialsForProxy.ToString());
                    Console.WriteLine("Proxy URI: " + endpoint.Proxy.Address);
                    Console.WriteLine("Proxy User: " + ((NetworkCredential)endpoint.Proxy.Credentials).UserName);
                    Console.WriteLine("Proxy Password: " + ((NetworkCredential)endpoint.Proxy.Credentials).Password);
                }
            }
            catch (DotNetRdfConfigurationException configEx)
            {
                reportError(output, "Configuration Error", configEx);
            }
            catch (RdfParseException parseEx)
            {
                reportError(output, "Parser Error", parseEx);
            }
            catch (RdfException rdfEx)
            {
                reportError(output, "RDF Error", rdfEx);
            }
            catch (Exception ex)
            {
                reportError(output, "Other Error", ex);
            }

            output.Close();
        }

        private static void LoadGraphTest(IGraph config, INode graphObj)
        {
            Object loaded = ConfigurationLoader.LoadObject(config, graphObj);
            if (loaded is IGraph)
            {
                Console.WriteLine("Success - Got an object which implements the IGraph interface");
                Console.WriteLine("Loaded type is " + loaded.GetType().ToString());
                Console.WriteLine();
                Console.WriteLine(VDS.RDF.Writing.StringWriter.Write((IGraph)loaded, new CompressingTurtleWriter()));
            }
            else
            {
                Console.WriteLine("Failure - Object loaded as " + loaded.GetType().ToString());
            }
            Console.WriteLine();
        }

        private static void LoadStoreTest(IGraph config, INode storeObj)
        {
            Object loaded = ConfigurationLoader.LoadObject(config, storeObj);
            if (loaded is ITripleStore)
            {
                Console.WriteLine("Success - Got an object which implements the ITripleStore interface");
                Console.WriteLine("Loaded type is " + loaded.GetType().ToString());
                if (loaded is IInMemoryQueryableStore)
                {
                    Console.WriteLine("This is an IInMemoryQueryableStore");
                }
                else if (loaded is INativelyQueryableStore)
                {
                    Console.WriteLine("This is an INativelyQueryableStore");
                }
                Console.WriteLine();
                ITripleStore store = (ITripleStore)loaded;
                Console.WriteLine(store.Graphs.Count + " Graphs in the Store");
                Console.WriteLine(store.Triples.Count() + " Triples in the Store");
                Console.WriteLine("Graph URIs are:");
                foreach (IGraph g in store.Graphs)
                {
                    if (g.BaseUri != null) Console.WriteLine(g.BaseUri.ToString());
                }
            }
            else
            {
                Console.WriteLine("Failure - Object loaded as " + loaded.GetType().ToString());
            }
            Console.WriteLine();
        }

        public static void reportError(StreamWriter output, String header, Exception ex)
        {
            output.WriteLine(header);
            output.WriteLine(ex.Message);
            output.WriteLine(ex.StackTrace);

            Exception innerEx = ex.InnerException;
            while (innerEx != null)
            {
                output.WriteLine();
                output.WriteLine(innerEx.Message);
                output.WriteLine(innerEx.StackTrace);
                innerEx = innerEx.InnerException;
            }
        }
    }
}
