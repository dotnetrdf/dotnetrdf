using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace VDS.Web.Handlers
{
    public class RestControlHandler : IHttpListenerHandler
    {

        #region IHttpListenerHandler Members

        public bool IsReusable
        {
            get 
            {
                return true; 
            }
        }

        public void ProcessRequest(HttpServerContext context)
        {
            if (context.Request.HttpMethod.Equals("POST"))
            {
                if (context.Request.QueryString["operation"] != null)
                {
                    switch (context.Request.QueryString["operation"])
                    {
                        case "stop":
                            context.Response.StatusCode = (int)HttpStatusCode.Accepted;
                            context.Response.Close();
                            context.Server.Shutdown(true);
                            break;
                        case "restart":
                            context.Response.StatusCode = (int)HttpStatusCode.Accepted;
                            context.Response.Close();
                            context.Server.Restart();
                            break;
                        default:
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            break;
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        #endregion
    }
}
