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
    /// Class representing SUM Aggregate Functions
    /// </summary>
    public class SumAggregate
        : BaseAggregate
    {
        private String _varname;

        /// <summary>
        /// Creates a new SUM Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public SumAggregate(VariableTerm expr, bool distinct)
            : base(expr, distinct)
        {
            _varname = expr.ToString().Substring(1);
        }

        /// <summary>
        /// Creates a new SUM Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public SumAggregate(ISparqlExpression expr, bool distinct)
            : base(expr, distinct) { }

        /// <summary>
        /// Creates a new SUM Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        public SumAggregate(VariableTerm expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new SUM Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public SumAggregate(ISparqlExpression expr)
            : this(expr, false) { }

        /// <summary>
        /// Applies the Sum Aggregate function to the results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs over which the Aggregate applies</param>
        /// <returns></returns>
        public override IValuedNode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            // Prep Variables
            long lngtotal = 0;
            decimal dectotal = 0.0m;
            float flttotal = 0.0f;
            double dbltotal = 0.0d;
            SparqlNumericType maxtype = SparqlNumericType.NaN;
            SparqlNumericType numtype;
            HashSet<IValuedNode> values = new HashSet<IValuedNode>();

            foreach (int id in bindingIDs)
            {
                IValuedNode temp;
                try
                {
                    temp = _expr.Evaluate(context, id);
                    if (_distinct)
                    {
                        
                        if (temp == null) continue;
                        if (values.Contains(temp))
                        {
                            continue;
                        }
                        else
                        {
                            values.Add(temp);
                        }
                    }
                    numtype = temp.NumericType;
                }
                catch
                {
                    continue;
                }

                // Skip if Not a Number
                if (numtype == SparqlNumericType.NaN) continue;

                // Track the Numeric Type
                if ((int)numtype > (int)maxtype)
                {
                    maxtype = numtype;
                }

                // Increment the Totals based on the current Numeric Type
                switch (maxtype)
                {
                    case SparqlNumericType.Integer:
                        lngtotal += temp.AsInteger();
                        dectotal += temp.AsDecimal();
                        flttotal += temp.AsFloat();
                        dbltotal += temp.AsDouble();
                        break;
                    case SparqlNumericType.Decimal:
                        dectotal += temp.AsDecimal();
                        flttotal += temp.AsFloat();
                        dbltotal += temp.AsDouble();
                        break;
                    case SparqlNumericType.Float:
                        flttotal += temp.AsFloat();
                        dbltotal += temp.AsDouble();
                        break;
                    case SparqlNumericType.Double:
                        dbltotal += temp.AsDouble();
                        break;
                }
            }

            // Return the Sum
            switch (maxtype)
            {
                case SparqlNumericType.NaN:
                    // No Numeric Values
                    return new LongNode(null, 0);

                case SparqlNumericType.Integer:
                    // Integer Values
                    return new LongNode(null, lngtotal);

                case SparqlNumericType.Decimal:
                    // Decimal Values
                    return new DecimalNode(null, dectotal);

                case SparqlNumericType.Float:
                    // Float Values
                    return new FloatNode(null, flttotal);

                case SparqlNumericType.Double:
                    // Double Values
                    return new DoubleNode(null, dbltotal);

                default:
                    throw new RdfQueryException("Failed to calculate a valid Sum");
            }
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("SUM(");
            if (_distinct) output.Append("DISTINCT ");
            output.Append(_expr.ToString() + ")");
            return output.ToString();
        }

        /// <summary>
        /// Gets the Functor of the Aggregate
        /// </summary>
        public override string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordSum;
            }
        }
    }
}
