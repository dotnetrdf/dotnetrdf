using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Configuration
{
    public class AdoObjectFactory : IObjectFactory
    {
        private const String MicrosoftAdoManager = "VDS.RDF.Storage.MicrosoftAdoManager";

        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;

            //Create the URI Nodes we're going to use to search for things
            INode propServer = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyServer),
                  propDb = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyDatabase);

            //Get Server and Database details
            String server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
            if (server == null) server = "localhost";
            String db = ConfigurationLoader.GetConfigurationString(g, objNode, propDb);
            if (db == null) return false;

            //Get user credentials
            String user, pwd;
            ConfigurationLoader.GetUsernameAndPassword(g, objNode, true, out user, out pwd);

            switch (targetType.FullName)
            {
                case MicrosoftAdoManager:
                    if (user != null && pwd != null)
                    {
                        obj = new MicrosoftAdoManager(server, db, user, pwd);
                    }
                    else
                    {
                        obj = new MicrosoftAdoManager(server, db);
                    }
                    break;
            }

            return (obj != null);
        }

        public bool CanLoadObject(Type t)
        {
            switch (t.FullName)
            {
                case MicrosoftAdoManager:
                    return true;
                default:
                    return false;
            }
        }
    }
}
