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
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Class for connecting to any Store that supports the Sesame 2.0 HTTP Communication protocol
    /// </summary>
    /// <remarks>
    /// <para>
    /// See <a href="http://www.openrdf.org/doc/sesame2/system/ch08.html">here</a> for the protocol specification
    /// </para>
    /// </remarks>
    public class SesameHttpProtocolConnector : IQueryableGenericIOManager, IConfigurationSerializable
    {
        /// <summary>
        /// Base Uri for the Store
        /// </summary>
        protected String _baseUri;
        /// <summary>
        /// Store ID
        /// </summary>
        protected String _store;
        /// <summary>
        /// Username for accessing the Store
        /// </summary>
        protected String _username;
        /// <summary>
        /// Password for accessing the Store
        /// </summary>
        protected String _pwd;
        /// <summary>
        /// Whether the User has provided credentials for accessing the Store using authentication
        /// </summary>
        protected bool _hasCredentials = false;

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="storeID">Store ID</param>
        public SesameHttpProtocolConnector(String baseUri, String storeID)
        {
            this._baseUri = baseUri;
            if (!this._baseUri.EndsWith("/")) this._baseUri += "/";
            this._store = storeID;
        }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="username">Username to use for requests that require authentication</param>
        /// <param name="password">Password to use for requests that require authentication</param>
        public SesameHttpProtocolConnector(String baseUri, String storeID, String username, String password)
            : this(baseUri, storeID)
        {
            this._username = username;
            this._pwd = password;
            this._hasCredentials = true;
        }

        /// <summary>
        /// Gets the Base URI to the repository
        /// </summary>
        public String BaseUri
        {
            get
            {
                return this._baseUri;
            }
        }

        /// <summary>
        /// Makes a Sparql Query against the underlying Store
        /// </summary>
        /// <param name="sparqlQuery">Sparql Query</param>
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

                    request = this.CreateRequest("repositories/" + this._store, MimeTypesHelper.HttpRdfOrSparqlAcceptHeader, "GET", queryParams);
                }
                else
                {
                    request = this.CreateRequest("repositories/" + this._store, MimeTypesHelper.HttpRdfOrSparqlAcceptHeader, "POST", queryParams);

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
                    //If we get a Parser Selection exception then the Content Type isn't valid for a Sparql Result Set

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
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Uri of the Graph to load</param>
        /// <remarks>If a Null Uri is specified then the entire contents of the Store will be loaded</remarks>
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
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Uri of the Graph to load</param>
        /// <remarks>If an empty Uri is specified then the entire contents of the Store will be loaded</remarks>
        public void LoadGraph(IGraph g, string graphUri)
        {
            try
            {
                HttpWebRequest request;
                Dictionary<String, String> serviceParams = new Dictionary<string, string>();

                String requestUri = "repositories/" + this._store + "/statements";
                if (!graphUri.Equals(String.Empty))
                {
                    serviceParams.Add("context", "<" + graphUri + ">");
                    if (g.IsEmpty) g.BaseUri = new Uri(graphUri);
                }

                request = this.CreateRequest(requestUri, MimeTypesHelper.HttpAcceptHeader, "GET", serviceParams);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                parser.Load(g, new StreamReader(response.GetResponseStream()));
                response.Close();
            }
            catch (WebException webEx)
            {
                throw new RdfStorageException("A HTTP Error occurred while trying to load a Graph from the Store", webEx);
            }
        }

        /// <summary>
        /// Saves a Graph into the Store (Warning: Completely replaces any existing Graph with the same URI unless there is no URI - see remarks for details)
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <remarks>
        /// If the Graph has no URI then the contents will be appended to the Store, if the Graph has a URI then existing data associated with that URI will be replaced
        /// </remarks>
        public void SaveGraph(IGraph g)
        {
            try
            {
                HttpWebRequest request;
                Dictionary<String, String> serviceParams = new Dictionary<string, string>();

                if (g.BaseUri != null)
                {
                    serviceParams.Add("context", "<" + g.BaseUri.ToString() + ">");
                    request = this.CreateRequest("repositories/" + this._store + "/statements", "*/*", "PUT", serviceParams);
                }
                else
                {
                    request = this.CreateRequest("repositories/" + this._store + "/statements", "*/*", "POST", serviceParams);
                }

                request.ContentType = MimeTypesHelper.NTriples[0];
                NTriplesWriter ntwriter = new NTriplesWriter();
                ntwriter.Save(g, new StreamWriter(request.GetRequestStream()));

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //If we get then it was OK
                response.Close();
            }
            catch (WebException webEx)
            {
                throw new RdfStorageException("A HTTP Error occurred while trying to save a Graph to the Store", webEx);
            }
        }

        /// <summary>
        /// Updates a Graph
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (graphUri != null)
            {
                this.UpdateGraph(graphUri.ToString(), additions, removals);
            }
            else
            {
                this.UpdateGraph(String.Empty, additions, removals);
            }
        }

        /// <summary>
        /// Updates a Graph
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            try
            {
                HttpWebRequest request;
                HttpWebResponse response;
                Dictionary<String, String> serviceParams = new Dictionary<string, string>();
                NTriplesWriter ntwriter = new NTriplesWriter();

                if (!graphUri.Equals(String.Empty))
                {
                    serviceParams.Add("context", "<" + graphUri + ">");
                }
                else
                {
                    serviceParams.Add("context", "null");
                }

                if (removals != null)
                {
                    if (removals.Any())
                    {
                        request = this.CreateRequest("repositories/" + this._store + "/statements", "*/*", "DELETE", serviceParams);

                        //Delete the Triples to be removed
                        Graph g = new Graph();
                        g.Assert(removals);
                        request.ContentType = MimeTypesHelper.NTriples[0];
                        ntwriter.Save(g, new StreamWriter(request.GetRequestStream()));

                        response = (HttpWebResponse)request.GetResponse();

                        //If we get here then the Delete worked OK
                        response.Close();
                    }
                }

                if (additions != null)
                {
                    if (additions.Any())
                    {
                        //Add the new Triples
                        request = this.CreateRequest("repositories/" + this._store + "/statements", "*/*", "POST", serviceParams);
                        Graph h = new Graph();
                        h.Assert(additions);
                        request.ContentType = MimeTypesHelper.NTriples[0];
                        ntwriter.Save(h, new StreamWriter(request.GetRequestStream()));

                        response = (HttpWebResponse)request.GetResponse();

                        //If we get then it was OK
                        response.Close();
                    }
                }
            }
            catch (WebException webEx)
            {
                throw new RdfStorageException("A HTTP Error occurred while trying to update a Graph in the Store", webEx);
            }
        }

        /// <summary>
        /// Returns that Updates are supported on Sesame HTTP Protocol supporting Stores
        /// </summary>
        public bool UpdateSupported
        {
            get 
            {
                return true;
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
        /// Returns that the Connection is not read-only
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
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

            //Add Credentials if needed
            if (this._hasCredentials)
            {
                NetworkCredential credentials = new NetworkCredential(this._username, this._pwd);
                request.Credentials = credentials;                
            }

            return request;
        }

        /// <summary>
        /// Disposes of the Connector
        /// </summary>
        public void Dispose()
        {
            //No Dispose actions
        }

        /// <summary>
        /// Gets a String which gives details of the Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[Sesame] Store '" + this._store + "' on Server '" + this._baseUri + "'";
        }

        /// <summary>
        /// Serializes the connection's configuration
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(new Uri(NamespaceMapper.RDFS + "label"));
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode genericManager = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassGenericManager);
            INode server = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyServer);
            INode store = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyStore);

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName)));
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(this._baseUri)));
            context.Graph.Assert(new Triple(manager, store, context.Graph.CreateLiteralNode(this._store)));

            if (this._username != null && this._pwd != null)
            {
                INode username = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyUser);
                INode pwd = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyPassword);
                context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(this._username)));
                context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(this._pwd)));
            }
        }
    }
}

#endif