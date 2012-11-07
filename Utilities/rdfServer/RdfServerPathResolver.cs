/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
