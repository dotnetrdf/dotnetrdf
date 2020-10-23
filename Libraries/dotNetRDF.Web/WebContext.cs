/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2020 dotNetRDF Project (http://dotnetrdf.org/)
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
            _context = context;
            _request = new WebRequest(context);
            _response = new WebResponse(context);
        }

        /// <summary>
        /// Gets the HTTP Request
        /// </summary>
        public IHttpProtocolRequest Request
        {
            get
            {
                return _request;
            }
        }

        /// <summary>
        /// Gets the HTTP Response
        /// </summary>
        public IHttpProtocolResponse Response
        {
            get
            {
                return _response;
            }
        }

        /// <summary>
        /// Gets the User
        /// </summary>
        public IPrincipal User
        {
            get
            {
                return _context.User;
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
            _context = context;
        }

        public String[] AcceptTypes
        {
            get
            {
                return _context.Request.AcceptTypes;
            }
        }

        public long ContentLength
        {
            get 
            {
                return _context.Request.ContentLength; 
            }
        }

        public string ContentType
        {
            get 
            {
                return _context.Request.ContentType;
            }
        }

        public NameValueCollection Headers
        {
            get
            {
                return _context.Request.Headers;
            }
        }

        public String HttpMethod
        {
            get
            {
                return _context.Request.HttpMethod;
            }
        }

        public Stream InputStream
        {
            get 
            { 
                return _context.Request.InputStream;
            }
        }

        public NameValueCollection QueryString
        {
            get
            {
                return _context.Request.QueryString;
            }
        }

        public Uri Url
        {
            get 
            {
                return _context.Request.Url;
            }
        }

        public string UserHostAddress
        {
            get 
            {
                return _context.Request.UserHostAddress; 
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
            _context = context;
        }

        public void AddHeader(string header, string value)
        {
            _context.Response.AddHeader(header, value);
        }

        public void Clear()
        {
            _context.Response.Clear();
        }

        public void Write(String data)
        {
            _context.Response.Write(data);
        }

        public Encoding ContentEncoding
        {
            get
            {
                return _context.Response.ContentEncoding;
            }
            set
            {
                _context.Response.ContentEncoding = value;
            }
        }

        public string ContentType
        {
            get
            {
                return _context.Response.ContentType;
            }
            set
            {
                _context.Response.ContentType = value;
            }
        }

        public NameValueCollection Headers
        {
            get 
            { 
                return _context.Response.Headers;    
            }
        }

        public Stream OutputStream
        {
            get 
            {
                return _context.Response.OutputStream;
            }
        }

        public int StatusCode
        {
            get
            {
                return _context.Response.StatusCode;
            }
            set
            {
                _context.Response.StatusCode = value;
            }
        }
    }
}
