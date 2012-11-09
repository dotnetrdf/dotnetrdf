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

using System.Net;
using System.Security.Principal;

namespace VDS.Web
{
    /// <summary>
    /// Represents the Server Context for a request
    /// </summary>
    public class HttpServerContext
    {
        private HttpServer _server;
        private HttpListenerContext _context;

        /// <summary>
        /// Creates a new Server Context
        /// </summary>
        /// <param name="server">Server</param>
        /// <param name="context">Listener Context</param>
        public HttpServerContext(HttpServer server, HttpListenerContext context)
        {
            this._server = server;
            this._context = context;
        }

        /// <summary>
        /// Gets the Server processing this request
        /// </summary>
        public HttpServer Server
        {
            get
            {
                return this._server;
            }
        }

        /// <summary>
        /// Gets the request
        /// </summary>
        public HttpListenerRequest Request
        {
            get
            {
                return this._context.Request;
            }
        }

        /// <summary>
        /// Gets the response
        /// </summary>
        public HttpListenerResponse Response
        {
            get
            {
                return this._context.Response;
            }
        }

        /// <summary>
        /// Gets the User
        /// </summary>
        public IPrincipal User
        {
            get
            {
                return this._context.User;
            }
        }

        /// <summary>
        /// Gets the Listener Context
        /// </summary>
        internal HttpListenerContext InnerContext
        {
            get
            {
                return this._context;
            }
        }
    }
}
