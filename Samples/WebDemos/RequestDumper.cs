using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDemos
{
    public class RequestDumper : IHttpHandler
    {
        /// <summary>
        /// You will need to configure this handler in the web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            HashSet<String> keys = new HashSet<String>();
            foreach (String key in context.Request.QueryString.Keys)
            {
                keys.Add(key);
            }

            context.Response.ContentType = "text/plain";
            context.Response.Write(keys.Count + " keys found with " + context.Request.QueryString.Count + " values\n\n");
            context.Response.Flush();

            foreach (String key in keys)
            {
                context.Response.Write(key + "=" + context.Request.QueryString[key] + "\n");
                foreach (String value in context.Request.QueryString.GetValues(key))
                {
                    context.Response.Write(key + "=" + value + "\n");
                }
            }

            context.Response.Flush();
        }

        #endregion
    }
}
