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
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing;
using System.Web;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Class for connecting to any store that implements the SPARQL Graph Store HTTP Protocol for Managing Graphs
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <a href="http://www.w3.org/TR/sparql11-http-rdf-update/">SPARQL Graph Store HTTP Protocol</a> is defined as part of SPARQL 1.1 and is currently a working draft so implementations are not guaranteed to be fully compliant with the draft and the protocol may change in the future.
    /// </para>
    /// <para>
    /// <strong>Note:</strong> While this connector supports the update of a Graph the Graph Store HTTP Protocol only allows for the addition of data to an existing Graph and not the removal of data, therefore any calls to <see cref="SparqlHttpProtocolConnector.UpdateGraph(Uri,System.Collections.Generic.IEnumerable{VDS.RDF.Triple},IEnumerable{Triple})">UpdateGraph()</see> that would require the removal of Triple(s) will result in an error.
    /// </para>
    /// </remarks>
    public class SparqlHttpProtocolConnector 
        : BaseAsyncHttpConnector, IConfigurationSerializable, IAsyncStorageProvider, IStorageProvider
    {
        /// <summary>
        /// URI of the Protocol Server
        /// </summary>
        protected String _serviceUri;

        /// <summary>
        /// Creates a new SPARQL Graph Store HTTP Protocol Connector
        /// </summary>
        /// <param name="serviceUri">URI of the Protocol Server</param>
        public SparqlHttpProtocolConnector(String serviceUri)
        {
            if (serviceUri == null) throw new ArgumentNullException("serviceUri", "Cannot create a connection to a Graph Store HTTP Protocol store if the Service URI is null");
            if (serviceUri.Equals(String.Empty)) throw new ArgumentException("Cannot create a connection to a Graph Store HTTP Protocol store if the Service URI is null/empty", "serviceUri");

            _serviceUri = serviceUri;
        }

        /// <summary>
        /// Creates a new SPARQL Graph Store HTTP Protocol Connector
        /// </summary>
        /// <param name="serviceUri">URI of the Protocol Server</param>
        public SparqlHttpProtocolConnector(Uri serviceUri)
            : this(serviceUri.ToSafeString()) { }

        /// <summary>
        /// Creates a new SPARQL Graph Store HTTP Protocol Connector
        /// </summary>
        /// <param name="serviceUri">URI of the Protocol Server</param>
        /// <param name="proxy">Proxy Server</param>
        public SparqlHttpProtocolConnector(String serviceUri, IWebProxy proxy)
            : this(serviceUri)
        {
            Proxy = proxy;
        }

        /// <summary>
        /// Creates a new SPARQL Graph Store HTTP Protocol Connector
        /// </summary>
        /// <param name="serviceUri">URI of the Protocol Server</param>
        /// <param name="proxy">Proxy Server</param>
        public SparqlHttpProtocolConnector(Uri serviceUri, IWebProxy proxy)
            : this(serviceUri.ToSafeString(), proxy) { }

        /// <summary>
        /// Gets the IO Behaviour of SPARQL Graph Store protocol based stores
        /// </summary>
        public override IOBehaviour IOBehaviour
        {
            get
            {
                return IOBehaviour.IsQuadStore | IOBehaviour.HasDefaultGraph | IOBehaviour.HasNamedGraphs | IOBehaviour.OverwriteDefault | IOBehaviour.OverwriteNamed | IOBehaviour.CanUpdateAddTriples;
            }
        }

        /// <summary>
        /// Gets that Updates are supported
        /// </summary>
        public override bool UpdateSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that deleting Graphs is supported
        /// </summary>
        public override bool DeleteSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that listing Graphs is not supported
        /// </summary>
        public override bool ListGraphsSupported
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets that the Store is ready
        /// </summary>
        public override bool IsReady
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets that the Store is not read-only
        /// </summary>
        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Loads a Graph from the Protocol Server
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public virtual void LoadGraph(IGraph g, Uri graphUri)
        {
            LoadGraph(g, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the Protocol Server
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public virtual void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            LoadGraph(handler, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the Protocol Server
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public virtual void LoadGraph(IGraph g, String graphUri)
        {
            Uri origUri = g.BaseUri;
            if (origUri == null && g.IsEmpty && graphUri != null && !graphUri.Equals(String.Empty))
            {
                origUri = UriFactory.Create(graphUri);
            }
            LoadGraph(new GraphHandler(g), graphUri);
            g.BaseUri = origUri;
        }

        /// <summary>
        /// Loads a Graph from the Protocol Server
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public virtual void LoadGraph(IRdfHandler handler, String graphUri)
        {
            String retrievalUri = _serviceUri;
            if (graphUri != null && !graphUri.Equals(String.Empty))
            {
                retrievalUri += "?graph=" + Uri.EscapeDataString(graphUri);
            }
            else
            {
                retrievalUri += "?default";
            }
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(retrievalUri);
                request.Method = "GET";
                request.Accept = MimeTypesHelper.HttpAcceptHeader;
                request = ApplyRequestOptions(request);

                Tools.HttpDebugRequest(request);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);
                    // Parse the retrieved RDF
                    IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                    parser.Load(handler, new StreamReader(response.GetResponseStream()));

                    // If we get here then it was OK
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
                // If the error is a 404 then return
                // Any other error caused the function to throw an error
                if (webEx.Response != null)
                {
                    Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                    if (((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.NotFound) return;
                }
                throw StorageHelper.HandleHttpError(webEx, "loading a Graph from");
            }
        }

        /// <summary>
        /// Sends a HEAD Command to the Protocol Server to determine whether a given Graph exists
        /// </summary>
        /// <param name="graphUri">URI of the Graph to check for</param>
        public virtual bool HasGraph(Uri graphUri)
        {
            return HasGraph(graphUri.ToSafeString());
        }

        /// <summary>
        /// Sends a HEAD Command to the Protocol Server to determine whether a given Graph exists
        /// </summary>
        /// <param name="graphUri">URI of the Graph to check for</param>
        public virtual bool HasGraph(String graphUri)
        {
            String lookupUri = _serviceUri;
            if (graphUri != null && !graphUri.Equals(String.Empty))
            {
                lookupUri += "?graph=" + Uri.EscapeDataString(graphUri);
            }
            else
            {
                lookupUri += "?default";
            }
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(lookupUri);
                request.Method = "HEAD";
                request = ApplyRequestOptions(request);

                Tools.HttpDebugRequest(request);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);
                    // If we get here then it was OK
                    response.Close();
                    return true;
                }
            }
            catch (WebException webEx)
            {
                // If the error is a 404 then return false
                // Any other error caused the function to throw an error
                if (webEx.Response != null)
                {
                    Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                    if (((HttpWebResponse)webEx.Response).StatusCode == HttpStatusCode.NotFound)
                    {
                        return false;
                    }
                }
                throw StorageHelper.HandleHttpError(webEx, "check Graph existence in");
            }
        }

        /// <summary>
        /// Saves a Graph to the Protocol Server
        /// </summary>
        /// <param name="g">Graph to save</param>
        public virtual void SaveGraph(IGraph g)
        {
            String saveUri = _serviceUri;
            if (g.BaseUri != null)
            {
                saveUri += "?graph=" + Uri.EscapeDataString(g.BaseUri.AbsoluteUri);
            }
            else
            {
                saveUri += "?default";
            }
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UriFactory.Create(saveUri));
                request.Method = "PUT";
                request.ContentType = MimeTypesHelper.RdfXml[0];
                request = ApplyRequestOptions(request);

                RdfXmlWriter writer = new RdfXmlWriter();
                writer.Save(g, new StreamWriter(request.GetRequestStream()));

                Tools.HttpDebugRequest(request);

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
        /// Updates a Graph on the Protocol Server
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <remarks>
        /// <strong>Note:</strong> The SPARQL Graph Store HTTP Protocol for Graph Management only supports the addition of Triples to a Graph and does not support removal of Triples from a Graph.  If you attempt to remove Triples then an <see cref="RdfStorageException">RdfStorageException</see> will be thrown
        /// </remarks>
        public virtual void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            UpdateGraph(graphUri.ToSafeString(), additions, removals);
        }

        /// <summary>
        /// Updates a Graph on the Protocol Server
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <remarks>
        /// <strong>Note:</strong> The SPARQL Graph Store HTTP Protocol for Graph Management only supports the addition of Triples to a Graph and does not support removal of Triples from a Graph.  If you attempt to remove Triples then an <see cref="RdfStorageException">RdfStorageException</see> will be thrown
        /// </remarks>
        public virtual void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (removals != null && removals.Any()) throw new RdfStorageException("Unable to Update a Graph since this update requests that Triples be removed from the Graph which the SPARQL Graph Store HTTP Protocol for Graph Management does not support");

            if (additions == null || !additions.Any()) return;

            String updateUri = _serviceUri;
            if (graphUri != null && !graphUri.Equals(String.Empty))
            {
                updateUri += "?graph=" + Uri.EscapeDataString(graphUri);
            }
            else
            {
                updateUri += "?default";
            }

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UriFactory.Create(updateUri));
                request.Method = "POST";
                request.ContentType = MimeTypesHelper.RdfXml[0];
                request = ApplyRequestOptions(request);

                RdfXmlWriter writer = new RdfXmlWriter();
                Graph g = new Graph();
                g.Assert(additions);
                writer.Save(g, new StreamWriter(request.GetRequestStream()));

                Tools.HttpDebugRequest(request);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);
                    // If we get here then it was OK
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpError(webEx, "updating a Graph in");
            }
        }

        /// <summary>
        /// Deletes a Graph from the store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public virtual void DeleteGraph(Uri graphUri)
        {
            DeleteGraph(graphUri.ToSafeString());
        }

        /// <summary>
        /// Deletes a Graph from the store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public virtual void DeleteGraph(String graphUri)
        {
            String deleteUri = _serviceUri;
            if (graphUri != null && !graphUri.Equals(String.Empty))
            {
                deleteUri += "?graph=" + Uri.EscapeDataString(graphUri);
            }
            else
            {
                deleteUri += "?default";
            }

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UriFactory.Create(deleteUri));
                request.Method = "DELETE";
                request = ApplyRequestOptions(request);

                Tools.HttpDebugRequest(request);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);
                    // If we get here then it was OK
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
                // Don't throw the error if we get a 404 - this means we couldn't do a delete as the graph didn't exist to start with
                if (webEx.Response == null || (webEx.Response != null && ((HttpWebResponse)webEx.Response).StatusCode != HttpStatusCode.NotFound))
                {
                    throw StorageHelper.HandleHttpError(webEx, "deleting a Graph from");
                }
            }
        }

        /// <summary>
        /// Throws an exception as listing graphs in a SPARQL Graph Store HTTP Protocol does not support listing graphs
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown since SPARQL Graph Store HTTP Protocol does not support listing graphs</exception>
        public virtual IEnumerable<Uri> ListGraphs()
        {
            throw new NotSupportedException("SPARQL HTTP Protocol Connector does not support listing Graphs");
        }

        /// <summary>
        /// Loads a Graph from the Protocol Server
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public override void LoadGraph(IGraph g, String graphUri, AsyncStorageCallback callback, Object state)
        {
            Uri origUri = g.BaseUri;
            if (origUri == null && g.IsEmpty && graphUri != null && !graphUri.Equals(String.Empty))
            {
                origUri = UriFactory.Create(graphUri);
            }
            LoadGraph(new GraphHandler(g), graphUri, (sender, args, st) =>
                {
                    callback(sender, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadGraph, g, args.Error), st);
                }, state);
            g.BaseUri = origUri;
        }

        /// <summary>
        /// Loads a Graph from the Protocol Server
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public override void LoadGraph(IRdfHandler handler, String graphUri, AsyncStorageCallback callback, Object state)
        {
            String retrievalUri = _serviceUri;
            if (graphUri != null && !graphUri.Equals(String.Empty))
            {
                retrievalUri += "?graph=" + Uri.EscapeDataString(graphUri);
            }
            else
            {
                retrievalUri += "?default";
            }
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(retrievalUri);
            request.Method = "GET";
            request.Accept = MimeTypesHelper.HttpAcceptHeader;
            request = ApplyRequestOptions(request);

            LoadGraphAsync(request, handler, callback, state);
        }

        /// <summary>
        /// Saves a Graph to the Protocol Server
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public override void SaveGraph(IGraph g, AsyncStorageCallback callback, Object state)
        {
            String saveUri = _serviceUri;
            if (g.BaseUri != null)
            {
                saveUri += "?graph=" + Uri.EscapeDataString(g.BaseUri.AbsoluteUri);
            }
            else
            {
                saveUri += "?default";
            }
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UriFactory.Create(saveUri));
            request.Method = "PUT";
            request.ContentType = MimeTypesHelper.RdfXml[0];
            request = ApplyRequestOptions(request);

            SaveGraphAsync(request, new RdfXmlWriter(), g, callback, state);
        }

        /// <summary>
        /// Updates a Graph on the Protocol Server
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <remarks>
        /// <strong>Note:</strong> The SPARQL Graph Store HTTP Protocol for Graph Management only supports the addition of Triples to a Graph and does not support removal of Triples from a Graph.  If you attempt to remove Triples then an <see cref="RdfStorageException">RdfStorageException</see> will be thrown
        /// </remarks>
        public override void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, Object state)
        {
            if (removals != null && removals.Any())
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(), new RdfStorageException("Unable to Update a Graph since this update requests that Triples be removed from the Graph which the SPARQL Graph Store HTTP Protocol for Graph Management does not support")), state);
                return;
            }

            if (additions == null || !additions.Any())
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
                return;
            }

            String updateUri = _serviceUri;
            if (graphUri != null && !graphUri.Equals(String.Empty))
            {
                updateUri += "?graph=" + Uri.EscapeDataString(graphUri);
            }
            else
            {
                updateUri += "?default";
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UriFactory.Create(updateUri));
            request.Method = "POST";
            request.ContentType = MimeTypesHelper.RdfXml[0];
            request = ApplyRequestOptions(request);

            RdfXmlWriter writer = new RdfXmlWriter();

            UpdateGraphAsync(request, writer, graphUri.ToSafeUri(), additions, callback, state);
        }

        /// <summary>
        /// Lists the Graphs in the Store asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public override void ListGraphs(AsyncStorageCallback callback, Object state)
        {
            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.ListGraphs, new NotSupportedException("SPARQL HTTP Protocol Connector does not support listing graphs")), state);
        }
        
        /// <summary>
        /// Deletes a Graph from the store asynchronously
        /// </summary>
        /// <param name="graphUri">URI of the graph to delete</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public override void DeleteGraph(String graphUri, AsyncStorageCallback callback, Object state)
        {
            String deleteUri = _serviceUri;
            if (graphUri != null && !graphUri.Equals(String.Empty))
            {
                deleteUri += "?graph=" + Uri.EscapeDataString(graphUri);
            }
            else
            {
                deleteUri += "?default";
            }

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UriFactory.Create(deleteUri));
                request.Method = "DELETE";
                request = ApplyRequestOptions(request);

                DeleteGraphAsync(request, true, graphUri, callback, state);
            }
            catch (WebException webEx)
            {
                // Don't throw the error if we get a 404 - this means we couldn't do a delete as the graph didn't exist to start with
                if (webEx.Response == null || (webEx.Response != null && ((HttpWebResponse)webEx.Response).StatusCode != HttpStatusCode.NotFound))
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), StorageHelper.HandleHttpError(webEx, "deleting a Graph from")), state);
                }
                else
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri()), state);
                }
            }
        }

        /// <summary>
        /// Disposes of the Connection
        /// </summary>
        public override void Dispose()
        {
            // Nothing to dispose of
        }

        /// <summary>
        /// Gets a String representation of the connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[SPARQL Graph Store HTTP Protocol] " + _serviceUri;
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

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(GetType().FullName)));
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(_serviceUri)));

            SerializeStandardConfig(manager, context);
        }
    }
}