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
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Web;

namespace VDS.RDF.Update.Protocol
{
    /// <summary>
    /// A processor for the SPARQL Graph Store HTTP Protocol which operates by performing the desired operations on some arbitrary underlying Store for which an <see cref="IStorageProvider">IStorageProvider</see> is available
    /// </summary>
    public class GenericProtocolProcessor
        : BaseProtocolProcessor
    {
        private IStorageProvider _manager;

        /// <summary>
        /// Creates a new Generic Protocol Processor
        /// </summary>
        /// <param name="manager">Generic IO Manager</param>
        public GenericProtocolProcessor(IStorageProvider manager)
        {
            _manager = manager;
        }

        /// <summary>
        /// Processes a GET operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <remarks>
        /// Implemented by making a call to <see cref="IStorageProvider.LoadGraph(IGraph, Uri)">LoadGraph()</see> on the underlying <see cref="IStorageProvider">IStorageProvider</see>
        /// </remarks>
        public override void ProcessGet(IHttpContext context)
        {
            var graphUri = ResolveGraphUri(context);
            try
            {
                // Send the Graph to the user
                var g = GetGraph(graphUri);
                SendResultsToClient(context, g);
            }
            catch
            {
                // If there is an error then we assume the Graph does not exist
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }

        /// <summary>
        /// Processes a POST operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <remarks>
        /// <para>
        /// <strong>Warning: </strong> If the underlying <see cref="IStorageProvider">IStorageProvider</see> is read-only then this operation returns a 403 Forbidden.
        /// </para>
        /// <para>
        /// Otherwise this is implemented using <see cref="IStorageProvider.UpdateGraph(Uri,System.Collections.Generic.IEnumerable{VDS.RDF.Triple},IEnumerable{Triple})">UpdateGraph()</see> if updates are supported, if not then the Graph has to be loaded, the POSTed data merged into it and then the Graph is saved again.
        /// </para>
        /// </remarks>
        public override void ProcessPost(IHttpContext context)
        {
            // If the Manager is read-only then a 403 Forbidden will be returned
            if (_manager.IsReadOnly)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            IGraph g = ParsePayload(context);
            if (g == null) return;

            Uri graphUri = ResolveGraphUri(context, g);

            if (_manager.UpdateSupported)
            {
                _manager.UpdateGraph(graphUri, g.Triples, Enumerable.Empty<Triple>());
            }
            else
            {
                // If the Manager does not support update we attempt to get around this by loading the Graph
                // appending the additions to it (via merging) and then saving it back to the Store
                Graph current = new Graph();
                _manager.LoadGraph(current, graphUri);
                current.Merge(g);
                _manager.SaveGraph(current);
            }
        }

        /// <summary>
        /// Processes a POST operation which adds triples to a new Graph in the Store and returns the URI of the newly created Graph
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <remarks>
        /// <para>
        /// This operation allows clients to POST data to an endpoint and have it create a Graph and assign a URI for them.
        /// </para>
        /// </remarks>
        public override void ProcessPostCreate(IHttpContext context)
        {
            // If the Manager is read-only then a 403 Forbidden will be returned
            if (_manager.IsReadOnly)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            IGraph g = ParsePayload(context);
            if (g == null) g = new Graph();

            Uri graphUri = MintGraphUri(context, g);
            g.BaseUri = graphUri;

            // Save the Payload under the newly Minted Graph URI
            _manager.SaveGraph(g);

            // Finally return a 201 Created and a Location header with the new Graph URI
            context.Response.StatusCode = (int)HttpStatusCode.Created;
            try
            {
                context.Response.Headers.Add("Location", graphUri.AbsoluteUri);
            }
            catch (PlatformNotSupportedException)
            {
                context.Response.AddHeader("Location", graphUri.AbsoluteUri);
            }
        }

        /// <summary>
        /// Processes a PUT operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <remarks>
        /// <para>
        /// <strong>Warning: </strong> If the underlying <see cref="IStorageProvider">IStorageProvider</see> is read-only then this operation returns a 403 Forbidden.
        /// </para>
        /// <para>
        /// Implemented by calling <see cref="IStorageProvider.SaveGraph">SaveGraph()</see> on the underlying manager
        /// </para>
        /// </remarks>
        public override void ProcessPut(IHttpContext context)
        {
            // If the Manager is read-only then a 403 Forbidden will be returned
            if (_manager.IsReadOnly)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            IGraph g = ParsePayload(context);
            Uri graphUri = ResolveGraphUri(context, g);
            if (g == null) g = new Graph();
            g.BaseUri = graphUri;

            _manager.SaveGraph(g);
        }

        /// <summary>
        /// Processes a DELETE operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <remarks>
        /// <para>
        /// <strong>Warning: </strong> If the underlying <see cref="IStorageProvider">IStorageProvider</see> is read-only then this operation returns a 403 Forbidden.
        /// </para>
        /// <para>
        /// The delete operation does not explicitly remove the Graph but simply replaces it with an empty Graph
        /// </para>
        /// </remarks>
        public override void ProcessDelete(IHttpContext context)
        {
            // If the Manager is read-only then a 403 Forbidden will be returned
            if (_manager.IsReadOnly)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }


            Uri graphUri = ResolveGraphUri(context);
            if (HasGraph(graphUri))
            {
                if (_manager.DeleteSupported)
                {
                    _manager.DeleteGraph(graphUri);
                }
                else
                {
                    // Have to simulate deletion by replacing with an empty graph
                    IGraph g = new Graph
                    {
                        BaseUri = graphUri,
                    };

                    _manager.SaveGraph(g);
                }
            }
            else
            {
                // If no Graph MUST respond 404
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }

        /// <summary>
        /// Processes a HEAD operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public override void ProcessHead(IHttpContext context)
        {
            // Work out the Graph URI we want to get
            Uri graphUri = ResolveGraphUri(context);

            try
            {
                bool exists = HasGraph(graphUri);
                if (exists)
                {
                    // Send the Content Type we'd select based on the Accept header to the user
                    var writer = MimeTypesHelper.GetWriter(context.GetAcceptTypes(), out var ctype);
                    context.Response.ContentType = ctype;
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            catch (RdfQueryException)
            {
                // If the GetGraph() method errors this implies that the Store does not contain the Graph
                // In such a case we should return a 404
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }
        }

        /// <summary>
        /// Processes a PATCH operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public override void ProcessPatch(IHttpContext context)
        {
            // Work out the Graph URI we want to patch
            Uri graphUri = ResolveGraphUri(context);

            // If the Request has the SPARQL Update MIME Type then we can process it
            if (context.Request.ContentLength > 0)
            {
                if (context.Request.ContentType.Equals("application/sparql-update"))
                {
                    // Try and parse the SPARQL Update
                    // No error handling here as we assume the calling IHttpHandler does that
                    String patchData;
                    using (StreamReader reader = new StreamReader(context.Request.InputStream))
                    {
                        patchData = reader.ReadToEnd();
                        reader.Close();
                    }
                    SparqlUpdateParser parser = new SparqlUpdateParser();
                    SparqlUpdateCommandSet cmds = parser.ParseFromString(patchData);

                    // Assuming that we've got here i.e. the SPARQL Updates are parseable then
                    // we need to check that they actually affect the relevant Graph
                    if (cmds.Commands.All(c => c.AffectsSingleGraph && c.AffectsGraph(graphUri)))
                    {
                        GenericUpdateProcessor processor = new GenericUpdateProcessor(_manager);
                        processor.ProcessCommandSet(cmds);
                        processor.Flush();
                    }
                    else
                    {
                        // One/More commands either do no affect a Single Graph or don't affect the Graph
                        // implied by the HTTP Request so give a 422 response
                        context.Response.StatusCode = 422;
                        return;
                    }
                }
                else
                {
                    // Don't understand other forms of PATCH requests
                    context.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                    return;
                }
            }
            else
            {
                // Empty Request is a Bad Request
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }
        }

        /// <summary>
        /// Retrieves the Graph with the given URI
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected override IGraph GetGraph(Uri graphUri)
        {
            Graph g = new Graph();
            _manager.LoadGraph(g, graphUri);
            return g;
        }

        /// <summary>
        /// Determines whether a Graph with the given URI exists
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected override bool HasGraph(Uri graphUri)
        {
            if (_manager is IQueryableStorage)
            {
                // Generate an ASK query based on this
                SparqlParameterizedString ask = new SparqlParameterizedString();
                if (graphUri != null)
                {
                    ask.CommandText = "ASK WHERE { GRAPH @graph { ?s ?p ?o . } }";
                    ask.SetUri("graph", graphUri);
                }
                else
                {
                    ask.CommandText = "ASK WHERE { ?s ?p ?o }";
                }

                Object results = ((IQueryableStorage)_manager).Query(ask.ToString());
                if (results is SparqlResultSet)
                {
                    return ((SparqlResultSet)results).Result;
                }
                else
                {
                    throw new SparqlHttpProtocolException("Failed to retrieve a Boolean Result since the query processor did not return a valid SPARQL Result Set as expected");
                }
            }
            else
            {
                IGraph g = GetGraph(graphUri);
                return !g.IsEmpty;
            }
        }
    }
}
