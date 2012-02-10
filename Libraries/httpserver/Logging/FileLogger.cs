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
using System.IO;
using System.Threading;

namespace VDS.Web.Logging
{
    /// <summary>
    /// An Apache style Logger which logs to files
    /// </summary>
    public class FileLogger
        : ApacheStyleLogger
    {
        private String _logFile;
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Creates a new Logger
        /// </summary>
        /// <param name="logFile">Log File</param>
        public FileLogger(String logFile)
            : this(logFile, LogCommon) { }

        /// <summary>
        /// Creates a new Logger
        /// </summary>
        /// <param name="logFile">Log File</param>
        /// <param name="logFormatString">Log Format</param>
        public FileLogger(String logFile, String logFormatString)
            : base(logFormatString)
        {
            if (logFile == null) throw new ArgumentNullException("logFile", "Log File cannot be null");
            if (!File.Exists(logFile))
            {
                try
                {
                    this._lock.EnterWriteLock();
                    FileStream file = File.Create(logFile);
                    file.Close();
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Unable to create the Log File '" + logFile + "'", "logFile", ex);
                }
                finally
                {
                    this._lock.ExitWriteLock();
                }
            }
            this._logFile = logFile;
        }

        /// <summary>
        /// Appends log lines to a file
        /// </summary>
        /// <param name="line">Log Line</param>
        protected override void AppendToLog(string line)
        {
            try
            {
                this._lock.EnterWriteLock();
                using (StreamWriter writer = new StreamWriter(this._logFile, true))
                {
                    writer.WriteLine(line);
                    writer.Close();
                }
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }
    }

    /// <summary>
    /// An Apache style Logger which logs to a stream
    /// </summary>
    public class StreamLogger
        : ApacheStyleLogger
    {
        private StreamWriter _writer;
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        /// <summary>
        /// Creates a new logger
        /// </summary>
        /// <param name="writer">Stream Writer</param>
        /// <param name="logFormatString">Log Format</param>
        public StreamLogger(StreamWriter writer, String logFormatString)
            : base(logFormatString)
        {
            if (writer == null) throw new ArgumentNullException("Cannot create a Stream Logger that uses a null StreamWriter");
            this._writer = writer;
        }

        /// <summary>
        /// Creates a new logger
        /// </summary>
        /// <param name="writer">Stream Writer</param>
        public StreamLogger(StreamWriter writer)
            : this(writer, LogCommon) { }

        /// <summary>
        /// Creates a new logger
        /// </summary>
        /// <param name="stream">Stream</param>
        public StreamLogger(Stream stream)
            : this(new StreamWriter(stream)) { }

        /// <summary>
        /// Creates a new logger
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="logFormatString">Log Format</param>
        public StreamLogger(Stream stream, String logFormatString)
            : this(new StreamWriter(stream), logFormatString) { }

        /// <summary>
        /// Appends log lines to the stream
        /// </summary>
        /// <param name="line">Log line</param>
        protected override void AppendToLog(string line)
        {
            try
            {
                this._lock.EnterWriteLock();
                this._writer.WriteLine(line);
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Disposes of the logger
        /// </summary>
        public override void Dispose()
        {
            this._writer.Close();
            base.Dispose();
        }
    }

    /// <summary>
    /// An Apache style Logger which logs to the console
    /// </summary>
    public class ConsoleLogger 
        : ApacheStyleLogger, IExtendedHttpLogger
    {
        /// <summary>
        /// Creates a new Logger
        /// </summary>
        /// <param name="logFormatString">Log Format</param>
        public ConsoleLogger(String logFormatString)
            : base(logFormatString) { }

        /// <summary>
        /// Creates a new Logger using Common Log format
        /// </summary>
        public ConsoleLogger()
            : this(LogCommon) { }

        /// <summary>
        /// Appends log lines to the console
        /// </summary>
        /// <param name="line">Log line</param>
        protected override void AppendToLog(string line)
        {
            Console.WriteLine(line);
        }

        /// <summary>
        /// Logs errors to the console
        /// </summary>
        /// <param name="ex">Error</param>
        public void LogError(Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex.StackTrace);
            if (ex.InnerException != null)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("Inner Exception:");
                this.LogError(ex.InnerException);
            }
        }
    }
}
