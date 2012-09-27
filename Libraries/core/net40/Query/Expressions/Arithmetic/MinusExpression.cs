/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Arithmetic
{
    /// <summary>
    /// Class representing Unary Minus expressions (sign of numeric expression is reversed)
    /// </summary>
    public class MinusExpression
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Unary Minus Expression
        /// </summary>
        /// <param name="expr">Expression to apply the Minus operator to</param>
        public MinusExpression(ISparqlExpression expr) 
            : base(expr) { }

        /// <summary>
        /// Calculates the Numeric Value of this Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode a = this._expr.Evaluate(context, bindingID);
            if (a == null) throw new RdfQueryException("Cannot apply unary minus to a null");

            switch (a.NumericType)
            {
                case SparqlNumericType.Integer:
                    return new LongNode(null, -1 * a.AsInteger());

                case SparqlNumericType.Decimal:
                    decimal decvalue = a.AsDecimal();
                    if (decvalue == Decimal.Zero)
                    {
                        return new DecimalNode(null, Decimal.Zero);
                    }
                    else
                    {
                        return new DecimalNode(null, -1 * decvalue);
                    }
                case SparqlNumericType.Float:
                    float fltvalue = a.AsFloat();
                    if (Single.IsNaN(fltvalue))
                    {
                        return new FloatNode(null, Single.NaN);
                    }
                    else if (Single.IsPositiveInfinity(fltvalue))
                    {
                        return new FloatNode(null, Single.NegativeInfinity);
                    }
                    else if (Single.IsNegativeInfinity(fltvalue))
                    {
                        return new FloatNode(null, Single.PositiveInfinity);
                    }
                    else
                    {
                        return new FloatNode(null, -1.0f * fltvalue);
                    }
                case SparqlNumericType.Double:
                    double dblvalue = a.AsDouble();
                    if (Double.IsNaN(dblvalue))
                    {
                        return new DoubleNode(null, Double.NaN);
                    }
                    else if (Double.IsPositiveInfinity(dblvalue))
                    {
                        return new DoubleNode(null, Double.NegativeInfinity);
                    }
                    else if (Double.IsNegativeInfinity(dblvalue))
                    {
                        return new DoubleNode(null, Double.PositiveInfinity);
                    }
                    else
                    {
                        return new DoubleNode(null, -1.0 * dblvalue);
                    }
                default:
                    throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "-" + this._expr.ToString();
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.UnaryOperator;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return "-";
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new MinusExpression(transformer.Transform(this._expr));
        }
    }
}
