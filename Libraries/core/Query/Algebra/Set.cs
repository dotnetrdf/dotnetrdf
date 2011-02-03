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
    /// Represents one possible set of values which is a solution to the query
    /// </summary>
    public class Set : IEquatable<Set>
    {
        private Dictionary<String, INode> _values;
        private int _id = 0;

        /// <summary>
        /// Creates a new Set
        /// </summary>
        public Set()
        {
            this._values = new Dictionary<string, INode>();
        }

        /// <summary>
        /// Creates a new Set which is the Join of the two Sets
        /// </summary>
        /// <param name="x">A Set</param>
        /// <param name="y">A Set</param>
        public Set(Set x, Set y)
        {
            this._values = new Dictionary<string, INode>(x._values);
            foreach (KeyValuePair<string, INode> pair in y._values)
            {
                if (!this._values.ContainsKey(pair.Key)) this._values.Add(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Creates a new Set which is a copy of an existing Set
        /// </summary>
        /// <param name="x">Set to copy</param>
        public Set(Set x)
        {
            this._values = new Dictionary<string, INode>(x._values);
        }

        /// <summary>
        /// Creates a new Set from a SPARQL Result
        /// </summary>
        /// <param name="result">Result</param>
        internal Set(SparqlResult result)
        {
            this._values = new Dictionary<string, INode>();
            foreach (String var in result.Variables)
            {
                this.Add(var, result[var]);
            }
        }

        /// <summary>
        /// Creates a new Set from a Binding Tuple
        /// </summary>
        /// <param name="tuple">Tuple</param>
        internal Set(BindingTuple tuple)
        {
            this._values = new Dictionary<string, INode>();
            foreach (KeyValuePair<String, PatternItem> binding in tuple.Values)
            {
                this.Add(binding.Key, ((NodeMatchPattern)binding.Value).Node);
            }
        }

        /// <summary>
        /// Retrieves the Value in this set for the given Variable
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <returns>Either a Node or a null</returns>
        public INode this[String variable]
        {
            get
            {
                if (this._values.ContainsKey(variable))
                {
                    return this._values[variable];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Adds a Value for a Variable to the Set
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <param name="value">Value</param>
        public void Add(String variable, INode value)
        {
            if (!this._values.ContainsKey(variable))
            {
                this._values.Add(variable, value);
            }
            else
            {
                throw new RdfQueryException("The value of a variable in a Set cannot be changed");
            }
        }

        /// <summary>
        /// Removes a Value for a Variable from the Set
        /// </summary>
        /// <param name="variable">Variable</param>
        public void Remove(String variable)
        {
            if (this._values.ContainsKey(variable)) this._values.Remove(variable);
        }

        /// <summary>
        /// Checks whether the Set contains a given Variable
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <returns></returns>
        public bool ContainsVariable(String variable)
        {
            return this._values.ContainsKey(variable);
        }

        /// <summary>
        /// Gets the Variables in the Set
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return (from var in this._values.Keys
                        select var);
            }
        }

        /// <summary>
        /// Gets the Values in the Set
        /// </summary>
        public IEnumerable<INode> Values
        {
            get
            {
                return (from value in this._values.Values
                        select value);
            }
        }

        /// <summary>
        /// Gets/Sets the ID of the Set
        /// </summary>
        public int ID
        {
            get
            {
                return this._id;
            }
            set
            {
                this._id = value;
            }
        }

        /// <summary>
        /// Gets the String representation of the Set
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            foreach (KeyValuePair<String, INode> pair in this._values)
            {
                output.Append("?" + pair.Key + " = " + pair.Value);
                output.Append(" , ");
            }
            if (this._values.Count > 0) output.Remove(output.Length - 3, 3);
            return output.ToString();
        }

        /// <summary>
        /// Gets whether a Set is equal to another Set
        /// </summary>
        /// <param name="obj">Object to compare against</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is Set)
            {
                return this.Equals((Set)obj);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets whether the Set is equal to another set
        /// </summary>
        /// <param name="other">Set to compare with</param>
        /// <returns></returns>
        public bool Equals(Set other)
        {
            if (other == null) return false;
            return this._values.All(pair => other.ContainsVariable(pair.Key) && ((pair.Value == null && other[pair.Key] == null) || pair.Value.Equals(other[pair.Key])));
            //return this._values.All(pair => (other.ContainsVariable(pair.Key) && ((pair.Value == null && other[pair.Key] == null) || pair.Value.Equals(other[pair.Key]))) || (pair.Value == null && other[pair.Key] == null));
        }

        /// <summary>
        /// Gets the Hash Code of the Set
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }
}
