/*

Copyright Robert Vesse 2009-10
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
using System.Linq;
using System.Text;
using System.Net;
using System.Web;

namespace VDS.RDF.Update.Protocol
{
    /// <summary>
    /// Interface for SPARQL Graph Store HTTP Protocol for Graph Management processors
    /// </summary>
    public interface ISparqlHttpProtocolProcessor
    {
        /// <summary>
        /// Processes a GET operation which should retrieve a Graph from the Store and return it
        /// </summary>
        /// <param name="context">HTTP Context</param>
        void ProcessGet(HttpContext context);

        /// <summary>
        /// Processes a POST operation which should add triples to a Graph in the Store
        /// </summary>
        /// <param name="context">HTTP Context</param>
        void ProcessPost(HttpContext context);

        /// <summary>
        /// Processes a POST operation which adds triples to a new Graph in the Store and returns the URI of the newly created Graph
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <remarks>
        /// <para>
        /// This operation allows clients to POST data to an endpoint and have it create a Graph and assign a URI for them.
        /// </para>
        /// </remarks>
        void ProcessPostCreate(HttpContext context);

        /// <summary>
        /// Processes a PUT operation which should save a Graph to the Store completely replacing any existing Graph with the same URI
        /// </summary>
        /// <param name="context">HTTP Context</param>
        void ProcessPut(HttpContext context);

        /// <summary>
        /// Processes a DELETE operation which delete a Graph from the Store
        /// </summary>
        /// <param name="context">HTTP Context</param>
        void ProcessDelete(HttpContext context);

        /// <summary>
        /// Processes a HEAD operation which gets information about a Graph in the Store
        /// </summary>
        /// <param name="context">HTTP Context</param>
        void ProcessHead(HttpContext context);

        /// <summary>
        /// Processes a PATCH operation which may choose
        /// </summary>
        /// <param name="context"></param>
        void ProcessPatch(HttpContext context);
    }
}

#endif