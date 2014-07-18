/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Engine
{
    /// <summary>
    /// Represents one possible set of values which is a solution to the query
    /// </summary>
    public sealed class Set 
        : BaseSet, IEquatable<Set>
#if PORTABLE
        , IComparable<Set>,
        IComparable
#endif
    {
        private readonly Dictionary<String, INode> _values;

        /// <summary>
        /// Creates a new empty Set
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
        public Set(ISet x, ISet y)
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
        public Set(ISet x)
        {
            this._values = new Dictionary<string, INode>();
            foreach (String var in x.Variables)
            {
                this._values.Add(var, x[var]);
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
                INode value = null;
                this._values.TryGetValue(variable, out value);
                return value;
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
        /// Gets whether the Set is compatible with a given set based on the given variables
        /// </summary>
        /// <param name="s">Set</param>
        /// <param name="vars">Variables</param>
        /// <returns></returns>
        public override bool IsCompatibleWith(ISet s, IEnumerable<string> vars)
        {
            return vars.All(v => this[v] == null || s[v] == null || this[v].Equals(s[v]));
        }

        /// <summary>
        /// Gets whether the Set is minus compatible with a given set based on the given variables
        /// </summary>
        /// <param name="s">Set</param>
        /// <param name="vars">Variables</param>
        /// <returns></returns>
        public override bool IsMinusCompatibleWith(ISet s, IEnumerable<string> vars)
        {
            return vars.Any(v => this[v] != null && this[v].Equals(s[v]));
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
        /// Gets whether the set is empty
        /// </summary>
        public override bool IsEmpty
        {
            get { return this._values.Count == 0; }
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

        /// <summary>
        /// Joins the set to another set
        /// </summary>
        /// <param name="other">Other Set</param>
        /// <returns></returns>
        public override ISet Join(ISet other)
        {
            return new Set(this, other);
        }

        /// <summary>
        /// Copies the Set
        /// </summary>
        /// <returns></returns>
        public override ISet Copy()
        {
            return new Set(this);
        }

        /// <summary>
        /// Gets whether the Set is equal to another set
        /// </summary>
        /// <param name="other">Set to compare with</param>
        /// <returns></returns>
        public bool Equals(Set other)
        {
            return other != null && this._values.All(pair => other.ContainsVariable(pair.Key) && ((pair.Value == null && other[pair.Key] == null) || EqualityHelper.AreNodesEqual(pair.Value, other[pair.Key])));
        }

#if PORTABLE
        //TODO: CORE-303 clean up - why is this necessary?

        public int CompareTo(Set other)
        {
            foreach (var pair in _values.OrderBy(v => v.Key))
            {
                var otherValue = other[pair.Key];
                if (otherValue == null) return 1;
                var cmp = pair.Value.CompareTo(otherValue);
                if (cmp != 0) return cmp;
            }
            return 0;
        }

        public int CompareTo(object other)
        {
            if (other is Set)
            {
                return CompareTo(other as Set);
            }
            return -1;
        }
#endif
    }
}
