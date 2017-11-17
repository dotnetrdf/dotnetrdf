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
    /// Class representing AVG Aggregate Functions
    /// </summary>
    public class AverageAggregate
        : BaseAggregate
    {
        private String _varname;

        /// <summary>
        /// Creates a new AVG Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public AverageAggregate(VariableTerm expr, bool distinct)
            : base(expr, distinct)
        {
            _varname = expr.ToString().Substring(1);
        }

        /// <summary>
        /// Creates a new AVG Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public AverageAggregate(ISparqlExpression expr, bool distinct)
            : base(expr, distinct) { }

        /// <summary>
        /// Creates a new AVG Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        public AverageAggregate(VariableTerm expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new AVG Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public AverageAggregate(ISparqlExpression expr)
            : this(expr, false) { }

        /// <summary>
        /// Applies the Average Aggregate function to the results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs over which the aggregate applies</param>
        /// <returns></returns>
        public override IValuedNode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            // Prep Variables
            HashSet<IValuedNode> values = new HashSet<IValuedNode>();
            int count = 0;
            // long lngtotal = 0;
            decimal dectotal = 0.0m;
            float flttotal = 0.0f;
            double dbltotal = 0.0d;
            SparqlNumericType maxtype = SparqlNumericType.NaN;
            SparqlNumericType numtype;

            foreach (int id in bindingIDs)
            {
                IValuedNode temp;
                try
                {
                    temp = _expr.Evaluate(context, id);
                    if (temp == null) return null;
                    // Apply DISTINCT modifier if required
                    if (_distinct)
                    {
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
                    // SPARQL Working Group changed spec so this should now return no binding
                    return null;
                }

                // No result if anything resolves to non-numeric
                if (numtype == SparqlNumericType.NaN) return null;

                // Track the Numeric Type
                if ((int)numtype > (int)maxtype)
                {
                    maxtype = numtype;
                }

                // Increment the Totals based on the current Numeric Type
                switch (maxtype)
                {
                    case SparqlNumericType.Integer:
                        // lngtotal += numExpr.IntegerValue(context, id);
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

                count++;
            }

            // Calculate the Average
            if (count == 0)
            {
                return new LongNode(null, 0);
            }
            else
            {
                // long lngavg;
                decimal decavg;
                float fltavg;
                double dblavg;

                switch (maxtype)
                {
                    case SparqlNumericType.Integer:
                    ////Integer Values
                    // lngavg = lngtotal / (long)count;
                    // return new LiteralNode(null, lngavg.ToString(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));

                    case SparqlNumericType.Decimal:
                        // Decimal Values
                        decavg = dectotal / (decimal)count;
                        return new DecimalNode(null, decavg);

                    case SparqlNumericType.Float:
                        // Float values
                        fltavg = flttotal / (float)count;
                        return new FloatNode(null, fltavg);

                    case SparqlNumericType.Double:
                        // Double Values
                        dblavg = dbltotal / (double)count;
                        return new DoubleNode(null, dblavg);

                    default:
                        throw new RdfQueryException("Failed to calculate a valid Average");
                }
            }
        }

        /// <summary>
        /// Gets the String representation of the Aggregate
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("AVG(");
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
                return SparqlSpecsHelper.SparqlKeywordAvg;
            }
        }
    }
}
