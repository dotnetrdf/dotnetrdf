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
            _action = action;
        }

        /// <summary>
        /// Gets whether the Permission is for the given action
        /// </summary>
        /// <param name="action">Action</param>
        /// <returns></returns>
        public bool IsPermissionFor(String action)
        {
            return _action.Equals(action);
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
            _actions.Add(action);
        }

        /// <summary>
        /// Creates a new Permissions Set
        /// </summary>
        /// <param name="actions">Actions</param>
        public PermissionSet(IEnumerable<String> actions)
        {
            _actions.AddRange(actions);
        }

        /// <summary>
        /// Gets whether the Permission is for the given action
        /// </summary>
        /// <param name="action">Action</param>
        /// <returns></returns>
        public bool IsPermissionFor(String action)
        {
            return _actions.Any(a => a.Equals(action));
        }
    }
}
