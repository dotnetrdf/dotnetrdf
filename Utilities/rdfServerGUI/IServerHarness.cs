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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using VDS.Web;
using VDS.Web.Consoles;
using VDS.Web.Logging;

namespace VDS.RDF.Utilities.Server.GUI
{
    /// <summary>
    /// Represents a harness around a server that can be used to control and monitor the server
    /// </summary>
    interface IServerHarness
        : IDisposable
    {
        /// <summary>
        /// Stop the Server
        /// </summary>
        void Stop();

        /// <summary>
        /// Start the Server
        /// </summary>
        void Start();

        /// <summary>
        /// Get whether the Server is running
        /// </summary>
        bool IsRunning
        {
            get;
        }

        /// <summary>
        /// Attaches a Monitor to the Server
        /// </summary>
        /// <param name="monitor">Monitor</param>
        /// <remarks>
        /// Only one monitor is permitted at any one time
        /// </remarks>
        void AttachMonitor(ServerMonitor monitor);

        /// <summary>
        /// Detaches the current monitor from the Server
        /// </summary>
        void DetachMonitor();
    }

    /// <summary>
    /// Server Harness for in-process servers
    /// </summary>
    class InProcessServerHarness 
        : IServerHarness
    {
        private HttpServer _server;
        private MonitorLogger _activeLogger;
        private MonitorConsole _activeConsole;
        private String _logFormat;

        /// <summary>
        /// Creates a new in-process harness
        /// </summary>
        /// <param name="server">Server</param>
        /// <param name="logFormat">Log Format</param>
        public InProcessServerHarness(HttpServer server, String logFormat)
        {
            this._server = server;
            this._logFormat = ApacheStyleLogger.GetLogFormat(logFormat);
        }

        #region IServerHarness Members

        /// <summary>
        /// Stops the Server
        /// </summary>
        public void Stop()
        {
            switch (this._server.State)
            {
                case ServerState.Running:
                case ServerState.Starting:
                    this._server.Stop();
                    break;
            }
        }

        /// <summary>
        /// Starts the Server
        /// </summary>
        public void Start()
        {
            switch (this._server.State)
            {
                case ServerState.Creating:
                case ServerState.Created:
                case ServerState.Stopping:
                case ServerState.Stopped:
                    this._server.Start();
                    break;
            }
        }

        /// <summary>
        /// Gets whether the Server is running
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return this._server.IsRunning;
            }
        }

        /// <summary>
        /// Attaches a Monitor to the Server
        /// </summary>
        /// <param name="monitor">Monitor</param>
        public void AttachMonitor(ServerMonitor monitor)
        {
            this.DetachMonitor();

            //Set up logger
            this._activeLogger = new MonitorLogger(monitor, this._logFormat);
            this._server.AddLogger(this._activeLogger);

            //Set up console
            this._activeConsole = new MonitorConsole(monitor);
            if (this._server.Console is MulticastConsole)
            {
                ((MulticastConsole)this._server.Console).AddConsole(this._activeConsole);
            }
            else
            {
                IServerConsole current = this._server.Console;
                IServerConsole multicast = new MulticastConsole(new IServerConsole[] { current, this._activeConsole });
                this._server.Console = multicast;
            }
        }

        /// <summary>
        /// Detaches the current monitor from the server
        /// </summary>
        public void DetachMonitor()
        {
            if (this._activeLogger != null)
            {
                this._server.RemoveLogger(this._activeLogger);
                this._activeLogger = null;
            }
            if (this._activeConsole != null)
            {
                if (this._server.Console is MulticastConsole)
                {
                    ((MulticastConsole)this._server.Console).RemoveConsole(this._activeConsole);
                }
                this._activeConsole = null;
            }
        }

        #endregion

        /// <summary>
        /// Gets the String representation of the Server
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[In-Process] " + this._server.Host + ":" + this._server.Port + " - " + ((this.IsRunning) ? "Running" : "Stopped");
        }

        /// <summary>
        /// Disposes of the harness and stops the server
        /// </summary>
        public void Dispose()
        {
            this.DetachMonitor();
            this.Stop();
            if (this._server != null) this._server.Dispose();
        }
    }

    /// <summary>
    /// Server Harness for out of process servers
    /// </summary>
    class ExternalProcessServerHarness 
        : IServerHarness
    {
        private ProcessStartInfo _info;
        private Process _process;
        private String _host;
        private int _port;
        private ConsoleCapture _activeLogger;

        /// <summary>
        /// Creates a new out of process Server harness
        /// </summary>
        /// <param name="info">Process Start Info</param>
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

        /// <summary>
        /// Creates a new out of process Server harness
        /// </summary>
        /// <param name="info">Process Start Info</param>
        /// <param name="process">Process</param>
        public ExternalProcessServerHarness(ProcessStartInfo info, Process process)
            : this(info)
        {
            this._process = process;
        }

        #region IServerHarness Members

        /// <summary>
        /// Stops the Server
        /// </summary>
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

        /// <summary>
        /// Starts the Server
        /// </summary>
        public void Start()
        {
            if (this._process == null)
            {
                //Ensure Start Info is correct
                this._info.UseShellExecute = false;
                this._info.RedirectStandardError = true;
                this._info.RedirectStandardOutput = true;
                this._info.CreateNoWindow = true;

                this._process = new Process();
                this._process.StartInfo = this._info;
                this._process.Start();
            }
        }

        /// <summary>
        /// Gets whether the Server is running
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return (this._process != null && !this._process.HasExited);
            }
        }

        /// <summary>
        /// Attaches a Monitor to the server
        /// </summary>
        /// <param name="monitor">Monitor</param>
        public void AttachMonitor(ServerMonitor monitor)
        {
            if (this._process != null)
            {
                this.DetachMonitor();
                if (this._activeLogger == null)
                {
                    this._activeLogger = new ConsoleCapture(this._process, monitor);
                }
            }
        }

        /// <summary>
        /// Detaches the current monitor from the server
        /// </summary>
        public void DetachMonitor()
        {
            if (this._activeLogger != null)
            {
                this._activeLogger.Dispose();
                this._activeLogger = null;
            }
        }

        #endregion

        /// <summary>
        /// Gets the string representation of the server
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[External Process] " + this._host + ":" + this._port + " - " + ((this.IsRunning) ? "Running" : "Stopped");
        }

        /// <summary>
        /// Disposes of the harness but leaves the external server running
        /// </summary>
        public void Dispose()
        {
            this.DetachMonitor();
            this._process = null;
        }
    }
}
