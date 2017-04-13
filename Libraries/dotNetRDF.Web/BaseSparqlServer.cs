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
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Update.Commands;
using VDS.RDF.Update.Protocol;
using VDS.RDF.Web.Configuration.Server;
using VDS.RDF.Writing;

namespace VDS.RDF.Web
{
    /// <summary>
    /// Abstract Base class for SPARQL Servers which provide combined SPARQL Query, Update and Graph Store HTTP Protocol endpoints
    /// </summary>
    public abstract class BaseSparqlServer
        : IHttpHandler
    {
        /// <summary>
        /// Handler Configuration
        /// </summary>
        protected BaseSparqlServerConfiguration _config;
        private String _basePath;

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
        /// Processes requests
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public void ProcessRequest(HttpContext context)
        {
            this._config = this.LoadConfig(context, out this._basePath);
            WebContext webContext = new WebContext(context);

            // Add our Standard Headers
            HandlerHelper.AddStandardHeaders(webContext, this._config);

            String path = context.Request.Path;
            if (path.StartsWith(this._basePath))
            {
                path = path.Substring(this._basePath.Length);
            }

            // OPTIONS requests always result in the Service Description document
            if (context.Request.HttpMethod.Equals("OPTIONS"))
            {
                IGraph svcDescrip = SparqlServiceDescriber.GetServiceDescription(this._config, new Uri(UriFactory.Create(context.Request.Url.AbsoluteUri), this._basePath + "description"), ServiceDescriptionType.All);
                HandlerHelper.SendToClient(webContext, svcDescrip, this._config);
            }
            else
            {
                // Otherwise determine the type of request based on the Path
                switch (path)
                {
                    case "query":
                        this.ProcessQueryRequest(context);
                        break;
                    case "update":
                        this.ProcessUpdateRequest(context);
                        break;
                    case "description":
                        this.ProcessDescriptionRequest(context);
                        break;
                    default:
                        this.ProcessProtocolRequest(context);
                        break;
                }
            }
        }

        /// <summary>
        /// Processes Query requests
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public void ProcessQueryRequest(HttpContext context)
        {
            if (this._config.QueryProcessor == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                return;
            }

            WebContext webContext = new WebContext(context);

            // Options we need to determine based on the HTTP Method used
            String[] queries;
            String queryText = null;
            List<String> userDefaultGraphs = new List<String>();
            List<String> userNamedGraphs = new List<String>();

            try
            {
                // Decide what to do based on the HTTP Method
                switch (context.Request.HttpMethod.ToUpper())
                {
                    case "OPTIONS":
                        // OPTIONS requests always result in the Service Description document
                        IGraph svcDescrip = SparqlServiceDescriber.GetServiceDescription(this._config, UriFactory.Create(context.Request.Url.AbsoluteUri), ServiceDescriptionType.Query);
                        HandlerHelper.SendToClient(webContext, svcDescrip, this._config);
                        return;

                    case "HEAD":
                        // Just return from a HEAD request
                        return;

                    case "GET":
                        // GET expects a query parameter in the querystring
                        queries = context.Request.QueryString.GetValues("query");
                        if (queries != null)
                        {
                            if (queries.Length > 1) throw new ArgumentException("The query parameter was specified multiple times in the querystring");
                            queryText = queries.Length == 1 ? queries[0] : null;
                        }

                        // If no Query sent either show Query Form or give a HTTP 400 response
                        if (String.IsNullOrEmpty(queryText))
                        {
                            // If there is no Query we may return the SPARQL Service Description where appropriate
                            try
                            {
                                // If we might show the Query Form only show the Description if the selected writer is
                                // not a HTML writer
                                MimeTypeDefinition definition = MimeTypesHelper.GetDefinitions(webContext.GetAcceptTypes()).FirstOrDefault(d => d.CanWriteRdf);
                                if (definition != null)
                                {
                                    IRdfWriter writer = definition.GetRdfWriter();
                                    if (!this._config.ShowQueryForm || !(writer is IHtmlWriter))
                                    {
                                        // If not a HTML Writer selected OR not showing Query Form then show the Service Description Graph
                                        // unless an error occurs creating it
                                        IGraph serviceDescrip = SparqlServiceDescriber.GetServiceDescription(this._config, UriFactory.Create(context.Request.Url.AbsoluteUri), ServiceDescriptionType.Query);
                                        context.Response.ContentType = definition.CanonicalMimeType;
                                        context.Response.ContentEncoding = definition.Encoding;
                                        writer.Save(serviceDescrip, new StreamWriter(context.Response.OutputStream, definition.Encoding));
                                        return;
                                    }
                                }
                            }
                            catch
                            {
                                // Ignore Exceptions - we'll just show the Query Form or return a 400 Bad Request instead
                            }

                            // Otherwise we'll either show the Query Form or return a 400 Bad Request
                            if (this._config.ShowQueryForm)
                            {
                                this.ShowQueryForm(context);
                            }
                            else
                            {
                                throw new ArgumentException("Missing required query parameter");
                            }
                            return;
                        }

                        // Get the Default Graph URIs (if any)
                        if (context.Request.QueryString["default-graph-uri"] != null)
                        {
                            userDefaultGraphs.AddRange(context.Request.QueryString.GetValues("default-graph-uri"));
                        }
                        // Get the Named Graph URIs (if any)
                        if (context.Request.QueryString["named-graph-uri"] != null)
                        {
                            userNamedGraphs.AddRange(context.Request.QueryString.GetValues("named-graph-uri"));
                        }
                        break;

                    case "POST":
                        // POST requires a valid content type
                        if (context.Request.ContentType != null)
                        {
                            MimeTypeSelector contentType = MimeTypeSelector.Create(context.Request.ContentType, 0);
                            if (contentType.Type.Equals(MimeTypesHelper.WWWFormURLEncoded))
                            {
                                // Form URL Encoded was declared type so expect a query parameter in the Form parameters
                                queries = context.Request.Form.GetValues("query");
                                if (queries == null) throw new ArgumentException("Required query parameter in POST body was missing");
                                if (queries.Length == 0) throw new ArgumentException("Required query parameter in POST body was missing");
                                if (queries.Length > 1) throw new ArgumentException("The query parameter was specified multiple times in the POST body");
                                queryText = queries[0];

                                // For Form URL Encoded the Default/Named Graphs may be specified by Form parameters
                                // Get the Default Graph URIs (if any)
                                if (context.Request.Form["default-graph-uri"] != null)
                                {
                                    userDefaultGraphs.AddRange(context.Request.Form.GetValues("default-graph-uri"));
                                }
                                // Get the Named Graph URIs (if any)
                                if (context.Request.Form["named-graph-uri"] != null)
                                {
                                    userNamedGraphs.AddRange(context.Request.Form.GetValues("named-graph-uri"));
                                }

                                break;
                            }
                            else if (contentType.Type.Equals(MimeTypesHelper.SparqlQuery))
                            {
                                // application/sparql-query was declared type so expect utf-8 charset (if present)
                                if (contentType.Charset != null && !contentType.Charset.ToLower().Equals(MimeTypesHelper.CharsetUtf8)) throw new ArgumentException("HTTP POST request was received with a " + MimeTypesHelper.SparqlQuery + " Content-Type but a non UTF-8 charset parameter");

                                // Read the query from the request body
                                using (StreamReader reader = new StreamReader(context.Request.InputStream))
                                {
                                    queryText = reader.ReadToEnd();
                                    reader.Close();
                                }

                                // For application/sparql-query the Default/Named Graphs may be specified by querystring parameters
                                // Get the Default Graph URIs (if any)
                                if (context.Request.QueryString["default-graph-uri"] != null)
                                {
                                    userDefaultGraphs.AddRange(context.Request.QueryString.GetValues("default-graph-uri"));
                                }
                                // Get the Named Graph URIs (if any)
                                if (context.Request.QueryString["named-graph-uri"] != null)
                                {
                                    userNamedGraphs.AddRange(context.Request.QueryString.GetValues("named-graph-uri"));
                                }

                                break;
                            }
                            else
                            {
                                throw new ArgumentException("HTTP POST made to SPARQL query endpoint had an invalid Content-Type header, only " + MimeTypesHelper.WWWFormURLEncoded + " and " + MimeTypesHelper.SparqlQuery + " are acceptable");
                            }
                        }
                        throw new ArgumentException("HTTP POST made to SPARQL Query endpoint was missing the required Content-Type header");

                    default:
                        throw new NotSupportedException("HTTP " + context.Request.HttpMethod.ToUpper() + " is not supported by a SPARQL Query endpoint");
                }

                // Get non-standard options associated with the query
                long timeout = 0;
                bool partialResults = this._config.DefaultPartialResults;

                // Get Timeout setting (if any)
                if (context.Request.QueryString["timeout"] != null)
                {
                    if (!Int64.TryParse(context.Request.QueryString["timeout"], out timeout))
                    {
                        timeout = this._config.DefaultTimeout;
                    }
                }
                else if (context.Request.Form["timeout"] != null)
                {
                    if (!Int64.TryParse(context.Request.Form["timeout"], out timeout))
                    {
                        timeout = this._config.DefaultTimeout;
                    }
                }
                // Get Partial Results Setting (if any);
                if (context.Request.QueryString["partialResults"] != null)
                {
                    if (!Boolean.TryParse(context.Request.QueryString["partialResults"], out partialResults))
                    {
                        partialResults = this._config.DefaultPartialResults;
                    }
                }
                else if (context.Request.Form["partialResults"] != null)
                {
                    if (!Boolean.TryParse(context.Request.Form["partialResults"], out partialResults))
                    {
                        partialResults = this._config.DefaultPartialResults;
                    }
                }

                // Now we're going to parse the Query
                SparqlQueryParser parser = new SparqlQueryParser(this._config.QuerySyntax);
                parser.DefaultBaseUri = context.Request.Url;
                parser.ExpressionFactories = this._config.ExpressionFactories;
                parser.QueryOptimiser = this._config.QueryOptimiser;
                SparqlQuery query = parser.ParseFromString(queryText);
                query.AlgebraOptimisers = this._config.AlgebraOptimisers;
                query.PropertyFunctionFactories = this._config.PropertyFunctionFactories;

                // Check whether we need to use authentication
                // If there are no user groups then no authentication is in use so we default to authenticated with no per-action authentication needed
                bool isAuth = true, requireActionAuth = false;
                if (this._config.UserGroups.Any())
                {
                    // If we have user
                    isAuth = HandlerHelper.IsAuthenticated(webContext, this._config.UserGroups);
                    requireActionAuth = true;
                }
                if (!isAuth) return;

                // Is this user allowed to make this kind of query?
                if (requireActionAuth) HandlerHelper.IsAuthenticated(webContext, this._config.UserGroups, this.GetQueryPermissionAction(query));

                // Clear query dataset if there is a protocol defined one
                userDefaultGraphs.RemoveAll(g => String.IsNullOrEmpty(g));
                userNamedGraphs.RemoveAll(g => String.IsNullOrEmpty(g));
                bool isProtocolDataset = false;
                if (userDefaultGraphs.Count > 0 || userNamedGraphs.Count > 0)
                {
                    query.ClearDefaultGraphs();
                    query.ClearNamedGraphs();
                    isProtocolDataset = true;
                }

                // Set the Default Graph URIs (if any)
                if (isProtocolDataset)
                {
                    foreach (String userDefaultGraph in userDefaultGraphs)
                    {
                        query.AddDefaultGraph(UriFactory.Create(userDefaultGraph));
                    }
                }
                else if (!this._config.DefaultGraphURI.Equals(String.Empty))
                {
                    // Only applies if the Query doesn't specify any Default Graph and there wasn't a protocol defined
                    // dataset present
                    if (!query.DefaultGraphs.Any())
                    {
                        query.AddDefaultGraph(UriFactory.Create(this._config.DefaultGraphURI));
                    }
                }

                // Set the Named Graph URIs (if any)
                if (isProtocolDataset)
                {
                    query.ClearNamedGraphs();
                    foreach (String userNamedGraph in userNamedGraphs)
                    {
                        query.AddNamedGraph(UriFactory.Create(userNamedGraph));
                    }
                }

                // Set Timeout setting
                if (timeout > 0)
                {
                    query.Timeout = timeout;
                }
                else
                {
                    query.Timeout = this._config.DefaultTimeout;
                }

                // Set Partial Results Setting                 
                query.PartialResultsOnTimeout = partialResults;

                // Set Describe Algorithm
                query.Describer = this._config.DescribeAlgorithm;

                // Now we can finally make the query and return the results
                Object result = this.ProcessQuery(query);
                this.ProcessQueryResults(context, result);

                // Update the Cache as the request may have changed the endpoint
                this.UpdateConfig(context);
            }
            catch (RdfParseException parseEx)
            {
                HandleQueryErrors(context, "Parsing Error", queryText, parseEx, (int)HttpStatusCode.BadRequest);
            }
            catch (RdfQueryTimeoutException timeoutEx)
            {
                HandleQueryErrors(context, "Query Timeout Error", queryText, timeoutEx);
            }
            catch (RdfQueryException queryEx)
            {
                HandleQueryErrors(context, "Update Error", queryText, queryEx);
            }
            catch (RdfWriterSelectionException writerSelEx)
            {
                HandleQueryErrors(context, "Output Selection Error", queryText, writerSelEx, (int)HttpStatusCode.NotAcceptable);
            }
            catch (RdfException rdfEx)
            {
                HandleQueryErrors(context, "RDF Error", queryText, rdfEx);
            }
            catch (NotSupportedException notSupEx)
            {
                HandleQueryErrors(context, "HTTP Request Error", null, notSupEx, (int)HttpStatusCode.MethodNotAllowed);
            }
            catch (ArgumentException argEx)
            {
                HandleQueryErrors(context, "HTTP Request Error", null, argEx, (int)HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                HandleQueryErrors(context, "Error", queryText, ex);
            }
        }

        /// <summary>
        /// Processes Update requests
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public void ProcessUpdateRequest(HttpContext context)
        {
            if (this._config.UpdateProcessor == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                return;
            }

            WebContext webContext = new WebContext(context);

            // Options we need to determine based on the HTTP Method used
            String[] updates;
            String updateText = null;
            List<String> userDefaultGraphs = new List<String>();
            List<String> userNamedGraphs = new List<String>();

            try
            {
                // Decide what to do based on the HTTP Method
                switch (context.Request.HttpMethod.ToUpper())
                {
                    case "OPTIONS":
                        // OPTIONS requests always result in the Service Description document
                        IGraph svcDescrip = SparqlServiceDescriber.GetServiceDescription(this._config, UriFactory.Create(context.Request.Url.AbsoluteUri), ServiceDescriptionType.Update);
                        HandlerHelper.SendToClient(webContext, svcDescrip, this._config);
                        return;

                    case "HEAD":
                        // Just return from a HEAD request
                        return;

                    case "GET":
                        // A GET with an update parameter is a Bad Request
                        updates = context.Request.QueryString.GetValues("update");
                        if (updates != null && updates.Length > 0) throw new ArgumentException("Updates cannot be submitted as GET requests");

                        // Otherwise GET either results in the Service Description if appropriately conneg'd or
                        // the update form if enabled

                        try
                        {
                            // If we might show the Update Form only show the Description if the selected writer is
                            // not a HTML writer
                            MimeTypeDefinition definition = MimeTypesHelper.GetDefinitions(webContext.GetAcceptTypes()).FirstOrDefault(d => d.CanWriteRdf);
                            if (definition != null)
                            {
                                IRdfWriter writer = definition.GetRdfWriter();
                                if (!(writer is IHtmlWriter))
                                {
                                    // If not a HTML Writer selected then show the Service Description Graph
                                    // unless an error occurs creating it
                                    IGraph serviceDescrip = SparqlServiceDescriber.GetServiceDescription(this._config, UriFactory.Create(context.Request.Url.AbsoluteUri), ServiceDescriptionType.Update);
                                    context.Response.ContentType = definition.CanonicalMimeType;
                                    context.Response.ContentEncoding = definition.Encoding;
                                    writer.Save(serviceDescrip, new StreamWriter(context.Response.OutputStream, definition.Encoding));
                                    return;
                                }
                            }
                        }
                        catch
                        {
                            // Ignore Exceptions - we'll just show the Query Form or return a 400 Bad Request instead
                        }

                        // If a Writer can't be selected then we'll either show the Update Form or return a 400 Bad Request
                        if (this._config.ShowUpdateForm)
                        {
                            this.ShowUpdateForm(context);
                        }
                        else
                        {
                            throw new ArgumentException("Updates cannot be submitted as GET requests");
                        }
                        return;

                    case "POST":

                        if (context.Request.ContentType != null)
                        {
                            MimeTypeSelector contentType = MimeTypeSelector.Create(context.Request.ContentType, 0);
                            if (contentType.Type.Equals(MimeTypesHelper.WWWFormURLEncoded))
                            {
                                // Form URL Encoded was declared type so expect an update parameter in the Form parameters
                                updates = context.Request.Form.GetValues("update");
                                if (updates == null) throw new ArgumentException("Required update parameter in POST body was missing");
                                if (updates.Length == 0) throw new ArgumentException("Required update parameter in POST body was missing");
                                if (updates.Length > 1) throw new ArgumentException("The update parameter was specified multiple times in the POST body");
                                updateText = updates[0];

                                // For Form URL Encoded the Using/Using Named Graphs may be specified by Form parameters
                                // Get the USING URIs (if any)
                                if (context.Request.Form["using-graph-uri"] != null)
                                {
                                    userDefaultGraphs.AddRange(context.Request.Form.GetValues("using-graph-uri"));
                                }
                                // Get the USING NAMED URIs (if any)
                                if (context.Request.Form["using-named-graph-uri"] != null)
                                {
                                    userNamedGraphs.AddRange(context.Request.Form.GetValues("using-named-graph-uri"));
                                }

                                break;
                            }
                            else if (contentType.Type.Equals(MimeTypesHelper.SparqlUpdate))
                            {
                                // application/sparql-update was declared type so expect utf-8 charset (if present)
                                if (contentType.Charset != null && !contentType.Charset.ToLower().Equals(MimeTypesHelper.CharsetUtf8)) throw new ArgumentException("HTTP POST request was received with a " + MimeTypesHelper.SparqlUpdate + " Content-Type but a non UTF-8 charset parameter");

                                using (StreamReader reader = new StreamReader(context.Request.InputStream))
                                {
                                    updateText = reader.ReadToEnd();
                                    reader.Close();
                                }

                                // For application/sparql-update the Using/Using Named Graphs may be specified by querystring parameters
                                // Get the USING URIs (if any)
                                if (context.Request.QueryString["using-graph-uri"] != null)
                                {
                                    userDefaultGraphs.AddRange(context.Request.QueryString.GetValues("using-graph-uri"));
                                }
                                // Get the USING NAMED URIs (if any)
                                if (context.Request.QueryString["using-named-graph-uri"] != null)
                                {
                                    userNamedGraphs.AddRange(context.Request.QueryString.GetValues("using-named-graph-uri"));
                                }

                                break;
                            }
                            else
                            {
                                throw new ArgumentException("HTTP POST made to SPARQL update endpoint had an invalid Content-Type header, only " + MimeTypesHelper.WWWFormURLEncoded + " and " + MimeTypesHelper.SparqlUpdate + " are acceptable");
                            }
                        }
                        throw new ArgumentException("HTTP POST made to SPARQL Query endpoint was missing the required Content-Type header");

                    default:
                        throw new NotSupportedException("HTTP " + context.Request.HttpMethod.ToUpper() + " is not supported by a SPARQL Update endpoint");
                }

                // Clean up protocol provided dataset
                userDefaultGraphs.RemoveAll(g => String.IsNullOrEmpty(g));
                userNamedGraphs.RemoveAll(g => String.IsNullOrEmpty(g));

                // Now we're going to parse the Updates
                SparqlUpdateParser parser = new SparqlUpdateParser();
                parser.DefaultBaseUri = context.Request.Url;
                parser.ExpressionFactories = this._config.ExpressionFactories;
                SparqlUpdateCommandSet commands = parser.ParseFromString(updateText);

                // Check whether we need to use authentication
                // If there are no user groups then no authentication is in use so we default to authenticated with no per-action authentication needed
                bool isAuth = true, requireActionAuth = false;
                if (this._config.UserGroups.Any())
                {
                    // If we have user
                    isAuth = HandlerHelper.IsAuthenticated(webContext, this._config.UserGroups);
                    requireActionAuth = true;
                }
                if (!isAuth) return;

                // First check actions to see whether they are all permissible and apply USING/USING NAMED parameters
                foreach (SparqlUpdateCommand cmd in commands.Commands)
                {
                    // Authenticate each action
                    bool actionAuth = true;
                    if (requireActionAuth) actionAuth = HandlerHelper.IsAuthenticated(webContext, this._config.UserGroups, this.GetUpdatePermissionAction(cmd));
                    if (!actionAuth)
                    {
                        throw new SparqlUpdatePermissionException("You are not authorised to perform the " + this.GetUpdatePermissionAction(cmd) + " action");
                    }

                    // Check whether we need to (and are permitted to) apply USING/USING NAMED parameters
                    if (userDefaultGraphs.Count > 0 || userNamedGraphs.Count > 0)
                    {
                        BaseModificationCommand modify = cmd as BaseModificationCommand;
                        if (modify != null)
                        {
                            if (modify.GraphUri != null || modify.UsingUris.Any() || modify.UsingNamedUris.Any())
                            {
                                // Invalid if a command already has a WITH/USING/USING NAMED
                                throw new SparqlUpdateMalformedException("A command in your update request contains a WITH/USING/USING NAMED clause but you have also specified one/both of the using-graph-uri or using-named-graph-uri parameters which is not permitted by the SPARQL Protocol");
                            }
                            else
                            {
                                // Otherwise go ahead and apply
                                userDefaultGraphs.ForEach(u => modify.AddUsingUri(UriFactory.Create(u)));
                                userNamedGraphs.ForEach(u => modify.AddUsingNamedUri(UriFactory.Create(u)));
                            }
                        }
                    }
                }

                // Then assuming we got here this means all our actions are permitted so now we can process the updates
                this.ProcessUpdates(commands);

                // Flush outstanding changes
                this._config.UpdateProcessor.Flush();

                // Update the Cache as the request may have changed the endpoint
                this.UpdateConfig(context);
            }
            catch (RdfParseException parseEx)
            {
                HandleUpdateErrors(context, "Parsing Error", updateText, parseEx, (int)HttpStatusCode.BadRequest);
            }
            catch (SparqlUpdatePermissionException permEx)
            {
                HandleUpdateErrors(context, "Permissions Error", updateText, permEx, (int)HttpStatusCode.Forbidden);
            }
            catch (SparqlUpdateMalformedException malEx)
            {
                HandleUpdateErrors(context, "Malformed Update Error", updateText, malEx, (int)HttpStatusCode.BadRequest);
            }
            catch (SparqlUpdateException updateEx)
            {
                HandleUpdateErrors(context, "Update Error", updateText, updateEx);
            }
            catch (RdfException rdfEx)
            {
                HandleUpdateErrors(context, "RDF Error", updateText, rdfEx);
            }
            catch (NotSupportedException notSupEx)
            {
                HandleUpdateErrors(context, "HTTP Request Error", null, notSupEx, (int)HttpStatusCode.MethodNotAllowed);
            }
            catch (ArgumentException argEx)
            {
                HandleUpdateErrors(context, "HTTP Request Error", null, argEx, (int)HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                HandleUpdateErrors(context, "Error", updateText, ex);
            }
        }

        /// <summary>
        /// Processes Protocol requests
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public void ProcessProtocolRequest(HttpContext context)
        {
            if (this._config.ProtocolProcessor == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                return;
            }

            WebContext webContext = new WebContext(context);

            if (context.Request.HttpMethod.Equals("OPTIONS"))
            {
                // OPTIONS requests always result in the Service Description document
                IGraph svcDescrip = SparqlServiceDescriber.GetServiceDescription(this._config, new Uri(UriFactory.Create(context.Request.Url.AbsoluteUri), this._basePath), ServiceDescriptionType.Protocol);
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
                        this._config.ProtocolProcessor.ProcessGet(webContext);
                        break;
                    case "PUT":
                        this._config.ProtocolProcessor.ProcessPut(webContext);
                        break;
                    case "POST":
                        Uri serviceUri = new Uri(UriFactory.Create(context.Request.Url.AbsoluteUri), this._basePath);
                        if (context.Request.Url.AbsoluteUri.Equals(serviceUri.AbsoluteUri))
                        {
                            // If there is a ?graph parameter or ?default parameter then this is a normal Post
                            // Otherwise it is a PostCreate
                            if (context.Request.QueryString["graph"] != null)
                            {
                                this._config.ProtocolProcessor.ProcessPost(webContext);
                            }
                            else if (context.Request.QueryString.AllKeys.Contains("default") || Regex.IsMatch(context.Request.QueryString.ToString(), BaseProtocolProcessor.DefaultParameterPattern))
                            {
                                this._config.ProtocolProcessor.ProcessPost(webContext);
                            }
                            else
                            {
                                this._config.ProtocolProcessor.ProcessPostCreate(webContext);
                            }
                        }
                        else
                        {
                            this._config.ProtocolProcessor.ProcessPost(webContext);
                        }
                        break;
                    case "DELETE":
                        this._config.ProtocolProcessor.ProcessDelete(webContext);
                        break;
                    case "PATCH":
                        this._config.ProtocolProcessor.ProcessPatch(webContext);
                        break;
                    case "HEAD":
                        this._config.ProtocolProcessor.ProcessHead(webContext);
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
        /// Processes Service Description requests
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public void ProcessDescriptionRequest(HttpContext context)
        {
            try
            {
                // Get the Service Description Graph
                IGraph serviceDescrip = SparqlServiceDescriber.GetServiceDescription(this._config, UriFactory.Create(context.Request.Url.AbsoluteUri), ServiceDescriptionType.All);
                HandlerHelper.SendToClient(new WebContext(context), serviceDescrip, this._config);
            }
            catch
            {
                // If any errors occur then return a 500 Internal Server Error
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }

        #region Configuration Loading

        /// <summary>
        /// Loads the Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="basePath">Base Path of the Handler</param>
        /// <returns></returns>
        protected abstract BaseSparqlServerConfiguration LoadConfig(HttpContext context, out String basePath);

        /// <summary>
        /// Updates the Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        protected virtual void UpdateConfig(HttpContext context)
        {

        }

        #endregion

        #region Operation Processing

        /// <summary>
        /// Processes SPARQL Queries
        /// </summary>
        /// <param name="query">Query</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Implementations should override this method if their behaviour requires more than just invoking the configured Query processor
        /// </para>
        /// </remarks>
        protected virtual Object ProcessQuery(SparqlQuery query)
        {
            return this._config.QueryProcessor.ProcessQuery(query);
        }

        /// <summary>
        /// Processes SPARQL Updates
        /// </summary>
        /// <param name="cmds">Update Command Set</param>
        /// <remarks>
        /// <para>
        /// Implementations should override this method if their behaviour requires more than just invoking the configured Update processor
        /// </para>
        /// </remarks>
        protected virtual void ProcessUpdates(SparqlUpdateCommandSet cmds)
        {
            this._config.UpdateProcessor.ProcessCommandSet(cmds);
        }

        /// <summary>
        /// Internal Helper function which returns the Results back to the Client in one of their accepted formats
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="result">Results of the Sparql Query</param>
        /// <remarks>
        /// <para>
        /// Implementations should override this if they want to control how results are sent to the client rather than using the default behaviour provided by <see cref="HandlerHelper.ProcessResults">HandlerHelper.ProcessResults()</see>
        /// </para>
        /// </remarks>
        protected virtual void ProcessQueryResults(HttpContext context, Object result)
        {
            HandlerHelper.SendToClient(new WebContext(context), result, this._config);
        }

        #endregion

        #region Error Handling

        /// <summary>
        /// Handles errors in processing SPARQL Query Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="title">Error title</param>
        /// <param name="query">Sparql Query</param>
        /// <param name="ex">Error</param>
        protected virtual void HandleQueryErrors(HttpContext context, String title, String query, Exception ex)
        {
            HandlerHelper.HandleQueryErrors(new WebContext(context), this._config, title, query, ex);
        }

        /// <summary>
        /// Handles errors in processing SPARQL Query Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="title">Error title</param>
        /// <param name="query">Sparql Query</param>
        /// <param name="ex">Error</param>
        /// <param name="statusCode">HTTP Status Code to return</param>
        protected virtual void HandleQueryErrors(HttpContext context, String title, String query, Exception ex, int statusCode)
        {
            HandlerHelper.HandleQueryErrors(new WebContext(context), this._config, title, query, ex, statusCode);
        }

        /// <summary>
        /// Handles errors in processing SPARQL Update Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="title">Error title</param>
        /// <param name="update">SPARQL Update</param>
        /// <param name="ex">Error</param>
        protected virtual void HandleUpdateErrors(HttpContext context, String title, String update, Exception ex)
        {
            HandlerHelper.HandleUpdateErrors(new WebContext(context), this._config, title, update, ex, (int)HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Handles errors in processing SPARQL Update Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="title">Error title</param>
        /// <param name="update">SPARQL Update</param>
        /// <param name="ex">Error</param>
        /// <param name="statusCode">HTTP Status Code</param>
        protected virtual void HandleUpdateErrors(HttpContext context, String title, String update, Exception ex, int statusCode)
        {
            HandlerHelper.HandleUpdateErrors(new WebContext(context), this._config, title, update, ex, statusCode);
        }

        #endregion

        #region Forms

        /// <summary>
        /// Generates a SPARQL Query Form
        /// </summary>
        /// <param name="context">HTTP Context</param>
        protected virtual void ShowQueryForm(HttpContext context)
        {
            // Set Content Type
            context.Response.Clear();
            context.Response.ContentType = "text/html";

            // Get a HTML Text Writer
            HtmlTextWriter output = new HtmlTextWriter(new StreamWriter(context.Response.OutputStream));

            // Page Header
            output.Write("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
            output.RenderBeginTag(HtmlTextWriterTag.Html);
            output.RenderBeginTag(HtmlTextWriterTag.Head);
            output.RenderBeginTag(HtmlTextWriterTag.Title);
            output.WriteEncodedText("SPARQL Query Interface");
            output.RenderEndTag();
            // Add Stylesheet
            if (!this._config.Stylesheet.Equals(String.Empty))
            {
                output.AddAttribute(HtmlTextWriterAttribute.Href, this._config.Stylesheet);
                output.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                output.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
                output.RenderBeginTag(HtmlTextWriterTag.Link);
                output.RenderEndTag();
            }
            output.RenderEndTag();


            // Header Text
            output.RenderBeginTag(HtmlTextWriterTag.Body);
            output.RenderBeginTag(HtmlTextWriterTag.H3);
            output.WriteEncodedText("SPARQL Query Interface");
            output.RenderEndTag();

            // Query Form
            output.AddAttribute(HtmlTextWriterAttribute.Name, "sparqlQuery");
            output.AddAttribute("method", "get");
            output.AddAttribute("action", context.Request.Path);
            output.RenderBeginTag(HtmlTextWriterTag.Form);

            if (!this._config.IntroductionText.Equals(String.Empty))
            {
                output.RenderBeginTag(HtmlTextWriterTag.P);
                output.Write(this._config.IntroductionText);
                output.RenderEndTag();
            }

            output.WriteEncodedText("Query");
            output.WriteBreak();
            output.AddAttribute(HtmlTextWriterAttribute.Name, "query");
            output.AddAttribute(HtmlTextWriterAttribute.Rows, "15");
            output.AddAttribute(HtmlTextWriterAttribute.Cols, "100");
            output.RenderBeginTag(HtmlTextWriterTag.Textarea);
            output.WriteEncodedText(this._config.DefaultQuery);
            output.RenderEndTag();
            output.WriteBreak();

            output.WriteEncodedText("Default Graph URI: ");
            output.AddAttribute(HtmlTextWriterAttribute.Name, "default-graph-uri");
            output.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            output.AddAttribute(HtmlTextWriterAttribute.Size, "100");
            output.AddAttribute(HtmlTextWriterAttribute.Value, this._config.DefaultGraphURI);
            output.RenderBeginTag(HtmlTextWriterTag.Input);
            output.RenderEndTag();
            output.WriteBreak();

            if (this._config.SupportsTimeout)
            {
                output.WriteEncodedText("Timeout: ");
                output.AddAttribute(HtmlTextWriterAttribute.Name, "timeout");
                output.AddAttribute(HtmlTextWriterAttribute.Type, "text");
                output.AddAttribute(HtmlTextWriterAttribute.Value, this._config.DefaultTimeout.ToString());
                output.RenderBeginTag(HtmlTextWriterTag.Input);
                output.RenderEndTag();
                output.WriteEncodedText(" Milliseconds");
                output.WriteBreak();
            }

            if (this._config.SupportsPartialResults)
            {
                output.AddAttribute(HtmlTextWriterAttribute.Name, "partialResults");
                output.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
                if (this._config.DefaultPartialResults) output.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
                output.AddAttribute(HtmlTextWriterAttribute.Value, "true");
                output.RenderBeginTag(HtmlTextWriterTag.Input);
                output.RenderEndTag();
                output.WriteEncodedText(" Partial Results on Timeout?");
                output.WriteBreak();
            }

            output.AddAttribute(HtmlTextWriterAttribute.Type, "submit");
            output.AddAttribute(HtmlTextWriterAttribute.Value, "Make Query");
            output.RenderBeginTag(HtmlTextWriterTag.Input);
            output.RenderEndTag();

            output.RenderEndTag(); //End Form

            // End of Page
            output.RenderEndTag(); //End Body
            output.RenderEndTag(); //End Html

            output.Flush();
        }

        /// <summary>
        /// Generates a SPARQL Update Form
        /// </summary>
        /// <param name="context">HTTP Context</param>
        protected virtual void ShowUpdateForm(HttpContext context)
        {
            // Set Content Type
            context.Response.Clear();
            context.Response.ContentType = "text/html";

            // Get a HTML Text Writer
            HtmlTextWriter output = new HtmlTextWriter(new StreamWriter(context.Response.OutputStream));

            // Page Header
            output.Write("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
            output.RenderBeginTag(HtmlTextWriterTag.Html);
            output.RenderBeginTag(HtmlTextWriterTag.Head);
            output.RenderBeginTag(HtmlTextWriterTag.Title);
            output.WriteEncodedText("SPARQL Update Interface");
            output.RenderEndTag();
            // Add Stylesheet
            if (!this._config.Stylesheet.Equals(String.Empty))
            {
                output.AddAttribute(HtmlTextWriterAttribute.Href, this._config.Stylesheet);
                output.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                output.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
                output.RenderBeginTag(HtmlTextWriterTag.Link);
                output.RenderEndTag();
            }
            output.RenderEndTag();


            // Header Text
            output.RenderBeginTag(HtmlTextWriterTag.Body);
            output.RenderBeginTag(HtmlTextWriterTag.H3);
            output.WriteEncodedText("SPARQL Update Interface");
            output.RenderEndTag();

            // Query Form
            output.AddAttribute(HtmlTextWriterAttribute.Name, "sparqlUpdate");
            output.AddAttribute("method", "post");
            output.AddAttribute("action", context.Request.Path);
            output.RenderBeginTag(HtmlTextWriterTag.Form);

            if (!this._config.IntroductionText.Equals(String.Empty))
            {
                output.RenderBeginTag(HtmlTextWriterTag.P);
                output.Write(this._config.IntroductionText);
                output.RenderEndTag();
            }

            output.WriteEncodedText("Update");
            output.WriteBreak();
            output.AddAttribute(HtmlTextWriterAttribute.Name, "update");
            output.AddAttribute(HtmlTextWriterAttribute.Rows, "15");
            output.AddAttribute(HtmlTextWriterAttribute.Cols, "100");
            output.RenderBeginTag(HtmlTextWriterTag.Textarea);
            output.WriteEncodedText(this._config.DefaultUpdate);
            output.RenderEndTag();
            output.WriteBreak();

            output.WriteEncodedText("Default Graph URI: ");
            output.AddAttribute(HtmlTextWriterAttribute.Name, "using-graph-uri");
            output.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            output.AddAttribute(HtmlTextWriterAttribute.Size, "100");
            output.AddAttribute(HtmlTextWriterAttribute.Value, this._config.DefaultGraphURI);
            output.RenderBeginTag(HtmlTextWriterTag.Input);
            output.RenderEndTag();
            output.WriteBreak();

            output.AddAttribute(HtmlTextWriterAttribute.Type, "submit");
            output.AddAttribute(HtmlTextWriterAttribute.Value, "Perform Update");
            output.RenderBeginTag(HtmlTextWriterTag.Input);
            output.RenderEndTag();

            output.RenderEndTag(); //End Form

            // End of Page
            output.RenderEndTag(); //End Body
            output.RenderEndTag(); //End Html

            output.Flush();
        }

        #endregion

        #region Permission Detection

        /// <summary>
        /// Gets the Permission action for a SPARQL Query
        /// </summary>
        /// <param name="query">Query</param>
        /// <returns></returns>
        private String GetQueryPermissionAction(SparqlQuery query)
        {
            switch (query.QueryType)
            {
                case SparqlQueryType.Ask:
                    return "ASK";
                case SparqlQueryType.Construct:
                    return "CONSTRUCT";
                case SparqlQueryType.Describe:
                case SparqlQueryType.DescribeAll:
                    return "DESCRIBE";
                case SparqlQueryType.Select:
                case SparqlQueryType.SelectAll:
                case SparqlQueryType.SelectAllDistinct:
                case SparqlQueryType.SelectAllReduced:
                case SparqlQueryType.SelectDistinct:
                case SparqlQueryType.SelectReduced:
                    return "SELECT";
                default:
                    return String.Empty;
            }
        }

        /// <summary>
        /// Gets the Permission action for a SPARQL Update Command
        /// </summary>
        /// <param name="cmd">Update Command</param>
        /// <returns></returns>
        private String GetUpdatePermissionAction(SparqlUpdateCommand cmd)
        {
            switch (cmd.CommandType)
            {
                case SparqlUpdateCommandType.InsertData:
                    return "INSERT DATA";
                case SparqlUpdateCommandType.DeleteData:
                    return "DELETE DATA";
                case SparqlUpdateCommandType.Insert:
                    return "INSERT";
                case SparqlUpdateCommandType.Delete:
                    return "DELETE";
                case SparqlUpdateCommandType.Modify:
                    return "MODIFY";
                case SparqlUpdateCommandType.Load:
                    return "LOAD";
                case SparqlUpdateCommandType.Clear:
                    return "CLEAR";
                case SparqlUpdateCommandType.Create:
                    return "CREATE";
                case SparqlUpdateCommandType.Drop:
                    return "DROP";
                default:
                    return String.Empty;
            }
        }

        #endregion

    }
}
