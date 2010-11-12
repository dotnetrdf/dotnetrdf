using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VDS.Web.Logging
{
    public class FileLogger : ApacheStyleLogger
    {
        private String _logFile;

        public FileLogger(String logFile)
            : this(logFile, CommonLogFormat) { }

        public FileLogger(String logFile, String logFormatString)
            : base(logFormatString)
        {
            if (logFile == null) throw new ArgumentNullException("logFile", "Log File cannot be null");
            if (!File.Exists(logFile))
            {
                try
                {
                    File.Create(logFile);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Unable to create the Log File '" + logFile + "'", "logFile", ex);
                }
            }
            this._logFile = logFile;
        }

        protected override void AppendToLog(string line)
        {
            using (StreamWriter writer = new StreamWriter(this._logFile, true))
            {
                writer.WriteLine(line);
                writer.Close();
            }
        }
    }

    public class StreamLogger : ApacheStyleLogger
    {
        private StreamWriter _writer;

        public StreamLogger(StreamWriter writer, String logFormatString)
            : base(logFormatString)
        {
            if (writer == null) throw new ArgumentNullException("Cannot create a Stream Logger that uses a null StreamWriter");
            this._writer = writer;
        }

        public StreamLogger(StreamWriter writer)
            : this(writer, CommonLogFormat) { }

        public StreamLogger(Stream stream)
            : this(new StreamWriter(stream)) { }

        public StreamLogger(Stream stream, String logFormatString)
            : this(new StreamWriter(stream), logFormatString) { }

        protected override void AppendToLog(string line)
        {
            this._writer.WriteLine(line);
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
            : this(CommonLogFormat) { }

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
                Console.WriteLine();
                Console.WriteLine("Inner Exception:");
                this.LogError(ex.InnerException);
            }
        }
    }
}
