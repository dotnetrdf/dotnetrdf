/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using VDS.Web;
using VDS.Web.Logging;

namespace VDS.RDF.Utilities.Server.GUI
{
    /// <summary>
    /// A logger to server monitor adaptor
    /// </summary>
    class MonitorLogger
        : ApacheStyleLogger, IExtendedHttpLogger
    {
        private ServerMonitor _monitor;

        /// <summary>
        /// Creates a new logger
        /// </summary>
        /// <param name="monitor">Monitor</param>
        /// <param name="logFormat">Log Format</param>
        public MonitorLogger(ServerMonitor monitor, String logFormat)
            : base(logFormat)
        {
            this._monitor = monitor;
        }

        /// <summary>
        /// Creates a new logger
        /// </summary>
        /// <param name="monitor">Monitor</param>
        public MonitorLogger(ServerMonitor monitor)
            : this(monitor, ApacheStyleLogger.LogCommon) { }

        /// <summary>
        /// Appends log lines to the monitor
        /// </summary>
        /// <param name="line">Log line</param>
        protected override void AppendToLog(string line)
        {
            this._monitor.WriteLine(line);
        }

        /// <summary>
        /// Logs errors
        /// </summary>
        /// <param name="ex">Error</param>
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
