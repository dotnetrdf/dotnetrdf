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
using System.Web;
using System.Reflection;
using System.Net;
using System.Web.UI;
using System.IO;
using VDS.RDF.Query;
using VDS.RDF.Writing;
using VDS.RDF.Web.Configuration.Query;

namespace VDS.RDF.Web
{
    /// <summary>
    /// Abstract Base class for creating SPARQL Query Handler implementations
    /// </summary>
    /// <remarks>
    /// <p>
    /// This Handler supports registering the Handler multiple times in one Web application with each able to use its own settings.
    /// </p>
    /// <p>
    /// Each Handler registered in Web.config may have a prefix for their Configuration variables set by adding a AppSetting key using the virtual path of the handler like so:
    /// <code>&lt;add key="/virtualRoot/sparql/" value="ABC" /&gt;</code>
    /// Then when the Handler at that path is invoked it will look for Configuration variables prefixed with that name.
    /// </p>
    /// <p>
    /// The following Configuration Variables are supported on all Sparql Handlers:
    /// </p>
    /// <ul>
    /// <li><strong>DefaultGraph</strong> (<em>Optional</em>) - Sets the Default Graph Uri for queries which don't specify a Default Graph, defaults to the empty string which indicates no default graph is used for the query unless explicitly set with a FROM clause</li>
    /// <li><strong>DefaultTimeout</strong> (<em>Optional</em>) - Sets the Default Query Execution Timeout in milliseconds, queries which take longer than this time to execute will be aborted.  Defaults to 30000 (30s)</li>
    /// <li><strong>DefaultPartialResults</strong> (<em>Optional</em>) - Sets the Default Partial Results on Timeout behaviour for the endpoint, queries which take longer than the timeout can return partial results.  Defaults to false (Disabled)</li>
    /// <li><strong>DefaultQueryFile</strong> (<em>Optional</em>) - Sets a relative path to a File containing the Default Query that should be displayed in the Query Form which is generated if a user accesses the endpoint Uri without making a Query.  By default no default query is loaded and nothing will be displayed in the Query box of the Query Form.</li>
    /// <li><strong>ShowQueryForm</strong> (<em>Optional</em>) - Controls whether users accessing the Endpoint without making a Query are presented with a Query Form.  By default it is enabled, when disabled users are redirected to the Default.aspx page of the Web Application.</li>
    /// <li><strong>ShowErrors</strong> (<em>Optional</em>) - Controls whether error messages are shown to users or not.  If enabled users will see error messages when their queries fail detailing why they failed, if disabled users recieve a HTTP 500 Internal Server Error response.  Defaults to true (Enabled).  <strong>Note:</strong> Stack Traces from errors are only displayed if using a Debug build of dotNetRDF.</li>
    /// <li><strong>Stylesheet</strong> (<em>Optional</em>) - Sets a Stylesheet used for the Query Form and for HTML formatted output.  By default no Stylesheet is used</li>
    /// <li><strong>CacheDuration</strong> (<em>Optional</em>) - Sets the Cache Duration in minutes used for Caching Configuration and Stores, this duration is used for the Sliding Expiration of cached information.  Defaults to 15, only values between 15 and 120 are permitted.</li>
    /// <li><strong>IntroFile</strong> (<em>Optional</em>) - Sets a relative path to a File containing some Introduction text to be added to your Query Form.  By default no Introduction text is used</li>
    /// <li><strong>FullIndexing</strong> (<em>Optional</em>) - Sets whether Full Triple Indexing should be used for Handlers which use the libraries in-memory Sparql implementation - this improves query performance by 2-3 times at the cost of increased memory requirements.  Defaults to true (Enabled)</li>
    /// </ul>
    /// </remarks>
    [Obsolete("This class is considered deprecated in favour of BaseQueryHandler and its concrete implementation QueryHandler",true)]
    public abstract class BaseSparqlHandler : IHttpHandler
    {
        private BaseSparqlHandlerConfiguration _config;

        /// <summary>
        /// Indicates that the Handler is not reusable
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Processes a request to the Sparql Handler
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <remarks>
        /// Works by calling the <see cref="BaseSparqlHandler.LoadConfig">LoadConfig</see> method which causes the derived class to load it's configuration and pass it back to this class.  It then retrieves the relevant standard fields from the Querystring and invokes the <see cref="BaseSparqlHandler.ProcessQuery">ProcessQuery</see> method which the derived class has implemented the actual logic for the Sparql query in.
        /// </remarks>
        public void ProcessRequest(HttpContext context)
        {
            //Turn on Response Buffering
            context.Response.Buffer = true;

            String query = String.Empty;
            try
            {
                //Load ourselves
                this._config = this.LoadConfig(context);
                this._config.SetPersistentProperties();

                //Add our Custom Headers
                try
                {
                    context.Response.Headers.Add("X-SPARQL-Handler", this.GetType().ToString());
                    context.Response.Headers.Add("X-dotNetRDF-Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    context.Response.Headers.Add("X-dotNetRDF-SPARQL-Engine", this._config.QueryEngine.ToString());
                }
                catch (PlatformNotSupportedException)
                {
                    context.Response.AddHeader("X-SPARQL-Handler", this.GetType().ToString());
                    context.Response.AddHeader("X-dotNetRDF-Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    context.Response.AddHeader("X-dotNetRDF-SPARQL-Engine", this._config.QueryEngine.ToString());
                }

                //Get the Query and any Options attached to it
                List<String> userDefaultGraphs = new List<String>();
                List<String> userNamedGraphs = new List<String>();
                long timeout = 0;
                bool partialResults = this._config.DefaultPartialResults;

                //Get the Query
                query = context.Request.QueryString["query"];
                if (query == null || query.Equals(String.Empty))
                {
                    query = context.Request.Form["query"];
                    if (query == null || query.Equals(String.Empty))
                    {
                        if (this._config.ShowQueryForm)
                        {
                            this.ShowQueryForm(context);
                            return;
                        }
                        else
                        {
                            context.Response.Redirect("~/Default.aspx");
                        }
                    }
                }
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

                //Call the ProcessQuery method to get the derived class to process the Query appropriately
                this.ProcessQuery(context, query, userDefaultGraphs, userNamedGraphs, timeout, partialResults);
            }
            catch (Exception ex)
            {
                HandleErrors(context, "Error", query, ex);
            }
        }

        /// <summary>
        /// Abstract method for implementation by derived classes which implement the actual logic of loading the relevant Configuration
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <returns></returns>
        protected abstract BaseSparqlHandlerConfiguration LoadConfig(HttpContext context);

        /// <summary>
        /// Abstract method for implementation by derived classes which implements the actual logic of processing the Query
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="query">Sparql Query</param>
        /// <param name="userDefaultGraphs">User specified default Graph(s)</param>
        /// <param name="userNamedGraphs">User specified named Graph(s)</param>
        /// <param name="timeout">User specified Timeout</param>
        /// <param name="partialResults">Partial Results setting</param>
        protected abstract void ProcessQuery(HttpContext context, String query, List<String> userDefaultGraphs, List<String> userNamedGraphs, long timeout, bool partialResults);

        /// <summary>
        /// Internal Helper function which returns the Results back to the Client in one of their accepted formats
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="result">Results of the Sparql Query</param>
        protected void ProcessResults(HttpContext context, Object result)
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
                    ctype = MimeTypesHelper.Sparql[0];
                }
                context.Response.ContentType = ctype;
                if (sparqlwriter is IHtmlWriter)
                {
                    ((IHtmlWriter)sparqlwriter).Stylesheet = this._config.Stylesheet;
                }

                //Clear any existing Response
                context.Response.Clear();

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

                //Clear any existing Response
                context.Response.Clear();

                //Send Graph to Client
                rdfwriter.Save((Graph)result, new StreamWriter(context.Response.OutputStream));
            }
            else
            {
                throw new RdfQueryException("Unexpected Query Result Object of Type '" + result.GetType().ToString() + "' returned");
            }
        }

        /// <summary>
        /// Handles errors in processing Sparql Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="title">Error title</param>
        /// <param name="query">Sparql Query</param>
        /// <param name="ex">Error</param>
        protected virtual void HandleErrors(HttpContext context, String title, String query, Exception ex)
        {
            //Clear any existing Response
            context.Response.Clear();

            if (this._config != null)
            {
                if (!this._config.ShowErrors)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return;
                }
            }

            //Set to Plain Text output and report the error
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.ContentType = "text/plain";

            //Error Title
            context.Response.Write(title + "\n");
            context.Response.Write(new String('-', title.Length) + "\n\n");

            //Output Query with Line Numbers
            if (query != null && !query.Equals(String.Empty))
            {
                String[] lines = query.Split('\n');
                for (int l = 0; l < lines.Length; l++)
                {
                    context.Response.Write((l + 1) + ": " + lines[l] + "\n");
                }
                context.Response.Write("\n\n");
            }

            //Error Message
            context.Response.Write(ex.Message + "\n");

#if DEBUG
            //Stack Trace only when Debug build
            context.Response.Write(ex.StackTrace + "\n\n");
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                context.Response.Write(ex.Message + "\n");
                context.Response.Write(ex.StackTrace + "\n\n");
            }
#endif
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
    }
}

#endif