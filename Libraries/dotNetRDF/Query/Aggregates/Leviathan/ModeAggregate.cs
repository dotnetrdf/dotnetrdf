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
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Aggregates.Leviathan
{
    /// <summary>
    /// Class representing MODE Aggregate Functions
    /// </summary>
    public class ModeAggregate
        : BaseAggregate
    {
        private String _varname;

        /// <summary>
        /// Creates a new MODE Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        public ModeAggregate(VariableTerm expr)
            : base(expr, false) { }

        /// <summary>
        /// Creates a new MODE Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public ModeAggregate(ISparqlExpression expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new MODE Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public ModeAggregate(VariableTerm expr, bool distinct)
            : base(expr, distinct)
        {
            _varname = expr.ToString().Substring(1);
        }
        /// <summary>
        /// Creates a new MODE Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public ModeAggregate(ISparqlExpression expr, bool distinct)
            : base(expr, distinct) { }

        /// <summary>
        /// Applies the Mode Aggregate function to the results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs over which the Aggregate applies</param>
        /// <returns></returns>
        public override IValuedNode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            if (_varname != null)
            {
                // Ensured the MODEd variable is in the Variables of the Results
                if (!context.Binder.Variables.Contains(_varname))
                {
                    throw new RdfQueryException("Cannot use the Variable " + _expr.ToString() + " in a MODE Aggregate since the Variable does not occur in a Graph Pattern");
                }
            }

            Dictionary<IValuedNode, int> values = new Dictionary<IValuedNode, int>();
            int nullCount = 0;
            foreach (int id in bindingIDs)
            {
                try
                {
                    IValuedNode temp = _expr.Evaluate(context, id);
                    if (temp == null)
                    {
                        nullCount++;
                    }
                    else
                    {
                        if (values.ContainsKey(temp))
                        {
                            values[temp]++;
                        }
                        else
                        {
                            values.Add(temp, 1);
                        }
                    }
                }
                catch
                {
                    // Errors count as nulls
                    nullCount++;
                }
            }

            int mostPopular = values.Values.Max();
            if (mostPopular > nullCount)
            {
                return values.FirstOrDefault(p => p.Value == mostPopular).Key;
            }
            else
            {
                // Null is the most popular item
                return null;
            }
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append('<');
            output.Append(LeviathanFunctionFactory.LeviathanFunctionsNamespace);
            output.Append(LeviathanFunctionFactory.Mode);
            output.Append(">(");
            if (_distinct) output.Append("DISTINCT ");
            output.Append(_expr.ToString());
            output.Append(')');
            return output.ToString();
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Mode;
            }
        }
    }
}
