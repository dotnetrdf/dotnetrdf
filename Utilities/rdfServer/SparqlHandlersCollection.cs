using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.Web;

namespace rdfServer
{
    public class SparqlHandlersCollection : HttpListenerHandlerCollection
    {
        public SparqlHandlersCollection()
        {
            base.AddMapping(new HttpRequestMapping(HttpRequestMapping.AllVerbs, HttpRequestMapping.AnyPath, typeof(SparqlServerHandler)));
        }
    }
}
