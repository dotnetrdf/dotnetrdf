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
using VDS.RDF.Configuration.Permissions;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Web.Configuration;
using VDS.RDF.Writing;

namespace VDS.RDF.Web
{
    /// <summary>
    /// Static Helper class for HTTP Handlers
    /// </summary>
    public static class HandlerHelper
    {
        /// <summary>
        /// Gets the Username of the User for the HTTP Request provided that they are authenticated
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <returns></returns>
        /// <remarks>
        /// <strong>Note: </strong> Unauthenticated Users are treated as guests
        /// </remarks>
        private static String GetUsername(HttpContext context)
        {
            if (context.User != null)
            {
                if (context.User.Identity != null)
                {
                    if (context.User.Identity.IsAuthenticated)
                    {
                        return context.User.Identity.Name;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Checks whether a User is authenticated (or guests are permitted)
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="groups">User Groups to test against</param>
        /// <returns></returns>
        public static bool IsAuthenticated(HttpContext context, IEnumerable<UserGroup> groups)
        {
            String user = HandlerHelper.GetUsername(context);
            if (groups.Any())
            {
                if (user != null && groups.Any(g => g.HasMember(user)))
                {
                    //A Group has the given Member so is authenticated
                    return true;
                }
                else if (!groups.Any(g => g.AllowGuests))
                {
                    //No Groups allow guests so we require authentication
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks whether a User is authenticated (or guests are permitted) and the given action is allowed
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="groups">User Groups to test against</param>
        /// <param name="action">Action to check for permission for</param>
        /// <returns></returns>
        public static bool IsAuthenticated(HttpContext context, IEnumerable<UserGroup> groups, String action)
        {
            if (groups.Any())
            {
                //Does any Group have this Member and allow this action?
                String user = HandlerHelper.GetUsername(context);
                if (user != null && !groups.Any(g => g.HasMember(user) && g.IsActionPermitted(context.Request.HttpMethod)))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return false;
                }
                else if (!groups.Any(g => g.AllowGuests))
                {
                    //No Groups allow guests so we require authentication
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return false;
                }
                else
                {
                    //No Authorization so does a Group that allows guests allow this action?
                    if (!groups.Any(g => g.AllowGuests && g.IsActionPermitted(context.Request.HttpMethod)))
                    {
                        //There are no Groups that allow guests and allow this action so this is forbidden
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return false;
                    }
                }
            }
            return true;
        }

        #region Results Processing

        /// <summary>
        /// Retrieves the Accept Types to be used to determine the content format to be used in responding to requests
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// This method was added in 0.4.1 to allow for the <see cref="NegotiateByFileExtension">NegotiateByFileExtension</see> module to work properly.  Essentially the module may rewrite the <strong>Accept</strong> header of the HTTP request but this will not be visible directly via the <em>AcceptTypes</em> property of the HTTP request as that is fixed at the time the HTTP request is parsed and enters the ASP.Net pipeline.  This method checks whether the <strong>Accept</strong> header is present and if it has been modified from the <em>AcceptTypes</em> property uses the header instead of the property
        /// </para>
        /// </remarks>
        public static String[] GetAcceptTypes(HttpContext context)
        {
            String accept = context.Request.Headers["Accept"];
            if (accept != null && !accept.Equals(String.Empty))
            {
                //If Accept Header is not null or empty then check to see if it matches up with the AcceptTypes
                //array
                String[] acceptTypes = accept.Split(',');
                if (Array.Equals(acceptTypes, context.Request.AcceptTypes))
                {
                    return context.Request.AcceptTypes;
                }
                else
                {
                    return acceptTypes;
                }
            }
            else
            {
                //Otherwise use full accept header
                return context.Request.AcceptTypes;
            }
        }

        /// <summary>
        /// Helper function which returns the Results (Graph/Triple Store/SPARQL Results) back to the Client in one of their accepted formats
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="result">Results of the Sparql Query</param>
        public static void SendToClient(HttpContext context, Object result)
        {
            HandlerHelper.SendToClient(context, result, null);
        }

        /// <summary>
        /// Helper function which returns the Results (Graph/Triple Store/SPARQL Results) back to the Client in one of their accepted formats
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="result">Results of the Sparql Query</param>
        /// <param name="config">Handler Configuration</param>
        public static void SendToClient(HttpContext context, Object result, BaseHandlerConfiguration config)
        {
            MimeTypeDefinition definition = null;
            String ctype = "text/plain";
            String[] acceptTypes = HandlerHelper.GetAcceptTypes(context);

            //Return the Results
            if (result is SparqlResultSet)
            {
                ISparqlResultsWriter sparqlWriter = null;      
       
                //Try and get a MIME Type Definition using the HTTP Requests Accept Header
                if (acceptTypes != null)
                {
                    definition = MimeTypesHelper.GetDefinitions(acceptTypes).FirstOrDefault(d => d.CanWriteSparqlResults);
                } 
                //Try and get the registered Definition for SPARQL Results XML
                if (definition == null)
                {
                    definition = MimeTypesHelper.GetDefinitions(MimeTypesHelper.SparqlXml[0]).FirstOrDefault();
                }
                //If Definition is still null create a temporary definition
                if (definition == null)
                {
                    definition = new MimeTypeDefinition("SPARQL Results XML", MimeTypesHelper.SparqlXml, Enumerable.Empty<String>());
                    definition.SparqlResultsWriterType = typeof(VDS.RDF.Writing.SparqlXmlWriter);
                }
                
                //Set up the Writer appropriately
                sparqlWriter = definition.GetSparqlResultsWriter();
                context.Response.ContentType = definition.CanonicalMimeType;
                HandlerHelper.ApplyWriterOptions(sparqlWriter, config);

                //Clear any existing Response
                context.Response.Clear();

                //Send Result Set to Client
                context.Response.ContentEncoding = definition.Encoding;
                sparqlWriter.Save((SparqlResultSet)result, new StreamWriter(context.Response.OutputStream, definition.Encoding));
            }
            else if (result is IGraph)
            {
                IRdfWriter rdfWriter = null;

                //Try and get a MIME Type Definition using the HTTP Requests Accept Header
                if (acceptTypes != null)
                {
                    definition = MimeTypesHelper.GetDefinitions(acceptTypes).FirstOrDefault(d => d.CanWriteRdf);
                }
                if (definition == null)
                {
                    //If no appropriate definition then use the GetWriter method instead
                    rdfWriter = MimeTypesHelper.GetWriter(acceptTypes, out ctype);
                }
                else
                {
                    rdfWriter = definition.GetRdfWriter();
                }

                //Setup the writer
                if (definition != null) ctype = definition.CanonicalMimeType;
                context.Response.ContentType = ctype;
                HandlerHelper.ApplyWriterOptions(rdfWriter, config);

                //Clear any existing Response
                context.Response.Clear();

                //Send Graph to Client
                if (definition != null)
                {
                    context.Response.ContentEncoding = definition.Encoding;
                    rdfWriter.Save((IGraph)result, new StreamWriter(context.Response.OutputStream, definition.Encoding));
                }
                else 
                {
                    rdfWriter.Save((IGraph)result, new StreamWriter(context.Response.OutputStream));
                }
            }
            else if (result is ITripleStore)
            {
                IStoreWriter storeWriter = null;

                //Try and get a MIME Type Definition using the HTTP Requests Accept Header
                if (acceptTypes != null)
                {
                    definition = MimeTypesHelper.GetDefinitions(acceptTypes).FirstOrDefault(d => d.CanWriteRdfDatasets);
                }
                if (definition == null)
                {
                    //If no appropriate definition then use the GetStoreWriter method instead
                    storeWriter = MimeTypesHelper.GetStoreWriter(acceptTypes, out ctype);
                }
                else
                {
                    storeWriter = definition.GetRdfDatasetWriter();
                }

                //Setup the writer
                if (definition != null) ctype = definition.CanonicalMimeType;
                context.Response.ContentType = ctype;
                HandlerHelper.ApplyWriterOptions(storeWriter, config);

                //Clear any existing Response
                context.Response.Clear();

                //Send Triple Store to Client
                if (definition != null) 
                {
                    context.Response.ContentEncoding = definition.Encoding;
                    storeWriter.Save((ITripleStore)result, new VDS.RDF.Storage.Params.StreamParams(context.Response.OutputStream, definition.Encoding));
                } 
                else
                {
                    storeWriter.Save((ITripleStore)result, new VDS.RDF.Storage.Params.StreamParams(context.Response.OutputStream));
                }
            }
            else if (result is ISparqlDataset)
            {
                //Wrap in a Triple Store and then call self so the Triple Store writing branch of this if gets called instead
                TripleStore store = new TripleStore(new DatasetGraphCollection((ISparqlDataset)result));
                HandlerHelper.SendToClient(context, store, config);
            }
            else
            {
                throw new RdfOutputException("Unexpected Result Object of Type '" + result.GetType().ToString() + "' returned - unable to write Objects of this Type to the HTTP Response");
            }
        }

        /// <summary>
        /// Applies the Writer Options from a Handler Configuration to a Writer
        /// </summary>
        /// <param name="writer">Writer</param>
        /// <param name="config">Handler Configuration</param>
        public static void ApplyWriterOptions(Object writer, BaseHandlerConfiguration config)
        {
            if (config != null)
            {
                //Apply Stylesheet to HTML writers
                if (writer is IHtmlWriter)
                {
                    ((IHtmlWriter)writer).Stylesheet = config.Stylesheet;
                }

                //Apply Compression Options
                if (writer is ICompressingWriter)
                {
                    ((ICompressingWriter)writer).CompressionLevel = config.WriterCompressionLevel;
                }
                if (writer is INamespaceWriter)
                {
                    ((INamespaceWriter)writer).DefaultNamespaces = config.DefaultNamespaces;
                }
                if (writer is IDtdWriter)
                {
                    ((IDtdWriter)writer).UseDtd = config.WriterUseDtds;
                }
                if (writer is IAttributeWriter)
                {
                    ((IAttributeWriter)writer).UseAttributes = config.WriterUseAttributes;
                }
                if (writer is IHighSpeedWriter)
                {
                    ((IHighSpeedWriter)writer).HighSpeedModePermitted = config.WriterHighSpeedMode;
                }
                if (writer is IPrettyPrintingWriter)
                {
                    ((IPrettyPrintingWriter)writer).PrettyPrintMode = config.WriterPrettyPrinting;
                }
            }
        }

        #endregion

        #region Error Handling

        /// <summary>
        /// Handles errors in processing SPARQL Query Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="config">Handler Configuration</param>
        /// <param name="title">Error title</param>
        /// <param name="query">Sparql Query</param>
        /// <param name="ex">Error</param>
        public static void HandleQueryErrors(HttpContext context, BaseHandlerConfiguration config, String title, String query, Exception ex)
        {
            HandleQueryErrors(context, config, title, query, ex, (int)HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Handles errors in processing SPARQL Query Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="config">Handler Configuration</param>
        /// <param name="title">Error title</param>
        /// <param name="query">Sparql Query</param>
        /// <param name="ex">Error</param>
        /// <param name="statusCode">HTTP Status Code to return</param>
        public static void HandleQueryErrors(HttpContext context, BaseHandlerConfiguration config, String title, String query, Exception ex, int statusCode)
        {
            //Clear any existing Response and set our HTTP Status Code
            context.Response.Clear();
            context.Response.StatusCode = statusCode;

            if (config != null)
            {
                //If not showing errors then we won't return our custom error description
                if (!config.ShowErrors) return;
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
        /// Handles errors in processing SPARQL Update Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="config">Handler Configuration</param>
        /// <param name="title">Error title</param>
        /// <param name="update">SPARQL Update</param>
        /// <param name="ex">Error</param>
        public static void HandleUpdateErrors(HttpContext context, BaseHandlerConfiguration config, String title, String update, Exception ex)
        {
            HandleUpdateErrors(context, config, title, update, ex, (int)HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Handles errors in processing SPARQL Update Requests
        /// </summary>
        /// <param name="context">Context of the HTTP Request</param>
        /// <param name="config">Handler Configuration</param>
        /// <param name="title">Error title</param>
        /// <param name="update">SPARQL Update</param>
        /// <param name="ex">Error</param>
        /// <param name="statusCode">HTTP Status Code to return</param>
        public static void HandleUpdateErrors(HttpContext context, BaseHandlerConfiguration config, String title, String update, Exception ex, int statusCode)
        {
            //Clear any existing Response
            context.Response.Clear();

            if (config != null)
            {
                if (!config.ShowErrors)
                {
                    context.Response.StatusCode = statusCode;
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
            if (update != null && !update.Equals(String.Empty))
            {
                String[] lines = update.Split('\n');
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

        #endregion

        #region HTTP Headers and Caching

        /// <summary>
        /// Computes the ETag for a Graph
        /// </summary>
        /// <param name="g">Graph</param>
        /// <returns></returns>
        public static String GetETag(this IGraph g)
        {
            List<Triple> ts = g.Triples.ToList();
            ts.Sort();

            StringBuilder hash = new StringBuilder();
            foreach (Triple t in ts)
            {
                hash.AppendLine(t.GetHashCode().ToString());
            }
            String h = hash.ToString().GetHashCode().ToString();

            SHA1 sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(h));
            hash = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                hash.Append(b.ToString("x2"));
            }
            return hash.ToString();
        }

        /// <summary>
        /// Checks whether the HTTP Request contains caching headers that means a 304 Modified response can be sent
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="etag">ETag</param>
        /// <param name="lastModified">Last Modified</param>
        /// <returns>True if a 304 Not Modified can be sent</returns>
        public static bool CheckCachingHeaders(HttpContext context, String etag, DateTime? lastModified)
        {
            if (context == null) return false;
            if (etag == null && lastModified == null) return false;

            try
            {
                if (etag != null)
                {
                    //If ETags match then can send a 304 Not Modified
                    if (etag.Equals(context.Request.Headers["If-None-Match"])) return true;
                }

                if (lastModified != null)
                {
                    String requestLastModifed = context.Request.Headers["If-Modified-Since"];
                    if (requestLastModifed != null)
                    {
                        DateTime test = DateTime.Parse(requestLastModifed);
                        //If the resource has not been modified after the date the request gave then can send a 304 Not Modified
                        if (lastModified < test) return true;
                    }
                }
             }
            catch
            {
                //In the event of an error continue processing the request normally
                return false;
            }
            return false;
        }

        /// <summary>
        /// Adds ETag and/or Last-Modified headers as appropriate to a response
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="etag">ETag</param>
        /// <param name="lastModified">Last Modified</param>
        public static void AddCachingHeaders(HttpContext context, String etag, DateTime? lastModified)
        {
            if (context == null) return;
            if (etag == null && lastModified == null) return;

            try
            {
                if (etag != null)
                {
                    try
                    {
                        context.Response.Headers.Add("ETag", etag);
                    }
                    catch (PlatformNotSupportedException)
                    {
                        context.Response.AddHeader("ETag", etag);
                    }
                }

                if (lastModified != null)
                {
                    try
                    {
                        context.Response.Headers.Add("Last-Modified", ((DateTime)lastModified).ToRfc2822());
                    }
                    catch (PlatformNotSupportedException)
                    {
                        context.Response.AddHeader("Last-Modified", ((DateTime)lastModified).ToRfc2822());
                    }
                }
            }
            catch
            {
                //In the event of an error then the Headers won't get attached
            }
        }

        /// <summary>
        /// Adds the Standard Custom Headers that dotNetRDF attaches to all responses from it's Handlers
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="config">Handler Configuration</param>
        public static void AddStandardHeaders(HttpContext context, BaseHandlerConfiguration config)
        {
            try
            {
                context.Response.Headers.Add("X-dotNetRDF-Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            }
            catch (PlatformNotSupportedException)
            {
                context.Response.AddHeader("X-dotNetRDF-Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            }
            if (config.IsCorsEnabled) AddCorsHeaders(context);
        }

        /// <summary>
        /// Adds CORS headers which are needed to allow JS clients to access RDF/SPARQL endpoints powered by dotNetRDF
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public static void AddCorsHeaders(HttpContext context)
        {
            try
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            }
            catch (PlatformNotSupportedException)
            {
                context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            }
        }

        /// <summary>
        /// Converts a DateTime to RFC 2822 format
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private static String ToRfc2822(this DateTime dt)
        {
            return dt.ToString("ddd, d MMM yyyy HH:mm:ss K");
        }

        #endregion
    }
}

#endif