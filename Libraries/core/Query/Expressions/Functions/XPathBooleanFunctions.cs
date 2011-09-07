/*

Copyright Robert Vesse 2009-10
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

namespace VDS.RDF.Query.Expressions.Functions
{
    /// <summary>
    /// Represents the XPath boolean() function
    /// </summary>
    public class XPathBooleanFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new XPath Boolean Function
        /// </summary>
        /// <param name="expr">Expression to compute the Effective Boolean Value of</param>
        public XPathBooleanFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Calculates the Effective Boolean Value of the contained expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID)
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this._expr.Value(context, bindingID));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Boolean + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get 
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Boolean;
            }
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
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new XPathBooleanFunction(transformer.Transform(this._expr));
        }
    }
}
