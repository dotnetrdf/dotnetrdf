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
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Writing;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Class for connecting to any dataset that can be exposed by Joseki (i.e. any existing Jena model)
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Experimental and partially untested</strong>
    /// </para>
    /// <para>
    /// Since Joseki can be used to expose practically any type of Jena based model via Sparql and SPARUL endpoints some/all operations may fail depending on the underlying storage model of the Joseki instance.  For example not all models support named graphs and not all Joseki instances provide full read/write capabilities.
    /// </para>
    /// <para>
    /// The Joseki connector permits use in a read-only mode in the event when you only specify a Query Service path to the constructor (or enter null for the Update Service path).  When instantiated in read-only mode any attempt to use the <see cref="JosekiConnector.SaveGraph">SaveGraph</see> or <see cref="JosekiConnector.UpdateGraph">UpdateGraph</see> methods will result in errors and the <see cref="JosekiConnector.UpdateSupported">UpdateSupported</see> property will return false.
    /// </para>
    /// </remarks>
    public class JosekiConnector : IUpdateableGenericIOManager, IConfigurationSerializable
    {
        private String _baseUri, _queryService, _updateService;

        /// <summary>
        /// Creates a new connection to a Joseki server
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="queryServicePath">Path to the Query Service</param>
        /// <param name="updateServicePath">Path to the Update Service</param>
        /// <remarks>
        /// For example the Base Uri might be <strong>http://example.org:8080/</strong> with a Query Service path of <strong>sparql</strong> and an Update Service path of <strong>update</strong>
        /// </remarks>
        public JosekiConnector(String baseUri, String queryServicePath, String updateServicePath)
        {
            this._baseUri = baseUri;
            if (!this._baseUri.EndsWith("/")) this._baseUri += "/";
            this._queryService = queryServicePath;
            this._updateService = updateServicePath;
        }

        /// <summary>
        /// Creates a new read only connection to a Joseki server
        /// </summary>
        /// <param name="baseUri">Base Uri of the server</param>
        /// <param name="queryServicePath">Path to the Query Service</param>
        /// <remarks>
        /// For example the Base Uri might be <strong>http://example.org:8080/</strong> with a Query Service path of <strong>sparql</strong>
        /// </remarks>
        public JosekiConnector(String baseUri, String queryServicePath)
            : this(baseUri, queryServicePath, null) { }

        /// <summary>
        /// Loads a Graph from the Joseki store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Uri of the Graph to load</param>
        public void LoadGraph(IGraph g, Uri graphUri)
        {
            if (graphUri != null)
            {
                this.LoadGraph(g, graphUri.ToString());
            }
            else
            {
                this.LoadGraph(g, String.Empty);
            }
        }

        /// <summary>
        /// Loads a Graph from the Joseki store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Uri of the Graph to load</param>
        public void LoadGraph(IGraph g, string graphUri)
        {
            try
            {
                HttpWebRequest request;
                Dictionary<String, String> serviceParams = new Dictionary<string, string>();

                String query = "CONSTRUCT {?s ?p ?o}";
                if (!graphUri.Equals(String.Empty))
                {
                    query += " FROM <" + graphUri.ToString().Replace(">","\\>") + ">";
                    if (g.IsEmpty) g.BaseUri = new Uri(graphUri);
                }
                query += " WHERE {?s ?p ?o}";
                serviceParams.Add("query", query);

                request = this.CreateRequest(this._queryService, MimeTypesHelper.HttpAcceptHeader, "GET", serviceParams);

                #if DEBUG
                    if (Options.HttpDebugging) Tools.HttpDebugRequest(request);
                #endif

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                #if DEBUG
                    if (Options.HttpDebugging) Tools.HttpDebugResponse(response);
                #endif

                IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                parser.Load(g, new StreamReader(response.GetResponseStream()));
                response.Close();
            }
            catch (WebException webEx)
            {
                throw new RdfStorageException("A HTTP Error occurred while communicating with Joseki", webEx);
            }
        }

        /// <summary>
        /// Saves a Graph to the Joseki store (appends to any existing Graph with the same URI)
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <remarks>
        /// <para>
        /// Contents of this Graph will be appended to any existing Graph with the same URI
        /// </para>
        /// </remarks>
        public void SaveGraph(IGraph g)
        {
            if (this._updateService == null) throw new RdfStorageException("Unable to save a Graph to the Joseki store - the connector was created in read-only mode");
            try
            {
                HttpWebRequest request = this.CreateRequest(this._updateService, MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
                
                //Build our Sparql Update Query
                StringBuilder postData = new StringBuilder();
                postData.Append("INSERT DATA");
                if (g.BaseUri != null)
                {
                    postData.Append(" INTO <" + g.BaseUri.ToString().Replace(">", "\\>") + ">");
                }
                postData.AppendLine(" {");
                postData.AppendLine(VDS.RDF.Writing.StringWriter.Write(g, new NTriplesWriter()));
                postData.AppendLine("}");

                StreamWriter postWriter = new StreamWriter(request.GetRequestStream());
                postWriter.Write(postData.ToString());
                postWriter.Close();

                #if DEBUG
                    if (Options.HttpDebugging) Tools.HttpDebugRequest(request);
                #endif

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                #if DEBUG
                    if (Options.HttpDebugging) Tools.HttpDebugResponse(response);
                #endif

                //If we get then it was OK
                response.Close();
            }
            catch (WebException webEx)
            {
                #if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
                #endif

                throw new RdfStorageException("A HTTP error occurred while communicating with the Joseki server", webEx);
            }
        }

        /// <summary>
        /// Updates a Graph in the Joseki store
        /// </summary>
        /// <param name="graphUri">Graph Uri</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (this._updateService == null) throw new RdfStorageException("Unable to update a Graph in the Joseki store - the connector was created in read-only mode");
            try
            {
                //Build up the SPARUL MODIFY command
                StringBuilder modify = new StringBuilder();
                if (graphUri != null)
                {
                    modify.AppendLine("MODIFY <" + graphUri.ToString().Replace(">", "\\>") + ">");
                }
                else
                {
                    modify.AppendLine("MODIFY");
                }

                if (removals != null)
                {
                    if (removals.Any())
                    {
                        Graph deleteGraph = new Graph();
                        deleteGraph.Assert(removals);
                        String deleteData = VDS.RDF.Writing.StringWriter.Write(deleteGraph, new NTriplesWriter());
                        modify.AppendLine("DELETE");
                        modify.AppendLine("{");
                        modify.AppendLine(deleteData);
                        modify.AppendLine("}");
                    }
                }

                if (additions != null)
                {
                    if (additions.Any())
                    {
                        Graph insertGraph = new Graph();
                        insertGraph.Assert(additions);
                        String insertData = VDS.RDF.Writing.StringWriter.Write(insertGraph, new NTriplesWriter());
                        modify.AppendLine("INSERT");
                        modify.AppendLine("{");

                        modify.AppendLine(insertData);
                        modify.AppendLine("}");
                    }
                }

                //If we didn't actually have any deletions or deletions to do we can exit here
                if (!modify.ToString().Contains("DELETE") && !modify.ToString().Contains("INSERT")) return;

                //Try to make the Update request
                HttpWebRequest request = this.CreateRequest(this._updateService, MimeTypesHelper.Any, "POST", new Dictionary<string, string>());

                StreamWriter postWriter = new StreamWriter(request.GetRequestStream());
                postWriter.Write(modify.ToString());
                postWriter.Close();

                #if DEBUG
                    if (Options.HttpDebugging) Tools.HttpDebugRequest(request);
                #endif

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                #if DEBUG
                    if (Options.HttpDebugging) Tools.HttpDebugResponse(response);
                #endif  

                //If we get then it was OK
                response.Close();
            }
            catch (WebException webEx)
            {
                #if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
                #endif

                throw new RdfStorageException("A HTTP error occurred while communicating with the Joseki server", webEx);

            }
        }

        /// <summary>
        /// Updates a Graph in the Joseki store
        /// </summary>
        /// <param name="graphUri">Graph Uri</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (graphUri.Equals(String.Empty))
            {
                this.UpdateGraph((Uri)null, additions, removals);
            }
            else
            {
                this.UpdateGraph(new Uri(graphUri), additions, removals);
            }
        }

        /// <summary>
        /// Returns whether Updates are supported by this Joseki connector
        /// </summary>
        /// <remarks>
        /// If the connector was instantiated in read-only mode this returns false
        /// </remarks>
        public bool UpdateSupported
        {
            get 
            {
                return (this._updateService != null);
            }
        }

        /// <summary>
        /// Returns that the Connection is ready
        /// </summary>
        public bool IsReady
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns whether the Connection was created in read-only mode
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return (this._updateService == null);
            }
        }

        /// <summary>
        /// Makes a SPARQL Query against the Joseki store
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public object Query(string sparqlQuery)
        {
            try
            {
                HttpWebRequest request;

                //Create the Request
                Dictionary<String, String> queryParams = new Dictionary<string, string>();
                if (sparqlQuery.Length < 2048)
                {
                    queryParams.Add("query", sparqlQuery);

                    request = this.CreateRequest(this._queryService, MimeTypesHelper.HttpRdfOrSparqlAcceptHeader, "GET", queryParams);
                }
                else
                {
                    request = this.CreateRequest(this._queryService, MimeTypesHelper.HttpRdfOrSparqlAcceptHeader, "POST", queryParams);

                    //Build the Post Data and add to the Request Body
                    request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                    StringBuilder postData = new StringBuilder();
                    postData.Append("query=");
                    postData.Append(Uri.EscapeDataString(sparqlQuery));
                    StreamWriter writer = new StreamWriter(request.GetRequestStream());
                    writer.Write(postData);
                    writer.Close();
                }

                //Get the Response and process based on the Content Type
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                StreamReader data = new StreamReader(response.GetResponseStream());
                String ctype = response.ContentType;
                try
                {
                    //Is the Content Type referring to a Sparql Result Set format?
                    ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(ctype, true);
                    SparqlResultSet results = new SparqlResultSet();
                    resreader.Load(results, data);
                    response.Close();
                    return results;
                }
                catch (RdfParserSelectionException)
                {
                    //If we get a Parse exception then the Content Type isn't valid for a Sparql Result Set

                    //Is the Content Type referring to a RDF format?
                    IRdfReader rdfreader = MimeTypesHelper.GetParser(ctype);
                    Graph g = new Graph();
                    rdfreader.Load(g, data);
                    response.Close();
                    return g;
                }
            }
            catch (WebException webEx)
            {
                throw new RdfQueryException("A HTTP error occurred while querying the Store", webEx);
            }
        }

        /// <summary>
        /// Makes a SPARQL Update against the Joseki Store
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update</param>
        /// <exception cref="RdfStorageException">Thrown if the connector was created in read-only mode</exception>
        public void Update(String sparqlUpdate)
        {
           if (this._updateService == null) throw new RdfStorageException("Unable to perform SPARQL Updates on the Joseki store - the connector was created in read-only mode");

           try
           {
               HttpWebRequest request;

               //Create the Request
               Dictionary<String, String> updateParams = new Dictionary<string, string>();
               if (sparqlUpdate.Length < 2048)
               {
                   updateParams.Add("update", sparqlUpdate);

                   request = this.CreateRequest(this._updateService, MimeTypesHelper.Any, "GET", updateParams);
               }
               else
               {
                   request = this.CreateRequest(this._updateService, MimeTypesHelper.Any, "POST", updateParams);

                   //Build the Post Data and add to the Request Body
                   request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                   StringBuilder postData = new StringBuilder();
                   postData.Append("update=");
                   postData.Append(Uri.EscapeDataString(sparqlUpdate));
                   StreamWriter writer = new StreamWriter(request.GetRequestStream());
                   writer.Write(postData);
                   writer.Close();
               }

               //Get the Response and process based on the Content Type
               HttpWebResponse response = (HttpWebResponse)request.GetResponse();
           }
           catch (WebException webEx)
           {
               throw new SparqlUpdateException("A HTTP error occurred while attempting to Update the Store", webEx);
           }
        }

        /// <summary>
        /// Helper method for creating HTTP Requests to the Store
        /// </summary>
        /// <param name="servicePath">Path to the Service requested</param>
        /// <param name="accept">Acceptable Content Types</param>
        /// <param name="method">HTTP Method</param>
        /// <param name="queryParams">Querystring Parameters</param>
        /// <returns></returns>
        protected virtual HttpWebRequest CreateRequest(String servicePath, String accept, String method, Dictionary<String, String> queryParams)
        {
            //Build the Request Uri
            String requestUri = this._baseUri + servicePath;
            if (queryParams.Count > 0)
            {
                requestUri += "?";
                foreach (String p in queryParams.Keys)
                {
                    requestUri += p + "=" + Uri.EscapeDataString(queryParams[p]) + "&";
                }
                requestUri = requestUri.Substring(0, requestUri.Length - 1);
            }

            //Create our Request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Accept = accept;
            request.Method = method;

            ////Add Credentials if needed
            //if (this._hasCredentials)
            //{
            //    NetworkCredential credentials = new NetworkCredential(this._username, this._pwd);
            //    request.Credentials = credentials;
            //}

            return request;
        }

        /// <summary>
        /// Disposes of a connection to the Joseki store
        /// </summary>
        public void Dispose()
        {
            //No Disposes actions
        }

        /// <summary>
        /// Gets a String which gives details of the Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[Joseki] " + this._baseUri;
        }

        /// <summary>
        /// Serializes the connection's configuration
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"));
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode genericManager = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassGenericManager);
            INode server = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyServer);
            INode queryPath = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyQueryPath);
            INode updatePath = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyUpdatePath);

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName)));
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(this._baseUri)));
            context.Graph.Assert(new Triple(manager, queryPath, context.Graph.CreateLiteralNode(this._queryService)));
            if (this._updateService != null)
            {
                context.Graph.Assert(new Triple(manager, updatePath, context.Graph.CreateLiteralNode(this._updateService)));
            }
        }
    }
}

#endif