using System;
using VDS.RDF.Configuration;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;
using VDS.RDF.Update;
using VDS.RDF.Update.Protocol;

namespace VDS.RDF.Web.Configuration
{
    /// <summary>
    /// Factory class for producing SPARQL Graph Store HTTP Protocol Processors from Configuration Graphs.
    /// </summary>
    public class ProtocolProcessorFactory
        : IObjectFactory
    {
        private const string ProtocolToUpdateProcessor = "VDS.RDF.Update.Protocol.ProtocolToUpdateProcessor",
                             LeviathanProtocolProcessor = "VDS.RDF.Update.Protocol.LeviathanProtocolProcessor",
                             GenericProtocolProcessor = "VDS.RDF.Update.Protocol.GenericProtocolProcessor";

        /// <summary>
        /// Tries to load a SPARQL Graph Store HTTP Protocol Processor based on information from the Configuration Graph.
        /// </summary>
        /// <param name="g">Configuration Graph.</param>
        /// <param name="objNode">Object Node.</param>
        /// <param name="targetType">Target Type.</param>
        /// <param name="obj">Output Object.</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            ISparqlHttpProtocolProcessor processor = null;
            object temp;

            INode propStorageProvider = g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyStorageProvider));

            switch (targetType.FullName)
            {
                case ProtocolToUpdateProcessor:
                    INode qNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyQueryProcessor)));
                    INode uNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUpdateProcessor)));
                    if (qNode == null || uNode == null) return false;

                    var queryProc = ConfigurationLoader.LoadObject(g, qNode);
                    var updateProc = ConfigurationLoader.LoadObject(g, uNode);

                    if (queryProc is ISparqlQueryProcessor)
                    {
                        if (updateProc is ISparqlUpdateProcessor)
                        {
                            processor = new ProtocolToUpdateProcessor((ISparqlQueryProcessor)queryProc, (ISparqlUpdateProcessor)updateProc, UriFactory.Root);
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
                    INode datasetNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingDataset)));
                    if (datasetNode != null)
                    {
                        temp = ConfigurationLoader.LoadObject(g, datasetNode);
                        if (temp is ISparqlDataset)
                        {
                            processor = new LeviathanProtocolProcessor((ISparqlDataset)temp, UriFactory.Root);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the Leviathan Protocol Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingDataset property points to an Object that cannot be loaded as an object which implements the ISparqlDataset interface");
                        }
                    }
                    else
                    {
                        INode storeNode = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingStore)));
                        if (storeNode == null) return false;

                        var store = ConfigurationLoader.LoadObject(g, storeNode);

                        if (store is IInMemoryQueryableStore)
                        {
                            processor = new LeviathanProtocolProcessor((IInMemoryQueryableStore)store, UriFactory.Root);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the SPARQL HTTP Protocol Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:usingStore property points to an Object that cannot be loaded as an object which implements the IInMemoryQueryableStore interface");
                        }
                    }
                    break;

                case GenericProtocolProcessor:
                    INode managerObj = ConfigurationLoader.GetConfigurationNode(g, objNode, propStorageProvider);
                    if (managerObj == null) return false;
                    temp = ConfigurationLoader.LoadObject(g, managerObj);
                    if (temp is IStorageProvider)
                    {
                        processor = new GenericProtocolProcessor((IStorageProvider)temp, UriFactory.Root);
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the Generic Protocol Processor identified by the Node '" + objNode.ToString() + "' as the value given for the dnr:genericManager property points to an Object that cannot be loaded as an object which implements the IStorageProvider interface");
                    }
                    break;

            }

            obj = processor;
            return (processor != null);
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
                case ProtocolToUpdateProcessor:
                case LeviathanProtocolProcessor:
                case GenericProtocolProcessor:
                    return true;
                default:
                    return false;
            }
        }
    }
}
