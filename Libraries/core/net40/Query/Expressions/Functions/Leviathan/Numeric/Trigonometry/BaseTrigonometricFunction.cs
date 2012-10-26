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

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric.Trigonometry
{
    /// <summary>
    /// Abstract Base Class for Unary Trigonometric Functions in the Leviathan Function Library
    /// </summary>
    public abstract class BaseTrigonometricFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Trigonometric function
        /// </summary>
        protected Func<double, double> _func;

        /// <summary>
        /// Creates a new Unary Trigonometric Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public BaseTrigonometricFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Creates a new Unary Trigonometric Function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="func">Trigonometric Function</param>
        public BaseTrigonometricFunction(ISparqlExpression expr, Func<double, double> func)
            : base(expr)
        {
            this._func = func;
        }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode temp = this._expr.Evaluate(context, bindingID);
            if (temp == null) throw new RdfQueryException("Cannot apply a trigonometric function to a null");

            if (temp.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot apply a trigonometric function to a non-numeric argument");

            return new DoubleNode(this._func(temp.AsDouble()));
        }

        /// <summary>
        /// Gets the expression type
        /// </summary>
        public override SparqlExpressionType Type
        {
            get 
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the string representation of the Function
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();
    }
}
