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
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric
{
    /// <summary>
    /// Represents the Leviathan lfn:log() function
    /// </summary>
    public class LogFunction
        : BaseBinaryExpression
    {
        private bool _log10 = false;

        /// <summary>
        /// Creates a new Leviathan Log Function
        /// </summary>
        /// <param name="arg">Expression</param>
        public LogFunction(ISparqlExpression arg)
            : base(arg, new ConstantTerm(new DoubleNode(null, 10)))
        {
            _log10 = true;
        }

        /// <summary>
        /// Creates a new Leviathan Log Function
        /// </summary>
        /// <param name="arg">Expression</param>
        /// <param name="logBase">Log Base Expression</param>
        public LogFunction(ISparqlExpression arg, ISparqlExpression logBase)
            : base(arg, logBase) { }

        /// <summary>
        /// Evaluates the expression
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        public override IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            IValuedNode arg = _leftExpr.Evaluate(context, bindingID);
            if (arg == null) throw new RdfQueryException("Cannot log a null");
            IValuedNode logBase = _rightExpr.Evaluate(context, bindingID);
            if (logBase == null) throw new RdfQueryException("Cannot log to a null base");

            if (arg.NumericType == SparqlNumericType.NaN || logBase.NumericType == SparqlNumericType.NaN) throw new RdfQueryException("Cannot log when one/both arguments are non-numeric");

            return new DoubleNode(null, Math.Log(arg.AsDouble(), logBase.AsDouble()));
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (_log10)
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Log + ">(" + _leftExpr.ToString() + ")";
            }
            else
            {
                return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Log + ">(" + _leftExpr.ToString() + "," + _rightExpr.ToString() + ")";
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.Log;
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
            if (_log10)
            {
                return new LogFunction(transformer.Transform(_leftExpr));
            }
            else
            {
                return new LogFunction(transformer.Transform(_leftExpr), transformer.Transform(_rightExpr));
            }
        }
    }
}
