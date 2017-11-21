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
using System.Text;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Represents an Group of Bindings which is used when executing Queries with GROUP BY clauses
    /// </summary>
    public class BindingGroup 
        : IEnumerable<int>
    {
        private List<int> _bindingIDs = new List<int>();
        private Dictionary<String, INode> _assignments = new Dictionary<string, INode>();

        /// <summary>
        /// Creates a new Binding Group
        /// </summary>
        public BindingGroup()
        {

        }

        /// <summary>
        /// Creates a new Binding Group which is a sub-group of the given Parent Group
        /// </summary>
        /// <param name="parent">Parent Group</param>
        public BindingGroup(BindingGroup parent)
        {
            foreach (KeyValuePair<String, INode> assignment in parent.Assignments)
            {
                _assignments.Add(assignment.Key, assignment.Value);
            }
        }

        /// <summary>
        /// Creates a new Binding Group from the specified IDs
        /// </summary>
        /// <param name="ids">IDs</param>
        public BindingGroup(IEnumerable<int> ids)
        {
            _bindingIDs.AddRange(ids);
        }

        /// <summary>
        /// Adds a Binding ID to the Group
        /// </summary>
        /// <param name="id">ID</param>
        public void Add(int id)
        {
            _bindingIDs.Add(id);
        }

        /// <summary>
        /// Gets the Enumerator for the Binding IDs in the Group
        /// </summary>
        /// <returns></returns>
        public IEnumerator<int> GetEnumerator()
        {
            return _bindingIDs.GetEnumerator();
        }

        /// <summary>
        /// Gets the Enumerator for the Binding IDs in the Group
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _bindingIDs.GetEnumerator();
        }

        /// <summary>
        /// Gets the Binding IDs in the Group
        /// </summary>
        public IEnumerable<int> BindingIDs
        {
            get
            {
                return (from id in _bindingIDs
                        select id);
            }
        }

        /// <summary>
        /// Adds a Variable Assignment to the Group
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <param name="value">Value</param>
        public void AddAssignment(String variable, INode value)
        {
            if (_assignments.ContainsKey(variable))
            {
                throw new RdfQueryException("Cannot assign the value of a GROUP BY expression to a Variable assigned to by an earlier GROUP BY");
            }
            else
            {
                _assignments.Add(variable, value);
            }
        }

        /// <summary>
        /// Gets the Variable Assignments for the Group
        /// </summary>
        public IEnumerable<KeyValuePair<String, INode>> Assignments
        {
            get
            {
                return (from kvp in _assignments
                        select kvp);
            }
        }

        /// <summary>
        /// Gets a String summarising the group
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(_bindingIDs.Count);
            builder.Append(" Member(s)");
            if (_assignments.Count > 0)
            {
                builder.Append(" {");
                foreach (String var in _assignments.Keys)
                {
                    builder.Append("?" + var + " = " + _assignments[var].ToSafeString());
                }
                builder.Append('}');
            }
            return builder.ToString();
        }
    }
}