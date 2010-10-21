using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;

namespace VDS.RDF.Configuration
{
    public class DatasetFactory : IObjectFactory
    {
        private const String InMemoryDataset = "VDS.RDF.Query.Datasets.InMemoryDataset",
                             SqlDataset = "VDS.RDF.Query.Datasets.SqlDataset";

        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;

            switch (targetType.FullName)
            {
                case InMemoryDataset:
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
                    break;

#if !NO_DATA && !NO_STORAGE

                case SqlDataset:
                    INode sqlManager = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertySqlManager));
                    if (sqlManager == null)
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the SQL Dataset identified by the Node '" + objNode.ToString() + "' since there was no value given for the required property dnr:sqlManager");
                    }
                    else
                    {
                        Object temp = ConfigurationLoader.LoadObject(g, sqlManager);
                        if (temp is IDotNetRDFStoreManager)
                        {
                            obj = new SqlDataset((IDotNetRDFStoreManager)temp);
                            return true;
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the SQL Dataset identified by the Node '" + objNode.ToString() + "' since the Object pointed to by the dnr:sqlManager property could not be laoded as an object which implements the IDotNetRDFStoreManager interface");
                        }
                    }
                    break;

#endif
            } 

            return false;
        }

        public bool CanLoadObject(Type t)
        {
            switch (t.FullName)
            {
                case InMemoryDataset:
#if !NO_DATA && !NO_STORAGE
                case SqlDataset:
#endif
                    return true;
                default:
                    return false;
            }
        }
    }
}
