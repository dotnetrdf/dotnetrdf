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
using System.Text;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Multiset which represents a Grouping of Sets from another Multiset
    /// </summary>
    public class GroupMultiset : Multiset
    {
        private BaseMultiset _contents;
        private List<BindingGroup> _groups;

        /// <summary>
        /// Creates a new Group Multiset
        /// </summary>
        /// <param name="contents">Multiset which contains the Member Sets of the Groups</param>
        /// <param name="groups">Groups</param>
        public GroupMultiset(BaseMultiset contents, List<BindingGroup> groups)
        {
            this._contents = contents;
            this._groups = groups;

            bool first = true;
            foreach (BindingGroup group in groups)
            {
                Set s = new Set();
                foreach (KeyValuePair<String, INode> assignment in group.Assignments)
                {
                    if (first) this.AddVariable(assignment.Key);
                    s.Add(assignment.Key, assignment.Value);
                }
                first = false;
                base.Add(s);
            }
        }

        /// <summary>
        /// Gets the enumeration of the Groups in the Multiset
        /// </summary>
        public IEnumerable<BindingGroup> Groups
        {
            get
            {
                return this._groups;
            }
        }

        /// <summary>
        /// Gets the enumeration of the IDs of Sets in the group with the given ID
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <returns></returns>
        public IEnumerable<int> GroupSetIDs(int id)
        {
            return this._groups[id-1].BindingIDs;
        }

        /// <summary>
        /// Gets the Group with the given ID
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <returns></returns>
        public BindingGroup Group(int id)
        {
            return this._groups[id - 1];
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
                return this._contents;
            }
        }
    }
}
