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

namespace VDS.RDF.Configuration.Permissions
{
    /// <summary>
    /// Represents a Group of Users and the permissions they have to perform actions
    /// </summary>
    public class UserGroup
    {
        private List<NetworkCredential> _users = new List<NetworkCredential>();
        private List<IPermission> _allowedActions = new List<IPermission>();
        private List<IPermission> _deniedActions = new List<IPermission>();
        private PermissionModel _mode = PermissionModel.DenyAllow;
        private bool _guest = false;

        /// <summary>
        /// Creates a new User Group
        /// </summary>
        public UserGroup()
        {

        }

        /// <summary>
        /// Creates a new User Group which may allow guests
        /// </summary>
        /// <param name="allowGuest">Are guests allowed?</param>
        /// <remarks>
        /// If guests are allowed then this Groups permissions apply to unauthenticated users
        /// </remarks>
        public UserGroup(bool allowGuest)
        {
            this._guest = false;
        }

        /// <summary>
        /// Gets/Sets whether Guests are allowed
        /// </summary>
        public bool AllowGuests
        {
            get
            {
                return this._guest;
            }
            set
            {
                this._guest = value;
            }
        }

        /// <summary>
        /// Gets/Sets the in-use Permission Model
        /// </summary>
        public PermissionModel PermissionModel
        {
            get
            {
                return this._mode;
            }
            set
            {
                this._mode = value;
            }
        }

        /// <summary>
        /// Adds a User to the Group
        /// </summary>
        /// <param name="credentials">User Credentials</param>
        public void AddUser(NetworkCredential credentials)
        {
            this._users.Add(credentials);
        }

        /// <summary>
        /// Adds an allow action permission to the Group
        /// </summary>
        /// <param name="permission">Permission</param>
        public void AddAllowedAction(IPermission permission)
        {
            this._allowedActions.Add(permission);
        }

        /// <summary>
        /// Adds a deny action permission to the Group
        /// </summary>
        /// <param name="permission">Permission</param>
        public void AddDeniedAction(IPermission permission)
        {
            this._deniedActions.Add(permission);
        }

        /// <summary>
        /// Returns whether the Group has a member with the given username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns></returns>
        public bool HasMember(String username)
        {
            return this._users.Any(u => u.UserName.Equals(username));
        }

        /// <summary>
        /// Returns whether the Group has a member with the given credentials
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns></returns>
        public bool HasMember(String username, String password)
        {
            return this._users.Any(u => u.UserName.Equals(username) && u.Password.Equals(password));
        }

        /// <summary>
        /// Gets whether the Group permits the action
        /// </summary>
        /// <param name="action">Action</param>
        /// <returns></returns>
        public bool IsActionPermitted(String action)
        {
            if (this._mode == PermissionModel.AllowAny) return true;
            if (this._mode == PermissionModel.DenyAny) return false;

            bool ok;
            if (this._mode == PermissionModel.AllowDeny)
            {
                //Is is allowed?
                ok = this._allowedActions.Any(p => p.IsPermissionFor(action));
                if (ok)
                {
                    //If it is also denied then Deny takes precedence
                    ok = !this._deniedActions.Any(p => p.IsPermissionFor(action));
                    return ok;
                }
                return false;
            }
            else
            {
                //Is it denied?
                ok = !this._deniedActions.Any(p => p.IsPermissionFor(action));
                if (!ok)
                {
                    //If it is also allowed then Allow takes precedence
                    ok = this._allowedActions.Any(p => p.IsPermissionFor(action));
                    return ok;
                }
                return true;
            }
        }
    }
}
