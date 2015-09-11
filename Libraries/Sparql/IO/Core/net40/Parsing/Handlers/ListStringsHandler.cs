using System;
using System.Collections.Generic;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Results;

namespace VDS.RDF.Parsing.Handlers
{
    /// <summary>
    /// A Results Handler which extracts Literals from one/more variables in a Result Set
    /// </summary>
    public class ListStringsHandler
        : BaseResultsHandler
    {
        private List<string> _values;
        private readonly HashSet<string> _vars = new HashSet<string>();

        /// <summary>
        /// Creates a new List Strings handler
        /// </summary>
        /// <param name="var">Variable to build the list from</param>
        public ListStringsHandler(String var)
        {
            this._vars.Add(var);
        }

        /// <summary>
        /// Creates a new List Strings handler
        /// </summary>
        /// <param name="vars">Variables to build the list from</param>
        public ListStringsHandler(IEnumerable<String> vars)
        {
            foreach (String var in vars)
            {
                this._vars.Add(var);
            }
        }

        /// <summary>
        /// Gets the Strings
        /// </summary>
        public IEnumerable<String> Strings
        {
            get
            {
                return this._values;
            }
        }

        /// <summary>
        /// Starts handling results
        /// </summary>
        protected override void StartResultsInternal()
        {
            this._values = new List<string>();
        }

        /// <summary>
        /// Handles boolean results
        /// </summary>
        /// <param name="result">Result</param>
        protected override void HandleBooleanResultInternal(bool result)
        {
            //Nothing to do
        }

        /// <summary>
        /// Handles variable declarations
        /// </summary>
        /// <param name="var">Variable</param>
        /// <returns></returns>
        protected override bool HandleVariableInternal(string var)
        {
            //Nothing to do
            return true;
        }

        /// <summary>
        /// Handles results by extracting strings from relevant variables
        /// </summary>
        /// <param name="result">Result</param>
        /// <returns></returns>
        protected override bool HandleResultInternal(IResultRow result)
        {
            foreach (String var in result.Variables)
            {
                if (this._vars.Contains(var) && result.HasValue(var))
                {
                    INode value = result[var];
                    if (value.NodeType == NodeType.Literal)
                    {
                        this._values.Add(value.Value);
                    }
                }
            }
            return true;
        }
    }
}