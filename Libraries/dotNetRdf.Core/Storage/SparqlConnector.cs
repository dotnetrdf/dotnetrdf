/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Net.Http;
using System.Threading;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage.Management;

namespace VDS.RDF.Storage;

/// <summary>
/// Class for connecting to any SPARQL Endpoint as a read-only Store.
/// </summary>
/// <remarks>
/// <para>
/// This class is effectively a read-only wrapper around a <see cref="SparqlRemoteEndpoint">SparqlRemoteEndpoint</see> using it with it's default settings, if you only need to query an endpoint and require more control over the settings used to access the endpoint you should use that class directly or use the constructors which allow you to provide your own pre-configure <see cref="SparqlRemoteEndpoint">SparqlRemoteEndpoint</see> instance.
/// </para>
/// <para>
/// Unlike other HTTP based connectors this connector does not derive from <see cref="BaseAsyncHttpConnector">BaseHttpConnector</see> - if you need to specify proxy information you should do so on the SPARQL Endpoint you are wrapping either by providing a <see cref="SparqlRemoteEndpoint">SparqlRemoteEndpoint</see> instance pre-configured with the proxy settings or by accessing the endpoint via the <see cref="SparqlConnector.Endpoint">Endpoint</see> property and programmatically adding the settings.
/// </para>
/// </remarks>
public class SparqlConnector
    : IQueryableStorage, IConfigurationSerializable
{
    /// <summary>
    /// Underlying SPARQL query endpoint.
    /// </summary>
#pragma warning disable 618
    protected SparqlRemoteEndpoint _endpoint;
#pragma warning restore 618

    /// <summary>
    /// Method for loading graphs.
    /// </summary>
    protected SparqlConnectorLoadMethod _mode = SparqlConnectorLoadMethod.Construct;

    /// <summary>
    /// Timeout for endpoints.
    /// </summary>
    protected int _timeout;

    /// <summary>
    /// Creates a new SPARQL Connector which uses the given SPARQL Endpoint.
    /// </summary>
    /// <param name="endpoint">Endpoint.</param>
    [Obsolete("Replaced by SparqlConnector(SparqlQueryClient)")]
    public SparqlConnector(SparqlRemoteEndpoint endpoint)
    {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint), "A valid Endpoint must be specified");
        _timeout = endpoint.Timeout;
    }

    /// <summary>
    /// Creates a new SPARQL Connector which uses the given SPARQL Endpoint.
    /// </summary>
    /// <param name="endpoint">Endpoint.</param>
    /// <param name="mode">Load Method to use.</param>
    [Obsolete("Replaced by SparqlConnector(SparqlQueryClient, SparqlConnectorLoadMethod)")]
    public SparqlConnector(SparqlRemoteEndpoint endpoint, SparqlConnectorLoadMethod mode)
        : this(endpoint)
    {
        _mode = mode;
    }

    /// <summary>
    /// Creates a new SPARQL connector which uses the given SPARQL query client.
    /// </summary>
    /// <param name="client"></param>
    public SparqlConnector(SparqlQueryClient client)
    {
        QueryClient = client ??
                       throw new ArgumentNullException(nameof(client), "A valid query client must be specified");
    }

    /// <summary>
    /// Creates a new SPARQL connector which uses the given SPARQL query client.
    /// </summary>
    /// <param name="client">Query client to use.</param>
    /// <param name="mode">Load method to use.</param>
    public SparqlConnector(SparqlQueryClient client, SparqlConnectorLoadMethod mode) : this(client)
    {
        _mode = mode;
    }
    
    /// <summary>
    /// Creates a new SPARQL Connector which uses the given SPARQL Endpoint.
    /// </summary>
    /// <param name="endpointUri">Endpoint URI.</param>
    [Obsolete("Replaced by SparqlConnector(SparqlQueryClient)")]
    public SparqlConnector(Uri endpointUri)
        : this(new SparqlRemoteEndpoint(endpointUri)) { }

    /// <summary>
    /// Creates a new SPARQL Connector which uses the given SPARQL Endpoint.
    /// </summary>
    /// <param name="endpointUri">Endpoint URI.</param>
    /// <param name="mode">Load Method to use.</param>
    [Obsolete("Replaced by SparqlConnector(SparqlQueryClient, SparqlConnectorLoadMethod)")]
    public SparqlConnector(Uri endpointUri, SparqlConnectorLoadMethod mode)
        : this(new SparqlRemoteEndpoint(endpointUri), mode) { }

    /// <summary>
    /// Gets the parent server (if any).
    /// </summary>
    public IStorageServer ParentServer
    {
        get
        {
            return null;
        }
    }

    /// <summary>
    /// Controls whether the Query will be parsed locally to accurately determine its Query Type for processing the response.
    /// </summary>
    /// <remarks>
    /// If the endpoint you are connecting to provides extensions to SPARQL syntax which are not permitted by the libraries parser then you may wish to enable this option as otherwise you will not be able to execute such queries.
    /// </remarks>
    [Obsolete("This property is no longer supported as local query parsing is not supported by this implementation. Clients wishing to ensure that only valid SPARQL is sent to a remote server should apply query parsing before invoking this class.")]
    [Description("Determines whether queries are parsed locally before being sent to the remote endpoint.  Should be disabled if the remote endpoint supports non-standard extensions that won't parse locally.")]
    public bool SkipLocalParsing
    {
        get
        {
            return true;
        }
        set
        {
            if (value != true)
                throw new ArgumentException("The SparqlConnector class no longer supports local parsing.");
        }
    }

    /// <summary>
    /// Gets/Sets the HTTP Timeout in milliseconds used for communicating with the SPARQL Endpoint.
    /// </summary>
    /// <remarks>This property is only used when using the obsolete <see cref="SparqlRemoteEndpoint"/>-backed implementation. When using the replacement <see cref="SparqlQueryClient"/>-backed implementation, timeout is controlled by the <see cref="HttpClient"/> used by the SparqlQueryClient.</remarks>
    [Obsolete("This property is only used by the obsolete SparqlRemoteEndpoint-backed implementation.")]
    public virtual int Timeout
    {
        get 
        {
            return _timeout;
        }
        set 
        {
            _timeout = value;
            _endpoint.Timeout = value;
        }
    }

    /// <summary>
    /// Gets the underlying <see cref="SparqlRemoteEndpoint">SparqlRemoteEndpoint</see> which this class is a wrapper around.
    /// </summary>
    [Description("The Remote Endpoint to which queries are sent using HTTP."),TypeConverter(typeof(ExpandableObjectConverter))]
    [Obsolete]
    public SparqlRemoteEndpoint Endpoint 
    {
        get 
        {
            return _endpoint;
        }
    }
    
    /// <summary>
    /// Gets the underlying <see cref="SparqlQueryClient"/> which this class is a wrapper around.
    /// </summary>
    public SparqlQueryClient QueryClient { get; }

    /// <summary>
    /// Makes a Query against the SPARQL Endpoint.
    /// </summary>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <returns></returns>
    public object Query(string sparqlQuery)
    {
        var g = new Graph();
        var results = new SparqlResultSet();
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
    /// Makes a Query against the SPARQL Endpoint processing the results with an appropriate handler from those provided.
    /// </summary>
    /// <param name="rdfHandler">RDF Handler.</param>
    /// <param name="resultsHandler">Results Handler.</param>
    /// <param name="sparqlQuery">SPARQL Query.</param>
    /// <returns></returns>
    public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
    {
        if (QueryClient != null)
        {
            QueryWithQueryClient(rdfHandler, resultsHandler, sparqlQuery);
        }
        else
        {
            QueryWithRemoteEndpoint(rdfHandler, resultsHandler, sparqlQuery);
        }
    }
    
    private void QueryWithQueryClient(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
    {
        QueryClient.QueryAsync(sparqlQuery, rdfHandler, resultsHandler, CancellationToken.None).Wait();
    }

    private void QueryWithRemoteEndpoint(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
    {
        // If we're skipping local parsing then we'll need to just make a raw query and process the response
        using (HttpWebResponse response = _endpoint.QueryRaw(sparqlQuery))
        {
            try
            {
                // Is the Content Type referring to a Sparql Result Set format?
                ISparqlResultsReader sparqlParser = MimeTypesHelper.GetSparqlParser(response.ContentType);
                sparqlParser.Load(resultsHandler, new StreamReader(response.GetResponseStream()));
                response.Close();
            }
            catch (RdfParserSelectionException)
            {
                // If we get a Parser Selection exception then the Content Type isn't valid for a Sparql Result Set

                // Is the Content Type referring to a RDF format?
                IRdfReader rdfParser = MimeTypesHelper.GetParser(response.ContentType);
                rdfParser.Load(rdfHandler, new StreamReader(response.GetResponseStream()));
                response.Close();
            }
        }
    }

    /// <summary>
    /// Loads a Graph from the SPARQL Endpoint.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    public virtual void LoadGraph(IGraph g, Uri graphUri)
    {
        LoadGraph(g, graphUri.ToSafeString());
    }

    /// <summary>
    /// Loads a Graph from the SPARQL Endpoint.
    /// </summary>
    /// <param name="handler">RDF Handler.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    public virtual void LoadGraph(IRdfHandler handler, Uri graphUri)
    {
        LoadGraph(handler, graphUri.ToSafeString());
    }

    /// <summary>
    /// Loads a Graph from the SPARQL Endpoint.
    /// </summary>
    /// <param name="g">Graph to load into.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    public virtual void LoadGraph(IGraph g, string graphUri)
    {
        if (g.IsEmpty && graphUri != null && !graphUri.Equals(string.Empty))
        {
            g.BaseUri = g.UriFactory.Create(graphUri);
        }
        LoadGraph(new GraphHandler(g), graphUri.ToSafeString());
    }

    /// <summary>
    /// Loads a Graph from the SPARQL Endpoint.
    /// </summary>
    /// <param name="handler">RDF Handler.</param>
    /// <param name="graphUri">URI of the Graph to load.</param>
    public virtual void LoadGraph(IRdfHandler handler, string graphUri)
    {
        string query;

        if (graphUri.Equals(string.Empty))
        {
            if (_mode == SparqlConnectorLoadMethod.Describe)
            {
                throw new RdfStorageException("Cannot retrieve the Default Graph when the Load Method is Describe");
            }
            else
            {
                query = "CONSTRUCT {?s ?p ?o} WHERE {?s ?p ?o}";
            }
        }
        else
        {
            switch (_mode)
            {
                case SparqlConnectorLoadMethod.Describe:
                    query = "DESCRIBE <" + graphUri.Replace(">", "\\>") + ">";
                    break;
                case SparqlConnectorLoadMethod.Construct:
                default:
                    query = "CONSTRUCT {?s ?p ?o} FROM <" + graphUri.Replace(">", "\\>") + "> WHERE {?s ?p ?o}";
                    break;
            }
        }

        Query(handler, null, query);
    }

    /// <summary>
    /// Throws an error since this Manager is read-only.
    /// </summary>
    /// <param name="g">Graph to save.</param>
    /// <exception cref="RdfStorageException">Always thrown since this Manager provides a read-only connection.</exception>
    public virtual void SaveGraph(IGraph g)
    {
        throw new RdfStorageException("The SparqlConnector provides a read-only connection");

    }

    /// <summary>
    /// Gets the IO Behaviour of SPARQL Connections.
    /// </summary>
    public virtual IOBehaviour IOBehaviour
    {
        get
        {
            return IOBehaviour.ReadOnlyGraphStore;
        }
    }

    /// <summary>
    /// Throws an error since this Manager is read-only.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <param name="additions">Triples to be added.</param>
    /// <param name="removals">Triples to be removed.</param>
    public virtual void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
    {
        throw new RdfStorageException("The SparqlConnector provides a read-only connection");
    }

    /// <summary>
    /// Throws an error since this Manager is read-only.
    /// </summary>
    /// <param name="graphUri">Graph URI.</param>
    /// <param name="additions">Triples to be added.</param>
    /// <param name="removals">Triples to be removed.</param>
    public virtual void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
    {
        throw new RdfStorageException("The SparqlConnector provides a read-only connection");
    }

    /// <summary>
    /// Throws an error since this Manager is read-only.
    /// </summary>
    /// <param name="graphName">Graph name.</param>
    /// <param name="additions">Triples to be added.</param>
    /// <param name="removals">Triples to be removed.</param>
    public virtual void UpdateGraph(IRefNode graphName, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
    {
        throw new RdfStorageException("The SparqlConnector provides a read-only connection");
    }

    /// <summary>
    /// Returns that Updates are not supported since this connection is read-only.
    /// </summary>
    public virtual bool UpdateSupported
    {
        get 
        {
            return false;
        }
    }

    /// <summary>
    /// Throws an exception as this connector provides a read-only connection.
    /// </summary>
    /// <param name="graphUri">URI of this Graph to delete.</param>
    /// <exception cref="RdfStorageException">Thrown since this connection is read-only so you cannot delete graphs using it.</exception>
    public virtual void DeleteGraph(Uri graphUri)
    {
        throw new RdfStorageException("The SparqlConnector provides a read-only connection");
    }

    /// <summary>
    /// Throws an exception as this connector provides a read-only connection.
    /// </summary>
    /// <param name="graphUri">URI of this Graph to delete.</param>
    /// <exception cref="RdfStorageException">Thrown since this connection is read-only so you cannot delete graphs using it.</exception>
    public virtual void DeleteGraph(string graphUri)
    {
        throw new RdfStorageException("The SparqlConnector provides a read-only connection");
    }

    /// <summary>
    /// Returns that deleting graphs is not supported.
    /// </summary>
    public virtual bool DeleteSupported
    {
        get
        {
            return false;
        }
    }

    /// <summary>
    /// Lists the Graphs in the Store.
    /// </summary>
    /// <returns></returns>
    [Obsolete("Replaced by ListGraphNames()")]
    public virtual IEnumerable<Uri> ListGraphs()
    {
        try
        {
            // Technically the ?s ?p ?o is unecessary here but we may not get the right results if we don't include this because some stores
            // won't interpret GRAPH ?g { } correctly
            object results = Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }");
            if (results is SparqlResultSet resultSet)
            {
                var graphs = new List<Uri>();
                foreach (SparqlResult r in resultSet)
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

    /// <inheritdoc/>
    public virtual IEnumerable<string> ListGraphNames()
    {
        try
        {
            // Technically the ?s ?p ?o is unecessary here but we may not get the right results if we don't include this because some stores
            // won't interpret GRAPH ?g { } correctly
            var results = Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }");
            if (results is SparqlResultSet resultSet)
            {
                var graphs = new List<string>();
                foreach (SparqlResult r in resultSet)
                {
                    if (r.HasValue("g"))
                    {
                        INode temp = r["g"];
                        if (temp.NodeType == NodeType.Uri)
                        {
                            graphs.Add(((IUriNode)temp).Uri.AbsoluteUri);
                        }
                        else if (temp.NodeType == NodeType.Blank)
                        {
                            graphs.Add("_:" + ((IBlankNode)temp).InternalID);
                        }
                    }
                }
                return graphs;
            }

            return Enumerable.Empty<string>();
        }
        catch (Exception ex)
        {
            throw StorageHelper.HandleError(ex, "listing Graphs from");
        }
    }
    /// <summary>
    /// Returns that listing graphs is supported.
    /// </summary>
    public virtual bool ListGraphsSupported
    {
        get
        {
            return true;
        }
    }

    /// <summary>
    /// Returns that the Connection is ready.
    /// </summary>
    public virtual bool IsReady
    {
        get 
        {
            return true; 
        }
    }

    /// <summary>
    /// Returns that the Connection is read-only.
    /// </summary>
    public virtual bool IsReadOnly
    {
        get 
        {
            return true; 
        }
    }

    /// <summary>
    /// Disposes of the Connection.
    /// </summary>
    public void Dispose()
    {
        // Nothing to do
    }

    /// <summary>
    /// Gets a String which gives details of the Connection.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "[SPARQL Query] " + _endpoint.Uri.AbsoluteUri;
    }

    /// <summary>
    /// Serializes the connection's configuration.
    /// </summary>
    /// <param name="context">Configuration Serialization Context.</param>
    public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
    {
        INode manager = context.NextSubject;
        INode rdfType = context.Graph.CreateUriNode(context.UriFactory.Create(RdfSpecsHelper.RdfType));
        INode rdfsLabel = context.Graph.CreateUriNode(context.UriFactory.Create(NamespaceMapper.RDFS + "label"));
        INode dnrType = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyType));
        INode genericManager = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.ClassStorageProvider));
        INode loadMode = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyLoadMode));
        INode skipParsing = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertySkipParsing));

        // Basic information
        context.Graph.Assert(new Triple(manager, rdfType, genericManager));
        context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(ToString())));
        context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(GetType().FullName)));

        // Serialize Load Mode
        context.Graph.Assert(new Triple(manager, loadMode, context.Graph.CreateLiteralNode(_mode.ToString())));

        // Query Endpoint
        if (_endpoint != null)
        {
            // Use the indirect serialization method

            // Serialize the Endpoints Configuration
            INode endpoint = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyQueryEndpoint));
            INode endpointObj = context.Graph.CreateBlankNode();
            context.NextSubject = endpointObj;
            ((IConfigurationSerializable)_endpoint).SerializeConfiguration(context);

            // Link that serialization to our serialization
            context.Graph.Assert(new Triple(manager, endpoint, endpointObj));
        }
        else if (QueryClient != null)
        {
            // Use the indirect serialization method

            // Serialize the Endpoints Configuration
            INode endpoint = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyQueryEndpoint));
            INode endpointObj = context.Graph.CreateBlankNode();
            context.NextSubject = endpointObj;
            ((IConfigurationSerializable)QueryClient).SerializeConfiguration(context);

            // Link that serialization to our serialization
            context.Graph.Assert(new Triple(manager, endpoint, endpointObj));
        }
        else
        {
            // Use the direct serialization method
            INode endpointUri = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyQueryEndpointUri));
            INode defGraphUri = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyDefaultGraphUri));
            INode namedGraphUri = context.Graph.CreateUriNode(context.UriFactory.Create(ConfigurationLoader.PropertyNamedGraphUri));

            if (_endpoint != null)
            {
                context.Graph.Assert(new Triple(manager, endpointUri, context.Graph.CreateLiteralNode(_endpoint.Uri.AbsoluteUri)));
                foreach (var u in _endpoint.DefaultGraphs)
                {
                    context.Graph.Assert(new Triple(manager, defGraphUri, context.Graph.CreateUriNode(context.UriFactory.Create(u))));
                }
                foreach (var u in _endpoint.NamedGraphs)
                {
                    context.Graph.Assert(new Triple(manager, namedGraphUri, context.Graph.CreateUriNode(context.UriFactory.Create(u))));
                }
            } else if (QueryClient != null)
            {
                context.Graph.Assert(new Triple(manager, endpointUri, context.Graph.CreateLiteralNode(QueryClient.EndpointUri.AbsoluteUri)));
                foreach (var u in QueryClient.DefaultGraphs)
                {
                    context.Graph.Assert(new Triple(manager, defGraphUri, context.Graph.CreateUriNode(context.UriFactory.Create(u))));
                }
                foreach (var u in QueryClient.NamedGraphs)
                {
                    context.Graph.Assert(new Triple(manager, namedGraphUri, context.Graph.CreateUriNode(context.UriFactory.Create(u))));
                }
            }
        }
    }
}
