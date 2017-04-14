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
using System.Linq;
using System.Net;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Management;
using VDS.RDF.Update;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Factory class for producing <see cref="IStorageProvider">IStorageProvider</see> and <see cref="IStorageServer"/> instances from Configuration Graphs
    /// </summary>
    public class StorageFactory
        : IObjectFactory
    {
        private const String AllegroGraph = "VDS.RDF.Storage.AllegroGraphConnector",
                             AllegroGraphServer = "VDS.RDF.Storage.Management.AllegroGraphServer",
                             DatasetFile = "VDS.RDF.Storage.DatasetFileManager",
                             Dydra = "VDS.RDF.Storage.DydraConnector",
                             FourStore = "VDS.RDF.Storage.FourStoreConnector",
                             Fuseki = "VDS.RDF.Storage.FusekiConnector",
                             InMemory = "VDS.RDF.Storage.InMemoryManager",
                             ReadOnly = "VDS.RDF.Storage.ReadOnlyConnector",
                             ReadOnlyQueryable = "VDS.RDF.Storage.QueryableReadOnlyConnector",
                             ReadWriteSparql = "VDS.RDF.Storage.ReadWriteSparqlConnector",
                             Sesame = "VDS.RDF.Storage.SesameHttpProtocolConnector",
                             SesameV5 = "VDS.RDF.Storage.SesameHttpProtocolVersion5Connector",
                             SesameV6 = "VDS.RDF.Storage.SesameHttpProtocolVersion6Connector",
                             SesameServer = "VDS.RDF.Storage.Management.SesameServer",
                             Sparql = "VDS.RDF.Storage.SparqlConnector",
                             SparqlHttpProtocol = "VDS.RDF.Storage.SparqlHttpProtocolConnector",
                             Stardog = "VDS.RDF.Storage.StardogConnector",
                             StardogV1 = "VDS.RDF.Storage.StardogV1Connector",
                             StardogV2 = "VDS.RDF.Storage.StardogV2Connector",
                             StardogV3 = "VDS.RDF.Storage.StardogV3Connector",
                             StardogServer = "VDS.RDF.Storage.Management.StardogServer",
                             StardogServerV1 = "VDS.RDF.Storage.Management.StardogV1Server",
                             StardogServerV2 = "VDS.RDF.Storage.Management.StardogV2Server",
                             StardogServerV3 = "VDS.RDF.Storage.Management.StardogV3Server"
                             ;

        /// <summary>
        /// Tries to load a Generic IO Manager based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            IStorageProvider storageProvider = null;
            IStorageServer storageServer = null;
            SparqlConnectorLoadMethod loadMode;
            obj = null;

            String server, user, pwd, store, catalog, loadModeRaw;

            Object temp;
            INode storeObj;

            // Create the URI Nodes we're going to use to search for things
            INode propServer = g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyServer)),
                  propDb = g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyDatabase)),
                  propStore = g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyStore)),
                  propAsync = g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyAsync)),
                  propStorageProvider = g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyStorageProvider));

            switch (targetType.FullName)
            {
                case AllegroGraph:
                    // Get the Server, Catalog and Store
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                    if (server == null) return false;
                    catalog = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyCatalog)));
                    store = ConfigurationLoader.GetConfigurationString(g, objNode, propStore);
                    if (store == null) return false;

                    // Get User Credentials
                    ConfigurationLoader.GetUsernameAndPassword(g, objNode, true, out user, out pwd);

                    if (user != null && pwd != null)
                    {
                        storageProvider = new AllegroGraphConnector(server, catalog, store, user, pwd);
                    }
                    else
                    {
                        storageProvider = new AllegroGraphConnector(server, catalog, store);
                    }
                    break;

                case AllegroGraphServer:
                    // Get the Server, Catalog and User Credentials
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                    if (server == null) return false;
                    catalog = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyCatalog)));
                    ConfigurationLoader.GetUsernameAndPassword(g, objNode, true, out user, out pwd);

                    if (user != null && pwd != null)
                    {
                        storageServer = new AllegroGraphServer(server, catalog, user, pwd);
                    }
                    else
                    {
                        storageServer = new AllegroGraphServer(server, catalog);
                    }
                    break;
                case DatasetFile:
                    // Get the Filename and whether the loading should be done asynchronously
                    String file = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyFromFile)));
                    if (file == null) return false;
                    file = ConfigurationLoader.ResolvePath(file);
                    bool isAsync = ConfigurationLoader.GetConfigurationBoolean(g, objNode, propAsync, false);
                    storageProvider = new DatasetFileManager(file, isAsync);
                    break;

                case Dydra:
                    throw new DotNetRdfConfigurationException("DydraConnector is no longer supported by dotNetRDF and is considered obsolete");

                case FourStore:
                    // Get the Server and whether Updates are enabled
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                    if (server == null) return false;
                    bool enableUpdates = ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyEnableUpdates)), true);
                    storageProvider = new FourStoreConnector(server, enableUpdates);
                    break;

                case Fuseki:
                    // Get the Server URI
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                    if (server == null) return false;
                    storageProvider = new FusekiConnector(server);
                    break;

                case InMemory:
                    // Get the Dataset/Store
                    INode datasetObj = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUsingDataset)));
                    if (datasetObj != null)
                    {
                        temp = ConfigurationLoader.LoadObject(g, datasetObj);
                        if (temp is ISparqlDataset)
                        {
                            storageProvider = new InMemoryManager((ISparqlDataset) temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the In-Memory Manager identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingDataset property points to an Object that cannot be loaded as an object which implements the ISparqlDataset interface");
                        }
                    }
                    else
                    {
                        // If no dnr:usingDataset try dnr:usingStore instead
                        storeObj = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUsingStore)));
                        if (storeObj != null)
                        {
                            temp = ConfigurationLoader.LoadObject(g, storeObj);
                            if (temp is IInMemoryQueryableStore)
                            {
                                storageProvider = new InMemoryManager((IInMemoryQueryableStore) temp);
                            }
                            else
                            {
                                throw new DotNetRdfConfigurationException("Unable to load the In-Memory Manager identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingStore property points to an Object that cannot be loaded as an object which implements the IInMemoryQueryableStore interface");
                            }
                        }
                        else
                        {
                            // If no dnr:usingStore either then create a new empty store
                            storageProvider = new InMemoryManager();
                        }
                    }
                    break;

                case ReadOnly:
                    // Get the actual Manager we are wrapping
                    storeObj = ConfigurationLoader.GetConfigurationNode(g, objNode, propStorageProvider);
                    temp = ConfigurationLoader.LoadObject(g, storeObj);
                    if (temp is IStorageProvider)
                    {
                        storageProvider = new ReadOnlyConnector((IStorageProvider) temp);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the Read-Only Connector identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:genericManager property points to an Object which cannot be loaded as an object which implements the required IStorageProvider interface");
                    }
                    break;

                case ReadOnlyQueryable:
                    // Get the actual Manager we are wrapping
                    storeObj = ConfigurationLoader.GetConfigurationNode(g, objNode, propStorageProvider);
                    temp = ConfigurationLoader.LoadObject(g, storeObj);
                    if (temp is IQueryableStorage)
                    {
                        storageProvider = new QueryableReadOnlyConnector((IQueryableStorage) temp);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the Queryable Read-Only Connector identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:genericManager property points to an Object which cannot be loaded as an object which implements the required IQueryableStorage interface");
                    }
                    break;

                case Sesame:
                case SesameV5:
                case SesameV6:
                    // Get the Server and Store ID
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                    if (server == null) return false;
                    store = ConfigurationLoader.GetConfigurationString(g, objNode, propStore);
                    if (store == null) return false;
                    ConfigurationLoader.GetUsernameAndPassword(g, objNode, true, out user, out pwd);
                    if (user != null && pwd != null)
                    {
                        storageProvider = (IStorageProvider) Activator.CreateInstance(targetType, new Object[] {server, store, user, pwd});
                    }
                    else
                    {
                        storageProvider = (IStorageProvider) Activator.CreateInstance(targetType, new Object[] {server, store});
                    }
                    break;

                case SesameServer:
                    // Get the Server and User Credentials
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                    if (server == null) return false;
                    ConfigurationLoader.GetUsernameAndPassword(g, objNode, true, out user, out pwd);

                    if (user != null && pwd != null)
                    {
                        storageServer = new SesameServer(server, user, pwd);
                    }
                    else
                    {
                        storageServer = new SesameServer(server);
                    }
                    break;

                case Sparql:
                    // Get the Endpoint URI or the Endpoint
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, new INode[] {g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyQueryEndpointUri)), g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyEndpointUri))});

                    // What's the load mode?
                    loadModeRaw = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyLoadMode)));
                    loadMode = SparqlConnectorLoadMethod.Construct;
                    if (loadModeRaw != null)
                    {
                        try
                        {
                            loadMode = (SparqlConnectorLoadMethod) Enum.Parse(typeof (SparqlConnectorLoadMethod), loadModeRaw);
                        }
                        catch
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the SparqlConnector identified by the Node '" + objNode.ToString() + "' as the value given for the property dnr:loadMode is not valid");
                        }
                    }

                    if (server == null)
                    {
                        INode endpointObj = ConfigurationLoader.GetConfigurationNode(g, objNode, new INode[] {g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyQueryEndpoint)), g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyEndpoint))});
                        if (endpointObj == null) return false;
                        temp = ConfigurationLoader.LoadObject(g, endpointObj);
                        if (temp is SparqlRemoteEndpoint)
                        {
                            storageProvider = new SparqlConnector((SparqlRemoteEndpoint) temp, loadMode);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the SparqlConnector identified by the Node '" + objNode.ToString() + "' as the value given for the property dnr:endpoint points to an Object which cannot be loaded as an object which is of the type SparqlRemoteEndpoint");
                        }
                    }
                    else
                    {
                        // Are there any Named/Default Graph URIs
                        IEnumerable<Uri> defGraphs = from def in ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyDefaultGraphUri)))
                                                     where def.NodeType == NodeType.Uri
                                                     select ((IUriNode) def).Uri;
                        IEnumerable<Uri> namedGraphs = from named in ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyNamedGraphUri)))
                                                       where named.NodeType == NodeType.Uri
                                                       select ((IUriNode) named).Uri;
                        if (defGraphs.Any() || namedGraphs.Any())
                        {
                            storageProvider = new SparqlConnector(new SparqlRemoteEndpoint(UriFactory.Create(server), defGraphs, namedGraphs), loadMode);
                        }
                        else
                        {
                            storageProvider = new SparqlConnector(UriFactory.Create(server), loadMode);
                        }
                    }
                    break;

                case ReadWriteSparql:
                    SparqlRemoteEndpoint queryEndpoint;
                    SparqlRemoteUpdateEndpoint updateEndpoint;

                    // Get the Query Endpoint URI or the Endpoint
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, new INode[] {g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUpdateEndpointUri)), g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyEndpointUri))});

                    // What's the load mode?
                    loadModeRaw = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyLoadMode)));
                    loadMode = SparqlConnectorLoadMethod.Construct;
                    if (loadModeRaw != null)
                    {
                        try
                        {
                            loadMode = (SparqlConnectorLoadMethod) Enum.Parse(typeof (SparqlConnectorLoadMethod), loadModeRaw);
                        }
                        catch
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the ReadWriteSparqlConnector identified by the Node '" + objNode.ToString() + "' as the value given for the property dnr:loadMode is not valid");
                        }
                    }

                    if (server == null)
                    {
                        INode endpointObj = ConfigurationLoader.GetConfigurationNode(g, objNode, new INode[] {g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyQueryEndpoint)), g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyEndpoint))});
                        if (endpointObj == null) return false;
                        temp = ConfigurationLoader.LoadObject(g, endpointObj);
                        if (temp is SparqlRemoteEndpoint)
                        {
                            queryEndpoint = (SparqlRemoteEndpoint) temp;
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the ReadWriteSparqlConnector identified by the Node '" + objNode.ToString() + "' as the value given for the property dnr:queryEndpoint/dnr:endpoint points to an Object which cannot be loaded as an object which is of the type SparqlRemoteEndpoint");
                        }
                    }
                    else
                    {
                        // Are there any Named/Default Graph URIs
                        IEnumerable<Uri> defGraphs = from def in ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyDefaultGraphUri)))
                                                     where def.NodeType == NodeType.Uri
                                                     select ((IUriNode) def).Uri;
                        IEnumerable<Uri> namedGraphs = from named in ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyNamedGraphUri)))
                                                       where named.NodeType == NodeType.Uri
                                                       select ((IUriNode) named).Uri;
                        if (defGraphs.Any() || namedGraphs.Any())
                        {
                            queryEndpoint = new SparqlRemoteEndpoint(UriFactory.Create(server), defGraphs, namedGraphs);
                            ;
                        }
                        else
                        {
                            queryEndpoint = new SparqlRemoteEndpoint(UriFactory.Create(server));
                        }
                    }

                    // Find the Update Endpoint or Endpoint URI
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, new INode[] {g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUpdateEndpointUri)), g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyEndpointUri))});

                    if (server == null)
                    {
                        INode endpointObj = ConfigurationLoader.GetConfigurationNode(g, objNode, new INode[] {g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyUpdateEndpoint)), g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyEndpoint))});
                        if (endpointObj == null) return false;
                        temp = ConfigurationLoader.LoadObject(g, endpointObj);
                        if (temp is SparqlRemoteUpdateEndpoint)
                        {
                            updateEndpoint = (SparqlRemoteUpdateEndpoint) temp;
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the ReadWriteSparqlConnector identified by the Node '" + objNode.ToString() + "' as the value given for the property dnr:updateEndpoint/dnr:endpoint points to an Object which cannot be loaded as an object which is of the type SparqlRemoteUpdateEndpoint");
                        }
                    }
                    else
                    {
                        updateEndpoint = new SparqlRemoteUpdateEndpoint(UriFactory.Create(server));
                    }
                    storageProvider = new ReadWriteSparqlConnector(queryEndpoint, updateEndpoint, loadMode);
                    break;

                case SparqlHttpProtocol:
                    // Get the Service URI
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                    if (server == null) return false;
                    storageProvider = new SparqlHttpProtocolConnector(UriFactory.Create(server));
                    break;

                case Stardog:
                case StardogV1:
                case StardogV2:
                case StardogV3:
                    // Get the Server and Store
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                    if (server == null) return false;
                    store = ConfigurationLoader.GetConfigurationString(g, objNode, propStore);
                    if (store == null) return false;

                    // Get User Credentials
                    ConfigurationLoader.GetUsernameAndPassword(g, objNode, true, out user, out pwd);

                    // Get Reasoning Mode
                    StardogReasoningMode reasoning = StardogReasoningMode.None;
                    String mode = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyLoadMode)));
                    if (mode != null)
                    {
                        try
                        {
                            reasoning = (StardogReasoningMode) Enum.Parse(typeof (StardogReasoningMode), mode, true);
                        }
                        catch
                        {
                            reasoning = StardogReasoningMode.None;
                        }
                    }

                    if (user != null && pwd != null)
                    {
                        switch (targetType.FullName)
                        {
                            case StardogV1:
                                storageProvider = new StardogV1Connector(server, store, reasoning, user, pwd);
                                break;
                            case StardogV2:
                                storageProvider = new StardogV2Connector(server, store, reasoning, user, pwd);
                                break;
                            case StardogV3:
                                storageProvider = new StardogV3Connector(server, store, user, pwd);
                                break;
                            case Stardog:
                            default:
                                storageProvider = new StardogConnector(server, store, user, pwd);
                                break;
                        }
                    }
                    else
                    {
                        switch (targetType.FullName)
                        {
                            case StardogV1:
                                storageProvider = new StardogV1Connector(server, store, reasoning);
                                break;
                            case StardogV2:
                                storageProvider = new StardogV2Connector(server, store, reasoning);
                                break;
                            case StardogV3:
                                storageProvider = new StardogV3Connector(server, store);
                                break;
                            case Stardog:
                            default:
                                storageProvider = new StardogConnector(server, store);
                                break;
                        }
                    }
                    break;

                case StardogServer:
                case StardogServerV1:
                case StardogServerV2:
                case StardogServerV3:
                    // Get the Server and User Credentials
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                    if (server == null) return false;
                    ConfigurationLoader.GetUsernameAndPassword(g, objNode, true, out user, out pwd);

                    if (user != null && pwd != null)
                    {
                        switch (targetType.FullName)
                        {
                            case StardogServerV1:
                                storageServer = new StardogV1Server(server, user, pwd);
                                break;
                            case StardogServerV2:
                                storageServer = new StardogV2Server(server, user, pwd);
                                break;
                            case StardogServerV3:
                                storageServer = new StardogV3Server(server, user, pwd);
                                break;
                            case StardogServer:
                            default:
                                storageServer = new StardogServer(server, user, pwd);
                                break;
                        }
                    }
                    else
                    {
                        switch (targetType.FullName)
                        {
                            case StardogServerV1:
                                storageServer = new StardogV1Server(server);
                                break;
                            case StardogServerV2:
                                storageServer = new StardogV2Server(server);
                                break;
                            case StardogServerV3:
                                storageServer = new StardogV3Server(server);
                                break;
                            case StardogServer:
                            default:
                                storageServer = new StardogServer(server);
                                break;
                        }
                    }
                    break;
            }

            // Set the return object if one has been loaded
            if (storageProvider != null)
            {
                obj = storageProvider;
            }
            else if (storageServer != null)
            {
                obj = storageServer;
            }

            // Check whether this is a standard HTTP manager and if so load standard configuration
            if (obj is BaseHttpConnector)
            {
                BaseHttpConnector connector = (BaseHttpConnector) obj;

                int timeout = ConfigurationLoader.GetConfigurationInt32(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyTimeout)), 0);
                if (timeout > 0)
                {
                    connector.Timeout = timeout;
                }
                INode proxyNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyProxy)));
                if (proxyNode != null)
                {
                    temp = ConfigurationLoader.LoadObject(g, proxyNode);
                    if (temp is IWebProxy)
                    {
                        connector.Proxy = (IWebProxy) temp;
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load storage provider/server identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:proxy property pointed to an Object which could not be loaded as an object of the required type WebProxy");
                    }
                }
            }

            return (obj != null);
        }

        /// <summary>
        /// Gets whether this Factory can load objects of the given Type
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns></returns>
        public bool CanLoadObject(Type t)
        {
            switch (t.FullName)
            {
                case AllegroGraph:
                case AllegroGraphServer:
                case DatasetFile:
                case Dydra:
                case FourStore:
                case Fuseki:
                case InMemory:
                case Sesame:
                case SesameV5:
                case SesameV6:
                case SesameServer:
                case ReadOnly:
                case ReadOnlyQueryable:
                case Sparql:
                case ReadWriteSparql:
                case SparqlHttpProtocol:
                case Stardog:
                case StardogV1:
                case StardogV2:
                case StardogV3:
                case StardogServer:
                case StardogServerV1:
                case StardogServerV2:
                case StardogServerV3:
                    return true;
                default:
                    return false;
            }
        }
    }
}