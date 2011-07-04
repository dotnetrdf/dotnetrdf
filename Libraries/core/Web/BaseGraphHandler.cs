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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
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

            //Add our Standard Headers
            HandlerHelper.AddStandardHeaders(context, this._config);

            //Check whether we need to use authentication
            //If there are no user groups then no authentication is in use so we default to authenticated with no per-action authentication needed
            bool isAuth = true;
            if (this._config.UserGroups.Any())
            {
                //If we have user
                isAuth = HandlerHelper.IsAuthenticated(context, this._config.UserGroups);
            }
            if (!isAuth) return;

            //Check whether we can just send a 304 Not Modified
            if (HandlerHelper.CheckCachingHeaders(context, this._config.ETag, null))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                HandlerHelper.AddCachingHeaders(context, this._config.ETag, null);
                return;
            }

            try
            {
                String[] acceptTypes = HandlerHelper.GetAcceptTypes(context);

                //Retrieve an appropriate MIME Type Definition which can be used to get a Writer
                MimeTypeDefinition definition = MimeTypesHelper.GetDefinitions(acceptTypes).FirstOrDefault(d => d.CanWriteRdf);
                if (definition == null) throw new RdfWriterSelectionException("No MIME Type Definitions have a registered RDF Writer for the MIME Types specified in the HTTP Accept Header");
                IRdfWriter writer = this.SelectWriter(definition);
                HandlerHelper.ApplyWriterOptions(writer, this._config);

                IGraph g = this.ProcessGraph(this._config.Graph);
                if (this._config.ETag == null)
                {
                    this._config.ETag = this.ComputeETag(g);
                }

                //Serve the Graph to the User
                context.Response.ContentType = definition.CanonicalMimeType;
                HandlerHelper.AddCachingHeaders(context, this._config.ETag, null);
                if (writer is IHtmlWriter)
                {
                    if (!this._config.Stylesheet.Equals(String.Empty))
                    {
                        ((IHtmlWriter)writer).Stylesheet = this._config.Stylesheet;
                    }
                }
                context.Response.ContentEncoding = definition.Encoding;
                HandlerHelper.ApplyWriterOptions(writer, this._config);
                writer.Save(g, new StreamWriter(context.Response.OutputStream, definition.Encoding));

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
        /// Selects the Writer to use for sending the Graph to the Client
        /// </summary>
        /// <param name="definition">Selected MIME Type Definition</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Implementations may override this if they wish to substitute in an alternative writer for certain MIME types (e.g. as done by the <see cref="SchemaGraphHandler">SchemaGraphHandler</see>)
        /// </para>
        /// </remarks>
        protected virtual IRdfWriter SelectWriter(MimeTypeDefinition definition)
        {
            return definition.GetRdfWriter();
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
        /// Method which computes an ETag for a Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        /// <remarks>
        /// Method may return null if no ETag can be computed or you do not wish to serve ETag Headers
        /// </remarks>
        protected virtual String ComputeETag(IGraph g)
        {
            return g.GetETag();
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