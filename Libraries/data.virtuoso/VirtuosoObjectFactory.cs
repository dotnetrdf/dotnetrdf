/*

Copyright Robert Vesse 2009-11
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using VDS.RDF.Storage;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// An Object Factory that can create objects of the classes provided by the dotNetRDF.Data.Virtuoso library
    /// </summary>
    public class VirtuosoObjectFactory
        : IObjectFactory
    {
        private const String NonNativeVirtuosoManager = "VDS.RDF.Storage.NonNativeVirtuosoManager",
                             Virtuoso = "VDS.RDF.Storage.VirtuosoManager";

        /// <summary>
        /// Attempts to load an Object of the given type identified by the given Node and returned as the Type that this loader generates
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Created Object</param>
        /// <returns>True if the loader succeeded in creating an Object</returns>
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

        /// <summary>
        /// Returns whether this Factory is capable of creating objects of the given type
        /// </summary>
        /// <param name="t">Target Type</param>
        /// <returns></returns>
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
