using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.Web.Logging
{
    public class ConsoleErrorLogger : IExtendedHttpLogger
    {
        public void LogError(Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex.StackTrace);
            if (ex.InnerException != null)
            {
                Console.WriteLine();
                Console.WriteLine("Inner Exception:");
                this.LogError(ex.InnerException);
            }
        }

        public void LogRequest(HttpServerContext context)
        {
            //Does Nothing
        }

        public void Dispose()
        {
            //Nothing to do
        }
    }
}
