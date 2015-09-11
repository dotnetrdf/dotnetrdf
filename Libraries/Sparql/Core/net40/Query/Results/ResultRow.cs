/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

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
using System.Text;
using VDS.Common.Collections;
using VDS.RDF.Nodes;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Results
{
    /// <summary>
    /// Basic implementation of a result row
    /// </summary>
    public class ResultRow 
        : IResultRow
    {
        protected readonly IList<String> _variables;
        protected readonly IDictionary<String, INode> _values;

        /// <summary>
        /// Creates a new result row which has no variables and no values
        /// </summary>
        public ResultRow()
            : this(new String[0], null) { }

        /// <summary>
        /// Creates a new result row that has the given variables but no values
        /// </summary>
        /// <param name="variables">Variables</param>
        public ResultRow(IEnumerable<String> variables)
            : this(variables, null) { }

        /// <summary>
        /// Creates a new result row that has the given variables and values
        /// </summary>
        /// <param name="variables">Variables</param>
        /// <param name="values">Values</param>
        public ResultRow(IEnumerable<String> variables, IDictionary<String, INode> values)
        {
            if (variables == null) throw new ArgumentNullException("variables");
            this._variables = new List<string>(variables);
            this._values = values ?? new Dictionary<string, INode>();

            // TODO Should we validate that values does not have any variables that aren't also in variables?
        }

        /// <summary>
        /// Creates a new result row as a copy of another result row
        /// </summary>
        /// <param name="row">Row</param>
        protected ResultRow(IResultRow row)
            : this(row != null ? row.Variables : null)
        {
            if (row == null) throw new ArgumentNullException("row");
            foreach (String var in this._variables)
            {
                this._values.Add(var, row[var]);
            }
        }

        public bool IsEmpty { get { return this._variables.Count == 0; } }

        public INode this[string var]
        {
            get
            {
                INode value;
                if (this._values.TryGetValue(var, out value))
                {
                    return value;
                }
                throw new RdfException(String.Format("Variable {0} is not present in this row", var));
            }
        }

        public INode this[int index]
        {
            get
            {
                if (index < 0 || index >= this._variables.Count) throw new IndexOutOfRangeException(String.Format("Column Index {0} is not within the valid range of 0 to {1}", index, this._variables.Count - 1));
                String var = this._variables[index];
                return this[var];
            }
        }

        public bool TryGetValue(string var, out INode value)
        {
            return this._values.TryGetValue(var, out value);
        }

        public bool TryGetBoundValue(string var, out INode value)
        {
            if (this._values.TryGetValue(var, out value))
            {
                return value != null;
            }
            return false;
        }

        public bool HasValue(string var)
        {
            return this._values.ContainsKey(var);
        }

        public bool HasBoundValue(string var)
        {
            INode value;
            return this.TryGetBoundValue(var, out value);
        }

        public bool IsGround
        {
            get
            {
                return this._variables.All(v =>
                {
                    INode value;
                    return this.TryGetBoundValue(v, out value) && value.NodeType != NodeType.Blank;
                });
            }
        }

        public IEnumerable<string> Variables
        {
            get { return new ImmutableView<String>(this._variables); }
        }

        public bool Equals(IResultRow other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (this._variables.Count != other.Variables.Count()) return false;

            foreach (String var in this._variables)
            {
                INode n;
                if (this.TryGetValue(var, out n))
                {
                    // There is value for the variable in this row
                    INode m;
                    // If there is not a value for it in the other row they are not equal
                    if (!other.TryGetValue(var, out m)) return false;
                    // If the values are not equal then the rows are not equal
                    if (!EqualityHelper.AreNodesEqual(n, m)) return false;
                }
                else
                {
                    // No value for the variable in this row
                    // If there is a value for it in the other row then these rows are not equal
                    if (other.TryGetValue(var, out n)) return false;
                }
            }
            // All values match
            return true;
        }

        public override string ToString()
        {
            return ToString(new AlgebraFormatter());
        }

        public String ToString(INodeFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            if (this._variables.Count == 0) return "<empty row>";

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < this._variables.Count; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.Append("?");
                builder.Append(this._variables[i]);
                builder.Append(" = ");
                INode n = this[this._variables[i]];
                if (n == null) continue;
                builder.Append(n.ToString(formatter));
            }
            return builder.ToString();
        }
    }
}
