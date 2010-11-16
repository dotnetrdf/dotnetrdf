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
                    EventLog.WriteEntry("rdfServer Started with options\n" + this._options.ToString());
                    options = this._options;
                }
                try 
                {
                    this._server = options.GetServerInstance();

                    EventLog.WriteEntry("HttpServer Instance for rdfServer created OK");
                } 
                catch (Exception ex)
                {
                    EventLog.WriteEntry("Failed to create the Server instance due to the error " + ex.Message);
                    throw;
                }
            }

            try
            {
                this._server.Start();

                EventLog.WriteEntry("HttpServer is " + ((this._server.IsRunning) ? "running" : "not running"));
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("HttpServer Failed to Start due to the error " + ex.Message);
                throw;
            }
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
