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
                HandlerHelper.SendToClient(context, dataset, this._config);
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

#endif
