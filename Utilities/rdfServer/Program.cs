using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.Web;

namespace rdfServer
{
    class Program
    {
        static void Main(string[] args)
        {
            String configFile;
            if (args.Length == 0)
            {
                configFile = "default.ttl";
            } 
            else
            {
                configFile = args[0];
            }

            HttpListenerHandlerCollection handlers = new SparqlHandlersCollection();
            using (HttpServer server = new HttpServer(1986, handlers))
            {
                //Need to load up the Configuration Graph and add to Server State
                Graph g = new Graph();
                try
                {
                    FileLoader.Load(g, configFile);
                    server.State["ConfigurationGraph"] = g;

                    Console.WriteLine("rdfServer: Running");
                    while (true)
                    {
                        server.Start();
                    }
                }
                catch (FileNotFoundException)
                {
                    Console.Error.WriteLine("rdfServer: Error: Configuration File '" + configFile + "' was not found");
                }
            }
        }
    }
}
