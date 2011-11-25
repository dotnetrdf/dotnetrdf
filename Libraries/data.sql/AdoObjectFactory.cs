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
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Storage;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// An Object Factory that can create objects of the classes provided by the dotNetRDF.Data.Sql library
    /// </summary>
    public class AdoObjectFactory 
        : IObjectFactory
    {
        private const String AzureAdoManager = "VDS.RDF.Storage.AzureAdoManager",
                             MicrosoftAdoManager = "VDS.RDF.Storage.MicrosoftAdoManager",
                             MicrosoftAdoDataset = "VDS.RDF.Query.Datasets.MicrosoftAdoDataset",
                             MicrosoftAdoOptimiser = "VDS.RDF.Query.Optimisation.MicrosoftAdoOptimiser";

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

                case AzureAdoManager:
                case MicrosoftAdoManager:
                    //Get Server and Database details
                    String server = ConfigurationLoader.GetConfigurationString(g, objNode, propServer);
                    if (server == null) server = "localhost";
                    String db = ConfigurationLoader.GetConfigurationString(g, objNode, propDb);
                    if (db == null) return false;

                    //Get user credentials
                    String user, pwd;
                    ConfigurationLoader.GetUsernameAndPassword(g, objNode, true, out user, out pwd);

                    bool encrypt = ConfigurationLoader.GetConfigurationBoolean(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyEncryptConnection), false);

                    String mode = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyLoadMode));
                    AdoAccessMode accessMode = (mode != null ? (AdoAccessMode)Enum.Parse(typeof(AdoAccessMode), mode) : AdoAccessMode.Streaming);

                    //Create the Manager
                    if (user != null && pwd != null)
                    {
                        if (targetType.FullName.Equals(MicrosoftAdoManager))
                        {
                            obj = new MicrosoftAdoManager(server, db, user, pwd, encrypt, accessMode);
                        }
                        else
                        {
                            obj = new AzureAdoManager(server, db, user, pwd, accessMode);
                        }
                    }
                    else
                    {
                        if (targetType.FullName.Equals(AzureAdoManager)) throw new DotNetRdfConfigurationException("Unable to load the object identified by the Node '" + objNode.ToString() + "' as an AzureAdoManager as the required dnr:user and dnr:password properties were missing");
                        obj = new MicrosoftAdoManager(server, db, encrypt, accessMode);
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

        /// <summary>
        /// Returns whether this Factory is capable of creating objects of the given type
        /// </summary>
        /// <param name="t">Target Type</param>
        /// <returns></returns>
        public bool CanLoadObject(Type t)
        {
            switch (t.FullName)
            {
                case AzureAdoManager:
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
