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

#if !NO_STORAGE

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
    /// This class is effectively a read-only wrapper around a <see cref="SparqlRemoteEndpoint">SparqlRemoteEndpoint</see> using it with it's default settings, if you only need to query an endpoint and require more control over the settings used to access the endpoint you should use that class directly or use the constructors which allow you to provide your own pre-configure <see cref="SparqlRemoteEndpoint">SparqlRemoteEndpoint</see> instance
    /// </remarks>
    public class SparqlConnector
        : IQueryableGenericIOManager, IConfigurationSerializable
    {
        private SparqlRemoteEndpoint _endpoint;
        private SparqlConnectorLoadMethod _mode = SparqlConnectorLoadMethod.Construct;
        private bool _skipLocalParsing = false;
        private int _timeout;

        /// <summary>
        /// Creates a new SPARQL Connector which uses the given SPARQL Endpoint
        /// </summary>
        /// <param name="endpoint">Endpoint</param>
        public SparqlConnector(SparqlRemoteEndpoint endpoint)
        {
            if (endpoint == null) throw new ArgumentNullException("endpoint", "A valid Endpoint must be specified");
            this._endpoint = endpoint;
            this._timeout = endpoint.Timeout;
        }

        /// <summary>
        /// Creates a new SPARQL Connector which uses the given SPARQL Endpoint
        /// </summary>
        /// <param name="endpoint">Endpoint</param>
        /// <param name="mode">Load Method to use</param>
        public SparqlConnector(SparqlRemoteEndpoint endpoint, SparqlConnectorLoadMethod mode)
            : this(endpoint)
        {
            this._mode = mode;
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
                return this._skipLocalParsing;
            }
            set
            {
                this._skipLocalParsing = value;
            }
        }

        /// <summary>
        /// Gets/Sets the HTTP Timeout used for communicating with the SPARQL Endpoint
        /// </summary>
        public int Timeout
        {
            get 
            {
                return this._timeout;
            }
            set 
            {
                this._timeout = value;
                this._endpoint.Timeout = value;
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
                return this._endpoint;
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
        /// Makes a Query against the SPARQL Endpoint processing the results with an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery)
        {
            if (!this._skipLocalParsing)
            {
                //Parse the query locally to validate it and so we can decide what to do
                //when we receive the Response more easily as we'll know the query type
                //This also saves us wasting a HttpWebRequest on a malformed query
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
                        //Some kind of Sparql Result Set
                        this._endpoint.QueryWithResultSet(resultsHandler, sparqlQuery);
                        break;

                    case SparqlQueryType.Construct:
                    case SparqlQueryType.Describe:
                    case SparqlQueryType.DescribeAll:
                        //Some kind of Graph
                        this._endpoint.QueryWithResultGraph(rdfHandler, sparqlQuery);
                        break;

                    case SparqlQueryType.Unknown:
                    default:
                        //Error
                        throw new RdfQueryException("Unknown Query Type was used, unable to determine how to process the response from Talis");
                }
            }
            else
            {
                //If we're skipping local parsing then we'll need to just make a raw query and process the response
                using (HttpWebResponse response = this._endpoint.QueryRaw(sparqlQuery))
                {
                    try
                    {
                        //Is the Content Type referring to a Sparql Result Set format?
                        ISparqlResultsReader sparqlParser = MimeTypesHelper.GetSparqlParser(response.ContentType);
                        sparqlParser.Load(resultsHandler, new StreamReader(response.GetResponseStream()));
                        response.Close();
                    }
                    catch (RdfParserSelectionException)
                    {
                        //If we get a Parser Selection exception then the Content Type isn't valid for a Sparql Result Set

                        //Is the Content Type referring to a RDF format?
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
        public void LoadGraph(IGraph g, Uri graphUri)
        {
            this.LoadGraph(g, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the SPARQL Endpoint
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            this.LoadGraph(handler, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the SPARQL Endpoint
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraph(IGraph g, String graphUri)
        {
            if (g.IsEmpty && graphUri != null && !graphUri.Equals(String.Empty))
            {
                g.BaseUri = new Uri(graphUri);
            }
            this.LoadGraph(new GraphHandler(g), graphUri);
        }

        /// <summary>
        /// Loads a Graph from the SPARQL Endpoint
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public void LoadGraph(IRdfHandler handler, String graphUri)
        {
            String query;

            if (graphUri.Equals(String.Empty))
            {
                if (this._mode == SparqlConnectorLoadMethod.Describe)
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
                switch (this._mode)
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

            this._endpoint.QueryWithResultGraph(handler, query);
        }

        /// <summary>
        /// Throws an error since this Manager is read-only
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <exception cref="RdfStorageException">Always thrown since this Manager provides a read-only connection</exception>
        public void SaveGraph(IGraph g)
        {
            throw new RdfStorageException("The SparqlConnector provides a read-only connection");

        }

        /// <summary>
        /// Gets the IO Behaviour of SPARQL Connections
        /// </summary>
        public IOBehaviour IOBehaviour
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
        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new RdfStorageException("The SparqlConnector provides a read-only connection");
        }

        /// <summary>
        /// Throws an error since this Manager is read-only
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public void UpdateGraph(String graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new RdfStorageException("The SparqlConnector provides a read-only connection");
        }

        /// <summary>
        /// Returns that Updates are not supported since this connection is read-only
        /// </summary>
        public bool UpdateSupported
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
        public void DeleteGraph(Uri graphUri)
        {
            throw new RdfStorageException("The SparqlConnector provides a read-only connection");
        }

        /// <summary>
        /// Throws an exception as this connector provides a read-only connection
        /// </summary>
        /// <param name="graphUri">URI of this Graph to delete</param>
        /// <exception cref="RdfStorageException">Thrown since this connection is read-only so you cannot delete graphs using it</exception>
        public void DeleteGraph(String graphUri)
        {
            throw new RdfStorageException("The SparqlConnector provides a read-only connection");
        }

        /// <summary>
        /// Returns that deleting graphs is not supported
        /// </summary>
        public bool DeleteSupported
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
                throw new RdfStorageException("SPARQL Endpoint returned an error while trying to List Graphs", ex);
            }
        }

        /// <summary>
        /// Returns that listing graphs is supported
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
        /// Returns that the Connection is read-only
        /// </summary>
        public bool IsReadOnly
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
            //Nothing to do
        }

        /// <summary>
        /// Gets a String which gives details of the Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[SPARQL Query] " + this._endpoint.Uri.ToString();
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

            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName)));

            //TODO: Serialize Load Mode here

            if (this._endpoint is IConfigurationSerializable)
            {
                //Use the indirect serialization method

                //Serialize the Endpoints Configuration
                INode endpoint = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyEndpoint);
                INode endpointObj = context.Graph.CreateBlankNode();
                context.NextSubject = endpointObj;
                ((IConfigurationSerializable)this._endpoint).SerializeConfiguration(context);

                //Link that serialization to our serialization
                context.Graph.Assert(new Triple(manager, endpoint, endpointObj));
            }
            else
            {
                //Use the direct serialization method
                INode endpointUri = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyEndpointUri);
                INode defGraphUri = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyDefaultGraphUri);
                INode namedGraphUri = ConfigurationLoader.CreateConfigurationNode(context.Graph, ConfigurationLoader.PropertyNamedGraphUri);
                
                context.Graph.Assert(new Triple(manager, endpointUri, context.Graph.CreateLiteralNode(this._endpoint.Uri.ToString())));
                foreach (String u in this._endpoint.DefaultGraphs)
                {
                    context.Graph.Assert(new Triple(manager, defGraphUri, context.Graph.CreateUriNode(new Uri(u))));
                }
                foreach (String u in this._endpoint.NamedGraphs)
                {
                    context.Graph.Assert(new Triple(manager, namedGraphUri, context.Graph.CreateUriNode(new Uri(u))));
                }
            }
        }
    }
}

#endif