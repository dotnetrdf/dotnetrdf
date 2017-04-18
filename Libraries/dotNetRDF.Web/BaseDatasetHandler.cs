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
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Web.Configuration.Resource;
using VDS.RDF.Writing;

namespace VDS.RDF.Web
{
    /// <summary>
    /// Abstract Base Class for HTTP Handlers which serve SPARQL Datasets
    /// </summary>
    public abstract class BaseDatasetHandler : IHttpHandler
    {
        /// <summary>
        /// Holds the Configuration for this HTTP Handler
        /// </summary>
        protected BaseDatasetHandlerConfiguration _config;

        /// <summary>
        /// Returns that the Handler is reusable
        /// </summary>
        public bool IsReusable
        {
            get 
            {
                return true;
            }
        }

        /// <summary>
        /// Processes the request by loading the Configuration in order to obtain the Dataset to be served and then serving it to the client
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public void ProcessRequest(HttpContext context)
        {
            this._config = this.LoadConfig(context);

            // Add our Standard Headers
            HandlerHelper.AddStandardHeaders(new WebContext(context), this._config);

            // Check whether we need to use authentication
            // If there are no user groups then no authentication is in use so we default to authenticated with no per-action authentication needed
            bool isAuth = true;
            if (this._config.UserGroups.Any())
            {
                // If we have user
                isAuth = HandlerHelper.IsAuthenticated(new WebContext(context), this._config.UserGroups);
            }
            if (!isAuth) return;

            this.SendDatasetToClient(context, this._config.Dataset);

            this.UpdateConfig(context);
        }

        /// <summary>
        /// Serves the Dataset to the Client
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="dataset">Dataset to serve</param>
        /// <remarks>
        /// <para>
        /// Implementations should override this if they wish to override the default behaviour of outputting the entire dataset using the <see cref="HandlerHelper.SendToClient">HandlerHelper.SendToClient()</see> method e.g. to use a custom writer or server only portions of the dataset
        /// </para>
        /// </remarks>
        public virtual void SendDatasetToClient(HttpContext context, ISparqlDataset dataset)
        {
            try
            {
                HandlerHelper.SendToClient(new WebContext(context), dataset, this._config);
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
        /// Abstract method in which concrete implementations must load and return their Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <returns></returns>
        protected abstract BaseDatasetHandlerConfiguration LoadConfig(HttpContext context);

        /// <summary>
        /// Abstract method in which concrete implementations may update their Configuration post-request processing if necessary
        /// </summary>
        /// <param name="context">HTTP Context</param>
        protected virtual void UpdateConfig(HttpContext context)
        {

        }
    }
}
