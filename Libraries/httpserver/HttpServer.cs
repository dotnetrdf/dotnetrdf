using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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

        #region Constructors

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
            this._preResponseModules.Add(new LoggingModule());
            this.Intialise(host, port);

            //Auto-start if specified
            if (autostart) this.Start();
        }

        private void Intialise(String host, int port)
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
                    HttpListenerContext context = this._listener.GetContext();

                    //Hand it off to be processed asynchronously
                    this._delegate.BeginInvoke(context, new AsyncCallback(this.EndRequest), null);
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
