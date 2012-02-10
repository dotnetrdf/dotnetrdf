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
using VDS.RDF.Configuration;
using VDS.Web;

namespace VDS.RDF.Utilities.Server
{
    /// <summary>
    /// A Path Resolver for helping in the loading of Configuration Files which is aware of the VDS.Web.Server architecture
    /// </summary>
    class RdfServerPathResolver
        : IPathResolver
    {
        private HttpServer _server;

        /// <summary>
        /// Creates a new Path resolver for the Server
        /// </summary>
        /// <param name="server">Server</param>
        public RdfServerPathResolver(HttpServer server)
        {
            this._server = server;
        }

        /// <summary>
        /// Resolves a Path
        /// </summary>
        /// <param name="path">Path to resolve</param>
        /// <returns>Resolved Path</returns>
        public string ResolvePath(string path)
        {
            if (this._server == null)
            {
                return path;
            }
            else
            {
                String temp = this._server.MapPath(path);
                if (temp == null) throw new DotNetRdfConfigurationException("Path '" + path + "' is invalid as a Path for rdfServer.  Paths may not go outside the base directory unless they refer to a Virtual Directory");
                return temp;
            }
        }
    }
}
