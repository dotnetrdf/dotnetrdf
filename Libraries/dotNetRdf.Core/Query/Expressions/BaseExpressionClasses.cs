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

namespace VDS.RDF.Query.Expressions;

/// <summary>
/// Abstract base class for Unary Expressions.
/// </summary>
public abstract class BaseUnaryExpression 
    : ISparqlExpression
{
    /// <summary>
    /// Creates a new Base Unary Expression.
    /// </summary>
    /// <param name="expr">Expression.</param>
    public BaseUnaryExpression(ISparqlExpression expr)
    {
        InnerExpression = expr;
    }

    /// <summary>
    /// The sub-expression of this Expression.
    /// </summary>
    public ISparqlExpression InnerExpression { get; }

    /// <summary>
    /// Gets the String representation of the Expression.
    /// </summary>
    /// <returns></returns>
    public abstract override string ToString();

    /// <inheritdoc />
    public abstract TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding);

    /// <inheritdoc />
    public abstract T Accept<T>(ISparqlExpressionVisitor<T> visitor);

    /// <summary>
    /// Gets an enumeration of all the Variables used in this expression.
    /// </summary>
    public virtual IEnumerable<string> Variables
    {
        get
        {
            return InnerExpression.Variables;
        }
    }

    /// <summary>
    /// Gets the Type of the Expression.
    /// </summary>
    public abstract SparqlExpressionType Type
    {
        get;
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
    public virtual IEnumerable<ISparqlExpression> Arguments
    {
        get
        {
            return InnerExpression.AsEnumerable();
        }
    }

    /// <summary>
    /// Gets whether an expression can safely be evaluated in parallel.
    /// </summary>
    public virtual bool CanParallelise
    {
        get
        {
            return InnerExpression.CanParallelise;
        }
    }

    /// <summary>
    /// Transforms the arguments of the expression using the given transformer.
    /// </summary>
    /// <param name="transformer">Expression Transformer.</param>
    /// <returns></returns>
    public abstract ISparqlExpression Transform(IExpressionTransformer transformer);
}

/// <summary>
/// Abstract base class for Binary Expressions.
/// </summary>
public abstract class BaseBinaryExpression
    : ISparqlExpression
{
    /// <summary>
    /// The sub-expressions of this Expression.
    /// </summary>
    protected ISparqlExpression _leftExpr, _rightExpr;

    /// <summary>
    /// Creates a new Base Binary Expression.
    /// </summary>
    /// <param name="leftExpr">Left Expression.</param>
    /// <param name="rightExpr">Right Expression.</param>
    protected BaseBinaryExpression(ISparqlExpression leftExpr, ISparqlExpression rightExpr)
    {
        _leftExpr = leftExpr;
        _rightExpr = rightExpr;
    }

    /// <summary>
    /// Get the sub-expression on the left-hand side of this expression.
    /// </summary>
    public ISparqlExpression LeftExpression { get => _leftExpr; }

    /// <summary>
    /// Get the sub-expression on the right-hand side of this expression.
    /// </summary>
    public ISparqlExpression RightExpression { get => _rightExpr; }

    
    /// <summary>
    /// Gets the String representation of the Expression.
    /// </summary>
    /// <returns></returns>
    public abstract override string ToString();

    /// <inheritdoc />
    public abstract TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding);

    /// <inheritdoc />
    public abstract T Accept<T>(ISparqlExpressionVisitor<T> visitor);

    /// <summary>
    /// Gets an enumeration of all the Variables used in this expression.
    /// </summary>
    public virtual IEnumerable<string> Variables
    {
        get
        {
            return _leftExpr.Variables.Concat(_rightExpr.Variables);
        }
    }

    /// <summary>
    /// Gets the Type of the Expression.
    /// </summary>
    public abstract SparqlExpressionType Type
    {
        get;
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
            return new ISparqlExpression[] { _leftExpr, _rightExpr };
        }
    }

    /// <summary>
    /// Gets whether an expression can safely be evaluated in parallel.
    /// </summary>
    public virtual bool CanParallelise
    {
        get
        {
            return _leftExpr.CanParallelise && _rightExpr.CanParallelise;
        }
    }

    /// <summary>
    /// Transforms the arguments of the expression using the given transformer.
    /// </summary>
    /// <param name="transformer">Expression Transformer.</param>
    /// <returns></returns>
    public abstract ISparqlExpression Transform(IExpressionTransformer transformer);
}