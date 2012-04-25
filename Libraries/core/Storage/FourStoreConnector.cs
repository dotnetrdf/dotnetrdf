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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
#if !NO_WEB
using System.Web;
#endif
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Class for connecting to 4store
    /// </summary>
    /// <remarks>
    /// <para>
    /// Depending on the version of <a href="http://librdf.org/rasqal/">RASQAL</a> used for your 4store instance and the options it was built with some kinds of queries may not suceed or return unexpected results.
    /// </para>
    /// <para>
    /// Prior to the 1.x releases 4store did not permit the saving of unamed Graphs to the Store or Triple level updates.  There was a branch of 4store that supports Triple level updates and you could tell the connector if your 4store instance supports this when you instantiate it.  From the 0.4.0 release of the library onwards this support was enabled by default since the 1.x builds of 4store have this feature integrated into them by default.
    /// </para>
    /// </remarks>
    public class FourStoreConnector
        : BaseHttpConnector, IAsyncUpdateableStorage, IConfigurationSerializable
#if !NO_SYNC_HTTP
        , IQueryableGenericIOManager, IUpdateableGenericIOManager
#endif
    {
        private String _baseUri;
        private SparqlRemoteEndpoint _endpoint;
        private SparqlRemoteUpdateEndpoint _updateEndpoint;
        private bool _updatesEnabled = true;
        private SparqlFormatter _formatter = new SparqlFormatter();
        private SparqlQueryParser _parser = new SparqlQueryParser();

        /// <summary>
        /// Creates a new 4store connector which manages access to the services provided by a 4store server
        /// </summary>
        /// <param name="baseUri">Base Uri of the 4store</param>
        /// <remarks>
        /// <strong>Note:</strong> As of the 0.4.0 release 4store support defaults to Triple Level updates enabled as all recent 4store releases have supported this.  You can still optionally disable this with the two argument version of the constructor
        /// </remarks>
        public FourStoreConnector(String baseUri)
        {
            //Determine the appropriate actual Base Uri
            if (baseUri.EndsWith("sparql/"))
            {
                this._baseUri = baseUri.Substring(0, baseUri.IndexOf("sparql/"));
            }
            else if (baseUri.EndsWith("data/"))
            {
                this._baseUri = baseUri.Substring(0, baseUri.IndexOf("data/"));
            }
            else if (!baseUri.EndsWith("/"))
            {
                this._baseUri = baseUri + "/";
            }
            else
            {
                this._baseUri = baseUri;
            }

            this._endpoint = new SparqlRemoteEndpoint(UriFactory.Create(this._baseUri + "sparql/"));
            this._updateEndpoint = new SparqlRemoteUpdateEndpoint(UriFactory.Create(this._baseUri + "update/"));
            this._endpoint.Timeout = 60000;
            this._updateEndpoint.Timeout = 60000;
        }

        /// <summary>
        /// Creates a new 4store connector which manages access to the services provided by a 4store server
        /// </summary>
        /// <param name="baseUri">Base Uri of the 4store</param>
        /// <param name="proxy">Proxy Server</param>
        /// <remarks>
        /// <strong>Note:</strong> As of the 0.4.0 release 4store support defaults to Triple Level updates enabled as all recent 4store releases have supported this.  You can still optionally disable this with the two argument version of the constructor
        /// </remarks>
        public FourStoreConnector(String baseUri, WebProxy proxy)
            : this(baseUri)
        {
            this.Proxy = proxy;
        }

        /// <summary>
        /// Creates a new 4store connector which manages access to the services provided by a 4store server
        /// </summary>
        /// <param name="baseUri">Base Uri of the 4store</param>
        /// <param name="enableUpdateSupport">Indicates to the connector that you are using a 4store instance that supports Triple level updates</param>
        /// <remarks>
        /// If you enable Update support but are using a 4store instance that does not support Triple level updates then you will almost certainly experience errors while using the connector.
        /// </remarks>
        public FourStoreConnector(String baseUri, bool enableUpdateSupport)
            : this(baseUri)
        {
            this._updatesEnabled = enableUpdateSupport;
        }

        /// <summary>
        /// Creates a new 4store connector which manages access to the services provided by a 4store server
        /// </summary>
        /// <param name="baseUri">Base Uri of the 4store</param>
        /// <param name="enableUpdateSupport">Indicates to the connector that you are using a 4store instance that supports Triple level updates</param>
        /// <param name="proxy">Proxy Server</param>
        /// <remarks>
        /// If you enable Update support but are using a 4store instance that does not support Triple level updates then you will almost certainly experience errors while using the connector.
        /// </remarks>
        public FourStoreConnector(String baseUri, bool enableUpdateSupport, WebProxy proxy)
            : this(baseUri, enableUpdateSupport)
        {
            this.Proxy = proxy;
        }

        /// <summary>
        /// Returns whether this connector has been instantiated with update support or not
        /// </summary>
        /// <remarks>
        /// If this property returns true it does not guarantee that the 4store instance actually supports updates it simply indicates that the user has enabled updates on the connector.  If Updates are enabled and the 4store server being connected to does not support updates then errors will occur.
        /// </remarks>
        public override bool UpdateSupported
        {
            get
            {
                return false;
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
        /// Gets the IO Behaviour of 4store
        /// </summary>
        public override IOBehaviour IOBehaviour
        {
            get
            {
                return IOBehaviour.IsQuadStore | IOBehaviour.HasNamedGraphs | IOBehaviour.OverwriteNamed | IOBehaviour.CanUpdateTriples;
            }
        }

        /// <summary>
        /// Returns that deleting Graph is supported
        /// </summary>
        public override bool DeleteSupported
        {
            get
            {
                return true;
            }
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

#if !NO_SYNC_HTTP

        /// <summary>
        /// Loads a Graph from the 4store instance
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Uri of the Graph to load</param>
        public void LoadGraph(IGraph g, Uri graphUri)
        {
            this.LoadGraph(g, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the 4store instance using an RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            this.LoadGraph(handler, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the 4store instance
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraph(IGraph g, String graphUri)
        {
            if (g.IsEmpty && graphUri != null & !graphUri.Equals(String.Empty))
            {
                g.BaseUri = UriFactory.Create(graphUri);
            }
            this.LoadGraph(new GraphHandler(g), graphUri);
        }

        /// <summary>
        /// Loads a Graph from the 4store instance
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraph(IRdfHandler handler, String graphUri)
        {
            if (!graphUri.Equals(String.Empty))
            {
                this._endpoint.QueryWithResultGraph(handler, "CONSTRUCT { ?s ?p ?o } FROM <" + graphUri.Replace(">", "\\>") + "> WHERE { ?s ?p ?o }");
            }
            else
            {
                throw new RdfStorageException("Cannot retrieve a Graph from 4store without specifying a Graph URI");
            }
        }

        /// <summary>
        /// Saves a Graph to a 4store instance (Warning: Completely replaces any existing Graph with the same URI)
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <remarks>
        /// <para>
        /// Completely replaces any existing Graph with the same Uri in the store
        /// </para>
        /// <para>
        /// Attempting to save a Graph which doesn't have a Base Uri will result in an error
        /// </para>
        /// </remarks>
        /// <exception cref="RdfStorageException">Thrown if you try and save a Graph without a Base Uri or if there is an error communicating with the 4store instance</exception>
        public void SaveGraph(IGraph g)
        {
            try
            {
                //Set up the Request
                HttpWebRequest request;
                if (g.BaseUri != null)
                {
                    request = (HttpWebRequest)WebRequest.Create(this._baseUri + "data/" + Uri.EscapeUriString(g.BaseUri.ToString()));
                }
                else
                {
                    throw new RdfStorageException("Cannot save a Graph without a Base URI to a 4store Server");
                }
                request.Method = "PUT";
                request.ContentType = MimeTypesHelper.Turtle[0];
                request = base.GetProxiedRequest(request);

                //Write the Graph as Turtle to the Request Stream
                CompressingTurtleWriter writer = new CompressingTurtleWriter(WriterCompressionLevel.High);
                writer.Save(g, new StreamWriter(request.GetRequestStream()));

                #if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugRequest(request);
                    }
                #endif

                //Make the Request
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


            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                throw new RdfStorageException("A HTTP error occurred while communicating with the 4store Server", webEx);
            }
        }

        /// <summary>
        /// Updates a Graph in the store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to Update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <remarks>
        /// May throw an error since the default builds of 4store don't support Triple level updates.  There are builds that do support this and the user can instantiate the connector with support for this enabled if they wish, if they do so and the underlying 4store doesn't support updates errors will occur when updates are attempted.
        /// </remarks>
        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (graphUri == null)
            {
                throw new RdfStorageException("Cannot update a Graph without a Graph URI on a 4store Server");
            }
            else
            {
                this.UpdateGraph(graphUri.ToString(), additions, removals);
            }
        }

        /// <summary>
        /// Updates a Graph in the store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to Update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <remarks>
        /// May throw an error since the default builds of 4store don't support Triple level updates.  There are builds that do support this and the user can instantiate the connector with support for this enabled if they wish, if they do so and the underlying 4store doesn't support updates errors will occur when updates are attempted.
        /// </remarks>
        public void UpdateGraph(String graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (!this._updatesEnabled)
            {
                throw new RdfStorageException("4store does not support Triple level updates");
            }
            else if (graphUri.Equals(String.Empty))
            {
                throw new RdfStorageException("Cannot update a Graph without a Graph URI on a 4store Server");
            }
            else
            {
                try
                {
                    StringBuilder delete = new StringBuilder();
                    if (removals != null)
                    {
                        if (removals.Any())
                        {
                            //Build up the DELETE command and execute
                            delete.AppendLine("DELETE DATA");
                            delete.AppendLine("{ GRAPH <" + graphUri.Replace(">", "\\>") + "> {");
                            foreach (Triple t in removals)
                            {
                                delete.AppendLine(t.ToString(this._formatter));
                            }
                            delete.AppendLine("}}");
                        }
                    }

                    StringBuilder insert = new StringBuilder();
                    if (additions != null)
                    {
                        if (additions.Any())
                        {
                            //Build up the INSERT command and execute
                            insert.AppendLine("INSERT DATA");
                            insert.AppendLine("{ GRAPH <" + graphUri.Replace(">", "\\>") + "> {");
                            foreach (Triple t in additions)
                            {
                                insert.AppendLine(t.ToString(this._formatter));
                            }
                            insert.AppendLine("}}");

                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this._baseUri + "update/");
                            request.Method = "POST";
                            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                            request = base.GetProxiedRequest(request);
                        }
                    }

                    //Use Update() method to send the updates
                    if (delete.Length > 0)
                    {
                        if (insert.Length > 0)
                        {
                            this.Update(delete.ToString() + "\n;\n"  + insert.ToString());
                        }
                        else
                        {
                            this.Update(delete.ToString());
                        }
                    }
                    else if (insert.Length > 0)
                    {
                        this.Update(insert.ToString());
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
                    throw new RdfStorageException("A HTTP error occurred while communicating with the 4store Server", webEx);
                }
            }
        }

        /// <summary>
        /// Makes a SPARQL Query against the underlying 4store Instance
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns>A <see cref="Graph">Graph</see> or a <see cref="SparqlResultSet">SparqlResultSet</see></returns>
        /// <remarks>
        /// Depending on the version of <a href="http://librdf.org/rasqal/">RASQAL</a> used and the options it was built with some kinds of queries may not suceed or return unexpected results.
        /// </remarks>
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
        /// Makes a SPARQL Query against the underlying 4store Instance processing the results with the appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery)
        {
            //Ensure Proxy Settings have been taken from the class
            this._endpoint.Proxy = this.Proxy;
            this._endpoint.UseCredentialsForProxy = false;

            HttpWebResponse response = this._endpoint.QueryRaw(sparqlQuery);
            StreamReader data = new StreamReader(response.GetResponseStream());
            try
            {
                //Is the Content Type referring to a Sparql Result Set format?
                ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(response.ContentType);
                resreader.Load(resultsHandler, data);
                response.Close();
            }
            catch (RdfParserSelectionException)
            {
                //If we get a Parser Selection exception then the Content Type isn't valid for a Sparql Result Set

                //Is the Content Type referring to a RDF format?
                IRdfReader rdfreader = MimeTypesHelper.GetParser(response.ContentType);
                rdfreader.Load(rdfHandler, data);
                response.Close();
            }
        }

        /// <summary>
        /// Deletes a Graph from the 4store server
        /// </summary>
        /// <param name="graphUri">Uri of Graph to delete</param>
        public void DeleteGraph(Uri graphUri)
        {
            if (graphUri == null)
            {
                throw new RdfStorageException("You must specify a valid URI in order to delete a Graph from 4store");
            }
            else
            {
                this.DeleteGraph(graphUri.ToString());
            }
        }

        /// <summary>
        /// Deletes a Graph from the 4store server
        /// </summary>
        /// <param name="graphUri">Uri of Graph to delete</param>
        public void DeleteGraph(String graphUri)
        {
            try
            {
                //Set up the Request
                HttpWebRequest request;
                if (!graphUri.Equals(String.Empty))
                {
                    request = (HttpWebRequest)WebRequest.Create(this._baseUri + "data/" + Uri.EscapeUriString(graphUri));
                }
                else
                {
                    throw new RdfStorageException("Cannot delete a Graph without a Base URI from a 4store Server");
                }
                request.Method = "DELETE";
                request = base.GetProxiedRequest(request);

                #if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugRequest(request);
                    }
                #endif

                //Make the Request
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

                //If we get here then it's OK
            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                throw new RdfStorageException("A HTTP error occurred while communicating with the 4store Server", webEx);
            }
        }

        /// <summary>
        /// Lists the Graphs in the Store
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
                throw new RdfStorageException("Underlying Store returned an error while trying to List Graphs", ex);
            }
        }

        /// <summary>
        /// Applies a SPARQL Update against 4store
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update</param>
        /// <remarks>
        /// <strong>Note:</strong> Please be aware that some valid SPARQL Updates may not be accepted by 4store since the SPARQL parser used by 4store does not support some of the latest editors draft syntax changes.
        /// </remarks>
        public void Update(String sparqlUpdate)
        {
            this._updateEndpoint.Update(sparqlUpdate);
        }

#endif
        public override void SaveGraph(IGraph g, AsyncStorageCallback callback, object state)
        {
                //Set up the Request
                HttpWebRequest request;
                if (g.BaseUri != null)
                {
                    request = (HttpWebRequest)WebRequest.Create(this._baseUri + "data/" + Uri.EscapeUriString(g.BaseUri.ToString()));
                }
                else
                {
                    throw new RdfStorageException("Cannot save a Graph without a Base URI to a 4store Server");
                }
                request.Method = "PUT";
                request.ContentType = MimeTypesHelper.Turtle[0];
                request = base.GetProxiedRequest(request);

                //Write the Graph as Turtle to the Request Stream
                CompressingTurtleWriter writer = new CompressingTurtleWriter(WriterCompressionLevel.High);
                this.SaveGraphAsync(request, writer, g, (sender, args, st) =>
                    {
                        callback(this, args, state);
                    }, state);
        }

        public override void LoadGraph(IRdfHandler handler, string graphUri, AsyncStorageCallback callback, object state)
        {
            if (!graphUri.Equals(String.Empty))
            {
                this._endpoint.QueryWithResultGraph(handler, "CONSTRUCT { ?s ?p ?o } FROM <" + graphUri.Replace(">", "\\>") + "> WHERE { ?s ?p ?o }", (rdfH, resH, st) =>
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.LoadWithHandler, handler), state);
                    }, state);
            }
            else
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.LoadWithHandler, new RdfStorageException("Cannot retrieve a Graph from 4store without specifying a Graph URI")), state);
            }
        }

        public override void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, object state)
        {
            if (!this._updatesEnabled)
            {
                throw new RdfStorageException("4store does not support Triple level updates");
            }
            else if (graphUri.Equals(String.Empty))
            {
                throw new RdfStorageException("Cannot update a Graph without a Graph URI on a 4store Server");
            }
            else
            {
                try
                {
                    StringBuilder delete = new StringBuilder();
                    if (removals != null)
                    {
                        if (removals.Any())
                        {
                            //Build up the DELETE command and execute
                            delete.AppendLine("DELETE DATA");
                            delete.AppendLine("{ GRAPH <" + graphUri.Replace(">", "\\>") + "> {");
                            foreach (Triple t in removals)
                            {
                                delete.AppendLine(t.ToString(this._formatter));
                            }
                            delete.AppendLine("}}");
                        }
                    }

                    StringBuilder insert = new StringBuilder();
                    if (additions != null)
                    {
                        if (additions.Any())
                        {
                            //Build up the INSERT command and execute
                            insert.AppendLine("INSERT DATA");
                            insert.AppendLine("{ GRAPH <" + graphUri.Replace(">", "\\>") + "> {");
                            foreach (Triple t in additions)
                            {
                                insert.AppendLine(t.ToString(this._formatter));
                            }
                            insert.AppendLine("}}");

                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this._baseUri + "update/");
                            request.Method = "POST";
                            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                            request = base.GetProxiedRequest(request);
                        }
                    }

                    //Use Update() method to send the updates
                    if (delete.Length > 0)
                    {
                        if (insert.Length > 0)
                        {
                            this.Update(delete.ToString() + "\n;\n" + insert.ToString(), (sender, args, st) =>
                                {
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.UpdateGraph, graphUri.ToSafeUri(), args.Error), state);
                                }, state);
                        }
                        else
                        {
                            this.Update(delete.ToString(), (sender, args, st) =>
                                {
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.UpdateGraph, graphUri.ToSafeUri(), args.Error), state);
                                }, state);
                        }
                    }
                    else if (insert.Length > 0)
                    {
                        this.Update(insert.ToString(), (sender, args, st) =>
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.UpdateGraph, graphUri.ToSafeUri(), args.Error), state);
                        }, state);
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
                    throw new RdfStorageException("A HTTP error occurred while communicating with the 4store Server", webEx);
                }
            }
        }

        public override void DeleteGraph(string graphUri, AsyncStorageCallback callback, object state)
        {
            //Set up the Request
            HttpWebRequest request;
            if (!graphUri.Equals(String.Empty))
            {
                request = (HttpWebRequest)WebRequest.Create(this._baseUri + "data/" + Uri.EscapeUriString(graphUri));
                request.Method = "DELETE";
                request = base.GetProxiedRequest(request);
                this.DeleteGraphAsync(request, false, graphUri, callback, state);
            }
            else
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.DeleteGraph, new RdfStorageException("Cannot delete a Graph without a Base URI from a 4store Server")), state);
            }
        }

        public void Update(string sparqlUpdates, AsyncStorageCallback callback, object state)
        {
            try
            {
                this._updateEndpoint.Update(sparqlUpdates, (st) =>
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.SparqlUpdate, sparqlUpdates), state);
                    }, state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.SparqlUpdate, new RdfStorageException("Unexpected error while trying to send SPARQL Updates to 4store, see inner exception for details", ex)), state);
            }
        }


        public void Query(string sparqlQuery, AsyncStorageCallback callback, object state)
        {
            Graph g = new Graph();
            SparqlResultSet results = new SparqlResultSet();
            this.Query(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery, (sender, args, st) =>
            {
                if (results.ResultsType != SparqlResultsType.Unknown)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.SparqlQuery, sparqlQuery, results, args.Error), state);
                }
                else
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.SparqlQuery, sparqlQuery, g, args.Error), state);
                }
            }, state);
        }

        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery, AsyncStorageCallback callback, object state)
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
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.SparqlQueryWithHandler, parseEx), state);
                    return;
                }
                catch (Exception ex)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.SparqlQueryWithHandler, new RdfStorageException("An unexpected error occurred while trying to parse the SPARQL Query prior to sending it to the Store, see inner exception for details", ex)), state);
                    return;
                }

                //Now select the Accept Header based on the query type
                String accept = (SparqlSpecsHelper.IsSelectQuery(q.QueryType) || q.QueryType == SparqlQueryType.Ask) ? MimeTypesHelper.HttpSparqlAcceptHeader : MimeTypesHelper.HttpAcceptHeader;

                //Create the Request, for simplicity async requests are always POST
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this._endpoint.Uri);
                request.Accept = accept;
                request.Method = "POST";
                request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                request = base.GetProxiedRequest(request);

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif

                request.BeginGetRequestStream(r =>
                {
                    try
                    {
                        Stream stream = request.EndGetRequestStream(r);
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            writer.Write("query=");
                            writer.Write(HttpUtility.UrlEncode(sparqlQuery));
                            writer.Close();
                        }

                        request.BeginGetResponse(r2 =>
                        {
                            //Get the Response and process based on the Content Type
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
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.SparqlQueryWithHandler, new RdfQueryException("A HTTP error occured while querying the Store.  Store returned the following error message: " + responseText, webEx)), state);
                                        }
                                        catch
                                        {
                                            //Ignore and drop through to the generic error message
                                        }
                                    }
                                }
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.SparqlQueryWithHandler, new RdfQueryException("A HTTP error occurred while querying the Store, see inner exception for details", webEx)), state);
                            }
                            catch (Exception ex)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.SparqlQueryWithHandler, new RdfQueryException("Unexpected error while querying the store, see inner exception for details", ex)), state);
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
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.SparqlQueryWithHandler, new RdfQueryException("A HTTP error occured while querying the Store.  Store returned the following error message: " + responseText, webEx)), state);
                                }
                                catch
                                {
                                    //Ignore and drop through to the generic error message
                                }
                            }
                        }
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.SparqlQueryWithHandler, new RdfQueryException("A HTTP error occurred while querying the Store, see inner exception for details", webEx)), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageAction.SparqlQueryWithHandler, new RdfQueryException("Unexpected error while querying the store, see inner exception for details", ex)), state);
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
        /// Disposes of a 4store connection
        /// </summary>
        public override void Dispose()
        {
            //No Dispose actions needed
        }

        /// <summary>
        /// Gets a String which gives details of the Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[4store] " + this._baseUri;
        }

        /// <summary>
        /// Serializes the connection's configuration
        /// </summary>
        /// <param name="context"></param>
        public void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"));
            INode dnrType = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyType);
            INode genericManager = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.ClassGenericManager);
            INode server = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyServer);
            INode enableUpdates = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyEnableUpdates);

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName)));
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(this._baseUri)));
            context.Graph.Assert(new Triple(manager, enableUpdates, this._updatesEnabled.ToLiteral(context.Graph)));

            base.SerializeProxyConfig(manager, context);
        }
    }
}