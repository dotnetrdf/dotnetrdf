/*

Copyright Robert Vesse 2009-12
rvesse@vdesign-studios.com

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

namespace VDS.RDF.Query.Expressions.Functions.XPath.Numeric
{
    /// <summary>
    /// Represents the XPath fn:round() function
    /// </summary>
    public class RoundFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new XPath Round function
        /// </summary>
        /// <param name="expr">Expression</param>
        public RoundFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Gets the Numeric Value of the function as evaluated in the given Context for the given Binding ID
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode a = this._expr.Evaluate(context, bindingID);
            if (a == null) throw new RdfQueryException("Cannot calculate an arithmetic expression on a null");

            switch (a.NumericType)
            {
                case SparqlNumericType.Integer:
                    //Rounding an Integer has no effect
                    return a;

                case SparqlNumericType.Decimal:
#if !SILVERLIGHT
                    return new DecimalNode(null, Math.Round(a.AsDecimal(), MidpointRounding.AwayFromZero));
#else
                    return new DecimalNode(null, Math.Round(a.AsDecimal()));
#endif

                case SparqlNumericType.Float:
                    try
                    {
#if !SILVERLIGHT
                        return new FloatNode(null, Convert.ToSingle(Math.Round(a.AsDouble(), MidpointRounding.AwayFromZero)));
#else
                        return new FloatNode(null, Convert.ToSingle(Math.Round(a.AsDouble())));
#endif
                    }
                    catch (RdfQueryException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new RdfQueryException("Unable to cast the float value of a round to a float", ex);
                    }

                case SparqlNumericType.Double:
#if !SILVERLIGHT
                    return new DoubleNode(null, Math.Round(a.AsDouble(), MidpointRounding.AwayFromZero));
#else
                    return new DoubleNode(null, Math.Round(a.AsDouble()));
#endif

                default:
                    throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Round + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of the Expression
        /// </summary>
        public override SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Round;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new RoundFunction(transformer.Transform(this._expr));
        }
    }
}
