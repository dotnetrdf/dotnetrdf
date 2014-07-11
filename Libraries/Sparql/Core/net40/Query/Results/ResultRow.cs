using System;
using System.Collections.Generic;
using System.Linq;
using VDS.Common.Collections;
using VDS.RDF.Nodes;

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
                INode value;
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

        public bool IsGroundResult
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
    }
}
