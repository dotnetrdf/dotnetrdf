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

namespace VDS.Web
{
    /// <summary>
    /// Represents an eror in the Server
    /// </summary>
    public class HttpServerException
        : Exception
    {
        /// <summary>
        /// Creates a new Server Exception
        /// </summary>
        /// <param name="message">Error Message</param>
        public HttpServerException(String message)
            : base(message) { }

        /// <summary>
        /// Creates a new Server Exception
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <param name="cause">Inner Exception</param>
        public HttpServerException(String message, Exception cause)
            : base(message, cause) { }
    }

    /// <summary>
    /// Represents the situation where no handler is available to process a request
    /// </summary>
    public class NoHandlerException
        : HttpServerException
    {
        /// <summary>
        /// Creates a new No Handler Exception
        /// </summary>
        /// <param name="message">Error Message</param>
        public NoHandlerException(String message)
            : base(message) { }

        /// <summary>
        /// Creates a new No Handler Exception
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <param name="cause">Inner Exception</param>
        public NoHandlerException(String message, Exception cause)
            : base(message, cause) { }
    }
}
