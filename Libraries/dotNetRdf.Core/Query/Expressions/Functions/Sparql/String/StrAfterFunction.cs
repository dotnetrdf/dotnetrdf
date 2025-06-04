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

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String;

/// <summary>
/// Represents the SPARQL STRAFTER Function.
/// </summary>
public class StrAfterFunction
: ISparqlExpression
{
    /// <summary>
    /// Creates a new STRAFTER Function.
    /// </summary>
    /// <param name="stringExpr">String Expression.</param>
    /// <param name="startsExpr">Starts Expression.</param>
    public StrAfterFunction(ISparqlExpression stringExpr, ISparqlExpression startsExpr)
    {
        StringExpression = stringExpr;
        StartsExpression = startsExpr;
    }

    /// <summary>
    /// Get the expression that evaluates to the string to be processed.
    /// </summary>
    public ISparqlExpression StringExpression { get; }

    /// <summary>
    /// Get the expression that evaluates to the starting substring to locate.
    /// </summary>
    public ISparqlExpression StartsExpression { get; }


    /// <inheritdoc />
    public TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
    {
        return processor.ProcessStrAfterFunction(this, context, binding);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlExpressionVisitor<T> visitor)
    {
        return visitor.VisitStrAfterFunction(this);
    }

    /// <summary>
    /// Gets the Variables used in the function.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get
        {
            return StartsExpression.Variables.Concat(StringExpression.Variables);
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
            return SparqlSpecsHelper.SparqlKeywordStrAfter;
        }
    }

    /// <summary>
    /// Gets the Arguments of the Function.
    /// </summary>
    public IEnumerable<ISparqlExpression> Arguments
    {
        get
        {
            return new[] { StringExpression, StartsExpression };
        }
    }

    /// <summary>
    /// Gets whether an expression can safely be evaluated in parallel.
    /// </summary>
    public virtual bool CanParallelise
    {
        get
        {
            return StringExpression.CanParallelise && StartsExpression.CanParallelise;
        }
    }

    /// <summary>
    /// Transforms the Expression using the given Transformer.
    /// </summary>
    /// <param name="transformer">Expression Transformer.</param>
    /// <returns></returns>
    public ISparqlExpression Transform(IExpressionTransformer transformer)
    {
        return new StrAfterFunction(transformer.Transform(StringExpression), transformer.Transform(StartsExpression));
    }

    /// <summary>
    /// Gets the String representation of the function.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return SparqlSpecsHelper.SparqlKeywordStrAfter + "(" + StringExpression + ", " + StartsExpression + ")";
    }
}
