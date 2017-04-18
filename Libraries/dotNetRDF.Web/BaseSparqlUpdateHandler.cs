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
using System.Reflection;
using System.IO;
using System.Net;
using System.Web;
using System.Web.UI;
using VDS.RDF.Web.Configuration.Update;
using VDS.RDF.Parsing;
using VDS.RDF.Update;
using VDS.RDF.Update.Commands;
using VDS.RDF.Writing;

namespace VDS.RDF.Web
{
    /// <summary>
    /// Abstract Base Class for creating SPARQL Update Handler implementations
    /// </summary>
    public abstract class BaseSparqlUpdateHandler
        : IHttpHandler
    {
        /// <summary>
        /// Handler Configuration
        /// </summary>
        protected BaseUpdateHandlerConfiguration _config;

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
        /// Processes SPARQL Update requests
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public void ProcessRequest(HttpContext context)
        {
            this._config = this.LoadConfig(context);
            WebContext webContext = new WebContext(context);

            // Add our Standard Headers
            HandlerHelper.AddStandardHeaders(webContext, this._config);

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
                        IGraph svcDescrip = SparqlServiceDescriber.GetServiceDescription(this._config, UriFactory.Create(context.Request.Url.AbsoluteUri));
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
                                    IGraph serviceDescrip = SparqlServiceDescriber.GetServiceDescription(this._config, UriFactory.Create(context.Request.Url.AbsoluteUri));
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
                    if (requireActionAuth) actionAuth = HandlerHelper.IsAuthenticated(webContext, this._config.UserGroups, this.GetPermissionAction(cmd));
                    if (!actionAuth)
                    {
                        throw new SparqlUpdatePermissionException("You are not authorised to perform the " + this.GetPermissionAction(cmd) + " action");
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
                this._config.Processor.Flush();

                // Update the Cache as the request may have changed the endpoint
                this.UpdateConfig(context);
            }
            catch (RdfParseException parseEx)
            {
                HandleErrors(context, "Parsing Error", updateText, parseEx, (int)HttpStatusCode.BadRequest);
            }
            catch (SparqlUpdatePermissionException permEx)
            {
                HandleErrors(context, "Permissions Error", updateText, permEx, (int)HttpStatusCode.Forbidden);
            }
            catch (SparqlUpdateMalformedException malEx)
            {
                HandleErrors(context, "Malformed Update Error", updateText, malEx, (int)HttpStatusCode.BadRequest);
            }
            catch (SparqlUpdateException updateEx)
            {
                HandleErrors(context, "Update Error", updateText, updateEx);
            }
            catch (RdfException rdfEx)
            {
                HandleErrors(context, "RDF Error", updateText, rdfEx);
            }
            catch (NotSupportedException notSupEx)
            {
                HandleErrors(context, "HTTP Request Error", null, notSupEx, (int)HttpStatusCode.MethodNotAllowed);
            }
            catch (ArgumentException argEx)
            {
                HandleErrors(context, "HTTP Request Error", null, argEx, (int)HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                HandleErrors(context, "Error", updateText, ex);
            }
        }

        /// <summary>
        /// Loads the Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <returns></returns>
        protected abstract BaseUpdateHandlerConfiguration LoadConfig(HttpContext context);

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
            this._config.Processor.ProcessCommandSet(cmds);
        }

        /// <summary>
        /// Updates the Handler Configuration
        /// </summary>
        /// <param name="context">HTTP Context</param>
        protected virtual void UpdateConfig(HttpContext context)
        {

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
            // output.AddAttribute(HtmlTextWriterAttribute.Value, this._config.DefaultGraphURI);
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

        /// <summary>
        /// Handles errors in processing SPARQL Update Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="title">Error title</param>
        /// <param name="update">SPARQL Update</param>
        /// <param name="ex">Error</param>
        protected virtual void HandleErrors(HttpContext context, String title, String update, Exception ex)
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
        protected virtual void HandleErrors(HttpContext context, String title, String update, Exception ex, int statusCode)
        {
            HandlerHelper.HandleUpdateErrors(new WebContext(context), this._config, title, update, ex, statusCode);
        }

        /// <summary>
        /// Gets the Permission action for the SPARQL Update Command
        /// </summary>
        /// <param name="cmd">Update Command</param>
        /// <returns></returns>
        private String GetPermissionAction(SparqlUpdateCommand cmd)
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
    }
}
