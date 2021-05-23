using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;

namespace VDS.RDF.Configuration
{
    public class InMemoryQueryProcessorFactory
    {
        private const string LeviathanQueryProcessor = "VDS.RDF.Query.LeviathanQueryProcessor";

        /// <summary>
        /// Tries to load a SPARQL Query Processor based on information from the Configuration Graph.
        /// </summary>
        /// <param name="g">Configuration Graph.</param>
        /// <param name="objNode">Object Node.</param>
        /// <param name="targetType">Target Type.</param>
        /// <param name="obj">Output Object.</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            ISparqlQueryProcessor processor = null;
            INode storeObj;
            object temp;

            INode propStorageProvider = g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyStorageProvider));

            switch (targetType.FullName)
            {
                case LeviathanQueryProcessor:
                    INode datasetObj = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingDataset)));
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
                        // If no dnr:usingDataset try dnr:usingStore instead
                        storeObj = ConfigurationLoader.GetConfigurationNode(g, objNode, g.CreateUriNode(g.UriFactory.Create(ConfigurationLoader.PropertyUsingStore)));
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
                case LeviathanQueryProcessor:
                    return true;
                default:
                    return false;
            }
        }
    }
}
