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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using VDS.RDF.Parsing;
using VDS.RDF.Update.Protocol;
using VDS.RDF.Web.Configuration.Protocol;
using VDS.RDF.Writing;

namespace VDS.RDF.Web
{
    /// <summary>
    /// Abstract Base Class for creating SPARQL Graph Store HTTP Protocol Handler implementations
    /// </summary>
    public abstract class BaseSparqlHttpProtocolHandler : IHttpHandler
    {
        /// <summary>
        /// Handler Configuration
        /// </summary>
        protected BaseProtocolHandlerConfiguration _config;
        /// <summary>
        /// Base Path of the Handler as determined by the implementing class when loading Configuration using the <see cref="BaseSparqlHttpProtocolHandler.LoadConfig">LoadConfig()</see> method
        /// </summary>
        protected String _basePath;

        /// <summary>
        /// Indicates that the Handler is reusable
        /// </summary>
        public bool IsReusable
        {
            get 
            {
                return true;
            }
        }

        /// <summary>
        /// Processes requests made to the Graph Store HTTP Protocol endpoint and invokes the appropriate methods on the Protocol Processor that is in use
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <remarks>
        /// <para>
        /// Implementations may override this if necessary - if the implementation is only providing additional logic such as authentication, ACLs etc. then it is recommended that the override applies its logic and then calls the base method since this base method will handle much of the error handling and sending of appropriate HTTP Response Codes.
        /// </para>
        /// </remarks>
        public virtual void ProcessRequest(HttpContext context)
        {
            this._config = this.LoadConfig(context, out this._basePath);
            WebContext webContext = new WebContext(context);

            // Add our Standard Headers
            HandlerHelper.AddStandardHeaders(webContext, this._config);

            if (context.Request.HttpMethod.Equals("OPTIONS"))
            {
                // OPTIONS requests always result in the Service Description document
                IGraph svcDescrip = SparqlServiceDescriber.GetServiceDescription(this._config, new Uri(UriFactory.Create(context.Request.Url.AbsoluteUri), this._basePath));
                HandlerHelper.SendToClient(webContext, svcDescrip, this._config);
                return;
            }

            // Check whether we need to use authentication
            if (!HandlerHelper.IsAuthenticated(webContext, this._config.UserGroups, context.Request.HttpMethod)) return;

            try
            {
                // Invoke the appropriate method on our protocol processor
                switch (context.Request.HttpMethod)
                {
                    case "GET":
                        this._config.Processor.ProcessGet(webContext);
                        break;
                    case "PUT":
                        this._config.Processor.ProcessPut(webContext);
                        break;
                    case "POST":
                        Uri serviceUri = new Uri(UriFactory.Create(context.Request.Url.AbsoluteUri), this._basePath);
                        if (context.Request.Url.AbsoluteUri.Equals(serviceUri.AbsoluteUri))
                        {
                            // If there is a ?graph parameter or ?default parameter then this is a normal Post
                            // Otherwise it is a PostCreate
                            if (context.Request.QueryString["graph"] != null)
                            {
                                this._config.Processor.ProcessPost(webContext);
                            }
                            else if (context.Request.QueryString.AllKeys.Contains("default") || Regex.IsMatch(context.Request.QueryString.ToString(), BaseProtocolProcessor.DefaultParameterPattern))
                            {
                                this._config.Processor.ProcessPost(webContext);
                            }
                            else
                            {
                                this._config.Processor.ProcessPostCreate(webContext);
                            }
                        }
                        else
                        {
                            this._config.Processor.ProcessPost(webContext);
                        }
                        break;
                    case "DELETE":
                        this._config.Processor.ProcessDelete(webContext);
                        break;
                    case "HEAD":
                        this._config.Processor.ProcessHead(webContext);
                        break;
                    case "PATCH":
                        this._config.Processor.ProcessPatch(webContext);
                        break;
                    default:
                        // For any other HTTP Verb we send a 405 Method Not Allowed
                        context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                        break;
                }

                // Update the Cache as the request may have changed the endpoint
                this.UpdateConfig(context);
            }
            catch (SparqlHttpProtocolUriResolutionException)
            {
                // If URI Resolution fails we send a 400 Bad Request
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            catch (SparqlHttpProtocolUriInvalidException)
            {
                // If URI is invalid we send a 400 Bad Request
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            catch (NotSupportedException)
            {
                // If Not Supported we send a 405 Method Not Allowed
                context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
            }
            catch (NotImplementedException)
            {
                // If Not Implemented we send a 501 Not Implemented
                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
            }
            catch (RdfWriterSelectionException)
            {
                // If we can't select a valid Writer when returning content we send a 406 Not Acceptable
                context.Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
            }
            catch (RdfParserSelectionException)
            {
                // If we can't select a valid Parser when receiving content we send a 415 Unsupported Media Type
                context.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
            }
            catch (RdfParseException)
            {
                // If we can't parse the received content successfully we send a 400 Bad Request
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            catch (Exception)
            {
                // For any other error we'll send a 500 Internal Server Error
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        /// <summary>
        /// Loads the Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="basePath">Base Path of the Handler to be determined by an implementing class</param>
        /// <returns></returns>
        protected abstract BaseProtocolHandlerConfiguration LoadConfig(HttpContext context, out String basePath);

        /// <summary>
        /// Updates the Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        protected virtual void UpdateConfig(HttpContext context)
        {

        }
    }
}
