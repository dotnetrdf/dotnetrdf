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

#if !NO_WEB && !NO_ASP && !NO_STORAGE

using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Web;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Web;

namespace VDS.RDF.Update.Protocol
{
    /// <summary>
    /// A processor for the SPARQL Graph Store HTTP Protocol which operates by performing the desired operations on some arbitrary underlying Store for which an <see cref="IGenericIOManager">IGenericIOManager</see> is available
    /// </summary>
    public class GenericProtocolProcessor : BaseProtocolProcessor
    {
        private IGenericIOManager _manager;

        /// <summary>
        /// Creates a new Generic Protocol Processor
        /// </summary>
        /// <param name="manager">Generic IO Manager</param>
        public GenericProtocolProcessor(IGenericIOManager manager)
        {
            this._manager = manager;
        }

        /// <summary>
        /// Processes a GET operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <remarks>
        /// Implemented by making a call to <see cref="IGenericIOManager.LoadGraph">LoadGraph()</see> on the underlying <see cref="IGenericIOManager">IGenericIOManager</see>
        /// </remarks>
        public override void ProcessGet(HttpContext context)
        {
            Uri graphUri = this.ResolveGraphUri(context);
            try
            {
                //Send the Graph to the user
                IGraph g = this.GetGraph(graphUri);
                this.SendResultsToClient(context, g);
            }
            catch
            {
                //If there is an error then we assume the Graph does not exist
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }
        }

        /// <summary>
        /// Processes a POST operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <remarks>
        /// <para>
        /// <strong>Warning: </strong> If the underlying <see cref="IGenericIOManager">IGenericIOManager</see> is read-only then this operation returns a 403 Forbidden.
        /// </para>
        /// <para>
        /// Otherwise this is implemented using <see cref="IGenericIOManager.UpdateGraph">UpdateGraph()</see> if updates are supported, if not then the Graph has to be loaded, the POSTed data merged into it and then the Graph is saved again.
        /// </para>
        /// </remarks>
        public override void ProcessPost(HttpContext context)
        {
            //If the Manager is read-only then a 403 Forbidden will be returned
            if (this._manager.IsReadOnly)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            IGraph g = this.ParsePayload(context);
            if (g == null) return;

            Uri graphUri = this.ResolveGraphUri(context, g);

            if (this._manager.UpdateSupported)
            {
                this._manager.UpdateGraph(graphUri, g.Triples, Enumerable.Empty<Triple>());
            }
            else
            {
                //If the Manager does not support update we attempt to get around this by loading the Graph
                //appending the additions to it (via merging) and then saving it back to the Store
                Graph current = new Graph();
                this._manager.LoadGraph(current, graphUri);
                current.Merge(g);
                this._manager.SaveGraph(current);
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
        public override void ProcessPostCreate(HttpContext context)
        {
            //If the Manager is read-only then a 403 Forbidden will be returned
            if (this._manager.IsReadOnly)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            IGraph g = this.ParsePayload(context);
            if (g == null) g = new Graph();

            Uri graphUri = this.MintGraphUri(context, g);
            g.BaseUri = graphUri;

            //Save the Payload under the newly Minted Graph URI
            this._manager.SaveGraph(g);

            //Finally return a 201 Created and a Location header with the new Graph URI
            context.Response.StatusCode = (int)HttpStatusCode.Created;
            try
            {
                context.Response.Headers.Add("Location", graphUri.ToString());
            }
            catch (PlatformNotSupportedException)
            {
                context.Response.AddHeader("Location", graphUri.ToString());
            }
        }

        /// <summary>
        /// Processes a PUT operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <remarks>
        /// <para>
        /// <strong>Warning: </strong> If the underlying <see cref="IGenericIOManager">IGenericIOManager</see> is read-only then this operation returns a 403 Forbidden.
        /// </para>
        /// <para>
        /// Implemented by calling <see cref="IGenericIOManager.SaveGraph">SaveGraph()</see> on the underlying manager
        /// </para>
        /// </remarks>
        public override void ProcessPut(HttpContext context)
        {
            //If the Manager is read-only then a 403 Forbidden will be returned
            if (this._manager.IsReadOnly)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            IGraph g = this.ParsePayload(context);
            Uri graphUri = this.ResolveGraphUri(context, g);
            if (g == null) g = new Graph();
            g.BaseUri = graphUri;

            this._manager.SaveGraph(g);
        }

        /// <summary>
        /// Processes a DELETE operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        /// <remarks>
        /// <para>
        /// <strong>Warning: </strong> If the underlying <see cref="IGenericIOManager">IGenericIOManager</see> is read-only then this operation returns a 403 Forbidden.
        /// </para>
        /// <para>
        /// The delete operation does not explicitly remove the Graph but simply replaces it with an empty Graph
        /// </para>
        /// </remarks>
        public override void ProcessDelete(HttpContext context)
        {
            //If the Manager is read-only then a 403 Forbidden will be returned
            if (this._manager.IsReadOnly)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }


            Uri graphUri = this.ResolveGraphUri(context);
            if (this.HasGraph(graphUri))
            {
                if (this._manager.DeleteSupported)
                {
                    this._manager.DeleteGraph(graphUri);
                }
                else
                {
                    //Have to simulate deletion by replacing with an empty graph
                    IGraph g = new Graph();
                    g.BaseUri = graphUri;

                    this._manager.SaveGraph(g);
                }
            }
            else
            {
                //If no Graph MUST respond 404
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
        }

        /// <summary>
        /// Processes a HEAD operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public override void ProcessHead(HttpContext context)
        {
            //Work out the Graph URI we want to get
            Uri graphUri = this.ResolveGraphUri(context);

            try
            {
                bool exists = this.HasGraph(graphUri);
                if (exists)
                {
                    //Send the Content Type we'd select based on the Accept header to the user
                    String ctype;
                    IRdfWriter writer = MimeTypesHelper.GetWriter(HandlerHelper.GetAcceptTypes(context), out ctype);
                    context.Response.ContentType = ctype;
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            catch (RdfQueryException)
            {
                //If the GetGraph() method errors this implies that the Store does not contain the Graph
                //In such a case we should return a 404
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }
        }

        /// <summary>
        /// Processes a PATCH operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public override void ProcessPatch(HttpContext context)
        {
            //Work out the Graph URI we want to patch
            Uri graphUri = this.ResolveGraphUri(context);

            //If the Request has the SPARQL Update MIME Type then we can process it
            if (context.Request.ContentLength > 0)
            {
                if (context.Request.ContentType.Equals("application/sparql-update"))
                {
                    //Try and parse the SPARQL Update
                    //No error handling here as we assume the calling IHttpHandler does that
                    String patchData;
                    using (StreamReader reader = new StreamReader(context.Request.InputStream))
                    {
                        patchData = reader.ReadToEnd();
                        reader.Close();
                    }
                    SparqlUpdateParser parser = new SparqlUpdateParser();
                    SparqlUpdateCommandSet cmds = parser.ParseFromString(patchData);

                    //Assuming that we've got here i.e. the SPARQL Updates are parseable then
                    //we need to check that they actually affect the relevant Graph
                    if (cmds.Commands.All(c => c.AffectsSingleGraph && c.AffectsGraph(graphUri)))
                    {
                        GenericUpdateProcessor processor = new GenericUpdateProcessor(this._manager);
                        processor.ProcessCommandSet(cmds);
                        processor.Flush();
                    }
                    else
                    {
                        //One/More commands either do no affect a Single Graph or don't affect the Graph
                        //implied by the HTTP Request so give a 422 response
                        context.Response.StatusCode = 422;
                        return;
                    }
                }
                else
                {
                    //Don't understand other forms of PATCH requests
                    context.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                    return;
                }
            }
            else
            {
                //Empty Request is a Bad Request
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
            this._manager.LoadGraph(g, graphUri);
            return g;
        }

        /// <summary>
        /// Determines whether a Graph with the given URI exists
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected override bool HasGraph(Uri graphUri)
        {
            if (this._manager is IQueryableGenericIOManager)
            {
                //Generate an ASK query based on this
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

                Object results = ((IQueryableGenericIOManager)this._manager).Query(ask.ToString());
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
                IGraph g = this.GetGraph(graphUri);
                return !g.IsEmpty;
            }
        }
    }
}

#endif