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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.Web;
using VDS.Web.Consoles;
using VDS.Web.Handlers;
using VDS.Web.Logging;

namespace VDS.RDF.Utilities.Server
{
    /// <summary>
    /// Possible modes for the server when run from the Console
    /// </summary>
    public enum RdfServerConsoleMode
    {
        /// <summary>
        /// Quit
        /// </summary>
        Quit,
        /// <summary>
        /// Run
        /// </summary>
        Run
    }

    /// <summary>
    /// Represents options for the Server
    /// </summary>
    public class RdfServerOptions
    {
        public const String DefaultServiceName = "rdfServerService";
        public const int DefaultPort = 1986;
        public const String ServerOptionsKey = "rdfServerOptions";
        public const String DotNetRdfConfigKey = "dotNetRDFConfig";

        private RdfServerConsoleMode _mode = RdfServerConsoleMode.Run;

        private int _port = DefaultPort;
        private String _host = HttpServer.DefaultHost;
        private String _configFile = "default.ttl";
        private String _logFile = null;
        private String _logFormat = ApacheStyleLogger.LogCommon;
        private bool _verbose = false, _quiet = false;
        private String _baseDir = null;
        private bool _restControl = false;

        /// <summary>
        /// Creates new options from the command line arguments
        /// </summary>
        /// <param name="args">Arguments</param>
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

        /// <summary>
        /// Shows usage for the Server
        /// </summary>
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
            Console.WriteLine(" Sets the log file used for HTTP request logging");
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

        /// <summary>
        /// Creates an actual <see cref="HttpServer">HttpServer</see> instance from the options
        /// </summary>
        /// <returns></returns>
        public HttpServer GetServerInstance()
        {
            //Set up the server
            IHttpListenerHandlerCollection handlers = new HttpListenerHandlerCollection();
            if (this.RestControl) handlers.AddMapping(new HttpRequestMapping("POST", "/control", typeof(RestControlHandler)));

            HttpServer server = new HttpServer(this.Host, this.Port, handlers);
            server.BaseDirectory = this.BaseDirectory;

            ConfigurationLoader.PathResolver = new RdfServerPathResolver(server);

            //Add options to app settings for later use
            server.AppSettings[ServerOptionsKey] = this;

            //Configure handler mappings based on the configuration file
            IGraph g = this.LoadConfigurationGraph();
            if (g == null) throw new DotNetRdfConfigurationException("Specified Configuration File could not be found");
            this.ApplyHandlerMappings(g, handlers);
            if (handlers.Count == 0 || handlers.Count == 1 && this.RestControl) throw new DotNetRdfConfigurationException("Configuration File fails to specify any appropriate HTTP handler configuration");

            //Add static file handler if appropriate
            if (this.BaseDirectory != null)
            {
                handlers.AddMapping(new HttpRequestMapping(HttpRequestMapping.AllVerbs, HttpRequestMapping.AnyPath, typeof(StaticFileHandler)));
            }

            //Add MIME Type Mappings for RDF File Types
            foreach (MimeTypeDefinition definition in MimeTypesHelper.Definitions)
            {
                if (definition.FileExtensions.Any())
                {
                    server.MimeTypes.AddMimeType(definition.CanonicalFileExtension, definition.CanonicalMimeType);
                }
            }

            //Enable Log file
            if (this.LogFile != null)
            {
                server.AddLogger(new FileLogger(this.LogFile, this.LogFormat));
            }

            //Always enable process console
            server.Console = new ProcessConsole();

            //If Quite Mode is off then HTTP Request Logging to console is enabled
            if (!this.QuietMode)
            {
                server.AddLogger(new ConsoleLogger(this.LogFormat));
            }

            return server;
        }

        /// <summary>
        /// Applies the handler mappings
        /// </summary>
        /// <param name="g">Graph</param>
        /// <param name="handlers">Handlers Collection</param>
        private void ApplyHandlerMappings(IGraph g, IHttpListenerHandlerCollection handlers)
        {
            INode rdfType = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            INode httpHandler = g.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassHttpHandler));

            foreach (Triple t in g.GetTriplesWithPredicateObject(rdfType, httpHandler))
            {
                INode pathNode = t.Subject;
                if (pathNode.NodeType == NodeType.Uri)
                {
                    Uri pathUri = ((IUriNode)pathNode).Uri;
                    if (pathUri.Scheme.Equals("dotnetrdf"))
                    {
                        String path = pathUri.AbsoluteUri.Substring(10);
                        handlers.AddMapping(new HttpRequestMapping(HttpRequestMapping.AllVerbs, path, typeof(SparqlServerHandler)));
                    }
                }
                else
                {
                    continue;
                }
            }
        }

        /// <summary>
        /// Loads the Configuration Graph
        /// </summary>
        /// <returns></returns>
        internal IGraph LoadConfigurationGraph()
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
                        IRdfReader reader = MimeTypesHelper.GetParserByFileExtension(MimeTypesHelper.GetTrueFileExtension(this._configFile));
                        g = new Graph();
                        reader.Load(g, new StreamReader(stream));
                    }
                }
            }
            catch (RdfParserSelectionException)
            {
                g = null;
                Console.Error.WriteLine("rdfServer: Error: Configuration File '" + this.ConfigurationFile + "' could not be loaded as a suitable parser was not found");
            }
            catch (RdfParseException)
            {
                g = null;
                Console.Error.WriteLine("rdfServer: Error: Configuration File '" + this.ConfigurationFile + "' was not valid RDF");
            }

            //If got a Graph OK then prep the dotNetRDF Configuration API
            if (g != null)
            {
                ConfigurationLoader.AutoConfigure(g);
            }
            return g;
        }

        /// <summary>
        /// Gets the Server Mode
        /// </summary>
        public RdfServerConsoleMode Mode
        {
            get
            {
                return this._mode;
            }
        }

        /// <summary>
        /// Gets the Port
        /// </summary>
        public int Port
        {
            get
            {
                return this._port;
            }
        }

        /// <summary>
        /// Gets the Host
        /// </summary>
        public String Host
        {
            get
            {
                return this._host;
            }
        }

        /// <summary>
        /// Gets the Configuration File
        /// </summary>
        public String ConfigurationFile
        {
            get
            {
                return this._configFile;
            }
        }

        /// <summary>
        /// Gets the Log File
        /// </summary>
        public String LogFile
        {
            get
            {
                return this._logFile;
            }
        }

        /// <summary>
        /// Gets the Log Format
        /// </summary>
        public String LogFormat
        {
            get
            {
                return this._logFormat;
            }
        }

        /// <summary>
        /// Gets whether Verbose Mode is enabled
        /// </summary>
        public bool VerboseMode
        {
            get
            {
                return this._verbose;
            }
        }

        /// <summary>
        /// Gets whether Quiet Mode is enabled
        /// </summary>
        public bool QuietMode
        {
            get
            {
                return this._quiet;
            }
        }

        /// <summary>
        /// Gets whether REST control is enabled
        /// </summary>
        public bool RestControl
        {
            get
            {
                return this._restControl;
            }
        }

        /// <summary>
        /// Gets the Base Directory
        /// </summary>
        public String BaseDirectory
        {
            get 
            {
                return this._baseDir;
            }
        }

        /// <summary>
        /// Gets the string representation of the options which is the command line arguments used to invoke it
        /// </summary>
        /// <returns></returns>
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
