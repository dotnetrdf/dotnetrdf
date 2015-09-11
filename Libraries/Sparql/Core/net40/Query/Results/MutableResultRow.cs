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
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Results
{
    /// <summary>
    /// Basic implementation of a mutable result row
    /// </summary>
    public class MutableResultRow
        : ResultRow, IMutableResultRow
    {
        private IResultRow _result;

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

        /// <summary>
        /// Creates a new result row as a copy of an existing row
        /// </summary>
        /// <param name="row">Row</param>
        public MutableResultRow(IResultRow row)
            : base(row) { }

        /// <summary>
        /// Sets the value of a variable
        /// </summary>
        /// <param name="var">Variable</param>
        /// <param name="value">Value</param>
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