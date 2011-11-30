/*

Copyright Robert Vesse 2009-11
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

#if !NO_DATA && !NO_STORAGE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
#if !NO_WEB
using System.Web;
#endif
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Class for connecting to repositories hosted on Dydra
    /// </summary>
    /// <remarks>
    /// <strong>Warning: </strong> This support is experimental and unstable, Dydra has exhibited many API consistencies, transient HTTP errors and other problems in our testing and we do not recommend that you use our support for it in production.
    /// </remarks>
    public class DydraConnector
        : IUpdateableGenericIOManager
    {
        private const String DydraBaseUri = "http://dydra.com/";
        private const String DydraApiKeyPassword = "X";
        private String _account, _repo, _apiKey, _username, _pwd;
        private String _baseUri;
        private bool _hasCredentials = false;
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlFormatter _formatter = new SparqlFormatter();

        /// <summary>
        /// Creates a new connection to Dydra
        /// </summary>
        /// <param name="accountID">Account ID</param>
        /// <param name="repositoryID">Repository ID</param>
        public DydraConnector(String accountID, String repositoryID)
        {
            this._account = accountID;
            this._repo = repositoryID;
            this._baseUri = DydraBaseUri + accountID + "/" + repositoryID;
        }

        /// <summary>
        /// Creates a new connection to Dydra
        /// </summary>
        /// <param name="accountID">Account ID</param>
        /// <param name="repositoryID">Repository ID</param>
        /// <param name="apiKey">API Key</param>
        public DydraConnector(String accountID, String repositoryID, String apiKey)
            : this(accountID, repositoryID)
        {
            this._apiKey = apiKey;
            this._username = this._apiKey;
            this._pwd = DydraApiKeyPassword;
            this._hasCredentials = !String.IsNullOrEmpty(apiKey);
        }

        //public DydraConnector(String accountID, String repositoryID, String username, String password)
        //    : this(accountID, repositoryID)
        //{
        //    this._username = username;
        //    this._pwd = password;
        //    this._hasCredentials = true;
        //}

        /// <summary>
        /// Gets the Account Name under which the repository is located
        /// </summary>
        [Description("The Account Name under which the repository is located.")]
        public String AccountName
        {
            get
            {
                return this._account;
            }
        }

        /// <summary>
        /// Gets the Repository Name
        /// </summary>
        [Description("The Dydra Repository to which this is a connection.")]
        public String RepositoryName
        {
            get
            {
                return this._repo;
            }
        }

        /// <summary>
        /// Gets whether the Store is ready
        /// </summary>
        public bool IsReady
        {
            get
            {
                return true;
            }
        }
        
        /// <summary>
        /// Returns false because Dydra stores are always read/write
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Saves a Graph to the Store
        /// </summary>
        /// <param name="g">Graph to save</param>
        public void SaveGraph(IGraph g)
        {
            HttpWebRequest request;
            Dictionary<String, String> requestParams = new Dictionary<string, string>();
            if (g.BaseUri != null)
            {
                requestParams.Add("context", g.BaseUri.ToString());
                request = this.CreateRequest("/statements", MimeTypesHelper.Any, "PUT", requestParams);
            }
            else
            {
                request = this.CreateRequest("/statements", MimeTypesHelper.Any, "POST", requestParams);
            }

            IRdfWriter rdfWriter = new RdfXmlWriter();
            request.ContentType = MimeTypesHelper.RdfXml[0];
            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                rdfWriter.Save(g, writer);
                writer.Close();
            }

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
                //If we get here then operation completed OK
                response.Close();
            }
        }

        /// <summary>
        /// Gets the IO Behaviour of the Store
        /// </summary>
        public IOBehaviour IOBehaviour
        {
            get
            {
                return IOBehaviour.GraphStore | IOBehaviour.CanUpdateTriples;
            }
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the graph to load</param>
        public void LoadGraph(IGraph g, Uri graphUri)
        {
            this.LoadGraph(new GraphHandler(g), graphUri);
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the graph to load</param>
        public void LoadGraph(IGraph g, String graphUri)
        {
            this.LoadGraph(new GraphHandler(g), graphUri);
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the graph to load</param>
        public void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            this.LoadGraph(handler, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the graph to load</param>
        public void LoadGraph(IRdfHandler handler, String graphUri)
        {
            Dictionary<String, String> requestParams = new Dictionary<string, string>();
            if (graphUri != null && !graphUri.Equals(String.Empty))
            {
                requestParams.Add("context", "<" + graphUri + ">");
            }

            HttpWebRequest request = this.CreateRequest("/statements", MimeTypesHelper.HttpAcceptHeader, "GET", requestParams);
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

                //If we get here try and parse the response
                IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                parser.Load(handler, new StreamReader(response.GetResponseStream()));

                response.Close();
            }
        }

        /// <summary>
        /// Returns true as listing graphs is supported by Dydra
        /// </summary>
        public bool ListGraphsSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Lists the Graphs from the Repository
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Uri> ListGraphs()
        {
            try
            {
                //Use the /contexts method to get the Graph URIs
                //HACK: Have to use SPARQL JSON as currently Dydra's SPARQL XML Results are malformed
                HttpWebRequest request = this.CreateRequest("/contexts", MimeTypesHelper.CustomHttpAcceptHeader(MimeTypesHelper.SparqlJson), "GET", new Dictionary<string, string>());
                SparqlResultSet results = new SparqlResultSet();
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    ISparqlResultsReader parser = MimeTypesHelper.GetSparqlParser(response.ContentType);
                    parser.Load(results, new StreamReader(response.GetResponseStream()));
                    response.Close();
                }

                List<Uri> graphUris = new List<Uri>();
                foreach (SparqlResult r in results)
                {
                    if (r.HasValue("contextID"))
                    {
                        INode value = r["contextID"];
                        if (value.NodeType == NodeType.Uri)
                        {
                            graphUris.Add(((IUriNode)value).Uri);
                        }
                        else if (value.NodeType == NodeType.Blank)
                        {
                            //Dydra allows BNode Graph URIs
                            graphUris.Add(new Uri("dydra:bnode:" + ((IBlankNode)value).InternalID));
                        }
                    }
                }
                return graphUris;
            }
            catch (Exception ex)
            {
                throw new RdfStorageException("An error occurred while attempting to retrieve the Graph List from the Store, see inner exception for details", ex);
            }
        }

        /// <summary>
        /// Returns true as Triple Level updates are supported by Dydra
        /// </summary>
        public bool UpdateSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Updates an existing Graph in the Store by adding and removing triples
        /// </summary>
        /// <param name="graphUri">URI of the graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <remarks>
        /// Removals are processed before any additions, to force a specific order of additions and removals you should make multiple calls to this function specifying each set of additions or removals you wish to perform seperately
        /// </remarks>
        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            this.UpdateGraph(graphUri.ToSafeString(), additions, removals);
        }

        /// <summary>
        /// Updates an existing Graph in the Store by adding and removing triples
        /// </summary>
        /// <param name="graphUri">URI of the graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <remarks>
        /// Removals are processed before any additions, to force a specific order of additions and removals you should make multiple calls to this function specifying each set of additions or removals you wish to perform seperately
        /// </remarks>
        public void UpdateGraph(String graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            StringBuilder sparqlUpdate = new StringBuilder();

            //Build the SPARQL Update Commands
            if (removals != null && removals.Any())
            {
                sparqlUpdate.AppendLine("DELETE DATA");
                sparqlUpdate.AppendLine("{");
                if (graphUri != null && !graphUri.Equals(String.Empty))
                {
                    sparqlUpdate.AppendLine("GRAPH <" + this._formatter.FormatUri(graphUri) + "> {");
                }
                foreach (Triple t in removals)
                {
                    sparqlUpdate.AppendLine(t.ToString(this._formatter));
                }
                if (graphUri != null && !graphUri.Equals(String.Empty))
                {
                    sparqlUpdate.AppendLine("}");
                }
                sparqlUpdate.AppendLine("}");
            }
            if (additions != null && additions.Any())
            {
                sparqlUpdate.AppendLine("INSERT DATA");
                sparqlUpdate.AppendLine("{");
                if (graphUri != null && !graphUri.Equals(String.Empty))
                {
                    sparqlUpdate.AppendLine("GRAPH <" + this._formatter.FormatUri(graphUri) + "> {");
                }
                foreach (Triple t in additions)
                {
                    sparqlUpdate.AppendLine(t.ToString(this._formatter));
                }
                if (graphUri != null && !graphUri.Equals(String.Empty))
                {
                    sparqlUpdate.AppendLine("}");
                }
                sparqlUpdate.AppendLine("}");
            }

            //Send them to Dydra for processing
            if (sparqlUpdate.Length > 0)
            {
                this.Update(sparqlUpdate.ToString());
            }
        }

        /// <summary>
        /// Gets that deleting Graphs is not supported
        /// </summary>
        public bool DeleteSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public void DeleteGraph(string graphUri)
        {
            if (graphUri != null && !graphUri.Equals(String.Empty))
            {
                this.Update("DROP GRAPH <" + this._formatter.FormatUri(graphUri) + ">");
            }
            else
            {
                this.Update("DROP DEFAULT");
            }
        }

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public void DeleteGraph(Uri graphUri)
        {
            this.DeleteGraph(graphUri.ToSafeString());
        }

        /// <summary>
        /// Performs a SPARQL Query against the underlying Store
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">SPARQL Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
        {
            try
            {
                //First off parse the Query to see what kind of query it is
                SparqlQuery q;
                try
                {
                    q = this._parser.ParseFromString(sparqlQuery);
                }
                catch (RdfParseException parseEx)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new RdfStorageException("An unexpected error occurred while trying to parse the SPARQL Query prior to sending it to the Store, see inner exception for details", ex);
                }

                //Now select the Accept Header based on the query type
                String accept = (SparqlSpecsHelper.IsSelectQuery(q.QueryType) || q.QueryType == SparqlQueryType.Ask) ? MimeTypesHelper.HttpSparqlAcceptHeader : MimeTypesHelper.HttpAcceptHeader;

                //Create the Request
                HttpWebRequest request;
                Dictionary<String, String> queryParams = new Dictionary<string, string>();
                if (sparqlQuery.Length < 2048)
                {
                    queryParams.Add("query", sparqlQuery);

                    request = this.CreateRequest("/sparql", accept, "GET", queryParams);
                }
                else
                {
                    request = this.CreateRequest("/sparql", accept, "POST", queryParams);

                    //Build the Post Data and add to the Request Body
                    request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                    StringBuilder postData = new StringBuilder();
                    postData.Append("query=");
                    postData.Append(HttpUtility.UrlEncode(sparqlQuery));
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
                    if (SparqlSpecsHelper.IsSelectQuery(q.QueryType) || q.QueryType == SparqlQueryType.Ask)
                    {
                        //ASK/SELECT should return SPARQL Results
                        ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(ctype, q.QueryType == SparqlQueryType.Ask);
                        resreader.Load(resultsHandler, data);
                        response.Close();
                    }
                    else
                    {
                        //CONSTRUCT/DESCRIBE should return a Graph
                        IRdfReader rdfreader = MimeTypesHelper.GetParser(ctype);
                        rdfreader.Load(rdfHandler, data);
                        response.Close();
                    }
                }
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                    }
#endif
                    if (webEx.Response.ContentLength > 0)
                    {
                        try
                        {
                            String responseText = new StreamReader(webEx.Response.GetResponseStream()).ReadToEnd();
                            throw new RdfQueryException("A HTTP error occured while querying the Store.  Store returned the following error message: " + responseText, webEx);
                        }
                        catch
                        {
                            throw new RdfQueryException("A HTTP error occurred while querying the Store", webEx);
                        }
                    }
                    else
                    {
                        throw new RdfQueryException("A HTTP error occurred while querying the Store", webEx);
                    }
                }
                else
                {
                    throw new RdfQueryException("A HTTP error occurred while querying the Store", webEx);
                }
            }
        }

        /// <summary>
        /// Performs a SPARQL Query against the underlying Store
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public object Query(string sparqlQuery)
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
        /// Performs a SPARQL Update request on the Store
        /// </summary>
        /// <param name="sparqlUpdates">SPARQL Updates</param>
        public void Update(String sparqlUpdates)
        {
            HttpWebRequest request = this.CreateRequest("/sparql", MimeTypesHelper.HttpSparqlAcceptHeader, "POST", null);

            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write("query=");
                writer.Write(HttpUtility.UrlEncode(sparqlUpdates));
                writer.Close();
            }
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
                //If we get here then it completed OK
                response.Close();
            }
        }

        /// <summary>
        /// Helper method for creating HTTP Requests to the Store
        /// </summary>
        /// <param name="servicePath">Path to the Service requested</param>
        /// <param name="accept">Acceptable Content Types</param>
        /// <param name="method">HTTP Method</param>
        /// <param name="requestParams">Querystring Parameters</param>
        /// <returns></returns>
        protected HttpWebRequest CreateRequest(String servicePath, String accept, String method, Dictionary<String, String> requestParams)
        {
            //Modify the Accept header appropriately to remove any mention of HTML
            //HACK: Have to do this otherwise Dydra won't HTTP authenticate nicely
            if (accept.Contains("application/xhtml+xml"))
            {
                accept = accept.Replace("application/xhtml+xml,", String.Empty);
                if (accept.Contains(",,")) accept = accept.Replace(",,", ",");
            }
            if (accept.Contains("text/html"))
            {
                accept = accept.Replace("text/html", String.Empty);
                if (accept.Contains(",,")) accept = accept.Replace(",,", ",");
            }
            if (accept.Contains(",;")) accept = accept.Replace(",;", ",");

            //HACK: If the Accept header is */* switch it for application/rdf+xml to make Dydra HTTP authenticate nicely
            if (accept.Equals(MimeTypesHelper.Any)) accept = MimeTypesHelper.RdfXml[0];

            //HACK: If the Accept header contains */* strip that part of the header
            if (accept.Contains("*/*")) accept = accept.Substring(0, accept.IndexOf("*/*"));

            if (accept.EndsWith(",")) accept = accept.Substring(0, accept.Length - 1);

            //Build the Request Uri
            //String requestUri = this._baseUri + servicePath;
            String requestUri = this.GetCredentialedUri() + servicePath;
            //if (this._apiKey != null)
            //{
            //    requestUri += "?auth_token=" + Uri.EscapeDataString(this._apiKey);
            //}
            if (requestParams != null)
            {
                if (requestParams.Count > 0)
                {
                    if (requestUri.Contains("?"))
                    {
                        if (!requestUri.EndsWith("&")) requestUri += "&";
                    }
                    else
                    {
                        requestUri += "?";
                    }
                    foreach (String p in requestParams.Keys)
                    {
                        requestUri += p + "=" + Uri.EscapeDataString(requestParams[p]) + "&";
                    }
                    requestUri = requestUri.Substring(0, requestUri.Length - 1);
                }
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

        private String GetCredentialedUri()
        {
            if (this._hasCredentials)
            {
                if (this._apiKey != null)
                {
                    return this._baseUri.Substring(0, 7) + Uri.EscapeUriString(this._apiKey) + "@" + this._baseUri.Substring(7);
                }
                else
                {
                    return this._baseUri.Substring(0, 7) + Uri.EscapeUriString(this._username) + ":" + Uri.EscapeUriString(this._pwd) + "@" + this._baseUri.Substring(7);
                }
            }
            else
            {
                return this._baseUri;
            }
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
            INode catalog = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyCatalog);
            INode store = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyStore);

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName)));
            context.Graph.Assert(new Triple(manager, catalog, context.Graph.CreateLiteralNode(this._account)));
            context.Graph.Assert(new Triple(manager, store, context.Graph.CreateLiteralNode(this._repo)));

            if (this._apiKey != null || (this._username != null && this._pwd != null))
            {
                INode username = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyUser);
                INode pwd = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyPassword);
                if (this._apiKey != null)
                {
                    context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(this._apiKey)));
                }
                else
                {
                    context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(this._username)));
                    context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(this._pwd)));
                }
            }
        }

        /// <summary>
        /// Disposes of the connection
        /// </summary>
        public void Dispose()
        {
            //No Dispose actions needed
        }

        /// <summary>
        /// Gets a String representation of the Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[Dydra] Repository '" + this._repo + "' on Account '" + this._account + "'";
        }
    }
}

#endif
