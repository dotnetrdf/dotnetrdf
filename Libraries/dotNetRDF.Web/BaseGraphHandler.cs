/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

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
    public abstract class BaseGraphHandler
        : IHttpHandler
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
            WebContext webContext = new WebContext(context);

            // Add our Standard Headers
            HandlerHelper.AddStandardHeaders(webContext, this._config);

            // Check whether we need to use authentication
            // If there are no user groups then no authentication is in use so we default to authenticated with no per-action authentication needed
            bool isAuth = true;
            if (this._config.UserGroups.Any())
            {
                // If we have user
                isAuth = HandlerHelper.IsAuthenticated(webContext, this._config.UserGroups);
            }
            if (!isAuth) return;

            // Check whether we can just send a 304 Not Modified
            if (HandlerHelper.CheckCachingHeaders(webContext, this._config.ETag, null))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotModified;
                HandlerHelper.AddCachingHeaders(webContext, this._config.ETag, null);
                return;
            }

            try
            {
                String[] acceptTypes = webContext.GetAcceptTypes();

                // Retrieve an appropriate MIME Type Definition which can be used to get a Writer
                MimeTypeDefinition definition = MimeTypesHelper.GetDefinitions(acceptTypes).FirstOrDefault(d => d.CanWriteRdf);
                if (definition == null) throw new RdfWriterSelectionException("No MIME Type Definitions have a registered RDF Writer for the MIME Types specified in the HTTP Accept Header");
                IRdfWriter writer = this.SelectWriter(definition);
                HandlerHelper.ApplyWriterOptions(writer, this._config);

                IGraph g = this.ProcessGraph(this._config.Graph);
                if (this._config.ETag == null)
                {
                    this._config.ETag = this.ComputeETag(g);
                }

                // Serve the Graph to the User
                context.Response.ContentType = definition.CanonicalMimeType;
                HandlerHelper.AddCachingHeaders(webContext, this._config.ETag, null);
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
