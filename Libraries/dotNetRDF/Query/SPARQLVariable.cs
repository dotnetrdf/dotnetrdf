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
using System.Text;
using VDS.RDF.Query.Aggregates;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query
{
    /// <summary>
    /// Class of Sparql Variables
    /// </summary>
    public class SparqlVariable
    {
        private String _name;
        private bool _isResultVar;
        private ISparqlAggregate _aggregate = null;
        private ISparqlExpression _expr = null;

        /// <summary>
        /// Creates a new Sparql Variable
        /// </summary>
        /// <param name="name">Variable Name</param>
        /// <param name="isResultVar">Does this Variable appear in the Result Set?</param>
        public SparqlVariable(String name, bool isResultVar) {
            _name = name;
            _isResultVar = isResultVar;

            // Strip leading ?/$ if present
            if (_name.StartsWith("?") || _name.StartsWith("$"))
            {
                _name = _name.Substring(1);
            }
        }

        /// <summary>
        /// Creates a new Sparql Variable
        /// </summary>
        /// <param name="name">Variable Name (with leading ?/$ removed)</param>
        public SparqlVariable(String name) 
            : this(name, false) { }

        /// <summary>
        /// Creates a new Sparql Variable which is an Aggregate
        /// </summary>
        /// <param name="name">Variable Name (with leading ?/$ removed)</param>
        /// <param name="aggregate">Aggregate Function</param>
        /// <remarks>All Aggregate Variables are automatically considered as Result Variables</remarks>
        public SparqlVariable(String name, ISparqlAggregate aggregate)
            : this(name, true)
        {
            _aggregate = aggregate;
        }

        /// <summary>
        /// Creates a new Sparql Variable which is a Projection Expression
        /// </summary>
        /// <param name="name">Variable Name (with leading ?/$ removed)</param>
        /// <param name="expr">Projection Expression</param>
        public SparqlVariable(String name, ISparqlExpression expr)
            : this(name, true)
        {
            _expr = expr;
        }

        /// <summary>
        /// Variable Name
        /// </summary>
        public String Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets whether the Variable appears in the Result Set
        /// </summary>
        public bool IsResultVariable
        {
            get
            {
                return _isResultVar;
            }
        }

        /// <summary>
        /// Gets whether the Variable is an Aggregate 
        /// </summary>
        public bool IsAggregate
        {
            get
            {
                return (_aggregate != null);
            }
        }

        /// <summary>
        /// Gets whether the Variable is a Projection Expression
        /// </summary>
        public bool IsProjection
        {
            get
            {
                return (_expr != null);
            }
        }

        /// <summary>
        /// Gets the Aggregate Function for this Variable
        /// </summary>
        public ISparqlAggregate Aggregate
        {
            get
            {
                return _aggregate;
            }
        }

        /// <summary>
        /// Gets the Projection Expression for this Variable
        /// </summary>
        public ISparqlExpression Projection
        {
            get
            {
                return _expr;
            }
        }
        
        /// <summary>
        /// Get the String representation of the Variable
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            if (_aggregate != null)
            {
                output.Append('(');
                output.Append(_aggregate.ToString());
                output.Append(" AS ?" + _name);
                output.Append(')');
            }
            else if (_expr != null)
            {
                output.Append('(');
                output.Append(_expr.ToString());
                output.Append(" AS ?" + _name);
                output.Append(')');
            }
            else
            {
                output.Append("?" + _name);
            }
            return output.ToString();
        }
    }
}
