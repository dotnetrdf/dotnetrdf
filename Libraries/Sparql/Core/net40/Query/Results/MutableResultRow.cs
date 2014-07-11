using System;
using System.Collections.Generic;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Results
{
    /// <summary>
    /// Basic implementation of a mutable result row
    /// </summary>
    public class MutableResultRow
        : ResultRow, IMutableResultRow
    {
        /// <summary>
        /// Creates a new result row which has no variables and no values
        /// </summary>
        public MutableResultRow()
            : this(new String[0], null) {}

        /// <summary>
        /// Creates a new result row that has the given variables but no values
        /// </summary>
        /// <param name="variables">Variables</param>
        public MutableResultRow(IEnumerable<String> variables)
            : this(variables, null) {}

        /// <summary>
        /// Creates a new result row that has the given variables and values
        /// </summary>
        /// <param name="variables">Variables</param>
        /// <param name="values">Values</param>
        public MutableResultRow(IEnumerable<String> variables, IDictionary<String, INode> values)
            : base(variables, values) {}

        public void Set(string var, INode value)
        {
            if (this._values.ContainsKey(var))
            {
                this._values[var] = value;
            }
            else
            {
                if (!this._variables.Contains(var)) this._variables.Add(var);
                this._values.Add(var, value);
            }
        }
    }
}