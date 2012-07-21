/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System.Net;

namespace VDS.Web.Handlers
{
    /// <summary>
    /// Handler that provides basic REST control of the server
    /// </summary>
    public class RestControlHandler
        : IHttpListenerHandler
    {
        /// <summary>
        /// Gets that the Handler is reusable
        /// </summary>
        public bool IsReusable
        {
            get 
            {
                return true; 
            }
        }

        /// <summary>
        /// Processes a request
        /// </summary>
        /// <param name="context">Server Context</param>
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
                            context.Server.Shutdown(true, false);
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
    }
}
