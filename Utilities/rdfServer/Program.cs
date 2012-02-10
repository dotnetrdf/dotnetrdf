using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.Web;
using VDS.Web.Handlers;
using VDS.Web.Logging;

namespace rdfServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RdfServerOptions options = new RdfServerOptions(args);
                Hashtable hashtable = new Hashtable();

                switch (options.Mode)
                {
                    case RdfServerConsoleMode.Run:
                        using (HttpServer server = options.GetServerInstance())
                        {
                            if (!options.QuietMode) Console.WriteLine("rdfServer: Running");
                            server.Start();
                            while (true)
                            {
                                Thread.Sleep(1000);
                            }
                        }
                        break;

                    case RdfServerConsoleMode.Quit:
                        return;
                }

                if (!options.QuietMode)
                {
                    Console.WriteLine("rdfServer: Finished - press any key to exit");
                    Console.ReadKey();
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
