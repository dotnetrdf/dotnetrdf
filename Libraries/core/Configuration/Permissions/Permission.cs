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

namespace VDS.RDF.Configuration.Permissions
{
    /// <summary>
    /// Possible permission models
    /// </summary>
    public enum PermissionModel
    {
        /// <summary>
        /// If the action appears in the deny list it is denied unless it is in the allow list, otherwise it is allowed
        /// </summary>
        DenyAllow,
        /// <summary>
        /// If the action appears in the allow list it is allowed unless it is in the deny list, otherwise it is denied
        /// </summary>
        AllowDeny,
        /// <summary>
        /// All actions are allowed
        /// </summary>
        AllowAny,
        /// <summary>
        /// All actions are denied
        /// </summary>
        DenyAny
    }

    /// <summary>
    /// Interface for Permission
    /// </summary>
    public interface IPermission
    {
        /// <summary>
        /// Gets whether the Permission is for a specific action
        /// </summary>
        /// <param name="action">Action</param>
        /// <returns></returns>
        bool IsPermissionFor(String action);
    }

    /// <summary>
    /// Represents a action that can be allowed/denied
    /// </summary>
    public class Permission : IPermission
    {
        private String _action;

        /// <summary>
        /// Creates a new Permission for the given Action
        /// </summary>
        /// <param name="action">Action</param>
        public Permission(String action)
        {
            this._action = action;
        }

        /// <summary>
        /// Gets whether the Permission is for the given action
        /// </summary>
        /// <param name="action">Action</param>
        /// <returns></returns>
        public bool IsPermissionFor(String action)
        {
            return this._action.Equals(action);
        }
    }

    /// <summary>
    /// Represents a set of Permissions that can be allowed/denied
    /// </summary>
    public class PermissionSet : IPermission
    {
        private List<String> _actions = new List<string>();

        /// <summary>
        /// Creates a new Permissions Set
        /// </summary>
        /// <param name="action">Action</param>
        public PermissionSet(String action)
        {
            this._actions.Add(action);
        }

        /// <summary>
        /// Creates a new Permissions Set
        /// </summary>
        /// <param name="actions">Actions</param>
        public PermissionSet(IEnumerable<String> actions)
        {
            this._actions.AddRange(actions);
        }

        /// <summary>
        /// Gets whether the Permission is for the given action
        /// </summary>
        /// <param name="action">Action</param>
        /// <returns></returns>
        public bool IsPermissionFor(String action)
        {
            return this._actions.Any(a => a.Equals(action));
        }
    }
}
