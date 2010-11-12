using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.Web;

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

        public override void Initialise(HttpServerState state)
        {
            base.Initialise(state);
            if (this._options.BaseDirectory != null)
            {
                state["BaseDirectory"] = this._options.BaseDirectory;
            }
        }
    }
}
