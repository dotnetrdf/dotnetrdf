using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.Web;
using VDS.Web.Logging;

namespace rdfServer
{
    class Program
    {
        static void Main(string[] args)
        {
            RdfServerOptions options = new RdfServerOptions(args);

            HttpListenerHandlerCollection handlers = new SparqlHandlersCollection(options);
            try
            {
                using (HttpServer server = new HttpServer(options.Host, options.Port, handlers))
                {
                    //Need to load up the Configuration Graph and add to Server State
                    Graph g = new Graph();
                    try
                    {
                        FileLoader.Load(g, options.ConfigurationFile);
                        server.State["ConfigurationGraph"] = g;

                        //Setup Logging appropriately
                        if (options.LogFile != null)
                        {
                            server.AddLogger(new FileLogger(options.LogFile, options.LogFormat));
                        }
                        if (options.VerboseMode)
                        {
                            server.AddLogger(new ConsoleLogger(options.LogFormat));
                        }
                        else
                        {
                            server.AddLogger(new ConsoleErrorLogger());
                        }

                        Console.WriteLine("rdfServer: Running");
                        while (true)
                        {
                            server.Start();
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        Console.Error.WriteLine("rdfServer: Error: Configuration File '" + options.ConfigurationFile + "' was not found");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("rdfServer: Error: An unexpected error occurred while trying to start up the Server.  See subsequent error messages for details:");
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                while (ex.InnerException != null)
                {
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("Inner Exception:");
                    Console.Error.WriteLine(ex.InnerException.Message);
                    Console.Error.WriteLine(ex.InnerException.StackTrace);
                    ex = ex.InnerException;
                }
            }
        }
    }
}
