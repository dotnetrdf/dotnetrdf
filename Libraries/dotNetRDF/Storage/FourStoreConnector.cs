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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Update;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;
using System.Web;

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
        : BaseAsyncHttpConnector, IAsyncUpdateableStorage, IConfigurationSerializable
        , IUpdateableStorage
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
            // Determine the appropriate actual Base Uri
            if (baseUri.EndsWith("sparql/"))
            {
                _baseUri = baseUri.Substring(0, baseUri.IndexOf("sparql/"));
            }
            else if (baseUri.EndsWith("data/"))
            {
                _baseUri = baseUri.Substring(0, baseUri.IndexOf("data/"));
            }
            else if (!baseUri.EndsWith("/"))
            {
                _baseUri = baseUri + "/";
            }
            else
            {
                _baseUri = baseUri;
            }

            _endpoint = new SparqlRemoteEndpoint(UriFactory.Create(_baseUri + "sparql/"));
            _updateEndpoint = new SparqlRemoteUpdateEndpoint(UriFactory.Create(_baseUri + "update/"));
            _endpoint.Timeout = 60000;
            _updateEndpoint.Timeout = 60000;
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
            _updatesEnabled = enableUpdateSupport;
        }

        /// <summary>
        /// Creates a new 4store connector which manages access to the services provided by a 4store server
        /// </summary>
        /// <param name="baseUri">Base Uri of the 4store</param>
        /// <param name="proxy">Proxy Server</param>
        /// <remarks>
        /// <strong>Note:</strong> As of the 0.4.0 release 4store support defaults to Triple Level updates enabled as all recent 4store releases have supported this.  You can still optionally disable this with the two argument version of the constructor
        /// </remarks>
        public FourStoreConnector(String baseUri, IWebProxy proxy)
            : this(baseUri)
        {
            Proxy = proxy;
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
        public FourStoreConnector(String baseUri, bool enableUpdateSupport, IWebProxy proxy)
            : this(baseUri, enableUpdateSupport)
        {
            Proxy = proxy;
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

        /// <summary>
        /// Loads a Graph from the 4store instance
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Uri of the Graph to load</param>
        public void LoadGraph(IGraph g, Uri graphUri)
        {
            LoadGraph(g, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the 4store instance using an RDF Handler
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            LoadGraph(handler, graphUri.ToSafeString());
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
            LoadGraph(new GraphHandler(g), graphUri);
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
                _endpoint.QueryWithResultGraph(handler, "CONSTRUCT { ?s ?p ?o } FROM <" + graphUri.Replace(">", "\\>") + "> WHERE { ?s ?p ?o }");
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
                // Set up the Request
                HttpWebRequest request;
                if (g.BaseUri != null)
                {
                    request = (HttpWebRequest)WebRequest.Create(_baseUri + "data/" + Uri.EscapeUriString(g.BaseUri.AbsoluteUri));
                }
                else
                {
                    throw new RdfStorageException("Cannot save a Graph without a Base URI to a 4store Server");
                }
                request.Method = "PUT";
                request.ContentType = MimeTypesHelper.Turtle[0];
                request = ApplyRequestOptions(request);

                // Write the Graph as Turtle to the Request Stream
                CompressingTurtleWriter writer = new CompressingTurtleWriter(WriterCompressionLevel.High);
                writer.Save(g, new StreamWriter(request.GetRequestStream()));

                Tools.HttpDebugRequest(request);

                // Make the Request
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);
                    // If we get here then it was OK
                    response.Close();
                }


            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpError(webEx, "saving a Graph to");
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
                UpdateGraph(graphUri.ToString(), additions, removals);
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
            if (!_updatesEnabled)
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
                            // Build up the DELETE command and execute
                            delete.AppendLine("DELETE DATA");
                            delete.AppendLine("{ GRAPH <" + graphUri.Replace(">", "\\>") + "> {");
                            foreach (Triple t in removals)
                            {
                                delete.AppendLine(t.ToString(_formatter));
                            }
                            delete.AppendLine("}}");
                        }
                    }

                    StringBuilder insert = new StringBuilder();
                    if (additions != null)
                    {
                        if (additions.Any())
                        {
                            // Build up the INSERT command and execute
                            insert.AppendLine("INSERT DATA");
                            insert.AppendLine("{ GRAPH <" + graphUri.Replace(">", "\\>") + "> {");
                            foreach (Triple t in additions)
                            {
                                insert.AppendLine(t.ToString(_formatter));
                            }
                            insert.AppendLine("}}");
                        }
                    }

                    // Use Update() method to send the updates
                    if (delete.Length > 0)
                    {
                        if (insert.Length > 0)
                        {
                            Update(delete.ToString() + "\n;\n"  + insert.ToString());
                        }
                        else
                        {
                            Update(delete.ToString());
                        }
                    }
                    else if (insert.Length > 0)
                    {
                        Update(insert.ToString());
                    }
                }
                catch (WebException webEx)
                {
                    throw StorageHelper.HandleHttpError(webEx, "updating a Graph in");
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
        /// Makes a SPARQL Query against the underlying 4store Instance processing the results with the appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery)
        {
            try
            {
                // Ensure Proxy Settings have been taken from the class
                _endpoint.Proxy = Proxy;
                _endpoint.UseCredentialsForProxy = false;
                HttpWebResponse response = _endpoint.QueryRaw(sparqlQuery);
                StreamReader data = new StreamReader(response.GetResponseStream());
                try
                {
                    // Is the Content Type referring to a Sparql Result Set format?
                    ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(response.ContentType);
                    resreader.Load(resultsHandler, data);
                    response.Close();
                }
                catch (RdfParserSelectionException)
                {
                    // If we get a Parser Selection exception then the Content Type isn't valid for a Sparql Result Set

                    // Is the Content Type referring to a RDF format?
                    IRdfReader rdfreader = MimeTypesHelper.GetParser(response.ContentType);
                    rdfreader.Load(rdfHandler, data);
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpQueryError(webEx);
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
                DeleteGraph(graphUri.AbsoluteUri);
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
                // Set up the Request
                HttpWebRequest request;
                if (!graphUri.Equals(String.Empty))
                {
                    request = (HttpWebRequest)WebRequest.Create(_baseUri + "data/" + Uri.EscapeUriString(graphUri));
                }
                else
                {
                    throw new RdfStorageException("Cannot delete a Graph without a Base URI from a 4store Server");
                }
                request.Method = "DELETE";
                request = ApplyRequestOptions(request);

                Tools.HttpDebugRequest(request);

                // Make the Request
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {

                    Tools.HttpDebugResponse(response);
                    response.Close();
                }

                // If we get here then it's OK
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpError(webEx, "deleting a Graph from");
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
                Object results = Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }");
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
        /// Applies a SPARQL Update against 4store
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update</param>
        /// <remarks>
        /// <strong>Note:</strong> Please be aware that some valid SPARQL Updates may not be accepted by 4store since the SPARQL parser used by 4store does not support some of the latest editors draft syntax changes.
        /// </remarks>
        public void Update(String sparqlUpdate)
        {
            _updateEndpoint.Update(sparqlUpdate);
        }

        /// <summary>
        /// Saves a Graph to the Store asynchronously
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public override void SaveGraph(IGraph g, AsyncStorageCallback callback, object state)
        {
            // Set up the Request
            HttpWebRequest request;
            if (g.BaseUri != null)
            {
                request = (HttpWebRequest)WebRequest.Create(_baseUri + "data/" + g.BaseUri.AbsoluteUri);
            }
            else
            {
                throw new RdfStorageException("Cannot save a Graph without a Base URI to a 4store Server");
            }
            request.Method = "PUT";
            request.ContentType = MimeTypesHelper.Turtle[0];
            request = ApplyRequestOptions(request);

            // Write the Graph as Turtle to the Request Stream
            CompressingTurtleWriter writer = new CompressingTurtleWriter(WriterCompressionLevel.High);
            SaveGraphAsync(request, writer, g, callback, state);
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
            if (!graphUri.Equals(String.Empty))
            {
                _endpoint.QueryWithResultGraph(handler, "CONSTRUCT { ?s ?p ?o } FROM <" + graphUri.Replace(">", "\\>") + "> WHERE { ?s ?p ?o }", (rdfH, resH, st) =>
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler), state);
                    }, state);
            }
            else
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, new RdfStorageException("Cannot retrieve a Graph from 4store without specifying a Graph URI")), state);
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
            if (!_updatesEnabled)
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
                            // Build up the DELETE command and execute
                            delete.AppendLine("DELETE DATA");
                            delete.AppendLine("{ GRAPH <" + graphUri.Replace(">", "\\>") + "> {");
                            foreach (Triple t in removals)
                            {
                                delete.AppendLine(t.ToString(_formatter));
                            }
                            delete.AppendLine("}}");
                        }
                    }

                    StringBuilder insert = new StringBuilder();
                    if (additions != null)
                    {
                        if (additions.Any())
                        {
                            // Build up the INSERT command and execute
                            insert.AppendLine("INSERT DATA");
                            insert.AppendLine("{ GRAPH <" + graphUri.Replace(">", "\\>") + "> {");
                            foreach (Triple t in additions)
                            {
                                insert.AppendLine(t.ToString(_formatter));
                            }
                            insert.AppendLine("}}");
                        }
                    }

                    // Use Update() method to send the updates
                    if (delete.Length > 0)
                    {
                        if (insert.Length > 0)
                        {
                            Update(delete.ToString() + "\n;\n" + insert.ToString(), (sender, args, st) =>
                                {
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(), args.Error), state);
                                }, state);
                        }
                        else
                        {
                            Update(delete.ToString(), (sender, args, st) =>
                                {
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(), args.Error), state);
                                }, state);
                        }
                    }
                    else if (insert.Length > 0)
                    {
                        Update(insert.ToString(), (sender, args, st) =>
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(), args.Error), state);
                        }, state);
                    }
                    else
                    {
                        // Nothing to do
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
                    }
                }
                catch (WebException webEx)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(), StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")), state);
                }
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
            // Set up the Request
            HttpWebRequest request;
            if (!graphUri.Equals(String.Empty))
            {
                request = (HttpWebRequest)WebRequest.Create(_baseUri + "data/" + Uri.EscapeUriString(graphUri));
                request.Method = "DELETE";
                request = ApplyRequestOptions(request);
                DeleteGraphAsync(request, false, graphUri, callback, state);
            }
            else
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, new RdfStorageException("Cannot delete a Graph without a Base URI from a 4store Server")), state);
            }
        }

        /// <summary>
        /// Updates the store asynchronously
        /// </summary>
        /// <param name="sparqlUpdates">SPARQL Update</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void Update(string sparqlUpdates, AsyncStorageCallback callback, object state)
        {
            try
            {
                _updateEndpoint.Update(sparqlUpdates, (st) =>
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdates), state);
                    }, state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, new RdfStorageException("Unexpected error while trying to send SPARQL Updates to 4store, see inner exception for details", ex)), state);
            }
        }

        /// <summary>
        /// Queries the store asynchronously
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void Query(string sparqlQuery, AsyncStorageCallback callback, object state)
        {
            Graph g = new Graph();
            SparqlResultSet results = new SparqlResultSet();
            Query(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery, (sender, args, st) =>
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
        /// Queries the store asynchronously
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery, AsyncStorageCallback callback, object state)
        {
            try
            {
                // First off parse the Query to see what kind of query it is
                SparqlQuery q;
                try
                {
                    q = _parser.ParseFromString(sparqlQuery);
                }
                catch (RdfParseException parseEx)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, parseEx), state);
                    return;
                }
                catch (Exception ex)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, new RdfStorageException("An unexpected error occurred while trying to parse the SPARQL Query prior to sending it to the Store, see inner exception for details", ex)), state);
                    return;
                }

                // Now select the Accept Header based on the query type
                String accept = (SparqlSpecsHelper.IsSelectQuery(q.QueryType) || q.QueryType == SparqlQueryType.Ask) ? MimeTypesHelper.HttpSparqlAcceptHeader : MimeTypesHelper.HttpAcceptHeader;

                // Create the Request, for simplicity async requests are always POST
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_endpoint.Uri);
                request.Accept = accept;
                request.Method = "POST";
                request.ContentType = MimeTypesHelper.Utf8WWWFormURLEncoded;
                request = ApplyRequestOptions(request);

                Tools.HttpDebugRequest(request);

                request.BeginGetRequestStream(r =>
                {
                    try
                    {
                        Stream stream = request.EndGetRequestStream(r);
                        using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(Options.UseBomForUtf8)))
                        {
                            writer.Write("query=");
                            writer.Write(HttpUtility.UrlEncode(sparqlQuery));
                            writer.Close();
                        }

                        request.BeginGetResponse(r2 =>
                        {
                            // Get the Response and process based on the Content Type
                            try
                            {
                                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r2);
                                Tools.HttpDebugResponse(response);
                                StreamReader data = new StreamReader(response.GetResponseStream());
                                String ctype = response.ContentType;
                                if (SparqlSpecsHelper.IsSelectQuery(q.QueryType) || q.QueryType == SparqlQueryType.Ask)
                                {
                                    // ASK/SELECT should return SPARQL Results
                                    ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(ctype, q.QueryType == SparqlQueryType.Ask);
                                    resreader.Load(resultsHandler, data);
                                    response.Close();
                                }
                                else
                                {
                                    // CONSTRUCT/DESCRIBE should return a Graph
                                    IRdfReader rdfreader = MimeTypesHelper.GetParser(ctype);
                                    rdfreader.Load(rdfHandler, data);
                                    response.Close();
                                }
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, sparqlQuery, rdfHandler, resultsHandler), state);
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

        /// <summary>
        /// Disposes of a 4store connection
        /// </summary>
        public override void Dispose()
        {
            // No Dispose actions needed
        }

        /// <summary>
        /// Gets a String which gives details of the Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[4store] " + _baseUri;
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
            INode dnrType = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType));
            INode genericManager = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassStorageProvider));
            INode server = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyServer));
            INode enableUpdates = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyEnableUpdates));

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(GetType().FullName)));
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(_baseUri)));
            context.Graph.Assert(new Triple(manager, enableUpdates, _updatesEnabled.ToLiteral(context.Graph)));

            SerializeStandardConfig(manager, context);
        }
    }
}