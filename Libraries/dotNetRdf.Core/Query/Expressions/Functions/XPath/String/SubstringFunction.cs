/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2023 dotNetRDF Project (http://dotnetrdf.org/)
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

using System.Collections.Generic;
using System.Linq;

namespace VDS.RDF.Query.Expressions.Functions.XPath.String
{
    /// <summary>
    /// Represents the XPath fn:substring() function.
    /// </summary>
    public class SubstringFunction
        : ISparqlExpression
    {
        /// <summary>
        /// Creates a new XPath Substring function.
        /// </summary>
        /// <param name="stringExpr">Expression.</param>
        /// <param name="startExpr">Start.</param>
        public SubstringFunction(ISparqlExpression stringExpr, ISparqlExpression startExpr)
            : this(stringExpr, startExpr, null) { }

        /// <summary>
        /// Creates a new XPath Substring function.
        /// </summary>
        /// <param name="stringExpr">Expression.</param>
        /// <param name="startExpr">Start.</param>
        /// <param name="lengthExpr">Length.</param>
        public SubstringFunction(ISparqlExpression stringExpr, ISparqlExpression startExpr, ISparqlExpression lengthExpr)
        {
            InnerExpression = stringExpr;
            StartExpression = startExpr;
            LengthExpression = lengthExpr;
        }

        public ISparqlExpression InnerExpression { get; }
        public ISparqlExpression StartExpression { get; }
        public ISparqlExpression LengthExpression { get; }

        public TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
        {
            return processor.ProcessSubstringFunction(this, context, binding);
        }

        public T Accept<T>(ISparqlExpressionVisitor<T> visitor)
        {
            return visitor.VisitSubstringFunction(this);
        }

        /// <summary>
        /// Gets the Variables used in the function.
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                if (LengthExpression != null)
                {
                    return InnerExpression.Variables.Concat(StartExpression.Variables).Concat(LengthExpression.Variables);
                }
                else
                {
                    return InnerExpression.Variables.Concat(StartExpression.Variables);
                }
            }
        }

        /// <summary>
        /// Gets the String representation of the function.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (LengthExpression != null)
            {
                return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Substring + ">(" + InnerExpression.ToString() + "," + StartExpression.ToString() + "," + LengthExpression.ToString() + ")";
            }
            else
            {
                return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Substring + ">(" + InnerExpression.ToString() + "," + StartExpression.ToString() + ")";
            }
        }

        /// <summary>
        /// Gets the Type of the Expression.
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression.
        /// </summary>
        public string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Substring;
            }
        }

        /// <summary>
        /// Gets the Arguments of the Expression.
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                if (LengthExpression != null)
                {
                    return new ISparqlExpression[] { InnerExpression, StartExpression, LengthExpression };
                }
                else
                {
                    return new ISparqlExpression[] { InnerExpression, StartExpression };
                }
            }
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel.
        /// </summary>
        public virtual bool CanParallelise
        {
            get
            {
                return InnerExpression.CanParallelise && StartExpression.CanParallelise && (LengthExpression == null || LengthExpression.CanParallelise);
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer.
        /// </summary>
        /// <param name="transformer">Expression Transformer.</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            if (LengthExpression != null)
            {
                return new SubstringFunction(transformer.Transform(InnerExpression), transformer.Transform(StartExpression), transformer.Transform(LengthExpression));
            }
            else
            {
                return new SubstringFunction(transformer.Transform(InnerExpression), transformer.Transform(StartExpression));
            }
        }
    }
}
