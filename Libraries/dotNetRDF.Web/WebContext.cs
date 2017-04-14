/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

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

        /// <summary>
        /// Creates a new Web Context
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public WebContext(HttpContext context)
        {
            this._context = context;
            this._request = new WebRequest(context);
            this._response = new WebResponse(context);
        }

        /// <summary>
        /// Gets the HTTP Request
        /// </summary>
        public IHttpProtocolRequest Request
        {
            get
            {
                return this._request;
            }
        }

        /// <summary>
        /// Gets the HTTP Response
        /// </summary>
        public IHttpProtocolResponse Response
        {
            get
            {
                return this._response;
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
    }

    /// <summary>
    /// Implementation of <see cref="IHttpProtocolRequest"/> which wraps the standard System.Web <see cref="HttpRequest"/>
    /// </summary>
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

    /// <summary>
    /// Implementation of <see cref="IHttpProtocolResponse"/> which wraps the standard System.Web <see cref="HttpResponse"/>
    /// </summary>
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
