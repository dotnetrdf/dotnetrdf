using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using VDS.Web;

namespace rdfServer
{
    partial class RdfServerService : ServiceBase
    {
        private HttpServer _server;
        private RdfServerOptions _options;
        private String[] _args = new String[] { };

        public RdfServerService(String name)
        {
            InitializeComponent();
            this.ServiceName = name;
            this.CanShutdown = true;
            this.CanStop = true;
            this.CanPauseAndContinue = true;
        }

        public RdfServerService()
            : this(RdfServerOptions.DefaultServiceName) { }
        
        protected override void OnStart(string[] args)
        {
            if (args.Length == 0) args = this._args;
            if (this._server == null)
            {
                RdfServerOptions options;
                if (this._options == null)
                {
                    EventLog.WriteEntry("rdfServer Started with arguments:\n" + String.Join(" ", args));
                    options = new RdfServerOptions(args);
                }
                else
                {
                    EventLog.WriteEntry("rdfServer Started with options");
                    options = this._options;
                }
                this._server = options.GetServerInstance();
            }
            this._server.Start();
        }

        public String[] StartupArguments
        {
            set
            {
                if (value != null) this._args = value;
            }
        }

        public RdfServerOptions StartupOptions
        {
            set
            {
                this._options = value;
            }
        }

        protected override void OnStop()
        {
            if (this._server != null)
            {
                this._server.Stop();
            }
        }

        protected override void OnPause()
        {
            this.OnStop();
        }

        protected override void OnContinue()
        {
            this.OnStart(new String[] { });
        }

        protected override void OnShutdown()
        {
            if (this._server != null) this._server.Dispose();
            this._server = null;
            base.OnShutdown();
        }
    }
}
