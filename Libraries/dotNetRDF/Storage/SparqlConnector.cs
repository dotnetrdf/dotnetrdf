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
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage.Management;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Controls how the <see cref="SparqlConnector">SparqlConnector</see> loads Graphs from the Endpoint
    /// </summary>
    public enum SparqlConnectorLoadMethod 
    {
        /// <summary>
        /// Graphs are loaded by issuing a DESCRIBE query using the Graph URI
        /// </summary>
        Describe,
        /// <summary>
        /// Graphs are loaded by issuing a CONSTRUCT FROM query using the Graph URI
        /// </summary>
        Construct
    }

    /// <summary>
    /// Class for connecting to any SPARQL Endpoint as a read-only Store
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is effectively a read-only wrapper around a <see cref="SparqlRemoteEndpoint">SparqlRemoteEndpoint</see> using it with it's default settings, if you only need to query an endpoint and require more control over the settings used to access the endpoint you should use that class directly or use the constructors which allow you to provide your own pre-configure <see cref="SparqlRemoteEndpoint">SparqlRemoteEndpoint</see> instance
    /// </para>
    /// <para>
    /// Unlike other HTTP based connectors this connector does not derive from <see cref="BaseAsyncHttpConnector">BaseHttpConnector</see> - if you need to specify proxy information you should do so on the SPARQL Endpoint you are wrapping either by providing a <see cref="SparqlRemoteEndpoint">SparqlRemoteEndpoint</see> instance pre-configured with the proxy settings or by accessing the endpoint via the <see cref="SparqlConnector.Endpoint">Endpoint</see> property and programmatically adding the settings.
    /// </para>
    /// </remarks>
    public class SparqlConnector
        : IQueryableStorage, IConfigurationSerializable
    {
        /// <summary>
        /// Underlying SPARQL query endpoint
        /// </summary>
        protected SparqlRemoteEndpoint _endpoint;
        /// <summary>
        /// Method for loading graphs
        /// </summary>
        protected SparqlConnectorLoadMethod _mode = SparqlConnectorLoadMethod.Construct;
        /// <summary>
        /// Whether to skip local parsing
        /// </summary>
        protected bool _skipLocalParsing = false;
        /// <summary>
        /// Timeout for endpoints
        /// </summary>
        protected int _timeout;

        /// <summary>
        /// Creates a new SPARQL Connector which uses the given SPARQL Endpoint
        /// </summary>
        /// <param name="endpoint">Endpoint</param>
        public SparqlConnector(SparqlRemoteEndpoint endpoint)
        {
            if (endpoint == null) throw new ArgumentNullException("endpoint", "A valid Endpoint must be specified");
            _endpoint = endpoint;
            _timeout = endpoint.Timeout;
        }

        /// <summary>
        /// Creates a new SPARQL Connector which uses the given SPARQL Endpoint
        /// </summary>
        /// <param name="endpoint">Endpoint</param>
        /// <param name="mode">Load Method to use</param>
        public SparqlConnector(SparqlRemoteEndpoint endpoint, SparqlConnectorLoadMethod mode)
            : this(endpoint)
        {
            _mode = mode;
        }

        /// <summary>
        /// Creates a new SPARQL Connector which uses the given SPARQL Endpoint
        /// </summary>
        /// <param name="endpointUri">Endpoint URI</param>
        public SparqlConnector(Uri endpointUri)
            : this(new SparqlRemoteEndpoint(endpointUri)) { }

        /// <summary>
        /// Creates a new SPARQL Connector which uses the given SPARQL Endpoint
        /// </summary>
        /// <param name="endpointUri">Endpoint URI</param>
        /// <param name="mode">Load Method to use</param>
        public SparqlConnector(Uri endpointUri, SparqlConnectorLoadMethod mode)
            : this(new SparqlRemoteEndpoint(endpointUri), mode) { }

        /// <summary>
        /// Gets the parent server (if any)
        /// </summary>
        public IStorageServer ParentServer
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Controls whether the Query will be parsed locally to accurately determine its Query Type for processing the response
        /// </summary>
        /// <remarks>
        /// If the endpoint you are connecting to provides extensions to SPARQL syntax which are not permitted by the libraries parser then you may wish to enable this option as otherwise you will not be able to execute such queries
        /// </remarks>
        [Description("Determines whether queries are parsed locally before being sent to the remote endpoint.  Should be disabled if the remote endpoint supports non-standard extensions that won't parse locally.")]
        public bool SkipLocalParsing
        {
            get
            {
                return _skipLocalParsing;
            }
            set
            {
                _skipLocalParsing = value;
            }
        }

        /// <summary>
        /// Gets/Sets the HTTP Timeout in milliseconds used for communicating with the SPARQL Endpoint
        /// </summary>
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
        /// Gets the underlying <see cref="SparqlRemoteEndpoint">SparqlRemoteEndpoint</see> which this class is a wrapper around
        /// </summary>
        [Description("The Remote Endpoint to which queries are sent using HTTP."),TypeConverter(typeof(ExpandableObjectConverter))]
        public SparqlRemoteEndpoint Endpoint 
        {
            get 
            {
                return _endpoint;
            }
        }

        /// <summary>
        /// Makes a Query against the SPARQL Endpoint
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public object Query(String sparqlQuery)
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
        /// Makes a Query against the SPARQL Endpoint processing the results with an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery)
        {
            if (!_skipLocalParsing)
            {
                // Parse the query locally to validate it and so we can decide what to do
                // when we receive the Response more easily as we'll know the query type
                // This also saves us wasting a HttpWebRequest on a malformed query
                SparqlQueryParser qparser = new SparqlQueryParser();
                SparqlQuery q = qparser.ParseFromString(sparqlQuery);

                switch (q.QueryType)
                {
                    case SparqlQueryType.Ask:
                    case SparqlQueryType.Select:
                    case SparqlQueryType.SelectAll:
                    case SparqlQueryType.SelectAllDistinct:
                    case SparqlQueryType.SelectAllReduced:
                    case SparqlQueryType.SelectDistinct:
                    case SparqlQueryType.SelectReduced:
                        // Some kind of Sparql Result Set
                        _endpoint.QueryWithResultSet(resultsHandler, sparqlQuery);
                        break;

                    case SparqlQueryType.Construct:
                    case SparqlQueryType.Describe:
                    case SparqlQueryType.DescribeAll:
                        // Some kind of Graph
                        _endpoint.QueryWithResultGraph(rdfHandler, sparqlQuery);
                        break;

                    case SparqlQueryType.Unknown:
                    default:
                        // Error
                        throw new RdfQueryException("Unknown Query Type was used, unable to determine how to process the response");
                }
            }
            else
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
        }

        /// <summary>
        /// Loads a Graph from the SPARQL Endpoint
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public virtual void LoadGraph(IGraph g, Uri graphUri)
        {
            LoadGraph(g, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the SPARQL Endpoint
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public virtual void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            LoadGraph(handler, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the SPARQL Endpoint
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public virtual void LoadGraph(IGraph g, String graphUri)
        {
            if (g.IsEmpty && graphUri != null && !graphUri.Equals(String.Empty))
            {
                g.BaseUri = UriFactory.Create(graphUri);
            }
            LoadGraph(new GraphHandler(g), graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the SPARQL Endpoint
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public virtual void LoadGraph(IRdfHandler handler, String graphUri)
        {
            String query;

            if (graphUri.Equals(String.Empty))
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

            _endpoint.QueryWithResultGraph(handler, query);
        }

        /// <summary>
        /// Throws an error since this Manager is read-only
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <exception cref="RdfStorageException">Always thrown since this Manager provides a read-only connection</exception>
        public virtual void SaveGraph(IGraph g)
        {
            throw new RdfStorageException("The SparqlConnector provides a read-only connection");

        }

        /// <summary>
        /// Gets the IO Behaviour of SPARQL Connections
        /// </summary>
        public virtual IOBehaviour IOBehaviour
        {
            get
            {
                return IOBehaviour.ReadOnlyGraphStore;
            }
        }

        /// <summary>
        /// Throws an error since this Manager is read-only
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public virtual void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new RdfStorageException("The SparqlConnector provides a read-only connection");
        }

        /// <summary>
        /// Throws an error since this Manager is read-only
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public virtual void UpdateGraph(String graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new RdfStorageException("The SparqlConnector provides a read-only connection");
        }

        /// <summary>
        /// Returns that Updates are not supported since this connection is read-only
        /// </summary>
        public virtual bool UpdateSupported
        {
            get 
            {
                return false;
            }
        }

        /// <summary>
        /// Throws an exception as this connector provides a read-only connection
        /// </summary>
        /// <param name="graphUri">URI of this Graph to delete</param>
        /// <exception cref="RdfStorageException">Thrown since this connection is read-only so you cannot delete graphs using it</exception>
        public virtual void DeleteGraph(Uri graphUri)
        {
            throw new RdfStorageException("The SparqlConnector provides a read-only connection");
        }

        /// <summary>
        /// Throws an exception as this connector provides a read-only connection
        /// </summary>
        /// <param name="graphUri">URI of this Graph to delete</param>
        /// <exception cref="RdfStorageException">Thrown since this connection is read-only so you cannot delete graphs using it</exception>
        public virtual void DeleteGraph(String graphUri)
        {
            throw new RdfStorageException("The SparqlConnector provides a read-only connection");
        }

        /// <summary>
        /// Returns that deleting graphs is not supported
        /// </summary>
        public virtual bool DeleteSupported
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Lists the Graphs in the Store
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<Uri> ListGraphs()
        {
            try
            {
                // Technically the ?s ?p ?o is unecessary here but we may not get the right results if we don't include this because some stores
                // won't interpret GRAPH ?g { } correctly
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
        /// Returns that listing graphs is supported
        /// </summary>
        public virtual bool ListGraphsSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns that the Connection is ready
        /// </summary>
        public virtual bool IsReady
        {
            get 
            {
                return true; 
            }
        }

        /// <summary>
        /// Returns that the Connection is read-only
        /// </summary>
        public virtual bool IsReadOnly
        {
            get 
            {
                return true; 
            }
        }

        /// <summary>
        /// Disposes of the Connection
        /// </summary>
        public void Dispose()
        {
            // Nothing to do
        }

        /// <summary>
        /// Gets a String which gives details of the Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[SPARQL Query] " + _endpoint.Uri.AbsoluteUri;
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
            INode loadMode = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyLoadMode));
            INode skipParsing = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertySkipParsing));

            // Basic information
            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(GetType().FullName)));

            // Serialize Load Mode
            context.Graph.Assert(new Triple(manager, loadMode, context.Graph.CreateLiteralNode(_mode.ToString())));
            context.Graph.Assert(new Triple(manager, skipParsing, _skipLocalParsing.ToLiteral(context.Graph)));

            // Query Endpoint
            if (_endpoint is IConfigurationSerializable)
            {
                // Use the indirect serialization method

                // Serialize the Endpoints Configuration
                INode endpoint = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyQueryEndpoint));
                INode endpointObj = context.Graph.CreateBlankNode();
                context.NextSubject = endpointObj;
                ((IConfigurationSerializable)_endpoint).SerializeConfiguration(context);

                // Link that serialization to our serialization
                context.Graph.Assert(new Triple(manager, endpoint, endpointObj));
            }
            else
            {
                // Use the direct serialization method
                INode endpointUri = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyQueryEndpointUri));
                INode defGraphUri = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyDefaultGraphUri));
                INode namedGraphUri = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyNamedGraphUri));
                
                context.Graph.Assert(new Triple(manager, endpointUri, context.Graph.CreateLiteralNode(_endpoint.Uri.AbsoluteUri)));
                foreach (String u in _endpoint.DefaultGraphs)
                {
                    context.Graph.Assert(new Triple(manager, defGraphUri, context.Graph.CreateUriNode(UriFactory.Create(u))));
                }
                foreach (String u in _endpoint.NamedGraphs)
                {
                    context.Graph.Assert(new Triple(manager, namedGraphUri, context.Graph.CreateUriNode(UriFactory.Create(u))));
                }
            }
        }
    }

    /// <summary>
    /// Class for connecting to any SPARQL server that provides both a query and update endpoint
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is a wrapper around a <see cref="SparqlRemoteEndpoint"/> and a <see cref="SparqlRemoteUpdateEndpoint"/>.  The former is used for the query functionality while the latter is used for the update functionality.  As updates happen via SPARQL the behaviour with respects to adding and removing blank nodes will be somewhat up to the underlying SPARQL implementation.  This connector is <strong>not</strong> able to carry out <see cref="IStorageProvider.UpdateGraph(Uri,IEnumerable{Triple},IEnumerable{Triple})"/> operations which attempt to delete blank nodes and cannot guarantee that added blank nodes bear any relation to existing blank nodes in the store.
    /// </para>
    /// <para>
    /// Unlike other HTTP based connectors this connector does not derive from <see cref="BaseAsyncHttpConnector">BaseHttpConnector</see> - if you need to specify proxy information you should do so on the SPARQL Endpoint you are wrapping either by providing endpoint instance pre-configured with the proxy settings or by accessing the endpoint via the <see cref="ReadWriteSparqlConnector">Endpoint</see> and <see cref="ReadWriteSparqlConnector.UpdateEndpoint">UpdateEndpoint</see> properties and programmatically adding the settings.
    /// </para>
    /// </remarks>
    public class ReadWriteSparqlConnector
        : SparqlConnector, IUpdateableStorage
    {
        private readonly SparqlFormatter _formatter = new SparqlFormatter();
        private readonly SparqlRemoteUpdateEndpoint _updateEndpoint;

        /// <summary>
        /// Creates a new connection
        /// </summary>
        /// <param name="queryEndpoint">Query Endpoint</param>
        /// <param name="updateEndpoint">Update Endpoint</param>
        /// <param name="mode">Method for loading graphs</param>
        public ReadWriteSparqlConnector(SparqlRemoteEndpoint queryEndpoint, SparqlRemoteUpdateEndpoint updateEndpoint, SparqlConnectorLoadMethod mode)
            : base(queryEndpoint, mode)
        {
            _updateEndpoint = updateEndpoint ?? throw new ArgumentNullException(nameof(updateEndpoint), "Update Endpoint cannot be null, if you require a read-only SPARQL connector use the base class SparqlConnector instead");
        }

        /// <summary>
        /// Creates a new connection
        /// </summary>
        /// <param name="queryEndpoint">Query Endpoint</param>
        /// <param name="updateEndpoint">Update Endpoint</param>
        public ReadWriteSparqlConnector(SparqlRemoteEndpoint queryEndpoint, SparqlRemoteUpdateEndpoint updateEndpoint)
            : this(queryEndpoint, updateEndpoint, SparqlConnectorLoadMethod.Construct) { }

        /// <summary>
        /// Creates a new connection
        /// </summary>
        /// <param name="queryEndpoint">Query Endpoint</param>
        /// <param name="updateEndpoint">Update Endpoint</param>
        /// <param name="mode">Method for loading graphs</param>
        public ReadWriteSparqlConnector(Uri queryEndpoint, Uri updateEndpoint, SparqlConnectorLoadMethod mode)
            : this(new SparqlRemoteEndpoint(queryEndpoint), new SparqlRemoteUpdateEndpoint(updateEndpoint), mode) { }

        /// <summary>
        /// Creates a new connection
        /// </summary>
        /// <param name="queryEndpoint">Query Endpoint</param>
        /// <param name="updateEndpoint">Update Endpoint</param>
        public ReadWriteSparqlConnector(Uri queryEndpoint, Uri updateEndpoint)
            : this(queryEndpoint, updateEndpoint, SparqlConnectorLoadMethod.Construct) { }

        /// <summary>
        /// Gets the underlying <see cref="SparqlRemoteUpdateEndpoint">SparqlRemoteUpdateEndpoint</see> which this class is a wrapper around
        /// </summary>
        [Description("The Remote Update Endpoint to which queries are sent using HTTP."), TypeConverter(typeof(ExpandableObjectConverter))]
        public SparqlRemoteUpdateEndpoint UpdateEndpoint
        {
            get
            {
                return _updateEndpoint;
            }
        }

        /// <summary>
        /// Gets/Sets the HTTP Timeout in milliseconds used for communicating with the SPARQL Endpoint
        /// </summary>
        public override int Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                _timeout = value;
                _endpoint.Timeout = value;
                _updateEndpoint.Timeout = value;
            }
        }

        /// <summary>
        /// Gets that deleting graphs is supported
        /// </summary>
        public override bool DeleteSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets that the store is not read-only
        /// </summary>
        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the IO behaviour for the store
        /// </summary>
        public override IOBehaviour IOBehaviour
        {
            get
            {
                return IOBehaviour.IsQuadStore | IOBehaviour.HasDefaultGraph | IOBehaviour.HasNamedGraphs | IOBehaviour.CanUpdateTriples | IOBehaviour.OverwriteTriples | IOBehaviour.OverwriteDefault | IOBehaviour.OverwriteNamed;
            }
        }

        /// <summary>
        /// Gets that triple level updates are supported, see the remarks section of the <see cref="ReadWriteSparqlConnector"/> for exactly what is and isn't supported
        /// </summary>
        public override bool UpdateSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Deletes a graph from the store
        /// </summary>
        /// <param name="graphUri">URI of the graph to delete</param>
        public override void DeleteGraph(string graphUri)
        {
            DeleteGraph(graphUri.ToSafeUri());
        }

        /// <summary>
        /// Deletes a graph from the store
        /// </summary>
        /// <param name="graphUri">URI of the graph to delete</param>
        public override void DeleteGraph(Uri graphUri)
        {
            if (graphUri == null)
            {
                Update("DROP DEFAULT");
            }
            else
            {
                Update("DROP GRAPH <" + _formatter.FormatUri(graphUri) + ">");
            }
        }

        /// <summary>
        /// Saves a graph to the store
        /// </summary>
        /// <param name="g">Graph to save</param>
        public override void SaveGraph(IGraph g)
        {
            StringBuilder updates = new StringBuilder();

            // Saving a Graph ovewrites a previous graph so start with a CLEAR SILENT GRAPH
            if (g.BaseUri == null)
            {
                updates.AppendLine("CLEAR SILENT DEFAULT;");
            }
            else
            {
                updates.AppendLine("CLEAR SILENT GRAPH <" + _formatter.FormatUri(g.BaseUri) + ">;");
            }

            // Insert preamble
            // Note that we use INSERT { } WHERE { } rather than INSERT DATA { } so we can insert blank nodes
            if (g.BaseUri != null)
            {
                updates.AppendLine("WITH <" + _formatter.FormatUri(g.BaseUri) + ">");
            }
            updates.AppendLine("INSERT");
            updates.AppendLine("{");

            // Serialize triples
            foreach (Triple t in g.Triples)
            {
                updates.AppendLine(" " + _formatter.Format(t));
            }

            // End
            updates.AppendLine("} WHERE { }");

            // Save the graph
            Update(updates.ToString());
        }

        /// <summary>
        /// Updates a graph in the store
        /// </summary>
        /// <param name="graphUri">URI of the graph to update</param>
        /// <param name="additions">Triples to add</param>
        /// <param name="removals">Triples to remove</param>
        public override void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            UpdateGraph(graphUri.ToSafeUri(), additions, removals);
        }

        /// <summary>
        /// Updates a graph in the store
        /// </summary>
        /// <param name="graphUri">URI of the graph to update</param>
        /// <param name="additions">Triples to add</param>
        /// <param name="removals">Triples to remove</param>
        public override void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            StringBuilder updates = new StringBuilder();

            if (additions != null)
            {
                if (additions.Any())
                {
                    // Insert preamble
                    // Note that we use INSERT { } WHERE { } rather than INSERT DATA { } so we can insert blank nodes
                    if (graphUri != null)
                    {
                        updates.AppendLine("WITH <" + _formatter.FormatUri(graphUri) + ">");
                    }
                    updates.AppendLine("INSERT");
                    updates.AppendLine("{");

                    // Serialize triples
                    foreach (Triple t in additions)
                    {
                        updates.AppendLine(" " + _formatter.Format(t));
                    }

                    // End
                    updates.AppendLine("} WHERE { }");
                    if (removals != null && removals.Any()) updates.AppendLine(";");
                }
            }
            if (removals != null)
            {
                if (removals.Any())
                {
                    // Insert preamble
                    // Note that we use DELETE DATA { } for deletes so we don't support deleting blank nodes
                    updates.AppendLine("DELETE DATA");
                    updates.AppendLine("{");

                    if (graphUri != null)
                    {
                        updates.AppendLine("GRAPH <" + _formatter.FormatUri(graphUri) + "> {");
                    }

                    // Serialize triples
                    foreach (Triple t in removals)
                    {
                        if (!t.IsGroundTriple) throw new RdfStorageException("The ReadWriteSparqlConnector does not support the deletion of blank node containing triples");
                        updates.AppendLine("  " + _formatter.Format(t));
                    }

                    // End
                    if (graphUri != null) updates.AppendLine(" }");
                    updates.AppendLine("}");
                }
            }

            // Make an update if necessary
            if (updates.Length > 0)
            {
                Update(updates.ToString());
            }
        }

        /// <summary>
        /// Makes a SPARQL Update against the store
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update</param>
        public void Update(string sparqlUpdate)
        {
            if (!_skipLocalParsing)
            {
                // Parse the update locally to validate it
                // This also saves us wasting a HttpWebRequest on a malformed update
                SparqlUpdateParser uparser = new SparqlUpdateParser();
                uparser.ParseFromString(sparqlUpdate);

                _updateEndpoint.Update(sparqlUpdate);
            }
            else
            {
                // If we're skipping local parsing then we'll need to just make a raw update
                _updateEndpoint.Update(sparqlUpdate);
            }
        }

        /// <summary>
        /// Gets a String which gives details of the Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[SPARQL Query & Update] Query: " + _endpoint.Uri.AbsoluteUri + " Update: " + _updateEndpoint.Uri.AbsoluteUri;
        }

        /// <summary>
        /// Serializes the connection's configuration
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public override void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            // Call base SerializeConfiguration() first
            INode manager = context.NextSubject;
            context.NextSubject = manager;
            base.SerializeConfiguration(context);
            context.NextSubject = manager;

            if (_updateEndpoint is IConfigurationSerializable)
            {
                // Use the indirect serialization method

                // Serialize the Endpoints Configuration
                INode endpoint = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUpdateEndpoint));
                INode endpointObj = context.Graph.CreateBlankNode();
                context.NextSubject = endpointObj;
                ((IConfigurationSerializable)_updateEndpoint).SerializeConfiguration(context);

                // Link that serialization to our serialization
                context.Graph.Assert(new Triple(manager, endpoint, endpointObj));
            }
            else
            {
                // Use the direct serialization method
                INode endpointUri = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUpdateEndpointUri));

                context.Graph.Assert(new Triple(manager, endpointUri, context.Graph.CreateLiteralNode(_endpoint.Uri.AbsoluteUri)));
            }
        }
    }
}
