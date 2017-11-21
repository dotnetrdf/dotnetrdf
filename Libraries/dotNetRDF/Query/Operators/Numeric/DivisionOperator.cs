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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Operators.Numeric
{
    /// <summary>
    /// Represents the numeric division operator
    /// </summary>
    public class DivisionOperator
        : BaseNumericOperator
    {
        /// <summary>
        /// Gets the operator type
        /// </summary>
        public override SparqlOperatorType Operator
        {
            get
            {
                return SparqlOperatorType.Divide;
            }
        }

        /// <summary>
        /// Applies the operator
        /// </summary>
        /// <param name="ns">Arguments</param>
        /// <returns></returns>
        public override IValuedNode Apply(params IValuedNode[] ns)
        {
            if (ns.Any(n => n == null)) throw new RdfQueryException("Cannot apply division when any arguments are null");

            SparqlNumericType type = (SparqlNumericType)ns.Max(n => (int)n.NumericType);

            try
            {
                switch (type)
                {
                    case SparqlNumericType.Integer:
                    case SparqlNumericType.Decimal:
                        // For Division Integers are treated as decimals
                        decimal d = Divide(ns.Select(n => n.AsDecimal()));
                        if (Decimal.Floor(d).Equals(d) && d >= Int64.MinValue && d <= Int64.MaxValue)
                        {
                            return new LongNode(null, Convert.ToInt64(d));
                        }
                        return new DecimalNode(null, d);
                    case SparqlNumericType.Float:
                        return new FloatNode(null, Divide(ns.Select(n => n.AsFloat())));
                    case SparqlNumericType.Double:
                        return new DoubleNode(null, Divide(ns.Select(n => n.AsDouble())));
                    default:
                        throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
                }
            }
            catch (DivideByZeroException)
            {
                throw new RdfQueryException("Cannot evaluate a Division Expression where the divisor is Zero");
            }
        }

        private decimal Divide(IEnumerable<decimal> ls)
        {
            bool first = true;
            decimal total = 0;
            foreach (decimal l in ls)
            {
                if (first)
                {
                    total = l;
                    first = false;
                }
                else
                {
                    total /= l;
                }
            }
            return total;
        }

        private float Divide(IEnumerable<float> ls)
        {
            bool first = true;
            float total = 0;
            foreach (float l in ls)
            {
                if (first)
                {
                    total = l;
                    first = false;
                }
                else
                {
                    total /= l;
                }
            }
            return total;
        }

        private double Divide(IEnumerable<double> ls)
        {
            bool first = true;
            double total = 0;
            foreach (double l in ls)
            {
                if (first)
                {
                    total = l;
                    first = false;
                }
                else
                {
                    total /= l;
                }
            }
            return total;
        }
    }
}
