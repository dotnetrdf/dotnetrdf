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

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric.Trigonometry
{
    /// <summary>
    /// Represents the Leviathan lfn:cosec() or lfn:cosec-1 function
    /// </summary>
    public class CosecantFunction
        : BaseTrigonometricFunction
    {
        private bool _inverse = false;
        private static Func<double, double> _cosecant = (d => (1 / Math.Sin(d)));
        private static Func<double, double> _arccosecant = (d => Math.Asin(1 / d));

        /// <summary>
        /// Creates a new Leviathan Cosecant Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public CosecantFunction(ISparqlExpression expr)
            : base(expr, _cosecant) { }

        /// <summary>
        /// Creates a new Leviathan Cosecant Function
        /// </summary>
        /// <param name="expr">Expression</param>
        /// <param name="inverse">Whether this should be the inverse function</param>
        public CosecantFunction(ISparqlExpression expr, bool inverse)
            : base(expr)
        {
            _inverse = inverse;
            if (_inverse)
            {
                _func = _arccosecant;
            }
            else
            {
                _func = _cosecant;
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_inverse)
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCosecInv + ">(" + _expr.ToString() + ")";
            }
            else
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCosec + ">(" + _expr.ToString() + ")";
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                if (_inverse)
                {
                    return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCosecInv;
                }
                else
                {
                    return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCosec;
                }
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new CosecantFunction(transformer.Transform(_expr), _inverse);
        }
    }
}
