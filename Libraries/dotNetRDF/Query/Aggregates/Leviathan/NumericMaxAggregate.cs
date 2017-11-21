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
    /// Class representing NMAX Aggregate Functions
    /// </summary>
    /// <remarks>
    /// Only operates over numeric data which is typed to one of the supported SPARQL Numeric types (integers, decimals and doubles)
    /// </remarks>
    public class NumericMaxAggregate 
        : BaseAggregate
    {
        private String _varname;

        /// <summary>
        /// Creates a new NMAX Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        public NumericMaxAggregate(VariableTerm expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new NMAX Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        public NumericMaxAggregate(ISparqlExpression expr)
            : this(expr, false) { }

        /// <summary>
        /// Creates a new NMAX Aggregate
        /// </summary>
        /// <param name="expr">Variable Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public NumericMaxAggregate(VariableTerm expr, bool distinct)
            : base(expr, distinct)
        {
            _varname = expr.ToString().Substring(1);
        }

        /// <summary>
        /// Creates a new NMAX Aggregate
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="distinct">Whether a DISTINCT modifier applies</param>
        public NumericMaxAggregate(ISparqlExpression expr, bool distinct)
            : base(expr, distinct) { }

        /// <summary>
        /// Applies the Numeric Max Aggregate function to the results
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingIDs">Binding IDs over which the Aggregate applies</param>
        /// <returns></returns>
        public override IValuedNode Apply(SparqlEvaluationContext context, IEnumerable<int> bindingIDs)
        {
            if (_varname != null)
            {
                // Ensured the MAXed variable is in the Variables of the Results
                if (!context.Binder.Variables.Contains(_varname))
                {
                    throw new RdfQueryException("Cannot use the Variable " + _expr.ToString() + " in a NMAX Aggregate since the Variable does not occur in a Graph Pattern");
                }
            }

            // Prep Variables
            long lngmax = 0;
            decimal decmax = 0.0m;
            float fltmax = 0.0f;
            double dblmax = 0.0d;
            SparqlNumericType maxtype = SparqlNumericType.NaN;
            SparqlNumericType numtype;

            foreach (int id in bindingIDs)
            {
                IValuedNode temp;
                try
                {
                    temp = _expr.Evaluate(context, id);
                    if (temp == null) continue;
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
                    if (maxtype == SparqlNumericType.NaN)
                    {
                        // Initialise Maximums
                        switch (numtype)
                        {
                            case SparqlNumericType.Integer:
                                lngmax = temp.AsInteger();
                                decmax = temp.AsDecimal();
                                fltmax = temp.AsFloat();
                                dblmax = temp.AsDouble();
                                break;
                            case SparqlNumericType.Decimal:
                                decmax = temp.AsDecimal();
                                fltmax = temp.AsFloat();
                                dblmax = temp.AsDouble();
                                break;
                            case SparqlNumericType.Float:
                                fltmax = temp.AsFloat();
                                dblmax = temp.AsDouble();
                                break;
                            case SparqlNumericType.Double:
                                dblmax = temp.AsDouble();
                                break;
                        }
                        maxtype = numtype;
                        continue;
                    }
                    else
                    {
                        maxtype = numtype;
                    }
                }

                long lngval;
                decimal decval;
                float fltval;
                double dblval;
                switch (maxtype)
                {
                    case SparqlNumericType.Integer:
                        lngval = temp.AsInteger();

                        if (lngval > lngmax)
                        {
                            lngmax = lngval;
                            decmax = temp.AsDecimal();
                            fltmax = temp.AsFloat();
                            dblmax = temp.AsDouble();
                        }
                        break;
                    case SparqlNumericType.Decimal:
                        decval = temp.AsDecimal();

                        if (decval > decmax)
                        {
                            decmax = decval;
                            fltmax = temp.AsFloat();
                            dblmax = temp.AsDouble();
                        }
                        break;
                    case SparqlNumericType.Float:
                        fltval = temp.AsFloat();

                        if (fltval > fltmax)
                        {
                            fltmax = fltval;
                            dblmax = temp.AsDouble();
                        }
                        break;
                    case SparqlNumericType.Double:
                        dblval = temp.AsDouble();

                        if (dblval > dblmax)
                        {
                            dblmax = dblval;
                        }
                        break;
                }
            }

            // Return the Max
            switch (maxtype)
            {
                case SparqlNumericType.NaN:
                    // No Numeric Values
                    return null;

                case SparqlNumericType.Integer:
                    // Integer Values
                    return new LongNode(null, lngmax);

                case SparqlNumericType.Decimal:
                    // Decimal Values
                    return new DecimalNode(null, decmax);

                case SparqlNumericType.Float:
                    // Float values
                    return new FloatNode(null, fltmax);

                case SparqlNumericType.Double:
                    // Double Values
                    return new DoubleNode(null, dblmax);

                default:
                    throw new RdfQueryException("Failed to calculate a valid Maximum");
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
            output.Append(LeviathanFunctionFactory.NumericMax);
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
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.NumericMax;
            }
        }
    }
}
