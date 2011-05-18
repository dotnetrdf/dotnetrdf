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

#if !NO_STORAGE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
#if !NO_WEB
using System.Web;
#endif
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage.Params;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Reasoning modes supported by Stardog
    /// </summary>
    public enum StardogReasoningMode
    {
        /// <summary>
        /// No Reasoning (default)
        /// </summary>
        None,
        /// <summary>
        /// OWL-QL Reasoning
        /// </summary>
        QL
    }

    /// <summary>
    /// Class for connecting to a Stardog store via HTTP
    /// </summary>
    /// <remarks>
    /// <para>
    /// Has full support for Stardog Transactions, connection is in auto-commit mode by default i.e. all write operations (Delete/Save/Update) will create and use a dedicated transaction for their operation.  You can manage Transactions using the <see cref="StardogConnector.Begin">Begin()</see>, <see cref="StardogConnector.Commit">Commit()</see> and <see cref="StardogConnector.Rollback">Rollback()</see> methods.
    /// </para>
    /// <para>
    /// Transactions are always scoped to Managed Threads so each Thread has access to an independent and isolated transaction should you desire.  Attempting to start a new transaction when there is already an existing transaction on the thread will cause an error as will committing/rolling back a transaction when there is none on the current thread.  While the connector allows for multiple transactions and concurrency naturally be mindful that Stardog currently is MRSW (Multiple Reader Single Writer) safe only and you should ensure you adhere to that when using it as the connector will not enforce it for you.
    /// </para>
    /// </remarks>
    /// <threadsafety instance="true">
    /// <para>
    /// The StardogConnector is designed to be thread safe, each threads transactions are isolated from each other.  A thread may only have one active transaction at any one time, an <see cref="RdfStorageException">RdfStorageException</see> will be thrown if a thread violates this rule.
    /// </para>
    /// <para>
    /// <strong>Note:</strong> As per the remarks Stardog currently is only MRSW safe, this connector does not enforce this so please be mindful of this when coding
    /// </para>
    /// </threadsafety>
    public class StardogConnector : IQueryableGenericIOManager, IConfigurationSerializable
    {
        private String _baseUri;
        private String _kb;
        private String _username;
        private String _pwd;
        private bool _hasCredentials = false;
        private StardogReasoningMode _reasoning = StardogReasoningMode.None;

        private ThreadSafeReference<String> _activeTrans = new ThreadSafeReference<string>();

        private StringBuilder _output = new StringBuilder();
        private SparqlFormatter _sparqlFormatter = new SparqlFormatter();
        private TriGWriter _writer = new TriGWriter();

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="reasoning">Reasoning Mode</param>
        public StardogConnector(String baseUri, String kbID, StardogReasoningMode reasoning)
        {
            this._baseUri = baseUri;
            if (!this._baseUri.EndsWith("/")) this._baseUri += "/";
            this._kb = kbID;

            //Prep the writer
            this._writer.HighSpeedModePermitted = true;
            this._writer.CompressionLevel = WriterCompressionLevel.None;
            this._writer.UseMultiThreadedWriting = false;
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        public StardogConnector(String baseUri, String kbID)
            : this(baseUri, kbID, StardogReasoningMode.None) { }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="reasoning">Reasoning Mode</param>
        public StardogConnector(String baseUri, String kbID, StardogReasoningMode reasoning, String username, String password)
            : this(baseUri, kbID)
        {
            this._username = username;
            this._pwd = password;
            this._hasCredentials = true;
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public StardogConnector(String baseUri, String kbID, String username, String password)
            : this(baseUri, kbID, StardogReasoningMode.None, username, password) { }

        /// <summary>
        /// Gets the Base URI of the Stardog server
        /// </summary>
        public String BaseUri
        {
            get
            {
                return this._baseUri;
            }
        }

        /// <summary>
        /// Gets/Sets the reasoning mode to use for queries
        /// </summary>
        public StardogReasoningMode Reasoning
        {
            get
            {
                return this._reasoning;
            }
            set
            {
                this._reasoning = value;
            }
        }

        /// <summary>
        /// Makes a SPARQL Query against the underlying Store using whatever reasoning mode is currently in-use
        /// </summary>
        /// <param name="sparqlQuery">Sparql Query</param>
        /// <returns></returns>
        public object Query(string sparqlQuery)
        {
            try
            {
                HttpWebRequest request;

                String tID = (this._activeTrans.Value == null) ? String.Empty : "/" + this._activeTrans.Value;

                //Create the Request
                Dictionary<String, String> queryParams = new Dictionary<string, string>();
                if (sparqlQuery.Length < 2048)
                {
                    queryParams.Add("query", sparqlQuery);

                    request = this.CreateRequest(this._kb + tID + "/query", MimeTypesHelper.Any, "GET", queryParams);
                }
                else
                {
                    request = this.CreateRequest(this._kb + tID + "/query", MimeTypesHelper.Any, "POST", queryParams);

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
                        ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(ctype, Regex.IsMatch(sparqlQuery, "ASK", RegexOptions.IgnoreCase));
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
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Uri of the Graph to load</param>
        /// <remarks>
        /// If an empty/null Uri is specified then the Default Graph of the Store will be loaded
        /// </remarks>
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
        /// <remarks>
        /// If an empty/null Uri is specified then the Default Graph of the Store will be loaded
        /// </remarks>
        public void LoadGraph(IGraph g, string graphUri)
        {
            try
            {
                HttpWebRequest request;
                Dictionary<String, String> serviceParams = new Dictionary<string, string>();

                String tID = (this._activeTrans.Value == null) ? String.Empty : "/" + this._activeTrans.Value;
                String requestUri = this._kb + tID + "/query";
                SparqlParameterizedString construct = new SparqlParameterizedString();
                if (!graphUri.Equals(String.Empty))
                {
                    construct.CommandText = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH @graph { ?s ?p ?o } }";
                    construct.SetUri("graph", new Uri(graphUri));
                    if (g.IsEmpty) g.BaseUri = new Uri(graphUri);
                }
                else
                {
                    construct.CommandText = "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }";
                }
                serviceParams.Add("query", construct.ToString());

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
                    parser.Load(g, new StreamReader(response.GetResponseStream()));
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
        /// Saves a Graph into the Store (see remarks for notes on merge/overwrite behaviour)
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <remarks>
        /// <para>
        /// If the Graph has no URI then the contents will be appended to the Store's Default Graph.  If the Graph has a URI then existing Graph associated with that URI will be replaced.  To append to a named Graph use the <see cref="StardogConnector.UpdateGraph">UpdateGraph()</see> method instead
        /// </para>
        /// </remarks>
        public void SaveGraph(IGraph g)
        {
            String tID = null;
            try
            {
                //Get a Transaction ID, if there is no active Transaction then this operation will be auto-committed
                tID = (this._activeTrans.Value != null) ? this._activeTrans.Value : this.BeginTransaction();

                HttpWebRequest request = this.CreateRequest(this._kb + "/" + tID + "/add", MimeTypesHelper.Any, "POST", new Dictionary<string,string>());
                request.ContentType = MimeTypesHelper.TriG[0];

                if (g.BaseUri != null)
                {
                    try
                    {
                        this.DeleteGraph(g.BaseUri);
                    }
                    catch (Exception ex)
                    {
                        throw new RdfStorageException("Unable to save a Named Graph to the Store as this requires deleting any existing Named Graph with this name which failed, see inner exception for more detail", ex);
                    }
                }
                
                //Save the Data as TriG to the Request Stream
                TripleStore store = new TripleStore();
                store.Add(g);
                this._writer.Save(store, new StreamParams(request.GetRequestStream()));

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
                    //If we get here then it was OK
                    response.Close();
                }

                //Commit Transaction only if in auto-commit mode (active transaction will be null)
                if (this._activeTrans.Value == null)
                {
                    try
                    {
                        this.CommitTransaction(tID);
                    }
                    catch (Exception ex)
                    {
                        throw new RdfStorageException("Stardog failed to commit a Transaction", ex);
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

                //Rollback Transaction only if got as far as creating a Transaction
                //and in auto-commit mode
                if (tID != null)
                {
                    if (this._activeTrans.Value == null)
                    {
                        try
                        {
                            this.RollbackTransaction(tID);
                        }
                        catch (Exception ex)
                        {
                            throw new RdfStorageException("Stardog failed to rollback a Transaction", ex);
                        }
                    }
                }

                throw new RdfStorageException("A HTTP Error occurred while trying to save a Graph to the Store", webEx);
            }
        }

        /// <summary>
        /// Updates a Graph in the Stardog Store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <remarks>
        /// Removals happen before additions
        /// </remarks>
        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            String tID = null;
            try
            {
                //Get a Transaction ID, if there is no active Transaction then this operation will be auto-committed
                tID = (this._activeTrans.Value != null) ? this._activeTrans.Value : this.BeginTransaction();

                //First do the Removals
                if (removals != null)
                {
                    if (removals.Any())
                    {
                        HttpWebRequest request = this.CreateRequest(this._kb + "/" + tID + "/remove", MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
                        request.ContentType = MimeTypesHelper.TriG[0];

                        //Save the Data to be removed as TriG to the Request Stream
                        TripleStore store = new TripleStore();
                        Graph g = new Graph();
                        g.Assert(removals);
                        g.BaseUri = graphUri;
                        store.Add(g);
                        this._writer.Save(store, new StreamParams(request.GetRequestStream()));

                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            response.Close();
                        }
                    }
                }

                //Then do the Additions
                if (additions != null)
                {
                    if (additions.Any())
                    {
                        HttpWebRequest request = this.CreateRequest(this._kb + "/" + tID + "/add", MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
                        request.ContentType = MimeTypesHelper.TriG[0];

                        //Save the Data to be removed as TriG to the Request Stream
                        TripleStore store = new TripleStore();
                        Graph g = new Graph();
                        g.Assert(additions);
                        g.BaseUri = graphUri;
                        store.Add(g);
                        this._writer.Save(store, new StreamParams(request.GetRequestStream()));

                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            response.Close();
                        }
                    }
                }

                //Commit Transaction only if in auto-commit mode (active transaction will be null)
                if (this._activeTrans.Value == null)
                {
                    try
                    {
                        this.CommitTransaction(tID);
                    }
                    catch (Exception ex)
                    {
                        throw new RdfStorageException("Stardog failed to commit a Transaction", ex);
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
                //Rollback Transaction only if got as far as creating a Transaction
                //and in auto-commit mode
                if (tID != null)
                {
                    if (this._activeTrans.Value == null)
                    {
                        try
                        {
                            this.RollbackTransaction(tID);
                        }
                        catch (Exception ex)
                        {
                            throw new RdfStorageException("Stardog failed to rollback a Transaction", ex);
                        }
                    }
                }

                throw new RdfStorageException("A HTTP Error occurred while trying to update a Graph in the Store", webEx);
            }
        }

        /// <summary>
        /// Updates a Graph in the Stardog store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (graphUri == null || graphUri.Equals(String.Empty))
            {
                this.UpdateGraph((Uri)null, additions, removals);
            }
            else
            {
                this.UpdateGraph(new Uri(graphUri), additions, removals);
            }
        }

        /// <summary>
        /// Returns that Updates are supported on Stardog Stores
        /// </summary>
        public bool UpdateSupported
        {
            get 
            {
                return true;
            }
        }

        /// <summary>
        /// Deletes a Graph from the Stardog store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public void DeleteGraph(Uri graphUri)
        {
            this.DeleteGraph(graphUri.ToSafeString());
        }

        /// <summary>
        /// Deletes a Graph from the Stardog store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public void DeleteGraph(String graphUri)
        {
            String tID = null;
            try
            {
                //Get a Transaction ID, if there is no active Transaction then this operation will be auto-committed
                tID = (this._activeTrans.Value != null) ? this._activeTrans.Value : this.BeginTransaction();

                HttpWebRequest request;
                if (!graphUri.Equals(String.Empty))
                {
                    request = this.CreateRequest(this._kb + "/" + tID + "/clear/?graph-uri=" + Uri.EscapeDataString(graphUri), MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
                }
                else
                {
                    request = this.CreateRequest(this._kb + "/" + tID + "/clear/?graph-uri=DEFAULT", MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
                }
                request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
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
                    //If we get here then the Delete worked OK
                    response.Close();
                }

                //Commit Transaction only if in auto-commit mode (active transaction will be null)
                if (this._activeTrans.Value == null)
                {
                    try
                    {
                        this.CommitTransaction(tID);
                    }
                    catch (Exception ex)
                    {
                        throw new RdfStorageException("Stardog failed to commit a Transaction", ex);
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

                //Rollback Transaction only if got as far as creating a Transaction
                //and in auto-commit mode
                if (tID != null)
                {
                    if (this._activeTrans.Value == null)
                    {
                        try
                        {
                            this.RollbackTransaction(tID);
                        }
                        catch (Exception ex)
                        {
                            throw new RdfStorageException("Stardog failed to rollback a Transaction", ex);
                        }
                    }
                }

                throw new RdfStorageException("A HTTP Error occurred while trying to delete a Graph from the Store", webEx);
            }
        }

        /// <summary>
        /// Returns that deleting graphs from the Stardog store is not yet supported (due to a .Net specific issue)
        /// </summary>
        public bool DeleteSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the list of Graphs in the Stardog store
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Uri> ListGraphs()
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
                throw new RdfStorageException("Stardog returned an error while trying to List Graphs, see inner exception for details", ex);
            }
        }

        /// <summary>
        /// Returns that listing Graphs is supported
        /// </summary>
        public bool ListGraphsSupported
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
        private HttpWebRequest CreateRequest(String servicePath, String accept, String method, Dictionary<String, String> queryParams)
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

            //Add the special Stardog Headers
            request.Headers.Add("SD-Connection-String", "kb=" + this._kb + ";persist=sync" + this.GetReasoningParameter());
            request.Headers.Add("SD-Protocol", "1.0");

            //Add Credentials if needed
            if (this._hasCredentials)
            {
                NetworkCredential credentials = new NetworkCredential(this._username, this._pwd);
                request.Credentials = credentials;
            }

            return request;
        }

        private String GetReasoningParameter()
        {
            switch (this._reasoning)
            {
                case StardogReasoningMode.QL:
                    return ";reasoning=QL";
                case StardogReasoningMode.None:
                default:
                    return String.Empty;
            }
        }

        #region Stardog Transaction Support

        private String BeginTransaction()
        {
            String tID = null;

            HttpWebRequest request = this.CreateRequest(this._kb + "/transaction/begin", MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    tID = reader.ReadToEnd();
                    reader.Close();
                }
                response.Close();
            }

            if (tID.Equals(String.Empty))
            {
                throw new RdfStorageException("Stardog failed to begin a Transaction");
            }
            return tID;
        }

        private void CommitTransaction(String tID)
        {
            HttpWebRequest request = this.CreateRequest(this._kb + "/transaction/commit/" + tID, MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                response.Close();
            }

            //Reset the Active Transaction on this Thread if the IDs match up
            if (this._activeTrans.Value != null && this._activeTrans.Value.Equals(tID))
            {
                this._activeTrans.Value = null;
            }
        }

        private void RollbackTransaction(String tID)
        {
            HttpWebRequest request = this.CreateRequest(this._kb + "/transaction/rollback/" + tID, MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                response.Close();
            }

            //Reset the Active Transaction on this Thread if the IDs match up
            if (this._activeTrans.Value != null && this._activeTrans.Value.Equals(tID))
            {
                this._activeTrans.Value = null;
            }
        }

        /// <summary>
        /// Begins a new Transaction
        /// </summary>
        /// <exception cref="RdfStorageException">Thrown if there is already an active Transaction on the current Thread</exception>
        /// <remarks>
        /// Transactions are scoped to Managed Threads
        /// </remarks>
        public void Begin()
        {
            if (this._activeTrans.Value != null)
            {
                throw new RdfStorageException("Cannot start a new Transaction on the current Thread as there is already an active Transaction on this Thread");
            }
            this._activeTrans.Value = this.BeginTransaction();
        }

        /// <summary>
        /// Commits the active Transaction
        /// </summary>
        /// <exception cref="RdfStorageException">Thrown if there is not an active Transaction on the current Thread</exception>
        /// <remarks>
        /// Transactions are scoped to Managed Threads
        /// </remarks>
        public void Commit()
        {
            if (this._activeTrans.Value == null)
            {
                throw new RdfStorageException("Cannot commit a Transaction on the current Thread as there is no active Transaction on this Thread");
            }
            this.CommitTransaction(this._activeTrans.Value);
        }

        /// <summary>
        /// Rolls back the active Transaction
        /// </summary>
        /// <exception cref="RdfStorageException">Thrown if there is not an active Transaction on the current Thread</exception>
        /// <remarks>
        /// Transactions are scoped to Managed Threads
        /// </remarks>
        public void Rollback()
        {
            if (this._activeTrans.Value == null)
            {
                throw new RdfStorageException("Cannot rollback a Transaction on the current Thread as there is no active Transaction on this Thread");
            }
            this.RollbackTransaction(this._activeTrans.Value);
        }

        #endregion

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
            return "[Stardog] Knowledge Base '" + this._kb + "' on Server '" + this._baseUri + "'";
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
            INode store = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyStore);

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName)));
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(this._baseUri)));
            context.Graph.Assert(new Triple(manager, store, context.Graph.CreateLiteralNode(this._kb)));

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