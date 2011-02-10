using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace VDS.Web.Logging
{
    public interface IHttpLogger : IDisposable
    {
        void LogRequest(HttpServerContext context);
    }

    public interface IExtendedHttpLogger : IHttpLogger
    {
        void LogError(Exception ex);
    }
}
