using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VDS.Web.Handlers
{
    public class StaticFileHandlersCollection : HttpListenerHandlerCollection
    {
        public StaticFileHandlersCollection()
        {          
            //Add the Single Mapping we need
            this.AddMapping(new HttpRequestMapping(HttpRequestMapping.AllVerbs, "*", typeof(StaticFileHandler)));
        }
    }
}
