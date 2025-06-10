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
using System.Linq;
using System.Net;
using System.Net.Http;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Management;
using VDS.RDF.Update;

namespace VDS.RDF.Configuration;

/// <summary>
/// Factory class for producing <see cref="IStorageProvider">IStorageProvider</see> and <see cref="IStorageServer"/> instances from Configuration Graphs.
/// </summary>
public class StorageFactory
    : IObjectFactory
{
    private const string ReadOnly = "VDS.RDF.Storage.ReadOnlyConnector",
        ReadOnlyQueryable = "VDS.RDF.Storage.QueryableReadOnlyConnector",
        ReadWriteSparql = "VDS.RDF.Storage.ReadWriteSparqlConnector",
        Sparql = "VDS.RDF.Storage.SparqlConnector",
        SparqlHttpProtocol = "VDS.RDF.Storage.SparqlHttpProtocolConnector",
        DatasetFile = "VDS.RDF.Storage.DatasetFileManager",
        InMemory = "VDS.RDF.Storage.InMemoryManager";


    /// <summary>
    /// Tries to load a Generic IO Manager based on information from the Configuration Graph.
    /// </summary>
    /// <param name="g">Configuration Graph.</param>
    /// <param name="objNode">Object Node.</param>
    /// <param name="targetType">Target Type.</param>
    /// <param name="obj">Output Object.</param>
    /// <returns></returns>
    public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
    {
        IStorageProvider storageProvider = null;
        SparqlConnectorLoadMethod loadMode;
        obj = null;

        string server, loadModeRaw;

        object temp;
        INode storeObj;

        // Create the URI Nodes we're going to use to search for things
        INode propServer = g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyServer)),
              propStorageProvider = g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyStorageProvider)),
              propAsync = g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyAsync));

        switch (targetType.FullName)
        {
            case ReadOnly:
                // Get the actual Manager we are wrapping
                storeObj = ConfigurationLoader.GetConfigurationNode(g, objNode, propStorageProvider);
                temp = ConfigurationLoader.LoadObject(g, storeObj);
                if (temp is IStorageProvider provider)
                {
                    storageProvider = new ReadOnlyConnector(provider);
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


            case Sparql:
                // Get the Endpoint URI or the Endpoint
                server = ConfigurationLoader.GetConfigurationString(g, objNode, new INode[] {g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyQueryEndpointUri)), g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyEndpointUri))});

                // What's the load mode?
                loadModeRaw = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyLoadMode)));
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
                    INode endpointObj = ConfigurationLoader.GetConfigurationNode(g, objNode, new INode[] {g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyQueryEndpoint)), g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyEndpoint))});
                    if (endpointObj == null) return false;
                    temp = ConfigurationLoader.LoadObject(g, endpointObj);
                    
                    switch (temp)
                    {
#pragma warning disable 618
                        case SparqlRemoteEndpoint remoteEndpoint:
                            storageProvider = new SparqlConnector(remoteEndpoint, loadMode);
                            break;
#pragma warning restore 618
                        case SparqlQueryClient queryClient:
                            storageProvider = new SparqlConnector(queryClient, loadMode);
                            break;
                        default:
                            throw new DotNetRdfConfigurationException("Unable to load the SparqlConnector identified by the Node '" + objNode.ToString() + "' as the value given for the property dnr:endpoint points to an Object which cannot be loaded as an object which is of the type SparqlRemoteEndpoint");
                    }
                }
                else
                {
                    // Are there any Named/Default Graph URIs
                    IEnumerable<Uri> defGraphs = from def in ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyDefaultGraphUri)))
                                                 where def.NodeType == NodeType.Uri
                                                 select ((IUriNode) def).Uri;
                    IEnumerable<Uri> namedGraphs = from named in ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyNamedGraphUri)))
                                                   where named.NodeType == NodeType.Uri
                                                   select ((IUriNode) named).Uri;
                    var queryClient = new SparqlQueryClient(new HttpClient(), g.UriFactory.Create(server));
                    queryClient.DefaultGraphs.AddRange(defGraphs.Select(u=>u.AbsoluteUri));
                    queryClient.NamedGraphs.AddRange(namedGraphs.Select(u=>u.AbsoluteUri));
                    storageProvider = new SparqlConnector(queryClient);
                }
                break;

            case ReadWriteSparql:
            {
#pragma warning disable 618
                SparqlRemoteEndpoint queryEndpoint = null;
                SparqlRemoteUpdateEndpoint updateEndpoint = null;
#pragma warning restore 618
                SparqlQueryClient queryClient = null;
                SparqlUpdateClient updateClient = null;

                // Get the Query Endpoint URI or the Endpoint
                server = ConfigurationLoader.GetConfigurationString(g, objNode,
                    new INode[]
                    {
                        g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUpdateEndpointUri)),
                        g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyEndpointUri)),
                    });

                // What's the load mode?
                loadModeRaw = ConfigurationLoader.GetConfigurationString(g, objNode,
                    g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyLoadMode)));
                loadMode = SparqlConnectorLoadMethod.Construct;
                if (loadModeRaw != null)
                {
                    try
                    {
                        loadMode = (SparqlConnectorLoadMethod)Enum.Parse(typeof(SparqlConnectorLoadMethod),
                            loadModeRaw);
                    }
                    catch
                    {
                        throw new DotNetRdfConfigurationException(
                            "Unable to load the ReadWriteSparqlConnector identified by the Node '" +
                            objNode.ToString() + "' as the value given for the property dnr:loadMode is not valid");
                    }
                }

                if (server == null)
                {
                    INode endpointObj = ConfigurationLoader.GetConfigurationNode(g, objNode,
                        new INode[]
                        {
                            g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyQueryEndpoint)),
                            g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyEndpoint)),
                        });
                    if (endpointObj == null) return false;
                    temp = ConfigurationLoader.LoadObject(g, endpointObj);
                    switch (temp)
                    {
#pragma warning disable 618
                        case SparqlRemoteEndpoint remoteEndpoint:
#pragma warning restore 618
                            queryEndpoint = remoteEndpoint;
                            break;
                        case SparqlQueryClient qc:
                            queryClient = qc;
                            break;
                        default:
                            throw new DotNetRdfConfigurationException(
                                "Unable to load the ReadWriteSparqlConnector identified by the Node '" +
                                objNode.ToString() +
                                "' as the value given for the property dnr:queryEndpoint/dnr:endpoint points to an Object which cannot be loaded as an object which is of the type SparqlRemoteEndpoint");
                    }
                }
                else
                {
                    // Are there any Named/Default Graph URIs
                    IEnumerable<Uri> defGraphs = from def in ConfigurationLoader.GetConfigurationData(g, objNode,
                            g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyDefaultGraphUri)))
                        where def.NodeType == NodeType.Uri
                        select ((IUriNode)def).Uri;
                    IEnumerable<Uri> namedGraphs = from named in ConfigurationLoader.GetConfigurationData(g,
                            objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyNamedGraphUri)))
                        where named.NodeType == NodeType.Uri
                        select ((IUriNode)named).Uri;
                    queryClient = new SparqlQueryClient(new HttpClient(), g.UriFactory.Create(server));
                    queryClient.DefaultGraphs.AddRange(defGraphs.Select(g => g.AbsoluteUri));
                    queryClient.NamedGraphs.AddRange(namedGraphs.Select(u => u.AbsoluteUri));
                }

                // Find the Update Endpoint or Endpoint URI
                server = ConfigurationLoader.GetConfigurationString(g, objNode,
                    new INode[]
                    {
                        g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUpdateEndpointUri)),
                        g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyEndpointUri)),
                    });

                if (server == null)
                {
                    INode endpointObj = ConfigurationLoader.GetConfigurationNode(g, objNode,
                        new INode[]
                        {
                            g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUpdateEndpoint)),
                            g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyEndpoint)),
                        });
                    if (endpointObj == null) return false;
                    temp = ConfigurationLoader.LoadObject(g, endpointObj);
                    switch (temp)
                    {
#pragma warning disable 618
                        case SparqlRemoteUpdateEndpoint ue:
#pragma warning restore 618
                            updateEndpoint = ue;
                            break;
                        case SparqlUpdateClient uc:
                            updateClient = uc;
                            break;
                        default:
                            throw new DotNetRdfConfigurationException(
                                "Unable to load the ReadWriteSparqlConnector identified by the Node '" +
                                objNode.ToString() +
                                "' as the value given for the property dnr:updateEndpoint/dnr:endpoint points to an Object which cannot be loaded as an object which is of the type SparqlRemoteUpdateEndpoint");
                    }
                }
                else
                {
                    updateClient = new SparqlUpdateClient(new HttpClient(), g.UriFactory.Create(server));
                }

                if (queryClient != null && updateClient != null)
                {
                    storageProvider = new ReadWriteSparqlConnector(queryClient, updateClient, loadMode);
                }
                else if (queryEndpoint != null && updateEndpoint != null)
                {
#pragma warning disable 618
                    storageProvider = new ReadWriteSparqlConnector(queryEndpoint, updateEndpoint, loadMode);
#pragma warning restore 618
                }
                else
                {
                    throw new DotNetRdfConfigurationException(
                        $"Unable to load the ReadWriteSparqlConnector identified by the node '{objNode}' as the query and update endpoints are of incompatible types.");
                }

                break;
            }

            case SparqlHttpProtocol:
                // Get the Service URI
                server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                if (server == null) return false;
                storageProvider = new SparqlHttpProtocolConnector(g.UriFactory.Create(server));
                break;

            case DatasetFile:
                // Get the Filename and whether the loading should be done asynchronously
                var file = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyFromFile)));
                if (file == null) return false;
                file = ConfigurationLoader.ResolvePath(file);
                var isAsync = ConfigurationLoader.GetConfigurationBoolean(g, objNode, propAsync, false);
                storageProvider = new DatasetFileManager(file, isAsync);
                break;

            case InMemory:
                // Get the Dataset/Store
                INode datasetObj = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingDataset)));
                if (datasetObj != null)
                {
                    temp = ConfigurationLoader.LoadObject(g, datasetObj);
                    if (temp is ISparqlDataset)
                    {
                        storageProvider = new InMemoryManager((ISparqlDataset)temp);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the In-Memory Manager identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingDataset property points to an Object that cannot be loaded as an object which implements the ISparqlDataset interface");
                    }
                }
                else
                {
                    // If no dnr:usingDataset try dnr:usingStore instead
                    storeObj = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingStore)));
                    if (storeObj != null)
                    {
                        temp = ConfigurationLoader.LoadObject(g, storeObj);
                        if (temp is IInMemoryQueryableStore store)
                        {
                            storageProvider = new InMemoryManager(store);
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
        }

        // Set the return object if one has been loaded
        if (storageProvider != null)
        {
            obj = storageProvider;
        }
        
        // Check whether this is a standard HTTP manager and if so load standard configuration
        if (obj is BaseHttpConnector connector)
        {
            var timeout = ConfigurationLoader.GetConfigurationInt32(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyTimeout)), 0);
            if (timeout > 0)
            {
                connector.Timeout = timeout;
            }
            INode proxyNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyProxy)));
            if (proxyNode != null)
            {
                temp = ConfigurationLoader.LoadObject(g, proxyNode);
                if (temp is IWebProxy proxy)
                {
                    connector.Proxy = proxy;
                }
                else
                {
                    throw new DotNetRdfConfigurationException("Unable to load storage provider/server identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:proxy property pointed to an Object which could not be loaded as an object of the required type WebProxy");
                }
            }
        }

        return obj != null;
    }

    /// <summary>
    /// Gets whether this Factory can load objects of the given Type.
    /// </summary>
    /// <param name="t">Type.</param>
    /// <returns></returns>
    public bool CanLoadObject(Type t)
    {
        switch (t.FullName)
        {
            case ReadOnly:
            case ReadOnlyQueryable:
            case Sparql:
            case ReadWriteSparql:
            case SparqlHttpProtocol:
            case DatasetFile:
            case InMemory:
                return true;
            default:
                return false;
        }
    }
}