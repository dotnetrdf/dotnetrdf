using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace VDS.Web.Logging
{
    public abstract class ApacheStyleLogger : IHttpLogger
    {
        private String _formatString;
        private String[] _logParts;

        public const String ClfDateFormat = "[dd/MMM/yyyy:hh:mm:ss zzz]";
        public const String CommonLogFormat = "%h %l %u %t \"%r\" %>s %b";
        public const String CombinedLogFormat = CommonLogFormat + " \"%{Referer}i\" \"%{User-Agent}i\"";
        public const String LogAll = "%a %A %B %b %h %H %l %m %p %P %q \"%r\" %s %>s %t %U \"%u\" \"%{Referer}i \"%{User}i\" \"%{Accept}i \"%{Content-Type}o";

        public ApacheStyleLogger(String logFormatString)
        {
            if (logFormatString == null || logFormatString.Equals(String.Empty))
            {
                this._formatString = CommonLogFormat;
            }
            else
            {
                this._formatString = logFormatString;
            }
            if (this._formatString.Contains(' '))
            {
                this._logParts = this._formatString.Split(' ');
            }
            else
            {
                this._logParts = new String[] { this._formatString };
            }
        }

        public void LogRequest(HttpServerContext context)
        {
            StringBuilder logLine = new StringBuilder();
            String logPart;
            String logItem;
            for (int i = 0; i < this._logParts.Length; i++)
            {
                logPart = this._logParts[i];
                if (logPart.Contains("%"))
                {
                    int pos = logPart.IndexOf('%');
                    logLine.Append(logPart.Substring(0, pos));
                    logItem = logPart.Substring(pos);
                    if (logItem.Length > 1)
                    {
                        logItem = logItem.Substring(0, 2);
                    }

                    switch (logItem)
                    {
                        case "%%":
                            logLine.Append('%');
                            break;
                        case "%a":
                            logLine.Append(context.Request.RemoteEndPoint.Address.ToLogString());
                            break;
                        case "%A":
                            logLine.Append(context.Request.LocalEndPoint.Address.ToLogString());
                            break;
                        case "%B":
                            logLine.Append(context.Response.ContentLength64.ToString());
                            break;
                        case "%b":
                            if (context.Response.ContentLength64 > 0)
                            {
                                logLine.Append(context.Response.ContentLength64.ToString());
                            }
                            else
                            {
                                logLine.Append('-');
                            }
                            break;
                        case "%h":
                            logLine.Append(context.Request.UserHostName.ToLogString());
                            break;
                        case "%H":
                            logLine.Append("HTTP/");
                            logLine.Append(context.Request.ProtocolVersion.Major + "." + context.Request.ProtocolVersion.Minor);
                            break;
                        case "%l":
                            if (context.User != null)
                            {
                                logLine.Append(context.User.Identity.Name);
                            }
                            else
                            {
                                logLine.Append('-');
                            }
                            break;
                        case "%m":
                            logLine.Append(context.Request.HttpMethod);
                            break;
                        case "%p":
                            logLine.Append(context.Request.RemoteEndPoint.Port.ToString());
                            break;
                        case "%P":
                            logLine.Append(Process.GetCurrentProcess().Id.ToString());
                            break;
                        case "%q":
                            if (!context.Request.Url.Query.Equals(String.Empty))
                            {
                                logLine.Append(context.Request.Url.Query.ToString());
                            }
                            break;
                        case "%r":
                            logLine.Append(context.Request.HttpMethod);
                            logLine.Append(' ');
                            logLine.Append(context.Request.RawUrl);
                            if (!context.Request.Url.Query.Equals(String.Empty))
                            {
                                logLine.Append(context.Request.Url.Query);
                            }
                            logLine.Append(" HTTP/");
                            logLine.Append(context.Request.ProtocolVersion.Major + "." + context.Request.ProtocolVersion.Minor);
                            break;
                        case "%>":
                            if (logPart.Length > pos + logItem.Length)
                            {
                                logItem += logPart[pos + logItem.Length];
                                if (logItem.Equals("%>s"))
                                {
                                    logLine.Append(context.Response.StatusCode.ToString());
                                }
                                else
                                {
                                    logLine.Append(logItem);
                                }
                            }
                            else
                            {
                                logLine.Append(logItem);
                            }
                            break;
                        case "%s":
                            logLine.Append(context.Response.StatusCode.ToString());
                            break;                           
                        case "%t":
                            logLine.Append(DateTime.Now.ToString(ClfDateFormat));
                            break;
                        case "%u":
                            if (context.User != null)
                            {
                                logLine.Append(context.User.Identity.Name);
                            }
                            else
                            {
                                logLine.Append('-');
                            }
                            break;
                        case "%U":
                            logLine.Append(context.Request.RawUrl);
                            break;

                        case "%{":
                            if (logPart.Length > pos + logItem.Length)
                            {
                                logItem = logPart.Substring(pos, logPart.IndexOf('}', pos) + 1 - pos);
                                if (logPart.Length > pos + logItem.Length)
                                {
                                    char next = logPart[pos + logItem.Length];
                                    String header = logItem.Substring(logItem.IndexOf('{') + 1, logItem.IndexOf('}', logItem.IndexOf('{')) - logItem.IndexOf('{') - 1);
                                    if (next == 'i')
                                    {
                                        logItem = logItem + next;
                                        if (context.Request.Headers[header] != null)
                                        {
                                            logLine.Append(context.Request.Headers[header]);
                                        }
                                        else if (header.Equals("User-Agent"))
                                        {
                                            logLine.Append(context.Request.UserAgent);
                                        }
                                        else
                                        {
                                            logLine.Append('-');
                                        }
                                    }
                                    else if (next == 'o')
                                    {
                                        logItem = logItem + next;
                                        if (context.Response.Headers[header] != null)
                                        {
                                            logLine.Append(context.Response.Headers[header]);
                                        }
                                        else
                                        {
                                            logLine.Append('-');
                                        }
                                    }
                                    else
                                    {
                                        logLine.Append(logItem);
                                    }
                                }
                            }
                            else
                            {
                                logLine.Append(logItem);
                            }
                            break;

                        default:
                            logLine.Append(logItem);
                            break;
                    }

                    if (pos + logItem.Length < logPart.Length)
                    {
                        logLine.Append(logPart.Substring(pos + logItem.Length));
                    }
                }
                else
                {
                    logLine.Append(logPart);
                }
                if (i < this._logParts.Length - 1)
                {
                    logLine.Append(' ');
                }
            }

            this.AppendToLog(logLine.ToString());
        }

        protected abstract void AppendToLog(String line);

        public virtual void Dispose()
        {

        }
    }
}
