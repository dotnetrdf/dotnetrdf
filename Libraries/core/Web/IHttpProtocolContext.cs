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
    /// Abstraction which allows us to reuse code for request analysis and response processing across different HTTP server environments
    /// </remarks>
    public interface IHttpProtocolContext
    {
        IHttpProtocolRequest Request
        {
            get;
        }

        IHttpProtocolResponse Response
        {
            get;
        }

        IPrincipal User
        {
            get;
        }
    }

    public interface IHttpProtocolRequest
    {
        String[] AcceptTypes
        {
            get;
        }

        long ContentLength
        {
            get;
        }

        String ContentType
        {
            get;
        }

        NameValueCollection Headers
        {
            get;
        }

        String HttpMethod
        {
            get;
        }

        Stream InputStream
        {
            get;
        }

        NameValueCollection QueryString
        {
            get;
        }

        Uri Url
        {
            get;
        }

        String UserHostAddress
        {
            get;
        }
    }

    public interface IHttpProtocolResponse
    {
        void AddHeader(String header, String value);

        void Clear();

        void Write(String data);

        Encoding ContentEncoding
        {
            get;
            set;
        }

        String ContentType
        {
            get;
            set;
        }

        NameValueCollection Headers
        {
            get;
        }

        Stream OutputStream
        {
            get;
        }

        int StatusCode
        {
            get;
            set;
        }
    }
}
