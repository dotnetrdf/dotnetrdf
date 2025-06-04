/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;

/// <summary>
/// Class representing the SPARQL BNODE() function.
/// </summary>
public class BNodeFunction 
    : BaseUnaryExpression
{
    /// <summary>
    /// Creates a new BNode Function.
    /// </summary>
    public BNodeFunction()
        : base(null) { }

    /// <summary>
    /// Creates a new BNode Function.
    /// </summary>
    /// <param name="expr">Argument Expression.</param>
    public BNodeFunction(ISparqlExpression expr)
        : base(expr) { }

    

    /// <summary>
    /// Gets the Type of the Expression.
    /// </summary>
    public override SparqlExpressionType Type
    {
        get
        {
            return SparqlExpressionType.Function;
        }
    }

    /// <summary>
    /// Gets the Functor of the Expression.
    /// </summary>
    public override string Functor
    {
        get
        {
            return SparqlSpecsHelper.SparqlKeywordBNode;
        }
    }

    /// <inheritdoc />
    public override T Accept<T>(ISparqlExpressionVisitor<T> visitor)
    {
        return visitor.VisitBNodeFunction(this);
    }

    /// <summary>
    /// Gets the Variables used in the Expression.
    /// </summary>
    public override IEnumerable<string> Variables
    {
        get
        {
            return InnerExpression == null ? Enumerable.Empty<string>() : base.Variables;
        }
    }

    /// <summary>
    /// Gets the Arguments of the Expression.
    /// </summary>
    public override IEnumerable<ISparqlExpression> Arguments
    {
        get
        {
            if (InnerExpression == null) return Enumerable.Empty<ISparqlExpression>();
            return base.Arguments;
        }
    }

    /// <summary>
    /// Gets whether the expression can be parallelised.
    /// </summary>
    public override bool CanParallelise
    {
        get
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the String representation of the Expression.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return SparqlSpecsHelper.SparqlKeywordBNode + "(" + InnerExpression.ToSafeString() + ")";
    }

    /// <inheritdoc />
    public override TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
    {
        return processor.ProcessBNodeFunction(this, context, binding);
    }

    /// <summary>
    /// Transforms the Expression using the given Transformer.
    /// </summary>
    /// <param name="transformer">Expression Transformer.</param>
    /// <returns></returns>
    public override ISparqlExpression Transform(IExpressionTransformer transformer)
    {
        return new BNodeFunction(transformer.Transform(InnerExpression));
    }
}
