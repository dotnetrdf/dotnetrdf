using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace VDS.Web
{
    /// <summary>
    /// Interface for Handlers which handle requests made to a <see cref="HttpListener">HttpListener</see>
    /// </summary>
    public interface IHttpListenerHandler
    {
        /// <summary>
        /// Gets whether the Handlers can be reused
        /// </summary>
        bool IsReusable
        {
            get;
        }

        /// <summary>
        /// Processes a HTTP Request
        /// </summary>
        /// <param name="context">HTTP Listener Context</param>
        /// <param name="server">HTTP Server</param>
        void ProcessRequest(HttpListenerContext context, HttpServer server);
    }
}
