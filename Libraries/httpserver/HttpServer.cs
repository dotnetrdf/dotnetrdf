/*

Copyright Robert Vesse 2009-12
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml;
using VDS.Web.Configuration;
using VDS.Web.Handlers;
using VDS.Web.Logging;
using VDS.Web.Modules;

namespace VDS.Web
{
    /// <summary>
    /// A simple but extensible HTTP Server based on a <see cref="HttpListener">HttpListener</see>
    /// </summary>
    public class HttpServer
        : IDisposable
    {
        private HttpListener _listener;
        private IHttpListenerHandlerCollection _handlers;
        private List<IHttpListenerModule> _preRequestModules = new List<IHttpListenerModule>();
        private List<IHttpListenerModule> _preResponseModules = new List<IHttpListenerModule>();
        private VirtualDirectoryManager _virtualDirs;
        private MimeTypeManager _mimeTypes;

        private Thread _serverThread;
        private bool _shouldTerminate = false;
        private HandleRequestDelegate _delegate;
        private bool _disposed = false;

        private int _port = DefaultPort;
        private String _host = DefaultHost;
        private String _baseDirectory = null;

        private HttpServerState _state = new HttpServerState();
        private List<IHttpLogger> _loggers = new List<IHttpLogger>();

        /// <summary>
        /// Constant for using accepting requests to the port from any host name
        /// </summary>
        public const String AnyHost = "*";
        /// <summary>
        /// Constant for the Default Host used if none is specified (localhost)
        /// </summary>
        public const String DefaultHost = "localhost";
        /// <summary>
        /// Constant for the Default Port used if none is specified (80)
        /// </summary>
        public const int DefaultPort = 80;
        /// <summary>
        /// Constant for the Default Configuration File used
        /// </summary>
        public const String DefaultConfigurationFile = "server.config";

        #region Constructors

        #region Explicit Constructors

        /// <summary>
        /// Creates a new Server with the given host
        /// </summary>
        /// <param name="host">Host</param>
        /// <remarks>
        /// Must be explicitly started
        /// </remarks>
        public HttpServer(String host)
            : this(host, new HttpListenerHandlerCollection(), false) { }

        /// <summary>
        /// Creates a new Server with the given host and optionally starts the server
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="autostart">Whether to auto-start</param>
        public HttpServer(String host, bool autostart)
            : this(host, new HttpListenerHandlerCollection(), autostart) { }

        /// <summary>
        /// Creates a new Server with the given handlers
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="handlers">Handlers</param>
        /// <remarks>
        /// Must be explicitly started
        /// </remarks>
        public HttpServer(String host, HttpListenerHandlerCollection handlers)
            : this(host, handlers, false) { }

        /// <summary>
        /// Creates a new Server on the given port
        /// </summary>
        /// <param name="port">Port</param>
        /// <remarks>
        /// Must be explicitly started
        /// </remarks>
        public HttpServer(int port)
            : this(port, new HttpListenerHandlerCollection()) { }

        /// <summary>
        /// Creates a new Server on the given port and optionally starts the server
        /// </summary>
        /// <param name="port">Port</param>
        /// <param name="autostart">Whether to auto-start</param>
        public HttpServer(int port, bool autostart)
            : this(port, new HttpListenerHandlerCollection(), autostart) { }

        /// <summary>
        /// Creates a new Server on the given port with the given handlers
        /// </summary>
        /// <param name="port">Port</param>
        /// <param name="handlers">Handlers</param>
        /// <remarks>
        /// Must be explicitly started
        /// </remarks>
        public HttpServer(int port, IHttpListenerHandlerCollection handlers)
            : this(port, handlers, false) { }

        /// <summary>
        /// Creates a new Server on the given host with the given handlers
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="handlers">Handlers</param>
        /// <param name="autostart">Whether to auto-start</param>
        public HttpServer(String host, IHttpListenerHandlerCollection handlers, bool autostart)
            : this(host, DefaultPort, handlers, autostart) { }

        /// <summary>
        /// Creates a new Server on the given port with the given handlers
        /// </summary>
        /// <param name="port">Port</param>
        /// <param name="handlers">Handlers</param>
        /// <param name="autostart">Whether to auto-start</param>
        public HttpServer(int port, IHttpListenerHandlerCollection handlers, bool autostart)
            : this(DefaultHost, port, handlers, autostart) { }

        /// <summary>
        /// Creates a new Server on the given host and port
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="port">Port</param>
        /// <remarks>
        /// Must be explicitly started
        /// </remarks>
        public HttpServer(String host, int port)
            : this(host, port, new HttpListenerHandlerCollection(), false) { }

        /// <summary>
        /// Creates a new Server on the given host and port and optionally starts the server
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="port">Port</param>
        /// <param name="autostart">Whether to auto-start</param>
        public HttpServer(String host, int port, bool autostart)
            : this(host, port, new HttpListenerHandlerCollection(), autostart) { }

        /// <summary>
        /// Creates a new Server on the given host and port with the given handlers
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="port">Port</param>
        /// <param name="handlers">Handlers</param>
        /// <remarks>
        /// Must be explicitly started
        /// </remarks>
        public HttpServer(String host, int port, IHttpListenerHandlerCollection handlers)
            : this(host, port, handlers, false) { }

        /// <summary>
        /// Creates a new Server on the given host and port with the given handlers and optionally starts the server
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="port">Port</param>
        /// <param name="handlers">Handlers</param>
        /// <param name="autostart">Whether to auto-start</param>
        public HttpServer(String host, int port, IHttpListenerHandlerCollection handlers, bool autostart)
        {
            if (host == null) throw new ArgumentNullException("host", "Cannot specify a null host, to specify any host please use the constant HttpServer.AnyHost");
            if (handlers == null) throw new ArgumentNullException("handlers", "Cannot specify a null Handlers Collection");
            if (handlers.Count == 0) throw new ArgumentException("Cannot specify an empty Handlers Collection", "handlers");
            if (!HttpListener.IsSupported) throw new PlatformNotSupportedException("HttpServer is not able to run on your Platform as HttpListener is not supported");

            this._handlers = handlers;
            this._mimeTypes = new MimeTypeManager(true);
            this._preResponseModules.Add(new LoggingModule());
            this.Initialise(host, port);

            //Auto-start if specified
            if (autostart) this.Start();
        }

        #endregion

        #region Using Configuration File Constructors

        /// <summary>
        /// Creates a new Server on the given host and port using a specified configuration file and optionally starts the server
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="port">Port</param>
        /// <param name="configFile">Configuration File</param>
        /// <param name="autostart">Whether to auto-start</param>
        public HttpServer(String host, int port, String configFile, bool autostart)
        {
            if (host == null) throw new ArgumentNullException("host", "Cannot specify a null host, to specify any host please use the constant HttpServer.AnyHost");
            if (!HttpListener.IsSupported) throw new PlatformNotSupportedException("HttpServer is not able to run on your Platform as HttpListener is not supported");
            if (!File.Exists(configFile)) throw new ArgumentException("Cannot specify a non-existent Configuration File for the Server", "configFile");
            
            this._handlers = new HttpListenerHandlerCollection();

            //Read in the Configuration File
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(configFile);

                if (doc.DocumentElement.Name.Equals("server"))
                {
                    if (doc.DocumentElement.Attributes.Count > 0)
                    {
                        foreach (XmlAttribute attr in doc.DocumentElement.Attributes)
                        {
                            switch (attr.Name)
                            {
                                case "basedir":
                                    String baseDir = attr.Value;
                                    if (baseDir.Equals(".")) baseDir = Environment.CurrentDirectory;
                                    if (!Directory.Exists(baseDir)) throw new HttpServerException("Configuration File specifies a base directory with the basedir attribute of the <server> element which is not a valid directory");
                                    this._baseDirectory = baseDir;
                                    if (!this._baseDirectory.EndsWith(new String(new char[]{Path.DirectorySeparatorChar}))) this._baseDirectory += Path.DirectorySeparatorChar;
                                    break;
                            }
                        }
                    }

                    foreach (XmlNode child in doc.DocumentElement.ChildNodes)
                    {
                        //Ignore non-element Nodes (e.g. comments)
                        if (child.NodeType != XmlNodeType.Element) continue;

                        switch (child.Name)
                        {
                            case "appSettings":
                                //Load App Settings
                                foreach (XmlNode appSetting in child.ChildNodes)
                                {
                                    if (appSetting.NodeType != XmlNodeType.Element) continue;

                                    if (appSetting.Name.Equals("add"))
                                    {
                                        String name = appSetting.Attributes.GetSafeNamedItem("key");
                                        if (name == null) throw new HttpServerException("Configuration File is invalid since an <add> element in the <appSettings> section does not have a key attribute");
                                        String value = appSetting.Attributes.GetSafeNamedItem("value");

                                        this.State[name] = value;
                                    }
                                    else
                                    {
                                        throw new HttpServerException("Configuration File is invalid since one of the child elements of the <appSettings> element is not an <add> element");
                                    }
                                }
                                break;

                            case "mimeTypes":
                                //MIME Type mappings
                                this._mimeTypes = new MimeTypeManager(child);
                                break;

                            case "virtualDirs":
                                //Load Virtual Directories
                                foreach (XmlNode vDir in child.ChildNodes)
                                {
                                    if (vDir.NodeType != XmlNodeType.Element) continue;

                                    if (vDir.Name.Equals("virtualDir"))
                                    {
                                        String path = vDir.Attributes.GetSafeNamedItem("path");
                                        if (path == null) throw new HttpServerException("Configuration File is invalid since a <virtualDir> element does not have the required path attribute");
                                        String dir = vDir.Attributes.GetSafeNamedItem("directory");
                                        if (dir == null) throw new HttpServerException("Configuration File is invalid since a <virtualDir> element does not have the required directory attribute");
                                        if (!Directory.Exists(dir)) throw new HttpServerException("Configuration File is invalid since a <virtualDir> element specified a non-existent directory for its directory attribute");

                                        this.AddVirtualDirectory(path, dir);
                                    }
                                    else
                                    {
                                        throw new HttpServerException("Configuration File is invalid since of the child elements of the <virtualDirs> element is not a <virtualDir> element");
                                    }
                                }
                                break;

                            case "modules":
                                //Load Modules
                                foreach (XmlNode moduleTypeNode in child.ChildNodes)
                                {
                                    if (moduleTypeNode.NodeType != XmlNodeType.Element) continue;

                                    String moduleType;
                                    switch (moduleTypeNode.Name)
                                    {
                                        case "prerequest":
                                            foreach (XmlNode module in moduleTypeNode.ChildNodes)
                                            {
                                                moduleType = module.Attributes.GetSafeNamedItem("type");
                                                if (moduleType == null) throw new HttpServerException("Configuration File is invalid since the a <module> element does not have the required type attribute");

                                                try
                                                {
                                                    Object temp = Activator.CreateInstance(Type.GetType(moduleType));
                                                    if (temp is IHttpListenerModule)
                                                    {
                                                        this.AddPreRequestModule((IHttpListenerModule)temp);
                                                    }
                                                    else
                                                    {
                                                        throw new HttpServerException("Configuration File is invalid since it contains a <module> element which specifies '" + moduleType + "' for its type attribute but this type does not implement the IHttpListenerModule interface");
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    throw new HttpServerException("Configuration File is invalid since it contains a <module> element which specifies '" + moduleType + "' for its type attribute but this type and this type could not be instantiated successfully", ex);
                                                }
                                            }
                                            break;
                                        case "preresponse":
                                            foreach (XmlNode module in moduleTypeNode.ChildNodes)
                                            {
                                                moduleType = module.Attributes.GetSafeNamedItem("type");
                                                if (moduleType == null) throw new HttpServerException("Configuration File is invalid since the a <module> element does not have the required type attribute");

                                                try
                                                {
                                                    Object temp = Activator.CreateInstance(Type.GetType(moduleType));
                                                    if (temp is IHttpListenerModule)
                                                    {
                                                        this.AddPreResponseModule((IHttpListenerModule)temp);
                                                    }
                                                    else
                                                    {
                                                        throw new HttpServerException("Configuration File is invalid since it contains a <module> element which specifies '" + moduleType + "' for its type attribute but this type does not implement the IHttpListenerModule interface");
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    throw new HttpServerException("Configuration File is invalid since it contains a <module> element which specifies '" + moduleType + "' for its type attribute but this type and this type could not be instantiated successfully", ex);
                                                }
                                            }
                                            break;
                                        default:
                                            throw new HttpServerException("Configuration File is invalid since the <modules> element contains an unknown child element <" + moduleTypeNode.Name + ">");
                                    }
                                }

                                break;

                            case "handlers":
                                //Load Handlers
                                foreach (XmlNode handler in child.ChildNodes)
                                {
                                    if (handler.NodeType != XmlNodeType.Element) continue;

                                    if (handler.Name.Equals("handler"))
                                    {
                                        String verb = handler.Attributes.GetSafeNamedItem("verb");
                                        if (verb == null) verb = HttpRequestMapping.AllVerbs;
                                        String path = handler.Attributes.GetSafeNamedItem("path");
                                        if (path == null) path = HttpRequestMapping.AnyPath;
                                        String type = handler.Attributes.GetSafeNamedItem("type");
                                        if (type == null) throw new HttpServerException("Configuration File is invalid since one of the <handler> elements in the <handlers> section does not specify a type attribute");

                                        Type t = Type.GetType(type);
                                        if (t == null) throw new HttpServerException("Configuration File is invalid since the value '" + type + "' given for the type attribute of a <handler> element is not the type name of a valid type (you may be missing an Assembly reference of failed to specify a Fully Qualified Type Name");
                                        this._handlers.AddMapping(new HttpRequestMapping(verb, path, t));
                                    }
                                    else
                                    {
                                        throw new HttpServerException("Configuration File is invalid since one of the child elements of the <handlers> element is not a <handler> element");
                                    }
                                }

                                break;

                            case "loggers":
                                //Load Loggers
                                foreach (XmlNode logger in child.ChildNodes)
                                {
                                    if (logger.NodeType != XmlNodeType.Element) continue;

                                    String logFormat;
                                    switch (logger.Name)
                                    {
                                        case "filelogger":
                                            String logFile = logger.Attributes.GetSafeNamedItem("file");
                                            if (logFile == null) throw new HttpServerException("Configuration File is invalid since a <filelogger> element does not have the required file attribute");
                                            logFormat = logger.Attributes.GetSafeNamedItem("format");
                                            if (logFormat == null) logFormat = "common";
                                            logFormat = ApacheStyleLogger.GetLogFormat(logFormat);
                                            this.AddLogger(new FileLogger(logFile, logFormat));
                                            break;

                                        case "consolelogger":
                                            logFormat = logger.Attributes.GetSafeNamedItem("format");
                                            if (logFormat == null) logFormat = "common";
                                            logFormat = ApacheStyleLogger.GetLogFormat(logFormat);
                                            String verbose = logger.Attributes.GetSafeNamedItem("verbose");
                                            if (verbose == null) verbose = "false";
                                            if (verbose.Equals("true"))
                                            {
                                                this.AddLogger(new ConsoleLogger());
                                            }
                                            else
                                            {
                                                this.AddLogger(new ConsoleErrorLogger());
                                            }
                                            break;

                                        default:
                                            if (logger.Attributes.GetNamedItem("loadwith") != null)
                                            {
                                                String loader = logger.Attributes["loadwith"].Value;
                                                try
                                                {
                                                    Object temp = Activator.CreateInstance(Type.GetType(loader));
                                                    if (temp is IConfigurationLoader)
                                                    {
                                                        ((IConfigurationLoader)temp).Load(child, this);
                                                    }
                                                    else
                                                    {
                                                        throw new HttpServerException("Configuration File contains unexpected logger element <" + logger.Name + "> whose loadwith attribute points to a type which does not implement the required IConfigurationLoader interface");
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    throw new HttpServerException("Configuration File contains unexpected logger element <" + logger.Name + "> whose loadwith attribute points to a type which could not be instantiated successfully or whose Load() method raised an exception", ex);
                                                }
                                            }
                                            else
                                            {
                                                throw new HttpServerException("Configuration File contains unexpected logger element <" + logger.Name + "> which does not have a loadwith attribute to specify a type which implement IConfigurationLoader and can load its configuration information");
                                            }
                                            break;
                                    }
                                }
                                break;

                            default:
                                //Unknown attributes are processed by an IConfigurationLoader specified by a @loadwith attribute
                                if (child.Attributes.GetNamedItem("loadwith") != null)
                                {
                                    String loader = child.Attributes["loadwith"].Value;
                                    try
                                    {
                                        Object temp = Activator.CreateInstance(Type.GetType(loader));
                                        if (temp is IConfigurationLoader)
                                        {
                                            ((IConfigurationLoader)temp).Load(child, this);
                                        }
                                        else
                                        {
                                            throw new HttpServerException("Configuration File contains unexpected element <" + child.Name + "> whose loadwith attribute points to a type which does not implement the required IConfigurationLoader interface");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new HttpServerException("Configuration File contains unexpected element <" + child.Name + "> whose loadwith attribute points to a type which could not be instantiated successfully or whose Load() method raised an exception", ex);
                                    }
                                }
                                else
                                {
                                    throw new HttpServerException("Configuration File contains unexpected element <" + child.Name + "> which does not have a loadwith attribute to specify a type which implement IConfigurationLoader and can load its configuration information");
                                }
                                break;
                        }
                    }
                }
                else
                {
                    throw new HttpServerException("Configuration File did not contain the required root element <server>");
                }
            }
            catch (HttpServerException)
            {
                //This catch exists since the try block may throw this error and if it does we don't want to wrap it which is what the general catch block will do
                throw;
            }
            catch (Exception ex)
            {
                throw new HttpServerException("Unable to create a HTTP Server instance since the Configuration File was invalid due to the following error: " + ex.Message, ex);
            }

            //If there was no <mimeTypes> section then use defaults
            if (this._mimeTypes == null) this._mimeTypes = new MimeTypeManager(true);

            this.Initialise(host, port);

            //Auto-start if specified
            if (autostart) this.Start();
        }

        /// <summary>
        /// Creates a new Server on the given host and port using a specified configuration file
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="port">Port</param>
        /// <param name="configFile">Configuration File</param>
        /// <remarks>
        /// Must be explicitly started
        /// </remarks>
        public HttpServer(String host, int port, String configFile)
            : this(host, port, configFile, false) { }

        /// <summary>
        /// Creates a new Server on the given host using a specified configuration file and optionally starts the server
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="configFile">Configuration File</param>
        /// <param name="autostart">Whether to auto-start</param>
        public HttpServer(String host, String configFile, bool autostart)
            : this(host, DefaultPort, configFile, autostart) { }

        /// <summary>
        /// Creates a new Server on the given host using a specified configuration file
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="configFile">Configuration File</param>
        /// <remarks>
        /// Must be explicitly started
        /// </remarks>
        public HttpServer(String host, String configFile)
            : this(host, DefaultPort, configFile, false) { }

        /// <summary>
        /// Creates a new Server on the given port using a specified configuration file and optionally starts the server
        /// </summary>
        /// <param name="port">Port</param>
        /// <param name="configFile">Configuration File</param>
        /// <param name="autostart">Whether to auto-start</param>
        public HttpServer(int port, String configFile, bool autostart)
            : this(DefaultHost, port, configFile, autostart) { }

        /// <summary>
        /// Creates a new Server on the given port using a specified configuration file
        /// </summary>
        /// <param name="port">Port</param>
        /// <param name="configFile">Configuration File</param>
        /// <remarks>
        /// Must be explicitly started
        /// </remarks>
        public HttpServer(int port, String configFile)
            : this(DefaultHost, port, configFile, false) { }

        #endregion

        /// <summary>
        /// Destructor which ensures the server is cleaned up
        /// </summary>
        ~HttpServer()
        {
            this.DisposeInternal(false);
        }

        /// <summary>
        /// Initialisation of the Server
        /// </summary>
        /// <param name="host">Host</param>
        /// <param name="port">Port</param>
        private void Initialise(String host, int port)
        {
            this._port = port;
            this._host = host;
            this._listener = new HttpListener();
            this._listener.Prefixes.Add("http://" + host + ":" + port + "/");
            this._listener.IgnoreWriteExceptions = true;

            this._delegate = new HandleRequestDelegate(this.HandleRequest);
            this._serverThread = new Thread(new ThreadStart(this.Run));
            this._serverThread.Start();
        }

        #endregion

        #region Server Properties

        /// <summary>
        /// Gets whether the Server is running
        /// </summary>
        public bool IsRunning
        {
            get
            {
                if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
                return this._listener.IsListening;
            }
        }

        /// <summary>
        /// Gets the Port the Server is using
        /// </summary>
        public int Port
        {
            get
            {
                if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
                return this._port;
            }
        }

        /// <summary>
        /// Gets the Host the Server is using
        /// </summary>
        public String Host
        {
            get
            {
                if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
                return this._host;
            }
        }

        /// <summary>
        /// Gets the Base Directory used for static content
        /// </summary>
        public String BaseDirectory
        {
            get
            {
                if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
                return this._baseDirectory;
            }
            set
            {
                if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
                this._baseDirectory = value;
            }
        }

        /// <summary>
        /// Gets the State of the Server
        /// </summary>
        public HttpServerState State
        {
            get
            {
                if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
                return this._state;
            }
        }

        /// <summary>
        /// Gets the Handlers for the Server
        /// </summary>
        public IHttpListenerHandlerCollection Handlers
        {
            get
            {
                if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
                return this._handlers;
            }
        }

        /// <summary>
        /// Gets the Loggers for the Server
        /// </summary>
        public IEnumerable<IHttpLogger> Loggers
        {
            get
            {
                if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
                return this._loggers;
            }
        }

        /// <summary>
        /// Gets the pre-request modules for the Server
        /// </summary>
        public IEnumerable<IHttpListenerModule> PreRequestModules
        {
            get
            {
                if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
                return this._preRequestModules;
            }
        }

        /// <summary>
        /// Gets the pre-response modules for the Server
        /// </summary>
        public IEnumerable<IHttpListenerModule> PreResponseModules
        {
            get
            {
                if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
                return this._preResponseModules;
            }
        }

        #endregion

        #region Server Configuration

        /// <summary>
        /// Adds a virtual directory to the Server
        /// </summary>
        /// <param name="path">Virtual Path</param>
        /// <param name="directory">Physical Directory</param>
        public void AddVirtualDirectory(String path, String directory)
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
            if (this._virtualDirs == null) this._virtualDirs = new VirtualDirectoryManager();
            this._virtualDirs.AddVirtualDirectory(path, directory);
        }

        /// <summary>
        /// Adds a MIME Type to the Server
        /// </summary>
        /// <param name="ext">File Extension</param>
        /// <param name="mimeType">MIME Type</param>
        /// <param name="binary">Whether the type should be treated as binary content</param>
        public void AddMimeType(String ext, String mimeType, bool binary)
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
            this._mimeTypes.AddMimeType(ext, mimeType, binary);
        }

        /// <summary>
        /// Adds a MIME Type to the Server
        /// </summary>
        /// <param name="ext">File Extension</param>
        /// <param name="mimeType">MIME Type</param>
        public void AddMimeType(String ext, String mimeType)
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
            this._mimeTypes.AddMimeType(ext, mimeType);
        }

        /// <summary>
        /// Adds a pre-request module to the Server
        /// </summary>
        /// <param name="module">Module</param>
        public void AddPreRequestModule(IHttpListenerModule module)
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
            this._preRequestModules.Add(module);
        }

        /// <summary>
        /// Adds a pre-response module to the Server
        /// </summary>
        /// <param name="module">Module</param>
        public void AddPreResponseModule(IHttpListenerModule module)
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
            this._preResponseModules.Add(module);
        }

        /// <summary>
        /// Inserts a pre-request module to the Server
        /// </summary>
        /// <param name="module">Module</param>
        /// <param name="insertAt">Index to insert at</param>
        public void InsertPreRequestModule(IHttpListenerModule module, int insertAt)
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
            this._preRequestModules.Insert(insertAt, module);
        }

        /// <summary>
        /// Inserts a pre-response module to the Server
        /// </summary>
        /// <param name="module">Module</param>
        /// <param name="insertAt">Index to insert at</param>
        public void InsertPreResponseModule(IHttpListenerModule module, int insertAt)
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
            this._preResponseModules.Insert(insertAt, module);
        }

        /// <summary>
        /// Removes a pre-request module to the Server
        /// </summary>
        /// <param name="module">Module</param>
        public void RemovePreRequestModule(IHttpListenerModule module)
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
            this._preRequestModules.Remove(module);
        }

        /// <summary>
        /// Removes a pre-response module to the Server
        /// </summary>
        /// <param name="module">Module</param>
        public void RemovePreResponseModule(IHttpListenerModule module)
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
            this._preResponseModules.Remove(module);
        }

        /// <summary>
        /// Removes all pre-request modules
        /// </summary>
        public void ClearPreRequestModules()
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
            this._preRequestModules.Clear();
        }

        /// <summary>
        /// Removes all pre-response modules
        /// </summary>
        public void ClearPreResponseModules()
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
            this._preResponseModules.Clear();
        }

        /// <summary>
        /// Adds a Logger
        /// </summary>
        /// <param name="logger">Logger</param>
        public void AddLogger(IHttpLogger logger)
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
            this._loggers.Add(logger);
        }

        /// <summary>
        /// Inserts a Logger
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="insertAt">Index to insert at</param>
        public void InsertLogger(IHttpLogger logger, int insertAt)
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
            this._loggers.Insert(insertAt, logger);
        }

        /// <summary>
        /// Removes a Logger
        /// </summary>
        /// <param name="logger">Logger</param>
        public void RemoveLogger(IHttpLogger logger)
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
            this._loggers.Remove(logger);
        }

        /// <summary>
        /// Removes all loggers
        /// </summary>
        public void ClearLoggers()
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
            this._loggers.Clear();
        }

        #endregion

        #region Server Control

        /// <summary>
        /// Starts the Server
        /// </summary>
        public void Start()
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");

            //Restart Listener if required
            if (!this._listener.IsListening)
            {
                this._listener.Start();
                Console.WriteLine("HttpServer Starting on " + this._host + ":" + this._port);
                while (!this._listener.IsListening)
                {
                    //Wait for listener to start
                }
                Console.WriteLine("HttpServer Started");
            }
        }

        /// <summary>
        /// Restarts the Server
        /// </summary>
        public void Restart()
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");

            Console.WriteLine("HttpServer Restarting...");
            this.Stop();
            this.Start();
        }

        /// <summary>
        /// Stops the Server
        /// </summary>
        public void Stop()
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");

            //Only Stop if Listener is starting
            if (this._listener.IsListening)
            {
                Console.WriteLine("HttpServer Stopping...");
                this._listener.Stop();
                Console.WriteLine("HttpServer Stopped");
            }
        }

        /// <summary>
        /// Shuts down the server and optionally stops the process in which it is running
        /// </summary>
        /// <param name="stopProcess">Whether to stop the containing process</param>
        /// <param name="force">Whether to force the containing process to stop by killing it</param>
        /// <remarks>
        /// <para>
        /// After a Server has been shut down it's methods and properties cannot be accessed
        /// </para>
        /// </remarks>
        public void Shutdown(bool stopProcess, bool force)
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");

            Console.WriteLine("HttpServer shutting down...");
            this.Dispose();

            if (stopProcess)
            {
                Process p = System.Diagnostics.Process.GetCurrentProcess();
                Console.WriteLine("HttpServer " + (force ? "killing" : "closing") + " containing process...");
                if (force)
                {
                    p.Kill();
                }
                else
                {
                    p.Close();
                }
            }
            Console.WriteLine("HttpServer shut down");
        }

        #endregion

        #region Request Processing

        /// <summary>
        /// Actual method which implements the main server process
        /// </summary>
        private void Run()
        {
            while (true)
            {
                if (this._disposed || this._shouldTerminate) return;

                if (this._listener.IsListening)
                {
                    try
                    {
                        HttpListenerContext context = this._listener.GetContext();

                        //Hand it off to be processed asynchronously
                        this._delegate.BeginInvoke(context, new AsyncCallback(this.EndRequest), null);
                    }
                    catch (Exception ex)
                    {
                        if (this._listener.IsListening) this.LogErrors(ex);
                    }
                }
                else
                {
                    //Wait until we're asked to listen again
                    Thread.Sleep(50);
                }
            }
        }

        /// <summary>
        /// Delegate for handing off request processing to be asynchronous
        /// </summary>
        /// <param name="context">Listener Context</param>
        /// <returns></returns>
        private delegate HttpServerContext HandleRequestDelegate(HttpListenerContext context);

        /// <summary>
        /// Begins request handling
        /// </summary>
        /// <param name="context">Listener Context</param>
        /// <returns></returns>
        private HttpServerContext HandleRequest(HttpListenerContext context)
        {
            IHttpListenerHandler handler;
            HttpServerContext serverContext = new HttpServerContext(this, context);
            try
            {
                bool skipHandling = this.ApplyPreRequestModules(serverContext);
                if (!skipHandling)
                {
                    handler = this._handlers.GetHandler(serverContext);
                    handler.ProcessRequest(serverContext);
                }
            }
            catch (NoHandlerException noHandlerEx)
            {
                this.LogErrors(noHandlerEx);
                context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            }
            catch (HttpServerException serverEx)
            {
                this.LogErrors(serverEx);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            return serverContext;
        }

        /// <summary>
        /// Ends request handling
        /// </summary>
        /// <param name="result">Async Result</param>
        private void EndRequest(IAsyncResult result)
        {
            try
            {
                HttpServerContext context = this._delegate.EndInvoke(result);
                if (this._listener.IsListening)
                {
                    this.ApplyPreResponseModules(context);
                    context.Response.Close();
                }
            }
            catch (Exception ex)
            {
                this.LogErrors(ex);
            }
        }

        /// <summary>
        /// Applies pre-request modules returning whether actual request handling should be skipped
        /// </summary>
        /// <param name="context">Server Context</param>
        /// <returns></returns>
        private bool ApplyPreRequestModules(HttpServerContext context)
        {
            foreach (IHttpListenerModule module in this._preRequestModules)
            {
                try
                {
                    //If a Module returns false then Handling should be skipped and go onto pre-response modules
                    if (!module.ProcessRequest(context)) return true;
                }
                catch (Exception ex)
                {
                    this.LogErrors(ex);
                }
            }

            //If all Modules return true then Handling continues normally
            return false;
        }

        /// <summary>
        /// Applies pre-response modules
        /// </summary>
        /// <param name="context">Server Context</param>
        private void ApplyPreResponseModules(HttpServerContext context)
        {
            foreach (IHttpListenerModule module in this._preResponseModules)
            {
                try
                {
                    //If a Module returns false then further pre-response modules are skipped
                    if (!module.ProcessRequest(context)) return;
                }
                catch (Exception ex)
                {
                    this.LogErrors(ex);
                }
            }
        }

        /// <summary>
        /// Maps a URL Path to a physical path returning null if no such mapping is possible
        /// </summary>
        /// <param name="path">URL Path</param>
        /// <returns>Either a Physical Path or null is not a valid path</returns>
        public String MapPath(String path)
        {
            String realPath;
            if (this._virtualDirs != null)
            {
                //Try to Map against Virtual Directories
                String relativePath;
                realPath = this._virtualDirs.GetDirectory(path, out relativePath);
                if (realPath != null)
                {
                    //Maps to a Virtual Directory so resolve the path relative to the virtual directory
                    //Not allowed to break out of directory, if the content you output has parent paths in it 
                    //then is up to the client to resolve these themselves before making subsequent requests
                    if (relativePath.StartsWith("/")) relativePath = relativePath.Substring(1);
                    if (relativePath.Contains("../") || relativePath.Contains("./")) return null;
                    realPath = Path.Combine(realPath, relativePath);
                    return realPath;
                }
            }
            
            //If Virtual Directories not available or don't apply then map against Base Directory
            if (this._baseDirectory != null)
            {
                if (path.StartsWith("/")) path = path.Substring(1);

                //Try to ensure that paths cannot go outside the base directory
                if (path.Contains("../") || path.Contains("./")) return null;
                realPath = Path.Combine(this._baseDirectory, path.Replace('/', Path.DirectorySeparatorChar));
                if (realPath.StartsWith(this._baseDirectory))
                {
                    return realPath;
                }
                else
                {
                    //If URL Path tries to go outside base directory then this should fail
                    return null;
                }
            }
            else
            {
                //No Base Directory so cannot map URL paths to physical paths
                return null;
            }
        }

        /// <summary>
        /// Gets the MIME Type for the response based on the file extension or null if the MIME type is not allowed to be served
        /// </summary>
        /// <param name="extension">File Extension</param>
        /// <returns></returns>
        public MimeTypeMapping GetMimeType(String extension)
        {
            return this._mimeTypes.GetMapping(extension);
        }

        /// <summary>
        /// Remaps the Request to another Handler, the Handler must be a registered Handler for remapping to succeed
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="handlerType">Handler Type</param>
        public void RemapHandler(HttpServerContext context, Type handlerType)
        {
            try
            {
                IHttpListenerHandler handler = this._handlers.GetHandler(handlerType);
                handler.ProcessRequest(context);
            }
            catch (NoHandlerException noHandlerEx)
            {
                this.LogErrors(noHandlerEx);
                context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            }
            catch (HttpServerException serverEx)
            {
                this.LogErrors(serverEx);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        /// <summary>
        /// Logs errors
        /// </summary>
        /// <param name="ex">Error</param>
        public void LogErrors(Exception ex)
        {
            foreach (IExtendedHttpLogger logger in this._loggers.OfType<IExtendedHttpLogger>())
            {
                logger.LogError(ex);
            }
        }

        #endregion

        /// <summary>
        /// Disposes of the Server
        /// </summary>
        public void Dispose()
        {
            this.DisposeInternal(true);
        }

        /// <summary>
        /// Actually dispose of the Server
        /// </summary>
        /// <param name="disposing">Whether this was called from the Dispose() method or not</param>
        private void DisposeInternal(bool disposing)
        {
            if (this._disposed) return;
            if (disposing) GC.SuppressFinalize(this);

            this._disposed = true;
            if (this._listener != null)
            {
                if (this._listener.IsListening) this._listener.Stop();
            }
            this._shouldTerminate = true;
            while (this._serverThread.ThreadState != System.Threading.ThreadState.Stopped)
            {
                Thread.Sleep(10);
            }
        }
    }
}
