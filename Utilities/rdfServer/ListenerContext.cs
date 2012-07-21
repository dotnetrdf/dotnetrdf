/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Text;
using VDS.RDF.Web;
using VDS.Web;

namespace VDS.RDF.Utilities.Server
{
    public class ServerContext
        : IHttpContext
    {
        private HttpServerContext _context;
        private IHttpProtocolRequest _request;
        private IHttpProtocolResponse _response;

        public ServerContext(HttpServerContext context)
        {
            this._context = context;
            this._request = new ServerRequest(context.Request);
            this._response = new ServerResponse(context.Response);
        }

        public IHttpProtocolRequest Request
        {
            get
            {
                return this._request;
            }
        }

        public IHttpProtocolResponse Response
        {
            get
            { 
                return this._response; 
            }
        }

        public IPrincipal User
        {
            get 
            {
                return this._context.User;
            }
        }
    }

    class ServerRequest
        : IHttpProtocolRequest
    {
        private HttpListenerRequest _request;

        public ServerRequest(HttpListenerRequest request)
        {
            this._request = request;
        }

        public string[] AcceptTypes
        {
            get 
            {
                return this._request.AcceptTypes;
            }
        }

        public long ContentLength
        {
            get 
            {
                return this._request.ContentLength64;
            }
        }

        public string ContentType
        {
            get 
            { 
                return this._request.ContentType;
            }
        }

        public NameValueCollection Headers
        {
            get
            { 
                return this._request.Headers;
            }
        }

        public string HttpMethod
        {
            get 
            {
                return this._request.HttpMethod; 
            }
        }

        public Stream InputStream
        {
            get 
            {
                return this._request.InputStream;
            }
        }

        public NameValueCollection QueryString
        {
            get
            { 
                return this._request.QueryString;
            }
        }

        public Uri Url
        {
            get
            { 
                return this._request.Url;
            }
        }

        public string UserHostAddress
        {
            get 
            {
                return this._request.UserHostAddress;
            }
        }
    }

    class ServerResponse
        : IHttpProtocolResponse
    {
        private HttpListenerResponse _response;
        private TextWriter _writer;

        public ServerResponse(HttpListenerResponse response)
        {
            this._response = response;
        }

        public void AddHeader(string header, string value)
        {
            this._response.AddHeader(header, value);
        }

        public void Clear()
        {
            //Do nothing
        }

        public void Write(string data)
        {
            if (this._writer == null) this._writer = new StreamWriter(this._response.OutputStream, this.ContentEncoding);
            this._writer.Write(data);
        }

        public Encoding ContentEncoding
        {
            get
            {
                return this._response.ContentEncoding;
            }
            set
            {
                this._response.ContentEncoding = value;
            }
        }

        public string ContentType
        {
            get
            {
                return this._response.ContentType;
            }
            set
            {
                this._response.ContentType = value;
            }
        }

        public NameValueCollection Headers
        {
            get
            {
                return this._response.Headers;
            }
        }

        public Stream OutputStream
        {
            get
            { 
                return this._response.OutputStream;
            }
        }

        public int StatusCode
        {
            get
            {
                return this._response.StatusCode;
            }
            set
            {
                this._response.StatusCode = value;
            }
        }
    }
}
