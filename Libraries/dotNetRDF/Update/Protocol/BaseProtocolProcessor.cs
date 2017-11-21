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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VDS.RDF.Web;
using VDS.RDF.Writing;

namespace VDS.RDF.Update.Protocol
{
    /// <summary>
    /// Abstract Base class for SPARQL Graph Store HTTP Protocol for Graph Management implementations
    /// </summary>
    public abstract class BaseProtocolProcessor 
        : ISparqlHttpProtocolProcessor
    {
        /// <summary>
        /// This is the Pattern that is used to check whether ?default is present in the querystring.  This is needed since IIS does not recognise ?default as being a valid querystring key unless it ends in a = which the specification does not mandate so cannot be assumed
        /// </summary>
        public const String DefaultParameterPattern = "^default$|^default&|&default&|&default$";

        /// <summary>
        /// Processes a GET operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public abstract void ProcessGet(IHttpContext context);

        /// <summary>
        /// Processes a POST operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public abstract void ProcessPost(IHttpContext context);

        /// <summary>
        /// Processes a POST operation which adds triples to a new Graph in the Store and returns the URI of the newly created Graph
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <remarks>
        /// <para>
        /// This operation allows clients to POST data to an endpoint and have it create a Graph and assign a URI for them.
        /// </para>
        /// </remarks>
        public abstract void ProcessPostCreate(IHttpContext context);

        /// <summary>
        /// Processes a PUT operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public abstract void ProcessPut(IHttpContext context);

        /// <summary>
        /// Processes a DELETE operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public abstract void ProcessDelete(IHttpContext context);

        /// <summary>
        /// Processes a HEAD operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public abstract void ProcessHead(IHttpContext context);

        /// <summary>
        /// Processes a PATCH operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public abstract void ProcessPatch(IHttpContext context);

        /// <summary>
        /// Gets the Graph URI that the request should affect
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <returns></returns>
        protected Uri ResolveGraphUri(IHttpContext context)
        {
            Uri graphUri;
            try
            {
                if (context.Request.QueryString["graph"] != null)
                {
                    graphUri = new Uri(context.Request.QueryString["graph"], UriKind.RelativeOrAbsolute);
                    if (!graphUri.IsAbsoluteUri)
                    {
                        throw new SparqlHttpProtocolUriResolutionException("Graph URIs specified using the ?graph parameter must be absolute URIs");
                    }
                }
                else if (context.Request.QueryString.AllKeys.Contains("default") || Regex.IsMatch(context.Request.QueryString.ToString(), DefaultParameterPattern))
                {
                    graphUri = null;
                }
                else
                {
                    graphUri = UriFactory.Create(context.Request.Url.AbsoluteUri);
                }
            }
            catch (UriFormatException)
            {
                throw new SparqlHttpProtocolUriInvalidException();
            }

            return graphUri;
        }

        /// <summary>
        /// Gets the Graph URI that the request should affect
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="g">Graph parsed from the request body</param>
        /// <returns></returns>
        /// <remarks>
        /// The Graph parameter may be null in which case the other overload of this method will be invoked
        /// </remarks>
        protected Uri ResolveGraphUri(IHttpContext context, IGraph g)
        {
            if (g == null) return ResolveGraphUri(context);

            Uri graphUri;
            try
            {
                if (context.Request.QueryString["graph"] != null)
                {
                    graphUri = new Uri(context.Request.QueryString["graph"], UriKind.RelativeOrAbsolute);
                    if (!graphUri.IsAbsoluteUri)
                    {
                        throw new SparqlHttpProtocolUriResolutionException("Graph URIs specified using the ?graph parameter must be absolute URIs");
                    }
                }
                else if (context.Request.QueryString.AllKeys.Contains("default") || Regex.IsMatch(context.Request.QueryString.ToString(), DefaultParameterPattern))
                {
                    graphUri = null;
                }
                else if (g.BaseUri != null)
                {
                    graphUri = g.BaseUri;
                }
                else
                {
                    graphUri = UriFactory.Create(context.Request.Url.AbsoluteUri);
                }
            }
            catch (UriFormatException)
            {
                throw new SparqlHttpProtocolUriInvalidException();
            }

            return graphUri;
        }

        /// <summary>
        /// Generates a new Graph URI that should be used to create a new Graph in the Store in conjunction with the <see cref="ISparqlHttpProtocolProcessor.ProcessPostCreate">ProcessPostCreate()</see> operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="g">Graph parsed from the request body</param>
        /// <returns></returns>
        /// <remarks>
        /// Default behaviour is to mint a URI based on a hash of the Request IP and Date Time.  Implementations can override this method to control URI creation as they desire
        /// </remarks>
        protected virtual Uri MintGraphUri(IHttpContext context, IGraph g)
        {
            String graphID = context.Request.UserHostAddress + "/" + DateTime.Now.ToString("yyyyMMddHHmmssfffffffK");
            graphID = graphID.GetSha256Hash();

            Uri baseUri = UriFactory.Create(context.Request.Url.AbsoluteUri);
            if (baseUri.ToString().EndsWith("/"))
            {
                return new Uri(baseUri, graphID);
            }
            else if (baseUri.Segments.Any())
            {
                return new Uri(baseUri, baseUri.Segments.Last() + "/" + graphID);
            }
            else
            {
                return new Uri(baseUri, graphID);
            }
        }

        /// <summary>
        /// Gets the Graph which can be parsed from the request body
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <returns></returns>
        /// <remarks>
        /// In the event that there is no request body a null will be returned
        /// </remarks>
        protected IGraph ParsePayload(IHttpContext context)
        {
            if (context.Request.ContentLength == 0) return null;

            Graph g = new Graph();
            IRdfReader parser = MimeTypesHelper.GetParser(context.Request.ContentType);
            parser.Load(g, new StreamReader(context.Request.InputStream));
            g.NamespaceMap.Clear();

            return g;
        }

        /// <summary>
        /// Sends the given Graph to the Client via the HTTP Response
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <param name="g">Graph to send</param>
        protected void SendResultsToClient(IHttpContext context, IGraph g)
        {
            IRdfWriter writer;
            String ctype;

            // Look up the MIME Type Definition - if none use GetWriter instead
            MimeTypeDefinition definition = MimeTypesHelper.GetDefinitions(context.GetAcceptTypes()).FirstOrDefault(d => d.CanWriteRdf);
            if (definition != null)
            {
                writer = definition.GetRdfWriter();
                ctype = definition.CanonicalMimeType;
            }
            else
            {
                writer = MimeTypesHelper.GetWriter(context.GetAcceptTypes(), out ctype);
            }

            // Set up the Writer
            if (writer is ICompressingWriter)
            {
                ((ICompressingWriter)writer).CompressionLevel = Options.DefaultCompressionLevel;
            }

            // Send Content to Client
            context.Response.ContentType = ctype;
            if (definition != null)
            {
                context.Response.ContentEncoding = definition.Encoding;
                writer.Save(g, new StreamWriter(context.Response.OutputStream, definition.Encoding));
            }
            else
            {
                writer.Save(g, new StreamWriter(context.Response.OutputStream));
            }
        }

        /// <summary>
        /// Retrieves the Graph with the given URI
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Helper method intended for use by the <see cref="BaseProtocolProcessor.ProcessGet">ProcessGet()</see> and <see cref="BaseProtocolProcessor.ProcessHead">ProcessHead()</see> methods
        /// </para>
        /// </remarks>
        protected abstract IGraph GetGraph(Uri graphUri);

        /// <summary>
        /// Determines whether a Graph with the given URI exists
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected abstract bool HasGraph(Uri graphUri);
    }
}
