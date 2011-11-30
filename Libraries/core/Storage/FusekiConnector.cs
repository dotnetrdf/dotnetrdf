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

#if !NO_STORAGE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Class for connecting to any dataset that can be exposed via Fuseki
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses all three Services provided by a Fuseki instance - Query, Update and HTTP Update
    /// </para>
    /// </remarks>
    public class FusekiConnector 
        : SparqlHttpProtocolConnector, IUpdateableGenericIOManager, IConfigurationSerializable
    {
        private SparqlFormatter _formatter = new SparqlFormatter();
        private String _updateUri;
        private String _queryUri;

        private const String FusekiDefaultGraphUri = "?default";

        /// <summary>
        /// Creates a new connection to a Fuseki Server
        /// </summary>
        /// <param name="serviceUri">The /data URI of the Fuseki Server</param>
        public FusekiConnector(Uri serviceUri)
            : this(serviceUri.ToSafeString()) { }

        /// <summary>
        /// Creates a new connection to a Fuseki Server
        /// </summary>
        /// <param name="serviceUri">The /data URI of the Fuseki Server</param>
        public FusekiConnector(String serviceUri)
            : base(serviceUri) 
        {
            if (!serviceUri.ToString().EndsWith("/data")) throw new ArgumentException("This does not appear to be a valid Fuseki Server URI, you must provide the URI that ends with /data", "serviceUri");

            this._updateUri = serviceUri.Substring(0, serviceUri.Length - 4) + "update";
            this._queryUri = serviceUri.Substring(0, serviceUri.Length - 4) + "query";
        }

        /// <summary>
        /// Returns that Listing Graphs is supported
        /// </summary>
        public override bool ListGraphsSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the List of Graphs from the store
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Uri> ListGraphs()
        {
            try
            {
                SparqlResultSet results = this.Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }") as SparqlResultSet;
                if (results != null)
                {
                    List<Uri> uris = new List<Uri>();
                    foreach (SparqlResult r in results)
                    {
                        if (r.HasValue("g"))
                        {
                            INode n = r["g"];
                            if (n != null && n.NodeType == NodeType.Uri)
                            {
                                uris.Add(((IUriNode)n).Uri);
                            }
                        }
                    }
                    return uris;
                }
                else
                {
                    throw new RdfStorageException("Tried to list graphs from Fuseki but failed to get a SPARQL Result Set as expected");
                }
            }
                catch (RdfStorageException)
            {
                    throw;
                }
            catch (Exception ex)
            {
                throw new RdfStorageException("An error occurred while trying to list graphs, see inner exception for details", ex);
            }
        }

        /// <summary>
        /// Gets the IO Behaviour of the Store
        /// </summary>
        public override IOBehaviour IOBehaviour
        {
            get
            {
                return base.IOBehaviour | IOBehaviour.CanUpdateDeleteTriples;
            }
        }

        /// <summary>
        /// Returns that Triple level updates are supported using Fuseki
        /// </summary>
        public override bool UpdateSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Updates a Graph in the Fuseki store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public override void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            try
            {
                String graph = (graphUri != null && !graphUri.Equals(String.Empty)) ? "GRAPH <" + this._formatter.FormatUri(graphUri) + "> {" : String.Empty;
                StringBuilder update = new StringBuilder();

                if (additions != null)
                {
                    if (additions.Any())
                    {
                        update.AppendLine("INSERT DATA {");
                        if (!graph.Equals(String.Empty)) update.AppendLine(graph);

                        foreach (Triple t in additions)
                        {
                            update.AppendLine(this._formatter.Format(t));
                        }

                        if (!graph.Equals(String.Empty)) update.AppendLine("}");
                        update.AppendLine("}");
                    }
                }

                if (removals != null)
                {
                    if (removals.Any())
                    {
                        if (update.Length > 0) update.AppendLine(";");

                        update.AppendLine("DELETE DATA {");
                        if (!graph.Equals(String.Empty)) update.AppendLine(graph);

                        foreach (Triple t in removals)
                        {
                            update.AppendLine(this._formatter.Format(t));
                        }

                        if (!graph.Equals(String.Empty)) update.AppendLine("}");
                        update.AppendLine("}");
                    }
                }

                if (update.Length > 0)
                {
                    //Make the SPARQL Update Request
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this._updateUri);
                    request.Method = "POST";
                    request.ContentType = "application/sparql-update";

#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugRequest(request);
                    }
#endif

                    StreamWriter writer = new StreamWriter(request.GetRequestStream());
                    writer.Write(update.ToString());
                    writer.Close();

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
#if DEBUG
                        if (Options.HttpDebugging)
                        {
                            Tools.HttpDebugResponse(response);
                        }
#endif

                        //If we get here without erroring then the request was OK
                        response.Close();
                    }
                }
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                throw new RdfStorageException("A HTTP error occurred while communicating with the Fuseki Server", webEx);
            }
        }

        /// <summary>
        /// Updates a Graph in the Fuseki store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public override void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            this.UpdateGraph(graphUri.ToSafeString(), additions, removals);
        }

        /// <summary>
        /// Executes a SPARQL Query on the Fuseki store
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public Object Query(String sparqlQuery)
        {
            Graph g = new Graph();
            SparqlResultSet results = new SparqlResultSet();
            this.Query(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery);

            if (results.ResultsType != SparqlResultsType.Unknown)
            {
                return results;
            }
            else
            {
                return g;
            }
        }

        /// <summary>
        /// Executes a SPARQL Query on the Fuseki store processing the results using an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery)
        {
            try
            {
                HttpWebRequest request;

                //Create the Request
                String queryUri = this._queryUri;
                if (sparqlQuery.Length < 2048)
                {
                    queryUri += "?query=" + Uri.EscapeDataString(sparqlQuery);
                    request = (HttpWebRequest)WebRequest.Create(queryUri);
                    request.Method = "GET";
                    request.Accept = MimeTypesHelper.HttpRdfOrSparqlAcceptHeader;
                }
                else
                {
                    request = (HttpWebRequest)WebRequest.Create(queryUri);
                    request.Method = "POST";
                    request.Accept = MimeTypesHelper.HttpRdfOrSparqlAcceptHeader;

                    //Build the Post Data and add to the Request Body
                    request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                    StringBuilder postData = new StringBuilder();
                    postData.Append("query=");
                    postData.Append(Uri.EscapeDataString(sparqlQuery));
                    StreamWriter writer = new StreamWriter(request.GetRequestStream());
                    writer.Write(postData);
                    writer.Close();
                }

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                //Get the Response and process based on the Content Type
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif

                    StreamReader data = new StreamReader(response.GetResponseStream());
                    String ctype = response.ContentType;
                    try
                    {
                        //Is the Content Type referring to a Sparql Result Set format?
                        ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(ctype, true);
                        resreader.Load(resultsHandler, data);
                        response.Close();
                    }
                    catch (RdfParserSelectionException)
                    {
                        //If we get a Parse exception then the Content Type isn't valid for a Sparql Result Set

                        //Is the Content Type referring to a RDF format?
                        IRdfReader rdfreader = MimeTypesHelper.GetParser(ctype);
                        rdfreader.Load(rdfHandler, data);
                        response.Close();
                    }
                }
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                throw new RdfQueryException("A HTTP error occurred while querying the Store", webEx);
            }
        }

        /// <summary>
        /// Executes SPARQL Updates against the Fuseki store
        /// </summary>
        /// <param name="sparqlUpdate"></param>
        public void Update(String sparqlUpdate)
        {
            try
            {
                //Make the SPARQL Update Request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this._updateUri);
                request.Method = "POST";
                request.ContentType = "application/sparql-update";

                StreamWriter writer = new StreamWriter(request.GetRequestStream());
                writer.Write(sparqlUpdate);
                writer.Close();

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
                    //If we get here without erroring then the request was OK
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                throw new RdfStorageException("A HTTP error occurred while communicating with the Fuseki Server", webEx);
            }
        }

        /// <summary>
        /// Gets a String which gives details of the Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[Fuseki] " + this._serviceUri;
        }

        #region IConfigurationSerializable Members

        /// <summary>
        /// Serializes the connection's configuration
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public override void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"));
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode genericManager = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassGenericManager);
            INode server = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyServer);

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName)));
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(this._serviceUri)));
        }

        #endregion
    }
}

#endif