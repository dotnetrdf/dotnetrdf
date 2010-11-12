using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.Web;
using VDS.Web.Logging;

namespace rdfServer
{
    public class RdfServerOptions
    {
        private int _port = 1986;
        private String _host = HttpServer.DefaultHost;
        private String _configFile = "default.ttl";
        private String _logFile = null;
        private String _logFormat = ApacheStyleLogger.CommonLogFormat;
        private bool _verbose = false;
        private String _baseDir = null;

        public RdfServerOptions(String[] args)
        {
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
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
                                if (this._logFormat.Equals("common"))
                                {
                                    this._logFormat = ApacheStyleLogger.CommonLogFormat;
                                }
                                else if (this._logFormat.Equals("combined"))
                                {
                                    this._logFormat = ApacheStyleLogger.CombinedLogFormat;
                                }
                            }
                            else
                            {
                                Console.Error.WriteLine("rdfServer: Error: Expected an argument after the -f/-format option to specify the log format string - Common Log Format will be used");
                            }
                            break;

                        case "-s":
                        case "-server":
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
                                    Console.Error.WriteLine("rdfServer: Error: The Base Directory specified for the -s/-server option does not exist");
                                }
                            }
                            else
                            {
                                Console.Error.WriteLine("rdfServer: Error: Expected an argument after the -s/-server option to specify the base directory from which to serve static content");
                            }
                            break;

                        default:
                            Console.Error.WriteLine("rdfServer: Error: Unknown option/argument '" + args[i] + "' was ignored");
                            break;
                    }
                }
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
