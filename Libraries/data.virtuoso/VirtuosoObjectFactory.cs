using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Storage;

namespace VDS.RDF.Configuration
{
    public class VirtuosoObjectFactory : IObjectFactory
    {
        private const String NonNativeVirtuosoManager = "VDS.RDF.Storage.NonNativeVirtuosoManager",
                             Virtuoso = "VDS.RDF.Storage.VirtuosoManager";

        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;

            String server, port, db, user, pwd;
            int p = -1;

            //Create the URI Nodes we're going to use to search for things
            INode propServer = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyServer),
                  propDb = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyDatabase);

            //Get Server and Database details
            server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
            if (server == null) server = "localhost";
            db = ConfigurationLoader.GetConfigurationString(g, objNode, propDb);
            if (db == null) db = VirtuosoManager.DefaultDB;

            //Get Port
            port = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyPort));
            if (!Int32.TryParse(port, out p)) p = VirtuosoManager.DefaultPort;

            //Get user credentials
            ConfigurationLoader.GetUsernameAndPassword(g, objNode, true, out user, out pwd);
            if (user == null || pwd == null) return false;

            //Based on this information create a Manager if possible
            switch (targetType.FullName)
            {
                case NonNativeVirtuosoManager:
                    obj = new NonNativeVirtuosoManager(server, p, db, user, pwd);
                    break;

                case Virtuoso:
                    //Get Server settings
                    server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                    if (server == null) return false;

                    obj = new VirtuosoManager(server, p, db, user, pwd);

                    break;
            }

            return (obj != null);
        }

        public bool CanLoadObject(Type t)
        {
            switch (t.FullName)
            {
                case NonNativeVirtuosoManager:
                case Virtuoso:
                    return true;
                default:
                    return false;
            }
        }
    }
}
