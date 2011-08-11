using System;
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

using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Inference.Pellet;
using VDS.RDF.Storage;
using VDS.RDF.Update;
using VDS.RDF.Update.Protocol;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Factory class for producing SPARQL Query Processors from Configuration Graphs
    /// </summary>
    public class QueryProcessorFactory : IObjectFactory
    {
        private const String LeviathanQueryProcessor = "VDS.RDF.Query.LeviathanQueryProcessor",
                             SimpleQueryProcessor = "VDS.RDF.Query.SimpleQueryProcessor",
                             GenericQueryProcessor = "VDS.RDF.Query.GenericQueryProcessor",
                             RemoteQueryProcessor = "VDS.RDF.Query.RemoteQueryProcessor",
                             PelletQueryProcessor = "VDS.RDF.Query.PelletQueryProcessor";

        /// <summary>
        /// Tries to load a SPARQL Query Processor based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            ISparqlQueryProcessor processor = null;
            INode storeObj;
            Object temp;

            switch (targetType.FullName)
            {
                case LeviathanQueryProcessor:
                    INode datasetObj = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUsingDataset));
                    if (datasetObj != null)
                    {
                        temp = ConfigurationLoader.LoadObject(g, datasetObj);
                        if (temp is ISparqlDataset)
                        {
                            processor = new LeviathanQueryProcessor((ISparqlDataset)temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the Leviathan Query Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingDataset property points to an Object that cannot be loaded as an object which implements the ISparqlDataset interface");
                        }
                    }
                    else
                    {
                        //If no dnr:usingDataset try dnr:usingStore instead
                        storeObj = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUsingStore));
                        if (storeObj == null) return false;
                        temp = ConfigurationLoader.LoadObject(g, storeObj);
                        if (temp is IInMemoryQueryableStore)
                        {
                            processor = new LeviathanQueryProcessor((IInMemoryQueryableStore)temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the Leviathan Query Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingStore property points to an Object that cannot be loaded as an object which implements the IInMemoryQueryableStore interface");
                        }
                    }
                    break;

                case SimpleQueryProcessor:
                    storeObj = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUsingStore));
                    if (storeObj == null) return false;
                    temp = ConfigurationLoader.LoadObject(g, storeObj);
                    if (temp is INativelyQueryableStore)
                    {
                        processor = new SimpleQueryProcessor((INativelyQueryableStore)temp);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the Simple Query Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingStore property points to an Object that cannot be loaded as an object which implements the INativelyQueryableStore interface");
                    }
                    break;

#if !NO_STORAGE

                case GenericQueryProcessor:
                    INode managerObj = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyGenericManager));
                    if (managerObj == null) return false;
                    temp = ConfigurationLoader.LoadObject(g, managerObj);
                    if (temp is IQueryableGenericIOManager)
                    {
                        processor = new GenericQueryProcessor((IQueryableGenericIOManager)temp);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the Generic Query Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:genericManager property points to an Object that cannot be loaded as an object which implements the IQueryableGenericIOManager interface");
                    }
                    break;

#endif

#if !SILVERLIGHT
                case RemoteQueryProcessor:
                    INode endpointObj = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyEndpoint));
                    if (endpointObj == null) return false;
                    temp = ConfigurationLoader.LoadObject(g, endpointObj);
                    if (temp is SparqlRemoteEndpoint)
                    {
                        processor = new RemoteQueryProcessor((SparqlRemoteEndpoint)temp);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the Remote Query Processor identified by the Node '" + objNode.ToSafeString() + "' as the value given for the dnr:endpoint property points to an Object that cannot be loaded as an object which is a SparqlRemoteEndpoint");
                    }
                    break;


                case PelletQueryProcessor:
                    String server = ConfigurationLoader.GetConfigurationValue(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyServer));
                    if (server == null) return false;
                    String kb = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyStore));
                    if (kb == null) return false;

                    processor = new PelletQueryProcessor(new Uri(server), kb);
                    break;
#endif
            }

            obj = processor;
            return (processor != null);
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
                case LeviathanQueryProcessor:
                case SimpleQueryProcessor:
                case GenericQueryProcessor:
                case RemoteQueryProcessor:
                case PelletQueryProcessor:
                    return true;
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Factory class for producing SPARQL Update Processors from Configuration Graphs
    /// </summary>
    public class UpdateProcessorFactory : IObjectFactory
    {
        private const String LeviathanUpdateProcessor = "VDS.RDF.Update.LeviathanUpdateProcessor",
                             SimpleUpdateProcessor = "VDS.RDF.Update.SimpleUpdateProcessor",
                             GenericUpdateProcessor = "VDS.RDF.Update.GenericUpdateProcessor";

        /// <summary>
        /// Tries to load a SPARQL Update based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            ISparqlUpdateProcessor processor = null;
            INode storeObj;
            Object temp;

            switch (targetType.FullName)
            {
                case LeviathanUpdateProcessor:
                    INode datasetObj = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUsingDataset));
                    if (datasetObj != null)
                    {
                        temp = ConfigurationLoader.LoadObject(g, datasetObj);
                        if (temp is ISparqlDataset)
                        {
                            processor = new LeviathanUpdateProcessor((ISparqlDataset)temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the Leviathan Update Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingDataset property points to an Object that cannot be loaded as an object which implements the ISparqlDataset interface");
                        }
                    }
                    else
                    {
                        storeObj = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUsingStore));
                        if (storeObj == null) return false;
                        temp = ConfigurationLoader.LoadObject(g, storeObj);
                        if (temp is IInMemoryQueryableStore)
                        {
                            processor = new LeviathanUpdateProcessor((IInMemoryQueryableStore)temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the Leviathan Update Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingStore property points to an Object that cannot be loaded as an object which implements the IInMemoryQueryableStore interface");
                        }
                    }
                    break;

                case SimpleUpdateProcessor:
                    storeObj = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUsingStore));
                    if (storeObj == null) return false;
                    temp = ConfigurationLoader.LoadObject(g, storeObj);
                    if (temp is IUpdateableTripleStore)
                    {
                        processor = new SimpleUpdateProcessor((IUpdateableTripleStore)temp);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the Simple Update Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingStore property points to an Object that cannot be loaded as an object which implements the IUpdateableTripleStore interface");
                    }
                    break;

#if !NO_STORAGE

                case GenericUpdateProcessor:
                    INode managerObj = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyGenericManager));
                    if (managerObj == null) return false;
                    temp = ConfigurationLoader.LoadObject(g, managerObj);
                    if (temp is IGenericIOManager)
                    {
                        processor = new GenericUpdateProcessor((IGenericIOManager)temp);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the Generic Update Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:genericManager property points to an Object that cannot be loaded as an object which implements the IGenericIOManager interface");
                    }

                    break;
#endif
            }

            obj = processor;
            return (processor != null);
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
                case LeviathanUpdateProcessor:
                case SimpleUpdateProcessor:
                case GenericUpdateProcessor:
                    return true;
                default:
                    return false;
            }
        }
    }

#if !NO_WEB && !NO_ASP

    /// <summary>
    /// Factory class for producing SPARQL Graph Store HTTP Protocol Processors from Configuration Graphs
    /// </summary>
    public class ProtocolProcessorFactory : IObjectFactory
    {
        private const String ProtocolToUpdateProcessor = "VDS.RDF.Update.Protocol.ProtocolToUpdateProcessor",
                             LeviathanProtocolProcessor = "VDS.RDF.Update.Protocol.LeviathanProtocolProcessor",
                             GenericProtocolProcessor = "VDS.RDF.Update.Protocol.GenericProtocolProcessor";

        /// <summary>
        /// Tries to load a SPARQL Graph Store HTTP Protocol Processor based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            ISparqlHttpProtocolProcessor processor = null;
            Object temp;

            switch (targetType.FullName)
            {
                case ProtocolToUpdateProcessor:
                    INode qNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyQueryProcessor));
                    INode uNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUpdateProcessor));
                    if (qNode == null || uNode == null) return false;

                    Object queryProc = ConfigurationLoader.LoadObject(g, qNode);
                    Object updateProc = ConfigurationLoader.LoadObject(g, uNode);

                    if (queryProc is ISparqlQueryProcessor)
                    {
                        if (updateProc is ISparqlUpdateProcessor)
                        {
                            processor = new ProtocolToUpdateProcessor((ISparqlQueryProcessor)queryProc, (ISparqlUpdateProcessor)updateProc);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the SPARQL HTTP Protocol Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:updateProcessor property points to an Object that cannot be loaded as an object which implements the ISparqlUpdateProcessor interface");
                        }
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the SPARQL HTTP Protocol Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:queryProcessor property points to an Object that cannot be loaded as an object which implements the ISparqlQueryProcessor interface");
                    }

                    break;

                case LeviathanProtocolProcessor:
                    INode datasetNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUsingDataset));
                    if (datasetNode != null)
                    {
                        temp = ConfigurationLoader.LoadObject(g, datasetNode);
                        if (temp is ISparqlDataset)
                        {
                            processor = new LeviathanProtocolProcessor((ISparqlDataset)temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the Leviathan Protocol Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingDataset property points to an Object that cannot be loaded as an object which implements the ISparqlDataset interface");
                        }
                    }
                    else
                    {
                        INode storeNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUsingStore));
                        if (storeNode == null) return false;

                        Object store = ConfigurationLoader.LoadObject(g, storeNode);

                        if (store is IInMemoryQueryableStore)
                        {
                            processor = new LeviathanProtocolProcessor((IInMemoryQueryableStore)store);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the SPARQL HTTP Protocol Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingStore property points to an Object that cannot be loaded as an object which implements the IInMemoryQueryableStore interface");
                        }
                    }
                    break;

#if !NO_STORAGE

                case GenericProtocolProcessor:
                    INode managerObj = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyGenericManager));
                    if (managerObj == null) return false;
                    temp = ConfigurationLoader.LoadObject(g, managerObj);
                    if (temp is IGenericIOManager)
                    {
                        processor = new GenericProtocolProcessor((IGenericIOManager)temp);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the Generic Protocol Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:genericManager property points to an Object that cannot be loaded as an object which implements the IGenericIOManager interface");
                    }
                    break;

#endif
            }

            obj = processor;
            return (processor != null);
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
                case ProtocolToUpdateProcessor:
                case LeviathanProtocolProcessor:
                case GenericProtocolProcessor:
                    return true;
                default:
                    return false;
            }
        }
    }

#endif
}
