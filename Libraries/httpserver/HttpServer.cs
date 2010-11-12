using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace VDS.Web
{
    /// <summary>
    /// A simple but extensible HTTP Server based on a <see cref="HttpListener">HttpListener</see>
    /// </summary>
    public class HttpServer : IDisposable
    {
        private HttpListener _listener;
        private HttpListenerHandlerCollection _handlers;
        private Thread _serverThread;
        private bool _shouldTerminate = false;
        private HandleRequestDelegate _delegate;
        private bool _disposed = false;
        private HttpServerState _state = new HttpServerState();

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

        public HttpServer(String host, HttpListenerHandlerCollection handlers)
            : this(host, handlers, false) { }

        public HttpServer(int port, HttpListenerHandlerCollection handlers)
            : this(port, handlers, false) { }

        public HttpServer(String host, HttpListenerHandlerCollection handlers, bool autostart)
            : this(host, DefaultPort, handlers, autostart) { }

        public HttpServer(int port, HttpListenerHandlerCollection handlers, bool autostart)
            : this(DefaultHost, port, handlers, autostart) { }

        public HttpServer(String host, int port, HttpListenerHandlerCollection handlers, bool autostart)
        {
            if (host == null) throw new ArgumentNullException("host", "Cannot specify a null host, to specify any host please use the constant HttpServer.AnyHost");
            if (handlers == null) throw new ArgumentNullException("handlers", "Cannot specify a null Handlers Collection");
            if (handlers.Count == 0) throw new ArgumentException("Cannot specify an empty Handlers Collection", "handlers");
            if (!HttpListener.IsSupported) throw new PlatformNotSupportedException("HttpServer is not able to run on your Platform as HttpListener is not supported");

            this._handlers = handlers;
            this._handlers.Initialise(this._state);

            this._listener = new HttpListener();
            this._listener.Prefixes.Add("http://" + host + ":" + port + "/");
            this._listener.IgnoreWriteExceptions = true;

            this._delegate = new HandleRequestDelegate(this.HandleRequest);
            this._serverThread = new Thread(new ThreadStart(this.Run));
            this._serverThread.Start();

            //Auto-start if specified
            if (autostart) this.Start();
        }

        public bool IsRunning
        {
            get
            {
                if (this._disposed) throw new ObjectDisposedException("Cannot access the properties/methods of a HttpServer instance after it has been disposed of");
                return this._listener.IsListening;
            }
        }

        public HttpServerState State
        {
            get
            {
                return this._state;
            }
        }

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

        private delegate HttpListenerContext HandleRequestDelegate(HttpListenerContext context);

        private HttpListenerContext HandleRequest(HttpListenerContext context)
        {
            IHttpListenerHandler handler;
            try
            {
                handler = this._handlers.GetHandler(context);
                handler.ProcessRequest(context, this);
            }
            catch (NoHandlerException noHandlerEx)
            {
                //TODO: Log error
                context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                context.Response.Close();
            }
            catch (HttpServerException serverEx)
            {
                //TODO: Log error
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Close();
            }

            return context;
        }

        private void EndRequest(IAsyncResult result)
        {
            try
            {
                HttpListenerContext context = this._delegate.EndInvoke(result);

                context.Response.Close();
            }
            catch (Exception ex)
            {
                //TODO: Log errors here
            }
        }

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
