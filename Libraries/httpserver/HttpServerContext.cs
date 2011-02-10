using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Principal;
using System.Text;

namespace VDS.Web
{
    public class HttpServerContext
    {
        private HttpServer _server;
        private HttpListenerContext _context;

        public HttpServerContext(HttpServer server, HttpListenerContext context)
        {
            this._server = server;
            this._context = context;
        }

        public HttpServer Server
        {
            get
            {
                return this._server;
            }
        }

        public HttpListenerRequest Request
        {
            get
            {
                return this._context.Request;
            }
        }

        public HttpListenerResponse Response
        {
            get
            {
                return this._context.Response;
            }
        }

        public IPrincipal User
        {
            get
            {
                return this._context.User;
            }
        }

        internal HttpListenerContext InnerContext
        {
            get
            {
                return this._context;
            }
        }
    }
}
