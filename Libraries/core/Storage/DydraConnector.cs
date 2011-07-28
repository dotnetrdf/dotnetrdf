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

#if !NO_DATA && !NO_STORAGE

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

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Class for connecting to repositories hosted on Dydra
    /// </summary>
    public class DydraConnector : SesameHttpProtocolConnector
    {
        private const String DydraBaseUri = "http://dydra.com/";
        private const String DydraApiKeyPassword = "X";
        private String _account, _apiKey;
        private SparqlQueryParser _parser = new SparqlQueryParser();

        public DydraConnector(String accountID, String repositoryID)
            : base(DydraBaseUri + accountID + "/", repositoryID)
        {
            this._account = accountID;
            this._repositoriesPrefix = String.Empty;
            this._queryPath = "/sparql";
            //this._fullContextEncoding = false;
            //this._postAllQueries = true;
        }

        public DydraConnector(String accountID, String repositoryID, String apiKey)
            : this(accountID, repositoryID)
        {
            this._apiKey = apiKey;
            this._username = this._apiKey;
            this._pwd = DydraApiKeyPassword;
            this._hasCredentials = true;
        }

        //public DydraConnector(String accountID, String repositoryID, String username, String password)
        //    : this(accountID, repositoryID)
        //{
        //    this._username = username;
        //    this._pwd = password;
        //    this._hasCredentials = true;
        //}

        public override IEnumerable<Uri> ListGraphs()
        {
            try
            {
                //Use the /contexts method to get the Graph URIs
                //HACK: Have to use SPARQL JSON as currently Dydra's SPARQL XML Results are malformed
                HttpWebRequest request = this.CreateRequest(this._store + "/contexts", MimeTypesHelper.CustomHttpAcceptHeader(MimeTypesHelper.SparqlJson), "GET", new Dictionary<string, string>());
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

        public override void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
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
                if (sparqlQuery.Length < 2048 && !this._postAllQueries)
                {
                    queryParams.Add("query", EscapeQuery(sparqlQuery));

                    request = this.CreateRequest(this._repositoriesPrefix + this._store + this._queryPath, accept, "GET", queryParams);
                }
                else
                {
                    request = this.CreateRequest(this._repositoriesPrefix + this._store + this._queryPath, accept, "POST", queryParams);

                    //Build the Post Data and add to the Request Body
                    request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                    StringBuilder postData = new StringBuilder();
                    postData.Append("query=");
                    postData.Append(Uri.EscapeDataString(EscapeQuery(sparqlQuery)));
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
        /// Makes a SPARQL Query against the underlying Store
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public override object Query(string sparqlQuery)
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
        /// Helper method for creating HTTP Requests to the Store
        /// </summary>
        /// <param name="servicePath">Path to the Service requested</param>
        /// <param name="accept">Acceptable Content Types</param>
        /// <param name="method">HTTP Method</param>
        /// <param name="queryParams">Querystring Parameters</param>
        /// <returns></returns>
        protected override HttpWebRequest CreateRequest(String servicePath, String accept, String method, Dictionary<String, String> queryParams)
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
            if (queryParams.Count > 0)
            {
                if (requestUri.Contains("?"))
                {
                    if (!requestUri.EndsWith("&")) requestUri += "&";
                }
                else
                {
                    requestUri += "?";
                }
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

        protected override string EscapeQuery(string query)
        {
            return query;
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
        public override void SerializeConfiguration(ConfigurationSerializationContext context)
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
            context.Graph.Assert(new Triple(manager, store, context.Graph.CreateLiteralNode(this._store)));

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

        public override string ToString()
        {
            return "[Dydra] Repository '" + this._store + "' on Account '" + this._account + "'";
        }
    }
}

#endif