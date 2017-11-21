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
    /// Represents the Leviathan lfn:cube() function
    /// </summary>
    public class CubeFunction
        : BaseUnaryExpression
    {
        /// <summary>
        /// Creates a new Leviathan Cube Function
        /// </summary>
        /// <param name="expr">Expression</param>
        public CubeFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode temp = _expr.Evaluate(context, bindingID);
            if (temp == null) throw new RdfQueryException("Cannot square a null");

            switch (temp.NumericType)
            {
                case SparqlNumericType.Integer:
                    long l = temp.AsInteger();
                    return new LongNode(null, l * l * l);
                case SparqlNumericType.Decimal:
                    decimal d = temp.AsDecimal();
                    return new DecimalNode(null, d * d * d);
                case SparqlNumericType.Float:
                    float f = temp.AsFloat();
                    return new FloatNode(null, f * f * f);
                case SparqlNumericType.Double:
                    double dbl = temp.AsDouble();
                    return new DoubleNode(null, Math.Pow(dbl, 3));
                case SparqlNumericType.NaN:
                default:
                    throw new RdfQueryException("Cannot square a non-numeric argument");
            }
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Cube + ">(" + _expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Type of this expression
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
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Cube;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new CubeFunction(transformer.Transform(_expr));
        }
    }
}
