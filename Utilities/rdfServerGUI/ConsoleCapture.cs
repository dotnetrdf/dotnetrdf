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
using System.Diagnostics;

namespace VDS.RDF.Utilities.Server.GUI
{
    /// <summary>
    /// Class for capturing the output of a out of process rdfServer
    /// </summary>
    class ConsoleCapture 
        : IDisposable
    {
        private Process _process;
        private ServerMonitor _monitor;

        private DataReceivedEventHandler _errorHandler, _outputHandler;

        /// <summary>
        /// Creates a new Console Capture
        /// </summary>
        /// <param name="p">Process</param>
        /// <param name="monitor">Server Monitor which wishes to receive the capture</param>
        public ConsoleCapture(Process p, ServerMonitor monitor)
        {
            this._process = p;
            this._monitor = monitor;

            this._errorHandler = new DataReceivedEventHandler(this.HandleError);
            this._outputHandler = new DataReceivedEventHandler(this.HandleOutput);

            if (!this._process.StartInfo.UseShellExecute && this._process.StartInfo.RedirectStandardError && this._process.StartInfo.RedirectStandardOutput)
            {
                this._process.BeginErrorReadLine();
                this._process.BeginOutputReadLine();
                this._process.ErrorDataReceived += this._errorHandler;
                this._process.OutputDataReceived += this._outputHandler;
            }
            else
            {
                monitor.WriteLine("Cannot monitor a process that was not started in the current rdfServerGUI session but you may still stop this server");
            }
        }

        /// <summary>
        /// Handles Errors
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        private void HandleError(Object sender, DataReceivedEventArgs args)
        {
            this._monitor.WriteLine(args.Data);
        }

        /// <summary>
        /// Handles Output
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Arguments</param>
        private void HandleOutput(Object sender, DataReceivedEventArgs args)
        {
            this._monitor.WriteLine(args.Data);
        }

        /// <summary>
        /// Disposes of the capture ending any active capture behaviour
        /// </summary>
        public void Dispose()
        {
            if (this._process != null)
            {
                if (!this._process.StartInfo.UseShellExecute && this._process.StartInfo.RedirectStandardError && this._process.StartInfo.RedirectStandardOutput)
                {
                    this._process.CancelErrorRead();
                    this._process.CancelOutputRead();
                    this._process.ErrorDataReceived -= this._errorHandler;
                    this._process.OutputDataReceived -= this._outputHandler;
                }
                this._process = null;
            }
        }
    }
}
