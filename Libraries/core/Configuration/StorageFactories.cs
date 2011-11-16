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
using System.Linq;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;

namespace VDS.RDF.Configuration
{

#if !NO_DATA && !NO_STORAGE

    /// <summary>
    /// Factory class for producing <see cref="ISqlIOManager">ISqlIOManager</see> instances from Configuration Graphs
    /// </summary>
    public class SqlManagerFactory : IObjectFactory
    {
        private const String MicrosoftSqlManager = "VDS.RDF.Storage.MicrosoftSqlStoreManager";
        private const String MySqlManager = "VDS.RDF.Storage.MySqlStoreManager";

        /// <summary>
        /// Tries to load a SQL IO Manager based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out Object obj)
        {
            ISqlIOManager manager = null;
            String server, port, db, user, pwd;

            //Create the URI Nodes we're going to use to search for things
            INode propServer = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyServer),
                  propDb = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyDatabase);

            //Get Server and Database details
            server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
            if (server == null) server = "localhost";
            db = ConfigurationLoader.GetConfigurationString(g, objNode, propDb);
            if (db == null)
            {
                obj = null;
                return false;
            }

            //Get user credentials
            ConfigurationLoader.GetUsernameAndPassword(g, objNode, true, out user, out pwd);

            //Based on this information create a Manager if possible
            switch (targetType.FullName)
            {
                case MicrosoftSqlManager:
                    if (user == null || pwd == null)
                    {
                        manager = new MicrosoftSqlStoreManager(server, db);
                    }
                    else
                    {
                        manager = new MicrosoftSqlStoreManager(server, db, user, pwd);
                    }
                    break;

                case MySqlManager:
                    if (user != null && pwd != null)
                    {
                        manager = new MySqlStoreManager(server, db, user, pwd);
                    }
                    break;

            }
            obj = manager;
            return (manager != null);
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
                case MicrosoftSqlManager:
                case MySqlManager:
                    return true;
                default:
                    return false;
            }
        }
    }

#endif

#if !NO_STORAGE

    /// <summary>
    /// Factory class for producing <see cref="IGenericIOManager">IGenericIOManager</see> instances from Configuration Graphs
    /// </summary>
    public class GenericManagerFactory : IObjectFactory
    {
        private const String AllegroGraph = "VDS.RDF.Storage.AllegroGraphConnector",
                             DatasetFile = "VDS.RDF.Storage.DatasetFileManager",
                             Dydra = "VDS.RDF.Storage.DydraConnector",
                             FourStore = "VDS.RDF.Storage.FourStoreConnector",
                             Fuseki = "VDS.RDF.Storage.FusekiConnector",
                             InMemory = "VDS.RDF.Storage.InMemoryManager",
                             Joseki = "VDS.RDF.Storage.JosekiConnector",
                             ReadOnly = "VDS.RDF.Storage.ReadOnlyConnector",
                             ReadOnlyQueryable = "VDS.RDF.Storage.QueryableReadOnlyConnector",
                             Sesame = "VDS.RDF.Storage.SesameHttpProtocolConnector",
                             SesameV5 = "VDS.RDF.Storage.SesameHttpProtocolVersion5Connector",
                             SesameV6 = "VDS.RDF.Storage.SesameHttpProtocolVersion6Connector",
                             Sparql = "VDS.RDF.Storage.SparqlConnector",
                             SparqlHttpProtocol = "VDS.RDF.Storage.SparqlHttpProtocolConnector",
                             Stardog = "VDS.RDF.Storage.StardogConnector",
                             Talis = "VDS.RDF.Storage.TalisPlatformConnector"
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
            IGenericIOManager manager = null;
            obj = null;

            String server, user, pwd, store;
            bool isAsync;

            Object temp;
            INode storeObj;

            //Create the URI Nodes we're going to use to search for things
            INode propServer = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyServer),
                  propDb = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyDatabase),
                  propStore = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyStore),
                  propAsync = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyAsync);

            switch (targetType.FullName)
            {
                case AllegroGraph:
                    //Get the Server, Catalog and Store
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                    if (server == null) return false;
                    String catalog = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyCatalog));
                    store = ConfigurationLoader.GetConfigurationString(g, objNode, propStore);
                    if (store == null) return false;

                    //Get User Credentials
                    ConfigurationLoader.GetUsernameAndPassword(g, objNode, true, out user, out pwd);

                    if (user != null && pwd != null)
                    {
                        manager = new AllegroGraphConnector(server, catalog, store, user, pwd);
                    }
                    else
                    {
                        manager = new AllegroGraphConnector(server, catalog, store);
                    }
                    break;

                case DatasetFile:
                    //Get the Filename and whether the loading should be done asynchronously
                    String file = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyFromFile));
                    if (file == null) return false;
                    file = ConfigurationLoader.ResolvePath(file);
                    isAsync = ConfigurationLoader.GetConfigurationBoolean(g, objNode, propAsync, false);
                    manager = new DatasetFileManager(file, isAsync);
                    break;

                case Dydra:
                    //Get the Account Name and Store
                    String account = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyCatalog));
                    if (account == null) return false;
                    store = ConfigurationLoader.GetConfigurationString(g, objNode, propStore);
                    if (store == null) return false;

                    //Get User Credentials
                    ConfigurationLoader.GetUsernameAndPassword(g, objNode, true, out user, out pwd);

                    if (user != null)
                    {
                        manager = new DydraConnector(account, store, user);
                    }
                    else
                    {
                        manager = new DydraConnector(account, store);
                    }
                    break;

                case FourStore:
                    //Get the Server and whether Updates are enabled
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                    if (server == null) return false;
                    bool enableUpdates = ConfigurationLoader.GetConfigurationBoolean(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyEnableUpdates), true);
                    manager = new FourStoreConnector(server, enableUpdates);
                    break;

                case Fuseki:
                    //Get the Server URI
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                    if (server == null) return false;
                    manager = new FusekiConnector(server);
                    break;

                case InMemory:
                    //Get the Dataset/Store
                    INode datasetObj = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUsingDataset));
                    if (datasetObj != null)
                    {
                        temp = ConfigurationLoader.LoadObject(g, datasetObj);
                        if (temp is ISparqlDataset)
                        {
                            manager = new InMemoryManager((ISparqlDataset)temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the In-Memory Manager identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingDataset property points to an Object that cannot be loaded as an object which implements the ISparqlDataset interface");
                        }
                    }
                    else
                    {
                        //If no dnr:usingDataset try dnr:usingStore instead
                        storeObj = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUsingStore));
                        if (storeObj != null)
                        {
                            temp = ConfigurationLoader.LoadObject(g, storeObj);
                            if (temp is IInMemoryQueryableStore)
                            {
                                manager = new InMemoryManager((IInMemoryQueryableStore)temp);
                            }
                            else
                            {
                                throw new DotNetRdfConfigurationException("Unable to load the In-Memory Manager identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingStore property points to an Object that cannot be loaded as an object which implements the IInMemoryQueryableStore interface");
                            }
                        }
                        else
                        {
                            //If no dnr:usingStore either then create a new empty store
                            manager = new InMemoryManager();
                        }
                    }
                    break;

                case Joseki:
                    //Get the Query and Update URIs
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                    if (server == null) return false;
                    String queryService = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyQueryPath));
                    if (queryService == null) return false;
                    String updateService = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUpdatePath));
                    if (updateService == null)
                    {
                        manager = new JosekiConnector(server, queryService);
                    }
                    else
                    {
                        manager = new JosekiConnector(server, queryService, updateService);
                    }
                    break;

                case ReadOnly:
                    //Get the actual Manager we are wrapping
                    storeObj = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyGenericManager));
                    temp = ConfigurationLoader.LoadObject(g, storeObj);
                    if (temp is IGenericIOManager)
                    {
                        manager = new ReadOnlyConnector((IGenericIOManager)temp);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the Read-Only Connector identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:genericManager property points to an Object which cannot be loaded as an object which implements the required IGenericIOManager interface");
                    }
                    break;

                case ReadOnlyQueryable:
                    //Get the actual Manager we are wrapping
                    storeObj = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyGenericManager));
                    temp = ConfigurationLoader.LoadObject(g, storeObj);
                    if (temp is IQueryableGenericIOManager)
                    {
                        manager = new QueryableReadOnlyConnector((IQueryableGenericIOManager)temp);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the Queryable Read-Only Connector identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:genericManager property points to an Object which cannot be loaded as an object which implements the required IQueryableGenericIOManager interface");
                    }
                    break;

                case Sesame:
                case SesameV5:
                case SesameV6:
                    //Get the Server and Store ID
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                    if (server == null) return false;
                    store = ConfigurationLoader.GetConfigurationString(g, objNode, propStore);
                    if (store == null) return false;
                    ConfigurationLoader.GetUsernameAndPassword(g, objNode, true, out user, out pwd);
                    if (user != null && pwd != null)
                    {
                        manager = (IGenericIOManager)Activator.CreateInstance(targetType, new Object[] { server, store, user, pwd });
                    }
                    else
                    {
                        manager = (IGenericIOManager)Activator.CreateInstance(targetType, new Object[] { server, store });
                    }
                    break;

                case Sparql:
                    //Get the Endpoint URI or the Endpoint
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyEndpointUri));

                    //What's the load mode?
                    String loadModeRaw = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyLoadMode));
                    SparqlConnectorLoadMethod loadMode = SparqlConnectorLoadMethod.Construct;
                    if (loadModeRaw != null)
                    {
                        try
                        {
#if SILVERLIGHT
                            loadMode = (SparqlConnectorLoadMethod)Enum.Parse(typeof(SparqlConnectorLoadMethod), loadModeRaw, false);
#else
                            loadMode = (SparqlConnectorLoadMethod)Enum.Parse(typeof(SparqlConnectorLoadMethod), loadModeRaw);
#endif
                        }
                        catch
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the SparqlConnector identified by the Node '" + objNode.ToString() + "' as the value given for the property dnr:loadMode is not valid");
                        }
                    }

                    if (server == null)
                    {
                        INode endpointObj = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyEndpoint));
                        if (endpointObj == null) return false;
                        temp = ConfigurationLoader.LoadObject(g, endpointObj);
                        if (temp is SparqlRemoteEndpoint)
                        {
                            manager = new SparqlConnector((SparqlRemoteEndpoint)temp, loadMode);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the SparqlConnector identified by the Node '" + objNode.ToString() + "' as the value given for the property dnr:endpoint points to an Object which cannot be loaded as an object which is of the type SparqlRemoteEndpoint");
                        }
                    }
                    else
                    {
                        //Are there any Named/Default Graph URIs
                        IEnumerable<Uri> defGraphs = from def in ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyDefaultGraphUri))
                                                     where def.NodeType == NodeType.Uri
                                                     select ((IUriNode)def).Uri;
                        IEnumerable<Uri> namedGraphs = from named in ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyNamedGraphUri))
                                                       where named.NodeType == NodeType.Uri
                                                       select ((IUriNode)named).Uri;
                        if (defGraphs.Any() || namedGraphs.Any())
                        {
                            manager = new SparqlConnector(new SparqlRemoteEndpoint(new Uri(server), defGraphs, namedGraphs), loadMode);
                        }
                        else
                        {
                            manager = new SparqlConnector(new Uri(server), loadMode);
                        }                        
                    }
                    break;

                case SparqlHttpProtocol:
                    //Get the Service URI
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                    if (server == null) return false;
                    manager = new SparqlHttpProtocolConnector(new Uri(server));
                    break;

                case Stardog:
                    //Get the Server and Store
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                    if (server == null) return false;
                    store = ConfigurationLoader.GetConfigurationString(g, objNode, propStore);
                    if (store == null) return false;

                    //Get User Credentials
                    ConfigurationLoader.GetUsernameAndPassword(g, objNode, true, out user, out pwd);

                    //Get Reasoning Mode
                    StardogReasoningMode reasoning = StardogReasoningMode.None;
                    String mode = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyLoadMode));
                    if (mode != null)
                    {
                        try
                        {
                            reasoning = (StardogReasoningMode)Enum.Parse(typeof(StardogReasoningMode), mode);
                        }
                        catch
                        {
                            reasoning = StardogReasoningMode.None;
                        }
                    }

                    if (user != null && pwd != null)
                    {
                        manager = new StardogConnector(server, store, reasoning, user, pwd);
                    }
                    else
                    {
                        manager = new StardogConnector(server, store, reasoning);
                    }
                    break;

                case Talis:
                    //Get the Store Name and User credentials
                    store = ConfigurationLoader.GetConfigurationString(g, objNode, propStore);
                    if (store == null) return false;
                    ConfigurationLoader.GetUsernameAndPassword(g, objNode, true, out user, out pwd);
                    if (user != null && pwd != null)
                    {
                        manager = new TalisPlatformConnector(store, user, pwd);
                    }
                    else
                    {
                        manager = new TalisPlatformConnector(store);
                    }
                    break;
            }

            obj = manager;
            return (manager != null);
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
                case DatasetFile:
                case Dydra:
                case FourStore:
                case Fuseki:
                case InMemory:
                case Joseki:
                case Sesame:
                case SesameV5:
                case SesameV6:
                case ReadOnly:
                case ReadOnlyQueryable:
                case Sparql:
                case SparqlHttpProtocol:
                case Stardog:
                case Talis:
                    return true;
                default:
                    return false;
            }
        }
    }

#endif
}
