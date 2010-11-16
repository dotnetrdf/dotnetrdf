using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using VDS.Web;
using VDS.Web.Logging;

namespace rdfServer.GUI
{
    interface IServerHarness : IDisposable
    {
        void Stop();

        void Start();

        bool IsRunning
        {
            get;
        }

        bool CanPauseAndResume
        {
            get;
        }

        void Pause();

        void Resume();

        void AttachMonitor(ServerMonitor monitor);

        void DetachMonitor();
    }

    class InProcessServerHarness : IServerHarness
    {
        private HttpServer _server;
        private MonitorLogger _activeLogger;
        private String _logFormat;

        public InProcessServerHarness(HttpServer server, String logFormat)
        {
            this._server = server;
            this._logFormat = ApacheStyleLogger.GetLogFormat(logFormat);
        }

        #region IServerHarness Members

        public void Stop()
        {
            this._server.Stop();
        }

        public void Start()
        {
            this._server.Start();
        }

        public bool IsRunning
        {
            get
            {
                return this._server.IsRunning;
            }
        }

        public bool CanPauseAndResume
        {
            get 
            {
                return true; 
            }
        }

        public void Pause()
        {
            this.Stop();
        }

        public void Resume()
        {
            this.Start();
        }

        public void AttachMonitor(ServerMonitor monitor)
        {
            if (this._activeLogger == null)
            {
                this._activeLogger = new MonitorLogger(monitor, this._logFormat);
                this._server.AddLogger(this._activeLogger);
            }
        }

        public void DetachMonitor()
        {
            if (this._activeLogger != null)
            {
                this._server.RemoveLogger(this._activeLogger);
            }
        }

        #endregion

        public override string ToString()
        {
            return "[In-Process] " + this._server.Host + ":" + this._server.Port + " - " + ((this.IsRunning) ? "Running" : "Stopped");
        }

        public void Dispose()
        {
            this.DetachMonitor();
            this.Stop();
            if (this._server != null) this._server.Dispose();
        }
    }

    class ExternalProcessServerHarness : IServerHarness
    {
        private ProcessStartInfo _info;
        private Process _process;
        private String _host;
        private int _port;
        private ConsoleCapture _activeLogger;

        public ExternalProcessServerHarness(ProcessStartInfo info)
        {
            this._info = info;
            Match m = Regex.Match(info.Arguments, @"-h(ost)? ([\w\-\.]+)");
            if (m.Success)
            {
                this._host = m.Groups[2].Value;
            }
            else
            {
                this._host = HttpServer.DefaultHost;
            }
            m = Regex.Match(info.Arguments, @"-p(ort)? (\d+)");
            if (m.Success)
            {
                this._port = Int32.Parse(m.Groups[2].Value);
            }
            else
            {
                this._port = RdfServerOptions.DefaultPort;
            }
        }

        #region IServerHarness Members

        public void Stop()
        {
            if (this._process != null)
            {
                if (this._activeLogger != null)
                {
                    this._activeLogger.Dispose();
                    this._activeLogger = null;
                }
                this._process.Kill();
                this._process = null;
            }
        }

        public void Start()
        {
            if (this._process == null)
            {
                this._process = new Process();
                this._process.StartInfo = this._info;
                this._process.Start();
            }
        }

        public bool IsRunning
        {
            get
            {
                return (this._process != null && !this._process.HasExited);
            }
        }

        public bool CanPauseAndResume
        {
            get 
            {
                return false; 
            }
        }

        public void Pause()
        {
            throw new NotSupportedException("Pause not supported by external process servers");
        }

        public void Resume()
        {
            throw new NotSupportedException("Resume not supported by external process servers");
        }

        public void AttachMonitor(ServerMonitor monitor)
        {
            if (this._process != null)
            {
                if (this._activeLogger == null)
                {
                    this._activeLogger = new ConsoleCapture(this._process, monitor);
                }
            }
        }

        public void DetachMonitor()
        {
            if (this._activeLogger != null)
            {
                this._activeLogger.Dispose();
                this._activeLogger = null;
            }
        }

        #endregion

        public override string ToString()
        {
            return "[External Process] " + this._host + ":" + this._port + " - " + ((this.IsRunning) ? "Running" : "Stopped");
        }

        public void Dispose()
        {
            this.DetachMonitor();
            this._process = null;
        }
    }
}
