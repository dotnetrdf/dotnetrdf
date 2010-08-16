/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

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

#if !NO_WEB && !NO_ASP

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using VDS.RDF.Web.Configuration.Resource;
using VDS.RDF.Writing;

namespace VDS.RDF.Web
{
    /// <summary>
    /// Abstract base class for HTTP Handlers for serving Graphs in ASP.Net applications
    /// </summary>
    public abstract class BaseGraphHandler : IHttpHandler
    {
        /// <summary>
        /// Handler Configuration
        /// </summary>
        protected BaseGraphHandlerConfiguration _config;

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
        /// Processes the request by loading the Configuration in order to obtain the Graph to be served and then serving it to the client
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public void ProcessRequest(HttpContext context)
        {
            this._config = this.LoadConfig(context);

            //Add our Custom Headers
            try
            {
                context.Response.Headers.Add("X-dotNetRDF-Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            }
            catch (PlatformNotSupportedException)
            {
                context.Response.AddHeader("X-dotNetRDF-Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            }

            //Check whether we need to use authentication
            //If there are no user groups then no authentication is in use so we default to authenticated with no per-action authentication needed
            bool isAuth = true;
            if (this._config.UserGroups.Any())
            {
                //If we have user
                isAuth = HandlerHelper.IsAuthenticated(context, this._config.UserGroups);
            }
            if (!isAuth) return;

            try
            {
                String ctype;
                IRdfWriter writer = MimeTypesHelper.GetWriter(context.Request.AcceptTypes, out ctype);

                IGraph g = this.ProcessGraph(this._config.Graph);

                //Serve the Graph to the User
                context.Response.ContentType = ctype;
                if (writer is IHtmlWriter)
                {
                    if (!this._config.Stylesheet.Equals(String.Empty))
                    {
                        ((IHtmlWriter)writer).Stylesheet = this._config.Stylesheet;
                    }
                }
                writer.Save(g, new StreamWriter(context.Response.OutputStream));

                this.UpdateConfig(context);
            }
            catch (RdfWriterSelectionException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
            }
            catch
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        /// <summary>
        /// Method which can be used to alter the Graph before it is served
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        protected virtual IGraph ProcessGraph(IGraph g)
        {
            return g;
        }

        /// <summary>
        /// Abstract method in which concrete implementations must load and return their Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <returns></returns>
        protected abstract BaseGraphHandlerConfiguration LoadConfig(HttpContext context);

        /// <summary>
        /// Abstract method in which concrete implementations may update their Configuration post-request processing if necessary
        /// </summary>
        /// <param name="context">HTTP Context</param>
        protected virtual void UpdateConfig(HttpContext context)
        {

        }
    }
}

#endif