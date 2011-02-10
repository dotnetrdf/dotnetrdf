using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace VDS.Web.Logging
{
    public class FileLogger : ApacheStyleLogger
    {
        private String _logFile;
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public FileLogger(String logFile)
            : this(logFile, LogCommon) { }

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

    public class StreamLogger : ApacheStyleLogger
    {
        private StreamWriter _writer;
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public StreamLogger(StreamWriter writer, String logFormatString)
            : base(logFormatString)
        {
            if (writer == null) throw new ArgumentNullException("Cannot create a Stream Logger that uses a null StreamWriter");
            this._writer = writer;
        }

        public StreamLogger(StreamWriter writer)
            : this(writer, LogCommon) { }

        public StreamLogger(Stream stream)
            : this(new StreamWriter(stream)) { }

        public StreamLogger(Stream stream, String logFormatString)
            : this(new StreamWriter(stream), logFormatString) { }

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

        public override void Dispose()
        {
            this._writer.Close();
            base.Dispose();
        }
    }

    public class ConsoleLogger : ApacheStyleLogger, IExtendedHttpLogger
    {
        public ConsoleLogger(String logFormatString)
            : base(logFormatString) { }

        public ConsoleLogger()
            : this(LogCommon) { }

        protected override void AppendToLog(string line)
        {
            Console.WriteLine(line);
        }

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
