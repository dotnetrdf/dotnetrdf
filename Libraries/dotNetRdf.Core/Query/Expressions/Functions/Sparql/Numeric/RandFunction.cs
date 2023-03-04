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

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Numeric
{
    /// <summary>
    /// Represents the SPARQL RAND() Function.
    /// </summary>
    public class RandFunction
        : ISparqlExpression
    {
        public TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
        {
            return processor.ProcessRandFunction(this, context, binding);
        }

        public T Accept<T>(ISparqlExpressionVisitor<T> visitor)
        {
            return visitor.VisitRandFunction(this);
        }

        /// <summary>
        /// Gets the Variables used in this Expression.
        /// </summary>
        public IEnumerable<string> Variables
        {
            get
            {
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Gets the Type of this Expression.
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Arguments of this Expression.
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get
            {
                return Enumerable.Empty<ISparqlExpression>();
            }
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel.
        /// </summary>
        public virtual bool CanParallelise
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the Functor of this Expression.
        /// </summary>
        public string Functor
        {
            get
            {
                return SparqlSpecsHelper.SparqlKeywordRand;
            }
        }

        /// <summary>
        /// Gets the String representation of this Expression.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SparqlSpecsHelper.SparqlKeywordRand + "()";
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer.
        /// </summary>
        /// <param name="transformer">Expression Transformer.</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return this;
        }
    }
}
