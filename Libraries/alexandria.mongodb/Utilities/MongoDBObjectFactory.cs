using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB;
using MongoDB.Configuration;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.Alexandria.Datasets;
using VDS.Alexandria.Documents;

namespace VDS.Alexandria.Utilities
{
    public class MongoDBObjectFactory : IObjectFactory
    {
        private const String MongoDBManager = "VDS.Alexandria.AlexandriaMongoDBManager";
        private const String MongoDBDataset = "VDS.Alexandria.Datasets.AlexandriaMongoDBDataset";

        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            switch (targetType.FullName)
            {
                case MongoDBManager:
                    String server = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyServer));
                    String storeID = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyDatabase));
                    String collectionID = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyCatalog));
                    String loadMode = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyLoadMode));

                    MongoDBSchemas schema = MongoDBDocumentManager.DefaultSchema;
                    if (loadMode != null)
                    {
                        switch (loadMode)
                        {
                            case "GraphCentric":
                                schema = MongoDBSchemas.GraphCentric;
                                break;
                            case "TripleCentric":
                                schema = MongoDBSchemas.TripleCentric;
                                break;
                        }
                    }

                    if (storeID != null)
                    {
                        AlexandriaMongoDBManager manager;
                        if (server == null)
                        {
                            if (collectionID == null)
                            {
                                manager = new AlexandriaMongoDBManager(storeID, schema);
                            }
                            else
                            {
                                manager = new AlexandriaMongoDBManager(new MongoDBDocumentManager(new MongoConfiguration(), storeID, collectionID, schema));
                            }
                        }
                        else
                        {
                            //Have a Custom Connection String
                            if (collectionID == null)
                            {
                                manager = new AlexandriaMongoDBManager(new MongoDBDocumentManager(server, storeID, schema));
                            }
                            else
                            {
                                manager = new AlexandriaMongoDBManager(new MongoDBDocumentManager(server, storeID, collectionID, schema));
                            }
                        }

                        obj = manager;
                        return true;
                    }

                    //If we get here then the required dnr:database property was missing
                    throw new DotNetRdfConfigurationException("Unable to load the MongoDB Manager identified by the Node '" + objNode.ToString() + "' since there was no value given for the required property dnr:database");
                    break;

                case MongoDBDataset:
                    INode managerNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, "dnr:genericManager"));
                    if (managerNode == null)
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the MongoDB Dataset identified by the Node '" + objNode.ToString() + "' since there we no value for the required property dnr:genericManager");
                    }

                    Object temp = ConfigurationLoader.LoadObject(g, managerNode);
                    if (temp is AlexandriaMongoDBManager)
                    {
                        obj = new AlexandriaMongoDBDataset((AlexandriaMongoDBManager)temp);
                        return true;
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the MongoDB Dataset identified by the Node '" + objNode.ToString() + "' since the value for the dnr:genericManager property pointed to an Object which could not be loaded as an object of type AlexandriaMongoDBManager");
                    }

                    break;
            }

            obj = null;
            return false;
        }

        public bool CanLoadObject(Type t)
        {
            switch (t.FullName)
            {
                case MongoDBManager:
                case MongoDBDataset:
                    return true;
                default:
                    return false;
            }
        }
    }
}
