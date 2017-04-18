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
using System.Collections.Specialized;
using System.IO;
#if NET40
using System.Security.Principal;
#endif
using System.Text;

namespace VDS.RDF.Web
{
    /// <summary>
    /// Interface which represents the context of some request to a HTTP server
    /// </summary>
    /// <remarks>
    /// Abstraction which allows us to reuse code for request and response processing across different HTTP server environments
    /// </remarks>
    public interface IHttpContext
    {
        /// <summary>
        /// Gets the HTTP Request
        /// </summary>
        IHttpProtocolRequest Request
        {
            get;
        }

        /// <summary>
        /// Gets the HTTP Response
        /// </summary>
        IHttpProtocolResponse Response
        {
            get;
        }

#if NET40
        /// <summary>
        /// Gets the User
        /// </summary>
        IPrincipal User
        {
            get;
        }
#endif
    }

    /// <summary>
    /// Interface which represents a HTTP request
    /// </summary>
    /// <remarks>
    /// Abstraction which allows us to reuse code for request processing across different HTTP server environments
    /// </remarks>
    public interface IHttpProtocolRequest
    {
        /// <summary>
        /// Gets the MIME Types specified in the Accept header
        /// </summary>
        String[] AcceptTypes
        {
            get;
        }

        /// <summary>
        /// Gets the Content Length
        /// </summary>
        long ContentLength
        {
            get;
        }

        /// <summary>
        /// Gets the Content Type
        /// </summary>
        String ContentType
        {
            get;
        }

        /// <summary>
        /// Gets the Headers
        /// </summary>
        NameValueCollection Headers
        {
            get;
        }

        /// <summary>
        /// Gets the HTTP Method
        /// </summary>
        String HttpMethod
        {
            get;
        }

        /// <summary>
        /// Gets the Input Stream
        /// </summary>
        Stream InputStream
        {
            get;
        }

        /// <summary>
        /// Gets the Querystring parameters
        /// </summary>
        NameValueCollection QueryString
        {
            get;
        }

        /// <summary>
        /// Gets the URL
        /// </summary>
        Uri Url
        {
            get;
        }

        /// <summary>
        /// Gets the Users Host Address
        /// </summary>
        String UserHostAddress
        {
            get;
        }
    }

    /// <summary>
    /// Interface which represents a HTTP response
    /// </summary>
    /// <remarks>
    /// Abstraction which allows us to reuse code for response processing across different HTTP server environments
    /// </remarks>
    public interface IHttpProtocolResponse
    {
        /// <summary>
        /// Adds a Header to the resposne
        /// </summary>
        /// <param name="header">Name</param>
        /// <param name="value">Value</param>
        void AddHeader(String header, String value);

        /// <summary>
        /// Clears the Response
        /// </summary>
        void Clear();

        /// <summary>
        /// Writes a String to the response body
        /// </summary>
        /// <param name="data">Data to write</param>
        void Write(String data);

        /// <summary>
        /// Gets/Sets the Content Encoding for the response
        /// </summary>
        Encoding ContentEncoding
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Content Type for the response
        /// </summary>
        String ContentType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the Headers for the response
        /// </summary>
        NameValueCollection Headers
        {
            get;
        }

        /// <summary>
        /// Gets the output stream
        /// </summary>
        Stream OutputStream
        {
            get;
        }

        /// <summary>
        /// Gets/Sets the HTTP Status Code for the response
        /// </summary>
        int StatusCode
        {
            get;
            set;
        }
    }
}
