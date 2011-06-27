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
    public sealed class Set : BaseSet, IEquatable<Set>
    {
        private Dictionary<String, INode> _values;

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
        private Set(ISet x, ISet y)
        {
            this._values = new Dictionary<string, INode>();
            foreach (String var in x.Variables)
            {
                this._values.Add(var, x[var]);
            }
            foreach (String var in y.Variables)
            {
                if (!this._values.ContainsKey(var))
                {
                    this._values.Add(var, y[var]);
                }
                else if (this._values[var] == null)
                {
                    this._values[var] = y[var];
                }
            }
        }

        /// <summary>
        /// Creates a new Set which is a copy of an existing Set
        /// </summary>
        /// <param name="x">Set to copy</param>
        internal Set(ISet x)
        {
            this._values = new Dictionary<string, INode>();
            foreach (String var in x.Variables)
            {
                this._values.Add(var, x[var]);
            }
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
                this.Add(binding.Key, tuple[binding.Key]);
            }
        }

        /// <summary>
        /// Retrieves the Value in this set for the given Variable
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <returns>Either a Node or a null</returns>
        public override INode this[String variable]
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
        public override void Add(String variable, INode value)
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
        public override void Remove(String variable)
        {
            if (this._values.ContainsKey(variable)) this._values.Remove(variable);
        }

        /// <summary>
        /// Checks whether the Set contains a given Variable
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <returns></returns>
        public override bool ContainsVariable(String variable)
        {
            return this._values.ContainsKey(variable);
        }

        /// <summary>
        /// Gets the Variables in the Set
        /// </summary>
        public override IEnumerable<String> Variables
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
        public override IEnumerable<INode> Values
        {
            get
            {
                return (from value in this._values.Values
                        select value);
            }
        }

        public override ISet Join(ISet other)
        {
            //return new Set(this, other);
            return new JoinedSet(other, this);
        }

        public override ISet Copy()
        {
            return new Set(this);
            //return new CopiedSet(this);
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
                output.Append("?" + pair.Key + " = " + pair.Value.ToSafeString());
                output.Append(" , ");
            }
            if (this._values.Count > 0) output.Remove(output.Length - 3, 3);
            return output.ToString();
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

    /// <summary>
    /// Represents one possible set of values which is a solution to the query where those values are the result of joining two possible sets
    /// </summary>
    public sealed class JoinedSet : BaseSet, IEquatable<JoinedSet>
    {
        private ISet _lhs, _rhs;

        public JoinedSet(ISet x, ISet y)
        {
            this._lhs = new Set(x);
            this._rhs = y;
        }

        public override void Add(string variable, INode value)
        {
            //Joined Sets are left associative so always add to the LHS set
            this._lhs.Add(variable, value);
        }

        public override bool ContainsVariable(string variable)
        {
            return this._lhs.ContainsVariable(variable) || this._rhs.ContainsVariable(variable);
        }

        public override void Remove(string variable)
        {
            this._lhs.Remove(variable);
            this._rhs.Remove(variable);
        }

        public override INode this[string variable]
        {
            get 
            {
                INode temp = this._lhs[variable];
                return (temp != null ? temp : this._rhs[variable]);
            }
        }

        public override IEnumerable<INode> Values
        {
            get 
            {
                return (from v in this.Variables
                        select this[v]);
            }
        }

        public override IEnumerable<string> Variables
        {
            get 
            {
                return this._lhs.Variables.Concat(this._rhs.Variables).Distinct();
            }
        }

        public override ISet Join(ISet other)
        {
            return new JoinedSet(other, this);
        }

        public override ISet Copy()
        {
            return new Set(this);
            //return new CopiedSet(this);
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            foreach (String v in this.Variables)
            {
                if (output.Length > 0) output.Append(" , ");
                output.Append("?" + v + " = " + this[v].ToSafeString());
            }
            return output.ToString();
        }

        public bool Equals(JoinedSet other)
        {
            return this.Equals((ISet)other);
        }
    }

    /// <summary>
    /// Represents one possible set of values which is a solution to the query where those values are the copy of another possible set
    /// </summary>
    public sealed class CopiedSet : BaseSet, IEquatable<CopiedSet>
    {
        private ISet _s;

        public CopiedSet(ISet s)
        {
            this._s = s;
        }

        public override void Add(string variable, INode value)
        {
            this._s.Add(variable, value);
        }

        public override bool ContainsVariable(string variable)
        {
            return this._s.ContainsVariable(variable);
        }

        public override void Remove(string variable)
        {
            this._s.Remove(variable);
        }

        public override INode this[string variable]
        {
            get 
            {
                return this._s[variable];
            }
        }

        public override IEnumerable<INode> Values
        {
            get 
            {
                return this._s.Values; 
            }
        }

        public override IEnumerable<string> Variables
        {
            get 
            {
                return this._s.Variables; 
            }
        }

        public override ISet Join(ISet other)
        {
            return new JoinedSet(other, this);
        }

        public override ISet Copy()
        {
            return new Set(this);
            //return new CopiedSet(this);
        }

        public override string ToString()
        {
            return this._s.ToString();
        }

        public override int GetHashCode()
        {
            return this._s.GetHashCode();
        }

        public bool Equals(CopiedSet other)
        {
            return this.Equals((ISet)other);
        }
    }
}
