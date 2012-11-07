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
using System.Linq;
using System.Text;

namespace VDS.Web.Logging
{
    /// <summary>
    /// An Apache Style logger that logs to the System Event Log
    /// </summary>
    public class EventLogger 
        : ApacheStyleLogger, IExtendedHttpLogger
    {
        private String _source;

        /// <summary>
        /// Creates a new Event Logger
        /// </summary>
        /// <param name="source">Source</param>
        public EventLogger(String source)
            : this(source, ApacheStyleLogger.LogCommon) { }

        /// <summary>
        /// Creates a new Event Logger
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="format">Log Format</param>
        public EventLogger(String source, String format)
            : base(format) 
        {
            this._source = source;
        }

        /// <summary>
        /// Appends log lines to the System Event Log
        /// </summary>
        /// <param name="line">Log Line</param>
        protected override void AppendToLog(string line)
        {
            System.Diagnostics.EventLog.WriteEntry(this._source, line);
        }

        /// <summary>
        /// Logs erros to the System Event Log
        /// </summary>
        /// <param name="ex">Error</param>
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
