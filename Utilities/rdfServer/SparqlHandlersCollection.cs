using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.Web;
using VDS.Web.Handlers;

namespace rdfServer
{
    public class SparqlHandlersCollection : HttpListenerHandlerCollection
    {
        private RdfServerOptions _options;

        public SparqlHandlersCollection(RdfServerOptions options)
        {
            this._options = options;
            if (options.BaseDirectory == null)
            {
                base.AddMapping(new HttpRequestMapping(HttpRequestMapping.AllVerbs, HttpRequestMapping.AnyPath, typeof(SparqlServerHandler)));
            }
            else
            {
                base.AddMapping(new HttpRequestMapping(HttpRequestMapping.AllVerbs, "/query", typeof(SparqlServerHandler)));
                base.AddMapping(new HttpRequestMapping(HttpRequestMapping.AllVerbs, "/update", typeof(SparqlServerHandler)));
                base.AddMapping(new HttpRequestMapping(HttpRequestMapping.AllVerbs, HttpRequestMapping.AnyPath, typeof(StaticFileHandler)));
            }
        }
    }
}
