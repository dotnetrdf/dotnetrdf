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
    /// Represents one possible solution to the query
    /// </summary>
    public sealed class Solution 
        : BaseSolution, IEquatable<Solution>
#if PORTABLE
        , IComparable<Solution>,
        IComparable
#endif
    {
        private readonly Dictionary<String, INode> _values;

        /// <summary>
        /// Creates a new empty solution
        /// </summary>
        public Solution()
        {
            this._values = new Dictionary<string, INode>();
        }

        /// <summary>
        /// Creates a new solution which is the Join of the two solutions
        /// </summary>
        /// <param name="x">A solution</param>
        /// <param name="y">A solution</param>
        public Solution(ISolution x, ISolution y)
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
        /// Creates a new solution which is a copy of an existing solution
        /// </summary>
        /// <param name="x">solution to copy</param>
        public Solution(ISolution x)
        {
            this._values = new Dictionary<string, INode>();
            foreach (String var in x.Variables)
            {
                this._values.Add(var, x[var]);
            }
        }

        /// <summary>
        /// Retrieves the Value in this solution for the given Variable
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <returns>Either a Node or a null</returns>
        public override INode this[String variable]
        {
            get
            {
                INode value;
                this._values.TryGetValue(variable, out value);
                return value;
            }
        }

        /// <summary>
        /// Adds a Value for a Variable to the solution
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
                throw new RdfQueryException("The value of a variable in a solution cannot be changed");
            }
        }

        /// <summary>
        /// Removes a Value for a Variable from the solution
        /// </summary>
        /// <param name="variable">Variable</param>
        public override void Remove(String variable)
        {
            if (this._values.ContainsKey(variable)) this._values.Remove(variable);
        }

        /// <summary>
        /// Checks whether the solution contains a given Variable
        /// </summary>
        /// <param name="variable">Variable</param>
        /// <returns></returns>
        public override bool ContainsVariable(String variable)
        {
            return this._values.ContainsKey(variable);
        }

        /// <summary>
        /// Gets whether the solution is compatible with a given solution based on the given variables
        /// </summary>
        /// <param name="s">Solution</param>
        /// <param name="vars">Variables</param>
        /// <returns></returns>
        public override bool IsCompatibleWith(ISolution s, IEnumerable<string> vars)
        {
            return vars.All(v => this[v] == null || s[v] == null || this[v].Equals(s[v]));
        }

        /// <summary>
        /// Gets whether the solution is minus compatible with a given solution based on the given variables
        /// </summary>
        /// <param name="s">Solution</param>
        /// <param name="vars">Variables</param>
        /// <returns></returns>
        public override bool IsMinusCompatibleWith(ISolution s, IEnumerable<string> vars)
        {
            return vars.Any(v => this[v] != null && this[v].Equals(s[v]));
        }

        /// <summary>
        /// Gets the Variables in the solution
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
        /// Gets whether the solution is empty
        /// </summary>
        public override bool IsEmpty
        {
            get { return this._values.Count == 0; }
        }

        /// <summary>
        /// Gets the Values in the solution
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
        /// Joins the solution to another solution
        /// </summary>
        /// <param name="other">Other solution</param>
        /// <returns></returns>
        public override ISolution Join(ISolution other)
        {
            return new Solution(this, other);
        }

        /// <summary>
        /// Copies the solution
        /// </summary>
        /// <returns></returns>
        public override ISolution Copy()
        {
            return new Solution(this);
        }

        /// <summary>
        /// Copies the solution only including the specified variables
        /// </summary>
        /// <returns></returns>
        public override ISolution Project(IEnumerable<string> vars)
        {
            Solution s = new Solution();
            foreach (String var in vars)
            {
                INode n;
                s.Add(var, this._values.TryGetValue(var, out n) ? n : null);
            }
            return s;
        }

        /// <summary>
        /// Gets whether the solution is equal to another solution
        /// </summary>
        /// <param name="other">solution to compare with</param>
        /// <returns></returns>
        public bool Equals(Solution other)
        {
            return other != null && this._values.All(pair => other.ContainsVariable(pair.Key) && ((pair.Value == null && other[pair.Key] == null) || EqualityHelper.AreNodesEqual(pair.Value, other[pair.Key])));
        }

#if PORTABLE
        //TODO: CORE-303 clean up - why is this necessary?

        public int CompareTo(Solution other)
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
            if (other is Solution)
            {
                return CompareTo(other as Solution);
            }
            return -1;
        }
#endif
    }
}
