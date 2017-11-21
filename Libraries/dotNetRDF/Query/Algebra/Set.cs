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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra
{
    /// <summary>
    /// Represents one possible set of values which is a solution to the query
    /// </summary>
    public sealed class Set 
        : BaseSet, IEquatable<Set>
    {
        private Dictionary<String, INode> _values;

        /// <summary>
        /// Creates a new Set
        /// </summary>
        public Set()
        {
            _values = new Dictionary<string, INode>();
        }

        /// <summary>
        /// Creates a new Set which is the Join of the two Sets
        /// </summary>
        /// <param name="x">A Set</param>
        /// <param name="y">A Set</param>
        internal Set(ISet x, ISet y)
        {
            _values = new Dictionary<string, INode>();
            foreach (String var in x.Variables)
            {
                _values.Add(var, x[var]);
            }
            foreach (String var in y.Variables)
            {
                if (!_values.ContainsKey(var))
                {
                    _values.Add(var, y[var]);
                }
                else if (_values[var] == null)
                {
                    _values[var] = y[var];
                }
            }
        }

        /// <summary>
        /// Creates a new Set which is a copy of an existing Set
        /// </summary>
        /// <param name="x">Set to copy</param>
        internal Set(ISet x)
        {
            _values = new Dictionary<string, INode>();
            foreach (String var in x.Variables)
            {
                _values.Add(var, x[var]);
            }
        }

        /// <summary>
        /// Creates a new Set from a SPARQL Result
        /// </summary>
        /// <param name="result">Result</param>
        internal Set(SparqlResult result)
        {
            _values = new Dictionary<string, INode>();
            foreach (String var in result.Variables)
            {
                Add(var, result[var]);
            }
        }

        /// <summary>
        /// Creates a new Set from a Binding Tuple
        /// </summary>
        /// <param name="tuple">Tuple</param>
        internal Set(BindingTuple tuple)
        {
            _values = new Dictionary<string, INode>();
            foreach (KeyValuePair<String, PatternItem> binding in tuple.Values)
            {
                Add(binding.Key, tuple[binding.Key]);
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
                if (_values.ContainsKey(variable))
                {
                    return _values[variable];
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
            if (!_values.ContainsKey(variable))
            {
                _values.Add(variable, value);
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
            if (_values.ContainsKey(variable)) _values.Remove(variable);
        }

        /// <summary>
        /// Checks whether the Set contains a given Variable
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <returns></returns>
        public override bool ContainsVariable(String variable)
        {
            return _values.ContainsKey(variable);
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
                return (from var in _values.Keys
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
                return (from value in _values.Values
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
            // return new JoinedSet(other, this);
        }

        /// <summary>
        /// Copies the Set
        /// </summary>
        /// <returns></returns>
        public override ISet Copy()
        {
            return new Set(this);
            // return new JoinedSet(this);
        }

        /// <summary>
        /// Gets whether the Set is equal to another set
        /// </summary>
        /// <param name="other">Set to compare with</param>
        /// <returns></returns>
        public bool Equals(Set other)
        {
            if (other == null) return false;
            return _values.All(pair => other.ContainsVariable(pair.Key) && ((pair.Value == null && other[pair.Key] == null) || pair.Value.Equals(other[pair.Key])));
        }

    }
}
