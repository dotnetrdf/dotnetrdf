using System;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Update;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Factory class for producing SPARQL Update Processors from Configuration Graphs.
    /// </summary>
    public class InMemoryUpdateProcessorFactory
        : IObjectFactory
    {
        private const string LeviathanUpdateProcessor = "VDS.RDF.Update.LeviathanUpdateProcessor";

        /// <summary>
        /// Tries to load a SPARQL Update based on information from the Configuration Graph.
        /// </summary>
        /// <param name="g">Configuration Graph.</param>
        /// <param name="objNode">Object Node.</param>
        /// <param name="targetType">Target Type.</param>
        /// <param name="obj">Output Object.</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            ISparqlUpdateProcessor processor = null;
            INode storeObj;
            object temp;

            INode propStorageProvider = g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyStorageProvider));

            switch (targetType.FullName)
            {
                case LeviathanUpdateProcessor:
                    INode datasetObj = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingDataset)));
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
                        storeObj = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingStore)));
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
                case LeviathanUpdateProcessor:
                    return true;
                default:
                    return false;
            }
        }
    }
}
