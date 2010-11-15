using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace VDS.Web
{
    /// <summary>
    /// A Wrapper for a <see cref="HttpServer">HttpServer</see> to use it as a service.  Server must be started via command line arguments of the form <em>host port configFile</em> or the default settings of localhost, port 80 and server.config will be used
    /// </summary>
    partial class HttpServerService : ServiceBase
    {
        private HttpServer _server;

        public HttpServerService(String name)
        {
            InitializeComponent();
            this.ServiceName = name;
            this.CanShutdown = true;
            this.CanStop = true;
            this.CanPauseAndContinue = true;
        }

        protected override void OnStart(string[] args)
        {
            if (this._server == null)
            {
                if (args.Length >= 3)
                {
                    this._server = new HttpServer(args[0], Int32.Parse(args[1]), args[2], true);
                }
                else
                {
                    this._server = new HttpServer(HttpServer.DefaultHost, HttpServer.DefaultPort, HttpServer.DefaultConfigurationFile, true);
                }
            }
            this._server.Start();
        }

        protected override void OnStop()
        {
            if (this._server.IsRunning)
            {
                this._server.Stop();
            }
            this._server.Dispose();
            this._server = null;
        }

        protected override void OnPause()
        {
            if (this._server != null)
            {
                this._server.Stop();
            }
        }

        protected override void OnContinue()
        {
            if (this._server != null)
            {
                this._server.Start();
            }
        }
    }
}
