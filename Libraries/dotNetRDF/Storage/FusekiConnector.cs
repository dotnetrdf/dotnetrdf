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
using VDS.RDF.Writing.Formatting;
using System.Web;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Class for connecting to any dataset that can be exposed via Fuseki
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses all three Services provided by a Fuseki instance - Query, Update and HTTP Update
    /// </para>
    /// </remarks>
    public class FusekiConnector 
        : SparqlHttpProtocolConnector, IAsyncUpdateableStorage, IConfigurationSerializable
        , IUpdateableStorage
    {
        private readonly SparqlFormatter _formatter = new SparqlFormatter();
        private readonly String _updateUri;
        private readonly String _queryUri;

        private const String FusekiDefaultGraphUri = "?default";

        /// <summary>
        /// Creates a new connection to a Fuseki Server
        /// </summary>
        /// <param name="serviceUri">The /data URI of the Fuseki Server</param>
        public FusekiConnector(Uri serviceUri)
            : this(serviceUri.ToSafeString()) { }

        /// <summary>
        /// Creates a new connection to a Fuseki Server
        /// </summary>
        /// <param name="serviceUri">The /data URI of the Fuseki Server</param>
        public FusekiConnector(String serviceUri)
            : base(serviceUri) 
        {
            if (!serviceUri.ToString().EndsWith("/data")) throw new ArgumentException("This does not appear to be a valid Fuseki Server URI, you must provide the URI that ends with /data", "serviceUri");

            _updateUri = serviceUri.Substring(0, serviceUri.Length - 4) + "update";
            _queryUri = serviceUri.Substring(0, serviceUri.Length - 4) + "query";
        }

        /// <summary>
        /// Creates a new connection to a Fuseki Server
        /// </summary>
        /// <param name="serviceUri">The /data URI of the Fuseki Server</param>
        /// <param name="proxy">Proxy Server</param>
        public FusekiConnector(Uri serviceUri, IWebProxy proxy)
            : this(serviceUri.ToSafeString(), proxy) { }

        /// <summary>
        /// Creates a new connection to a Fuseki Server
        /// </summary>
        /// <param name="serviceUri">The /data URI of the Fuseki Server</param>
        /// <param name="proxy">Proxy Server</param>
        public FusekiConnector(String serviceUri, IWebProxy proxy)
            : this(serviceUri)
        {
            Proxy = proxy;
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
        /// Gets the IO Behaviour of the Store
        /// </summary>
        public override IOBehaviour IOBehaviour
        {
            get
            {
                return base.IOBehaviour | IOBehaviour.CanUpdateDeleteTriples;
            }
        }

        /// <summary>
        /// Returns that Triple level updates are supported using Fuseki
        /// </summary>
        public override bool UpdateSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the List of Graphs from the store
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Uri> ListGraphs()
        {
            try
            {
                SparqlResultSet results = Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }") as SparqlResultSet;
                if (results != null)
                {
                    List<Uri> uris = new List<Uri>();
                    foreach (SparqlResult r in results)
                    {
                        if (r.HasValue("g"))
                        {
                            INode n = r["g"];
                            if (n != null && n.NodeType == NodeType.Uri)
                            {
                                uris.Add(((IUriNode)n).Uri);
                            }
                        }
                    }
                    return uris;
                }
                else
                {
                    throw new RdfStorageException("Tried to list graphs from Fuseki but failed to get a SPARQL Result Set as expected");
                }
            }
            catch (RdfStorageException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw StorageHelper.HandleError(ex, "listing Graphs from");
            }
        }

        /// <summary>
        /// Updates a Graph in the Fuseki store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public override void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            try
            {
                String graph = (graphUri != null && !graphUri.Equals(String.Empty)) ? "GRAPH <" + _formatter.FormatUri(graphUri) + "> {" : String.Empty;
                StringBuilder update = new StringBuilder();

                if (additions != null)
                {
                    if (additions.Any())
                    {
                        update.AppendLine("INSERT DATA {");
                        if (!graph.Equals(String.Empty)) update.AppendLine(graph);

                        foreach (Triple t in additions)
                        {
                            update.AppendLine(_formatter.Format(t));
                        }

                        if (!graph.Equals(String.Empty)) update.AppendLine("}");
                        update.AppendLine("}");
                    }
                }

                if (removals != null)
                {
                    if (removals.Any())
                    {
                        if (update.Length > 0) update.AppendLine(";");

                        update.AppendLine("DELETE DATA {");
                        if (!graph.Equals(String.Empty)) update.AppendLine(graph);

                        foreach (Triple t in removals)
                        {
                            update.AppendLine(_formatter.Format(t));
                        }

                        if (!graph.Equals(String.Empty)) update.AppendLine("}");
                        update.AppendLine("}");
                    }
                }

                if (update.Length > 0)
                {
                    // Make the SPARQL Update Request
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_updateUri);
                    request.Method = "POST";
                    request.ContentType = "application/sparql-update";
                    request = ApplyRequestOptions(request);

                    Tools.HttpDebugRequest(request);

                    StreamWriter writer = new StreamWriter(request.GetRequestStream());
                    writer.Write(update.ToString());
                    writer.Close();

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        Tools.HttpDebugResponse(response);

                        // If we get here without erroring then the request was OK
                        response.Close();
                    }
                }
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpError(webEx, "updating a Graph in");
            }
        }

        /// <summary>
        /// Updates a Graph in the Fuseki store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public override void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            UpdateGraph(graphUri.ToSafeString(), additions, removals);
        }

        /// <summary>
        /// Executes a SPARQL Query on the Fuseki store
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
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
        /// Executes a SPARQL Query on the Fuseki store processing the results using an appropriate handler from those provided
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

                // Create the Request
                String queryUri = _queryUri;
                if (sparqlQuery.Length < 2048)
                {
                    queryUri += "?query=" + Uri.EscapeDataString(sparqlQuery);
                    request = (HttpWebRequest)WebRequest.Create(queryUri);
                    request.Method = "GET";
                    request.Accept = MimeTypesHelper.HttpRdfOrSparqlAcceptHeader;
                    request = ApplyRequestOptions(request);
                }
                else
                {
                    request = (HttpWebRequest)WebRequest.Create(queryUri);
                    request.Method = "POST";
                    request.Accept = MimeTypesHelper.HttpRdfOrSparqlAcceptHeader;
                    request = ApplyRequestOptions(request);

                    // Build the Post Data and add to the Request Body
                    request.ContentType = MimeTypesHelper.Utf8WWWFormURLEncoded;
                    StringBuilder postData = new StringBuilder();
                    postData.Append("query=");
                    postData.Append(HttpUtility.UrlEncode(sparqlQuery));
                    using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), new UTF8Encoding(Options.UseBomForUtf8)))
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
                    String ctype = response.ContentType;
                    try
                    {
                        // Is the Content Type referring to a RDF format?
                        IRdfReader rdfreader = MimeTypesHelper.GetParser(ctype);
                        rdfreader.Load(rdfHandler, data);
                        response.Close();
                    }
                    catch (RdfParserSelectionException)
                    {
                        // If we get a Parser selection exception then the Content Type isn't valid for a RDF Graph

                        // Is the Content Type referring to a Sparql Result Set format?
                        ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(ctype, true);
                        resreader.Load(resultsHandler, data);
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
        /// Executes SPARQL Updates against the Fuseki store
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update</param>
        public void Update(String sparqlUpdate)
        {
            try
            {
                // Make the SPARQL Update Request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_updateUri);
                request.Method = "POST";
                request.ContentType = "application/sparql-update";
                request = ApplyRequestOptions(request);

                StreamWriter writer = new StreamWriter(request.GetRequestStream());
                writer.Write(sparqlUpdate);
                writer.Close();

                Tools.HttpDebugRequest(request);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);
                    // If we get here without erroring then the request was OK
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpError(webEx, "updating");
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
        /// Executes a SPARQL Query on the Fuseki store processing the results using an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <returns></returns>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery, AsyncStorageCallback callback, Object state)
        {
            try
            {
                HttpWebRequest request;

                // Create the Request, always use POST for async for simplicity
                String queryUri = _queryUri;

                request = (HttpWebRequest)WebRequest.Create(queryUri);
                request.Method = "POST";
                request.Accept = MimeTypesHelper.HttpRdfOrSparqlAcceptHeader;
                request = ApplyRequestOptions(request);

                // Build the Post Data and add to the Request Body
                request.ContentType = MimeTypesHelper.Utf8WWWFormURLEncoded;
                StringBuilder postData = new StringBuilder();
                postData.Append("query=");
                postData.Append(HttpUtility.UrlEncode(sparqlQuery));

                request.BeginGetRequestStream(r =>
                    {
                        try
                        {
                            Stream stream = request.EndGetRequestStream(r);
                            using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(Options.UseBomForUtf8)))
                            {
                                writer.Write(postData);
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
                                    String ctype = response.ContentType;
                                    try
                                    {
                                        // Is the Content Type referring to a Sparql Result Set format?
                                        ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(ctype, true);
                                        resreader.Load(resultsHandler, data);
                                        response.Close();
                                    }
                                    catch (RdfParserSelectionException)
                                    {
                                        // If we get a Parse exception then the Content Type isn't valid for a Sparql Result Set

                                        // Is the Content Type referring to a RDF format?
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
        /// Executes SPARQL Updates against the Fuseki store
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void Update(String sparqlUpdate, AsyncStorageCallback callback, Object state)
        {
            try
            {
                // Make the SPARQL Update Request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_updateUri);
                request.Method = "POST";
                request.ContentType = "application/sparql-update";
                request = ApplyRequestOptions(request);

                request.BeginGetRequestStream(r =>
                    {
                        try
                        {
                            Stream stream = request.EndGetRequestStream(r);
                            StreamWriter writer = new StreamWriter(stream);
                            writer.Write(sparqlUpdate);
                            writer.Close();

                            Tools.HttpDebugRequest(request);

                            request.BeginGetResponse(r2 =>
                                {
                                    try
                                    {
                                        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r2);
                                        Tools.HttpDebugResponse(response);
                                        // If we get here without erroring then the request was OK
                                        response.Close();
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate), state);
                                    }
                                    catch (WebException webEx)
                                    {
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, StorageHelper.HandleHttpError(webEx, "updating")), state);
                                    }
                                    catch (Exception ex)
                                    {
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, StorageHelper.HandleError(ex, "updating")), state);
                                    }
                                }, state);
                        }
                        catch (WebException webEx)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, StorageHelper.HandleHttpError(webEx, "updating")), state);
                        }
                        catch (Exception ex)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, StorageHelper.HandleError(ex, "updating")), state);
                        }
                    }, state);
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, StorageHelper.HandleHttpError(webEx, "updating")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlUpdate, sparqlUpdate, StorageHelper.HandleError(ex, "updating")), state);
            }
        }

        /// <summary>
        /// Lists the graph sin the Store asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public override void ListGraphs(AsyncStorageCallback callback, object state)
        {
            // Use ListUrisHandler and make an async query to list the graphs, when that returns we invoke the correct callback
            ListUrisHandler handler = new ListUrisHandler("g");
            ((IAsyncQueryableStorage)this).Query(null, handler, "SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }", (sender, args, st) =>
            {
                if (args.WasSuccessful)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListGraphs, handler.Uris), state);
                }
                else
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListGraphs, args.Error), state);
                }
            }, state);
        }

        /// <summary>
        /// Updates a Graph on the Fuseki Server
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
                String graph = (graphUri != null && !graphUri.Equals(String.Empty)) ? "GRAPH <" + _formatter.FormatUri(graphUri) + "> {" : String.Empty;
                StringBuilder update = new StringBuilder();

                if (additions != null)
                {
                    if (additions.Any())
                    {
                        update.AppendLine("INSERT DATA {");
                        if (!graph.Equals(String.Empty)) update.AppendLine(graph);

                        foreach (Triple t in additions)
                        {
                            update.AppendLine(_formatter.Format(t));
                        }

                        if (!graph.Equals(String.Empty)) update.AppendLine("}");
                        update.AppendLine("}");
                    }
                }

                if (removals != null)
                {
                    if (removals.Any())
                    {
                        if (update.Length > 0) update.AppendLine(";");

                        update.AppendLine("DELETE DATA {");
                        if (!graph.Equals(String.Empty)) update.AppendLine(graph);

                        foreach (Triple t in removals)
                        {
                            update.AppendLine(_formatter.Format(t));
                        }

                        if (!graph.Equals(String.Empty)) update.AppendLine("}");
                        update.AppendLine("}");
                    }
                }

                if (update.Length > 0)
                {
                    Update(update.ToString(), (sender, args, st) =>
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
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(), StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously")), state);
            }
        }

        /// <summary>
        /// Gets a String which gives details of the Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[Fuseki] " + _serviceUri;
        }

        #region IConfigurationSerializable Members

        /// <summary>
        /// Serializes the connection's configuration
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public override void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"));
            INode dnrType = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType));
            INode genericManager = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassStorageProvider));
            INode server = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyServer));

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(GetType().FullName)));
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(_serviceUri)));
        }

        #endregion
    }
}