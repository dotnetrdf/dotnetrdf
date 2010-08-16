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

#if !NO_WEB

using System;
using System.IO;
using System.Text;
using System.Web;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace VDS.RDF.Update.Protocol
{
    /// <summary>
    /// A processor for the SPARQL Uniform HTTP Protocol which operates by translating the requests into SPARQL Query/Update commands as specified by the SPARQL Uniform HTTP Protocol specification and passing the generated commands to a <see cref="ISparqlUpdateProcessor">ISparqlUpdateProcessor</see> which will handle the actual application of the updates
    /// </summary>
    /// <remarks>
    /// The conversion from HTTP operation to SPARQL Query/Update is as defined in the <a href="http://www.w3.org/TR/sparql11-http-rdf-update/">SPARQL 1.1 Uniform HTTP Protocol for Managing RDF Graphs</a> specification
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

            //Then generate a CONSTRUCT query based on this
            SparqlParameterizedString construct = new SparqlParameterizedString("CONSTRUCT { ?s ?p ?o . } WHERE { GRAPH @graph { ?s ?p ?o . } }");
            construct.SetUri("graph", graphUri);
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(construct);

            try
            {
                Object results = this._queryProcessor.ProcessQuery(q);
                if (results is Graph)
                {
                    //Send the Graph to the user
                    Graph g = (Graph)results;
                    String ctype;
                    IRdfWriter writer = MimeTypesHelper.GetWriter(context.Request.AcceptTypes, out ctype);
                    context.Response.ContentType = ctype;
                    writer.Save(g, new StreamWriter(context.Response.OutputStream));
                }
                else
                {
                    throw new SparqlHttpProtocolException("Failed to process a HTTP GET protocol operation since the query processor did not return a valid Graph as expected");
                }
            }
            catch (RdfQueryException)
            {
                //Q: In the event of a query exception what is the appropriate response?
                throw;
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
            Uri graphUri;
            try
            {
                graphUri = this.ResolveGraphUri(context, g);
            }
            catch (SparqlHttpProtocolException)
            {
                //Q: What do we need to do here? Should we be generating a new Graph in the event of an error?
                throw;
            }

            //Generate an INSERT DATA command for the POST
            StringBuilder insert = new StringBuilder();
            insert.AppendLine("INSERT DATA { GRAPH @graph {");

            System.IO.StringWriter writer = new System.IO.StringWriter(insert);
            CompressingTurtleWriter ttlwriter = new CompressingTurtleWriter(WriterCompressionLevel.High);
            ttlwriter.Save(g, writer);
            insert.AppendLine("} }");

            //Parse and evaluate the command
            SparqlParameterizedString insertCmd = new SparqlParameterizedString(insert.ToString());
            insertCmd.SetUri("graph", graphUri);
            SparqlUpdateCommandSet cmds = this._parser.ParseFromString(insertCmd);
            this._updateProcessor.ProcessCommandSet(cmds);
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

            //Generate a set of commands based upon this
            StringBuilder cmdSequence = new StringBuilder();
            cmdSequence.AppendLine("DROP SILENT GRAPH @graph ;");
            cmdSequence.Append("CREATE SILENT GRAPH @graph");
            if (g != null)
            {
                cmdSequence.AppendLine(" ;");
                cmdSequence.AppendLine("INSERT DATA { GRAPH @graph {");

                System.IO.StringWriter writer = new System.IO.StringWriter(cmdSequence);
                CompressingTurtleWriter ttlwriter = new CompressingTurtleWriter(WriterCompressionLevel.High);
                ttlwriter.Save(g, writer);
                cmdSequence.AppendLine("} }");
            }

            SparqlParameterizedString put = new SparqlParameterizedString(cmdSequence.ToString());
            put.SetUri("graph", graphUri);
            SparqlUpdateCommandSet putCmds = this._parser.ParseFromString(put);
            this._updateProcessor.ProcessCommandSet(putCmds);
        }

        /// <summary>
        /// Processes a DELETE operation
        /// </summary>
        /// <param name="context">HTTP Context</param>
        public override void ProcessDelete(HttpContext context)
        {
            //Get the Graph URI of the Graph to delete
            Uri graphUri = this.ResolveGraphUri(context);

            //Generate a DROP GRAPH command based on this
            SparqlParameterizedString drop = new SparqlParameterizedString("DROP GRAPH @graph");
            drop.SetUri("graph", graphUri);
            SparqlUpdateCommandSet dropCmd = this._parser.ParseFromString(drop);
            this._updateProcessor.ProcessCommandSet(dropCmd);
        }
    }
}

#endif