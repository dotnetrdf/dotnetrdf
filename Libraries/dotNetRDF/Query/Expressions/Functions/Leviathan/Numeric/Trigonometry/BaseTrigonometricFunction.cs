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
            _func = func;
        }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode temp = _expr.Evaluate(context, bindingID);
            if (temp == null) throw new RdfQueryException("Cannot apply a trigonometric function to a null");

            if (temp.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot apply a trigonometric function to a non-numeric argument");

            return new DoubleNode(null, _func(temp.AsDouble()));
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
