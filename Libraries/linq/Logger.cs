/* 
 * Copyright (C) 2007, Andrew Matthews http://aabs.wordpress.com/
 *
 * This file is Free Software and part of LinqToRdf http://code.google.com/fromName/linqtordf/
 *
 * It is licensed under the following license:
 *   - Berkeley License, V2.0 or any newer version
 *
 * You may not use this file except in compliance with the above license.
 *
 * See http://code.google.com/fromName/linqtordf/ for the complete text of the license agreement.
 *
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using log4net;
namespace VDS.RDF.Linq
{
	[DebuggerNonUserCode]
	public class Logger
    {
        private readonly ILog log;

// ReSharper disable UnusedPrivateMember
        private Logger()
// ReSharper restore UnusedPrivateMember
        {
        }

        public Logger(Type t)
        {
            log = LogManager.GetLogger(t);
        }

        #region Formatted Logging

        public void Debug(string message, params object[] args)
        {
            if (log.IsDebugEnabled) log.DebugFormat(message, args);
        }

        public void Info(string message, params object[] args)
        {
            if (log.IsInfoEnabled) log.InfoFormat(message, args);
        }

        public void Warn(string message, params object[] args)
        {
            if (log.IsWarnEnabled) log.WarnFormat(message, args);
        }

        public void Error(string message, params object[] args)
        {
            if (log.IsErrorEnabled) log.ErrorFormat(message, args);
        }

        public void Fatal(string message, params object[] args)
        {
            if (log.IsFatalEnabled) log.FatalFormat(message, args);
        }

        #endregion

        #region Exception Logging

        public void DebugEx(string message, Exception e)
        {
            if (log.IsDebugEnabled) log.Debug(message, e);
        }

        public void InfoEx(string message, Exception e)
        {
            if (log.IsInfoEnabled) log.Info(message, e);
        }

        public void WarnEx(string message, Exception e)
        {
            if (log.IsWarnEnabled) log.Warn(message, e);
        }

        public void ErrorEx(string message, Exception e)
        {
            if (log.IsErrorEnabled) log.Error(message, e);
        }

        public void FatalEx(string message, Exception e)
        {
            if (log.IsFatalEnabled) log.Fatal(message, e);
        }

        #endregion

		#region Log Switches
		public bool IsDebugEnabled
		{
			get { return log.IsDebugEnabled; }
		}
		public bool IsInfoEnabled
		{
			get { return log.IsInfoEnabled; }
		}
		public bool IsWarnEnabled
		{
			get { return log.IsWarnEnabled; }
		}
		public bool IsErrorEnabled
		{
			get { return log.IsErrorEnabled; }
		}
		public bool IsFatalEnabled
		{
			get { return log.IsFatalEnabled; }
		}
		#endregion
	}

    public class LoggingScope : Logger, IDisposable
    {
        public LoggingScope(string message, params object[] args)
            : base(new StackFrame(1).GetMethod().DeclaringType)
        {
            Message = message;
            Arguments = args;
            Debug("Start: " + Message, Arguments);
        }

        public string Message { get; set; }
        public object[] Arguments { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            Debug("End: " + Message, Arguments);
        }

        #endregion
    }

    public class LogWriter : TextWriter
    {
        private readonly Logger logger;

        public LogWriter(Logger logger)
        {
            this.logger = logger;
        }

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }

        public override void Write(string msg)
        {
            logger.Info(msg);
        }

        public override void WriteLine(string msg)
        {
            logger.Info(msg);
        }
    }
}