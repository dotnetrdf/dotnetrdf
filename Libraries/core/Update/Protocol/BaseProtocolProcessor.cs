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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using VDS.RDF.Web;
using VDS.RDF.Writing;

namespace VDS.RDF.Update.Protocol
{
    /// <summary>
    /// Abstract Base class for SPARQL Graph Store HTTP Protocol for Graph Management implementations
    /// </summary>
    public abstract class BaseProtocolProcessor : ISparqlHttpProtocolProcessor
    {
        /// <summary>
        /// This is the Pattern that is used to check whether ?default is present in the querystring.  This is needed since IIS does not recognise ?default as being a valid querystring key unless it ends in a = which the specification does not mandate so cannot be assumed
        /// </summary>
        internal const String DefaultParameterPattern = "^default$|^default&|&default&|&default$";

        /// <summary>
        /// Processes a GET operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public abstract void ProcessGet(HttpContext context);

        /// <summary>
        /// Processes a POST operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public abstract void ProcessPost(HttpContext context);

        /// <summary>
        /// Processes a POST operation which adds triples to a new Graph in the Store and returns the URI of the newly created Graph
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <remarks>
        /// <para>
        /// This operation allows clients to POST data to an endpoint and have it create a Graph and assign a URI for them.
        /// </para>
        /// </remarks>
        public abstract void ProcessPostCreate(HttpContext context);

        /// <summary>
        /// Processes a PUT operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public abstract void ProcessPut(HttpContext context);

        /// <summary>
        /// Processes a DELETE operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public abstract void ProcessDelete(HttpContext context);

        /// <summary>
        /// Processes a HEAD operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public abstract void ProcessHead(HttpContext context);

        /// <summary>
        /// Processes a PATCH operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public abstract void ProcessPatch(HttpContext context);

        /// <summary>
        /// Gets the Graph URI that the request should affect
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <returns></returns>
        protected Uri ResolveGraphUri(HttpContext context)
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
                    else if (graphUri.ToString().Equals(GraphCollection.DefaultGraphUri))
                    {
                        return null;
                    }
                }
                else if (context.Request.QueryString.AllKeys.Contains("default") || Regex.IsMatch(context.Request.QueryString.ToString(), DefaultParameterPattern))
                {
                    graphUri = null;
                }
                else
                {
                    graphUri = new Uri(context.Request.Url.AbsoluteUri);
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
        protected Uri ResolveGraphUri(HttpContext context, IGraph g)
        {
            if (g == null) return this.ResolveGraphUri(context);

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
                    else if (graphUri.ToString().Equals(GraphCollection.DefaultGraphUri))
                    {
                        return null;
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
                    graphUri = new Uri(context.Request.Url.AbsoluteUri);
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
        protected virtual Uri MintGraphUri(HttpContext context, IGraph g)
        {
            String graphID = context.Request.UserHostAddress + "/" + DateTime.Now.ToString("yyyyMMddHHmmssfffffffK");
            graphID = graphID.GetSha256Hash();

            Uri baseUri = new Uri(context.Request.Url.AbsoluteUri);
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
        protected IGraph ParsePayload(HttpContext context)
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
        protected void SendResultsToClient(HttpContext context, IGraph g)
        {
            IRdfWriter writer;
            String ctype;

            //Look up the MIME Type Definition - if none use GetWriter instead
            MimeTypeDefinition definition = MimeTypesHelper.GetDefinitions(HandlerHelper.GetAcceptTypes(context)).FirstOrDefault(d => d.CanWriteRdf);
            if (definition != null)
            {
                writer = definition.GetRdfWriter();
                ctype = definition.CanonicalMimeType;
            }
            else
            {
                writer = MimeTypesHelper.GetWriter(HandlerHelper.GetAcceptTypes(context), out ctype);
            }

            //Set up the Writer
            if (writer is ICompressingWriter)
            {
                ((ICompressingWriter)writer).CompressionLevel = Options.DefaultCompressionLevel;
            }

            //Send Content to Client
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

#endif