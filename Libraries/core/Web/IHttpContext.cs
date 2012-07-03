/*

Copyright Robert Vesse 2009-12
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

#if !NO_ASP

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.Principal;
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

        /// <summary>
        /// Gets the User
        /// </summary>
        IPrincipal User
        {
            get;
        }
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

#endif