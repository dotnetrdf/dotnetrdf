using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
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
    public class HttpServer : IDisposable
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

        public HttpServer(String host)
            : this(host, new HttpListenerHandlerCollection(), false) { }

        public HttpServer(String host, bool autostart)
            : this(host, new HttpListenerHandlerCollection(), autostart) { }

        public HttpServer(String host, HttpListenerHandlerCollection handlers)
            : this(host, handlers, false) { }

        public HttpServer(int port)
            : this(port, new HttpListenerHandlerCollection()) { }

        public HttpServer(int port, bool autostart)
            : this(port, new HttpListenerHandlerCollection(), autostart) { }

        public HttpServer(int port, IHttpListenerHandlerCollection handlers)
            : this(port, handlers, false) { }

        public HttpServer(String host, IHttpListenerHandlerCollection handlers, bool autostart)
            : this(host, DefaultPort, handlers, autostart) { }

        public HttpServer(int port, IHttpListenerHandlerCollection handlers, bool autostart)
            : this(DefaultHost, port, handlers, autostart) { }

        public HttpServer(String host, int port)
            : this(host, port, new HttpListenerHandlerCollection(), false) { }

        public HttpServer(String host, int port, bool autostart)
            : this(host, port, new HttpListenerHandlerCollection(), autostart) { }

        public HttpServer(String host, int port, IHttpListenerHandlerCollection handlers)
            : this(host, port, handlers, false) { }

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
            catch (HttpServerException serverEx)
            {
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

        public HttpServer(String host, int port, String configFile)
            : this(host, port, configFile, false) { }

        public HttpServer(String host, String configFile, bool autostart)
            : this(host, DefaultPort, configFile, autostart) { }

        public HttpServer(String host, String configFile)
            : this(host, DefaultPort, configFile, false) { }

        public HttpServer(int port, String configFile, bool autostart)
            : this(DefaultHost, port, configFile, autostart) { }

        public HttpServer(int port, String configFile)
            : this(DefaultHost, port, configFile, false) { }

        #endregion

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

        public bool IsRunning
        {
            get
            {
                if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
                return this._listener.IsListening;
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

        public String BaseDirectory
        {
            get
            {
                return this._baseDirectory;
            }
            set
            {
                this._baseDirectory = value;
            }
        }

        public HttpServerState State
        {
            get
            {
                return this._state;
            }
        }

        public IHttpListenerHandlerCollection Handlers
        {
            get
            {
                return this._handlers;
            }
        }

        public IEnumerable<IHttpLogger> Loggers
        {
            get
            {
                return this._loggers;
            }
        }

        public IEnumerable<IHttpListenerModule> PreRequestModules
        {
            get
            {
                return this._preRequestModules;
            }
        }

        public IEnumerable<IHttpListenerModule> PreResponseModules
        {
            get
            {
                return this._preResponseModules;
            }
        }

        #endregion

        #region Server Configuration

        public void AddVirtualDirectory(String path, String directory)
        {
            if (this._virtualDirs == null) this._virtualDirs = new VirtualDirectoryManager();
            this._virtualDirs.AddVirtualDirectory(path, directory);
        }

        public void AddPreRequestModule(IHttpListenerModule module)
        {
            this._preRequestModules.Add(module);
        }

        public void AddPreResponseModule(IHttpListenerModule module)
        {
            this._preResponseModules.Add(module);
        }

        public void InsertPreRequestModule(IHttpListenerModule module, int insertAt)
        {
            this._preRequestModules.Insert(insertAt, module);
        }

        public void InsertPreResponseModule(IHttpListenerModule module, int insertAt)
        {
            this._preResponseModules.Insert(insertAt, module);
        }

        public void RemovePreRequestModule(IHttpListenerModule module)
        {
            this._preRequestModules.Remove(module);
        }

        public void RemovePreResponseModule(IHttpListenerModule module)
        {
            this._preResponseModules.Remove(module);
        }

        public void ClearPreRequestModules()
        {
            this._preRequestModules.Clear();
        }

        public void ClearPreResponseModules()
        {
            this._preResponseModules.Clear();
        }

        public void AddLogger(IHttpLogger logger)
        {
            this._loggers.Add(logger);
        }

        public void InsertLogger(IHttpLogger logger, int insertAt)
        {
            this._loggers.Insert(insertAt, logger);
        }

        public void RemoveLogger(IHttpLogger logger)
        {
            this._loggers.Remove(logger);
        }

        public void ClearLoggers()
        {
            this._loggers.Clear();
        }

        #endregion

        #region Server Control

        public void Start()
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");

            //Restart Listener if required
            if (!this._listener.IsListening)
            {
                this._listener.Start();
            }
        }

        public void Restart()
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");

            this.Stop();
            this.Start();
        }

        public void Stop()
        {
            if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");

            //Only Stop if Listener is starting
            if (this._listener.IsListening)
            {
                this._listener.Stop();
            }
        }

        #endregion

        #region Request Processing

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
                        this.LogErrors(ex);
                    }
                }
            }
        }

        private delegate HttpServerContext HandleRequestDelegate(HttpListenerContext context);

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

        private void EndRequest(IAsyncResult result)
        {
            try
            {
                HttpServerContext context = this._delegate.EndInvoke(result);
                this.ApplyPreResponseModules(context);
                context.Response.Close();
            }
            catch (Exception ex)
            {
                this.LogErrors(ex);
            }
        }

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
                    if (relativePath.StartsWith("/")) relativePath = relativePath.Substring(1);
                    realPath = Path.Combine(realPath, relativePath);
                    return realPath;
                }
            }
            
            //If Virtual Directories not available or don't apply then map against Base Directory
            if (this._baseDirectory != null)
            {
                if (path.StartsWith("/")) path = path.Substring(1);
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

        public void LogErrors(Exception ex)
        {
            foreach (IExtendedHttpLogger logger in this._loggers.OfType<IExtendedHttpLogger>())
            {
                logger.LogError(ex);
            }
        }

        #endregion

        public void Dispose()
        {
            this._disposed = true;
            this._listener.Stop();
            this._shouldTerminate = true;
            while (this._serverThread.ThreadState != ThreadState.Stopped)
            {
                Thread.Sleep(10);
            }
        }
    }
}
