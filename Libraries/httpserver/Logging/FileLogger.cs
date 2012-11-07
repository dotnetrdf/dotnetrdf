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
