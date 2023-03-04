/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2023 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Represents a set of Bindings for a SPARQL Query or part thereof i.e. represents the VALUES clause.
    /// </summary>
    public class BindingsPattern
    {
        private readonly List<string> _vars = new List<string>();
        private readonly List<BindingTuple> _tuples = new List<BindingTuple>();

        /// <summary>
        /// Creates a new Empty Bindings Pattern.
        /// </summary>
        public BindingsPattern()
        { }

        /// <summary>
        /// Creates a new Bindings Pattern.
        /// </summary>
        /// <param name="vars">Variables.</param>
        public BindingsPattern(IEnumerable<string> vars)
        {
            _vars.AddRange(vars);
        }

        /// <summary>
        /// Gets the enumeration of Variables.
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                return _vars;
            }
        }

        /// <summary>
        /// Get the enumeration of fixed variables i.e. those guaranteed to be bound.
        /// </summary>
        public IEnumerable<string> FixedVariables
        {
            get { return _vars.Where(v => _tuples.All(t => t.IsBound(v))); }
        }

        /// <summary>
        /// Gets the enumeration of floating variables i.e. those not guaranteed to be bound.
        /// </summary>
        public IEnumerable<string> FloatingVariables
        {
            get { return _vars.Where(v => _tuples.Any(t => !t.IsBound(v))); }
        }

        /// <summary>
        /// Gets the enumeration of Tuples.
        /// </summary>
        public IEnumerable<BindingTuple> Tuples => _tuples;

        /// <summary>
        /// Adds a Tuple to the Bindings pattern.
        /// </summary>
        /// <param name="t"></param>
        public void AddTuple(BindingTuple t)
        {
            _tuples.Add(t);
        }


        /// <summary>
        /// Gets the String representation of the Pattern.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var output = new StringBuilder();
            output.Append("VALUES ( ");
            foreach (string var in _vars)
            {
                output.Append("?" + var + " ");
            }
            output.AppendLine(")");
            output.AppendLine("{");
            foreach (BindingTuple t in _tuples)
            {
                output.AppendLine("  " + t.ToString());
            }
            output.AppendLine("}");

            return output.ToString();
        }
    }

    /// <summary>
    /// Represents a Tuple in a BINDINGS clause.
    /// </summary>
    public class BindingTuple
    {
        private readonly Dictionary<string, PatternItem> _values = new Dictionary<string, PatternItem>();

        /// <summary>
        /// Creates a new Binding Tuple.
        /// </summary>
        /// <param name="variables">Variables.</param>
        /// <param name="values">Values.</param>
        public BindingTuple(List<string> variables, List<PatternItem> values)
        {
            for (var i = 0; i < variables.Count; i++)
            {
                _values.Add(variables[i], values[i]);
            }
        }

        /// <summary>
        /// Gets the enumeration of Variable-Value pairs.
        /// </summary>
        public IEnumerable<KeyValuePair<string, PatternItem>> Values
        {
            get
            {
                return _values;
            }
        }

        /// <summary>
        /// Gets the Value for a Variable.
        /// </summary>
        /// <param name="var">Variable.</param>
        /// <returns></returns>
        public INode this[string var]
        {
            get
            {
                if (!_values.ContainsKey(var)) throw new IndexOutOfRangeException();
                PatternItem temp = _values[var];
                if (temp is NodeMatchPattern)
                {
                    return ((NodeMatchPattern)temp).Node;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets whether this is an empty tuple.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return _values.Count == 0;
            }
        }

        /// <summary>
        /// Gets whether the Tuple is complete i.e. has no undefined entries.
        /// </summary>
        public bool IsComplete
        {
            get
            {
                return _values.Values.All(v => v != null);
            }
        }

        /// <summary>
        /// Gets whether the given variable is bound for this tuple i.e. is not UNDEF.
        /// </summary>
        /// <param name="var">Variable.</param>
        /// <returns>True if the variable exists in the tuple and is bound, false otherwise.</returns>
        public bool IsBound(string var)
        {
            return _values.TryGetValue(var, out PatternItem value) && value != null;
        }

        /// <summary>
        /// Gets the String representation of the Tuple.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var output = new StringBuilder();
            output.Append("( ");
            foreach (PatternItem p in _values.Values)
            {
                if (p != null) 
                {
                    output.Append(p + " ");
                } 
                else 
                {
                    output.Append("UNDEF ");
                }
            }
            output.Append(")");
            return output.ToString();
        }
    }
}
