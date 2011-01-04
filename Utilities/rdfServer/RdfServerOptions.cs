using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        Run
    }

    public class RdfServerOptions
    {
        private RdfServerConsoleMode _mode = RdfServerConsoleMode.Run;

        private int _port = DefaultPort;
        private String _host = HttpServer.DefaultHost;
        private String _configFile = "default.ttl";
        private String _logFile = null;
        private String _logFormat = ApacheStyleLogger.LogCommon;
        private bool _verbose = false, _quiet = false;
        private String _baseDir = null;
        private bool _restControl = false;

        public const String DefaultServiceName = "rdfServerService";
        public const int DefaultPort = 1986;

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
                                    if (!this._quiet) Console.WriteLine("rdfServer: Port Set to " + this._port);
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
                                if (!this._quiet) Console.WriteLine("rdfServer: Host set to " + this._host);
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
                                if (!this._quiet) Console.WriteLine("rdfServer: Configuration File set to " + this._configFile);
                            }
                            else
                            {
                                Console.Error.WriteLine("rdfServer: Error: Expected an argument after the -c/-config option to specify the configuration file - will use the default file default.ttl");
                            }
                            break;

                        case "-v":
                        case "-verbose":
                            if (!this._quiet) Console.WriteLine("rdfServer: Verbose Mode on - all requests and errors will be logged to the Console in the specified Log Format");
                            this._verbose = true;
                            break;

                        case "-q":
                        case "-quiet":
                            this._quiet = true;
                            break;

                        case "-l":
                        case "-log":
                            if (i < args.Length - 1)
                            {
                                i++;
                                this._logFile = args[i];
                                if (!this._quiet) Console.WriteLine("rdfServer: Log File set to " + this._logFile);
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
                                if (!this._quiet) Console.WriteLine("rdfServer: Log Format set to " + this._logFormat);
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
                                    if (!this._quiet) Console.WriteLine("rdfServer: Running with HTTP Server enabled - static files (HTML, Plain Text, Images etc) will be served from the base directory " + this._baseDir);
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

                        case "-r":
                        case "-rest":
                            this._restControl = true;
                            if (!this._quiet) Console.WriteLine("rdfServer: RESTful Control enabled, POST a request to /control with a querystring of operation=restart or operation=stop to control the server");
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
            Console.WriteLine("-help");
            Console.WriteLine(" Prints this usage summary and quits");
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
            Console.WriteLine("-q");
            Console.WriteLine("-quiet");
            Console.WriteLine(" Suppresses all information messages and ignores verbose mode if set");
            Console.WriteLine();
            Console.WriteLine("-r");
            Console.WriteLine("-rest");
            Console.WriteLine(" Enabled RESTful Control which allows a request to be POSTed to /control with operation=restart or operation=stop to control the server");
            Console.WriteLine();
            Console.WriteLine("-v");
            Console.WriteLine("-verbose");
            Console.WriteLine(" Sets Verbose mode - causes all requests and errors to be logged to console");
        }

        public HttpServer GetServerInstance()
        {
            IHttpListenerHandlerCollection handlers = new SparqlHandlersCollection(this);
            if (this.RestControl) handlers.InsertMapping(new HttpRequestMapping("POST", "/control", typeof(RestControlHandler)), 0);

            HttpServer server = new HttpServer(this.Host, this.Port, handlers);
            server.BaseDirectory = this.BaseDirectory;

            ConfigurationLoader.PathResolver = new RdfServerPathResolver(server);

            //Need to load up the Configuration Graph and add to Server State
            server.State["ConfigurationGraph"] = this.LoadConfigurationGraph();
            if (server.State["ConfigurationGraph"] == null)
            {
                throw new HttpServerException("Unable to create a HttpServer instance as the Configuration Graph for the server could not be loaded");
            }

            //Add MIME Type Mappings for RDF File Types
            foreach (MimeTypeDefinition definition in MimeTypesHelper.Definitions)
            {
                server.AddMimeType(definition.CanonicalFileExtension, definition.CanonicalMimeType);
            }

            //Setup Logging appropriately
            if (this.LogFile != null)
            {
                server.AddLogger(new FileLogger(this.LogFile, this.LogFormat));
            }
            if (this.Mode == RdfServerConsoleMode.Run)
            {
                if (!this.QuietMode)
                {
                    //Console Logging only applies when not in Quiet Mode
                    if (this.VerboseMode)
                    {
                        server.AddLogger(new ConsoleLogger(this.LogFormat));
                    }
                    else
                    {
                        server.AddLogger(new ConsoleErrorLogger());
                    }
                }
            }


            return server;
        }

        private IGraph LoadConfigurationGraph()
        {
            IGraph g = null;

            try
            {
                if (File.Exists(this._configFile))
                {
                    g = new Graph();
                    FileLoader.Load(g, this._configFile);
                }
                else
                {
                    //Try to get from embedded resource instead
                    Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(this._configFile);
                    if (stream == null)
                    {
                        Console.Error.WriteLine("rdfServer: Error: Configuration File '" + this.ConfigurationFile + "' was not found");
                    }
                    else
                    {
                        IRdfReader reader = MimeTypesHelper.GetParser(MimeTypesHelper.GetMimeType(Path.GetExtension(this._configFile)));
                        g = new Graph();
                        reader.Load(g, new StreamReader(stream));
                    }
                }
            }
            catch (RdfParserSelectionException selectEx)
            {
                g = null;
                Console.Error.WriteLine("rdfServer: Error: Configuration File '" + this.ConfigurationFile + "' could not be loaded as a suitable parser was not found");
            }
            catch (RdfParseException parseEx)
            {
                g = null;
                Console.Error.WriteLine("rdfServer: Error: Configuration File '" + this.ConfigurationFile + "' was not valid RDF");
            }

            //If got a Graph OK then prep the dotNetRDF Configuration API
            if (g != null)
            {
                ConfigurationLoader.AutoDetectObjectFactories(g);
            }
            return g;
        }

        public RdfServerConsoleMode Mode
        {
            get
            {
                return this._mode;
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

        public bool QuietMode
        {
            get
            {
                return this._quiet;
            }
        }

        public bool RestControl
        {
            get
            {
                return this._restControl;
            }
        }

        public String BaseDirectory
        {
            get 
            {
                return this._baseDir;
            }
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (this.BaseDirectory != null)
            {
                output.Append("-base \"" + this.BaseDirectory.Replace("\\", "\\\\") + "\"");
                output.Append(' ');
            }
            output.Append("-config " + this.ConfigurationFile);
            output.Append(' ');
            output.Append("-format \"" + this.LogFormat.Replace("\"", "\\\"") + "\"");
            output.Append(' ');
            output.Append("-host " + this.Host);
            output.Append(' ');
            if (this.LogFile != null)
            {
                output.Append("-log " + this.LogFile);
                output.Append(' ');
            }
            output.Append("-port " + this.Port);

            if (this.Mode != RdfServerConsoleMode.Quit)
            {
                output.Append(' ');
                switch (this.Mode)
                {
                    case RdfServerConsoleMode.Run:
                        if (this.VerboseMode)
                        {
                            output.Append("-verbose");
                        }
                        break;
                }
            }

            return output.ToString();
        }
    }
}
