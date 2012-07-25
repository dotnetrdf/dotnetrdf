/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage.Management;
using VDS.RDF.Storage.Management.Provisioning;
using VDS.RDF.Storage.Management.Provisioning.Sesame;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Abstract Base Class for connecting to any Store that supports the Sesame 2.0 HTTP Communication protocol
    /// </summary>
    /// <remarks>
    /// <para>
    /// See <a href="http://www.openrdf.org/doc/sesame2/system/ch08.html">here</a> for the protocol specification, this base class supports Version 5 of the protocol which does not include SPARQL Update support
    /// </para>
    /// </remarks>
    public abstract class BaseSesameHttpProtocolConnector
        : BaseAsyncHttpConnector, IAsyncQueryableStorage, IAsyncStorageServer, IConfigurationSerializable
#if !NO_SYNC_HTTP
        , IQueryableStorage, IStorageServer
#endif
    {
        /// <summary>
        /// System Repository ID
        /// </summary>
        public const String SystemRepositoryID = "SYSTEM";

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
        /// Repositories Prefix
        /// </summary>
        protected String _repositoriesPrefix = "repositories/";
        /// <summary>
        /// Query Path Prefix
        /// </summary>
        protected String _queryPath = String.Empty;
        /// <summary>
        /// Update Path Prefix
        /// </summary>
        protected String _updatePath = "/statements";
        /// <summary>
        /// Whether to do full encoding of contexts
        /// </summary>
        protected bool _fullContextEncoding = true;
        /// <summary>
        /// Whether queries should always be posted
        /// </summary>
        protected bool _postAllQueries = false;

        private StringBuilder _output = new StringBuilder();
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private NTriplesFormatter _formatter = new NTriplesFormatter();

        /// <summary>
        /// Available Sesame template types
        /// </summary>
        protected List<Type> _templateTypes = new List<Type>()
        {
            typeof(SesameMemTemplate),
            typeof(SesameNativeTemplate),
            typeof(SesameHttpTemplate)
        };

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="storeID">Store ID</param>
        public BaseSesameHttpProtocolConnector(String baseUri, String storeID)
            : this(baseUri, storeID, null, null) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="username">Username to use for requests that require authentication</param>
        /// <param name="password">Password to use for requests that require authentication</param>
        public BaseSesameHttpProtocolConnector(String baseUri, String storeID, String username, String password)
            : base()
        {
            this._baseUri = baseUri;
            if (!this._baseUri.EndsWith("/")) this._baseUri += "/";
            this._store = storeID;
            this._username = username;
            this._pwd = password;
            this._hasCredentials = (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password));
        }

#if !NO_PROXY

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="proxy">Proxy Server</param>
        public BaseSesameHttpProtocolConnector(String baseUri, String storeID, WebProxy proxy)
            : this(baseUri, storeID, null, null, proxy) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="username">Username to use for requests that require authentication</param>
        /// <param name="password">Password to use for requests that require authentication</param>
        /// <param name="proxy">Proxy Server</param>
        public BaseSesameHttpProtocolConnector(String baseUri, String storeID, String username, String password, WebProxy proxy)
            : this(baseUri, storeID, username, password)
        {
            this.Proxy = proxy;
        }

#endif

        /// <summary>
        /// Gets the Base URI to the repository
        /// </summary>
        [Description("The Base URI for requests made to the store.")]
        public String BaseUri
        {
            get
            {
                return this._baseUri;
            }
        }

        /// <summary>
        /// Gets the Repository Name that is in use
        /// </summary>
        [Description("The Repository to which this is a connection.")]
        public String RepositoryName
        {
            get
            {
                return this._store;
            }
        }

        /// <summary>
        /// Gets the Save Behaviour of Stores that use the Sesame HTTP Protocol
        /// </summary>
        public override IOBehaviour IOBehaviour
        {
            get
            {
                return IOBehaviour.GraphStore | IOBehaviour.CanUpdateTriples | IOBehaviour.HasMultipleStores | IOBehaviour.CanCreateStores;
            }
        }

        /// <summary>
        /// Returns that Updates are supported on Sesame HTTP Protocol supporting Stores
        /// </summary>
        public override bool UpdateSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that deleting graphs from the Sesame store is supported
        /// </summary>
        public override bool DeleteSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that listing Graphs is supported
        /// </summary>
        public override bool ListGraphsSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that the Connection is ready
        /// </summary>
        public override bool IsReady
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that the Connection is not read-only
        /// </summary>
        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

#if !NO_SYNC_HTTP

        /// <summary>
        /// Makes a SPARQL Query against the underlying Store
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public virtual object Query(String sparqlQuery)
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
        /// Makes a SPARQL Query against the underlying Store processing the results with an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public virtual void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery)
        {
            try
            {
                //Pre-parse the query to determine what the Query Type is
                bool isAsk = false;
                SparqlQuery q = null;
                try
                {
                    q = this._parser.ParseFromString(sparqlQuery);
                    isAsk = q.QueryType == SparqlQueryType.Ask;
                }
                catch
                {
                    //If parsing error fallback to naive detection
                    isAsk = Regex.IsMatch(sparqlQuery, "ASK", RegexOptions.IgnoreCase);
                }

                //Select Accept Header
                String accept;
                if (q != null)
                {
                    accept = (SparqlSpecsHelper.IsSelectQuery(q.QueryType) || q.QueryType == SparqlQueryType.Ask ? MimeTypesHelper.HttpSparqlAcceptHeader : MimeTypesHelper.HttpAcceptHeader);
                }
                else
                {
                    accept = MimeTypesHelper.HttpRdfOrSparqlAcceptHeader;
                }

                HttpWebRequest request;

                //Create the Request
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
                    try
                    {
                        //Is the Content Type referring to a Sparql Result Set format?
                        ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(ctype, isAsk);
                        resreader.Load(resultsHandler, data);
                        response.Close();
                    }
                    catch (RdfParserSelectionException)
                    {
                        //If we get a Parser Selection exception then the Content Type isn't valid for a SPARQL Result Set
                        //HACK: As this is Sesame this may be it being buggy and sending application/xml instead of application/sparql-results+xml
                        if (ctype.StartsWith("application/xml"))
                        {
                            try
                            {
                                ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser("application/sparql-results+xml");
                                resreader.Load(resultsHandler, data);
                                response.Close();

                            }
                            catch (RdfParserSelectionException)
                            {
                                //Ignore this and fall back to trying as an RDF format instead
                            }
                        }

                        //Is the Content Type referring to a RDF format?
                        IRdfReader rdfreader = MimeTypesHelper.GetParser(ctype);
                        if (q != null && (SparqlSpecsHelper.IsSelectQuery(q.QueryType) || q.QueryType == SparqlQueryType.Ask))
                        {
                            SparqlRdfParser resreader = new SparqlRdfParser(rdfreader);
                            resreader.Load(resultsHandler, data);
                        }
                        else
                        {
                            rdfreader.Load(rdfHandler, data);
                        }
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

#endif

        /// <summary>
        /// Escapes a Query to avoid a character encoding issue when communicating a query to Sesame
        /// </summary>
        /// <param name="query">Query</param>
        /// <returns></returns>
        protected virtual String EscapeQuery(String query)
        {
            StringBuilder output = new StringBuilder();
            foreach (char c in query.ToCharArray())
            {
                if (c <= 255)
                {
                    output.Append(c);
                }
                else if (c <= 65535)
                {
                    output.Append("\\u");
                    output.Append(((int)c).ToString("x4"));
                }
                else
                {
                    output.Append("\\U");
                    output.Append(((int)c).ToString("x8"));
                }
            }
            return output.ToString();
        }

#if !NO_SYNC_HTTP

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Uri of the Graph to load</param>
        /// <remarks>If a Null Uri is specified then the default graph (statements with no context in Sesame parlance) will be loaded</remarks>
        public virtual void LoadGraph(IGraph g, Uri graphUri)
        {
            this.LoadGraph(g, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">Uri of the Graph to load</param>
        /// <remarks>If a Null Uri is specified then the default graph (statements with no context in Sesame parlance) will be loaded</remarks>
        public virtual void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            this.LoadGraph(handler, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Uri of the Graph to load</param>
        /// <remarks>If a Null/Empty Uri is specified then the default graph (statements with no context in Sesame parlance) will be loaded</remarks>
        public virtual void LoadGraph(IGraph g, String graphUri)
        {
            if (g.IsEmpty && graphUri != null && !graphUri.Equals(String.Empty))
            {
                g.BaseUri = UriFactory.Create(graphUri);
            }
            this.LoadGraph(new GraphHandler(g), graphUri);
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">Uri of the Graph to load</param>
        /// <remarks>If a Null/Empty Uri is specified then the default graph (statements with no context in Sesame parlance) will be loaded</remarks>
        public virtual void LoadGraph(IRdfHandler handler, String graphUri)
        {
            try
            {
                HttpWebRequest request;
                Dictionary<String, String> serviceParams = new Dictionary<string, string>();

                String requestUri = this._repositoriesPrefix + this._store + "/statements";
                if (!graphUri.Equals(String.Empty))
                {
                    //if (this._fullContextEncoding)
                    //{
                        serviceParams.Add("context", "<" + graphUri + ">");
                    //}
                    //else
                    //{
                    //    serviceParams.Add("context", graphUri);
                    //}
                }

                request = this.CreateRequest(requestUri, MimeTypesHelper.HttpAcceptHeader, "GET", serviceParams);

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
                    IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                    parser.Load(handler, new StreamReader(response.GetResponseStream()));
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
        public virtual void SaveGraph(IGraph g)
        {
            try
            {
                HttpWebRequest request;
                Dictionary<String, String> serviceParams = new Dictionary<string, string>();

                if (g.BaseUri != null)
                {
                    if (this._fullContextEncoding)
                    {
                        serviceParams.Add("context", "<" + g.BaseUri.AbsoluteUri + ">");
                    }
                    else
                    {
                        serviceParams.Add("context", g.BaseUri.AbsoluteUri);
                    }
                    request = this.CreateRequest(this._repositoriesPrefix + this._store + "/statements", "*/*", "PUT", serviceParams);
                }
                else
                {
                    request = this.CreateRequest(this._repositoriesPrefix + this._store + "/statements", "*/*", "POST", serviceParams);
                }

                request.ContentType = MimeTypesHelper.NTriples[0];
                NTriplesWriter ntwriter = new NTriplesWriter();
                ntwriter.Save(g, new StreamWriter(request.GetRequestStream()));

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
                    //If we get then it was OK
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
                throw new RdfStorageException("A HTTP Error occurred while trying to save a Graph to the Store", webEx);
            }
        }

        /// <summary>
        /// Updates a Graph
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public virtual void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            this.UpdateGraph(graphUri.ToSafeString(), additions, removals);
        }

        /// <summary>
        /// Updates a Graph
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public virtual void UpdateGraph(String graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            try
            {
                HttpWebRequest request;
                HttpWebResponse response;
                Dictionary<String, String> serviceParams = new Dictionary<string, string>();
                NTriplesWriter ntwriter = new NTriplesWriter();
                //RdfXmlWriter writer = new RdfXmlWriter();

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
                        serviceParams.Add("subj", null);
                        serviceParams.Add("pred", null);
                        serviceParams.Add("obj", null);

                        //Have to do a DELETE for each individual Triple
                        foreach (Triple t in removals.Distinct())
                        {
                            this._output.Remove(0, this._output.Length);
                            serviceParams["subj"] = this._formatter.Format(t.Subject);
                            serviceParams["pred"] = this._formatter.Format(t.Predicate);
                            serviceParams["obj"] = this._formatter.Format(t.Object);
                            request = this.CreateRequest(this._repositoriesPrefix + this._store + "/statements", "*/*", "DELETE", serviceParams);

#if DEBUG
                            if (Options.HttpDebugging)
                            {
                                Tools.HttpDebugRequest(request);
                            }
#endif

                            using (response = (HttpWebResponse)request.GetResponse())
                            {
#if DEBUG
                                if (Options.HttpDebugging)
                                {
                                    Tools.HttpDebugResponse(response);
                                }
#endif
                                //If we get here then the Delete worked OK
                                response.Close();
                            }
                        }
                        serviceParams.Remove("subj");
                        serviceParams.Remove("pred");
                        serviceParams.Remove("obj");
                    }
                }

                if (additions != null)
                {
                    if (additions.Any())
                    {
                        //Add the new Triples
                        request = this.CreateRequest(this._repositoriesPrefix + this._store + "/statements", "*/*", "POST", serviceParams);
                        Graph h = new Graph();
                        h.Assert(additions);
                        request.ContentType = MimeTypesHelper.NTriples[0];
                        ntwriter.Save(h, new StreamWriter(request.GetRequestStream()));
                        //request.ContentType = MimeTypesHelper.RdfXml[0];
                        //writer.Save(h, new StreamWriter(request.GetRequestStream()));

#if DEBUG
                        if (Options.HttpDebugging)
                        {
                            Tools.HttpDebugRequest(request);
                        }
#endif

                        using (response = (HttpWebResponse)request.GetResponse())
                        {
#if DEBUG
                            if (Options.HttpDebugging)
                            {
                                Tools.HttpDebugResponse(response);
                            }
#endif
                            //If we get then it was OK
                            response.Close();
                        }
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
                throw new RdfStorageException("A HTTP Error occurred while trying to update a Graph in the Store", webEx);
            }
        }

        /// <summary>
        /// Deletes a Graph from the Sesame store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public virtual void DeleteGraph(Uri graphUri)
        {
            this.DeleteGraph(graphUri.ToSafeString());
        }

        /// <summary>
        /// Deletes a Graph from the Sesame store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public virtual void DeleteGraph(String graphUri)
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

                request = this.CreateRequest(this._repositoriesPrefix + this._store + "/statements", "*/*", "DELETE", serviceParams);
#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif
                using (response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
                    //If we get here then the Delete worked OK
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
                throw new RdfStorageException("A HTTP Error occurred while trying to delete a Graph from the Store", webEx);
            }
        }

        /// <summary>
        /// Gets the list of Graphs in the Sesame store
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<Uri> ListGraphs()
        {
            try
            {
                Object results = this.Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }");
                if (results is SparqlResultSet)
                {
                    List<Uri> graphs = new List<Uri>();
                    foreach (SparqlResult r in ((SparqlResultSet)results))
                    {
                        if (r.HasValue("g"))
                        {
                            INode temp = r["g"];
                            if (temp.NodeType == NodeType.Uri)
                            {
                                graphs.Add(((IUriNode)temp).Uri);
                            }
                        }
                    }
                    return graphs;
                }
                else
                {
                    return Enumerable.Empty<Uri>();
                }
            }
            catch (Exception ex)
            {
                throw new RdfStorageException("Underlying Store returned an error while trying to List Graphs", ex);
            }
        }

#endif
        /// <summary>
        /// Saves a Graph to the Store asynchronously
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public override void SaveGraph(IGraph g, AsyncStorageCallback callback, object state)
        {
            try
            {
                HttpWebRequest request;
                Dictionary<String, String> serviceParams = new Dictionary<string, string>();

                if (g.BaseUri != null)
                {
                    if (this._fullContextEncoding)
                    {
                        serviceParams.Add("context", "<" + g.BaseUri.AbsoluteUri + ">");
                    }
                    else
                    {
                        serviceParams.Add("context", g.BaseUri.AbsoluteUri);
                    }
                    request = this.CreateRequest(this._repositoriesPrefix + this._store + "/statements", "*/*", "PUT", serviceParams);
                }
                else
                {
                    request = this.CreateRequest(this._repositoriesPrefix + this._store + "/statements", "*/*", "POST", serviceParams);
                }

                request.ContentType = MimeTypesHelper.NTriples[0];
                NTriplesWriter ntwriter = new NTriplesWriter();

                this.SaveGraphAsync(request, ntwriter, g, callback, state);
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g, new RdfStorageException("A HTTP Error occurred while trying to save a Graph to the Store", webEx)), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g, new RdfStorageException("Unexpected error while trying to save a Graph to the Store asynchronously, see inner exception for details", ex)), state);
                return;
            }
        }

        /// <summary>
        /// Loads a Graph from the Store asynchronously
        /// </summary>
        /// <param name="handler">Handler to load with</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public override void LoadGraph(IRdfHandler handler, string graphUri, AsyncStorageCallback callback, object state)
        {
            try
            {
                HttpWebRequest request;
                Dictionary<String, String> serviceParams = new Dictionary<string, string>();

                String requestUri = this._repositoriesPrefix + this._store + "/statements";
                if (!graphUri.Equals(String.Empty))
                {
                    serviceParams.Add("context", "<" + graphUri + ">");
                }

                request = this.CreateRequest(requestUri, MimeTypesHelper.HttpAcceptHeader, "GET", serviceParams);

                this.LoadGraphAsync(request, handler, callback, state);
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler, new RdfStorageException("A HTTP Error occurred while trying to load a Graph from the Store", webEx)), state);
            }
        }

        /// <summary>
        /// Updates a Graph in the Store asychronously
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public override void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, object state)
        {
            try
            {
                HttpWebRequest request;
                Dictionary<String, String> serviceParams;
                NTriplesWriter ntwriter = new NTriplesWriter();

                if (removals != null)
                {
                    if (removals.Any())
                    {
                        //For Async deletes we need to build up a sequence of requests to send
                        Queue<HttpWebRequest> requests = new Queue<HttpWebRequest>();

                        //Have to do a DELETE for each individual Triple
                        foreach (Triple t in removals.Distinct())
                        {
                            //Prep Service Params
                            serviceParams = new Dictionary<String, String>();
                            if (!graphUri.Equals(String.Empty))
                            {
                                serviceParams.Add("context", "<" + graphUri + ">");
                            }
                            else
                            {
                                serviceParams.Add("context", "null");
                            }

                            this._output.Remove(0, this._output.Length);
                            serviceParams.Add("subj", this._formatter.Format(t.Subject));
                            serviceParams.Add("pred", this._formatter.Format(t.Predicate));
                            serviceParams.Add("obj", this._formatter.Format(t.Object));
                            request = this.CreateRequest(this._repositoriesPrefix + this._store + "/statements", "*/*", "DELETE", serviceParams);
                            requests.Enqueue(request);
                        }

                        //Run all the requests, if any error make an error callback and abort
                        this.MakeRequestSequence(requests, (sender, args, st) =>
                            {
                                if (!args.WasSuccessful)
                                {
                                    //Invoke callbakc and bail out
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(), new RdfStorageException("An error occurred while trying to asyncrhonously delete triples from the Store, see inner exception for details", args.Error)), state);
                                    return;
                                }
                            }, state);

                    }
                }

                if (additions != null)
                {
                    if (additions.Any())
                    {
                        //Prep Service Params
                        serviceParams = new Dictionary<string, string>();
                        if (!graphUri.Equals(String.Empty))
                        {
                            serviceParams.Add("context", "<" + graphUri + ">");
                        }
                        else
                        {
                            serviceParams.Add("context", "null");
                        }

                        //Add the new Triples
                        request = this.CreateRequest(this._repositoriesPrefix + this._store + "/statements", "*/*", "POST", serviceParams);
                        Graph h = new Graph();
                        h.Assert(additions);
                        request.ContentType = MimeTypesHelper.NTriples[0];

                        //Thankfully Sesame lets us do additions in one request so we don't end up with horrible code like for the removals above
                        this.UpdateGraphAsync(request, ntwriter, graphUri.ToSafeUri(), additions, callback, state);
                        return;
                    }
                }

                //If we get here then we may have done some deletes (which suceeded) but didn't do any adds so we still need to invoke the callback
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                throw new RdfStorageException("A HTTP Error occurred while trying to update a Graph in the Store", webEx);
            }
        }

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public override void DeleteGraph(string graphUri, AsyncStorageCallback callback, object state)
        {
            try
            {
                HttpWebRequest request;
                Dictionary<String, String> serviceParams = new Dictionary<string, string>();

                if (!graphUri.Equals(String.Empty))
                {
                    serviceParams.Add("context", "<" + graphUri + ">");
                }
                else
                {
                    serviceParams.Add("context", "null");
                }

                request = this.CreateRequest(this._repositoriesPrefix + this._store + "/statements", "*/*", "DELETE", serviceParams);
                this.DeleteGraphAsync(request, false, graphUri, callback, state);
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), new RdfStorageException("A HTTP Error occurred while trying to delete a Graph from the Store", webEx)), state);
            }
        }

        /// <summary>
        /// Makes a SPARQL Query against the underlying store
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <returns><see cref="SparqlResultSet">SparqlResultSet</see> or a <see cref="Graph">Graph</see> depending on the Sparql Query</returns>
        public void Query(string sparqlQuery, AsyncStorageCallback callback, object state)
        {
            Graph g = new Graph();
            SparqlResultSet results = new SparqlResultSet();
            this.Query(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery, (sender, args, st) =>
                {
                    if (results.ResultsType != SparqlResultsType.Unknown)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQuery, sparqlQuery, results, args.Error), state);
                    }
                    else
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQuery, sparqlQuery, g, args.Error), state);
                    }
                }, state);
        }

        /// <summary>
        /// Makes a SPARQL Query against the underlying store processing the resulting Graph/Result Set with a handler of your choice
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">SPARQL Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="callback">Callbakc</param>
        /// <param name="state">State to pass to the callback</param>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery, AsyncStorageCallback callback, object state)
        {
            try
            {
                //Pre-parse the query to determine what the Query Type is
                bool isAsk = false;
                SparqlQuery q = null;
                try
                {
                    q = this._parser.ParseFromString(sparqlQuery);
                    isAsk = q.QueryType == SparqlQueryType.Ask;
                }
                catch
                {
                    //If parsing error fallback to naive detection
                    isAsk = Regex.IsMatch(sparqlQuery, "ASK", RegexOptions.IgnoreCase);
                }

                //Select Accept Header
                String accept;
                if (q != null)
                {
                    accept = (SparqlSpecsHelper.IsSelectQuery(q.QueryType) || q.QueryType == SparqlQueryType.Ask ? MimeTypesHelper.HttpSparqlAcceptHeader : MimeTypesHelper.HttpAcceptHeader);
                }
                else
                {
                    accept = MimeTypesHelper.HttpRdfOrSparqlAcceptHeader;
                }

                HttpWebRequest request;

                //Create the Request, for simplicity async requests are always POST
                Dictionary<String, String> queryParams = new Dictionary<string, string>();
                request = this.CreateRequest(this._repositoriesPrefix + this._store + this._queryPath, accept, "POST", queryParams);

                //Build the Post Data and add to the Request Body
                request.ContentType = MimeTypesHelper.WWWFormURLEncoded;

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif
                //POST the response which in async requires extra pain
                request.BeginGetRequestStream(r =>
                    {
                        try
                        {
                            Stream stream = request.EndGetRequestStream(r);

                            StringBuilder postData = new StringBuilder();
                            postData.Append("query=");
                            postData.Append(Uri.EscapeDataString(EscapeQuery(sparqlQuery)));
                            StreamWriter writer = new StreamWriter(stream);
                            writer.Write(postData);
                            writer.Close();

                            request.BeginGetResponse(r2 =>
                                {
                                    try
                                    {
                                        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r2);
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
                                            ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(ctype, isAsk);
                                            resreader.Load(resultsHandler, data);
                                            response.Close();
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, sparqlQuery, rdfHandler, resultsHandler), state);
                                        }
                                        catch (RdfParserSelectionException)
                                        {
                                            //If we get a Parser Selection exception then the Content Type isn't valid for a SPARQL Result Set
                                            //HACK: As this is Sesame this may be it being buggy and sending application/xml instead of application/sparql-results+xml
                                            if (ctype.StartsWith("application/xml"))
                                            {
                                                try
                                                {
                                                    ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser("application/sparql-results+xml");
                                                    resreader.Load(resultsHandler, data);
                                                    response.Close();
                                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, sparqlQuery, rdfHandler, resultsHandler), state);

                                                }
                                                catch (RdfParserSelectionException)
                                                {
                                                    //Ignore this and fall back to trying as an RDF format instead
                                                }
                                            }

                                            //Is the Content Type referring to a RDF format?
                                            IRdfReader rdfreader = MimeTypesHelper.GetParser(ctype);
                                            if (q != null && (SparqlSpecsHelper.IsSelectQuery(q.QueryType) || q.QueryType == SparqlQueryType.Ask))
                                            {
                                                SparqlRdfParser resreader = new SparqlRdfParser(rdfreader);
                                                resreader.Load(resultsHandler, data);
                                            }
                                            else
                                            {
                                                rdfreader.Load(rdfHandler, data);
                                            }
                                            response.Close();
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, sparqlQuery, rdfHandler, resultsHandler), state);
                                        }
                                    }
                                    catch (RdfParseException parseEx)
                                    {
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, parseEx), state);
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
                                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, new RdfQueryException("A HTTP error occured while querying the Store.  Store returned the following error message: " + responseText, webEx)), state);
                                                }
                                                catch
                                                {
                                                    //Ignore and drop through to the generic error message
                                                }
                                            }
                                        }
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, new RdfQueryException("A HTTP error occurred while querying the Store, see inner exception for details", webEx)), state);
                                    }
                                    catch (Exception ex)
                                    {
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, new RdfQueryException("Unexpected error while querying the store, see inner exception for details", ex)), state);
                                    }
                                }, state);
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
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, new RdfQueryException("A HTTP error occured while querying the Store.  Store returned the following error message: " + responseText, webEx)), state);
                                    }
                                    catch
                                    {
                                        //Ignore and drop through to the generic error message
                                    }
                                }
                            }
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, new RdfQueryException("A HTTP error occurred while querying the Store, see inner exception for details", webEx)), state);
                        }
                        catch (Exception ex)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, new RdfQueryException("Unexpected error while querying the store, see inner exception for details", ex)), state);
                        }
                    }, state);
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
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, new RdfQueryException("A HTTP error occured while querying the Store.  Store returned the following error message: " + responseText, webEx)), state);
                        }
                        catch
                        {
                            //Ignore and drop through to the generic error message
                        }
                    }
                }
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, new RdfQueryException("A HTTP error occurred while querying the Store, see inner exception for details", webEx)), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, new RdfQueryException("Unexpected error while querying the store, see inner exception for details", ex)), state);
            }
        }

#if !NO_SYNC_HTTP

        public abstract IStoreTemplate GetDefaultTemplate(String id);

        public abstract IEnumerable<IStoreTemplate> GetAvailableTemplates(String id);

        /// <summary>
        /// Creates a new Store with the given ID
        /// </summary>
        /// <param name="template">Template for creating the new store</param>
        /// <returns>Whether creation succeeded</returns>
        public abstract bool CreateStore(IStoreTemplate template);

        /// <summary>
        /// Deletes the Store with the given ID
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <remarks>
        /// Whether attempting to delete the Store that you are accessing is permissible is up to the implementation
        /// </remarks>
        public virtual void DeleteStore(String storeID)
        {
            throw new RdfStorageException("Sesame does not support deleting stores via it's HTTP Protocol");
        }

        /// <summary>
        /// Gets the list of available stores
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<String> ListStores()
        {
            HttpWebRequest request = CreateRequest("repositories", MimeTypesHelper.SparqlResultsXml[0], "GET", new Dictionary<string, string>());
            ListStringsHandler handler = new ListStringsHandler("id");
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                SparqlXmlParser parser = new SparqlXmlParser();
                parser.Load(handler, new StreamReader(response.GetResponseStream()));
                response.Close();
            }
            return handler.Strings;
        }

        /// <summary>
        /// Gets the Store with the given ID
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <returns></returns>
        /// <remarks>
        /// If the implementation is also an instance of <see cref="IStorageProvider">IStorageProvider</see> and the requested Store ID represents the current instance then it is acceptable for an implementation to return itself.  Consumers of this method should be aware of this and if necessary use other means to create a connection to a store if they want a unique instance of the provider.
        /// </remarks>
        public abstract IStorageProvider GetStore(String storeID);

#endif

        public abstract void GetDefaultTemplate(String id, AsyncStorageCallback callback, Object state);

        public abstract void GetAvailableTemplates(String id, AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Creates a store asynchronously
        /// </summary>
        /// <param name="template">Template for creating the new store</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// Behaviour with regards to whether creating a store overwrites an existing store with the same ID is at the discretion of the implementation and <em>SHOULD</em> be documented in an implementations comments
        /// </remarks>
        public abstract void CreateStore(IStoreTemplate template, AsyncStorageCallback callback, Object state);

        /// <summary>
        /// Deletes a store asynchronously
        /// </summary>
        /// <param name="storeID">ID of the store to delete</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void DeleteStore(String storeID, AsyncStorageCallback callback, Object state)
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, new RdfStorageException("Sesame does not support deleting stores via it's HTTP Protocol")), state);
        }

        /// <summary>
        /// Lists the available stores asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void ListStores(AsyncStorageCallback callback, Object state)
        {
            HttpWebRequest request = CreateRequest("repositories", MimeTypesHelper.SparqlResultsXml[0], "GET", new Dictionary<string, string>());
            ListStringsHandler handler = new ListStringsHandler("id");
            try
            {
                request.BeginGetResponse(r =>
                    {
                        try
                        {
                            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);
                            SparqlXmlParser parser = new SparqlXmlParser();
                            parser.Load(handler, new StreamReader(response.GetResponseStream()));
                            response.Close();
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, handler.Strings), state);
                        }
                        catch (WebException webEx)
                        {
#if DEBUG
                            if (Options.HttpDebugging)
                            {
                                if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                            }
#endif
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, new RdfStorageException("A HTTP Error occurred while trying list stores asynchronously", webEx)), state);
                            return;
                        }
                        catch (Exception ex)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, new RdfStorageException("Unexpected error while trying to list stores asynchronously, see inner exception for details", ex)), state);
                            return;
                        }
                    }, state);
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, new RdfStorageException("A HTTP Error occurred while trying list stores asynchronously", webEx)), state);
                return;
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, new RdfStorageException("Unexpected error while trying to list stores asynchronously, see inner exception for details", ex)), state);
                return;
            }
        }

        /// <summary>
        /// Gets a store asynchronously
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// If the implementation also implements <see cref="IAsyncStorageProvider"/> and the store ID requested matches the current instance an instance <em>MAY</em> invoke the callback immediately returning a reference to itself
        /// </remarks>
        public abstract void GetStore(String storeID, AsyncStorageCallback callback, Object state);

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
            if (queryParams != null)
            {
                if (queryParams.Count > 0)
                {
                    requestUri += "?";
                    foreach (String p in queryParams.Keys)
                    {
                        requestUri += p + "=" + Uri.EscapeDataString(queryParams[p]) + "&";
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

            return base.GetProxiedRequest(request);
        }

        /// <summary>
        /// Disposes of the Connector
        /// </summary>
        public override void Dispose()
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
            INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"));
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

            base.SerializeProxyConfig(manager, context);
        }
    }

    /// <summary>
    /// Connector for connecting to a Store that supports the Sesame 2.0 HTTP Communication protocol
    /// </summary>
    /// <remarks>
    /// Acts as a synonym for whatever the latest version of the Sesame HTTP Protocol that is supported by dotNetRDF might be.  Currently this is Version 6 which includes SPARQL Update support (Sesame 2.4+ required)
    /// </remarks>
    public class SesameHttpProtocolConnector
        : SesameHttpProtocolVersion6Connector
    {
        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="storeID">Store ID</param>
        public SesameHttpProtocolConnector(String baseUri, String storeID)
            : base(baseUri, storeID) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="username">Username to use for requests that require authentication</param>
        /// <param name="password">Password to use for requests that require authentication</param>
        public SesameHttpProtocolConnector(String baseUri, String storeID, String username, String password)
            : base(baseUri, storeID, username, password) { }

#if !NO_PROXY

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="proxy">Proxy Server</param>
        public SesameHttpProtocolConnector(String baseUri, String storeID, WebProxy proxy)
            : base(baseUri, storeID, proxy) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="username">Username to use for requests that require authentication</param>
        /// <param name="password">Password to use for requests that require authentication</param>
        /// <param name="proxy">Proxy Server</param>
        public SesameHttpProtocolConnector(String baseUri, String storeID, String username, String password, WebProxy proxy)
            : base(baseUri, storeID, username, password, proxy) { }

#endif

#if !NO_SYNC_HTTP

        /// <summary>
        /// Gets the Store with the given ID
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <returns></returns>
        /// <remarks>
        /// If the Store ID requested represents the current instance then it is acceptable for an implementation to return itself.  Consumers of this method should be aware of this and if necessary use other means to create a connection to a store if they want a unique instance of the provider.
        /// </remarks>
        public override IStorageProvider GetStore(string storeID)
        {
            if (this._store.Equals(storeID))
            {
                return this;
            }
            else
            {
                return new SesameHttpProtocolConnector(this._baseUri, storeID);
            }
        }

#endif

        /// <summary>
        /// Gets a store asynchronously
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// If the store ID requested matches the current instance an instance <em>MAY</em> invoke the callback immediately returning a reference to itself
        /// </remarks>
        public override void GetStore(string storeID, AsyncStorageCallback callback, object state)
        {
            try
            {
                if (this._store.Equals(storeID))
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeID, this), state);
                }
                else
                {
                    IAsyncStorageProvider provider;
#if !NO_PROXY
                    provider = new SesameHttpProtocolConnector(this._baseUri, storeID, this._username, this._pwd, this.Proxy);
#else
                    provider = new SesameHttpProtocolConnector(this._baseUri, storeID, this._username, this._pwd);
#endif
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeID, provider), state);
                }
            }
            catch (Exception e)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeID, e), state);
            }
        }
    }

    /// <summary>
    /// Connector for connecting to a Store that supports the Sesame 2.0 HTTP Communication Protocol version 5 (i.e. no SPARQL Update support)
    /// </summary>
    public class SesameHttpProtocolVersion5Connector
        : BaseSesameHttpProtocolConnector
    {
        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="storeID">Store ID</param>
        public SesameHttpProtocolVersion5Connector(String baseUri, String storeID)
            : base(baseUri, storeID) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="username">Username to use for requests that require authentication</param>
        /// <param name="password">Password to use for requests that require authentication</param>
        public SesameHttpProtocolVersion5Connector(String baseUri, String storeID, String username, String password)
            : base(baseUri, storeID, username, password) { }

#if !NO_PROXY

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="proxy">Proxy Server</param>
        public SesameHttpProtocolVersion5Connector(String baseUri, String storeID, WebProxy proxy)
            : base(baseUri, storeID, proxy) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="username">Username to use for requests that require authentication</param>
        /// <param name="password">Password to use for requests that require authentication</param>
        /// <param name="proxy">Proxy Server</param>
        public SesameHttpProtocolVersion5Connector(String baseUri, String storeID, String username, String password, WebProxy proxy)
            : base(baseUri, storeID, username, password, proxy) { }

#endif

#if !NO_SYNC_HTTP

        public override IStoreTemplate GetDefaultTemplate(string id)
        {
            return new SesameMemTemplate(id);
        }

        public override IEnumerable<IStoreTemplate> GetAvailableTemplates(string id)
        {
            List<IStoreTemplate> templates = new List<IStoreTemplate>();
            Object[] args = new Object[] { id };
            foreach (Type t in this._templateTypes)
            {
                try
                {
                    IStoreTemplate template = Activator.CreateInstance(t, args) as IStoreTemplate;
                    if (template != null) templates.Add(template);
                }
                catch
                {
                    //Ignore and continue
                }
            }
            return templates;
        }

        public override bool CreateStore(IStoreTemplate template)
        {
            if (template is BaseSesameTemplate)
            {
                try
                {
                    Dictionary<String,String> createParams = new Dictionary<string,string>();
                    BaseSesameTemplate sesameTemplate = (BaseSesameTemplate)template;
                    IGraph g = sesameTemplate.GetTemplateGraph();

                    //Firstly we need to save the Repository Template as a new Context to Sesame
                    createParams.Add("context", sesameTemplate.ContextNode.ToString());
                    HttpWebRequest request = this.CreateRequest(this._repositoriesPrefix + BaseSesameHttpProtocolConnector.SystemRepositoryID + "/statements", "*/*", "POST", createParams);

                    request.ContentType = MimeTypesHelper.NTriples[0];
                    NTriplesWriter ntwriter = new NTriplesWriter();
                    ntwriter.Save(g, new StreamWriter(request.GetRequestStream()));

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
                        //If we get then it was OK
                        response.Close();
                    }

                    //Then we need to declare that said Context is of type rep:RepositoryContext
                    Triple repoType = new Triple(sesameTemplate.ContextNode, g.CreateUriNode("rdf:type"), g.CreateUriNode("rep:RepositoryContext"));
                    this.UpdateGraph(String.Empty, repoType.AsEnumerable(), null);

                    return true;
                }
                catch (WebException webEx)
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                    }
#endif
                    throw new RdfStorageException("A HTTP Error occurred while trying to create a new store", webEx);
                }
            }
            else
            {
                throw new RdfStorageException("Invalid templates, templates must derive from BaseSesameTemplate");
            }
        }

        /// <summary>
        /// Gets the Store with the given ID
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <returns></returns>
        /// <remarks>
        /// If the Store ID requested represents the current instance then it is acceptable for an implementation to return itself.  Consumers of this method should be aware of this and if necessary use other means to create a connection to a store if they want a unique instance of the provider.
        /// </remarks>
        public override IStorageProvider GetStore(string storeID)
        {
            if (this._store.Equals(storeID))
            {
                return this;
            }
            else
            {
#if !NO_PROXY
                return new SesameHttpProtocolVersion5Connector(this._baseUri, storeID, this._username, this._pwd, this.Proxy);
#else
                return new SesameHttpProtocolVersion5Connector(this._baseUri, storeID, this._username, this._pwd);
#endif
            }
        }
#endif

        public override void GetDefaultTemplate(string id, AsyncStorageCallback callback, object state)
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.NewTemplate, id, new SesameMemTemplate(id)), state);
        }

        public override void GetAvailableTemplates(string id, AsyncStorageCallback callback, object state)
        {
            List<IStoreTemplate> templates = new List<IStoreTemplate>();
            Object[] args = new Object[] { id };
            foreach (Type t in this._templateTypes)
            {
                try
                {
                    IStoreTemplate template = Activator.CreateInstance(t, args) as IStoreTemplate;
                    if (template != null) templates.Add(template);
                }
                catch
                {
                    //Ignore and continue
                }
            }
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.AvailableTemplates, id, templates), state);
        }

        public override void CreateStore(IStoreTemplate template, AsyncStorageCallback callback, object state)
        {
            if (template is BaseSesameTemplate)
            {
                try
                {
                    //First we need to store the template as a new context in the SYSTEM repository
                    Dictionary<String, String> createParams = new Dictionary<string, string>();
                    BaseSesameTemplate sesameTemplate = (BaseSesameTemplate)template;
                    IGraph g = sesameTemplate.GetTemplateGraph();
                    createParams.Add("context", sesameTemplate.ContextNode.ToString());
                    HttpWebRequest request = this.CreateRequest(this._repositoriesPrefix + BaseSesameHttpProtocolConnector.SystemRepositoryID + "/statements", "*/*", "POST", createParams);

                    request.ContentType = MimeTypesHelper.NTriples[0];
                    NTriplesWriter ntwriter = new NTriplesWriter();

                    this.SaveGraphAsync(request, ntwriter, g, (sender, args, st) =>
                        {
                            if (args.WasSuccessful)
                            {
                                //Then we need to declare that said Context is of type rep:RepositoryContext
                                Triple repoType = new Triple(sesameTemplate.ContextNode, g.CreateUriNode("rdf:type"), g.CreateUriNode("rep:RepositoryContext"));
                                this.UpdateGraph(String.Empty, repoType.AsEnumerable(), null, (sender2, args2, st2) =>
                                    {
                                        if (args.WasSuccessful)
                                        {
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, template), state);
                                        }
                                        else
                                        {
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, new RdfStorageException("An error occurred while trying to create a new Store, see inner exception for details", args.Error)), state);
                                        }
                                    }, st);
                            }
                            else
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, template, new RdfStorageException("An error occurred while trying to create a new Store, see inner exception for details", args.Error)), state);
                            }
                        }, state);
                }
                catch (WebException webEx)
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                    }
#endif
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, template, new RdfStorageException("A HTTP Error occurred while trying to create a new Store asynchronously, see inner exception for details", webEx)), state);
                }
                catch (Exception ex)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, template, new RdfStorageException("Unexpected error while trying to create a new Store asynchronously, see inner exception for details", ex)), state);
                    return;
                }
            }
            else
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, template, new RdfStorageException("Invalid template, templates must derive from BaseSesameTemplate")), state);
            }
        }

        /// <summary>
        /// Gets a store asynchronously
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// If the store ID requested matches the current instance an instance <em>MAY</em> invoke the callback immediately returning a reference to itself
        /// </remarks>
        public override void GetStore(string storeID, AsyncStorageCallback callback, object state)
        {
            try
            {
                if (this._store.Equals(storeID))
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeID, this), state);
                }
                else
                {
                    IAsyncStorageProvider provider;
#if !NO_PROXY
                    provider = new SesameHttpProtocolVersion5Connector(this._baseUri, storeID, this._username, this._pwd, this.Proxy);
#else
                    provider = new SesameHttpProtocolVersion5Connector(this._baseUri, storeID, this._username, this._pwd);
#endif
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeID, provider), state);
                }
            }
            catch (Exception e)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeID, e), state);
            }
        }
    }

    /// <summary>
    /// Connector for connecting to a Store that supports the Sesame 2.0 HTTP Communication Protocol version 6 (i.e. includes SPARQL Update support)
    /// </summary>
    public class SesameHttpProtocolVersion6Connector
        : SesameHttpProtocolVersion5Connector
#if !NO_SYNC_HTTP
        , IUpdateableStorage
#endif
    {
        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="storeID">Store ID</param>
        public SesameHttpProtocolVersion6Connector(String baseUri, String storeID)
            : base(baseUri, storeID) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="username">Username to use for requests that require authentication</param>
        /// <param name="password">Password to use for requests that require authentication</param>
        public SesameHttpProtocolVersion6Connector(String baseUri, String storeID, String username, String password)
            : base(baseUri, storeID, username, password) { }

#if !NO_PROXY

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="proxy">Proxy Server</param>
        public SesameHttpProtocolVersion6Connector(String baseUri, String storeID, WebProxy proxy)
            : base(baseUri, storeID, proxy) { }

        /// <summary>
        /// Creates a new connection to a Sesame HTTP Protocol supporting Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Store</param>
        /// <param name="storeID">Store ID</param>
        /// <param name="username">Username to use for requests that require authentication</param>
        /// <param name="password">Password to use for requests that require authentication</param>
        /// <param name="proxy">Proxy Server</param>
        public SesameHttpProtocolVersion6Connector(String baseUri, String storeID, String username, String password, WebProxy proxy)
            : base(baseUri, storeID, username, password, proxy) { }

#endif

#if !NO_SYNC_HTTP

        /// <summary>
        /// Gets the Store with the given ID
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <returns></returns>
        /// <remarks>
        /// If the implementation is also an instance of <see cref="IStorageProvider">IStorageProvider</see> and the requested Store ID represents the current instance then it is acceptable for an implementation to return itself.  Consumers of this method should be aware of this and if necessary use other means to create a connection to a store if they want a unique instance of the provider.
        /// </remarks>
        public override IStorageProvider GetStore(string storeID)
        {
            if (this._store.Equals(storeID))
            {
                return this;
            }
            else
            {
#if !NO_PROXY
                return new SesameHttpProtocolVersion6Connector(this._baseUri, storeID, this._username, this._pwd, this.Proxy);
#else
                return new SesameHttpProtocolVersion6Connector(this._baseUri, storeID, this._username, this._pwd);
#endif
            }
        }

        /// <summary>
        /// Makes a SPARQL Update request to the Sesame server
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update</param>
        public virtual void Update(string sparqlUpdate)
        {
            try
            {
                HttpWebRequest request;

                //Create the Request
                request = this.CreateRequest(this._repositoriesPrefix + this._store + this._updatePath, MimeTypesHelper.Any, "POST", new Dictionary<String, String>());

                //Build the Post Data and add to the Request Body
                request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                StringBuilder postData = new StringBuilder();
                postData.Append("update=");
                postData.Append(Uri.EscapeDataString(EscapeQuery(sparqlUpdate)));
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                {
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
                    //If we get here it completed OK
                    response.Close();
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
                            throw new RdfQueryException("A HTTP error occured while updating the Store.  Store returned the following error message: " + responseText, webEx);
                        }
                        catch
                        {
                            throw new RdfQueryException("A HTTP error occurred while updating the Store", webEx);
                        }
                    }
                    else
                    {
                        throw new RdfQueryException("A HTTP error occurred while updating the Store", webEx);
                    }
                }
                else
                {
                    throw new RdfQueryException("A HTTP error occurred while updating the Store", webEx);
                }
            }
        }

#endif

        /// <summary>
        /// Gets a store asynchronously
        /// </summary>
        /// <param name="storeID">Store ID</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// If the store ID requested matches the current instance an instance <em>MAY</em> invoke the callback immediately returning a reference to itself
        /// </remarks>
        public override void GetStore(string storeID, AsyncStorageCallback callback, object state)
        {
            try
            {
                if (this._store.Equals(storeID))
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeID, this), state);
                }
                else
                {
                    IAsyncStorageProvider provider;
#if !NO_PROXY
                    provider = new SesameHttpProtocolVersion6Connector(this._baseUri, storeID, this._username, this._pwd, this.Proxy);
#else
                    provider = new SesameHttpProtocolVersion6Connector(this._baseUri, storeID, this._username, this._pwd);
#endif
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeID, provider), state);
                }
            }
            catch (Exception e)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeID, e), state);
            }
        }

        /// <summary>
        /// Makes a SPARQL Update request to the Sesame server
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void Update(string sparqlUpdate, AsyncStorageCallback callback, Object state)
        {
            try
            {
                HttpWebRequest request;

                //Create the Request
                request = this.CreateRequest(this._repositoriesPrefix + this._store + this._updatePath, MimeTypesHelper.Any, "POST", new Dictionary<String, String>());

                //Build the Post Data and add to the Request Body
                request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                StringBuilder postData = new StringBuilder();
                postData.Append("update=");
                postData.Append(Uri.EscapeDataString(EscapeQuery(sparqlUpdate)));

                request.BeginGetRequestStream(r =>
                    {
                        try
                        {
                            Stream stream = request.EndGetRequestStream(r);
                            using (StreamWriter writer = new StreamWriter(stream))
                            {
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
                            request.BeginGetResponse(r2 =>
                                 {
                                     try
                                     {
                                         HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r2);
#if DEBUG
                                         if (Options.HttpDebugging)
                                         {
                                             Tools.HttpDebugResponse(response);
                                         }
#endif
                                         //If we get here it completed OK
                                         response.Close();
                                         callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate), state);
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
                                                     callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, new RdfStorageException("A HTTP error occured while updating the Store.  Store returned the following error message: " + responseText, webEx)), state);
                                                 }
                                                 catch
                                                 {
                                                     callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, new RdfStorageException("A HTTP error occurred while updating the Store", webEx)), state);
                                                 }
                                             }
                                             else
                                             {
                                                 callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, new RdfStorageException("A HTTP error occurred while updating the Store", webEx)), state);
                                             }
                                         }
                                         else
                                         {
                                             callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, new RdfStorageException("A HTTP error occurred while updating the Store", webEx)), state);
                                         }
                                     }
                                     catch (Exception ex)
                                     {
                                         callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, new RdfStorageException("An unexpected error occurred while updating the Store", ex)), state);
                                     }
                                 }, state);
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
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, new RdfStorageException("A HTTP error occured while updating the Store.  Store returned the following error message: " + responseText, webEx)), state);
                                    }
                                    catch
                                    {
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, new RdfStorageException("A HTTP error occurred while updating the Store", webEx)), state);
                                    }
                                }
                                else
                                {
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, new RdfStorageException("A HTTP error occurred while updating the Store", webEx)), state);
                                }
                            }
                            else
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, new RdfStorageException("A HTTP error occurred while updating the Store", webEx)), state);
                            }
                        }
                        catch (Exception ex)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, new RdfStorageException("An unexpected error occurred while updating the Store", ex)), state);
                        }
                    }, state);
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
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, new RdfStorageException("A HTTP error occured while updating the Store.  Store returned the following error message: " + responseText, webEx)), state);
                        }
                        catch
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, new RdfStorageException("A HTTP error occurred while updating the Store", webEx)), state);
                        }
                    }
                    else
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, new RdfStorageException("A HTTP error occurred while updating the Store", webEx)), state);
                    }
                }
                else
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, new RdfStorageException("A HTTP error occurred while updating the Store", webEx)), state);
                }
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, new RdfStorageException("An unexpected error occurred while updating the Store", ex)), state);
            }
        }
    }

}