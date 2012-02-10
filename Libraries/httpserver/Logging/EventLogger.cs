/*

Copyright Robert Vesse 2009-12
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

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
