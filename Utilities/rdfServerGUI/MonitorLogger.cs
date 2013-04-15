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
using VDS.Web;
using VDS.Web.Consoles;
using VDS.Web.Logging;

namespace VDS.RDF.Utilities.Server.GUI
{
    /// <summary>
    /// A logger to server monitor adaptor
    /// </summary>
    class MonitorLogger
        : ApacheStyleLogger
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
    }

    /// <summary>
    /// A console to server monitor adaptor
    /// </summary>
    class MonitorConsole
        : BaseConsole
    {
        private ServerMonitor _monitor;

        public MonitorConsole(ServerMonitor monitor)
        {
            this._monitor = monitor;
        }

        protected override void PrintError(string message, Exception e)
        {
            this.PrintError(message + "\n" + this.GetFullStackTrace(e));
        }

        protected override void PrintError(string message)
        {
            this._monitor.WriteLine(message);
        }

        protected override void PrintInformation(string message)
        {
            this._monitor.WriteLine(message);
        }

        protected override void PrintWarning(string message, Exception e)
        {
            this.PrintWarning(message + "\n" + this.GetFullStackTrace(e));
        }

        protected override void PrintWarning(string message)
        {
            this._monitor.WriteLine(message);
        }

        public override void Dispose()
        {
            //Do nothing
        }
    }
}
