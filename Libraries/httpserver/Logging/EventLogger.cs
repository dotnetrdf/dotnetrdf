using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.Web.Logging
{
    public class EventLogger : ApacheStyleLogger
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
    }
}
