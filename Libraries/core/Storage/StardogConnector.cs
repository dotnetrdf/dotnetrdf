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
using System.Threading;
#if !NO_WEB
using System.Web;
#endif
using Newtonsoft.Json.Linq;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage.Params;
using VDS.RDF.Storage.Management;
using VDS.RDF.Storage.Management.Provisioning;
using VDS.RDF.Storage.Management.Provisioning.Stardog;
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
        RDFS
    }

    /// <summary>
    /// Class for connecting to a Stardog store via HTTP
    /// </summary>
    /// <remarks>
    /// <para>
    /// Has full support for Stardog Transactions, connection is in auto-commit mode by default i.e. all write operations (Delete/Save/Update) will create and use a dedicated transaction for their operation, if the operation fails the transaction will automatically be rolled back.  You can manage Transactions using the <see cref="StardogConnector.Begin">Begin()</see>, <see cref="StardogConnector.Commit">Commit()</see> and <see cref="StardogConnector.Rollback">Rollback()</see> methods.
    /// </para>
    /// <para>
    /// The connector maintains a single transaction which is shared across all threads since Stardog is currently provides only MRSW (Multiple Reader Single Writer) concurrency and does not permit multiple transactions to occur simultaneously.  
    /// </para>
    /// </remarks>
    public class StardogConnector 
        : BaseAsyncHttpConnector, IAsyncQueryableStorage, IAsyncTransactionalStorage, IAsyncStorageServer, IConfigurationSerializable
#if !NO_SYNC_HTTP
        , IQueryableStorage, ITransactionalStorage, IStorageServer
#endif
    {
        /// <summary>
        /// Constant for the default Anonymous user account and password used by Stardog if you have not supplied a shiro.ini file or otherwise disabled security
        /// </summary>
        public const String AnonymousUser = "anonymous";

        private String _baseUri, _adminUri, _kb, _username, _pwd;
        private bool _hasCredentials = false;
        private StardogReasoningMode _reasoning = StardogReasoningMode.None;

        private String _activeTrans = null;
        private TriGWriter _writer = new TriGWriter();

        /// <summary>
        /// Available Sesame template types
        /// </summary>
        private List<Type> _templateTypes = new List<Type>()
        {
            typeof(StardogMemTemplate),
            typeof(StardogDiskTemplate)
        };

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="reasoning">Reasoning Mode</param>
        public StardogConnector(String baseUri, String kbID, StardogReasoningMode reasoning)
            : this(baseUri, kbID, reasoning, null, null) { }

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
        public StardogConnector(String baseUri, String kbID, String username, String password)
            : this(baseUri, kbID, StardogReasoningMode.None, username, password) { }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="reasoning">Reasoning Mode</param>
        public StardogConnector(String baseUri, String kbID, StardogReasoningMode reasoning, String username, String password)
            : base()
        {
            this._baseUri = baseUri;
            if (!this._baseUri.EndsWith("/")) this._baseUri += "/";
            this._adminUri = this._baseUri + "admin/";
            this._kb = kbID;
            this._reasoning = reasoning;

            //Prep the writer
            this._writer.HighSpeedModePermitted = true;
            this._writer.CompressionLevel = WriterCompressionLevel.None;
            this._writer.UseMultiThreadedWriting = false;

            this._username = username;
            this._pwd = password;
            this._hasCredentials = (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password));
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
            : this(baseUri, kbID, reasoning, null, null, proxy) { }

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
        public StardogConnector(String baseUri, String kbID, WebProxy proxy)
            : this(baseUri, kbID, StardogReasoningMode.None, proxy) { }

        /// <summary>
        /// Creates a new connection to a Stardog Store
        /// </summary>
        /// <param name="baseUri">Base Uri of the Server</param>
        /// <param name="kbID">Knowledge Base (i.e. Database) ID</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="proxy">Proxy Server</param>
        public StardogConnector(String baseUri, String kbID, String username, String password, WebProxy proxy)
            : this(baseUri, kbID, StardogReasoningMode.None, username, password, proxy) { }

#endif

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
        [Description("What reasoning mode (if any) is currently in use for SPARQL Queries")]
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
        /// Gets the IO Behaviour of Stardog
        /// </summary>
        public override IOBehaviour IOBehaviour
        {
            get
            {
                return IOBehaviour.GraphStore | IOBehaviour.CanUpdateTriples | IOBehaviour.StorageServer;
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

        /// <summary>
        /// Returns that Updates are supported on Stardog Stores
        /// </summary>
        public override bool UpdateSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that deleting graphs from the Stardog store is not yet supported (due to a .Net specific issue)
        /// </summary>
        public override bool DeleteSupported
        {
            get
            {
                return true;
            }
        }

#if !NO_SYNC_HTTP

        /// <summary>
        /// Makes a SPARQL Query against the underlying Store using whatever reasoning mode is currently in-use
        /// </summary>
        /// <param name="sparqlQuery">Sparql Query</param>
        /// <returns></returns>
        public object Query(String sparqlQuery)
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
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery)
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
        public void LoadGraph(IGraph g, Uri graphUri)
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
        public void LoadGraph(IRdfHandler handler, Uri graphUri)
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
        public void LoadGraph(IGraph g, String graphUri)
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
        public void LoadGraph(IRdfHandler handler, String graphUri)
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

                HttpWebRequest request = this.CreateRequest(this._kb + "/" + tID + "/add", MimeTypesHelper.Any, "POST", new Dictionary<string,string>());
                request.ContentType = MimeTypesHelper.TriG[0];
                
                //Save the Data as TriG to the Request Stream
                TripleStore store = new TripleStore();
                store.Add(g);
                this._writer.Save(store, new StreamWriter(request.GetRequestStream()));

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
                    if (this._activeTrans == null)
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

            // if there are no adds or deletes, just return and avoid creating empty transaction

            bool anyData = false;

            if (removals != null)
                if (removals.Any())
                    anyData = true;
            
            if (additions != null)
                if (additions.Any())
                    anyData = true;

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
                        this._writer.Save(store, new StreamWriter(request.GetRequestStream()));

                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
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
                    if (this._activeTrans == null)
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
        public void UpdateGraph(String graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (graphUri == null || graphUri.Equals(String.Empty))
            {
                this.UpdateGraph((Uri)null, additions, removals);
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
                tID = (this._activeTrans != null) ? this._activeTrans : this.BeginTransaction();

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
                    if (this._activeTrans == null)
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

        #region IStorageServer Members

        public IEnumerable<string> ListStores()
        {
            //GET /admin/databases - application/json
            HttpWebRequest request = this.CreateAdminRequest("databases", "application/json", "GET", new Dictionary<string, string>());

#if DEBUG
            if (Options.HttpDebugging) Tools.HttpDebugRequest(request);
#endif
            try
            {
                List<String> stores = new List<string>();
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging) Tools.HttpDebugResponse(response);
#endif

                    String data = null;
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        data = reader.ReadToEnd();
                    }
                    if (String.IsNullOrEmpty(data)) throw new RdfStorageException("Invalid Empty response from Stardog when listing Stores");

                    JObject obj = JObject.Parse(data);
                    JArray dbs = (JArray)obj["databases"];
                    foreach (JValue db in dbs.OfType<JValue>())
                    {
                        stores.Add(db.Value.ToString());
                    }

                    response.Close();
                }
                return stores;
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpError(webEx, "listing Stores from");
            }
        }

        public IStoreTemplate GetDefaultTemplate(string id)
        {
            return new StardogDiskTemplate(id);
        }

        public IEnumerable<IStoreTemplate> GetAvailableTemplates(string id)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Uses some code based off on answers <a href="http://stackoverflow.com/questions/566462/upload-files-with-httpwebrequest-multipart-form-data">here</a> to help do the multipart form data request.
        /// </para>
        /// </remarks>
        public bool CreateStore(IStoreTemplate template)
        {
            if (template is BaseStardogTemplate)
            {
                //POST /admin/databases
                //Creates a new database; expects a multipart request with a JSON specifying database name, options and filenames followed by (optional) file contents as a multipart POST request.
                try
                {
                    //Get the Template
                    BaseStardogTemplate stardogTemplate = (BaseStardogTemplate)template;
                    IEnumerable<String> errors = stardogTemplate.Validate();
                    if (errors.Any()) throw new RdfStorageException("Template is not valid, call Validate() on the template to see the list of errors");
                    JObject jsonTemplate = stardogTemplate.GetTemplateJson();
                    Console.WriteLine(jsonTemplate.ToString());

                    //Create the request and write the JSON
                    HttpWebRequest request = this.CreateAdminRequest("databases", MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
                    String boundary = StorageHelper.HttpMultipartBoundary;
                    byte[] boundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                    byte[] terminatorBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                    request.ContentType = MimeTypesHelper.FormMultipart + "; boundary=" + boundary;

                    using (Stream stream = request.GetRequestStream())
                    {
                        //Boundary
                        stream.Write(boundaryBytes, 0, boundaryBytes.Length);
                        //Then the root Item
                        String templateItem = String.Format(StorageHelper.HttpMultipartContentTemplate, "root", jsonTemplate.ToString());
                        byte[] itemBytes = System.Text.Encoding.UTF8.GetBytes(templateItem);
                        stream.Write(itemBytes, 0, itemBytes.Length);
                        //Then terminating boundary
                        stream.Write(terminatorBytes, 0, terminatorBytes.Length);
                        stream.Close();
                    }

#if DEBUG
                    if (Options.HttpDebugging) Tools.HttpDebugRequest(request);
#endif

                    //Make the request
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
#if DEBUG
                        if (Options.HttpDebugging) Tools.HttpDebugResponse(response);
#endif

                        //If we get here it completed OK
                        response.Close();
                    }
                    return true;
                }
                catch (WebException webEx)
                {
                    throw StorageHelper.HandleHttpError(webEx, "creating a new Store '" + template.ID + "' in");
                }
            }
            else
            {
                throw new RdfStorageException("Invalid template, templates must derive from BaseStardogTemplate");
            }
        }

        public void DeleteStore(string storeID)
        {
            //DELETE /admin/databases/{db}
            HttpWebRequest request = this.CreateAdminRequest("databases/" + storeID, MimeTypesHelper.Any, "DELETE", new Dictionary<String, String>());

#if DEBUG
            if (Options.HttpDebugging) Tools.HttpDebugRequest(request);
#endif

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging) Tools.HttpDebugResponse(response);
#endif
                    //If we get here then it completed OK
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpError(webEx, "deleting Store " + storeID + " from");
            }
        }

        public IStorageProvider GetStore(string storeID)
        {
            if (this._kb.Equals(storeID)) return this;
#if !NO_PROXY
            return new StardogConnector(this._baseUri, storeID, this._username, this._pwd, this.Proxy);
#else
            return new StardogConnector(this._baseUri, storeID, this._username, this._pwd);
#endif
        }

        #endregion

#endif
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

        private void SaveGraphAsync(IGraph g, AsyncStorageCallback callback, Object state)
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

        private void SaveGraphAsync(String tID, bool autoCommit, IGraph g, AsyncStorageCallback callback, Object state)
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

#if DEBUG
                            if (Options.HttpDebugging)
                            {
                                Tools.HttpDebugRequest(request);
                            }
#endif
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
#if DEBUG
                                    if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                                    if (autoCommit)
                                    {
                                        //If something went wrong try to rollback, don't care what the rollback response is
                                        this.Rollback((sender, args, st) => { }, state);
                                    }
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, new RdfStorageException("A HTTP Error occurred while trying to save a Graph, see inner exception for details", webEx)), state);
                                }
                                catch (Exception ex)
                                {
                                    if (autoCommit)
                                    {
                                        //If something went wrong try to rollback, don't care what the rollback response is
                                        this.Rollback((sender, args, st) => { }, state);
                                    }
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, new RdfStorageException("An unexpected error occurred while trying to save a Graph, see inner exception for details", ex)), state);
                                }
                            }, state);
                        }
                        catch (WebException webEx)
                        {
#if DEBUG
                            if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                            if (autoCommit)
                            {
                                //If something went wrong try to rollback, don't care what the rollback response is
                                this.Rollback((sender, args, st) => { }, state);
                            }
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, new RdfStorageException("A HTTP Error occurred while trying to save a Graph, see inner exception for details", webEx)), state);
                        }
                        catch (Exception ex)
                        {
                            if (autoCommit)
                            {
                                //If something went wrong try to rollback, don't care what the rollback response is
                                this.Rollback((sender, args, st) => { }, state);
                            }
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, new RdfStorageException("An unexpected error occurred while trying to save a Graph, see inner exception for details", ex)), state);
                        }
                    }, state);
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                if (autoCommit)
                {
                    //If something went wrong try to rollback, don't care what the rollback response is
                    this.Rollback((sender, args, st) => { }, state);
                }
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, new RdfStorageException("A HTTP Error occurred while trying to save a Graph, see inner exception for details", webEx)), state);
            }
            catch (Exception ex)
            {
                if (autoCommit)
                {
                    //If something went wrong try to rollback, don't care what the rollback response is
                    this.Rollback((sender, args, st) => { }, state);
                }
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, new RdfStorageException("An unexpected error occurred while trying to save a Graph, see inner exception for details", ex)), state);
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

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                request.BeginGetResponse(r =>
                    {
                        try
                        {
                            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);
#if DEBUG
                            if (Options.HttpDebugging)
                            {
                                Tools.HttpDebugResponse(response);
                            }
#endif
                            IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                            parser.Load(handler, new StreamReader(response.GetResponseStream()));
                            response.Close();

                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler), state);
                        }
                        catch (WebException webEx)
                        {
#if DEBUG
                            if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, new RdfStorageException("A HTTP Error occurred while trying to load a Graph, see inner exception for details", webEx)), state);
                        }
                        catch (Exception ex)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, new RdfStorageException("An unexpected error occurred while trying to load a Graph, see inner exception for details", ex)), state);
                        }
                    }, state);
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, new RdfStorageException("A HTTP Error occurred while trying to load a Graph, see inner exception for details", webEx)), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, new RdfStorageException("An unexpected error occurred while trying to load a Graph, see inner exception for details", ex)), state);
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
            // if there are no adds or deletes, just callback and avoid creating empty transaction

            bool anyData = false;

            if (removals != null)
                if (removals.Any())
                    anyData = true;

            if (additions != null)
                if (additions.Any())
                    anyData = true;

            if (!anyData)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
            }
            else
            {
                this.UpdateGraphAsync(graphUri, additions, removals, callback, state);
            }
        }

        private void UpdateGraphAsync(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, Object state)
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

        private void UpdateGraphAsync(String tID, bool autoCommit, String graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, Object state)
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

#if DEBUG
                            if (Options.HttpDebugging)
                            {
                                Tools.HttpDebugRequest(request);
                            }
#endif
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

#if DEBUG
                                                if (Options.HttpDebugging)
                                                {
                                                    Tools.HttpDebugRequest(request);
                                                }
#endif
                                                request.BeginGetResponse(r4 =>
                                                {
                                                    try
                                                    {
                                                        response = (HttpWebResponse)request.EndGetResponse(r4);
#if DEBUG
                                                        if (Options.HttpDebugging)
                                                        {
                                                            Tools.HttpDebugResponse(response);
                                                        }
#endif
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
#if DEBUG
                                                        if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                                                        if (autoCommit)
                                                        {
                                                            //If something went wrong try to rollback, don't care what the rollback response is
                                                            this.Rollback((sender, args, st) => { }, state);
                                                        }
                                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, new RdfStorageException("A HTTP Error occurred while trying to update a Graph, see inner exception for details", webEx)), state);
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        if (autoCommit)
                                                        {
                                                            //If something went wrong try to rollback, don't care what the rollback response is
                                                            this.Rollback((sender, args, st) => { }, state);
                                                        }
                                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, new RdfStorageException("An unexpected error occurred while trying to update a Graph, see inner exception for details", ex)), state);
                                                    }
                                                }, state);
                                            }
                                            catch (WebException webEx)
                                            {
#if DEBUG
                                                if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                                                if (autoCommit)
                                                {
                                                    //If something went wrong try to rollback, don't care what the rollback response is
                                                    this.Rollback((sender, args, st) => { }, state);
                                                }
                                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, new RdfStorageException("A HTTP Error occurred while trying to update a Graph, see inner exception for details", webEx)), state);
                                            }
                                            catch (Exception ex)
                                            {
                                                if (autoCommit)
                                                {
                                                    //If something went wrong try to rollback, don't care what the rollback response is
                                                    this.Rollback((sender, args, st) => { }, state);
                                                }
                                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, new RdfStorageException("An unexpected error occurred while trying to update a Graph, see inner exception for details", ex)), state);
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
#if DEBUG
                                    if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                                    if (autoCommit)
                                    {
                                        //If something went wrong try to rollback, don't care what the rollback response is
                                        this.Rollback((sender, args, st) => { }, state);
                                    }
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, new RdfStorageException("A HTTP Error occurred while trying to uodate a Graph, see inner exception for details", webEx)), state);
                                }
                                catch (Exception ex)
                                {
                                    if (autoCommit)
                                    {
                                        //If something went wrong try to rollback, don't care what the rollback response is
                                        this.Rollback((sender, args, st) => { }, state);
                                    }
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, new RdfStorageException("An unexpected error occurred while trying to update a Graph, see inner exception for details", ex)), state);
                                }
                            }, state);
                        }
                        catch (WebException webEx)
                        {
#if DEBUG
                            if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                            if (autoCommit)
                            {
                                //If something went wrong try to rollback, don't care what the rollback response is
                                this.Rollback((sender, args, st) => { }, state);
                            }
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, new RdfStorageException("A HTTP Error occurred while trying to update a Graph, see inner exception for details", webEx)), state);
                        }
                        catch (Exception ex)
                        {
                            if (autoCommit)
                            {
                                //If something went wrong try to rollback, don't care what the rollback response is
                                this.Rollback((sender, args, st) => { }, state);
                            }
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, new RdfStorageException("An unexpected error occurred while trying to update a Graph, see inner exception for details", ex)), state);
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

#if DEBUG
                            if (Options.HttpDebugging)
                            {
                                Tools.HttpDebugRequest(request);
                            }
#endif
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
#if DEBUG
                                    if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                                    if (autoCommit)
                                    {
                                        //If something went wrong try to rollback, don't care what the rollback response is
                                        this.Rollback((sender, args, st) => { }, state);
                                    }
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, new RdfStorageException("A HTTP Error occurred while trying to update a Graph, see inner exception for details", webEx)), state);
                                }
                                catch (Exception ex)
                                {
                                    if (autoCommit)
                                    {
                                        //If something went wrong try to rollback, don't care what the rollback response is
                                        this.Rollback((sender, args, st) => { }, state);
                                    }
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, new RdfStorageException("An unexpected error occurred while trying to update a Graph, see inner exception for details", ex)), state);
                                }
                            }, state);
                        }
                        catch (WebException webEx)
                        {
#if DEBUG
                            if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                            if (autoCommit)
                            {
                                //If something went wrong try to rollback, don't care what the rollback response is
                                this.Rollback((sender, args, st) => { }, state);
                            }
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, new RdfStorageException("A HTTP Error occurred while trying to update a Graph, see inner exception for details", webEx)), state);
                        }
                        catch (Exception ex)
                        {
                            if (autoCommit)
                            {
                                //If something went wrong try to rollback, don't care what the rollback response is
                                this.Rollback((sender, args, st) => { }, state);
                            }
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, new RdfStorageException("An unexpected error occurred while trying to update a Graph, see inner exception for details", ex)), state);
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
#if DEBUG
                if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                if (autoCommit)
                {
                    //If something went wrong try to rollback, don't care what the rollback response is
                    this.Rollback((sender, args, st) => { }, state);
                }
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, new RdfStorageException("A HTTP Error occurred while trying to update a Graph, see inner exception for details", webEx)), state);
            }
            catch (Exception ex)
            {
                if (autoCommit)
                {
                    //If something went wrong try to rollback, don't care what the rollback response is
                    this.Rollback((sender, args, st) => { }, state);
                }
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, new RdfStorageException("An unexpected error occurred while trying to update a Graph, see inner exception for details", ex)), state);
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

        private void DeleteGraphAsync(String tID, bool autoCommit, String graphUri, AsyncStorageCallback callback, Object state)
        {
            try
            {
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
                request.BeginGetResponse(r =>
                    {
                        try
                        {
                            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);
#if DEBUG
                            if (Options.HttpDebugging)
                            {
                                Tools.HttpDebugResponse(response);
                            }
#endif
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
#if DEBUG
                            if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                            if (autoCommit)
                            {
                                //If something went wrong try to rollback, don't care what the rollback response is
                                this.Rollback((sender, args, st) => { }, state);
                            }
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), new RdfStorageException("A HTTP Error occurred while trying to delete a Graph, see inner exception for details", webEx)), state);
                        }
                        catch (Exception ex)
                        {
                            if (autoCommit)
                            {
                                //If something went wrong try to rollback, don't care what the rollback response is
                                this.Rollback((sender, args, st) => { }, state);
                            }
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), new RdfStorageException("An unexpected error occurred while trying to delete a Graph, see inner exception for details", ex)), state);
                        }
                    }, state);
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                if (autoCommit)
                {
                    //If something went wrong try to rollback, don't care what the rollback response is
                    this.Rollback((sender, args, st) => { }, state);
                }
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), new RdfStorageException("A HTTP Error occurred while trying to delete a Graph, see inner exception for details", webEx)), state);
            }
            catch (Exception ex)
            {
                if (autoCommit)
                {
                    //If something went wrong try to rollback, don't care what the rollback response is
                    this.Rollback((sender, args, st) => { }, state);
                }
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), new RdfStorageException("An unexpected error occurred while trying to delete a Graph, see inner exception for details", ex)), state);
            }
        }

        /// <summary>
        /// Queries the store asynchronously
        /// </summary>
        /// <param name="query">SPARQL Query</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void Query(String query, AsyncStorageCallback callback, Object state)
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
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String query, AsyncStorageCallback callback, Object state)
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
                request.ContentType = MimeTypesHelper.WWWFormURLEncoded;

                request.BeginGetRequestStream(r =>
                    {
                        try
                        {
                            Stream stream = request.EndGetRequestStream(r);
                            using (StreamWriter writer = new StreamWriter(stream))
                            {
                                writer.Write("query=");
                                writer.Write(HttpUtility.UrlEncode(query));
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
#if DEBUG
                                        if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, new RdfStorageException("A HTTP Error occurred while trying to query the store, see inner exception for details", webEx)), state);
                                    }
                                    catch (Exception ex)
                                    {
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, new RdfStorageException("An unexpected error occurred while trying to query the store, see inner exception for details", ex)), state);
                                    }
                                }, state);
                        }
                        catch (WebException webEx)
                        {
#if DEBUG
                            if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, new RdfStorageException("A HTTP Error occurred while trying to query the store, see inner exception for details", webEx)), state);
                        }
                        catch (Exception ex)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, new RdfStorageException("An unexpected error occurred while trying to query the store, see inner exception for details", ex)), state);
                        }
                    }, state);
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, new RdfStorageException("A HTTP Error occurred while trying to query the store, see inner exception for details", webEx)), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, new RdfStorageException("An unexpected error occurred while trying to query the store, see inner exception for details", ex)), state);
            }
        }

        #region IAsyncStorageServer Members

        public void ListStores(AsyncStorageCallback callback, object state)
        {
            //GET /admin/databases - application/json
            HttpWebRequest request = this.CreateAdminRequest("databases", "application/json", "GET", new Dictionary<string, string>());

#if DEBUG
            if (Options.HttpDebugging) Tools.HttpDebugRequest(request);
#endif
            try
            {
                List<String> stores = new List<string>();
                request.BeginGetResponse(r =>
                    {
                        try
                        {
                            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);
#if DEBUG
                            if (Options.HttpDebugging) Tools.HttpDebugResponse(response);
#endif

                            String data = null;
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                data = reader.ReadToEnd();
                            }
                            if (String.IsNullOrEmpty(data)) throw new RdfStorageException("Invalid Empty response from Stardog when listing Stores");

                            JObject obj = JObject.Parse(data);
                            JArray dbs = (JArray)obj["databases"];
                            foreach (JValue db in dbs.OfType<JValue>())
                            {
                                stores.Add(db.Value.ToString());
                            }

                            response.Close();
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, stores), state);
                        }
                        catch (WebException webEx)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, StorageHelper.HandleHttpError(webEx, "listing Stores asynchronously from")), state);
                        }
                        catch (Exception ex)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, StorageHelper.HandleError(ex, "listing Stores asynchronously from")), state);
                        }
                    }, state);
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, StorageHelper.HandleHttpError(webEx, "listing Stores asynchronously from")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListStores, StorageHelper.HandleError(ex, "listing Stores asynchronously from")), state);
            }
        }

        public void GetDefaultTemplate(string id, AsyncStorageCallback callback, object state)
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.NewTemplate, id, new StardogDiskTemplate(id)), state);
        }

        public void GetAvailableTemplates(string id, AsyncStorageCallback callback, object state)
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

        public void CreateStore(IStoreTemplate template, AsyncStorageCallback callback, object state)
        {
            if (template is BaseStardogTemplate)
            {
                //POST /admin/databases
                //Creates a new database; expects a multipart request with a JSON specifying database name, options and filenames followed by (optional) file contents as a multipart POST request.
                try
                {
                    //Get the Template
                    BaseStardogTemplate stardogTemplate = (BaseStardogTemplate)template;
                    IEnumerable<String> errors = stardogTemplate.Validate();
                    if (errors.Any()) throw new RdfStorageException("Template is not valid, call Validate() on the template to see the list of errors");
                    JObject jsonTemplate = stardogTemplate.GetTemplateJson();
                    Console.WriteLine(jsonTemplate.ToString());

                    //Create the request and write the JSON
                    HttpWebRequest request = this.CreateAdminRequest("databases", MimeTypesHelper.Any, "POST", new Dictionary<string, string>());
                    String boundary = StorageHelper.HttpMultipartBoundary;
                    byte[] boundaryBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                    byte[] terminatorBytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                    request.ContentType = MimeTypesHelper.FormMultipart + "; boundary=" + boundary;

                    request.BeginGetRequestStream(r =>
                        {
                            try
                            {
                                using (Stream stream = request.EndGetRequestStream(r))
                                {
                                    //Boundary
                                    stream.Write(boundaryBytes, 0, boundaryBytes.Length);
                                    //Then the root Item
                                    String templateItem = String.Format(StorageHelper.HttpMultipartContentTemplate, "root", jsonTemplate.ToString());
                                    byte[] itemBytes = System.Text.Encoding.UTF8.GetBytes(templateItem);
                                    stream.Write(itemBytes, 0, itemBytes.Length);
                                    //Then terminating boundary
                                    stream.Write(terminatorBytes, 0, terminatorBytes.Length);
                                    stream.Close();
                                }

#if DEBUG
                                if (Options.HttpDebugging) Tools.HttpDebugRequest(request);
#endif

                                //Make the request
                                request.BeginGetResponse(r2 =>
                                    {
                                        try
                                        {
                                            using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r2))
                                            {
#if DEBUG
                                                if (Options.HttpDebugging) Tools.HttpDebugResponse(response);
#endif
                                                //If we get here it completed OK
                                                response.Close();
                                            }
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID), state);
                                        }
                                        catch (WebException webEx)
                                        {
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, StorageHelper.HandleHttpError(webEx, "creating a new Store '" + template.ID + "' asynchronously in")), state);
                                        }
                                        catch (Exception ex)
                                        {
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, StorageHelper.HandleError(ex, "creating a new Store '" + template.ID + "' asynchronously in")), state);
                                        }
                                    }, state);
                            }
                            catch (WebException webEx)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, StorageHelper.HandleHttpError(webEx, "creating a new Store '" + template.ID + "' asynchronously in")), state);
                            }
                            catch (Exception ex)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, StorageHelper.HandleError(ex, "creating a new Store '" + template.ID + "' asynchronously in")), state);
                            }
                        }, state);
                }
                catch (WebException webEx)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, StorageHelper.HandleHttpError(webEx, "creating a new Store '" + template.ID + "' asynchronously in")), state);
                }
                catch (Exception ex)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, StorageHelper.HandleError(ex, "creating a new Store '" + template.ID + "' asynchronously in")), state);
                }
            }
            else
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.CreateStore, template.ID, new RdfStorageException("Invalid template, templates must derive from BaseStardogTemplate")), state);
            }
        }

        public void DeleteStore(string storeID, AsyncStorageCallback callback, object state)
        {
            //DELETE /admin/databases/{db}
            HttpWebRequest request = this.CreateAdminRequest("databases/" + storeID, MimeTypesHelper.Any, "DELETE", new Dictionary<String, String>());

#if DEBUG
            if (Options.HttpDebugging) Tools.HttpDebugRequest(request);
#endif

            try
            {
                request.BeginGetResponse(r =>
                    {
                        try
                        {
                            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);
#if DEBUG
                            if (Options.HttpDebugging) Tools.HttpDebugResponse(response);
#endif
                            //If we get here then it completed OK
                            response.Close();
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeID), state);
                        }
                        catch (WebException webEx)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeID, StorageHelper.HandleHttpError(webEx, "deleting Store " + storeID + " asynchronously from")), state);
                        }
                        catch (Exception ex)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeID, StorageHelper.HandleError(ex, "deleting Store " + storeID + " asynchronously from")), state);
                        }
                    }, state);
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeID, StorageHelper.HandleHttpError(webEx, "deleting Store " + storeID + " asynchronously from")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteStore, storeID, StorageHelper.HandleError(ex, "deleting Store " + storeID + " asynchronously from")), state);
            }
        }

        public void GetStore(string storeID, AsyncStorageCallback callback, object state)
        {
            if (this._kb.Equals(storeID))
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeID, this), state);
            }
            else
            {
#if !NO_PROXY
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeID, new StardogConnector(this._baseUri, storeID, this._username, this._pwd, this.Proxy)), state);
#else
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.GetStore, storeID, new StardogConnector(this._baseUri, storeID, this._username, this._pwd)), state);
#endif
            }
        }

        #endregion

        #region HTTP Helper Methods



        /// <summary>
        /// Helper method for creating HTTP Requests to the Store
        /// </summary>
        /// <param name="servicePath">Path to the Service requested</param>
        /// <param name="accept">Acceptable Content Types</param>
        /// <param name="method">HTTP Method</param>
        /// <param name="requestParams">Querystring Parameters</param>
        /// <returns></returns>
        private HttpWebRequest CreateRequest(String servicePath, String accept, String method, Dictionary<String, String> requestParams)
        {
            //Build the Request Uri
            String requestUri = this._baseUri + servicePath;
            if (requestParams.Count > 0)
            {
                requestUri += "?";
                foreach (String p in requestParams.Keys)
                {
                    requestUri += p + "=" + Uri.EscapeDataString(requestParams[p]) + "&";
                }
                requestUri = requestUri.Substring(0, requestUri.Length - 1);
            }

            //Create our Request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            //if (accept.EndsWith("*/*;q=0.5")) accept = accept.Substring(0, accept.LastIndexOf(","));
            request.Accept = accept;
            request.Method = method;
            request = base.GetProxiedRequest(request);

            //Add the special Stardog Headers
#if !SILVERLIGHT
            request.Headers.Add("SD-Connection-String", "kb=" + this._kb + this.GetReasoningParameter()); // removed persist=sync, no longer needed in latest stardog versions?
            request.Headers.Add("SD-Protocol", "1.0");
#else
            request.Headers["SD-Connection-String"] = "kb=" + this._kb + this.GetReasoningParameter();
            request.Headers["SD-Protocol"] = "1.0";
#endif

            //Add Credentials if needed
            if (this._hasCredentials)
            {
                NetworkCredential credentials = new NetworkCredential(this._username, this._pwd);
                request.Credentials = credentials;
            }

            return request;
        }

        private HttpWebRequest CreateAdminRequest(String servicePath, String accept, String method, Dictionary<String, String> requestParams)
        {
            //Build the Request Uri
            String requestUri = this._adminUri + servicePath;
            if (requestParams.Count > 0)
            {
                requestUri += "?";
                foreach (String p in requestParams.Keys)
                {
                    requestUri += p + "=" + Uri.EscapeDataString(requestParams[p]) + "&";
                }
                requestUri = requestUri.Substring(0, requestUri.Length - 1);
            }

            //Create our Request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Accept = accept;
            request.Method = method;
            request = base.GetProxiedRequest(request);

            //Add the special Stardog Headers
#if !SILVERLIGHT
            request.Headers.Add("SD-Protocol", "1.0");
#else
            request.Headers["SD-Protocol"] = "1.0";
#endif

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
                case StardogReasoningMode.EL:
                    return ";reasoning=EL";
                case StardogReasoningMode.RL:
                    return ";reasoning=RL";
                case StardogReasoningMode.DL:
                    return ";reasoning=DL";
                case StardogReasoningMode.RDFS:
                    return ";reasoning=RDFS";
                case StardogReasoningMode.None:
                default:
                    return String.Empty;
            }
        }

        #endregion

        #region Stardog Transaction Support

#if !NO_SYNC_HTTP

        private String BeginTransaction()
        {
            String tID = null;

            HttpWebRequest request = this.CreateRequest(this._kb + "/transaction/begin", "text/plain"/*MimeTypesHelper.Any*/, "POST", new Dictionary<string, string>());
            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
            try
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
#if DEBUG
                        if (Options.HttpDebugging)
                        {
                            Tools.HttpDebugResponse(response);
                        }
#endif
                        tID = reader.ReadToEnd();
                        reader.Close();
                    }
                    response.Close();
                }
            }
            catch (Exception ex)
            {
                throw new RdfStorageException("Stardog failed to begin a Transaction, see inner exception for details", ex);
            }

            if (String.IsNullOrEmpty(tID))
            {
                throw new RdfStorageException("Stardog failed to begin a Transaction");
            }
            return tID;
        }

        private void CommitTransaction(String tID)
        {
            HttpWebRequest request = this.CreateRequest(this._kb + "/transaction/commit/" + tID, "text/plain"/* MimeTypesHelper.Any*/, "POST", new Dictionary<string, string>());
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
                response.Close();
            }

            //Reset the Active Transaction on this Thread if the IDs match up
            if (this._activeTrans != null && this._activeTrans.Equals(tID))
            {
                this._activeTrans = null;
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
        public void Begin()
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
        public void Commit()
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
        public void Rollback()
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
        public void Begin(AsyncStorageCallback callback, object state)
        {
            try
            {
                if (this._activeTrans != null)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, new RdfStorageException("Cannot start a new Transaction as there is already an active Transaction")), state);
                }
                else
                {
                    HttpWebRequest request = this.CreateRequest(this._kb + "/transaction/begin", "text/plain"/*MimeTypesHelper.Any*/, "POST", new Dictionary<string, string>());
                    request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                    try
                    {
#if DEBUG
                        if (Options.HttpDebugging)
                        {
                            Tools.HttpDebugRequest(request);
                        }
#endif
                        request.BeginGetResponse(r =>
                        {
                            try
                            {
                                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);
                                String tID;
                                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                                {
#if DEBUG
                                    if (Options.HttpDebugging)
                                    {
                                        Tools.HttpDebugResponse(response);
                                    }
#endif
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
#if DEBUG
                                if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, new RdfStorageException("A HTTP Error occurred while trying to start a Transaction, see inner exception for details", webEx)), state);
                            }
                            catch (Exception ex)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, new RdfStorageException("An unexpected error occurred while trying to start a Transaction, see inner exception for details", ex)), state);
                            }
                        }, state);
                    }
                    catch (WebException webEx)
                    {
#if DEBUG
                        if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, new RdfStorageException("A HTTP Error occurred while trying to start a Transaction, see inner exception for details", webEx)), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, new RdfStorageException("An unexpected error occurred while trying to start a Transaction, see inner exception for details", ex)), state);
                    }
                }
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, new RdfStorageException("A HTTP Error occurred while trying to start a Transaction, see inner exception for details", webEx)), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, new RdfStorageException("An unexpected error occurred while trying to start a Transaction, see inner exception for details", ex)), state);
            }
        }

        /// <summary>
        /// Commits a transaction asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void Commit(AsyncStorageCallback callback, object state)
        {
            try
            {
                if (this._activeTrans == null)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, new RdfStorageException("Cannot commit a Transaction as there is currently no active Transaction")), state);
                }
                else
                {
                    HttpWebRequest request = this.CreateRequest(this._kb + "/transaction/commit/" + this._activeTrans, "text/plain"/* MimeTypesHelper.Any*/, "POST", new Dictionary<string, string>());
                    request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
        #if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugRequest(request);
                    }
        #endif
                    try
                    {
                        request.BeginGetResponse(r =>
                        {
                            try
                            {
                                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);
#if DEBUG
                                if (Options.HttpDebugging)
                                {
                                    Tools.HttpDebugResponse(response);
                                }
#endif
                                response.Close();
                                this._activeTrans = null;
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit), state);
                            }
                            catch (WebException webEx)
                            {
#if DEBUG
                                if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, new RdfStorageException("A HTTP Error occurred while trying to commit a Transaction, see inner exception for details", webEx)), state);
                            }
                            catch (Exception ex)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, new RdfStorageException("An unexpected error occurred while trying to commit a Transaction, see inner exception for details", ex)), state);
                            }
                        }, state);
                    }
                    catch (WebException webEx)
                    {
#if DEBUG
                        if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, new RdfStorageException("A HTTP Error occurred while trying to commit a Transaction, see inner exception for details", webEx)), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, new RdfStorageException("An unexpected error occurred while trying to commit a Transaction, see inner exception for details", ex)), state);
                    }
                }
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, new RdfStorageException("A HTTP Error occurred while trying to commit a Transaction, see inner exception for details", webEx)), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, new RdfStorageException("An unexpected error occurred while trying to commit a Transaction, see inner exception for details", ex)), state);
            }
        }

        /// <summary>
        /// Rolls back a transaction asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void Rollback(AsyncStorageCallback callback, object state)
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
                                    HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r);
                                    response.Close();
                                    this._activeTrans = null;
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback), state);
                                }
                                catch (WebException webEx)
                                {
#if DEBUG
                                    if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback, new RdfStorageException("A HTTP Error occurred while trying to rollback a Transaction, see inner exception for details", webEx)), state);
                                }
                                catch (Exception ex)
                                {
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback, new RdfStorageException("An unexpected error occurred while trying to rollback a Transaction, see inner exception for details", ex)), state);
                                }
                            }, state);
                    }
                    catch (WebException webEx)
                    {
#if DEBUG
                        if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback, new RdfStorageException("A HTTP Error occurred while trying to rollback a Transaction, see inner exception for details", webEx)), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback, new RdfStorageException("An unexpected error occurred while trying to rollback a Transaction, see inner exception for details", ex)), state);
                    }
                }
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (webEx.Response != null && Options.HttpDebugging) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
#endif
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback, new RdfStorageException("A HTTP Error occurred while trying to rollback a Transaction, see inner exception for details", webEx)), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback, new RdfStorageException("An unexpected error occurred while trying to rollback a Transaction, see inner exception for details", ex)), state);
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
        public void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"));
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode genericManager = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassGenericManager);
            INode server = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyServer);
            INode store = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyStore);
            INode loadMode = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyLoadMode);

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
                INode username = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyUser);
                INode pwd = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyPassword);
                context.Graph.Assert(new Triple(manager, username, context.Graph.CreateLiteralNode(this._username)));
                context.Graph.Assert(new Triple(manager, pwd, context.Graph.CreateLiteralNode(this._pwd)));
            }

            base.SerializeProxyConfig(manager, context);
        }

        /// <summary>
        /// Static Class containing constants relevant to provisioning new Stardog stores
        /// </summary>
        public static class DatabaseOptions
        {
            /// <summary>
            /// Constants for valid Stardog Options
            /// </summary>
            public const String Online = "database.online",
                                IcvActiveGraphs = "icv.active.graphs",
                                IcvEnabled = "icv.enabled",
                                IcvReasoningType = "icv.reasoning.type",
                                IndexDifferentialEnableLimit = "index.differential.enable.limit",
                                IndexDifferentialMergeLimit = "index.differential.merge.limit",
                                IndexLiteralsCanonical = "index.literals.canonical",
                                IndexNamedGraphs = "index.named.graphs",
                                IndexPersistTrue = "index.persist.true",
                                IndexPersistSync = "index.persist.sync",
                                IndexStatisticsAutoUpdate = "index.statistics.update.automatic",
                                IndexType = "index.type",
                                ReasoningAutoConsistency = "reasoning.consistency.automatic",
                                ReasoningPunning = "reasoning.punning.enabled",
                                ReasoningSchemaGraphs = "reasoning.schema.graphs",
                                SearchEnabled = "search.enabled",
                                SearchReIndexMode = "search.reindex.mode",
                                TransactionsDurable = "transactions.durable";

            /// <summary>
            /// Constants for valid Stardog Database types
            /// </summary>
            public const String DatabaseTypeDisk = "disk",
                                DatabaseTypeMemory = "memory";

            /// <summary>
            /// Constanst for valid Search Re-Index Modes
            /// </summary>
            public const String SearchReIndexModeSync = "sync",
                                SearchReIndexModeAsync = "async";

            /// <summary>
            /// Constants for special named graph URIs
            /// </summary>
            public const String SpecialNamedGraphDefault = "default",
                                SpecialNamedGraphUnionAll = "*";

            /// <summary>
            /// Constants for various Stardog reasoning settings
            /// </summary>
            public const StardogReasoningMode DefaultIcvReasoningMode = StardogReasoningMode.None;

            /// <summary>
            /// Constant for various Stardog integer settings
            /// </summary>
            public const int DefaultMinDifferentialIndexLimit = 1000000,
                             DefaultMaxDifferentialIndexLimit = 10000;

            /// <summary>
            /// Constants for various Stardog boolean flags
            /// </summary>
            public const bool DefaultCanonicaliseLiterals = true,
                              DefaultNamedGraphIndexing = true,
                              DefaultPersistIndex = false,
                              DefaultPersistIndexSync = true,
                              DefaultAutoUpdateStats = true,
                              DefaultIcvEnabled = false,
                              DefaultConsistencyChecking = false,
                              DefaultPunning = false,
                              DefaultFullTextSearch = false,
                              DefaultDurableTransactions = false;

            /// <summary>
            /// Pattern for valid Stardog database names
            /// </summary>
            public const String ValidDatabaseNamePattern = "^[A-Za-z]{1}[A-Za-z0-9_-]*$";

            /// <summary>
            /// Validates whether a Database Name is valid
            /// </summary>
            /// <param name="name">Database Name</param>
            /// <returns></returns>
            public static bool IsValidDatabaseName(String name)
            {
                return !String.IsNullOrEmpty(name) && Regex.IsMatch(name, ValidDatabaseNamePattern);
            }

            /// <summary>
            /// Validates whether a Database Type is valid
            /// </summary>
            /// <param name="type">Database Type</param>
            /// <returns></returns>
            public static bool IsValidDatabaseType(String type)
            {
                switch (type.ToLower())
                {
                    case DatabaseTypeDisk:
                    case DatabaseTypeMemory:
                        return true;
                    default:
                        return false;
                }
            }

            /// <summary>
            /// Validates whether a Search Re-Index Mode is valid
            /// </summary>
            /// <param name="mode">Mode</param>
            /// <returns></returns>
            public static bool IsValidSearchReIndexMode(String mode)
            {
                switch (mode.ToLower())
                {
                    case SearchReIndexModeAsync:
                    case SearchReIndexModeSync:
                        return true;
                    default:
                        return false;
                }
            }

            /// <summary>
            /// Validates whether a Named Graph URI is valid
            /// </summary>
            /// <param name="uri">URI</param>
            /// <returns></returns>
            public static bool IsValidNamedGraph(String uri)
            {
                if (String.IsNullOrEmpty(uri)) return false;
                if (uri.Equals(SpecialNamedGraphDefault) || uri.Equals(SpecialNamedGraphUnionAll))
                {
                    return true;
                }
                else
                {
                    try
                    {
                        Uri u = new Uri(uri);
                        return u.IsAbsoluteUri;
                    }
                    catch (UriFormatException)
                    {
                        return false;
                    }
                }
            }

            //The legal value of icv.active.graphs is a list of named graph identifiers. See reasoning.schema.graphs below for syntactic sugar URIs for default graph and all named graphs.

            //The legal value of icv.reasoning.type is one of the reasoning levels (i.e, one of the following strings): NONE, RDFS, QL, RL, EL, DL.

            //The legal value of index.differential.* is an integer.

            //The legal value of index.type is the string "disk" or "memory" (case-insensitive).

            //The legal value of reasoning.schema.graphs is a list of named graph identifiers, including (optionally) the special names, tag:stardog:api:context:default and tag:stardog:api:context:all, which represent the default graph and the union of all named graphs and the default graph, respectively. In the context of database configurations only, Stardog will recognize default and * as shorter forms of those URIs, respectively.

            //The legal value of search.reindex.mode is one of the strings sync or async (case insensitive) or a legal Quartz cron expression

            //Config Option	Mutability	Default	API
            //Config Option	Mutability	Default	API
            //database.name	false	{NO DEFAULT}	DatabaseOptions.NAME
            //database.online	false6	true	DatabaseOptions.ONLINE
            //icv.active.graphs	false	default	DatabaseOptions.ICV_ACTIVE_GRAPHS
            //icv.enabled	true	false	DatabaseOptions.ICV_ENABLED
            //icv.reasoning.type	true	NONE	DatabaseOptions.ICV_REASONING_TYPE
            //index.differential.enable.limit	true	1000000	IndexOptions.DIFF_INDEX_MIN_LIMIT
            //index.differential.merge.limit	true	10000	IndexOptions.DIFF_INDEX_MAX_LIMIT
            //index.literals.canonical	false	true	IndexOptions.CANONICAL_LITERALS
            //index.named.graphs	false	true	IndexOptions.INDEX_NAMED_GRAPHS
            //index.persist	true	false	IndexOptions.PERSIST
            //index.persist.sync	true	true	IndexOptions.SYNC
            //index.statistics.update.automatic	true	true	IndexOptions.AUTO_STATS_UPDATE
            //index.type	false	Disk	IndexOptions.INDEX_TYPE
            //reasoning.consistency.automatic	true	false	DatabaseOptions.CONSISTENCY_AUTOMATIC
            //reasoning.punning.enabled	false	false	DatabaseOptions.PUNNING_ENABLED
            //reasoning.schema.graphs	true	default	DatabaseOptions.SCHEMA_GRAPHS
            //search.enabled	false	false	DatabaseOptions.SEARCHABLE
            //search.reindex.mode	false	wait	DatabaseOptions.SEARCH_REINDEX_MODE
            //transactions.durable	true	false	DatabaseOptions.TRANSACTIONS_DURABLE
        }
    }
}