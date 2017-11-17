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

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric
{
    /// <summary>
    /// Represents the Leviathan lfn:pythagoras() function
    /// </summary>
    public class PythagoreanDistanceFunction
        : BaseBinaryExpression
    {
        /// <summary>
        /// Creates a new Leviathan Pythagorean Distance Function
        /// </summary>
        /// <param name="arg1">First Argument</param>
        /// <param name="arg2">Second Argument</param>
        public PythagoreanDistanceFunction(ISparqlExpression arg1, ISparqlExpression arg2)
            : base(arg1, arg2) { }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode x = _leftExpr.Evaluate(context, bindingID);
            if (x == null) throw new RdfQueryException("Cannot calculate distance of a null");
            IValuedNode y = _rightExpr.Evaluate(context, bindingID);
            if (y == null) throw new RdfQueryException("Cannot calculate distance of a null");

            if (x.NumericType == SparqlNumericType.NaN || y.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot calculate distance when one/both arguments are non-numeric");

            return new DoubleNode(null, Math.Sqrt(Math.Pow(x.AsDouble(), 2) + Math.Pow(y.AsDouble(), 2)));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Pythagoras + ">(" + _leftExpr.ToString() + "," + _rightExpr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Pythagoras;
            }
        }

        /// <summary>
        /// Gets the type of the expression
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
            return new PythagoreanDistanceFunction(transformer.Transform(_leftExpr), transformer.Transform(_rightExpr));
        }
    }
}
