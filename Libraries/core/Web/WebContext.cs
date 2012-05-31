/*

Copyright Robert Vesse 2012
rvesse@vdesign-studios.com

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

#if !NO_WEB && !NO_ASP

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;

namespace VDS.RDF.Web
{
    /// <summary>
    /// Implementation of <see cref="IHttpContext"/> which wraps the standard System.Web <see cref="HttpContext"/>
    /// </summary>
    public class WebContext
        : IHttpContext
    {
        private HttpContext _context;
        private IHttpProtocolRequest _request;
        private IHttpProtocolResponse _response;

        public WebContext(HttpContext context)
        {
            this._context = context;
            this._request = new WebRequest(context);
            this._response = new WebResponse(context);
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

    class WebRequest
        : IHttpProtocolRequest
    {
        private HttpContext _context;

        public WebRequest(HttpContext context)
        {
            this._context = context;
        }

        public String[] AcceptTypes
        {
            get
            {
                return this._context.Request.AcceptTypes;
            }
        }

        public long ContentLength
        {
            get 
            {
                return this._context.Request.ContentLength; 
            }
        }

        public string ContentType
        {
            get 
            {
                return this._context.Request.ContentType;
            }
        }

        public NameValueCollection Headers
        {
            get
            {
                return this._context.Request.Headers;
            }
        }

        public String HttpMethod
        {
            get
            {
                return this._context.Request.HttpMethod;
            }
        }

        public Stream InputStream
        {
            get 
            { 
                return this._context.Request.InputStream;
            }
        }

        public NameValueCollection QueryString
        {
            get
            {
                return this._context.Request.QueryString;
            }
        }

        public Uri Url
        {
            get 
            {
                return this._context.Request.Url;
            }
        }

        public string UserHostAddress
        {
            get 
            {
                return this._context.Request.UserHostAddress; 
            }
        }
    }

    class WebResponse
        : IHttpProtocolResponse
    {
        private HttpContext _context;

        public WebResponse(HttpContext context)
        {
            this._context = context;
        }

        public void AddHeader(string header, string value)
        {
            this._context.Response.AddHeader(header, value);
        }

        public void Clear()
        {
            this._context.Response.Clear();
        }

        public void Write(String data)
        {
            this._context.Response.Write(data);
        }

        public Encoding ContentEncoding
        {
            get
            {
                return this._context.Response.ContentEncoding;
            }
            set
            {
                this._context.Response.ContentEncoding = value;
            }
        }

        public string ContentType
        {
            get
            {
                return this._context.Response.ContentType;
            }
            set
            {
                this._context.Response.ContentType = value;
            }
        }

        public NameValueCollection Headers
        {
            get 
            { 
                return this._context.Response.Headers;    
            }
        }

        public Stream OutputStream
        {
            get 
            {
                return this._context.Response.OutputStream;
            }
        }

        public int StatusCode
        {
            get
            {
                return this._context.Response.StatusCode;
            }
            set
            {
                this._context.Response.StatusCode = value;
            }
        }
    }
}

#endif