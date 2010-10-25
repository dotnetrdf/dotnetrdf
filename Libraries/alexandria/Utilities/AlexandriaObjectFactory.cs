using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Configuration;
using VDS.Alexandria.Datasets;
using VDS.Alexandria.Documents;

namespace VDS.Alexandria.Utilities
{
    public class AlexandriaObjectFactory : IObjectFactory
    {
        private const String FileManager = "VDS.Alexandria.AlexandriaFileManager";
        private const String FileDataset = "VDS.Alexandria.Datasets.AlexandriaFileDataset";

        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            switch (targetType.FullName)
            {
                case FileManager:
                    String storeID = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyDatabase));

                    if (storeID != null)
                    {
                        //TODO: Add support for using dnr:loadMode to specify the indexes to be used
                        obj = new AlexandriaFileManager(ConfigurationLoader.ResolvePath(storeID));
                        return true;
                    }

                    //If we get here then the required dnr:database property was missing
                    throw new DotNetRdfConfigurationException("Unable to load the File Manager identified by the Node '" + objNode.ToString() + "' since there was no value given for the required property dnr:database");
                    break;

                case FileDataset:
                    INode managerNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, "dnr:genericManager"));
                    if (managerNode == null)
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the File Dataset identified by the Node '" + objNode.ToString() + "' since there we no value for the required property dnr:genericManager");
                    }

                    Object temp = ConfigurationLoader.LoadObject(g, managerNode);
                    if (temp is AlexandriaFileManager)
                    {
                        obj = new AlexandriaFileDataset((AlexandriaFileManager)temp);
                        return true;
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the File Dataset identified by the Node '" + objNode.ToString() + "' since the value for the dnr:genericManager property pointed to an Object which could not be loaded as an object of type AlexandriaFileManager");
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
                case FileManager:
                case FileDataset:
                    return true;
                default:
                    return false;
            }
        }
    }
}
