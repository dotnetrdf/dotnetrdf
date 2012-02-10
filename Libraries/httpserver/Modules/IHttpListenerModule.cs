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
using System.Linq;
using System.Text;
using VDS.Web.Handlers;

namespace VDS.Web.Modules
{
    /// <summary>
    /// Interface for Modules which can be used to modify the request/response as desired
    /// </summary>
    /// <remarks>
    /// <para>
    /// Modules are either applied Pre-Request (i.e. before a <see cref="IHttpListenerHandler">IHttpListenerHandler</see> is selected to provide the actual response) or Pre-Response (i.e. before the response object is closed and sent to the client)
    /// </para>
    /// </remarks>
    public interface IHttpListenerModule
    {
        /// <summary>
        /// Processes a Request and returns false if no further handling of the request should take place
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// If used as a Pre-Request Module returning false will cause the actual process request step where a <see cref="IHttpListenerHandler">IHttpListenerHandler</see> is applied to be skipped as well as any further pre-request modules
        /// </para>
        /// <para>
        /// If used as a Pre-Response Module returning false will cause any further Pre-Response modules to be skipped
        /// </para>
        /// </remarks>
        bool ProcessRequest(HttpServerContext context);
    }
}
