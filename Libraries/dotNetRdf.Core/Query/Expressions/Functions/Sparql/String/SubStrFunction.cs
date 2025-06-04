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
/// Represents the SPARQL SUBSTR Function.
/// </summary>
public class SubStrFunction
    : ISparqlExpression
{
    /// <summary>
    /// Creates a new XPath Substring function.
    /// </summary>
    /// <param name="stringExpr">Expression.</param>
    /// <param name="startExpr">Start.</param>
    public SubStrFunction(ISparqlExpression stringExpr, ISparqlExpression startExpr)
        : this(stringExpr, startExpr, null) { }

    /// <summary>
    /// Creates a new XPath Substring function.
    /// </summary>
    /// <param name="stringExpr">Expression.</param>
    /// <param name="startExpr">Start.</param>
    /// <param name="lengthExpr">Length.</param>
    public SubStrFunction(ISparqlExpression stringExpr, ISparqlExpression startExpr, ISparqlExpression lengthExpr)
    {
        StringExpression = stringExpr;
        StartExpression = startExpr;
        LengthExpression = lengthExpr;
    }

    /// <summary>
    /// Get the expression that evaluates to the string to be processed.
    /// </summary>
    public ISparqlExpression StringExpression { get; }

    /// <summary>
    /// Get the expression that evaluates to the start index of the substring to be returned.
    /// </summary>
    public ISparqlExpression StartExpression { get; }

    /// <summary>
    /// Get the expression that evaluates to the length of the substring to be returned.
    /// </summary>
    public ISparqlExpression LengthExpression { get; }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
    {
        return processor.ProcessSubStrFunction(this, context, binding);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlExpressionVisitor<T> visitor)
    {
        return visitor.VisitSubStrFunction(this);
    }

    /// <summary>
    /// Gets the Variables used in the function.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get
        {
            return LengthExpression != null ? StringExpression.Variables.Concat(StartExpression.Variables).Concat(LengthExpression.Variables) : StringExpression.Variables.Concat(StartExpression.Variables);
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
            return SparqlSpecsHelper.SparqlKeywordSubStr + "(" + StringExpression + "," + StartExpression + "," + LengthExpression + ")";
        }

        return SparqlSpecsHelper.SparqlKeywordSubStr + "(" + StringExpression + "," + StartExpression + ")";
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
            return SparqlSpecsHelper.SparqlKeywordSubStr;
        }
    }

    /// <summary>
    /// Gets the Arguments of the Function.
    /// </summary>
    public IEnumerable<ISparqlExpression> Arguments
    {
        get
        {
            return LengthExpression != null ? new[] { StringExpression, StartExpression, LengthExpression } : new[] { StringExpression, StartExpression };
        }
    }

    /// <summary>
    /// Gets whether an expression can safely be evaluated in parallel.
    /// </summary>
    public virtual bool CanParallelise
    {
        get
        {
            return StringExpression.CanParallelise && StartExpression.CanParallelise && (LengthExpression == null || LengthExpression.CanParallelise);
        }
    }

    /// <summary>
    /// Transforms the Expression using the given Transformer.
    /// </summary>
    /// <param name="transformer">Expression Transformer.</param>
    /// <returns></returns>
    public ISparqlExpression Transform(IExpressionTransformer transformer)
    {
        return LengthExpression != null 
            ? new SubStrFunction(transformer.Transform(StringExpression), transformer.Transform(StartExpression), transformer.Transform(LengthExpression)) 
            : new SubStrFunction(transformer.Transform(StringExpression), transformer.Transform(StartExpression));
    }
}
