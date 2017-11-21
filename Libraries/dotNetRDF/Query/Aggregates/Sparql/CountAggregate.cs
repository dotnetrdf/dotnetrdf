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
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Aggregates.Sparql
{
    /// <summary>
    /// Class representing COUNT Aggregate Function
    /// </summary>
    public class CountAggregate
        : BaseAggregate
    {
        private String _varname;

        /// <summary>
        /// Creates a new COUNT Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        public CountAggregate(VariableTerm expr)
            : base(expr)
        {
            _varname = expr.ToString().Substring(1);
        }

        /// <summary>
        /// Creates a new Count Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public CountAggregate(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Counts the results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs over which the Aggregate applies</param>
        /// <returns></returns>
        public override IValuedNode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            int c = 0;
            if (_varname != null)
            {
                // Just Count the number of results where the variable is bound
                VariableTerm varExpr = (VariableTerm)_expr;
                foreach (int id in bindingIDs)
                {
                    if (varExpr.Evaluate(context, id) != null) c++;
                }
            }
            else
            {
                // Count the number of results where the result in not null/error
                foreach (int id in bindingIDs)
                {
                    try
                    {
                        if (_expr.Evaluate(context, id) != null) c++;
                    }
                    catch
                    {
                        // Ignore errors
                    }
                }
            }

            return new LongNode(null, c);
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("COUNT(" + _expr.ToString() + ")");
            return output.ToString();
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordCount;
            }
        }
    }
}
