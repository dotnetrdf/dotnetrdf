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

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Multiset which represents a Grouping of Sets from another Multiset
    /// </summary>
    public class GroupMultiset 
        : Multiset
    {
        private BaseMultiset _contents;
        private Dictionary<int, BindingGroup> _groups = new Dictionary<int, BindingGroup>();

        /// <summary>
        /// Creates a new Group Multiset
        /// </summary>
        /// <param name="contents">Multiset which contains the sets that are being grouped</param>
        public GroupMultiset(BaseMultiset contents)
        {
            _contents = contents;
        }

        /// <summary>
        /// Gets the enumeration of the Groups in the Multiset
        /// </summary>
        public IEnumerable<BindingGroup> Groups
        {
            get
            {
                return _groups.Values;
            }
        }

        /// <summary>
        /// Gets the enumeration of the IDs of Sets in the group with the given ID
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <returns></returns>
        public IEnumerable<int> GroupSetIDs(int id)
        {
            return _groups[id].BindingIDs;
        }

        /// <summary>
        /// Gets the Group with the given ID
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <returns></returns>
        public BindingGroup Group(int id)
        {
            return _groups[id];
        }

        /// <summary>
        /// Adds a Group to the Multiset
        /// </summary>
        /// <param name="group"></param>
        public void AddGroup(BindingGroup group)
        {
            Set s = new Set();
            foreach (KeyValuePair<String, INode> assignment in group.Assignments)
            {
                s.Add(assignment.Key, assignment.Value);
            }
            base.Add(s);
            _groups.Add(s.ID, group);
        }

        /// <summary>
        /// Adds a Set to the Group Multiset
        /// </summary>
        /// <param name="s">Set</param>
        /// <exception cref="RdfQueryException">Thrown since this action is invalid on a Group Multiset</exception>
        public override void Add(ISet s)
        {
            throw new RdfQueryException("Cannot add a Set to a Group Multiset");
        }

        /// <summary>
        /// Gets the Multiset which contains the Sets who are the members of the Groups this Multiset represents
        /// </summary>
        public BaseMultiset Contents
        {
            get
            {
                return _contents;
            }
        }
    }
}
