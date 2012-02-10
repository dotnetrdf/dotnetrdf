using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Configuration;
using VDS.Web;

namespace rdfServer
{
    class RdfServerPathResolver : IPathResolver
    {
        private HttpServer _server;

        public RdfServerPathResolver(HttpServer server)
        {
            this._server = server;
        }

        #region IPathResolver Members

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

        #endregion
    }
}
