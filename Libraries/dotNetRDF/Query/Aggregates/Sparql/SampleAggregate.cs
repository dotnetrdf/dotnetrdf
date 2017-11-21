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

using System.Collections.Generic;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Aggregates.Sparql
{
    /// <summary>
    /// Class representing the SAMPLE aggregate
    /// </summary>
    public class SampleAggregate
        : BaseAggregate
    {
        /// <summary>
        /// Creates a new SAMPLE Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public SampleAggregate(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Applies the SAMPLE Aggregate
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs</param>
        /// <returns></returns>
        public override IValuedNode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            // Try the expression with each member of the Group until we find a non-null
            foreach (int id in bindingIDs)
            {
                try
                {

                    // First non-null result we find is returned
                    IValuedNode temp = _expr.Evaluate(context, id);
                    if (temp != null) return temp;
                }
                catch (RdfQueryException)
                {
                    // Ignore errors - we'll loop round and try the next
                }
            }

            // If the Group is Empty of the Expression fails to evaluate for the entire Group then the result is null
            return null;
        }

        /// <summary>
        /// Gets the String representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_distinct)
            {
                return "SAMPLE(DISTINCT " + _expr.ToString() + ")";
            }
            else
            {
                return "SAMPLE(" + _expr.ToString() + ")";
            }
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordSample;
            }
        }
    }
}
