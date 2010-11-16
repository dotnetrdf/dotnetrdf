using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.Web.Logging
{
    public class EventLogger : ApacheStyleLogger, IExtendedHttpLogger
    {
        private String _source;

        public EventLogger(String source)
            : this(source, ApacheStyleLogger.LogCommon) { }

        public EventLogger(String source, String format)
            : base(format) 
        {
            this._source = source;
        }

        protected override void AppendToLog(string line)
        {
            System.Diagnostics.EventLog.WriteEntry(this._source, line);
        }

        public void LogError(Exception ex)
        {
            StringBuilder output = new StringBuilder();

            output.AppendLine(ex.Message);
            output.AppendLine(ex.StackTrace);
            while (ex.InnerException != null)
            {
                output.AppendLine();
                output.AppendLine("Inner Exception:");
                output.AppendLine(ex.InnerException.Message);
                output.AppendLine(ex.InnerException.StackTrace);
                ex = ex.InnerException;
            }

            System.Diagnostics.EventLog.WriteEntry(this._source, output.ToString(), System.Diagnostics.EventLogEntryType.Error);
        }
    }
}
