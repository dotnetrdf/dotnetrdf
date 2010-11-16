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
            RdfServerOptions options = new RdfServerOptions(args);
            //RdfServerServiceInstaller installer = null;
            Hashtable hashtable = new Hashtable();

            switch (options.Mode)
            {
                case RdfServerConsoleMode.Run:
                    try
                    {
                        using (HttpServer server = options.GetServerInstance())
                        {
                            if (!options.QuietMode) Console.WriteLine("rdfServer: Running");
                            server.Start();
                            while (true)
                            {
                                Thread.Sleep(1000);
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
                    break;

                /*case RdfServerConsoleMode.RunService:
                    RdfServerService service = new RdfServerService(options.ServiceName);
                    service.StartupOptions = options;
                    ServiceBase.Run(service);
                    break;*/

                case RdfServerConsoleMode.Quit:
                    return;

                /*case RdfServerConsoleMode.InstallService:
                    try
                    {
                        //Find Location of the rdfServer executable
                        String location = Environment.CurrentDirectory;
                        if (!location.EndsWith(new String(new char[] { Path.DirectorySeparatorChar }))) location += Path.DirectorySeparatorChar;
                        location = Path.Combine(location, "rdfServer.exe");

                        //Start creating install options
                        List<String> installOps = new List<string>();
                        if (options.BaseDirectory != null)
                        {
                            installOps.Add("-base");
                            installOps.Add("\"" + options.BaseDirectory.Replace("\\", "\\\\") + "\"");
                        }
                        installOps.Add("-config");
                        installOps.Add(options.ConfigurationFile);

                        //Add host and port
                        installOps.Add("-host");
                        installOps.Add(options.Host);
                        installOps.Add("-port");
                        installOps.Add(options.Port.ToString());

                        //Add logging options
                        if (options.LogFile != null)
                        {
                            installOps.Add("-log");
                            installOps.Add(options.LogFile);
                        }
                        installOps.Add("-format");
                        installOps.Add("\"" + options.LogFormat.Replace("\"", "\\\"") + "\"");

                        //Add Verbose option
                        if (options.VerboseMode)
                        {
                            installOps.Add("-verbose");
                        }

                        //Set to run as service
                        installOps.Add("-service");
                        installOps.Add("run");
                        installOps.Add(options.ServiceName);

                        installer = new RdfServerServiceInstaller(options.ServiceName);
                        installer.Context = new InstallContext(null, new String[] { "/LogToConsole=true" });
                        installer.Context.Parameters["assemblypath"] = location + "\" " + String.Join(" ", installOps.ToArray());

                        installer.Install(hashtable);
                        installer.Commit(hashtable);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            if (installer != null) installer.Rollback(hashtable);
                        }
                        catch
                        {
                            Console.Error.WriteLine("rdfServer: Error: Unable to rollback");
                        }
                        Console.Error.WriteLine("rdfServer: Error: Unable to uninstall the service " + options.ServiceName);
                        Console.Error.WriteLine(ex.Message);
                        Console.Error.WriteLine(ex.StackTrace);
                    }
                    break;
                    
                case RdfServerConsoleMode.UninstallService:
                    Console.Error.WriteLine("rdfServer: Error: Service uninstallation is not yet supported");

                    //try
                    //{
                    //    //Find Location of the rdfServer executable
                    //    String location = Environment.CurrentDirectory;
                    //    if (!location.EndsWith(new String(new char[] { Path.DirectorySeparatorChar }))) location += Path.DirectorySeparatorChar;
                    //    location = Path.Combine(location, "rdfServer.exe");

                    //    installer = new RdfServerServiceInstaller(options.ServiceName);
                    //    installer.Context = new InstallContext(null, new String[] { "/LogToConsole=true" });
                    //    installer.Context.Parameters["assemblypath"] = location;
                    //    installer.Uninstall(hashtable);
                    //}
                    //catch (Exception ex)
                    //{
                    //    Console.Error.WriteLine("rdfServer: Error: Unable to uninstall the service " + options.ServiceName);
                    //    Console.Error.WriteLine(ex.Message);
                    //    Console.Error.WriteLine(ex.StackTrace);
                    //}
                    break;

                case RdfServerConsoleMode.StartService:
                    try
                    {
                        ServiceController controller = new ServiceController(options.ServiceName);
                        controller.Start();
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("rdfServer: Error: Unable to start the service " + options.ServiceName);
                        Console.Error.WriteLine(ex.Message);
                        Console.Error.WriteLine(ex.StackTrace);
                    }
                    break;

                case RdfServerConsoleMode.StopService:
                    try
                    {
                        ServiceController controller = new ServiceController(options.ServiceName);
                        controller.Stop();
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("rdfServer: Error: Unable to start the service " + options.ServiceName);
                        Console.Error.WriteLine(ex.Message);
                        Console.Error.WriteLine(ex.StackTrace);
                    }
                    break;

                case RdfServerConsoleMode.RestartService:
                    try
                    {
                        ServiceController controller = new ServiceController(options.ServiceName);
                        controller.Stop();
                        controller.Start();
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("rdfServer: Error: Unable to start the service " + options.ServiceName);
                        Console.Error.WriteLine(ex.Message);
                        Console.Error.WriteLine(ex.StackTrace);
                    }
                    break;*/
            }

            //if (options.Mode != RdfServerConsoleMode.RunService)
            //{
            if (!options.QuietMode)
            {
                Console.WriteLine("rdfServer: Finished - press any key to exit");
                Console.ReadKey();
            }
            //}
        }
    }
}
