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
using System.Text;
using System.Web;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Web;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Update.Protocol
{
    /// <summary>
    /// A processor for the SPARQL Graph Store HTTP Protocol which operates by translating the requests into SPARQL Query/Update commands as specified by the SPARQL Graph Store HTTP Protocol specification and passing the generated commands to a <see cref="ISparqlUpdateProcessor">ISparqlUpdateProcessor</see> which will handle the actual application of the updates
    /// </summary>
    /// <remarks>
    /// The conversion from HTTP operation to SPARQL Query/Update is as defined in the <a href="http://www.w3.org/TR/sparql11-http-rdf-update/">SPARQL 1.1 Graph Store HTTP Protocol</a> specification
    /// </remarks>
    public class ProtocolToUpdateProcessor : BaseProtocolProcessor
    {
        private ISparqlQueryProcessor _queryProcessor;
        private ISparqlUpdateProcessor _updateProcessor;
        private SparqlUpdateParser _parser = new SparqlUpdateParser();

        /// <summary>
        /// Creates a new Protocol to Update Processor
        /// </summary>
        /// <param name="queryProcessor">Query Processor</param>
        /// <param name="updateProcessor">Update Processor</param>
        public ProtocolToUpdateProcessor(ISparqlQueryProcessor queryProcessor, ISparqlUpdateProcessor updateProcessor)
        {
            this._queryProcessor = queryProcessor;
            this._updateProcessor = updateProcessor;
        }

        /// <summary>
        /// Processes a GET operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public override void ProcessGet(HttpContext context)
        {
            //Work out the Graph URI we want to get
            Uri graphUri = this.ResolveGraphUri(context);

            try
            {
                //Send the Graph to the user
                IGraph g = this.GetGraph(graphUri);
                this.SendResultsToClient(context, g);
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
        /// Processes a POST operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public override void ProcessPost(HttpContext context)
        {
            //Get the payload assuming there is one
            IGraph g = this.ParsePayload(context);

            if (g == null)
            {
                //Q: What should the behaviour be when the payload is null for a POST?  Assuming a 200 OK response
                return;
            }

            //Get the Graph URI of the Graph to be added
            Uri graphUri = this.ResolveGraphUri(context, g);

            //First we need a 

            //Generate an INSERT DATA command for the POST
            StringBuilder insert = new StringBuilder();
            if (graphUri != null)
            {
                insert.AppendLine("INSERT DATA { GRAPH @graph {");
            }
            else
            {
                insert.AppendLine("INSERT DATA {");
            }

            TurtleFormatter formatter = new TurtleFormatter(g.NamespaceMap);
            foreach (Triple t in g.Triples)
            {
                insert.AppendLine(t.ToString(formatter));
            }

            if (graphUri != null)
            {
                insert.AppendLine("} }");
            }
            else
            {
                insert.AppendLine("}");
            }

            //Parse and evaluate the command
            SparqlParameterizedString insertCmd = new SparqlParameterizedString(insert.ToString());
            insertCmd.Namespaces = g.NamespaceMap;
            if (graphUri != null) insertCmd.SetUri("graph", graphUri);
            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(insertCmd);
            this._updateProcessor.ProcessCommandSet(cmds);
            this._updateProcessor.Flush();
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
            //Get the payload assuming there is one
            IGraph g = this.ParsePayload(context);

            //Mint a URI for the Graph
            Uri graphUri = this.MintGraphUri(context, g);

            //First generate a CREATE to ensure that the Graph exists
            //We don't do a CREATE SILENT as this operation is supposed to generate a new Graph URI
            //so if MintGraphUri() fails to deliver a unused Graph URI then the operation should fail
            StringBuilder insert = new StringBuilder();
            insert.AppendLine("CREATE GRAPH @graph ;");

            //Then Generate an INSERT DATA command for the actual POST
            //Note that if the payload is empty this still has the effect of creating a Graph
            if (g != null)
            {
                insert.AppendLine("INSERT DATA { GRAPH @graph {");
                TurtleFormatter formatter = new TurtleFormatter(g.NamespaceMap);
                foreach (Triple t in g.Triples)
                {
                    insert.AppendLine(t.ToString(formatter));
                }
                insert.AppendLine("} }");
            }

            //Parse and evaluate the command
            SparqlParameterizedString insertCmd = new SparqlParameterizedString(insert.ToString());
            insertCmd.Namespaces = g.NamespaceMap;
            insertCmd.SetUri("graph", graphUri);
            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(insertCmd);
            this._updateProcessor.ProcessCommandSet(cmds);
            this._updateProcessor.Flush();

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
        public override void ProcessPut(HttpContext context)
        {
            //Get the payload assuming there is one
            IGraph g = this.ParsePayload(context);

            //Get the Graph URI of the Graph to be added
            Uri graphUri = this.ResolveGraphUri(context, g);

            //Determine whether the Graph already exists or not, if it doesn't then we have to send a 201 Response
            bool created = false;
            try
            {
                SparqlQueryParser parser = new SparqlQueryParser();
                SparqlParameterizedString graphExistsQuery = new SparqlParameterizedString();
                graphExistsQuery.CommandText = "ASK WHERE { GRAPH @graph { } }";
                graphExistsQuery.SetUri("graph", graphUri);

                Object temp = this._queryProcessor.ProcessQuery(parser.ParseFromString(graphExistsQuery));
                if (temp is SparqlResultSet)
                {
                    created = !((SparqlResultSet)temp).Result;
                }
            }
            catch
            {
                //If any error occurs assume the Graph doesn't exist and so we'll return a 201 created
                created = true;
            }            

            //Generate a set of commands based upon this
            StringBuilder cmdSequence = new StringBuilder();
            if (graphUri != null)
            {
                cmdSequence.AppendLine("DROP SILENT GRAPH @graph ;");
                cmdSequence.Append("CREATE SILENT GRAPH @graph");
            }
            else
            {
                cmdSequence.Append("DROP SILENT DEFAULT");
            }
            if (g != null)
            {
                cmdSequence.AppendLine(" ;");
                if (graphUri != null)
                {
                    cmdSequence.AppendLine("INSERT DATA { GRAPH @graph {");
                }
                else
                {
                    cmdSequence.AppendLine("INSERT DATA { ");
                }

                TurtleFormatter formatter = new TurtleFormatter(g.NamespaceMap);
                foreach (Triple t in g.Triples)
                {
                    cmdSequence.AppendLine(t.ToString(formatter));
                }

                if (graphUri != null)
                {
                    cmdSequence.AppendLine("} }");
                }
                else
                {
                    cmdSequence.AppendLine("}");
                }
            }

            SparqlParameterizedString put = new SparqlParameterizedString(cmdSequence.ToString());
            put.Namespaces = g.NamespaceMap;
            if (graphUri != null) put.SetUri("graph", graphUri);
            SparqlUpdateCommandSet putCmds = this._parser.ParseFromString(put);
            this._updateProcessor.ProcessCommandSet(putCmds);
            this._updateProcessor.Flush();

            //Return a 201 if required, otherwise the default behaviour of returning a 200 will occur automatically
            if (created)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Created;
            }
        }

        /// <summary>
        /// Processes a DELETE operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public override void ProcessDelete(HttpContext context)
        {
            //Get the Graph URI of the Graph to delete
            Uri graphUri = this.ResolveGraphUri(context);

            //Must return a 404 if the Graph doesn't exist
            if (!this.HasGraph(graphUri))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            //Generate a DROP GRAPH command based on this
            SparqlParameterizedString drop = new SparqlParameterizedString("DROP ");
            if (graphUri != null)
            {
                drop.CommandText += "GRAPH @graph";
                drop.SetUri("graph", graphUri);
            }
            else
            {
                drop.CommandText += "DEFAULT";
            }
            SparqlUpdateCommandSet dropCmd = this._parser.ParseFromString(drop);
            this._updateProcessor.ProcessCommandSet(dropCmd);
            this._updateProcessor.Flush();
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
                    SparqlUpdateCommandSet cmds = this._parser.ParseFromString(patchData);

                    //Assuming that we've got here i.e. the SPARQL Updates are parseable then
                    //we need to check that they actually affect the relevant Graph
                    if (cmds.Commands.All(c => c.AffectsSingleGraph && c.AffectsGraph(graphUri)))
                    {
                        this._updateProcessor.ProcessCommandSet(cmds);
                        this._updateProcessor.Flush();
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
            //Generate a CONSTRUCT query based on the Graph URI
            SparqlParameterizedString construct = new SparqlParameterizedString();
            if (graphUri != null)
            {
                construct.CommandText = "CONSTRUCT { ?s ?p ?o . } WHERE { GRAPH @graph { ?s ?p ?o . } }";
                construct.SetUri("graph", graphUri);
            }
            else
            {
                construct.CommandText = "CONSTRUCT { ?s ?p ?o . } WHERE { ?s ?p ?o }";
            }
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(construct);

            Object results = this._queryProcessor.ProcessQuery(q);
            if (results is IGraph)
            {
                return (IGraph)results;
            }
            else
            {
                throw new SparqlHttpProtocolException("Failed to retrieve a Graph since the query processor did not return a valid Graph as expected");
            }
        }

        /// <summary>
        /// Determines whether a Graph with the given URI exists
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected override bool HasGraph(Uri graphUri)
        {
            //Generate an ASK query based on the Graph URI
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
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(ask);

            Object results = this._queryProcessor.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                return ((SparqlResultSet)results).Result;
            }
            else
            {
                throw new SparqlHttpProtocolException("Failed to retrieve a Boolean Result since the query processor did not return a valid SPARQL Result Set as expected");
            }
        }
    }
}

#endif