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
