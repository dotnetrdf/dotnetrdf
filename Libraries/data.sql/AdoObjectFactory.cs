using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Storage;

namespace VDS.RDF.Configuration
{
    public class AdoObjectFactory : IObjectFactory
    {
        private const String MicrosoftAdoManager = "VDS.RDF.Storage.MicrosoftAdoManager",
                             MicrosoftAdoDataset = "VDS.RDF.Query.Datasets.MicrosoftAdoDataset",
                             MicrosoftAdoOptimiser = "VDS.RDF.Query.Optimisation.MicrosoftAdoOptimiser";

        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;

            INode managerNode;
            Object temp;

            //Create the URI Nodes we're going to use to search for things
            INode propServer = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyServer),
                  propDb = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyDatabase);


            switch (targetType.FullName)
            {
                case MicrosoftAdoDataset:
                    managerNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyGenericManager));
                    if (managerNode != null)
                    {
                        temp = ConfigurationLoader.LoadObject(g, managerNode);
                        if (temp is MicrosoftAdoManager)
                        {
                            obj = new MicrosoftAdoDataset((MicrosoftAdoManager)temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the object identified by the Node '" + objNode.ToString() + "' since the object pointed to by the dnr:genericManager property could not be loaded as an instance of the required MicrosoftAdoManager class");
                        }
                    } 
                    else 
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the object identified by the Node '" + objNode.ToString() + "' as it specified a dnr:type of MicrosoftAdoDataset but failed to specify a dnr:genericManager property to point to a MicrosoftAdoManager");
                    }
                    break;

                case MicrosoftAdoManager:
                    //Get Server and Database details
                    String server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                    if (server == null) server = "localhost";
                    String db = ConfigurationLoader.GetConfigurationString(g, objNode, propDb);
                    if (db == null) return false;

                    //Get user credentials
                    String user, pwd;
                    ConfigurationLoader.GetUsernameAndPassword(g, objNode, true, out user, out pwd);

                    //Create the Manager
                    if (user != null && pwd != null)
                    {
                        obj = new MicrosoftAdoManager(server, db, user, pwd);
                    }
                    else
                    {
                        obj = new MicrosoftAdoManager(server, db);
                    }
                    break;

                case MicrosoftAdoOptimiser:
                    managerNode = ConfigurationLoader.GetConfigurationNode(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyGenericManager));
                    if (managerNode != null)
                    {
                        temp = ConfigurationLoader.LoadObject(g, managerNode);
                        if (temp is MicrosoftAdoManager)
                        {
                            obj = new MicrosoftAdoOptimiser((MicrosoftAdoManager)temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the object identified by the Node '" + objNode.ToString() + "' since the object pointed to by the dnr:genericManager property could not be loaded as a MicrosoftAdoManager instance");
                        }
                    }
                    else
                    {
                        throw new DotNetRdfConfigurationException("Unable to load the object identified by the Node '" + objNode.ToString() + "' as it specified a dnr:type of AdoOptimiser but failed to specify a dnr:genericManager property to point to a MicrosoftAdoManager instance");
                    }
                    break;
            }

            return (obj != null);
        }

        public bool CanLoadObject(Type t)
        {
            switch (t.FullName)
            {
                case MicrosoftAdoDataset:
                case MicrosoftAdoManager:
                case MicrosoftAdoOptimiser:
                    return true;
                default:
                    return false;
            }
        }
    }
}
