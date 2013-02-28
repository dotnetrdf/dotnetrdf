/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Update.Commands;
using VDS.RDF.Web;
using VDS.RDF.Web.Configuration.Server;
using VDS.RDF.Writing;
using VDS.Web;
using VDS.Web.Handlers;

//TODO: Send appropriate Content Encoding

namespace VDS.RDF.Utilities.Server
{
    /// <summary>
    /// Handler for serving SPARQL requests
    /// </summary>
    public class SparqlServerHandler 
        : IHttpListenerHandler
    {
        private SparqlServerConfiguration _config;

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
            if (this._config == null)
            {
                //Try and get the Configuration Graph
                IGraph g = (IGraph)context.Server.State["ConfigurationGraph"];
                if (g == null) throw new DotNetRdfConfigurationException("The HTTP Server does not contain a Configuration Graph in its State Information");

                //Generate the expected Path and try and load the Configuration using the appropriate Node
                String expectedPath = context.Request.Url.AbsolutePath;
                if (expectedPath.LastIndexOf('/') > 0)
                {
                    expectedPath = expectedPath.Substring(0, expectedPath.LastIndexOf('/'));
                }
                else
                {
                    expectedPath = "/";
                }
                expectedPath += "*";
                INode objNode = g.GetUriNode(new Uri("dotnetrdf:" + expectedPath));
                if (objNode == null) throw new DotNetRdfConfigurationException("The Configuration Graph does not contain a URI Node with the expected URI <dotnetrdf:" + expectedPath + ">");
                this._config = new SparqlServerConfiguration(g, objNode);
            }

            String path = context.Request.Url.AbsolutePath;
            path = path.Substring(path.LastIndexOf('/') + 1);

            switch (path)
            {
                case "query":
                    this.ProcessQueryRequest(context);
                    break;
                case "update":
                    this.ProcessUpdateRequest(context);
                    break;
                case "description":
                    //TODO: Add Service Description support
                    context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                    break;
                default:
                    //TODO: Can we easily add Protocol Support or not?
                    //this.ProcessProtocolRequest(context);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
            }
        }

        /// <summary>
        /// Processes Query requests
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public void ProcessQueryRequest(HttpServerContext context)
        {
            if (this._config.QueryProcessor == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                return;
            }

            //Try and parse the Form Variables
            FormVariables form = new FormVariables(context);
            if (!form.IsValid)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            if (context.Request.HttpMethod.Equals("OPTIONS"))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                return;
                //TODO: Support Service Description?
                ////OPTIONS requests always result in the Service Description document
                //IGraph svcDescrip = SparqlServiceDescriber.GetServiceDescription(context, this._config, UriFactory.Create(context.Request.Url.AbsoluteUri), ServiceDescriptionType.Query);
                //HandlerHelper.SendToClient(context, svcDescrip, this._config);
                //return;
            }

            //See if there has been an query submitted
            String queryText = context.Request.QueryString["query"];
            if (queryText == null || queryText.Equals(String.Empty))
            {
                if (context.Request.ContentType != null)
                {
                    if (context.Request.ContentType.Equals(MimeTypesHelper.WWWFormURLEncoded))
                    {
                        queryText = form["query"];
                    }
                    else if (context.Request.ContentType.Equals(MimeTypesHelper.SparqlQuery))
                    {
                        queryText = new StreamReader(context.Request.InputStream).ReadToEnd();
                    }
                }
                else
                {
                    queryText = form["query"];
                }
            }

            //If no Query sent either show Query Form or give a HTTP 400 response
            if (queryText == null || queryText.Equals(String.Empty))
            {
                if (this._config.ShowQueryForm)
                {
                    this.ShowQueryForm(context);
                    return;
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
            }

            //Get Other options associated with this query
            List<String> userDefaultGraphs = new List<String>();
            List<String> userNamedGraphs = new List<String>();
            long timeout = 0;
            bool partialResults = this._config.DefaultPartialResults;

            //Get the Default Graph URIs (if any)
            if (context.Request.QueryString["default-graph-uri"] != null)
            {
                userDefaultGraphs.AddRange(context.Request.QueryString.GetValues("default-graph-uri"));
            }
            else if (form["default-graph-uri"] != null)
            {
                userDefaultGraphs.AddRange(form.GetValues("default-graph-uri"));
            }
            //Get the Named Graph URIs (if any)
            if (context.Request.QueryString["named-graph-uri"] != null)
            {
                userNamedGraphs.AddRange(context.Request.QueryString.GetValues("named-graph-uri"));
            }
            else if (form["named-graph-uri"] != null)
            {
                userNamedGraphs.AddRange(form.GetValues("named-graph-uri"));
            }

            //Get Timeout setting (if any)
            if (context.Request.QueryString["timeout"] != null)
            {
                if (!Int64.TryParse(context.Request.QueryString["timeout"], out timeout))
                {
                    timeout = this._config.DefaultTimeout;
                }
            }
            else if (form["timeout"] != null)
            {
                if (!Int64.TryParse(form["timeout"], out timeout))
                {
                    timeout = this._config.DefaultTimeout;
                }
            }
            //Get Partial Results Setting (if any);
            if (context.Request.QueryString["partialResults"] != null)
            {
                if (!Boolean.TryParse(context.Request.QueryString["partialResults"], out partialResults))
                {
                    partialResults = this._config.DefaultPartialResults;
                }
            }
            else if (form["partialResults"] != null)
            {
                if (!Boolean.TryParse(form["partialResults"], out partialResults))
                {
                    partialResults = this._config.DefaultPartialResults;
                }
            }

            try
            {
                //Now we're going to parse the Query
                SparqlQueryParser parser = new SparqlQueryParser(this._config.QuerySyntax);
                parser.ExpressionFactories = this._config.ExpressionFactories;
                parser.QueryOptimiser = this._config.QueryOptimiser;
                SparqlQuery query = parser.ParseFromString(queryText);
                query.AlgebraOptimisers = this._config.AlgebraOptimisers;
                query.PropertyFunctionFactories = this._config.PropertyFunctionFactories;

                //TODO: Support Authentication?
                ////Check whether we need to use authentication
                ////If there are no user groups then no authentication is in use so we default to authenticated with no per-action authentication needed
                //bool isAuth = true, requireActionAuth = false;
                //if (this._config.UserGroups.Any())
                //{
                //    //If we have user
                //    isAuth = HandlerHelper.IsAuthenticated(context, this._config.UserGroups);
                //    requireActionAuth = true;
                //}
                //if (!isAuth) return;

                ////Is this user allowed to make this kind of query?
                //if (requireActionAuth) HandlerHelper.IsAuthenticated(context, this._config.UserGroups, this.GetQueryPermissionAction(query));

                //Set the Default Graph URIs (if any)
                if (userDefaultGraphs.Count > 0)
                {
                    //Default Graph Uri specified by default-graph-uri parameter or Web.config settings
                    foreach (String userDefaultGraph in userDefaultGraphs)
                    {
                        if (!userDefaultGraph.Equals(String.Empty))
                        {
                            query.AddDefaultGraph(UriFactory.Create(userDefaultGraph));
                        }
                    }
                }
                else if (!this._config.DefaultGraphURI.Equals(String.Empty))
                {
                    //Only applies if the Query doesn't specify any Default Graph
                    if (!query.DefaultGraphs.Any())
                    {
                        query.AddDefaultGraph(UriFactory.Create(this._config.DefaultGraphURI));
                    }
                }

                //Set the Named Graph URIs (if any)
                if (userNamedGraphs.Count > 0)
                {
                    foreach (String userNamedGraph in userNamedGraphs)
                    {
                        if (!userNamedGraph.Equals(String.Empty))
                        {
                            query.AddNamedGraph(UriFactory.Create(userNamedGraph));
                        }
                    }
                }

                //Set Timeout setting
                if (timeout > 0)
                {
                    query.Timeout = timeout;
                }
                else
                {
                    query.Timeout = this._config.DefaultTimeout;
                }

                //Set Partial Results Setting                 
                query.PartialResultsOnTimeout = partialResults;

                //Set Describe Algorithm
                query.Describer = this._config.DescribeAlgorithm;

                //Now we can finally make the query and return the results
                Object result = this._config.QueryProcessor.ProcessQuery(query);
                this.ProcessQueryResults(context, result);
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
                HandleQueryErrors(context, "Query Error", queryText, queryEx);
            }
            catch (RdfWriterSelectionException writerSelEx)
            {
                HandleQueryErrors(context, "Output Selection Error", queryText, writerSelEx, (int)HttpStatusCode.NotAcceptable);
            }
            catch (RdfException rdfEx)
            {
                HandleQueryErrors(context, "RDF Error", queryText, rdfEx);
            }
            catch (Exception ex)
            {
                HandleQueryErrors(context, "Error", queryText, ex);
            }
        }

        /// <summary>
        /// Internal Helper function which returns the Results back to the Client in one of their accepted formats
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="result">Results of the Sparql Query</param>
        protected void ProcessQueryResults(HttpServerContext context, Object result)
        {
            //Return the Results
            String ctype;
            if (result is SparqlResultSet)
            {
                //Get the appropriate Writer and set the Content Type
                ISparqlResultsWriter sparqlwriter;
                if (context.Request.AcceptTypes != null)
                {
                    sparqlwriter = MimeTypesHelper.GetSparqlWriter(context.Request.AcceptTypes, out ctype);
                }
                else
                {
                    //Default to SPARQL XML Results Format if no accept header
                    sparqlwriter = new SparqlXmlWriter();
                    ctype = "application/sparql-results+xml";
                }
                context.Response.ContentType = ctype;
                if (sparqlwriter is IHtmlWriter)
                {
                    ((IHtmlWriter)sparqlwriter).Stylesheet = this._config.Stylesheet;
                }

                //Send Result Set to Client
                sparqlwriter.Save((SparqlResultSet)result, new StreamWriter(context.Response.OutputStream));
            }
            else if (result is Graph)
            {
                //Get the appropriate Writer and set the Content Type
                IRdfWriter rdfwriter = MimeTypesHelper.GetWriter(context.Request.AcceptTypes, out ctype);
                context.Response.ContentType = ctype;
                if (rdfwriter is IHtmlWriter)
                {
                    ((IHtmlWriter)rdfwriter).Stylesheet = this._config.Stylesheet;
                }

                //Send Graph to Client
                rdfwriter.Save((Graph)result, new StreamWriter(context.Response.OutputStream));
            }
            else
            {
                throw new RdfQueryException("Unexpected Query Result Object of Type '" + result.GetType().ToString() + "' returned");
            }
        }

        /// <summary>
        /// Processes Update requests
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public void ProcessUpdateRequest(HttpServerContext context)
        {
            if (this._config.UpdateProcessor == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                return;
            }

            //Try and parse the Form Variables
            FormVariables form = new FormVariables(context);
            if (!form.IsValid)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            if (context.Request.HttpMethod.Equals("OPTIONS"))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                return;
                //TODO: Support Service Description?
                ////OPTIONS requests always result in the Service Description document
                //IGraph svcDescrip = SparqlServiceDescriber.GetServiceDescription(context, this._config, UriFactory.Create(context.Request.Url.AbsoluteUri), ServiceDescriptionType.Update);
                //HandlerHelper.SendToClient(context, svcDescrip, this._config);
                //return;
            }

            //See if there has been an update submitted
            String updateText = null;
            if (context.Request.ContentType != null)
            {
                if (context.Request.ContentType.Equals(MimeTypesHelper.WWWFormURLEncoded))
                {
                    updateText = form["update"];
                }
                else if (context.Request.ContentType.Equals(MimeTypesHelper.SparqlUpdate))
                {
                    updateText = new StreamReader(context.Request.InputStream).ReadToEnd();
                }
            }
            else
            {
                updateText = form["update"];
            }

            //If no Update sent either show Update Form or give a HTTP 400 response
            if (updateText == null || updateText.Equals(String.Empty))
            {
                if (this._config.ShowUpdateForm)
                {
                    this.ShowUpdateForm(context);
                    return;
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
            }

            //Get Other options associated with this update
            List<String> userDefaultGraphs = new List<String>();
            List<String> userNamedGraphs = new List<String>();

            //Get the USING URIs (if any)
            if (context.Request.QueryString["using-graph-uri"] != null)
            {
                userDefaultGraphs.AddRange(context.Request.QueryString.GetValues("using-graph-uri"));
            }
            else if (form["using-graph-uri"] != null)
            {
                userDefaultGraphs.AddRange(form.GetValues("using-graph-uri"));
            }
            //Get the USING NAMED URIs (if any)
            if (context.Request.QueryString["using-named-graph-uri"] != null)
            {
                userNamedGraphs.AddRange(context.Request.QueryString.GetValues("using-named-graph-uri"));
            }
            else if (form["using-named-graph-uri"] != null)
            {
                userNamedGraphs.AddRange(form.GetValues("using-named-graph-uri"));
            }

            try
            {
                //Now we're going to parse the Updates
                SparqlUpdateParser parser = new SparqlUpdateParser();
                parser.ExpressionFactories = this._config.ExpressionFactories;
                SparqlUpdateCommandSet commands = parser.ParseFromString(updateText);

                //TODO: Support Authentication?
                ////Check whether we need to use authentication
                ////If there are no user groups then no authentication is in use so we default to authenticated with no per-action authentication needed
                //bool isAuth = true, requireActionAuth = false;
                //if (this._config.UserGroups.Any())
                //{
                //    //If we have user
                //    isAuth = HandlerHelper.IsAuthenticated(context, this._config.UserGroups);
                //    requireActionAuth = true;
                //}
                //if (!isAuth) return;

                //First check actions to see whether they are all permissible and apply USING/USING NAMED paramaters
                foreach (SparqlUpdateCommand cmd in commands.Commands)
                {
                    //TODO: Support Authentication?
                    ////Authenticate each action
                    //bool actionAuth = true;
                    //if (requireActionAuth) actionAuth = HandlerHelper.IsAuthenticated(context, this._config.UserGroups, this.GetUpdatePermissionAction(cmd));
                    //if (!actionAuth)
                    //{
                    //    throw new SparqlUpdatePermissionException("You are not authorised to perform the " + this.GetUpdatePermissionAction(cmd) + " action");
                    //}

                    //Check whether we need to (and are permitted to) apply USING/USING NAMED parameters
                    if (userDefaultGraphs.Count > 0 || userNamedGraphs.Count > 0)
                    {
                        BaseModificationCommand modify = cmd as BaseModificationCommand;
                        if (modify != null)
                        {
                            if (modify.GraphUri != null || modify.UsingUris.Any() || modify.UsingNamedUris.Any())
                            {
                                //Invalid if a command already has a WITH/USING/USING NAMED
                                throw new SparqlUpdateMalformedException("A command in your update request contains a WITH/USING/USING NAMED clause but you have also specified one/both of the using-graph-uri or using-named-graph-uri parameters which is not permitted by the SPARQL Protocol");
                            }
                            else
                            {
                                //Otherwise go ahead and apply
                                userDefaultGraphs.ForEach(u => modify.AddUsingUri(UriFactory.Create(u)));
                                userNamedGraphs.ForEach(u => modify.AddUsingNamedUri(UriFactory.Create(u)));
                            }
                        }
                    }
                }

                //Then assuming we got here this means all our actions are permitted so now we can process the updates
                this._config.UpdateProcessor.ProcessCommandSet(commands);

                //Flush outstanding changes
                this._config.UpdateProcessor.Flush();
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
            catch (Exception ex)
            {
                HandleUpdateErrors(context, "Error", updateText, ex);
            }
        }

        //TODO: Support Protocol Requests?
        ///// <summary>
        ///// Processes Protocol requests
        ///// </summary>
        ///// <param name="context">HTTP Context</param>
        //public void ProcessProtocolRequest(HttpListenerContext context)
        //{
        //    if (this._config.ProtocolProcessor == null)
        //    {
        //        context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
        //        return;
        //    }

        //    //Q: Support authentication?
        //    ////Check whether we need to use authentication
        //    //if (!HandlerHelper.IsAuthenticated(context, this._config.UserGroups, context.Request.HttpMethod)) return;

        //    try
        //    {
        //        //Invoke the appropriate method on our protocol processor
        //        switch (context.Request.HttpMethod)
        //        {
        //            case "GET":
        //                this._config.ProtocolProcessor.ProcessGet(context);
        //                break;
        //            case "PUT":
        //                this._config.ProtocolProcessor.ProcessPut(context);
        //                break;
        //            case "POST":
        //                this._config.ProtocolProcessor.ProcessPost(context);
        //                break;
        //            case "DELETE":
        //                this._config.ProtocolProcessor.ProcessDelete(context);
        //                break;
        //            default:
        //                //For any other HTTP Verb we send a 405 Method Not Allowed
        //                context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
        //                break;
        //        }

        //        //Update the Cache as the request may have changed the endpoint
        //        this.UpdateConfig(context);
        //    }
        //    catch (SparqlHttpProtocolUriResolutionException)
        //    {
        //        //If URI Resolution fails we send a 400 Bad Request
        //        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //    }
        //    catch (SparqlHttpProtocolUriInvalidException)
        //    {
        //        //If URI is invalid we send a 400 Bad Request
        //        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //    }
        //    catch (NotSupportedException)
        //    {
        //        //If Not Supported we send a 405 Method Not Allowed
        //        context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
        //    }
        //    catch (NotImplementedException)
        //    {
        //        //If Not Implemented we send a 501 Not Implemented
        //        context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
        //    }
        //    catch (RdfWriterSelectionException)
        //    {
        //        //If we can't select a valid Writer when returning content we send a 406 Not Acceptable
        //        context.Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
        //    }
        //    catch (RdfParserSelectionException)
        //    {
        //        //If we can't select a valid Parser when receiving content we send a 415 Unsupported Media Type
        //        context.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
        //    }
        //    catch (RdfParseException)
        //    {
        //        //If we can't parse the received content successfully we send a 400 Bad Request
        //        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //    }
        //    catch (Exception)
        //    {
        //        //For any other error we'll send a 500 Internal Server Error
        //        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        //    }
        //}

        #region Error Handling

        /// <summary>
        /// Handles errors in processing SPARQL Query Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="title">Error title</param>
        /// <param name="query">Sparql Query</param>
        /// <param name="ex">Error</param>
        protected virtual void HandleQueryErrors(HttpServerContext context, String title, String query, Exception ex)
        {
            HandlerHelper.HandleQueryErrors(new ServerContext(context), this._config, title, query, ex);
        }

        /// <summary>
        /// Handles errors in processing SPARQL Query Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="title">Error title</param>
        /// <param name="query">Sparql Query</param>
        /// <param name="ex">Error</param>
        /// <param name="statusCode">HTTP Status Code to return</param>
        protected virtual void HandleQueryErrors(HttpServerContext context, String title, String query, Exception ex, int statusCode)
        {
            HandlerHelper.HandleQueryErrors(new ServerContext(context), this._config, title, query, ex, statusCode);
        }

        /// <summary>
        /// Handles errors in processing SPARQL Update Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="title">Error title</param>
        /// <param name="update">SPARQL Update</param>
        /// <param name="ex">Error</param>
        protected virtual void HandleUpdateErrors(HttpServerContext context, String title, String update, Exception ex)
        {
            HandlerHelper.HandleUpdateErrors(new ServerContext(context), this._config, title, update, ex);
        }

        /// <summary>
        /// Handles errors in processing SPARQL Update Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="title">Error title</param>
        /// <param name="update">SPARQL Update</param>
        /// <param name="ex">Error</param>
        /// <param name="statusCode">HTTP Status code to return</param>
        protected virtual void HandleUpdateErrors(HttpServerContext context, String title, String update, Exception ex, int statusCode)
        {
            HandlerHelper.HandleUpdateErrors(new ServerContext(context), this._config, title, update, ex, statusCode);
        }

        #endregion

        #region Forms

        /// <summary>
        /// Generates a SPARQL Query Form
        /// </summary>
        /// <param name="context">HTTP Context</param>
        protected virtual void ShowQueryForm(HttpServerContext context)
        {
            //Set Content Type
            context.Response.ContentType = "text/html";

            //Get a HTML Text Writer
            HtmlTextWriter output = new HtmlTextWriter(new StreamWriter(context.Response.OutputStream));

            //Page Header
            output.Write("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
            output.RenderBeginTag(HtmlTextWriterTag.Html);
            output.RenderBeginTag(HtmlTextWriterTag.Head);
            output.RenderBeginTag(HtmlTextWriterTag.Title);
            output.WriteEncodedText("SPARQL Query Interface");
            output.RenderEndTag();
            //Add Stylesheet
            if (!this._config.Stylesheet.Equals(String.Empty))
            {
                output.AddAttribute(HtmlTextWriterAttribute.Href, this._config.Stylesheet);
                output.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                output.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
                output.RenderBeginTag(HtmlTextWriterTag.Link);
                output.RenderEndTag();
            }
            output.RenderEndTag();


            //Header Text
            output.RenderBeginTag(HtmlTextWriterTag.Body);
            output.RenderBeginTag(HtmlTextWriterTag.H3);
            output.WriteEncodedText("SPARQL Query Interface");
            output.RenderEndTag();

            //Query Form
            output.AddAttribute(HtmlTextWriterAttribute.Name, "sparqlQuery");
            output.AddAttribute("method", "get");
            output.AddAttribute("action", context.Request.Url.AbsoluteUri);
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
            int currIndent = output.Indent;
            output.Indent = 0;
            output.RenderBeginTag(HtmlTextWriterTag.Textarea);
            output.Indent = 0;
            if (!this._config.DefaultQuery.Equals(String.Empty))
            {
                output.WriteEncodedText(this._config.DefaultQuery);
            }
            output.RenderEndTag();
            output.Indent = currIndent;
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

            //End of Page
            output.RenderEndTag(); //End Body
            output.RenderEndTag(); //End Html

            output.Flush();
        }

        /// <summary>
        /// Generates a SPARQL Update Form
        /// </summary>
        /// <param name="context">HTTP Context</param>
        protected virtual void ShowUpdateForm(HttpServerContext context)
        {
            //Set Content Type
            context.Response.ContentType = "text/html";

            //Get a HTML Text Writer
            HtmlTextWriter output = new HtmlTextWriter(new StreamWriter(context.Response.OutputStream));

            //Page Header
            output.Write("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
            output.RenderBeginTag(HtmlTextWriterTag.Html);
            output.RenderBeginTag(HtmlTextWriterTag.Head);
            output.RenderBeginTag(HtmlTextWriterTag.Title);
            output.WriteEncodedText("SPARQL Update Interface");
            output.RenderEndTag();
            //Add Stylesheet
            if (!this._config.Stylesheet.Equals(String.Empty))
            {
                output.AddAttribute(HtmlTextWriterAttribute.Href, this._config.Stylesheet);
                output.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                output.AddAttribute(HtmlTextWriterAttribute.Rel, "stylesheet");
                output.RenderBeginTag(HtmlTextWriterTag.Link);
                output.RenderEndTag();
            }
            output.RenderEndTag();


            //Header Text
            output.RenderBeginTag(HtmlTextWriterTag.Body);
            output.RenderBeginTag(HtmlTextWriterTag.H3);
            output.WriteEncodedText("SPARQL Update Interface");
            output.RenderEndTag();

            //Query Form
            output.AddAttribute(HtmlTextWriterAttribute.Name, "sparqlUpdate");
            output.AddAttribute("method", "post");
            output.AddAttribute("action", context.Request.Url.AbsoluteUri);
            output.AddAttribute("enctype", MimeTypesHelper.WWWFormURLEncoded);
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
            int currIndent = output.Indent;
            output.Indent = 0;
            output.RenderBeginTag(HtmlTextWriterTag.Textarea);
            output.Indent = 0;
            if (!this._config.DefaultUpdate.Equals(String.Empty))
            {
                output.WriteEncodedText(this._config.DefaultUpdate);
            }
            output.RenderEndTag();
            output.Indent = currIndent;
            output.WriteBreak();

            //output.WriteEncodedText("Default Graph URI: ");
            //output.AddAttribute(HtmlTextWriterAttribute.Name, "default-graph-uri");
            //output.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            //output.AddAttribute(HtmlTextWriterAttribute.Size, "100");
            //output.AddAttribute(HtmlTextWriterAttribute.Value, this._config.DefaultGraphURI);
            //output.RenderBeginTag(HtmlTextWriterTag.Input);
            //output.RenderEndTag();
            //output.WriteBreak();

            output.AddAttribute(HtmlTextWriterAttribute.Type, "submit");
            output.AddAttribute(HtmlTextWriterAttribute.Value, "Perform Update");
            output.RenderBeginTag(HtmlTextWriterTag.Input);
            output.RenderEndTag();

            output.RenderEndTag(); //End Form

            //End of Page
            output.RenderEndTag(); //End Body
            output.RenderEndTag(); //End Html

            output.Flush();
        }

        #endregion
    }
}
