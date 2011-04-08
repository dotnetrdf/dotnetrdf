/*

Copyright Robert Vesse 2009-10
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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using VDS.RDF.Configuration.Permissions;

namespace VDS.RDF.Configuration
{
    /// <summary>
    /// Factory class for producing Permissions from Configuration Graphs
    /// </summary>
    public class PermissionFactory : IObjectFactory
    {
        private const String Permission = "VDS.RDF.Configuration.Permissions.Permission",
                             PermissionSet = "VDS.RDF.Configuration.Permissions.PermissionSet";

        /// <summary>
        /// Tries to load a Permission based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            IPermission result = null;

            switch (targetType.FullName)
            {
                case Permission:
                    String action = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyAction));
                    result = new Permission(action);
                    break;

                case PermissionSet:
                    IEnumerable<String> actions = from n in ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyAction))
                                                  where n.NodeType == NodeType.Literal
                                                  select ((ILiteralNode)n).Value;
                    result = new PermissionSet(actions);
                    break;
            }

            obj = result;
            return (result != null);
        }

        /// <summary>
        /// Gets whether this Factory can load objects of the given Type
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns></returns>
        public bool CanLoadObject(Type t)
        {
            switch (t.FullName)
            {
                case Permission:
                case PermissionSet:
                    return true;
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Factory class for producing User Groups from Configuration Graphs
    /// </summary>
    public class UserGroupFactory : IObjectFactory
    {
        private const String UserGroup = "VDS.RDF.Configuration.Permissions.UserGroup";

        /// <summary>
        /// Tries to load a User Group based on information from the Configuration Graph
        /// </summary>
        /// <param name="g">Configuration Graph</param>
        /// <param name="objNode">Object Node</param>
        /// <param name="targetType">Target Type</param>
        /// <param name="obj">Output Object</param>
        /// <returns></returns>
        public bool  TryLoadObject(IGraph g, INode objNode, Type targetType, out object obj)
        {
            obj = null;
            UserGroup result = null;

            switch (targetType.FullName)
            {
                case UserGroup:
                    result = new UserGroup();

                    //Get the members of the Group
                    IEnumerable<INode> members = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyMember));
                    foreach (INode member in members)
                    {
                        String username, password;
                        ConfigurationLoader.GetUsernameAndPassword(g, member, true, out username, out password);
                        if (username != null && password != null)
                        {
                            result.AddUser(new NetworkCredential(username, password));
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the User identified by the Node '" + member.ToString() + "' as there does not appear to be a valid username and password specified for this User either via the dnr:user and dnr:password properties or via a dnr:credentials property");
                        }
                    }

                    //Get the allow list for the Group
                    IEnumerable<INode> allowed = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyAllow));
                    foreach (INode allow in allowed)
                    {
                        Object temp = ConfigurationLoader.LoadObject(g, allow);
                        if (temp is IPermission)
                        {
                            result.AddAllowedAction((IPermission)temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the Permission identified by the Node '" + allow.ToString() + "' as the Object specified could not be loaded as an object which implements the IPermission interface");
                        }
                    }

                    //Get the deny list for the Group
                    IEnumerable<INode> denied = ConfigurationLoader.GetConfigurationData(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyDeny));
                    foreach (INode deny in denied)
                    {
                        Object temp = ConfigurationLoader.LoadObject(g, deny);
                        if (temp is IPermission)
                        {
                            result.AddDeniedAction((IPermission)temp);
                        }
                        else
                        {
                            throw new DotNetRdfConfigurationException("Unable to load the Permission identified by the Node '" + deny.ToString() + "' as the Object specified could not be loaded as an object which implements the IPermission interface");
                        }
                    }

                    //Does the User Group require authentication?
                    result.AllowGuests = !ConfigurationLoader.GetConfigurationBoolean(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyRequiresAuthentication), true);

                    //Is there a permission model specified?
                    String mode = ConfigurationLoader.GetConfigurationString(g, objNode, ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyPermissionModel));
                    if (mode != null)
                    {
#if SILVERLIGHT
                        result.PermissionModel = (PermissionModel)Enum.Parse(typeof(PermissionModel), mode, false);
#else
                        result.PermissionModel = (PermissionModel)Enum.Parse(typeof(PermissionModel), mode);
#endif
                    }

                    break;
            }

            obj = result;
            return (result != null);
        }

        /// <summary>
        /// Gets whether this Factory can load objects of the given Type
        /// </summary>
        /// <param name="t">Type</param>
        /// <returns></returns>
        public bool  CanLoadObject(Type t)
        {
            switch (t.FullName)
            {
                case UserGroup:
                    return true;
                default:
                    return false;
            }
        }
    }
}
