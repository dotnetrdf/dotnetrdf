/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2020 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Arithmetic
{
    /// <summary>
    /// Class representing Unary Minus expressions (sign of numeric expression is reversed).
    /// </summary>
    public class MinusExpression
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Unary Minus Expression.
        /// </summary>
        /// <param name="expr">Expression to apply the Minus operator to.</param>
        public MinusExpression(ISparqlExpression expr) 
            : base(expr) { }

        /// <summary>
        /// Calculates the Numeric Value of this Expression as evaluated for a given Binding.
        /// </summary>
        /// <param name="context">Evaluation Context.</param>
        /// <param name="bindingID">Binding ID.</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode a = _expr.Evaluate(context, bindingID);
            if (a == null) throw new RdfQueryException("Cannot apply unary minus to a null");

            switch (a.NumericType)
            {
                case SparqlNumericType.Integer:
                    return new LongNode(-1 * a.AsInteger());

                case SparqlNumericType.Decimal:
                    var decvalue = a.AsDecimal();
                    if (decvalue == decimal.Zero)
                    {
                        return new DecimalNode(decimal.Zero);
                    }
                    else
                    {
                        return new DecimalNode(-1 * decvalue);
                    }
                case SparqlNumericType.Float:
                    var fltvalue = a.AsFloat();
                    if (float.IsNaN(fltvalue))
                    {
                        return new FloatNode(float.NaN);
                    }
                    else if (float.IsPositiveInfinity(fltvalue))
                    {
                        return new FloatNode(float.NegativeInfinity);
                    }
                    else if (float.IsNegativeInfinity(fltvalue))
                    {
                        return new FloatNode(float.PositiveInfinity);
                    }
                    else
                    {
                        return new FloatNode(-1.0f * fltvalue);
                    }
                case SparqlNumericType.Double:
                    var dblvalue = a.AsDouble();
                    if (double.IsNaN(dblvalue))
                    {
                        return new DoubleNode(double.NaN);
                    }
                    else if (double.IsPositiveInfinity(dblvalue))
                    {
                        return new DoubleNode(double.NegativeInfinity);
                    }
                    else if (double.IsNegativeInfinity(dblvalue))
                    {
                        return new DoubleNode(double.PositiveInfinity);
                    }
                    else
                    {
                        return new DoubleNode(-1.0 * dblvalue);
                    }
                default:
                    throw new RdfQueryException("Cannot evaluate an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "-" + _expr.ToString();
        }

        /// <summary>
        /// Gets the Type of the Expression.
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.UnaryOperator;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression.
        /// </summary>
        public override string Functor
        {
            get
            {
                return "-";
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer.
        /// </summary>
        /// <param name="transformer">Expression Transformer.</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new MinusExpression(transformer.Transform(_expr));
        }
    }
}
