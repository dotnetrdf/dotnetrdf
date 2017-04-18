/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
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
                    String action = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyAction)));
                    result = new Permission(action);
                    break;

                case PermissionSet:
                    IEnumerable<String> actions = from n in ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyAction)))
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

                    // Get the members of the Group
                    IEnumerable<INode> members = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyMember)));
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

                    // Get the allow list for the Group
                    IEnumerable<INode> allowed = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyAllow)));
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

                    // Get the deny list for the Group
                    IEnumerable<INode> denied = ConfigurationLoader.GetConfigurationData(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyDeny)));
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

                    // Does the User Group require authentication?
                    result.AllowGuests = !ConfigurationLoader.GetConfigurationBoolean(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyRequiresAuthentication)), true);

                    // Is there a permission model specified?
                    String mode = ConfigurationLoader.GetConfigurationString(g, objNode, g.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyPermissionModel)));
                    if (mode != null)
                    {
                        result.PermissionModel = (PermissionModel)Enum.Parse(typeof(PermissionModel), mode);
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
