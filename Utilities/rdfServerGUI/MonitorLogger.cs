using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.Web;
using VDS.Web.Logging;

namespace rdfServer.GUI
{
    class MonitorLogger : ApacheStyleLogger, IExtendedHttpLogger
    {
        private ServerMonitor _monitor;

        public MonitorLogger(ServerMonitor monitor, String logFormat)
            : base(logFormat)
        {
            this._monitor = monitor;
        }

        public MonitorLogger(ServerMonitor monitor)
            : this(monitor, ApacheStyleLogger.LogCommon) { }

        protected override void AppendToLog(string line)
        {
            this._monitor.WriteLine(line);
        }

        public void LogError(Exception ex)
        {
            this._monitor.WriteLine(ex.Message);
            this._monitor.WriteLine(ex.StackTrace);
            while (ex.InnerException != null)
            {
                this._monitor.WriteLine("Inner Exception:");
                this._monitor.WriteLine(ex.InnerException.Message);
                this._monitor.WriteLine(ex.InnerException.StackTrace);
                ex = ex.InnerException;
            }
        }
    }
}
