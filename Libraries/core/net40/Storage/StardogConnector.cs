/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
#if !NO_WEB
using System.Web;
#endif
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage.Management;
using VDS.RDF.Writing;

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
        SL
    }

    /// <summary>
    /// Abstract implementation of a connector for Stardog that connects using the HTTP protocol
    /// </summary>
    /// <remarks>
    /// <para>
    /// Has full support for Stardog Transactions, connection is in auto-commit mode by default i.e. all write operations (Delete/Save/Update) will create and use a dedicated transaction for their operation, if the operation fails the transaction will automatically be rolled back.  You can manage Transactions using the <see cref="StardogConnector.Begin()">Begin()</see>, <see cref="StardogConnector.Commit()">Commit()</see> and <see cref="StardogConnector.Rollback()">Rollback()</see> methods.
    /// </para>
    /// <para>
    /// The connector maintains a single transaction which is shared across all threads since Stardog is currently provides only MRSW (Multiple Reader Single Writer) concurrency and does not permit multiple transactions to occur simultaneously.  
    /// </para>
    /// </remarks>
    public abstract class BaseStardogConnector
        : BaseAsyncHttpConnector, IAsyncQueryableStorage, IAsyncTransactionalStorage, IConfigurationSerializable
#if !NO_SYNC_HTTP
          , IQueryableStorage, ITransactionalStorage
#endif
    {
        /// <summary>
        /// Constant for the default Anonymous user account and password used by Stardog if you have not supplied a shiro.ini file or otherwise disabled security
        /// </summary>
        public const String AnonymousUser = "anonymous";

        protected String _baseUri, _kb, _username, _pwd;
        protected bool _hasCredentials = false;
        protected StardogReasoningMode _reasoning = StardogReasoningMode.None;

        protected String _activeTrans = null;
        protected TriGWriter _writer = new TriGWriter();
        protected BaseStardogServer _server;

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="reasoning">Reasoning Mode</param>
        public BaseStardogConnector(String baseUri, String kbID, StardogReasoningMode reasoning)
            : this(baseUri, kbID, reasoning, null, null)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        public BaseStardogConnector(String baseUri, String kbID)
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
        public BaseStardogConnector(String baseUri, String kbID, String username, String password)
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
        public BaseStardogConnector(String baseUri, String kbID, StardogReasoningMode reasoning, String username, String password)
            : base()
        {
            this._baseUri = baseUri;
            if (!this._baseUri.EndsWith("/")) this._baseUri += "/";
            this._kb = kbID;
            this._reasoning = reasoning;

            //Prep the writer
            this._writer.HighSpeedModePermitted = true;
            this._writer.CompressionLevel = WriterCompressionLevel.None;
            this._writer.UseMultiThreadedWriting = false;

            this._username = username;
            this._pwd = password;
            this._hasCredentials = (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password));

            //Server reference
            this._server = new StardogV1Server(this._baseUri, this._username, this._pwd);
        }

#if !NO_PROXY

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="reasoning">Reasoning Mode</param>
        /// <param name="proxy">Proxy Server</param>
        public BaseStardogConnector(String baseUri, String kbID, StardogReasoningMode reasoning, WebProxy proxy)
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
        public BaseStardogConnector(String baseUri, String kbID, StardogReasoningMode reasoning, String username, String password, WebProxy proxy)
            : this(baseUri, kbID, reasoning, username, password)
        {
            this.Proxy = proxy;
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="proxy">Proxy Server</param>
        public BaseStardogConnector(String baseUri, String kbID, WebProxy proxy)
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
        public BaseStardogConnector(String baseUri, String kbID, String username, String password, WebProxy proxy)
            : this(baseUri, kbID, StardogReasoningMode.None, username, password, proxy)
        {
        }

#endif

        /// <summary>
        /// Gets the Base URI of the Stardog server
        /// </summary>
        public String BaseUri
        {
            get { return this._baseUri; }
        }

        /// <summary>
        /// Gets/Sets the reasoning mode to use for queries
        /// </summary>
        [Description("What reasoning mode (if any) is currently in use for SPARQL Queries")]
        public StardogReasoningMode Reasoning
        {
            get { return this._reasoning; }
            set { this._reasoning = value; }
        }

        /// <summary>
        /// Gets the IO Behaviour of Stardog
        /// </summary>
        public override IOBehaviour IOBehaviour
        {
            get { return IOBehaviour.GraphStore | IOBehaviour.CanUpdateTriples; }
        }

        /// <summary>
        /// Returns that listing Graphs is supported
        /// </summary>
        public override bool ListGraphsSupported
        {
            get { return true; }
        }

        /// <summary>
        /// Returns that the Connection is ready
        /// </summary>
        public override bool IsReady
        {
            get { return true; }
        }

        /// <summary>
        /// Returns that the Connection is not read-only
        /// </summary>
        public override bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Returns that Updates are supported on Stardog Stores
        /// </summary>
        public override bool UpdateSupported
        {
            get { return true; }
        }

        /// <summary>
        /// Returns that deleting graphs from the Stardog store is not yet supported (due to a .Net specific issue)
        /// </summary>
        public override bool DeleteSupported
        {
            get { return true; }
        }

#if !NO_SYNC_HTTP

        /// <summary>
        /// Gets the parent server
        /// </summary>
        public override IStorageServer ParentServer
        {
            get { return this._server; }
        }

        /// <summary>
        /// Makes a SPARQL Query against the underlying Store using whatever reasoning mode is currently in-use
        /// </summary>
        /// <param name="sparqlQuery">Sparql Query</param>
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
        /// Makes a SPARQL Query against the underlying Store using whatever reasoning mode is currently in-use processing the results using an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public virtual void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery)
        {
            try
            {
                HttpWebRequest request;

                String tID = (this._activeTrans == null) ? String.Empty : "/" + this._activeTrans;

                //String accept = MimeTypesHelper.HttpRdfOrSparqlAcceptHeader;
                String accept = MimeTypesHelper.CustomHttpAcceptHeader(MimeTypesHelper.SparqlResultsXml.Concat(MimeTypesHelper.Definitions.Where(d => d.CanParseRdf).SelectMany(d => d.MimeTypes)));

                //Create the Request
                Dictionary<String, String> queryParams = new Dictionary<string, string>();
                if (sparqlQuery.Length < 2048)
                {
                    queryParams.Add("query", sparqlQuery);

                    request = this.CreateRequest(this._kb + tID + "/query", accept, "GET", queryParams);
                }
                else
                {
                    request = this.CreateRequest(this._kb + tID + "/query", accept, "POST", queryParams);

                    //Build the Post Data and add to the Request Body
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

                //Get the Response and process based on the Content Type
                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);

                    StreamReader data = new StreamReader(response.GetResponseStream());
                    String ctype = response.ContentType;
                    try
                    {
                        //Is the Content Type referring to a Sparql Result Set format?
                        ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(ctype, Regex.IsMatch(sparqlQuery, "ASK", RegexOptions.IgnoreCase));
                        resreader.Load(resultsHandler, data);
                        response.Close();
                    }
                    catch (RdfParserSelectionException)
                    {
                        //If we get a Parser Selection exception then the Content Type isn't valid for a Sparql Result Set

                        //Is the Content Type referring to a RDF format?
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
            this.LoadGraph(g, graphUri.ToSafeString());
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
            this.LoadGraph(handler, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Uri of the Graph to load</param>
        /// <remarks>
        /// If an empty/null Uri is specified then the Default Graph of the Store will be loaded
        /// </remarks>
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
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <remarks>
        /// If an empty/null URI is specified then the Default Graph of the Store will be loaded
        /// </remarks>
        public virtual void LoadGraph(IRdfHandler handler, String graphUri)
        {
            try
            {
                HttpWebRequest request;
                Dictionary<String, String> serviceParams = new Dictionary<string, string>();

                String tID = (this._activeTrans == null) ? String.Empty : "/" + this._activeTrans;
                String requestUri = this._kb + tID + "/query";
                SparqlParameterizedString construct = new SparqlParameterizedString();
                if (!graphUri.Equals(String.Empty))
                {
                    construct.CommandText = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH @graph { ?s ?p ?o } }";
                    construct.SetUri("graph", UriFactory.Create(graphUri));
                }
                else
                {
                    construct.CommandText = "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }";
                }
                serviceParams.Add("query", construct.ToString());

                request = this.CreateRequest(requestUri, MimeTypesHelper.HttpAcceptHeader, "GET", serviceParams);

                Tools.HttpDebugRequest(request);

                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
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
        /// If the Graph has no URI then the contents will be appended to the Store's Default Graph.  If the Graph has a URI then existing Graph associated with that URI will be replaced.  To append to a named Graph use the <see cref="StardogConnector.UpdateGraph(Uri,IEnumerable{Triple},IEnumerable{Triple})">UpdateGraph()</see> method instead
        /// </para>
        /// </remarks>
        public virtual void SaveGraph(IGraph g)
        {
            String tID = null;
            try
            {
                //Have to do the delete first as that requires a separate transaction
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

                //Get a Transaction ID, if there is no active Transaction then this operation will be auto-committed
                tID = (this._activeTrans != null) ? this._activeTrans : this.BeginTransaction();

                HttpWebRequest request = this.CreateRequest(this._kb + "/" + tID + "/add", MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
                request.ContentType = MimeTypesHelper.TriG[0];

                //Save the Data as TriG to the Request Stream
                TripleStore store = new TripleStore();
                store.Add(g);
                this._writer.Save(store, new StreamWriter(request.GetRequestStream()));

                Tools.HttpDebugRequest(request);

                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);
                    //If we get here then it was OK
                    response.Close();
                }

                //Commit Transaction only if in auto-commit mode (active transaction will be null)
                if (this._activeTrans == null)
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
                //Rollback Transaction only if got as far as creating a Transaction
                //and in auto-commit mode
                if (tID != null)
                {
                    if (this._activeTrans == null)
                    {
                        try
                        {
                            this.RollbackTransaction(tID);
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
            //If there are no adds or deletes, just return and avoid creating empty transaction
            bool anyData = false;
            if (removals != null && removals.Any()) anyData = true;
            if (additions != null && additions.Any()) anyData = true;
            if (!anyData) return;

            String tID = null;
            try
            {
                //Get a Transaction ID, if there is no active Transaction then this operation will be auto-committed
                tID = (this._activeTrans != null) ? this._activeTrans : this.BeginTransaction();

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
                        this._writer.Save(store, new StreamWriter(request.GetRequestStream()));

                        using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
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
                        this._writer.Save(store, new StreamWriter(request.GetRequestStream()));

                        using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                        {
                            response.Close();
                        }
                    }
                }

                //Commit Transaction only if in auto-commit mode (active transaction will be null)
                if (this._activeTrans == null)
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
                //Rollback Transaction only if got as far as creating a Transaction
                //and in auto-commit mode
                if (tID != null)
                {
                    if (this._activeTrans == null)
                    {
                        try
                        {
                            this.RollbackTransaction(tID);
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
        public virtual void UpdateGraph(String graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (graphUri == null || graphUri.Equals(String.Empty))
            {
                this.UpdateGraph((Uri) null, additions, removals);
            }
            else
            {
                this.UpdateGraph(UriFactory.Create(graphUri), additions, removals);
            }
        }

        /// <summary>
        /// Deletes a Graph from the Stardog store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public virtual void DeleteGraph(Uri graphUri)
        {
            this.DeleteGraph(graphUri.ToSafeString());
        }

        /// <summary>
        /// Deletes a Graph from the Stardog store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public virtual void DeleteGraph(String graphUri)
        {
            String tID = null;
            try
            {
                //Get a Transaction ID, if there is no active Transaction then this operation will be auto-committed
                tID = (this._activeTrans != null) ? this._activeTrans : this.BeginTransaction();

                HttpWebRequest request;
                if (!graphUri.Equals(String.Empty))
                {
                    request = this.CreateRequest(this._kb + "/" + tID + "/clear/?graph-uri=" + HttpUtility.UrlEncode(graphUri), MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
                }
                else
                {
                    request = this.CreateRequest(this._kb + "/" + tID + "/clear/?graph-uri=DEFAULT", MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
                }
                request.ContentType = MimeTypesHelper.WWWFormURLEncoded;

                Tools.HttpDebugRequest(request);

                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);
                    //If we get here then the Delete worked OK
                    response.Close();
                }

                //Commit Transaction only if in auto-commit mode (active transaction will be null)
                if (this._activeTrans == null)
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
                //Rollback Transaction only if got as far as creating a Transaction
                //and in auto-commit mode
                if (tID != null)
                {
                    if (this._activeTrans == null)
                    {
                        try
                        {
                            this.RollbackTransaction(tID);
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
                Object results = this.Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }");
                if (results is SparqlResultSet)
                {
                    List<Uri> graphs = new List<Uri>();
                    foreach (SparqlResult r in ((SparqlResultSet) results))
                    {
                        if (r.HasValue("g"))
                        {
                            INode temp = r["g"];
                            if (temp.NodeType == NodeType.Uri)
                            {
                                graphs.Add(((IUriNode) temp).Uri);
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

#endif

        /// <summary>
        /// Gets the parent server
        /// </summary>
        public override IAsyncStorageServer AsyncParentServer
        {
            get { return this._server; }
        }

        /// <summary>
        /// Saves a Graph to the Store asynchronously
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public override void SaveGraph(IGraph g, AsyncStorageCallback callback, object state)
        {
            this.SaveGraphAsync(g, callback, state);
        }

        protected virtual void SaveGraphAsync(IGraph g, AsyncStorageCallback callback, Object state)
        {
            //Get a Transaction ID, if there is no active Transaction then this operation will start a new transaction and be auto-committed
            if (this._activeTrans != null)
            {
                this.SaveGraphAsync(this._activeTrans, false, g, callback, state);
            }
            else
            {
                this.Begin((sender, args, st) =>
                    {
                        if (args.WasSuccessful)
                        {
                            //Have to do the delete first as that requires a separate transaction
                            if (g.BaseUri != null)
                            {
                                this.DeleteGraph(g.BaseUri, (_1, delArgs, _2) =>
                                    {
                                        if (delArgs.WasSuccessful)
                                        {
                                            this.SaveGraphAsync(this._activeTrans, true, g, callback, state);
                                        }
                                        else
                                        {
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, new RdfStorageException("Unable to save a Named Graph to the Store as this requires deleted any existing Named Graph with this name which failed, see inner exception for more detail", delArgs.Error)), state);
                                        }
                                    }, state);
                            }
                            else
                            {
                                this.SaveGraphAsync(this._activeTrans, true, g, callback, state);
                            }
                        }
                        else
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g, args.Error), state);
                        }
                    }, state);
            }
        }

        protected virtual void SaveGraphAsync(String tID, bool autoCommit, IGraph g, AsyncStorageCallback callback, Object state)
        {
            try
            {
                HttpWebRequest request = this.CreateRequest(this._kb + "/" + tID + "/add", MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
                request.ContentType = MimeTypesHelper.TriG[0];

                request.BeginGetRequestStream(r =>
                    {
                        try
                        {
                            //Save the Data as TriG to the Request Stream
                            Stream stream = request.EndGetRequestStream(r);
                            TripleStore store = new TripleStore();
                            store.Add(g);
                            this._writer.Save(store, new StreamWriter(stream));

                            Tools.HttpDebugRequest(request);
                            request.BeginGetResponse(r2 =>
                                {
                                    try
                                    {
                                        HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(r2);
                                        Tools.HttpDebugResponse(response);

                                        //If we get here then it was OK
                                        response.Close();

                                        //Commit Transaction only if in auto-commit mode (active transaction will be null)
                                        if (autoCommit)
                                        {
                                            this.Commit((sender, args, st) =>
                                                {
                                                    if (args.WasSuccessful)
                                                    {
                                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g), state);
                                                    }
                                                    else
                                                    {
                                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, args.Error), state);
                                                    }
                                                }, state);
                                        }
                                        else
                                        {
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g), state);
                                        }
                                    }
                                    catch (WebException webEx)
                                    {
                                        if (autoCommit)
                                        {
                                            //If something went wrong try to rollback, don't care what the rollback response is
                                            this.Rollback((sender, args, st) => { }, state);
                                        }
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, StorageHelper.HandleHttpError(webEx, "saving a Graph asynchronously to")), state);
                                    }
                                    catch (Exception ex)
                                    {
                                        if (autoCommit)
                                        {
                                            //If something went wrong try to rollback, don't care what the rollback response is
                                            this.Rollback((sender, args, st) => { }, state);
                                        }
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, StorageHelper.HandleError(ex, "saving a Graph asynchronously to")), state);
                                    }
                                }, state);
                        }
                        catch (WebException webEx)
                        {
                            if (autoCommit)
                            {
                                //If something went wrong try to rollback, don't care what the rollback response is
                                this.Rollback((sender, args, st) => { }, state);
                            }
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, StorageHelper.HandleHttpError(webEx, "saving a Graph asynchronously to")), state);
                        }
                        catch (Exception ex)
                        {
                            if (autoCommit)
                            {
                                //If something went wrong try to rollback, don't care what the rollback response is
                                this.Rollback((sender, args, st) => { }, state);
                            }
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, StorageHelper.HandleError(ex, "saving a Graph asynchronously to")), state);
                        }
                    }, state);
            }
            catch (WebException webEx)
            {
                if (autoCommit)
                {
                    //If something went wrong try to rollback, don't care what the rollback response is
                    this.Rollback((sender, args, st) => { }, state);
                }
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, StorageHelper.HandleHttpError(webEx, "saving a Graph asynchronously to")), state);
            }
            catch (Exception ex)
            {
                if (autoCommit)
                {
                    //If something went wrong try to rollback, don't care what the rollback response is
                    this.Rollback((sender, args, st) => { }, state);
                }
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, StorageHelper.HandleError(ex, "saving a Graph asynchronously to")), state);
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

                String tID = (this._activeTrans == null) ? String.Empty : "/" + this._activeTrans;
                String requestUri = this._kb + tID + "/query";
                SparqlParameterizedString construct = new SparqlParameterizedString();
                if (!graphUri.Equals(String.Empty))
                {
                    construct.CommandText = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH @graph { ?s ?p ?o } }";
                    construct.SetUri("graph", UriFactory.Create(graphUri));
                }
                else
                {
                    construct.CommandText = "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }";
                }
                serviceParams.Add("query", construct.ToString());

                request = this.CreateRequest(requestUri, MimeTypesHelper.HttpAcceptHeader, "GET", serviceParams);

                Tools.HttpDebugRequest(request);

                request.BeginGetResponse(r =>
                    {
                        try
                        {
                            HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(r);
                            Tools.HttpDebugResponse(response);

                            IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                            parser.Load(handler, new StreamReader(response.GetResponseStream()));
                            response.Close();

                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler), state);
                        }
                        catch (WebException webEx)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, StorageHelper.HandleHttpError(webEx, "loading a Graph from")), state);
                        }
                        catch (Exception ex)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, StorageHelper.HandleError(ex, "loading a Graph from")), state);
                        }
                    }, state);
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, StorageHelper.HandleHttpError(webEx, "loading a Graph from")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, StorageHelper.HandleError(ex, "loading a Graph from")), state);
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
            //If there are no adds or deletes, just callback and avoid creating empty transaction
            bool anyData = false;
            if (removals != null && removals.Any()) anyData = true;
            if (additions != null && additions.Any()) anyData = true;
            if (!anyData)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
            }
            else
            {
                this.UpdateGraphAsync(graphUri, additions, removals, callback, state);
            }
        }

        protected virtual void UpdateGraphAsync(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, Object state)
        {
            //Get a Transaction ID, if there is no active Transaction then this operation will start a new transaction and be auto-committed
            if (this._activeTrans != null)
            {
                this.UpdateGraphAsync(this._activeTrans, false, graphUri, additions, removals, callback, state);
            }
            else
            {
                this.Begin((sender, args, st) =>
                    {
                        if (args.WasSuccessful)
                        {
                            this.UpdateGraphAsync(this._activeTrans, true, graphUri, additions, removals, callback, state);
                        }
                        else
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(), args.Error), state);
                        }
                    }, state);
            }
        }

        protected virtual void UpdateGraphAsync(String tID, bool autoCommit, String graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, Object state)
        {
            try
            {
                if (removals != null && removals.Any())
                {
                    HttpWebRequest request = this.CreateRequest(this._kb + "/" + tID + "/remove", MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
                    request.ContentType = MimeTypesHelper.TriG[0];

                    request.BeginGetRequestStream(r =>
                        {
                            try
                            {
                                //Save the Data as TriG to the Request Stream
                                Stream stream = request.EndGetRequestStream(r);
                                TripleStore store = new TripleStore();
                                Graph g = new Graph();
                                g.BaseUri = graphUri.ToSafeUri();
                                g.Assert(removals);
                                store.Add(g);
                                this._writer.Save(store, new StreamWriter(stream));

                                Tools.HttpDebugRequest(request);
                                request.BeginGetResponse(r2 =>
                                    {
                                        try
                                        {
                                            HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(r2);
                                            Tools.HttpDebugResponse(response);

                                            //If we get here then it was OK
                                            response.Close();

                                            if (additions != null && additions.Any())
                                            {
                                                //Now we need to do additions
                                                request = this.CreateRequest(this._kb + "/" + tID + "/add", MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
                                                request.ContentType = MimeTypesHelper.TriG[0];

                                                request.BeginGetRequestStream(r3 =>
                                                    {
                                                        try
                                                        {
                                                            //Save the Data as TriG to the Request Stream
                                                            stream = request.EndGetRequestStream(r3);
                                                            store = new TripleStore();
                                                            g = new Graph();
                                                            g.BaseUri = graphUri.ToSafeUri();
                                                            g.Assert(additions);
                                                            store.Add(g);
                                                            this._writer.Save(store, new StreamWriter(stream));

                                                            Tools.HttpDebugRequest(request);

                                                            request.BeginGetResponse(r4 =>
                                                                {
                                                                    try
                                                                    {
                                                                        response = (HttpWebResponse) request.EndGetResponse(r4);
                                                                        Tools.HttpDebugResponse(response);

                                                                        //If we get here then it was OK
                                                                        response.Close();

                                                                        //Commit Transaction only if in auto-commit mode (active transaction will be null)
                                                                        if (autoCommit)
                                                                        {
                                                                            this.Commit((sender, args, st) =>
                                                                                {
                                                                                    if (args.WasSuccessful)
                                                                                    {
                                                                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(), args.Error), state);
                                                                                    }
                                                                                }, state);
                                                                        }
                                                                        else
                                                                        {
                                                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
                                                                        }
                                                                    }
                                                                    catch (WebException webEx)
                                                                    {
                                                                        if (autoCommit)
                                                                        {
                                                                            //If something went wrong try to rollback, don't care what the rollback response is
                                                                            this.Rollback((sender, args, st) => { }, state);
                                                                        }
                                                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")), state);
                                                                    }
                                                                    catch (Exception ex)
                                                                    {
                                                                        if (autoCommit)
                                                                        {
                                                                            //If something went wrong try to rollback, don't care what the rollback response is
                                                                            this.Rollback((sender, args, st) => { }, state);
                                                                        }
                                                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, StorageHelper.HandleError(ex, "updating a Graph asynchronously in")), state);
                                                                    }
                                                                }, state);
                                                        }
                                                        catch (WebException webEx)
                                                        {
                                                            if (autoCommit)
                                                            {
                                                                //If something went wrong try to rollback, don't care what the rollback response is
                                                                this.Rollback((sender, args, st) => { }, state);
                                                            }
                                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")), state);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            if (autoCommit)
                                                            {
                                                                //If something went wrong try to rollback, don't care what the rollback response is
                                                                this.Rollback((sender, args, st) => { }, state);
                                                            }
                                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, StorageHelper.HandleError(ex, "updating a Graph asynchronously in")), state);
                                                        }
                                                    }, state);
                                            }
                                            else
                                            {
                                                //No additions to do
                                                //Commit Transaction only if in auto-commit mode (active transaction will be null)
                                                if (autoCommit)
                                                {
                                                    this.Commit((sender, args, st) =>
                                                        {
                                                            if (args.WasSuccessful)
                                                            {
                                                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
                                                            }
                                                            else
                                                            {
                                                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(), args.Error), state);
                                                            }
                                                        }, state);
                                                }
                                                else
                                                {
                                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
                                                }
                                            }
                                        }
                                        catch (WebException webEx)
                                        {
                                            if (autoCommit)
                                            {
                                                //If something went wrong try to rollback, don't care what the rollback response is
                                                this.Rollback((sender, args, st) => { }, state);
                                            }
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")), state);
                                        }
                                        catch (Exception ex)
                                        {
                                            if (autoCommit)
                                            {
                                                //If something went wrong try to rollback, don't care what the rollback response is
                                                this.Rollback((sender, args, st) => { }, state);
                                            }
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, StorageHelper.HandleError(ex, "updating a Graph asynchronously in")), state);
                                        }
                                    }, state);
                            }
                            catch (WebException webEx)
                            {
                                if (autoCommit)
                                {
                                    //If something went wrong try to rollback, don't care what the rollback response is
                                    this.Rollback((sender, args, st) => { }, state);
                                }
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")), state);
                            }
                            catch (Exception ex)
                            {
                                if (autoCommit)
                                {
                                    //If something went wrong try to rollback, don't care what the rollback response is
                                    this.Rollback((sender, args, st) => { }, state);
                                }
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, StorageHelper.HandleError(ex, "updating a Graph asynchronously in")), state);
                            }
                        }, state);
                }
                else if (additions != null && additions.Any())
                {
                    HttpWebRequest request = this.CreateRequest(this._kb + "/" + tID + "/add", MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
                    request.ContentType = MimeTypesHelper.TriG[0];

                    request.BeginGetRequestStream(r =>
                        {
                            try
                            {
                                //Save the Data as TriG to the Request Stream
                                Stream stream = request.EndGetRequestStream(r);
                                TripleStore store = new TripleStore();
                                Graph g = new Graph();
                                g.Assert(additions);
                                g.BaseUri = graphUri.ToSafeUri();
                                store.Add(g);
                                this._writer.Save(store, new StreamWriter(stream));

                                Tools.HttpDebugRequest(request);

                                request.BeginGetResponse(r2 =>
                                    {
                                        try
                                        {
                                            HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(r2);
                                            Tools.HttpDebugResponse(response);

                                            //If we get here then it was OK
                                            response.Close();

                                            //Commit Transaction only if in auto-commit mode (active transaction will be null)
                                            if (autoCommit)
                                            {
                                                this.Commit((sender, args, st) =>
                                                    {
                                                        if (args.WasSuccessful)
                                                        {
                                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
                                                        }
                                                        else
                                                        {
                                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(), args.Error), state);
                                                        }
                                                    }, state);
                                            }
                                            else
                                            {
                                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
                                            }
                                        }
                                        catch (WebException webEx)
                                        {
                                            if (autoCommit)
                                            {
                                                //If something went wrong try to rollback, don't care what the rollback response is
                                                this.Rollback((sender, args, st) => { }, state);
                                            }
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")), state);
                                        }
                                        catch (Exception ex)
                                        {
                                            if (autoCommit)
                                            {
                                                //If something went wrong try to rollback, don't care what the rollback response is
                                                this.Rollback((sender, args, st) => { }, state);
                                            }
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, StorageHelper.HandleError(ex, "updating a Graph asynchronously in")), state);
                                        }
                                    }, state);
                            }
                            catch (WebException webEx)
                            {
                                if (autoCommit)
                                {
                                    //If something went wrong try to rollback, don't care what the rollback response is
                                    this.Rollback((sender, args, st) => { }, state);
                                }
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")), state);
                            }
                            catch (Exception ex)
                            {
                                if (autoCommit)
                                {
                                    //If something went wrong try to rollback, don't care what the rollback response is
                                    this.Rollback((sender, args, st) => { }, state);
                                }
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, StorageHelper.HandleError(ex, "updating a Graph asynchronously in")), state);
                            }
                        }, state);
                }
                else
                {
                    //Nothing to do, just invoke callback
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
                }
            }
            catch (WebException webEx)
            {
                if (autoCommit)
                {
                    //If something went wrong try to rollback, don't care what the rollback response is
                    this.Rollback((sender, args, st) => { }, state);
                }
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")), state);
            }
            catch (Exception ex)
            {
                if (autoCommit)
                {
                    //If something went wrong try to rollback, don't care what the rollback response is
                    this.Rollback((sender, args, st) => { }, state);
                }
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, StorageHelper.HandleError(ex, "updating a Graph asynchronously in")), state);
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
            //Get a Transaction ID, if there is no active Transaction then this operation will start a new transaction and be auto-committed
            if (this._activeTrans != null)
            {
                this.DeleteGraphAsync(this._activeTrans, false, graphUri, callback, state);
            }
            else
            {
                this.Begin((sender, args, st) =>
                    {
                        if (args.WasSuccessful)
                        {
                            this.DeleteGraphAsync(this._activeTrans, true, graphUri, callback, state);
                        }
                        else
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), args.Error), state);
                        }
                    }, state);
            }
        }

        protected virtual void DeleteGraphAsync(String tID, bool autoCommit, String graphUri, AsyncStorageCallback callback, Object state)
        {
            try
            {
                HttpWebRequest request;
                if (!graphUri.Equals(String.Empty))
                {
                    request = this.CreateRequest(this._kb + "/" + tID + "/clear/?graph-uri=" + HttpUtility.UrlEncode(graphUri), MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
                }
                else
                {
                    request = this.CreateRequest(this._kb + "/" + tID + "/clear/?graph-uri=DEFAULT", MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
                }
                request.ContentType = MimeTypesHelper.WWWFormURLEncoded;

                Tools.HttpDebugRequest(request);
                request.BeginGetResponse(r =>
                    {
                        try
                        {
                            HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(r);

                            Tools.HttpDebugResponse(response);
                            //If we get here then the Delete worked OK
                            response.Close();

                            //Commit Transaction only if in auto-commit mode (active transaction will be null)
                            if (autoCommit)
                            {
                                this.Commit((sender, args, st) =>
                                    {
                                        if (args.WasSuccessful)
                                        {
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri()), state);
                                        }
                                        else
                                        {
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), args.Error), state);
                                        }
                                    }, state);
                            }
                            else
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri()), state);
                            }
                        }
                        catch (WebException webEx)
                        {
                            if (autoCommit)
                            {
                                //If something went wrong try to rollback, don't care what the rollback response is
                                this.Rollback((sender, args, st) => { }, state);
                            }
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), StorageHelper.HandleHttpError(webEx, "deleting a Graph asynchronously from")), state);
                        }
                        catch (Exception ex)
                        {
                            if (autoCommit)
                            {
                                //If something went wrong try to rollback, don't care what the rollback response is
                                this.Rollback((sender, args, st) => { }, state);
                            }
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), StorageHelper.HandleError(ex, "deleting a Graph asynchronously from")), state);
                        }
                    }, state);
            }
            catch (WebException webEx)
            {
                if (autoCommit)
                {
                    //If something went wrong try to rollback, don't care what the rollback response is
                    this.Rollback((sender, args, st) => { }, state);
                }
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), StorageHelper.HandleHttpError(webEx, "deleting a Graph asynchronously from")), state);
            }
            catch (Exception ex)
            {
                if (autoCommit)
                {
                    //If something went wrong try to rollback, don't care what the rollback response is
                    this.Rollback((sender, args, st) => { }, state);
                }
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), StorageHelper.HandleError(ex, "deleting a Graph asynchronously from")), state);
            }
        }

        /// <summary>
        /// Queries the store asynchronously
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void Query(String query, AsyncStorageCallback callback, Object state)
        {
            Graph g = new Graph();
            SparqlResultSet results = new SparqlResultSet();
            this.Query(new GraphHandler(g), new ResultSetHandler(results), query, (sender, args, st) =>
                {
                    if (results.ResultsType != SparqlResultsType.Unknown)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQuery, query, results, args.Error), state);
                    }
                    else
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQuery, query, g, args.Error), state);
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
        public virtual void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String query, AsyncStorageCallback callback, Object state)
        {
            try
            {
                HttpWebRequest request;

                String tID = (this._activeTrans == null) ? String.Empty : "/" + this._activeTrans;

                //String accept = MimeTypesHelper.HttpRdfOrSparqlAcceptHeader;
                String accept = MimeTypesHelper.CustomHttpAcceptHeader(MimeTypesHelper.SparqlResultsXml.Concat(MimeTypesHelper.Definitions.Where(d => d.CanParseRdf).SelectMany(d => d.MimeTypes)));

                //Create the Request, for simplicity async requests are always POST
                Dictionary<String, String> queryParams = new Dictionary<string, string>();
                request = this.CreateRequest(this._kb + tID + "/query", accept, "POST", queryParams);

                //Build the Post Data and add to the Request Body
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

                            //Get the Response and process based on the Content Type
                            request.BeginGetResponse(r2 =>
                                {
                                    try
                                    {
                                        HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(r2);
                                        Tools.HttpDebugResponse(response);

                                        StreamReader data = new StreamReader(response.GetResponseStream());
                                        String ctype = response.ContentType;
                                        try
                                        {
                                            //Is the Content Type referring to a Sparql Result Set format?
                                            ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(ctype, Regex.IsMatch(query, "ASK", RegexOptions.IgnoreCase));
                                            resreader.Load(resultsHandler, data);
                                            response.Close();
                                        }
                                        catch (RdfParserSelectionException)
                                        {
                                            //If we get a Parser Selection exception then the Content Type isn't valid for a Sparql Result Set

                                            //Is the Content Type referring to a RDF format?
                                            IRdfReader rdfreader = MimeTypesHelper.GetParser(ctype);
                                            rdfreader.Load(rdfHandler, data);
                                            response.Close();
                                        }
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, query, rdfHandler, resultsHandler), state);
                                    }
                                    catch (WebException webEx)
                                    {
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, StorageHelper.HandleHttpQueryError(webEx)), state);
                                    }
                                    catch (Exception ex)
                                    {
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, StorageHelper.HandleQueryError(ex)), state);
                                    }
                                }, state);
                        }
                        catch (WebException webEx)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, StorageHelper.HandleHttpQueryError(webEx)), state);
                        }
                        catch (Exception ex)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, StorageHelper.HandleQueryError(ex)), state);
                        }
                    }, state);
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, StorageHelper.HandleHttpQueryError(webEx)), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, StorageHelper.HandleQueryError(ex)), state);
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
        protected virtual HttpWebRequest CreateRequest(String servicePath, String accept, String method, Dictionary<String, String> requestParams)
        {
            //Build the Request Uri
            String requestUri = this._baseUri + servicePath;
            if (!ReferenceEquals(requestParams, null) && requestParams.Count > 0)
            {
                requestUri += "?";
                foreach (String p in requestParams.Keys)
                {
                    requestUri += p + "=" + HttpUtility.UrlEncode(requestParams[p]) + "&";
                }
                requestUri = requestUri.Substring(0, requestUri.Length - 1);
            }

            //Create our Request
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(requestUri);
            request.Accept = accept;
            request.Method = method;
            request = base.GetProxiedRequest(request);

            //Add the special Stardog Headers
            this.AddStardogHeaders(request);

            //Add Credentials if needed
            if (this._hasCredentials)
            {
                if (Options.ForceHttpBasicAuth)
                {
                    //Forcibly include a HTTP basic authentication header
#if !SILVERLIGHT
                    string credentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(this._username + ":" + this._pwd));
                    request.Headers.Add("Authorization", "Basic " + credentials);
#else
                    string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(this._username + ":" + this._pwd));
                    request.Headers["Authorization"] = "Basic " + credentials;
#endif
                }
                else
                {
                    //Leave .Net to cope with HTTP auth challenge response
                    NetworkCredential credentials = new NetworkCredential(this._username, this._pwd);
                    request.Credentials = credentials;
#if !SILVERLIGHT
                    request.PreAuthenticate = true;
#endif
                }
            }

            return request;
        }

        /// <summary>
        /// Adds Stardog specific request headers
        /// </summary>
        /// <param name="request"></param>
        protected virtual void AddStardogHeaders(HttpWebRequest request)
        {
#if !SILVERLIGHT
            request.Headers.Add("SD-Connection-String", "kb=" + this._kb + this.GetReasoningParameter()); // removed persist=sync, no longer needed in latest stardog versions?
            request.Headers.Add("SD-Protocol", "1.0");
#else
            request.Headers["SD-Connection-String"] = "kb=" + this._kb + this.GetReasoningParameter();
            request.Headers["SD-Protocol"] = "1.0";
#endif
        }

        protected virtual String GetReasoningParameter()
        {
            switch (this._reasoning)
            {
                case StardogReasoningMode.QL:
                    return ";reasoning=QL";
                case StardogReasoningMode.EL:
                    return ";reasoning=EL";
                case StardogReasoningMode.RL:
                    return ";reasoning=RL";
                case StardogReasoningMode.DL:
                    return ";reasoning=DL";
                case StardogReasoningMode.RDFS:
                    return ";reasoning=RDFS";
                case StardogReasoningMode.SL:
                    throw new RdfStorageException("Stardog 1.* does not support the SL reasoning level, please ensure you are using a Stardog 2.* connector if you wish to use this reasoning level");
                case StardogReasoningMode.None:
                default:
                    return String.Empty;
            }
        }

        #endregion

        #region Stardog Transaction Support

#if !NO_SYNC_HTTP

        protected virtual String BeginTransaction()
        {
            String tID = null;

            HttpWebRequest request = this.CreateRequest(this._kb + "/transaction/begin", "text/plain" /*MimeTypesHelper.Any*/, "POST", new Dictionary<string, string>());
            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
            try
            {
                Tools.HttpDebugRequest(request);

                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
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

            if (String.IsNullOrEmpty(tID))
            {
                throw new RdfStorageException("Stardog failed to begin a Transaction");
            }
            return tID;
        }

        protected virtual void CommitTransaction(String tID)
        {
            HttpWebRequest request = this.CreateRequest(this._kb + "/transaction/commit/" + tID, "text/plain" /* MimeTypesHelper.Any*/, "POST", new Dictionary<string, string>());
            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;

            Tools.HttpDebugRequest(request);

            using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
            {
                Tools.HttpDebugResponse(response);
                response.Close();
            }

            //Reset the Active Transaction on this Thread if the IDs match up
            if (this._activeTrans != null && this._activeTrans.Equals(tID))
            {
                this._activeTrans = null;
            }
        }

        protected virtual void RollbackTransaction(String tID)
        {
            HttpWebRequest request = this.CreateRequest(this._kb + "/transaction/rollback/" + tID, MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
            using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
            {
                response.Close();
            }

            //Reset the Active Transaction on this Thread if the IDs match up
            if (this._activeTrans != null && this._activeTrans.Equals(tID))
            {
                this._activeTrans = null;
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
            try
            {
                Monitor.Enter(this);
                if (this._activeTrans != null)
                {
                    throw new RdfStorageException("Cannot start a new Transaction as there is already an active Transaction");
                }
                this._activeTrans = this.BeginTransaction();
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
                if (this._activeTrans == null)
                {
                    throw new RdfStorageException("Cannot commit a Transaction as there is currently no active Transaction");
                }
                this.CommitTransaction(this._activeTrans);
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
                if (this._activeTrans == null)
                {
                    throw new RdfStorageException("Cannot rollback a Transaction on the as there is currently no active Transaction");
                }
                this.RollbackTransaction(this._activeTrans);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

#endif

        /// <summary>
        /// Begins a transaction asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void Begin(AsyncStorageCallback callback, object state)
        {
            try
            {
                if (this._activeTrans != null)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, new RdfStorageException("Cannot start a new Transaction as there is already an active Transaction")), state);
                }
                else
                {
                    HttpWebRequest request = this.CreateRequest(this._kb + "/transaction/begin", "text/plain" /*MimeTypesHelper.Any*/, "POST", new Dictionary<string, string>());
                    request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                    try
                    {
                        Tools.HttpDebugRequest(request);
                        request.BeginGetResponse(r =>
                            {
                                try
                                {
                                    HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(r);
                                    String tID;
                                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                                    {
                                        Tools.HttpDebugResponse(response);
                                        tID = reader.ReadToEnd();
                                        reader.Close();
                                    }
                                    response.Close();

                                    if (String.IsNullOrEmpty(tID))
                                    {
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, new RdfStorageException("Stardog failed to begin a transaction")), state);
                                    }
                                    else
                                    {
                                        this._activeTrans = tID;
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin), state);
                                    }
                                }
                                catch (WebException webEx)
                                {
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, StorageHelper.HandleHttpError(webEx, "beginning a Transaction in")), state);
                                }
                                catch (Exception ex)
                                {
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, StorageHelper.HandleError(ex, "beginning a Transaction in")), state);
                                }
                            }, state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, StorageHelper.HandleHttpError(webEx, "beginning a Transaction in")), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, StorageHelper.HandleError(ex, "beginning a Transaction in")), state);
                    }
                }
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, StorageHelper.HandleHttpError(webEx, "beginning a Transaction in")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, StorageHelper.HandleError(ex, "beginning a Transaction in")), state);
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
                if (this._activeTrans == null)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, new RdfStorageException("Cannot commit a Transaction as there is currently no active Transaction")), state);
                }
                else
                {
                    HttpWebRequest request = this.CreateRequest(this._kb + "/transaction/commit/" + this._activeTrans, "text/plain" /* MimeTypesHelper.Any*/, "POST", new Dictionary<string, string>());
                    request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                    Tools.HttpDebugRequest(request);
                    try
                    {
                        request.BeginGetResponse(r =>
                            {
                                try
                                {
                                    HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(r);

                                    Tools.HttpDebugResponse(response);
                                    response.Close();
                                    this._activeTrans = null;
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit), state);
                                }
                                catch (WebException webEx)
                                {
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, StorageHelper.HandleHttpError(webEx, "committing a Transaction to")), state);
                                }
                                catch (Exception ex)
                                {
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, StorageHelper.HandleError(ex, "committing a Transaction to")), state);
                                }
                            }, state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, StorageHelper.HandleHttpError(webEx, "committing a Transaction to")), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, StorageHelper.HandleError(ex, "committing a Transaction to")), state);
                    }
                }
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, StorageHelper.HandleHttpError(webEx, "committing a Transaction to")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, StorageHelper.HandleError(ex, "committing a Transaction to")), state);
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
                if (this._activeTrans == null)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback, new RdfStorageException("Cannot rollback a Transaction on the as there is currently no active Transaction")), state);
                }
                else
                {
                    HttpWebRequest request = this.CreateRequest(this._kb + "/transaction/rollback/" + this._activeTrans, MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
                    request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                    try
                    {
                        request.BeginGetResponse(r =>
                            {
                                try
                                {
                                    HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(r);
                                    response.Close();
                                    this._activeTrans = null;
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback), state);
                                }
                                catch (WebException webEx)
                                {
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback, StorageHelper.HandleHttpError(webEx, "rolling back a Transaction from")), state);
                                }
                                catch (Exception ex)
                                {
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback, StorageHelper.HandleError(ex, "rolling back a Transaction from")), state);
                                }
                            }, state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback, StorageHelper.HandleHttpError(webEx, "rolling back a Transaction from")), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback, StorageHelper.HandleError(ex, "rolling back a Transaction from")), state);
                    }
                }
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback, StorageHelper.HandleHttpError(webEx, "rolling back a Transaction from")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback, StorageHelper.HandleError(ex, "rolling back a Transaction from")), state);
            }
        }

        #endregion

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
            String mode = String.Empty;
            switch (this._reasoning)
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
            }
            return "[Stardog] Knowledge Base '" + this._kb + "' on Server '" + this._baseUri + "'" + mode;
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
            INode genericManager = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassStorageProvider));
            INode server = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyServer));
            INode store = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyStore));
            INode loadMode = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyLoadMode));

            //Add Core config
            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName)));
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(this._baseUri)));
            context.Graph.Assert(new Triple(manager, store, context.Graph.CreateLiteralNode(this._kb)));

            //Add reasoning mode
            if (this._reasoning != StardogReasoningMode.None) context.Graph.Assert(new Triple(manager, loadMode, context.Graph.CreateLiteralNode(this._reasoning.ToString())));

            //Add User Credentials
            if (this._username != null && this._pwd != null)
            {
                INode username = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUser));
                INode pwd = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyPassword));
                context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(this._username)));
                context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(this._pwd)));
            }

            base.SerializeProxyConfig(manager, context);
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
        public StardogV1Connector(String baseUri, String kbID, StardogReasoningMode reasoning)
            : this(baseUri, kbID, reasoning, null, null)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        public StardogV1Connector(String baseUri, String kbID)
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
        public StardogV1Connector(String baseUri, String kbID, String username, String password)
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
        public StardogV1Connector(String baseUri, String kbID, StardogReasoningMode reasoning, String username, String password)
            : base(baseUri, kbID, reasoning, username, password)
        {
        }

#if !NO_PROXY

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="reasoning">Reasoning Mode</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogV1Connector(String baseUri, String kbID, StardogReasoningMode reasoning, WebProxy proxy)
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
        public StardogV1Connector(String baseUri, String kbID, StardogReasoningMode reasoning, String username, String password, WebProxy proxy)
            : base(baseUri, kbID, reasoning, username, password, proxy)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogV1Connector(String baseUri, String kbID, WebProxy proxy)
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
        public StardogV1Connector(String baseUri, String kbID, String username, String password, WebProxy proxy)
            : this(baseUri, kbID, StardogReasoningMode.None, username, password, proxy)
        {
        }

#endif
    }

    /// <summary>
    /// A Stardog Connector for connecting to Stardog version 2.* servers
    /// </summary>
    public class StardogV2Connector
        : StardogV1Connector,
#if !NO_SYNC_HTTP
          IUpdateableStorage,
#endif
          IAsyncUpdateableStorage
    {
        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="reasoning">Reasoning Mode</param>
        public StardogV2Connector(String baseUri, String kbID, StardogReasoningMode reasoning)
            : this(baseUri, kbID, reasoning, null, null)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        public StardogV2Connector(String baseUri, String kbID)
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
        public StardogV2Connector(String baseUri, String kbID, String username, String password)
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
        public StardogV2Connector(String baseUri, String kbID, StardogReasoningMode reasoning, String username, String password)
            : base(baseUri, kbID, reasoning, username, password)
        {
            this._server = new StardogV2Server(baseUri, username, password);
        }

#if !NO_PROXY

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="reasoning">Reasoning Mode</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogV2Connector(String baseUri, String kbID, StardogReasoningMode reasoning, WebProxy proxy)
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
        public StardogV2Connector(String baseUri, String kbID, StardogReasoningMode reasoning, String username, String password, WebProxy proxy)
            : base(baseUri, kbID, reasoning, username, password, proxy)
        {
            this._server = new StardogV2Server(baseUri, username, password, proxy);
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogV2Connector(String baseUri, String kbID, WebProxy proxy)
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
        public StardogV2Connector(String baseUri, String kbID, String username, String password, WebProxy proxy)
            : this(baseUri, kbID, StardogReasoningMode.None, username, password, proxy)
        {
        }

#endif

        /// <summary>
        /// Adds Stardog specific request headers
        /// </summary>
        /// <param name="request"></param>
        protected override void AddStardogHeaders(HttpWebRequest request)
        {
            String reasoning = this.GetReasoningParameter();
            if (reasoning.Length > 1) reasoning = reasoning.Substring(1);
#if !SILVERLIGHT
            request.Headers.Add("SD-Connection-String", reasoning); // Only reasoning parameter needed in Stardog 2.0
#else
            request.Headers["SD-Connection-String"] = reasoning;
#endif
        }

        protected override String GetReasoningParameter()
        {
            switch (this._reasoning)
            {
                case StardogReasoningMode.QL:
                    return ";reasoning=QL";
                case StardogReasoningMode.EL:
                    return ";reasoning=EL";
                case StardogReasoningMode.RL:
                    return ";reasoning=RL";
                case StardogReasoningMode.DL:
                    return ";reasoning=DL";
                case StardogReasoningMode.RDFS:
                    return ";reasoning=RDFS";
                case StardogReasoningMode.SL:
                    return ";reasoning=SL";
                case StardogReasoningMode.None:
                default:
                    return String.Empty;
            }
        }

#if !NO_SYNC_HTTP

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

                //Create the Request
                HttpWebRequest request = this.CreateRequest(this._kb + "/update", MimeTypesHelper.Any, "POST", null);

                //Build the Post Data and add to the Request Body
                request.ContentType = MimeTypesHelper.SparqlUpdate;
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), new UTF8Encoding()))
                {
                    writer.Write(sparqlUpdate);
                    writer.Close();
                }

                Tools.HttpDebugRequest(request);

                // Check the response
                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);

                    //If we got here then the update succeeded
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpError(webEx, "executing a SPARQL update against");
            }
        }

#endif

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

                //Create the Request, for simplicity async requests are always POST
                HttpWebRequest request = this.CreateRequest(this._kb + "/update", MimeTypesHelper.Any, "POST", null);

                //Create the request body
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

                        //Get the Response and process based on the Content Type
                        request.BeginGetResponse(r2 =>
                        {
                            try
                            {
                                using (HttpWebResponse response = (HttpWebResponse) request.EndGetResponse(r2))
                                {
                                    Tools.HttpDebugResponse(response);
                                    // If we get here the update completed OK
                                    response.Close();
                                }

                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdates), state);
                            }
                            catch (WebException webEx)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, StorageHelper.HandleHttpError(webEx, "executing a SPARQL update against")), state);
                            }
                            catch (Exception ex)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, StorageHelper.HandleError(ex, "executing a SPARQL update against")), state);
                            }
                        }, state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, StorageHelper.HandleHttpError(webEx, "executing a SPARQL update against")), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, StorageHelper.HandleError(ex, "executing a SPARQL update against")), state);
                    }
                }, state);
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, StorageHelper.HandleHttpError(webEx, "executing a SPARQL update against")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, StorageHelper.HandleError(ex, "executing a SPARQL update against")), state);
            }
        }
    }

    /// <summary>
    /// A Stardog connector for connecting to Stardog servers running the latest version, currently this is version 2.*
    /// </summary>
    public class StardogConnector
        : StardogV2Connector
    {
        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="reasoning">Reasoning Mode</param>
        public StardogConnector(String baseUri, String kbID, StardogReasoningMode reasoning)
            : this(baseUri, kbID, reasoning, null, null)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        public StardogConnector(String baseUri, String kbID)
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
        public StardogConnector(String baseUri, String kbID, String username, String password)
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
        public StardogConnector(String baseUri, String kbID, StardogReasoningMode reasoning, String username, String password)
            : base(baseUri, kbID, reasoning, username, password)
        {
        }

#if !NO_PROXY

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="reasoning">Reasoning Mode</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogConnector(String baseUri, String kbID, StardogReasoningMode reasoning, WebProxy proxy)
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
        public StardogConnector(String baseUri, String kbID, StardogReasoningMode reasoning, String username, String password, WebProxy proxy)
            : base(baseUri, kbID, reasoning, username, password, proxy)
        {
        }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogConnector(String baseUri, String kbID, WebProxy proxy)
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
        public StardogConnector(String baseUri, String kbID, String username, String password, WebProxy proxy)
            : this(baseUri, kbID, StardogReasoningMode.None, username, password, proxy)
        {
        }

#endif
    }
}