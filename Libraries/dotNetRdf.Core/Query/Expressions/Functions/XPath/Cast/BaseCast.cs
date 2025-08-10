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

namespace VDS.RDF.Query.Expressions.Functions.XPath.Cast;

/// <summary>
/// Abstract Expression class used as the base class for implementation of XPath Casting Function expressions.
/// </summary>
public abstract class BaseCast
    : ISparqlExpression
{
    /// <summary>
    /// Expression to be Cast by the Cast Function.
    /// </summary>
    protected ISparqlExpression _expr;

    /// <summary>
    /// Creates a new Base XPath Cast Expression.
    /// </summary>
    /// <param name="expr">Expression to be Cast.</param>
    protected BaseCast(ISparqlExpression expr)
    {
        _expr = expr;
    }

    /// <summary>
    /// Get the expression to be cast by the cast function.
    /// </summary>
    public ISparqlExpression InnerExpression { get => _expr; }

    /// <summary>
    /// Gets the String representation of this Expression.
    /// </summary>
    /// <returns></returns>
    public abstract override string ToString();

    /// <inheritdoc />
    public abstract TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding);

    /// <inheritdoc />
    public abstract T Accept<T>(ISparqlExpressionVisitor<T> visitor);

    /// <summary>
    /// Gets the enumeration of Variables involved in this expression.
    /// </summary>
    public virtual IEnumerable<string> Variables
    {
        get
        {
            return _expr.Variables;
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
    public abstract string Functor
    {
        get;
    }

    /// <summary>
    /// Gets the Arguments of the Expression.
    /// </summary>
    public IEnumerable<ISparqlExpression> Arguments
    {
        get
        {
            return _expr.AsEnumerable();
        }
    }

    /// <summary>
    /// Gets whether an expression can safely be evaluated in parallel.
    /// </summary>
    public virtual bool CanParallelise
    {
        get
        {
            return _expr.CanParallelise;
        }
    }

    /// <summary>
    /// Transforms the Expression using the given Transformer.
    /// </summary>
    /// <param name="transformer">Expression Transformer.</param>
    /// <returns></returns>
    public abstract ISparqlExpression Transform(IExpressionTransformer transformer);
}
