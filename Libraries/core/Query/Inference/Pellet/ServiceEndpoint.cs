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

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace VDS.RDF.Query.Inference.Pellet
{
    /// <summary>
    /// Represents the Service Endpoint for a Service provided by a Pellet Server
    /// </summary>
    public class ServiceEndpoint
    {
        private List<String> _methods = new List<string>();
        private String _uri;

        /// <summary>
        /// Creates a new Service Endpoint instance
        /// </summary>
        /// <param name="obj">JSON Object representing the Endpoint</param>
        internal ServiceEndpoint(JObject obj)
        {
            this._uri = (String)obj.SelectToken("url");
            JToken methods = obj.SelectToken("http-methods");
            foreach (JToken method in methods.Children())
            {
                this._methods.Add((String)method);
            }
        }

        /// <summary>
        /// Gets the URI of the Endpoint
        /// </summary>
        public String Uri
        {
            get
            {
                return this._uri;
            }
        }

        /// <summary>
        /// Gets the HTTP Methods supported by the Endpoint
        /// </summary>
        public IEnumerable<String> HttpMethods
        {
            get
            {
                return this._methods;
            }
        }
    }
}
