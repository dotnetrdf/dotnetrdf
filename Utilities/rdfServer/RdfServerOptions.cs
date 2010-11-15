using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.Web;
using VDS.Web.Handlers;
using VDS.Web.Logging;

namespace rdfServer
{
    public enum RdfServerConsoleMode
    {
        Quit,
        Run,
        InstallService,
        UninstallService,
        RunService,
        StartService,
        StopService,
        RestartService
    }

    public class RdfServerOptions
    {
        private RdfServerConsoleMode _mode = RdfServerConsoleMode.Run;

        private int _port = 1986;
        private String _host = HttpServer.DefaultHost;
        private String _configFile = "default.ttl";
        private String _logFile = null;
        private String _logFormat = ApacheStyleLogger.LogCommon;
        private bool _verbose = false;
        private String _baseDir = null;
        private String _serviceName = null;

        public const String DefaultServiceName = "rdfServerService";

        public RdfServerOptions(String[] args)
        {
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-help":
                            this.ShowUsage();
                            this._mode = RdfServerConsoleMode.Quit;
                            break;

                        case "-p":
                        case "-port":
                            if (i < args.Length - 1)
                            {
                                i++;
                                int port;
                                if (Int32.TryParse(args[i], out port))
                                {
                                    this._port = port;
                                    Console.WriteLine("rdfServer: Port Set to " + this._port);
                                }
                                else
                                {
                                    Console.Error.WriteLine("rdfServer: Error: Expected a valid port number after the -p/-port option - will use default port 1986");
                                }
                            }
                            else
                            {
                                Console.Error.WriteLine("rdfServer: Error: Expected an argument after the -p/-port option to specify the port number - will use default port 1986");
                            }
                            break;

                        case "-h":
                        case "-host":
                            if (i < args.Length - 1)
                            {
                                i++;
                                this._host = args[i];
                                Console.WriteLine("rdfServer: Host set to " + this._host);
                            }
                            else
                            {
                                Console.Error.WriteLine("rdfServer: Error: Expected an argument after the -h/-host option to specify the host name - will use the default host name localhost");
                            }
                            break;

                        case "-c":
                        case "-config":
                            if (i < args.Length - 1)
                            {
                                i++;
                                this._configFile = args[i];
                                Console.WriteLine("rdfServer: Configuration File set to " + this._configFile);
                            }
                            else
                            {
                                Console.Error.WriteLine("rdfServer: Error: Expected an argument after the -c/-config option to specify the configuration file - will use the default file default.ttl");
                            }
                            break;

                        case "-v":
                        case "-verbose":
                            Console.WriteLine("rdfServer: Verbose Mode on - all requests and errors will be logged to the Console in the specified Log Format");
                            this._verbose = true;
                            break;

                        case "-l":
                        case "-log":
                            if (i < args.Length - 1)
                            {
                                i++;
                                this._logFile = args[i];
                                Console.WriteLine("rdfServer: Log File set to " + this._logFile);
                            }
                            else
                            {
                                Console.Error.WriteLine("rdfServer: Error: Expected an argument after the -l/-log option to specify the log file - no log file will be used");
                            }
                            break;

                        case "-f":
                        case "-format":
                            if (i < args.Length - 1)
                            {
                                i++;
                                this._logFormat = args[i];
                                Console.WriteLine("rdfServer: Log Format set to " + this._logFormat);
                                this._logFormat = ApacheStyleLogger.GetLogFormat(this._logFormat);
                            }
                            else
                            {
                                Console.Error.WriteLine("rdfServer: Error: Expected an argument after the -f/-format option to specify the log format string - Common Log Format will be used");
                            }
                            break;

                        case "-b":
                        case "-base":
                            if (i < args.Length - 1)
                            {
                                i++;
                                this._baseDir = args[i];
                                if (this._baseDir.Equals(".")) this._baseDir = Environment.CurrentDirectory;
                                if (!this._baseDir.EndsWith(new String(new char[] { Path.DirectorySeparatorChar }))) this._baseDir += Path.DirectorySeparatorChar;
                                if (Directory.Exists(args[i]))
                                {                                  
                                    Console.WriteLine("rdfServer: Running with HTTP Server enabled - static files (HTML, Plain Text, Images etc) will be served from the base directory " + this._baseDir);
                                }
                                else
                                {
                                    this._baseDir = null;
                                    Console.Error.WriteLine("rdfServer: Error: The Base Directory specified for the -b/-base option does not exist");
                                }
                            }
                            else
                            {
                                Console.Error.WriteLine("rdfServer: Error: Expected an argument after the -b/-base option to specify the base directory from which to serve static content");
                            }
                            break;

                        case "-s":
                        case "-service":
                            if (i < args.Length - 1)
                            {
                                i++;
                                String op = args[i];
                                String svcName = DefaultServiceName;
                                if (i < args.Length - 1)
                                {
                                    if (!args[i + 1].StartsWith("-"))
                                    {
                                        i++;
                                        svcName = args[i];
                                    }
                                }
                                this._serviceName = svcName;

                                switch (op)
                                {
                                    case "install":
                                        this._mode = RdfServerConsoleMode.InstallService;
                                        break;
                                    case "uninstall":
                                        this._mode = RdfServerConsoleMode.UninstallService;
                                        break;
                                    case "start":
                                        this._mode = RdfServerConsoleMode.StartService;
                                        break;
                                    case "stop":
                                        this._mode = RdfServerConsoleMode.StopService;
                                        break;
                                    case "restart":
                                        this._mode = RdfServerConsoleMode.RestartService;
                                        break;
                                    case "run":
                                        this._mode = RdfServerConsoleMode.RunService;
                                        break;
                                    default:
                                        Console.Error.WriteLine("rdfServer: Error: Operation '" + op + "' is not a valid operation argument to be specified after the -s/-service operation");
                                        this._mode = RdfServerConsoleMode.Quit;
                                        break;
                                }
                            }
                            else
                            {
                                Console.Error.WriteLine("rdfServer: Error: Expected an argument after the -s/-service option to specify a service operation to perform");
                                this._mode = RdfServerConsoleMode.Quit;
                            }
                            break;

                        default:
                            Console.Error.WriteLine("rdfServer: Error: Unknown option/argument '" + args[i] + "' was ignored");
                            break;
                    }
                }
            }
        }

        public void ShowUsage()
        {
            Console.WriteLine("rdfServer");
            Console.WriteLine("=========");
            Console.WriteLine();
            Console.WriteLine("rdfServer is a HTTP Server which provides SPARQL Query and Update over user configurable data using the dotNetRDF Configuration API");
            Console.WriteLine();
            Console.WriteLine("Usage is:");
            Console.WriteLine("rdfServer [options]");
            Console.WriteLine();
            Console.WriteLine("Options");
            Console.WriteLine("-------");
            Console.WriteLine();
            Console.WriteLine("-b directory");
            Console.WriteLine("-base directory");
            Console.WriteLine(" Sets the Base Directory from which static content can be served");
            Console.WriteLine();
            Console.WriteLine("-c config.ttl");
            Console.WriteLine("-config config.ttl");
            Console.WriteLine(" Sets the Configuration File which specifies the Dataset to use for querying (see default.ttl for an example or the online documentation for the dotNetRDF Configuration API)");
            Console.WriteLine();
            Console.WriteLine("-f format");
            Console.WriteLine("-format format");
            Console.WriteLine(" Sets the Log Format for use with logging, format string is in Apache mod_log style");
            Console.WriteLine();
            Console.WriteLine("-h host");
            Console.WriteLine("-host host");
            Console.WriteLine(" Sets the host name that the server listens on");
            Console.WriteLine();
            Console.WriteLine("-l log.txt");
            Console.WriteLine("-log log.txt");
            Console.WriteLine(" Sets the log file used for logging");
            Console.WriteLine();
            Console.WriteLine("-p port");
            Console.WriteLine("-port port");
            Console.WriteLine(" Sets the port that the server listens on");
            Console.WriteLine();
            Console.WriteLine("-s operation [servicename]");
            Console.WriteLine("-service operation [servicename]");
            Console.WriteLine(" Performs a Windows Service related operation.");
            Console.WriteLine(" Supported options are:");
            Console.WriteLine("     install     Installs the Service");
            Console.WriteLine("     uninstall   Uninstalls the Service");
            Console.WriteLine("     start       Starts the Service");
            Console.WriteLine("     restart     Restarts the Service");
            Console.WriteLine("     stop        Stops the Service");
            Console.WriteLine(" Service Name is optional and if not specified will be rdfServerService");
            Console.WriteLine();
            Console.WriteLine("-v");
            Console.WriteLine("-verbose");
            Console.WriteLine(" Sets Verbose mode - causes all requests and errors to be logged to console");
        }

        public HttpServer GetServerInstance()
        {
            IHttpListenerHandlerCollection handlers = new SparqlHandlersCollection(this);
            HttpServer server = new HttpServer(this.Host, this.Port, handlers);
            server.BaseDirectory = this.BaseDirectory;

            //Need to load up the Configuration Graph and add to Server State
            Graph g = new Graph();
            try
            {
                FileLoader.Load(g, this.ConfigurationFile);
                server.State["ConfigurationGraph"] = g;

                //Setup Logging appropriately
                if (this.LogFile != null)
                {
                    server.AddLogger(new FileLogger(this.LogFile, this.LogFormat));
                }
                if (this.VerboseMode)
                {
                    server.AddLogger(new ConsoleLogger(this.LogFormat));
                }
                else
                {
                    server.AddLogger(new ConsoleErrorLogger());
                }
                server.AddLogger(new EventLogger(this.ServiceName));
            }
            catch (FileNotFoundException)
            {
                Console.Error.WriteLine("rdfServer: Error: Configuration File '" + this.ConfigurationFile + "' was not found");
            }

            return server;
        }

        public RdfServerConsoleMode Mode
        {
            get
            {
                return this._mode;
            }
        }

        public String ServiceName
        {
            get
            {
                return this._serviceName;
            }
        }

        public int Port
        {
            get
            {
                return this._port;
            }
        }

        public String Host
        {
            get
            {
                return this._host;
            }
        }

        public String ConfigurationFile
        {
            get
            {
                return this._configFile;
            }
        }

        public String LogFile
        {
            get
            {
                return this._logFile;
            }
        }

        public String LogFormat
        {
            get
            {
                return this._logFormat;
            }
        }

        public bool VerboseMode
        {
            get
            {
                return this._verbose;
            }
        }

        public String BaseDirectory
        {
            get 
            {
                return this._baseDir;
            }
        }
    }
}
