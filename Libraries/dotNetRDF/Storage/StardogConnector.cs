/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage.Management;
using VDS.RDF.Writing;
using System.Web;

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
        QL,

        /// <summary>
        /// OWL-EL Reasoning
        /// </summary>
        EL,

        /// <summary>
        /// OWL-RL Reasoning
        /// </summary>
        RL,

        /// <summary>
        /// OWL-DL Reasoning
        /// </summary>
        DL,

        /// <summary>
        /// RDFS Reasoning
        /// </summary>
        RDFS,

        /// <summary>
        /// RDFS, QL, RL, and EL axioms, plus SWRL rules
        /// </summary>
        SL,

        /// <summary>
        /// As of Stardog 3.x the reasoning mode is no longer a connection property and is instead managed at the database level
        /// </summary>
        DatabaseControlled
    }

    /// <summary>
    /// Abstract implementation of a connector for Stardog that connects using the HTTP protocol
    /// </summary>
    /// <remarks>
    /// <para>
    /// Has full support for Stardog Transactions, connection is in auto-commit mode by default i.e. all write operations (Delete/Save/Update) will create and use a dedicated transaction for their operation, if the operation fails the transaction will automatically be rolled back.  You can manage Transactions using the <see cref="BaseStardogConnector.Begin()">Begin()</see>, <see cref="BaseStardogConnector.Commit()">Commit()</see> and <see cref="BaseStardogConnector.Rollback()">Rollback()</see> methods.
    /// </para>
    /// <para>
    /// The connector maintains a single transaction which is shared across all threads since Stardog is currently provides only MRSW (Multiple Reader Single Writer) concurrency and does not permit multiple transactions to occur simultaneously.  
    /// </para>
    /// </remarks>
    public abstract class BaseStardogConnector
        : BaseAsyncHttpConnector, IAsyncQueryableStorage, IAsyncTransactionalStorage, IConfigurationSerializable
        , IQueryableStorage, ITransactionalStorage, IReasoningQueryableStorage
    {
        /// <summary>
        /// Constant for the default Anonymous user account and password used by Stardog if you have not supplied a shiro.ini file or otherwise disabled security
        /// </summary>
        public const string AnonymousUser = "anonymous";

        private readonly string _baseUri, _kb, _username, _pwd;
        private readonly bool _hasCredentials;
        private StardogReasoningMode _reasoning = StardogReasoningMode.None;

        private string _activeTrans;
        private readonly TriGWriter _writer = new TriGWriter();

        /// <summary>
        /// The underlying server connection
        /// </summary>
        protected BaseStardogServer Server;

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="reasoning">Reasoning Mode</param>
        protected BaseStardogConnector(string baseUri, string kbID, StardogReasoningMode reasoning)
            : this(baseUri, kbID, reasoning, null, null)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        protected BaseStardogConnector(string baseUri, string kbID)
            : this(baseUri, kbID, StardogReasoningMode.None)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        protected BaseStardogConnector(string baseUri, string kbID, string username, string password)
            : this(baseUri, kbID, StardogReasoningMode.None, username, password)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="reasoning">Reasoning Mode</param>
        protected BaseStardogConnector(string baseUri, string kbID, StardogReasoningMode reasoning, string username,
            string password)
        {
            _baseUri = baseUri;
            if (!_baseUri.EndsWith("/")) _baseUri += "/";
            _kb = kbID;
            _reasoning = reasoning;

            // Prep the writer
            _writer.HighSpeedModePermitted = true;
            _writer.CompressionLevel = WriterCompressionLevel.None;
            _writer.UseMultiThreadedWriting = false;

            _username = username;
            _pwd = password;
            _hasCredentials = (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password));

            // Server reference
            Server = new StardogV1Server(_baseUri, _username, _pwd);
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="reasoning">Reasoning Mode</param>
        /// <param name="proxy">Proxy Server</param>
        protected BaseStardogConnector(string baseUri, string kbID, StardogReasoningMode reasoning, IWebProxy proxy)
            : this(baseUri, kbID, reasoning, null, null, proxy)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="reasoning">Reasoning Mode</param>
        /// <param name="proxy">Proxy Server</param>
        protected BaseStardogConnector(string baseUri, string kbID, StardogReasoningMode reasoning, string username,
            string password, IWebProxy proxy)
            : this(baseUri, kbID, reasoning, username, password)
        {
            Proxy = proxy;
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="proxy">Proxy Server</param>
        protected BaseStardogConnector(string baseUri, string kbID, IWebProxy proxy)
            : this(baseUri, kbID, StardogReasoningMode.None, proxy)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="proxy">Proxy Server</param>
        protected BaseStardogConnector(string baseUri, string kbID, string username, string password, IWebProxy proxy)
            : this(baseUri, kbID, StardogReasoningMode.None, username, password, proxy)
        {
        }

        /// <summary>
        /// Gets the Base URI of the Stardog server
        /// </summary>
        public string BaseUri => _baseUri;

        /// <summary>
        /// Gets the knowledge base ID being used by this connector
        /// </summary>
        public string KbId => _kb;

        /// <summary>
        /// Gets/Sets the reasoning mode to use for queries
        /// </summary>
        [Description("What reasoning mode (if any) is currently in use for SPARQL Queries")]
        public virtual StardogReasoningMode Reasoning
        {
            get => _reasoning;
            set => _reasoning = value;
        }

        /// <summary>
        /// Gets the IO Behaviour of Stardog
        /// </summary>
        public override IOBehaviour IOBehaviour => IOBehaviour.GraphStore | IOBehaviour.CanUpdateTriples;

        /// <summary>
        /// Returns that listing Graphs is supported
        /// </summary>
        public override bool ListGraphsSupported => true;

        /// <summary>
        /// Returns that the Connection is ready
        /// </summary>
        public override bool IsReady => true;

        /// <summary>
        /// Returns that the Connection is not read-only
        /// </summary>
        public override bool IsReadOnly => false;

        /// <summary>
        /// Returns that Updates are supported on Stardog Stores
        /// </summary>
        public override bool UpdateSupported => true;

        /// <summary>
        /// Returns that deleting graphs from the Stardog store is not yet supported (due to a .Net specific issue)
        /// </summary>
        public override bool DeleteSupported => true;

        /// <summary>
        /// Gets the parent server
        /// </summary>
        public override IStorageServer ParentServer => Server;

        /// <summary>
        /// Makes a SPARQL Query against the underlying Store using whatever reasoning mode is currently in-use
        /// </summary>
        /// <param name="sparqlQuery">Sparql Query</param>
        /// <returns></returns>
        public virtual object Query(string sparqlQuery)
        {
            Graph g = new Graph();
            SparqlResultSet results = new SparqlResultSet();
            Query(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery);

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
        /// Makes a SPARQL Query against the underlying Store using whatever reasoning mode is currently in-use, the reasoning can be set by query
        /// </summary>
        /// <param name="sparqlQuery">Sparql Query</param>
        /// <param name="reasoning"></param>
        /// <returns></returns>
        public virtual object Query(string sparqlQuery, bool reasoning)
        {
            Graph g = new Graph();
            SparqlResultSet results = new SparqlResultSet();
            Query(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery, reasoning);

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
        /// Makes a SPARQL Query against the underlying Store using whatever reasoning mode is currently in-use processing the results using an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public virtual void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
        {
            try
            {
                HttpWebRequest request;

                string tID = (_activeTrans == null) ? string.Empty : "/" + _activeTrans;

                // String accept = MimeTypesHelper.HttpRdfOrSparqlAcceptHeader;
                string accept =
                    MimeTypesHelper.CustomHttpAcceptHeader(
                        MimeTypesHelper.SparqlResultsXml.Concat(
                            MimeTypesHelper.Definitions.Where(d => d.CanParseRdf).SelectMany(d => d.MimeTypes)));

                // Create the Request
                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                if (sparqlQuery.Length < 2048)
                {
                    queryParams.Add("query", sparqlQuery);
                    request = CreateRequest(_kb + tID + "/query", accept, "GET", queryParams);
                }
                else
                {
                    request = CreateRequest(_kb + tID + "/query", accept, "POST", queryParams);

                    // Build the Post Data and add to the Request Body
                    request.ContentType = MimeTypesHelper.Utf8WWWFormURLEncoded;
                    StringBuilder postData = new StringBuilder();
                    postData.Append("query=");
                    postData.Append(HttpUtility.UrlEncode(sparqlQuery));
                    using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), new UTF8Encoding()))
                    {
                        writer.Write(postData);
                        writer.Close();
                    }
                }

                Tools.HttpDebugRequest(request);

                // Get the Response and process based on the Content Type
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);

                    StreamReader data = new StreamReader(response.GetResponseStream());
                    string ctype = response.ContentType;
                    try
                    {
                        // Is the Content Type referring to a Sparql Result Set format?
                        ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(ctype,
                            Regex.IsMatch(sparqlQuery, "ASK", RegexOptions.IgnoreCase));
                        resreader.Load(resultsHandler, data);
                        response.Close();
                    }
                    catch (RdfParserSelectionException)
                    {
                        // If we get a Parser Selection exception then the Content Type isn't valid for a Sparql Result Set

                        // Is the Content Type referring to a RDF format?
                        IRdfReader rdfreader = MimeTypesHelper.GetParser(ctype);
                        rdfreader.Load(rdfHandler, data);
                        response.Close();
                    }
                }
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpQueryError(webEx);
            }
        }


        /// <summary>
        /// Makes a SPARQL Query against the underlying Store using whatever reasoning mode is currently in-use processing the results using an appropriate handler from those provided, the reasoning can be set by query
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="reasoning"></param>
        /// <returns></returns>
        public virtual void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery, bool reasoning)
        {
            try
            {
                HttpWebRequest request;

                string tID = (_activeTrans == null) ? string.Empty : "/" + _activeTrans;

                // String accept = MimeTypesHelper.HttpRdfOrSparqlAcceptHeader;
                string accept =
                    MimeTypesHelper.CustomHttpAcceptHeader(
                        MimeTypesHelper.SparqlResultsXml.Concat(
                            MimeTypesHelper.Definitions.Where(d => d.CanParseRdf).SelectMany(d => d.MimeTypes)));

                // Create the Request
                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                if (sparqlQuery.Length < 2048)
                {
                    queryParams.Add("query", sparqlQuery);
                    queryParams.Add("reasoning", reasoning.ToString());

                    request = CreateRequest(_kb + tID + "/query", accept, "GET", queryParams);
                }
                else
                {
                    request = CreateRequest(_kb + tID + "/query", accept, "POST", queryParams);

                    // Build the Post Data and add to the Request Body
                    request.ContentType = MimeTypesHelper.Utf8WWWFormURLEncoded;
                    StringBuilder postData = new StringBuilder();
                    if (reasoning)
                    {
                        postData.Append("reasoning=");
                        postData.Append(reasoning.ToString() + "&");
                    }
                    postData.Append("query=");
                    postData.Append(HttpUtility.UrlEncode(sparqlQuery));
                    using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), new UTF8Encoding()))
                    {
                        writer.Write(postData);
                        writer.Close();
                    }
                }

                Tools.HttpDebugRequest(request);

                // Get the Response and process based on the Content Type
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);

                    StreamReader data = new StreamReader(response.GetResponseStream());
                    string ctype = response.ContentType;
                    try
                    {
                        // Is the Content Type referring to a Sparql Result Set format?
                        ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(ctype,
                            Regex.IsMatch(sparqlQuery, "ASK", RegexOptions.IgnoreCase));
                        resreader.Load(resultsHandler, data);
                        response.Close();
                    }
                    catch (RdfParserSelectionException)
                    {
                        // If we get a Parser Selection exception then the Content Type isn't valid for a Sparql Result Set

                        // Is the Content Type referring to a RDF format?
                        IRdfReader rdfreader = MimeTypesHelper.GetParser(ctype);
                        rdfreader.Load(rdfHandler, data);
                        response.Close();
                    }
                }
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpQueryError(webEx);
            }
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <remarks>
        /// If an empty/null URI is specified then the Default Graph of the Store will be loaded
        /// </remarks>
        public virtual void LoadGraph(IGraph g, Uri graphUri)
        {
            LoadGraph(g, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <remarks>
        /// If an empty/null URI is specified then the Default Graph of the Store will be loaded
        /// </remarks>
        public virtual void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            LoadGraph(handler, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Uri of the Graph to load</param>
        /// <remarks>
        /// If an empty/null Uri is specified then the Default Graph of the Store will be loaded
        /// </remarks>
        public virtual void LoadGraph(IGraph g, string graphUri)
        {
            if (g.IsEmpty && graphUri != null && !graphUri.Equals(string.Empty))
            {
                g.BaseUri = UriFactory.Create(graphUri);
            }
            LoadGraph(new GraphHandler(g), graphUri);
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <remarks>
        /// If an empty/null URI is specified then the Default Graph of the Store will be loaded
        /// </remarks>
        public virtual void LoadGraph(IRdfHandler handler, string graphUri)
        {
            try
            {
                HttpWebRequest request;
                Dictionary<string, string> serviceParams = new Dictionary<string, string>();

                string tID = (_activeTrans == null) ? string.Empty : "/" + _activeTrans;
                string requestUri = _kb + tID + "/query";
                SparqlParameterizedString construct = new SparqlParameterizedString();
                if (!graphUri.Equals(string.Empty))
                {
                    construct.CommandText = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH @graph { ?s ?p ?o } }";
                    construct.SetUri("graph", UriFactory.Create(graphUri));
                }
                else
                {
                    construct.CommandText = "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }";
                }
                serviceParams.Add("query", construct.ToString());

                request = CreateRequest(requestUri, MimeTypesHelper.HttpAcceptHeader, "GET", serviceParams);

                Tools.HttpDebugRequest(request);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);

                    IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                    parser.Load(handler, new StreamReader(response.GetResponseStream()));
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpError(webEx, "loading a Graph from");
            }
        }

        /// <summary>
        /// Saves a Graph into the Store (see remarks for notes on merge/overwrite behaviour)
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <remarks>
        /// <para>
        /// If the Graph has no URI then the contents will be appended to the Store's Default Graph.  If the Graph has a URI then existing Graph associated with that URI will be replaced.  To append to a named Graph use the <see cref="BaseStardogConnector.UpdateGraph(Uri,System.Collections.Generic.IEnumerable{VDS.RDF.Triple},IEnumerable{Triple})">UpdateGraph()</see> method instead
        /// </para>
        /// </remarks>
        public virtual void SaveGraph(IGraph g)
        {
            string tID = null;
            try
            {
                // Have to do the delete first as that requires a separate transaction
                if (g.BaseUri != null)
                {
                    try
                    {
                        DeleteGraph(g.BaseUri);
                    }
                    catch (Exception ex)
                    {
                        throw new RdfStorageException(
                            "Unable to save a Named Graph to the Store as this requires deleting any existing Named Graph with this name which failed, see inner exception for more detail",
                            ex);
                    }
                }

                // Get a Transaction ID, if there is no active Transaction then this operation will be auto-committed
                tID = _activeTrans ?? BeginTransaction();

                HttpWebRequest request = CreateRequest(_kb + "/" + tID + "/add", MimeTypesHelper.Any, "POST",
                    new Dictionary<string, string>());
                request.ContentType = MimeTypesHelper.TriG[0];

                // Save the Data as TriG to the Request Stream
                TripleStore store = new TripleStore();
                store.Add(g);
                _writer.Save(store, new StreamWriter(request.GetRequestStream()));

                Tools.HttpDebugRequest(request);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);
                    // If we get here then it was OK
                    response.Close();
                }

                // Commit Transaction only if in auto-commit mode (active transaction will be null)
                if (_activeTrans != null) return;
                try
                {
                    CommitTransaction(tID);
                }
                catch (Exception ex)
                {
                    throw new RdfStorageException("Stardog failed to commit a Transaction", ex);
                }
            }
            catch (WebException webEx)
            {
                // Rollback Transaction only if got as far as creating a Transaction
                // and in auto-commit mode
                if (tID != null)
                {
                    if (_activeTrans == null)
                    {
                        try
                        {
                            RollbackTransaction(tID);
                        }
                        catch (Exception ex)
                        {
                            StorageHelper.HandleHttpError(webEx, "");
                            throw new RdfStorageException("Stardog failed to rollback a Transaction", ex);
                        }
                    }
                }

                throw StorageHelper.HandleHttpError(webEx, "saving a Graph to");
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
        public virtual void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            // If there are no adds or deletes, just return and avoid creating empty transaction
            bool anyData = (removals != null && removals.Any()) || (additions != null && additions.Any());
            if (!anyData) return;

            string tID = null;
            try
            {
                // Get a Transaction ID, if there is no active Transaction then this operation will be auto-committed
                tID = _activeTrans ?? BeginTransaction();

                // First do the Removals
                if (removals != null)
                {
                    if (removals.Any())
                    {
                        HttpWebRequest request = CreateRequest(_kb + "/" + tID + "/remove",
                            MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
                        request.ContentType = MimeTypesHelper.TriG[0];

                        // Save the Data to be removed as TriG to the Request Stream
                        TripleStore store = new TripleStore();
                        Graph g = new Graph();
                        g.Assert(removals);
                        g.BaseUri = graphUri;
                        store.Add(g);
                        _writer.Save(store, new StreamWriter(request.GetRequestStream()));

                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            response.Close();
                        }
                    }
                }

                // Then do the Additions
                if (additions != null)
                {
                    if (additions.Any())
                    {
                        HttpWebRequest request = CreateRequest(_kb + "/" + tID + "/add", MimeTypesHelper.Any,
                            "POST", new Dictionary<string, string>());
                        request.ContentType = MimeTypesHelper.TriG[0];

                        // Save the Data to be removed as TriG to the Request Stream
                        TripleStore store = new TripleStore();
                        Graph g = new Graph();
                        g.Assert(additions);
                        g.BaseUri = graphUri;
                        store.Add(g);
                        _writer.Save(store, new StreamWriter(request.GetRequestStream()));

                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            response.Close();
                        }
                    }
                }

                // Commit Transaction only if in auto-commit mode (active transaction will be null)
                if (_activeTrans != null) return;
                try
                {
                    CommitTransaction(tID);
                }
                catch (Exception ex)
                {
                    throw new RdfStorageException("Stardog failed to commit a Transaction", ex);
                }
            }
            catch (WebException webEx)
            {
                // Rollback Transaction only if got as far as creating a Transaction
                // and in auto-commit mode
                if (tID != null)
                {
                    if (_activeTrans == null)
                    {
                        try
                        {
                            RollbackTransaction(tID);
                        }
                        catch (Exception ex)
                        {
                            StorageHelper.HandleHttpError(webEx, "");
                            throw new RdfStorageException("Stardog failed to rollback a Transaction", ex);
                        }
                    }
                }

                throw StorageHelper.HandleHttpError(webEx, "updating a Graph in");
            }
        }

        /// <summary>
        /// Updates a Graph in the Stardog store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public virtual void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (graphUri == null || graphUri.Equals(string.Empty))
            {
                UpdateGraph((Uri)null, additions, removals);
            }
            else
            {
                UpdateGraph(UriFactory.Create(graphUri), additions, removals);
            }
        }

        /// <summary>
        /// Deletes a Graph from the Stardog store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public virtual void DeleteGraph(Uri graphUri)
        {
            DeleteGraph(graphUri.ToSafeString());
        }

        /// <summary>
        /// Deletes a Graph from the Stardog store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public virtual void DeleteGraph(string graphUri)
        {
            string tID = null;
            try
            {
                // Get a Transaction ID, if there is no active Transaction then this operation will be auto-committed
                tID = _activeTrans ?? BeginTransaction();

                var request = CreateRequest(
                    _kb + "/" + tID + "/clear/",
                    MimeTypesHelper.Any,
                    "POST",
                    new Dictionary<string, string>
                    {
                        {"graph-uri", graphUri.Equals(string.Empty) ? "DEFAULT" : graphUri},
                    }
                );

                request.ContentType = MimeTypesHelper.WWWFormURLEncoded;

                Tools.HttpDebugRequest(request);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);
                    // If we get here then the Delete worked OK
                    response.Close();
                }

                // Commit Transaction only if in auto-commit mode (active transaction will be null)
                if (_activeTrans != null) return;
                try
                {
                    CommitTransaction(tID);
                }
                catch (Exception ex)
                {
                    throw new RdfStorageException("Stardog failed to commit a Transaction", ex);
                }
            }
            catch (WebException webEx)
            {
                // Rollback Transaction only if got as far as creating a Transaction
                // and in auto-commit mode
                if (tID != null)
                {
                    if (_activeTrans == null)
                    {
                        try
                        {
                            RollbackTransaction(tID);
                        }
                        catch (Exception ex)
                        {
                            StorageHelper.HandleHttpError(webEx, "");
                            throw new RdfStorageException("Stardog failed to rollback a Transaction", ex);
                        }
                    }
                }

                throw StorageHelper.HandleHttpError(webEx, "deleting a Graph from");
            }
        }

        /// <summary>
        /// Gets the list of Graphs in the Stardog store
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<Uri> ListGraphs()
        {
            try
            {
                object results = Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }");
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
                throw StorageHelper.HandleError(ex, "listing Graphs from");
            }
        }

        /// <summary>
        /// Gets the parent server
        /// </summary>
        public override IAsyncStorageServer AsyncParentServer => Server;

        /// <summary>
        /// Saves a Graph to the Store asynchronously
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public override void SaveGraph(IGraph g, AsyncStorageCallback callback, object state)
        {
            SaveGraphAsync(g, callback, state);
        }

        /// <summary>
        /// Saves a Graph to the Store asynchronously
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        protected virtual void SaveGraphAsync(IGraph g, AsyncStorageCallback callback, object state)
        {
            // Get a Transaction ID, if there is no active Transaction then this operation will start a new transaction and be auto-committed
            if (_activeTrans != null)
            {
                SaveGraphAsync(_activeTrans, false, g, callback, state);
            }
            else
            {
                Begin((sender, args, st) =>
                {
                    if (args.WasSuccessful)
                    {
                        // Have to do the delete first as that requires a separate transaction
                        if (g.BaseUri != null)
                        {
                            DeleteGraph(g.BaseUri, (_1, delArgs, _2) =>
                            {
                                if (delArgs.WasSuccessful)
                                {
                                    SaveGraphAsync(_activeTrans, true, g, callback, state);
                                }
                                else
                                {
                                    callback(this,
                                        new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph,
                                            new RdfStorageException(
                                                "Unable to save a Named Graph to the Store as this requires deleted any existing Named Graph with this name which failed, see inner exception for more detail",
                                                delArgs.Error)), state);
                                }
                            }, state);
                        }
                        else
                        {
                            SaveGraphAsync(_activeTrans, true, g, callback, state);
                        }
                    }
                    else
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g, args.Error),
                            state);
                    }
                }, state);
            }
        }

        /// <summary>
        /// Save a graph to the database asynchronously within the context of an open transaction
        /// </summary>
        /// <param name="tID">The ID of the transaction to use for the update</param>
        /// <param name="autoCommit">True to commit the transaction on completion</param>
        /// <param name="g">The graph to write</param>
        /// <param name="callback">Callback invoked on completion</param>
        /// <param name="state">State parameter to pass to the callback</param>
        protected virtual void SaveGraphAsync(string tID, bool autoCommit, IGraph g, AsyncStorageCallback callback,
            object state)
        {
            try
            {
                HttpWebRequest request = CreateRequest(_kb + "/" + tID + "/add", MimeTypesHelper.Any, "POST",
                    new Dictionary<string, string>());
                request.ContentType = MimeTypesHelper.TriG[0];

                request.BeginGetRequestStream(r =>
                {
                    try
                    {
                        // Save the Data as TriG to the Request Stream
                        var stream = request.EndGetRequestStream(r);
                        var store = new TripleStore();
                        store.Add(g);
                        _writer.Save(store, new StreamWriter(stream));

                        Tools.HttpDebugRequest(request);
                        request.BeginGetResponse(r2 =>
                        {
                            try
                            {
                                var response = (HttpWebResponse)request.EndGetResponse(r2);
                                Tools.HttpDebugResponse(response);

                                // If we get here then it was OK
                                response.Close();

                                // Commit Transaction only if in auto-commit mode (active transaction will be null)
                                if (autoCommit)
                                {
                                    Commit((sender, args, st) =>
                                    {
                                        if (args.WasSuccessful)
                                        {
                                            callback(this,
                                                new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g), state);
                                        }
                                        else
                                        {
                                            callback(this,
                                                new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, args.Error),
                                                state);
                                        }
                                    }, state);
                                }
                                else
                                {
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g),
                                        state);
                                }
                            }
                            catch (WebException webEx)
                            {
                                if (autoCommit)
                                {
                                    // If something went wrong try to rollback, don't care what the rollback response is
                                    Rollback((sender, args, st) => { }, state);
                                }
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph,
                                        StorageHelper.HandleHttpError(webEx, "saving a Graph asynchronously to")), state);
                            }
                            catch (Exception ex)
                            {
                                if (autoCommit)
                                {
                                    // If something went wrong try to rollback, don't care what the rollback response is
                                    Rollback((sender, args, st) => { }, state);
                                }
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph,
                                        StorageHelper.HandleError(ex, "saving a Graph asynchronously to")), state);
                            }
                        }, state);
                    }
                    catch (WebException webEx)
                    {
                        if (autoCommit)
                        {
                            // If something went wrong try to rollback, don't care what the rollback response is
                            Rollback((sender, args, st) => { }, state);
                        }
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph,
                                StorageHelper.HandleHttpError(webEx, "saving a Graph asynchronously to")), state);
                    }
                    catch (Exception ex)
                    {
                        if (autoCommit)
                        {
                            // If something went wrong try to rollback, don't care what the rollback response is
                            Rollback((sender, args, st) => { }, state);
                        }
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph,
                                StorageHelper.HandleError(ex, "saving a Graph asynchronously to")), state);
                    }
                }, state);
            }
            catch (WebException webEx)
            {
                if (autoCommit)
                {
                    // If something went wrong try to rollback, don't care what the rollback response is
                    Rollback((sender, args, st) => { }, state);
                }
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph,
                        StorageHelper.HandleHttpError(webEx, "saving a Graph asynchronously to")), state);
            }
            catch (Exception ex)
            {
                if (autoCommit)
                {
                    // If something went wrong try to rollback, don't care what the rollback response is
                    Rollback((sender, args, st) => { }, state);
                }
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph,
                        StorageHelper.HandleError(ex, "saving a Graph asynchronously to")), state);
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
                Dictionary<string, string> serviceParams = new Dictionary<string, string>();

                string tID = (_activeTrans == null) ? string.Empty : "/" + _activeTrans;
                string requestUri = _kb + tID + "/query";
                SparqlParameterizedString construct = new SparqlParameterizedString();
                if (!graphUri.Equals(string.Empty))
                {
                    construct.CommandText = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH @graph { ?s ?p ?o } }";
                    construct.SetUri("graph", UriFactory.Create(graphUri));
                }
                else
                {
                    construct.CommandText = "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }";
                }
                serviceParams.Add("query", construct.ToString());

                request = CreateRequest(requestUri, MimeTypesHelper.HttpAcceptHeader, "GET", serviceParams);

                Tools.HttpDebugRequest(request);

                request.BeginGetResponse(r =>
                {
                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);
                        Tools.HttpDebugResponse(response);

                        IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                        parser.Load(handler, new StreamReader(response.GetResponseStream()));
                        response.Close();

                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler),
                            state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler,
                                StorageHelper.HandleHttpError(webEx, "loading a Graph from")), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler,
                                StorageHelper.HandleError(ex, "loading a Graph from")), state);
                    }
                }, state);
            }
            catch (WebException webEx)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler,
                        StorageHelper.HandleHttpError(webEx, "loading a Graph from")), state);
            }
            catch (Exception ex)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler,
                        StorageHelper.HandleError(ex, "loading a Graph from")), state);
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
        public override void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals,
            AsyncStorageCallback callback, object state)
        {
            // If there are no adds or deletes, just callback and avoid creating empty transaction
            bool anyData = removals != null && removals.Any();
            if (additions != null && additions.Any()) anyData = true;
            if (!anyData)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()),
                    state);
            }
            else
            {
                UpdateGraphAsync(graphUri, additions, removals, callback, state);
            }
        }

        /// <summary>
        /// Apply an update to a graph
        /// </summary>
        /// <param name="graphUri">The URI of the graph to be updated</param>
        /// <param name="additions">The triples to insert</param>
        /// <param name="removals">The triples to delete</param>
        /// <param name="callback">Callback invoked on completion</param>
        /// <param name="state">Additional state passed to the callback</param>
        /// <remarks>If a transaction is currently in progress, the update is applied
        /// as part of that transaction. Otherwise a new transaction is started and committed by this method.</remarks>
        protected virtual void UpdateGraphAsync(string graphUri, IEnumerable<Triple> additions,
            IEnumerable<Triple> removals, AsyncStorageCallback callback, object state)
        {
            // Get a Transaction ID, if there is no active Transaction then this operation will start a new transaction and be auto-committed
            if (_activeTrans != null)
            {
                UpdateGraphAsync(_activeTrans, false, graphUri, additions, removals, callback, state);
            }
            else
            {
                Begin((sender, args, st) =>
                {
                    if (args.WasSuccessful)
                    {
                        UpdateGraphAsync(_activeTrans, true, graphUri, additions, removals, callback, state);
                    }
                    else
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(),
                                args.Error), state);
                    }
                }, state);
            }
        }

        /// <summary>
        /// Apply an update to a graph as part of a transaction
        /// </summary>
        /// <param name="tID">The ID of the open transaction to use</param>
        /// <param name="autoCommit">True to commit the transaction at the end of the update, false otherwise</param>
        /// <param name="graphUri">The URI of the graph to be updated</param>
        /// <param name="additions">The triples to inser</param>
        /// <param name="removals">The triples to remove</param>
        /// <param name="callback">A callback to be invoked on completion</param>
        /// <param name="state">Additional state to pass to the callback</param>
        protected virtual void UpdateGraphAsync(string tID, bool autoCommit, string graphUri,
            IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, object state)
        {
            try
            {
                if (removals != null && removals.Any())
                {
                    HttpWebRequest request = CreateRequest(_kb + "/" + tID + "/remove", MimeTypesHelper.Any,
                        "POST", new Dictionary<string, string>());
                    request.ContentType = MimeTypesHelper.TriG[0];

                    request.BeginGetRequestStream(r =>
                    {
                        try
                        {
                            // Save the Data as TriG to the Request Stream
                            var stream = request.EndGetRequestStream(r);
                            var store = new TripleStore();
                            var g = new Graph { BaseUri = graphUri.ToSafeUri() };
                            g.Assert(removals);
                            store.Add(g);
                            _writer.Save(store, new StreamWriter(stream));

                            Tools.HttpDebugRequest(request);
                            request.BeginGetResponse(r2 =>
                            {
                                try
                                {
                                    HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r2);
                                    Tools.HttpDebugResponse(response);

                                    // If we get here then it was OK
                                    response.Close();

                                    if (additions != null && additions.Any())
                                    {
                                        // Now we need to do additions
                                        request = CreateRequest(_kb + "/" + tID + "/add", MimeTypesHelper.Any,
                                            "POST", new Dictionary<string, string>());
                                        request.ContentType = MimeTypesHelper.TriG[0];

                                        request.BeginGetRequestStream(r3 =>
                                        {
                                            try
                                            {
                                                // Save the Data as TriG to the Request Stream
                                                stream = request.EndGetRequestStream(r3);
                                                store = new TripleStore();
                                                g = new Graph { BaseUri = graphUri.ToSafeUri() };
                                                g.Assert(additions);
                                                store.Add(g);
                                                _writer.Save(store, new StreamWriter(stream));

                                                Tools.HttpDebugRequest(request);

                                                request.BeginGetResponse(r4 =>
                                                {
                                                    try
                                                    {
                                                        response = (HttpWebResponse)request.EndGetResponse(r4);
                                                        Tools.HttpDebugResponse(response);

                                                        // If we get here then it was OK
                                                        response.Close();

                                                        // Commit Transaction only if in auto-commit mode (active transaction will be null)
                                                        if (autoCommit)
                                                        {
                                                            Commit((sender, args, st) =>
                                                            {
                                                                if (args.WasSuccessful)
                                                                {
                                                                    callback(this,
                                                                        new AsyncStorageCallbackArgs(
                                                                            AsyncStorageOperation.UpdateGraph,
                                                                            graphUri.ToSafeUri()), state);
                                                                }
                                                                else
                                                                {
                                                                    callback(this,
                                                                        new AsyncStorageCallbackArgs(
                                                                            AsyncStorageOperation.UpdateGraph,
                                                                            graphUri.ToSafeUri(), args.Error), state);
                                                                }
                                                            }, state);
                                                        }
                                                        else
                                                        {
                                                            callback(this,
                                                                new AsyncStorageCallbackArgs(
                                                                    AsyncStorageOperation.UpdateGraph,
                                                                    graphUri.ToSafeUri()), state);
                                                        }
                                                    }
                                                    catch (WebException webEx)
                                                    {
                                                        if (autoCommit)
                                                        {
                                                            // If something went wrong try to rollback, don't care what the rollback response is
                                                            Rollback((sender, args, st) => { }, state);
                                                        }
                                                        callback(this,
                                                            new AsyncStorageCallbackArgs(
                                                                AsyncStorageOperation.UpdateGraph,
                                                                StorageHelper.HandleHttpError(webEx,
                                                                    "updating a Graph asynchronously in")), state);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        if (autoCommit)
                                                        {
                                                            // If something went wrong try to rollback, don't care what the rollback response is
                                                            Rollback((sender, args, st) => { }, state);
                                                        }
                                                        callback(this,
                                                            new AsyncStorageCallbackArgs(
                                                                AsyncStorageOperation.UpdateGraph,
                                                                StorageHelper.HandleError(ex,
                                                                    "updating a Graph asynchronously in")), state);
                                                    }
                                                }, state);
                                            }
                                            catch (WebException webEx)
                                            {
                                                if (autoCommit)
                                                {
                                                    // If something went wrong try to rollback, don't care what the rollback response is
                                                    Rollback((sender, args, st) => { }, state);
                                                }
                                                callback(this,
                                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                                        StorageHelper.HandleHttpError(webEx,
                                                            "updating a Graph asynchronously in")), state);
                                            }
                                            catch (Exception ex)
                                            {
                                                if (autoCommit)
                                                {
                                                    // If something went wrong try to rollback, don't care what the rollback response is
                                                    Rollback((sender, args, st) => { }, state);
                                                }
                                                callback(this,
                                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                                        StorageHelper.HandleError(ex,
                                                            "updating a Graph asynchronously in")), state);
                                            }
                                        }, state);
                                    }
                                    else
                                    {
                                        // No additions to do
                                        // Commit Transaction only if in auto-commit mode (active transaction will be null)
                                        if (autoCommit)
                                        {
                                            Commit((sender, args, st) =>
                                            {
                                                if (args.WasSuccessful)
                                                {
                                                    callback(this,
                                                        new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                                            graphUri.ToSafeUri()), state);
                                                }
                                                else
                                                {
                                                    callback(this,
                                                        new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                                            graphUri.ToSafeUri(), args.Error), state);
                                                }
                                            }, state);
                                        }
                                        else
                                        {
                                            callback(this,
                                                new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                                    graphUri.ToSafeUri()), state);
                                        }
                                    }
                                }
                                catch (WebException webEx)
                                {
                                    if (autoCommit)
                                    {
                                        // If something went wrong try to rollback, don't care what the rollback response is
                                        Rollback((sender, args, st) => { }, state);
                                    }
                                    callback(this,
                                        new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                            StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")),
                                        state);
                                }
                                catch (Exception ex)
                                {
                                    if (autoCommit)
                                    {
                                        // If something went wrong try to rollback, don't care what the rollback response is
                                        Rollback((sender, args, st) => { }, state);
                                    }
                                    callback(this,
                                        new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                            StorageHelper.HandleError(ex, "updating a Graph asynchronously in")), state);
                                }
                            }, state);
                        }
                        catch (WebException webEx)
                        {
                            if (autoCommit)
                            {
                                // If something went wrong try to rollback, don't care what the rollback response is
                                Rollback((sender, args, st) => { }, state);
                            }
                            callback(this,
                                new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                    StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")), state);
                        }
                        catch (Exception ex)
                        {
                            if (autoCommit)
                            {
                                // If something went wrong try to rollback, don't care what the rollback response is
                                Rollback((sender, args, st) => { }, state);
                            }
                            callback(this,
                                new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                    StorageHelper.HandleError(ex, "updating a Graph asynchronously in")), state);
                        }
                    }, state);
                }
                else if (additions != null && additions.Any())
                {
                    HttpWebRequest request = CreateRequest(_kb + "/" + tID + "/add", MimeTypesHelper.Any,
                        "POST", new Dictionary<string, string>());
                    request.ContentType = MimeTypesHelper.TriG[0];

                    request.BeginGetRequestStream(r =>
                    {
                        try
                        {
                            // Save the Data as TriG to the Request Stream
                            Stream stream = request.EndGetRequestStream(r);
                            TripleStore store = new TripleStore();
                            Graph g = new Graph();
                            g.Assert(additions);
                            g.BaseUri = graphUri.ToSafeUri();
                            store.Add(g);
                            _writer.Save(store, new StreamWriter(stream));

                            Tools.HttpDebugRequest(request);

                            request.BeginGetResponse(r2 =>
                            {
                                try
                                {
                                    HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r2);
                                    Tools.HttpDebugResponse(response);

                                    // If we get here then it was OK
                                    response.Close();

                                    // Commit Transaction only if in auto-commit mode (active transaction will be null)
                                    if (autoCommit)
                                    {
                                        Commit((sender, args, st) =>
                                        {
                                            if (args.WasSuccessful)
                                            {
                                                callback(this,
                                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                                        graphUri.ToSafeUri()), state);
                                            }
                                            else
                                            {
                                                callback(this,
                                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                                        graphUri.ToSafeUri(), args.Error), state);
                                            }
                                        }, state);
                                    }
                                    else
                                    {
                                        callback(this,
                                            new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                                graphUri.ToSafeUri()), state);
                                    }
                                }
                                catch (WebException webEx)
                                {
                                    if (autoCommit)
                                    {
                                        // If something went wrong try to rollback, don't care what the rollback response is
                                        Rollback((sender, args, st) => { }, state);
                                    }
                                    callback(this,
                                        new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                            StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")),
                                        state);
                                }
                                catch (Exception ex)
                                {
                                    if (autoCommit)
                                    {
                                        // If something went wrong try to rollback, don't care what the rollback response is
                                        Rollback((sender, args, st) => { }, state);
                                    }
                                    callback(this,
                                        new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                            StorageHelper.HandleError(ex, "updating a Graph asynchronously in")), state);
                                }
                            }, state);
                        }
                        catch (WebException webEx)
                        {
                            if (autoCommit)
                            {
                                // If something went wrong try to rollback, don't care what the rollback response is
                                Rollback((sender, args, st) => { }, state);
                            }
                            callback(this,
                                new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                    StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")), state);
                        }
                        catch (Exception ex)
                        {
                            if (autoCommit)
                            {
                                // If something went wrong try to rollback, don't care what the rollback response is
                                Rollback((sender, args, st) => { }, state);
                            }
                            callback(this,
                                new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                                    StorageHelper.HandleError(ex, "updating a Graph asynchronously in")), state);
                        }
                    }, state);
                }
                else
                {
                    // Nothing to do, just invoke callback
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()),
                        state);
                }
            }
            catch (WebException webEx)
            {
                if (autoCommit)
                {
                    // If something went wrong try to rollback, don't care what the rollback response is
                    Rollback((sender, args, st) => { }, state);
                }
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                        StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")), state);
            }
            catch (Exception ex)
            {
                if (autoCommit)
                {
                    // If something went wrong try to rollback, don't care what the rollback response is
                    Rollback((sender, args, st) => { }, state);
                }
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph,
                        StorageHelper.HandleError(ex, "updating a Graph asynchronously in")), state);
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
            // Get a Transaction ID, if there is no active Transaction then this operation will start a new transaction and be auto-committed
            if (_activeTrans != null)
            {
                DeleteGraphAsync(_activeTrans, false, graphUri, callback, state);
            }
            else
            {
                Begin((sender, args, st) =>
                {
                    if (args.WasSuccessful)
                    {
                        DeleteGraphAsync(_activeTrans, true, graphUri, callback, state);
                    }
                    else
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(),
                                args.Error), state);
                    }
                }, state);
            }
        }

        /// <summary>
        /// Delete a graph as part of an open transaction
        /// </summary>
        /// <param name="tID">The ID of the transaction to use</param>
        /// <param name="autoCommit">True to commit the transaction at the end of the delete operation, false to leave the transaction open</param>
        /// <param name="graphUri">The URI of the graph to delete</param>
        /// <param name="callback">Callback to invoked on completion of the operation</param>
        /// <param name="state">Additional state to pass into the callback</param>
        protected virtual void DeleteGraphAsync(string tID, bool autoCommit, string graphUri,
            AsyncStorageCallback callback, object state)
        {
            try
            {
                var request = CreateRequest(
                    _kb + "/" + tID + "/clear/",
                    MimeTypesHelper.Any,
                    "POST",
                    new Dictionary<string, string>()
                    {
                        {"graph-uri", graphUri.Equals(string.Empty) ? "DEFAULT" : graphUri},
                    }
                );

                request.ContentType = MimeTypesHelper.WWWFormURLEncoded;

                Tools.HttpDebugRequest(request);
                request.BeginGetResponse(r =>
                {
                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);

                        Tools.HttpDebugResponse(response);
                        // If we get here then the Delete worked OK
                        response.Close();

                        // Commit Transaction only if in auto-commit mode (active transaction will be null)
                        if (autoCommit)
                        {
                            Commit((sender, args, st) =>
                            {
                                if (args.WasSuccessful)
                                {
                                    callback(this,
                                        new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph,
                                            graphUri.ToSafeUri()), state);
                                }
                                else
                                {
                                    callback(this,
                                        new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph,
                                            graphUri.ToSafeUri(), args.Error), state);
                                }
                            }, state);
                        }
                        else
                        {
                            callback(this,
                                new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri()),
                                state);
                        }
                    }
                    catch (WebException webEx)
                    {
                        if (autoCommit)
                        {
                            // If something went wrong try to rollback, don't care what the rollback response is
                            Rollback((sender, args, st) => { }, state);
                        }
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(),
                                StorageHelper.HandleHttpError(webEx, "deleting a Graph asynchronously from")), state);
                    }
                    catch (Exception ex)
                    {
                        if (autoCommit)
                        {
                            // If something went wrong try to rollback, don't care what the rollback response is
                            Rollback((sender, args, st) => { }, state);
                        }
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(),
                                StorageHelper.HandleError(ex, "deleting a Graph asynchronously from")), state);
                    }
                }, state);
            }
            catch (WebException webEx)
            {
                if (autoCommit)
                {
                    // If something went wrong try to rollback, don't care what the rollback response is
                    Rollback((sender, args, st) => { }, state);
                }
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(),
                        StorageHelper.HandleHttpError(webEx, "deleting a Graph asynchronously from")), state);
            }
            catch (Exception ex)
            {
                if (autoCommit)
                {
                    // If something went wrong try to rollback, don't care what the rollback response is
                    Rollback((sender, args, st) => { }, state);
                }
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(),
                        StorageHelper.HandleError(ex, "deleting a Graph asynchronously from")), state);
            }
        }

        /// <summary>
        /// Queries the store asynchronously
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void Query(string query, AsyncStorageCallback callback, object state)
        {
            Graph g = new Graph();
            SparqlResultSet results = new SparqlResultSet();
            Query(new GraphHandler(g), new ResultSetHandler(results), query, (sender, args, st) =>
            {
                if (results.ResultsType != SparqlResultsType.Unknown)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQuery, query, results, args.Error),
                        state);
                }
                else
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQuery, query, g, args.Error),
                        state);
                }
            }, state);
        }

        /// <summary>
        /// Queries the store asynchronously
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string query,
            AsyncStorageCallback callback, object state)
        {
            try
            {
                HttpWebRequest request;

                string tID = (_activeTrans == null) ? string.Empty : "/" + _activeTrans;

                // String accept = MimeTypesHelper.HttpRdfOrSparqlAcceptHeader;
                string accept =
                    MimeTypesHelper.CustomHttpAcceptHeader(
                        MimeTypesHelper.SparqlResultsXml.Concat(
                            MimeTypesHelper.Definitions.Where(d => d.CanParseRdf).SelectMany(d => d.MimeTypes)));

                // Create the Request, for simplicity async requests are always POST
                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                request = CreateRequest(_kb + tID + "/query", accept, "POST", queryParams);

                // Build the Post Data and add to the Request Body
                request.ContentType = MimeTypesHelper.Utf8WWWFormURLEncoded;

                request.BeginGetRequestStream(r =>
                {
                    try
                    {
                        Stream stream = request.EndGetRequestStream(r);
                        using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(Options.UseBomForUtf8)))
                        {
                            writer.Write("query=");
                            writer.Write(HttpUtility.UrlEncode(query));
                            writer.Close();
                        }

                        Tools.HttpDebugRequest(request);

                        // Get the Response and process based on the Content Type
                        request.BeginGetResponse(r2 =>
                        {
                            try
                            {
                                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r2);
                                Tools.HttpDebugResponse(response);

                                StreamReader data = new StreamReader(response.GetResponseStream());
                                string ctype = response.ContentType;
                                try
                                {
                                    // Is the Content Type referring to a Sparql Result Set format?
                                    ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(ctype,
                                        Regex.IsMatch(query, "ASK", RegexOptions.IgnoreCase));
                                    resreader.Load(resultsHandler, data);
                                    response.Close();
                                }
                                catch (RdfParserSelectionException)
                                {
                                    // If we get a Parser Selection exception then the Content Type isn't valid for a Sparql Result Set

                                    // Is the Content Type referring to a RDF format?
                                    IRdfReader rdfreader = MimeTypesHelper.GetParser(ctype);
                                    rdfreader.Load(rdfHandler, data);
                                    response.Close();
                                }
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, query,
                                        rdfHandler, resultsHandler), state);
                            }
                            catch (WebException webEx)
                            {
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                                        StorageHelper.HandleHttpQueryError(webEx)), state);
                            }
                            catch (Exception ex)
                            {
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                                        StorageHelper.HandleQueryError(ex)), state);
                            }
                        }, state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                                StorageHelper.HandleHttpQueryError(webEx)), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                                StorageHelper.HandleQueryError(ex)), state);
                    }
                }, state);
            }
            catch (WebException webEx)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                        StorageHelper.HandleHttpQueryError(webEx)), state);
            }
            catch (Exception ex)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler,
                        StorageHelper.HandleQueryError(ex)), state);
            }
        }

        #region HTTP Helper Methods

        /// <summary>
        /// Helper method for creating HTTP Requests to the Store
        /// </summary>
        /// <param name="servicePath">Path to the Service requested</param>
        /// <param name="accept">Acceptable Content Types</param>
        /// <param name="method">HTTP Method</param>
        /// <param name="requestParams">Querystring Parameters</param>
        /// <returns></returns>
        protected virtual HttpWebRequest CreateRequest(string servicePath, string accept, string method,
            Dictionary<string, string> requestParams)
        {
            // Build the Request Uri
            string requestUri = _baseUri + servicePath + "?";

            if (!ReferenceEquals(requestParams, null) && requestParams.Count > 0)
            {
                foreach (string p in requestParams.Keys)
                {
                    requestUri += p + "=" + HttpUtility.UrlEncode(requestParams[p]) + "&";
                }
            }

            requestUri += GetReasoningParameter();

            // Create our Request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Accept = accept;
            request.Method = method;
            request = ApplyRequestOptions(request);

            // Add the special Stardog Headers
            AddStardogHeaders(request);

            // Add Credentials if needed
            if (_hasCredentials)
            {
                if (Options.ForceHttpBasicAuth)
                {
                    // Forcibly include a HTTP basic authentication header
#if !NETCORE
                    string credentials =
                        Convert.ToBase64String(Encoding.ASCII.GetBytes(_username + ":" + _pwd));
                    request.Headers.Add("Authorization", "Basic " + credentials);
#else
                    string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(this._username + ":" + this._pwd));
                    request.Headers["Authorization"] = "Basic " + credentials;
#endif
                }
                else
                {
                    // Leave .Net to cope with HTTP auth challenge response
                    NetworkCredential credentials = new NetworkCredential(_username, _pwd);
                    request.Credentials = credentials;
#if !NETCORE
                    request.PreAuthenticate = true;
#endif
                }
            }

            return request;
        }

        /// <summary>
        /// Adds Stardog specific request headers; reasoning needed for &lt; 2.2
        /// </summary>
        /// <param name="request"></param>
        protected virtual void AddStardogHeaders(HttpWebRequest request)
        {
#if !NETCORE
            request.Headers.Add("SD-Connection-String", "kb=" + _kb + ";" + GetReasoningParameter());
            // removed persist=sync, no longer needed in latest stardog versions?
            request.Headers.Add("SD-Protocol", "1.0");
#else
            request.Headers["SD-Connection-String"] = "kb=" + this._kb + ";" + this.GetReasoningParameter();
            request.Headers["SD-Protocol"] = "1.0";
#endif
        }

        /// <summary>
        /// Get the query parameter string that specifies the current reasoning mode
        /// </summary>
        /// <returns></returns>
        protected virtual string GetReasoningParameter()
        {
            switch (_reasoning)
            {
                case StardogReasoningMode.QL:
                    return "reasoning=QL";
                case StardogReasoningMode.EL:
                    return "reasoning=EL";
                case StardogReasoningMode.RL:
                    return "reasoning=RL";
                case StardogReasoningMode.DL:
                    return "reasoning=DL";
                case StardogReasoningMode.RDFS:
                    return "reasoning=RDFS";
                case StardogReasoningMode.SL:
                    throw new RdfStorageException(
                        "Stardog 1.* does not support the SL reasoning level, please ensure you are using a Stardog 2.* connector if you wish to use this reasoning level");
                default:
                    return string.Empty;
            }
        }

        #endregion

        #region Stardog Transaction Support

        /// <summary>
        /// Start a transaction
        /// </summary>
        /// <param name="enableReasoning">Opens the transaction with reasoning enabled (requires StarDog v5.3.0 or later).</param>
        /// <returns>A transaction ID for the new transaction</returns>
        protected virtual string BeginTransaction(bool enableReasoning = false)
        {
            string tID = null;

            var queryParams = new Dictionary<string, string>();
            if (enableReasoning) queryParams.Add("reasoning", "true");

            HttpWebRequest request = CreateRequest(_kb + "/transaction/begin", "text/plain"
                /*MimeTypesHelper.Any*/, "POST", queryParams);
            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
            try
            {
                Tools.HttpDebugRequest(request);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        Tools.HttpDebugResponse(response);

                        tID = reader.ReadToEnd();
                        reader.Close();
                    }
                    response.Close();
                }
            }
            catch (Exception ex)
            {
                throw StorageHelper.HandleError(ex, "beginning a Transaction in");
            }

            if (string.IsNullOrEmpty(tID))
            {
                throw new RdfStorageException("Stardog failed to begin a Transaction");
            }
            return tID;
        }

        /// <summary>
        /// Commit an open transaction
        /// </summary>
        /// <param name="tID">The ID of the transaction to commit</param>
        protected virtual void CommitTransaction(string tID)
        {
            HttpWebRequest request = CreateRequest(_kb + "/transaction/commit/" + tID, "text/plain"
                /* MimeTypesHelper.Any*/, "POST", new Dictionary<string, string>());
            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;

            Tools.HttpDebugRequest(request);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Tools.HttpDebugResponse(response);
                response.Close();
            }

            // Reset the Active Transaction on this Thread if the IDs match up
            if (_activeTrans != null && _activeTrans.Equals(tID))
            {
                _activeTrans = null;
            }
        }

        /// <summary>
        /// Rollback an open transaction
        /// </summary>
        /// <param name="tID">The ID of the transaction to rollback</param>
        protected virtual void RollbackTransaction(string tID)
        {
            var request = CreateRequest(_kb + "/transaction/rollback/" + tID, MimeTypesHelper.Any,
                "POST", new Dictionary<string, string>());
            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                response.Close();
            }

            // Reset the Active Transaction on this Thread if the IDs match up
            if (_activeTrans != null && _activeTrans.Equals(tID))
            {
                _activeTrans = null;
            }
        }

        /// <summary>
        /// Begins a new Transaction
        /// </summary>
        /// <remarks>
        /// A single transaction
        /// </remarks>
        public virtual void Begin()
        {
            Begin(false);
        }
        /// <summary>
        /// Begins a new Transaction
        /// </summary>
        /// <param name="enableReasoning">Opens the transaction with reasoning enabled.</param>
        /// <remarks>
        /// A single transaction
        /// </remarks>
        public virtual void Begin(bool enableReasoning)
        {
            try
            {
                Monitor.Enter(this);
                if (_activeTrans != null)
                {
                    throw new RdfStorageException(
                        "Cannot start a new Transaction as there is already an active Transaction");
                }
                _activeTrans = BeginTransaction(enableReasoning);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Commits the active Transaction
        /// </summary>
        /// <exception cref="RdfStorageException">Thrown if there is not an active Transaction on the current Thread</exception>
        /// <remarks>
        /// Transactions are scoped to Managed Threads
        /// </remarks>
        public virtual void Commit()
        {
            try
            {
                Monitor.Enter(this);
                if (_activeTrans == null)
                {
                    throw new RdfStorageException(
                        "Cannot commit a Transaction as there is currently no active Transaction");
                }
                CommitTransaction(_activeTrans);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Rolls back the active Transaction
        /// </summary>
        /// <exception cref="RdfStorageException">Thrown if there is not an active Transaction on the current Thread</exception>
        /// <remarks>
        /// Transactions are scoped to Managed Threads
        /// </remarks>
        public virtual void Rollback()
        {
            try
            {
                Monitor.Enter(this);
                if (_activeTrans == null)
                {
                    throw new RdfStorageException(
                        "Cannot rollback a Transaction on the as there is currently no active Transaction");
                }
                RollbackTransaction(_activeTrans);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

        /// <summary>
        /// Begins a transaction asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void Begin(AsyncStorageCallback callback, object state)
        {
            try
            {
                if (_activeTrans != null)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin,
                            new RdfStorageException(
                                "Cannot start a new Transaction as there is already an active Transaction")), state);
                }
                else
                {
                    HttpWebRequest request = CreateRequest(_kb + "/transaction/begin", "text/plain"
                        /*MimeTypesHelper.Any*/, "POST", new Dictionary<string, string>());
                    request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                    try
                    {
                        Tools.HttpDebugRequest(request);
                        request.BeginGetResponse(r =>
                        {
                            try
                            {
                                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);
                                string tID;
                                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                                {
                                    Tools.HttpDebugResponse(response);
                                    tID = reader.ReadToEnd();
                                    reader.Close();
                                }
                                response.Close();

                                if (string.IsNullOrEmpty(tID))
                                {
                                    callback(this,
                                        new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin,
                                            new RdfStorageException("Stardog failed to begin a transaction")), state);
                                }
                                else
                                {
                                    _activeTrans = tID;
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin),
                                        state);
                                }
                            }
                            catch (WebException webEx)
                            {
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin,
                                        StorageHelper.HandleHttpError(webEx, "beginning a Transaction in")), state);
                            }
                            catch (Exception ex)
                            {
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin,
                                        StorageHelper.HandleError(ex, "beginning a Transaction in")), state);
                            }
                        }, state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin,
                                StorageHelper.HandleHttpError(webEx, "beginning a Transaction in")), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin,
                                StorageHelper.HandleError(ex, "beginning a Transaction in")), state);
                    }
                }
            }
            catch (WebException webEx)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin,
                        StorageHelper.HandleHttpError(webEx, "beginning a Transaction in")), state);
            }
            catch (Exception ex)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin,
                        StorageHelper.HandleError(ex, "beginning a Transaction in")), state);
            }
        }

        /// <summary>
        /// Commits a transaction asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void Commit(AsyncStorageCallback callback, object state)
        {
            try
            {
                if (_activeTrans == null)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit,
                            new RdfStorageException(
                                "Cannot commit a Transaction as there is currently no active Transaction")), state);
                }
                else
                {
                    HttpWebRequest request = CreateRequest(_kb + "/transaction/commit/" + _activeTrans,
                        "text/plain" /* MimeTypesHelper.Any*/, "POST", new Dictionary<string, string>());
                    request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                    Tools.HttpDebugRequest(request);
                    try
                    {
                        request.BeginGetResponse(r =>
                        {
                            try
                            {
                                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);

                                Tools.HttpDebugResponse(response);
                                response.Close();
                                _activeTrans = null;
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit),
                                    state);
                            }
                            catch (WebException webEx)
                            {
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit,
                                        StorageHelper.HandleHttpError(webEx, "committing a Transaction to")), state);
                            }
                            catch (Exception ex)
                            {
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit,
                                        StorageHelper.HandleError(ex, "committing a Transaction to")), state);
                            }
                        }, state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit,
                                StorageHelper.HandleHttpError(webEx, "committing a Transaction to")), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit,
                                StorageHelper.HandleError(ex, "committing a Transaction to")), state);
                    }
                }
            }
            catch (WebException webEx)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit,
                        StorageHelper.HandleHttpError(webEx, "committing a Transaction to")), state);
            }
            catch (Exception ex)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit,
                        StorageHelper.HandleError(ex, "committing a Transaction to")), state);
            }
        }

        /// <summary>
        /// Rolls back a transaction asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void Rollback(AsyncStorageCallback callback, object state)
        {
            try
            {
                if (_activeTrans == null)
                {
                    callback(this,
                        new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback,
                            new RdfStorageException(
                                "Cannot rollback a Transaction on the as there is currently no active Transaction")),
                        state);
                }
                else
                {
                    HttpWebRequest request = CreateRequest(
                        _kb + "/transaction/rollback/" + _activeTrans, MimeTypesHelper.Any, "POST",
                        new Dictionary<string, string>());
                    request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                    try
                    {
                        request.BeginGetResponse(r =>
                        {
                            try
                            {
                                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);
                                response.Close();
                                _activeTrans = null;
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback),
                                    state);
                            }
                            catch (WebException webEx)
                            {
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback,
                                        StorageHelper.HandleHttpError(webEx, "rolling back a Transaction from")), state);
                            }
                            catch (Exception ex)
                            {
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback,
                                        StorageHelper.HandleError(ex, "rolling back a Transaction from")), state);
                            }
                        }, state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback,
                                StorageHelper.HandleHttpError(webEx, "rolling back a Transaction from")), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback,
                                StorageHelper.HandleError(ex, "rolling back a Transaction from")), state);
                    }
                }
            }
            catch (WebException webEx)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback,
                        StorageHelper.HandleHttpError(webEx, "rolling back a Transaction from")), state);
            }
            catch (Exception ex)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback,
                        StorageHelper.HandleError(ex, "rolling back a Transaction from")), state);
            }
        }

        #endregion

        /// <summary>
        /// Disposes of the Connector
        /// </summary>
        public override void Dispose()
        {
            // No Dispose actions
        }

        /// <summary>
        /// Gets a String which gives details of the Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string mode = string.Empty;
            switch (_reasoning)
            {
                case StardogReasoningMode.QL:
                    mode = " (OWL QL Reasoning)";
                    break;
                case StardogReasoningMode.EL:
                    mode = " (OWL EL Reasoning)";
                    break;
                case StardogReasoningMode.RL:
                    mode = " (OWL RL Reasoning)";
                    break;
                case StardogReasoningMode.DL:
                    mode = " (OWL DL Reasoning)";
                    break;
                case StardogReasoningMode.RDFS:
                    mode = " (RDFS Reasoning)";
                    break;
                case StardogReasoningMode.SL:
                    mode = " (SL Reasoning)";
                    break;
            }
            return "[Stardog] Knowledge Base '" + _kb + "' on Server '" + _baseUri + "'" + mode;
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
            INode dnrType = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType));
            INode genericManager =
                context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassStorageProvider));
            INode server = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyServer));
            INode store = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyStore));
            INode loadMode = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyLoadMode));

            // Add Core config
            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(GetType().FullName)));
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(_baseUri)));
            context.Graph.Assert(new Triple(manager, store, context.Graph.CreateLiteralNode(_kb)));

            // Add reasoning mode
            if (_reasoning != StardogReasoningMode.None)
                context.Graph.Assert(new Triple(manager, loadMode,
                    context.Graph.CreateLiteralNode(_reasoning.ToString())));

            // Add User Credentials
            if (_username != null && _pwd != null)
            {
                INode username = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUser));
                INode pwd = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyPassword));
                context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(_username)));
                context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(_pwd)));
            }

            SerializeStandardConfig(manager, context);
        }
    }

    /// <summary>
    /// A Stardog Connector for connecting to Stardog version 1.* servers
    /// </summary>
    public class StardogV1Connector
        : BaseStardogConnector
    {
        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="reasoning">Reasoning Mode</param>
        public StardogV1Connector(string baseUri, string kbID, StardogReasoningMode reasoning)
            : this(baseUri, kbID, reasoning, null, null)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        public StardogV1Connector(string baseUri, string kbID)
            : this(baseUri, kbID, StardogReasoningMode.None)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public StardogV1Connector(string baseUri, string kbID, string username, string password)
            : this(baseUri, kbID, StardogReasoningMode.None, username, password)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="reasoning">Reasoning Mode</param>
        public StardogV1Connector(string baseUri, string kbID, StardogReasoningMode reasoning, string username,
            string password)
            : base(baseUri, kbID, reasoning, username, password)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="reasoning">Reasoning Mode</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogV1Connector(string baseUri, string kbID, StardogReasoningMode reasoning, IWebProxy proxy)
            : this(baseUri, kbID, reasoning, null, null, proxy)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="reasoning">Reasoning Mode</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogV1Connector(string baseUri, string kbID, StardogReasoningMode reasoning, string username,
            string password, IWebProxy proxy)
            : base(baseUri, kbID, reasoning, username, password, proxy)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogV1Connector(string baseUri, string kbID, IWebProxy proxy)
            : this(baseUri, kbID, StardogReasoningMode.None, proxy)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogV1Connector(string baseUri, string kbID, string username, string password, IWebProxy proxy)
            : this(baseUri, kbID, StardogReasoningMode.None, username, password, proxy)
        {
        }
    }

    /// <summary>
    /// A Stardog Connector for connecting to Stardog version 2.* servers
    /// </summary>
    public class StardogV2Connector
        : StardogV1Connector, IUpdateableStorage, IAsyncUpdateableStorage
    {
        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="reasoning">Reasoning Mode</param>
        public StardogV2Connector(string baseUri, string kbID, StardogReasoningMode reasoning)
            : this(baseUri, kbID, reasoning, null, null)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        public StardogV2Connector(string baseUri, string kbID)
            : this(baseUri, kbID, StardogReasoningMode.None)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public StardogV2Connector(string baseUri, string kbID, string username, string password)
            : this(baseUri, kbID, StardogReasoningMode.None, username, password)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="reasoning">Reasoning Mode</param>
        public StardogV2Connector(string baseUri, string kbID, StardogReasoningMode reasoning, string username,
            string password)
            : base(baseUri, kbID, reasoning, username, password)
        {
            Server = new StardogV2Server(baseUri, username, password);
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="reasoning">Reasoning Mode</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogV2Connector(string baseUri, string kbID, StardogReasoningMode reasoning, IWebProxy proxy)
            : this(baseUri, kbID, reasoning, null, null, proxy)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="reasoning">Reasoning Mode</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogV2Connector(string baseUri, string kbID, StardogReasoningMode reasoning, string username,
            string password, IWebProxy proxy)
            : base(baseUri, kbID, reasoning, username, password, proxy)
        {
            Server = new StardogV2Server(baseUri, username, password, proxy);
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogV2Connector(string baseUri, string kbID, IWebProxy proxy)
            : this(baseUri, kbID, StardogReasoningMode.None, proxy)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogV2Connector(string baseUri, string kbID, string username, string password, IWebProxy proxy)
            : this(baseUri, kbID, StardogReasoningMode.None, username, password, proxy)
        {
        }

        /// <summary>
        /// Adds Stardog specific request headers
        /// </summary>
        /// <param name="request"></param>
        protected override void AddStardogHeaders(HttpWebRequest request)
        {
            string reasoning = GetReasoningParameter();
#if !NETCORE
            request.Headers.Add("SD-Connection-String", reasoning);
            // Only reasoning parameter needed in Stardog 2.0, but < 2.2
#else
            request.Headers["SD-Connection-String"] = reasoning;
#endif
        }

        /// <summary>
        /// Get the query string parameter that specifies the current reasoning mode
        /// </summary>
        /// <returns></returns>
        protected override string GetReasoningParameter()
        {
            switch (Reasoning)
            {
                case StardogReasoningMode.QL:
                    return "reasoning=QL";
                case StardogReasoningMode.EL:
                    return "reasoning=EL";
                case StardogReasoningMode.RL:
                    return "reasoning=RL";
                case StardogReasoningMode.DL:
                    return "reasoning=DL";
                case StardogReasoningMode.RDFS:
                    return "reasoning=RDFS";
                case StardogReasoningMode.SL:
                    return "reasoning=SL";
                case StardogReasoningMode.None:
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Executes a SPARQL Update against the Stardog store
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update</param>
        /// <remarks>
        /// Stardog executes SPARQL update requests in their own self contained transactions which do not interact with normal Stardog transactions that may be managed via this API.  In some cases this can lead to unexpected behaviour, for example if you call <see cref="BaseStardogConnector.Begin()"/>, make an update and then call <see cref="BaseStardogConnector.Rollback()"/> the updates will not be rolled back.
        /// </remarks>
        public void Update(string sparqlUpdate)
        {
            try
            {
                // NB - Updates don't run inside a transaction rather they use their own self-contained transaction

                // Create the Request
                HttpWebRequest request = CreateRequest(KbId + "/update", MimeTypesHelper.Any, "POST", null);

                // Build the Post Data and add to the Request Body
                request.ContentType = MimeTypesHelper.SparqlUpdate;
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), new UTF8Encoding()))
                {
                    writer.Write(sparqlUpdate);
                    writer.Close();
                }

                Tools.HttpDebugRequest(request);

                // Check the response
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);

                    // If we got here then the update succeeded
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpError(webEx, "executing a SPARQL update against");
            }
        }

        /// <summary>
        /// Executes a SPARQL Update against the Stardog store
        /// </summary>
        /// <param name="sparqlUpdates">SPARQL Update</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to callback</param>
        /// <remarks>
        /// Stardog executes SPARQL update requests in their own self contained transactions which do not interact with normal Stardog transactions that may be managed via this API.  In some cases this can lead to unexpected behaviour, for example if you call <see cref="BaseStardogConnector.Begin(AsyncStorageCallback, Object)"/>, make an update and then call <see cref="BaseStardogConnector.Rollback(AsyncStorageCallback, Object)"/> the updates will not be rolled back.
        /// </remarks>
        public void Update(string sparqlUpdates, AsyncStorageCallback callback, object state)
        {
            try
            {
                // NB - Updates don't run inside a transaction rather they use their own self-contained transaction

                // Create the Request, for simplicity async requests are always POST
                HttpWebRequest request = CreateRequest(KbId + "/update", MimeTypesHelper.Any, "POST", null);

                // Create the request body
                request.ContentType = MimeTypesHelper.SparqlUpdate;

                request.BeginGetRequestStream(r =>
                {
                    try
                    {
                        Stream stream = request.EndGetRequestStream(r);
                        using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(Options.UseBomForUtf8)))
                        {
                            writer.Write(sparqlUpdates);
                            writer.Close();
                        }

                        Tools.HttpDebugRequest(request);

                        // Get the Response and process based on the Content Type
                        request.BeginGetResponse(r2 =>
                        {
                            try
                            {
                                using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r2))
                                {
                                    Tools.HttpDebugResponse(response);
                                    // If we get here the update completed OK
                                    response.Close();
                                }

                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdates),
                                    state);
                            }
                            catch (WebException webEx)
                            {
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate,
                                        StorageHelper.HandleHttpError(webEx, "executing a SPARQL update against")),
                                    state);
                            }
                            catch (Exception ex)
                            {
                                callback(this,
                                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate,
                                        StorageHelper.HandleError(ex, "executing a SPARQL update against")), state);
                            }
                        }, state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate,
                                StorageHelper.HandleHttpError(webEx, "executing a SPARQL update against")), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this,
                            new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate,
                                StorageHelper.HandleError(ex, "executing a SPARQL update against")), state);
                    }
                }, state);
            }
            catch (WebException webEx)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate,
                        StorageHelper.HandleHttpError(webEx, "executing a SPARQL update against")), state);
            }
            catch (Exception ex)
            {
                callback(this,
                    new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate,
                        StorageHelper.HandleError(ex, "executing a SPARQL update against")), state);
            }
        }
    }

    /// <summary>
    /// A Stardog Connector for connecting to Stardog version 3.* servers
    /// </summary>
    public class StardogV3Connector
        : StardogV2Connector,
            IUpdateableStorage,
            IAsyncUpdateableStorage
    {
        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        public StardogV3Connector(string baseUri, string kbID)
            : this(baseUri, kbID, null, null)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public StardogV3Connector(string baseUri, string kbID, string username, string password)
            : base(baseUri, kbID, StardogReasoningMode.DatabaseControlled, username, password)
        {
            Server = new StardogV2Server(baseUri, username, password);
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogV3Connector(string baseUri, string kbID, IWebProxy proxy)
            : this(baseUri, kbID, null, null, proxy)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogV3Connector(string baseUri, string kbID, string username, string password, IWebProxy proxy)
            : base(baseUri, kbID, StardogReasoningMode.DatabaseControlled, username, password, proxy)
        {
            Server = new StardogV2Server(baseUri, username, password, proxy);
        }

        /// <inheritdoc />
        public override StardogReasoningMode Reasoning
        {
            get => StardogReasoningMode.DatabaseControlled;
            set => throw new RdfStorageException(
                "Stardog 3.x does not support configuring reasoning mode at the connection level, reasoning is instead controlled at the database level ");
        }

        /// <summary>
        /// Adds Stardog specific request headers
        /// </summary>
        /// <param name="request"></param>
        protected override void AddStardogHeaders(HttpWebRequest request)
        {
            // No special headers needed for V3
        }
    }

    /// <summary>
    /// A Stardog connector for connecting to Stardog servers running the latest version, currently this is version 3.*
    /// </summary>
    public class StardogConnector
        : StardogV3Connector
    {
        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        public StardogConnector(string baseUri, string kbID)
            : this(baseUri, kbID, null, null)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        public StardogConnector(string baseUri, string kbID, string username, string password)
            : base(baseUri, kbID, username, password)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogConnector(string baseUri, string kbID, IWebProxy proxy)
            : this(baseUri, kbID, null, null, proxy)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogConnector(string baseUri, string kbID, string username,
            string password, IWebProxy proxy)
            : base(baseUri, kbID, username, password, proxy)
        {
        }

    }
}