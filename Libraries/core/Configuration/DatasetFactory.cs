using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Configuration
{
    public class DatasetFactory : IObjectFactory
    {
        private const String InMemoryDataset = "VDS.RDF.Query.Datasets.InMemoryDataset";

        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            if (targetType.FullName.Equals(InMemoryDataset))
            {
                INode storeNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyUsingStore));
                if (storeNode == null)
                {
                    throw new DotNetRdfConfigurationException("Unable to load the In-Memory Dataset identified by the Node '" + objNode.ToString() + "' since there was no value given for the required property dnr:usingStore");
                }
                else
                {
                    Object temp = ConfigurationLoader.LoadObject(g, storeNode);
                    if (temp is IInMemoryQueryableStore)
                    {
                        obj = new InMemoryDataset((IInMemoryQueryableStore)temp);
                        return true;
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the In-Memory Dataset identified by the Node '" + objNode.ToString() + "' since the Object pointed to by the dnr:usingStore property could not be loaded as an object which implements the IInMemoryQueryableStore interface");
                    }
                }
            } 

            obj = null;
            return false;
        }

        public bool CanLoadObject(Type t)
        {
            return t.FullName.Equals(InMemoryDataset);
        }
    }
}
