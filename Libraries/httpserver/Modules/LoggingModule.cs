using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.Web.Logging;

namespace VDS.Web.Modules
{
    public class LoggingModule : IHttpListenerModule
    {

        public bool ProcessRequest(HttpServerContext context)
        {
            foreach (IHttpLogger logger in context.Server.Loggers)
            {
                try
                {
                    logger.LogRequest(context);
                }
                catch (Exception ex)
                {
                    context.Server.LogErrors(ex);
                }
            }

            //Logging Module always returns true to allow other Modules to execute
            return true;
        }
    }
}
