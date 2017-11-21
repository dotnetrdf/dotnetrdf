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
            _guest = false;
        }

        /// <summary>
        /// Gets/Sets whether Guests are allowed
        /// </summary>
        public bool AllowGuests
        {
            get
            {
                return _guest;
            }
            set
            {
                _guest = value;
            }
        }

        /// <summary>
        /// Gets/Sets the in-use Permission Model
        /// </summary>
        public PermissionModel PermissionModel
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
            }
        }

        /// <summary>
        /// Adds a User to the Group
        /// </summary>
        /// <param name="credentials">User Credentials</param>
        public void AddUser(NetworkCredential credentials)
        {
            _users.Add(credentials);
        }

        /// <summary>
        /// Adds an allow action permission to the Group
        /// </summary>
        /// <param name="permission">Permission</param>
        public void AddAllowedAction(IPermission permission)
        {
            _allowedActions.Add(permission);
        }

        /// <summary>
        /// Adds a deny action permission to the Group
        /// </summary>
        /// <param name="permission">Permission</param>
        public void AddDeniedAction(IPermission permission)
        {
            _deniedActions.Add(permission);
        }

        /// <summary>
        /// Returns whether the Group has a member with the given username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns></returns>
        public bool HasMember(String username)
        {
            return _users.Any(u => u.UserName.Equals(username));
        }

        /// <summary>
        /// Returns whether the Group has a member with the given credentials
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns></returns>
        public bool HasMember(String username, String password)
        {
            return _users.Any(u => u.UserName.Equals(username) && u.Password.Equals(password));
        }

        /// <summary>
        /// Gets whether the Group permits the action
        /// </summary>
        /// <param name="action">Action</param>
        /// <returns></returns>
        public bool IsActionPermitted(String action)
        {
            if (_mode == PermissionModel.AllowAny) return true;
            if (_mode == PermissionModel.DenyAny) return false;

            bool ok;
            if (_mode == PermissionModel.AllowDeny)
            {
                // Is is allowed?
                ok = _allowedActions.Any(p => p.IsPermissionFor(action));
                if (ok)
                {
                    // If it is also denied then Deny takes precedence
                    ok = !_deniedActions.Any(p => p.IsPermissionFor(action));
                    return ok;
                }
                return false;
            }
            else
            {
                // Is it denied?
                ok = !_deniedActions.Any(p => p.IsPermissionFor(action));
                if (!ok)
                {
                    // If it is also allowed then Allow takes precedence
                    ok = _allowedActions.Any(p => p.IsPermissionFor(action));
                    return ok;
                }
                return true;
            }
        }
    }
}
