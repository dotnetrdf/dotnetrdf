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
using System.Web;
using System.Web.UI;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Web.Configuration.Query;
using VDS.RDF.Writing;

namespace VDS.RDF.Web
{
    /// <summary>
    /// Abstract Base class for Handlers which provide SPARQL Query endpoints
    /// </summary>
    public abstract class BaseSparqlQueryHandler : IHttpHandler
    {
        /// <summary>
        /// Handler Configuration
        /// </summary>
        protected BaseQueryHandlerConfiguration _config;

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
        /// Processes a SPARQL Query Request
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public void ProcessRequest(HttpContext context)
        {
            this._config = this.LoadConfig(context);

            //Add our Standard Headers
            HandlerHelper.AddStandardHeaders(context, this._config);

            if (context.Request.HttpMethod.Equals("OPTIONS"))
            {
                //OPTIONS requests always result in the Service Description document
                IGraph svcDescrip = SparqlServiceDescriber.GetServiceDescription(context, this._config, new Uri(context.Request.Url.AbsoluteUri));
                HandlerHelper.SendToClient(context, svcDescrip, this._config);
                return;
            }

            //See if there has been an query submitted
            String queryText = context.Request.QueryString["query"];
            if (queryText == null || queryText.Equals(String.Empty))
            {
                queryText = context.Request.Form["query"];
            }

            //If no Query sent either show Query Form or give a HTTP 400 response
            if (queryText == null || queryText.Equals(String.Empty))
            {
                //If there is no Query we may return the SPARQL Service Description where appropriate
                try
                {
                    //If we might show the Query Form only show the Description if the selected writer is
                    //not a HTML writer
                    MimeTypeDefinition definition = MimeTypesHelper.GetDefinitions(HandlerHelper.GetAcceptTypes(context)).FirstOrDefault(d => d.CanWriteRdf);
                    if (definition != null)
                    {
                        IRdfWriter writer = definition.GetRdfWriter();
                        if (!this._config.ShowQueryForm || !(writer is IHtmlWriter))
                        {
                            //If not a HTML Writer selected OR not showing Query Form then show the Service Description Graph
                            //unless an error occurs creating it
                            IGraph serviceDescrip = SparqlServiceDescriber.GetServiceDescription(context, this._config, new Uri(context.Request.Url.AbsoluteUri));
                            context.Response.ContentType = definition.CanonicalMimeType;
                            context.Response.ContentEncoding = definition.Encoding;
                            writer.Save(serviceDescrip, new StreamWriter(context.Response.OutputStream, definition.Encoding));
                            return;
                        }
                    }
                }
                catch
                {
                    //Ignore Exceptions - we'll just show the Query Form or return a 400 Bad Request instead
                }

                //If a Writer can't be selected then we'll either show the Query Form or return a 400 Bad Request
                if (this._config.ShowQueryForm)
                {
                    this.ShowQueryForm(context);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
                return;
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
            else if (context.Request.Form["default-graph-uri"] != null)
            {
                userDefaultGraphs.AddRange(context.Request.Form.GetValues("default-graph-uri"));
            }
            //Get the Named Graph URIs (if any)
            if (context.Request.QueryString["named-graph-uri"] != null)
            {
                userNamedGraphs.AddRange(context.Request.QueryString.GetValues("named-graph-uri"));
            }
            else if (context.Request.Form["named-graph-uri"] != null)
            {
                userNamedGraphs.AddRange(context.Request.Form.GetValues("named-graph-uri"));
            }

            //Get Timeout setting (if any)
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
            //Get Partial Results Setting (if any);
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

            try
            {
                //Now we're going to parse the Query
                SparqlQueryParser parser = new SparqlQueryParser(this._config.Syntax);
                parser.ExpressionFactories = this._config.ExpressionFactories;
                parser.QueryOptimiser = this._config.QueryOptimiser;
                SparqlQuery query = parser.ParseFromString(queryText);
                query.AlgebraOptimisers = this._config.AlgebraOptimisers;

                //Check whether we need to use authentication
                //If there are no user groups then no authentication is in use so we default to authenticated with no per-action authentication needed
                bool isAuth = true, requireActionAuth = false;
                if (this._config.UserGroups.Any())
                {
                    //If we have user
                    isAuth = HandlerHelper.IsAuthenticated(context, this._config.UserGroups);
                    requireActionAuth = true;
                }
                if (!isAuth) return;
 
                //Is this user allowed to make this kind of query?
                if (requireActionAuth) HandlerHelper.IsAuthenticated(context, this._config.UserGroups, this.GetPermissionAction(query));

                //Set the Default Graph URIs (if any)
                if (userDefaultGraphs.Count > 0)
                {
                    //Default Graph Uri specified by default-graph-uri parameter or Web.config settings
                    foreach (String userDefaultGraph in userDefaultGraphs)
                    {
                        if (!userDefaultGraph.Equals(String.Empty))
                        {
                            query.AddDefaultGraph(new Uri(userDefaultGraph));
                        }
                    }
                }
                else if (!this._config.DefaultGraphURI.Equals(String.Empty))
                {
                    //Only applies if the Query doesn't specify any Default Graph
                    if (!query.DefaultGraphs.Any())
                    {
                        query.AddDefaultGraph(new Uri(this._config.DefaultGraphURI));
                    }
                }

                //Set the Named Graph URIs (if any)
                if (userNamedGraphs.Count > 0)
                {
                    foreach (String userNamedGraph in userNamedGraphs)
                    {
                        if (!userNamedGraph.Equals(String.Empty))
                        {
                            query.AddNamedGraph(new Uri(userNamedGraph));
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
                Object result = this.ProcessQuery(query);
                this.ProcessResults(context, result);

                //Update the Cache as the request may have changed the endpoint
                this.UpdateConfig(context);
            }
            catch (RdfParseException parseEx)
            {
                HandleErrors(context, "Parsing Error", queryText, parseEx, (int)HttpStatusCode.BadRequest);
            }
            catch (RdfQueryTimeoutException timeoutEx)
            {
                HandleErrors(context, "Query Timeout Error", queryText, timeoutEx);
            }
            catch (RdfQueryException queryEx)
            {
                HandleErrors(context, "Update Error", queryText, queryEx);
            }
            catch (RdfWriterSelectionException writerSelEx)
            {
                HandleErrors(context, "Output Selection Error", queryText, writerSelEx, (int)HttpStatusCode.NotAcceptable);
            }
            catch (RdfException rdfEx)
            {
                HandleErrors(context, "RDF Error", queryText, rdfEx);
            }
            catch (Exception ex)
            {
                HandleErrors(context, "Error", queryText, ex);
            }
        }

        /// <summary>
        /// Loads the Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <returns></returns>
        protected abstract BaseQueryHandlerConfiguration LoadConfig(HttpContext context);

        /// <summary>
        /// Processes a Query
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
            return this._config.Processor.ProcessQuery(query);
        }

        /// <summary>
        /// Processes the Results and returns them to the Client in the HTTP Response
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="result">Result Object</param>
        /// <remarks>
        /// <para>
        /// Implementations should override this if they do not want to use the default results processing behaviour provided by <see cref="HandlerHelper.SendToClient">HandlerHelper.SendToClient()</see>
        /// </para>
        /// </remarks>
        protected virtual void ProcessResults(HttpContext context, Object result)
        {
            HandlerHelper.SendToClient(context, result, this._config);
        }

        /// <summary>
        /// Updates the Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        protected virtual void UpdateConfig(HttpContext context)
        {

        }

        /// <summary>
        /// Handles errors in processing SPARQL Query Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="title">Error title</param>
        /// <param name="query">Sparql Query</param>
        /// <param name="ex">Error</param>
        protected virtual void HandleErrors(HttpContext context, String title, String query, Exception ex)
        {
            HandlerHelper.HandleQueryErrors(context, this._config, title, query, ex, (int)HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Handles errors in processing SPARQL Query Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="title">Error title</param>
        /// <param name="query">Sparql Query</param>
        /// <param name="ex">Error</param>
        /// <param name="statusCode">HTTP Status Code to return</param>
        protected virtual void HandleErrors(HttpContext context, String title, String query, Exception ex, int statusCode)
        {
            HandlerHelper.HandleQueryErrors(context, this._config, title, query, ex, statusCode);
        }

        /// <summary>
        /// Generates a Sparql Query Form
        /// </summary>
        /// <param name="context"></param>
        protected virtual void ShowQueryForm(HttpContext context)
        {
            //Set Content Type
            context.Response.Clear();
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

            //End of Page
            output.RenderEndTag(); //End Body
            output.RenderEndTag(); //End Html

            output.Flush();
        }

        /// <summary>
        /// Determines the Permission Action for a SPARQL Query
        /// </summary>
        /// <param name="query">Query</param>
        /// <returns></returns>
        private String GetPermissionAction(SparqlQuery query)
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
    }
}

#endif