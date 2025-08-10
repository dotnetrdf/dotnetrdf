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
using System.Net;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Management;

namespace VDS.RDF.Configuration;

/// <summary>
/// Factory class for producing <see cref="IStorageProvider">IStorageProvider</see> and <see cref="IStorageServer"/> instances from Configuration Graphs.
/// </summary>
public class StorageFactory
    : IObjectFactory
{
    private const string AllegroGraph = "VDS.RDF.Storage.AllegroGraphConnector",
                         AllegroGraphServer = "VDS.RDF.Storage.Management.AllegroGraphServer",
                         Dydra = "VDS.RDF.Storage.DydraConnector",
                         FourStore = "VDS.RDF.Storage.FourStoreConnector",
                         Fuseki = "VDS.RDF.Storage.FusekiConnector",
                         Sesame = "VDS.RDF.Storage.SesameHttpProtocolConnector",
                         SesameV5 = "VDS.RDF.Storage.SesameHttpProtocolVersion5Connector",
                         SesameV6 = "VDS.RDF.Storage.SesameHttpProtocolVersion6Connector",
                         SesameServer = "VDS.RDF.Storage.Management.SesameServer",
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
        IStorageServer storageServer = null;
        obj = null;

        string server, user, pwd, store, catalog;

        // Create the URI Nodes we're going to use to search for things
        INode propServer = g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyServer)),
            propStore = g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyStore));

        switch (targetType.FullName)
        {
            case AllegroGraph:
                // Get the Server, Catalog and Store
                server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                if (server == null) return false;
                catalog = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyCatalog)));
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
                catalog = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyCatalog)));
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

            case Dydra:
                throw new DotNetRdfConfigurationException("DydraConnector is no longer supported by dotNetRDF and is considered obsolete");

            case FourStore:
                // Get the Server and whether Updates are enabled
                server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                if (server == null) return false;
                var enableUpdates = ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyEnableUpdates)), true);
                storageProvider = new FourStoreConnector(server, enableUpdates);
                break;

            case Fuseki:
                // Get the Server URI
                server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                if (server == null) return false;
                storageProvider = new FusekiConnector(server);
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
                    storageProvider = (IStorageProvider) Activator.CreateInstance(targetType, new object[] {server, store, user, pwd});
                }
                else
                {
                    storageProvider = (IStorageProvider) Activator.CreateInstance(targetType, new object[] {server, store});
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
                var temp = ConfigurationLoader.LoadObject(g, proxyNode);
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
            case AllegroGraph:
            case AllegroGraphServer:
            case Dydra:
            case FourStore:
            case Fuseki:
            case Sesame:
            case SesameV5:
            case SesameV6:
            case SesameServer:
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