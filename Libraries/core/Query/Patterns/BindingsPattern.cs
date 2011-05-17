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
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Patterns
{
    /// <summary>
    /// Represents a set of Bindings for a SPARQL Query
    /// </summary>
    public class BindingsPattern
    {
        private List<String> _vars = new List<string>();
        private List<BindingTuple> _tuples = new List<BindingTuple>();

        /// <summary>
        /// Creates a new Empty Bindings Pattern
        /// </summary>
        public BindingsPattern()
        { }

        /// <summary>
        /// Creates a new Bindings Pattern
        /// </summary>
        /// <param name="vars">Variables</param>
        public BindingsPattern(IEnumerable<String> vars)
        {
            this._vars.AddRange(vars);
        }

        /// <summary>
        /// Gets the enumeration of Variables
        /// </summary>
        public IEnumerable<String> Variables
        {
            get
            {
                return this._vars;
            }
        }

        /// <summary>
        /// Gets the enumeration of Tuples
        /// </summary>
        public IEnumerable<BindingTuple> Tuples
        {
            get
            {
                return this._tuples;
            }
        }

        /// <summary>
        /// Adds a Tuple to the Bindings pattern
        /// </summary>
        /// <param name="t"></param>
        internal void AddTuple(BindingTuple t)
        {
            this._tuples.Add(t);
        }

        /// <summary>
        /// Converts a Bindings Clause to a Multiset
        /// </summary>
        /// <returns></returns>
        public BaseMultiset ToMultiset()
        {
            if (this._vars.Any())
            {
                Multiset m = new Multiset();
                foreach (String var in this._vars)
                {
                    m.AddVariable(var);
                }
                foreach (BindingTuple tuple in this._tuples)
                {
                    m.Add(new Set(tuple));
                }
                return m;
            }
            else
            {
                return new IdentityMultiset();
            }
        }

        /// <summary>
        /// Gets the String representation of the Pattern
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("BINDINGS ");
            foreach (String var in this._vars)
            {
                output.Append("?" + var + " ");
            }
            output.AppendLine();
            output.AppendLine("{");
            foreach (BindingTuple t in this._tuples)
            {
                output.AppendLine("  " + t.ToString());
            }
            output.AppendLine("}");

            return output.ToString();
        }
    }

    /// <summary>
    /// Represents a Tuple in a BINDINGS clause
    /// </summary>
    public class BindingTuple
    {
        private Dictionary<String, PatternItem> _values = new Dictionary<String, PatternItem>();

        /// <summary>
        /// Creates a new Binding Tuple
        /// </summary>
        /// <param name="variables">Variables</param>
        /// <param name="values">Values</param>
        public BindingTuple(List<String> variables, List<PatternItem> values)
        {
            for (int i = 0; i < variables.Count; i++)
            {
                this._values.Add(variables[i], values[i]);
            }
        }

        /// <summary>
        /// Gets the enumeration of Variable-Value pairs
        /// </summary>
        public IEnumerable<KeyValuePair<String, PatternItem>> Values
        {
            get
            {
                return this._values;
            }
        }

        /// <summary>
        /// Gets the Value for a Variable
        /// </summary>
        /// <param name="var">Variable</param>
        /// <returns></returns>
        public INode this[String var]
        {
            get
            {
                if (!this._values.ContainsKey(var)) throw new IndexOutOfRangeException();
                PatternItem temp = this._values[var];
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
        /// Gets whether this is an empty tuple
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return this._values.Count == 0;
            }
        }

        /// <summary>
        /// Gets whether the Tuple is complete i.e. has no undefined entries
        /// </summary>
        public bool IsComplete
        {
            get
            {
                return this._values.Values.All(v => v != null);
            }
        }

        /// <summary>
        /// Gets the String representation of the Tuple
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
 	        StringBuilder output = new StringBuilder();
            output.Append("( ");
            foreach (PatternItem p in this._values.Values)
            {
                if (p != null) 
                {
                    output.Append(p.ToString() + " ");
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
