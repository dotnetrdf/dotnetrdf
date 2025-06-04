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

namespace VDS.RDF.Query.Expressions.Functions.Arq;

/// <summary>
/// Represents the ARQ afn:substring() function which is a sub-string with Java semantics.
/// </summary>
public class SubstringFunction 
    : ISparqlExpression
{
    /// <summary>
    /// Get the expression which gives the string to be sliced.
    /// </summary>
    public ISparqlExpression StringExpression { get; }

    /// <summary>
    /// Get the expression which gives the start index of the substring to return.
    /// </summary>
    public ISparqlExpression StartExpression { get; }

    /// <summary>
    /// Get the expression which gives the end index of the substring to return.
    /// </summary>
    public ISparqlExpression EndExpression { get; }

    /// <summary>
    /// Creates a new ARQ substring function.
    /// </summary>
    /// <param name="stringExpr">Expression.</param>
    /// <param name="startExpr">Expression giving an index at which to start the substring.</param>
    public SubstringFunction(ISparqlExpression stringExpr, ISparqlExpression startExpr)
        : this(stringExpr, startExpr, null) { }

    /// <summary>
    /// Creates a new ARQ substring function.
    /// </summary>
    /// <param name="stringExpr">Expression.</param>
    /// <param name="startExpr">Expression giving an index at which to start the substring.</param>
    /// <param name="endExpr">Expression giving an index at which to end the substring.</param>
    public SubstringFunction(ISparqlExpression stringExpr, ISparqlExpression startExpr, ISparqlExpression endExpr)
    {
        StringExpression = stringExpr;
        StartExpression = startExpr;
        EndExpression = endExpr;
    }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
    {
        return processor.ProcessSubstringFunction(this, context, binding);
    }

    /// <inheritdoc />
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
            if (EndExpression != null)
            {
                return StringExpression.Variables.Concat(StartExpression.Variables).Concat(EndExpression.Variables);
            }
            else
            {
                return StringExpression.Variables.Concat(StartExpression.Variables);
            }
        }
    }

    /// <summary>
    /// Gets the String representation of the function.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (EndExpression != null)
        {
            return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Substring + ">(" + StringExpression + "," + StartExpression + "," + EndExpression + ")";
        }
        else
        {
            return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Substring + ">(" + StringExpression + "," + StartExpression + ")";
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
            return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Substring;
        }
    }

    /// <summary>
    /// Gets the Arguments of the Expression.
    /// </summary>
    public IEnumerable<ISparqlExpression> Arguments
    {
        get
        {
            if (EndExpression != null)
            {
                return new ISparqlExpression[] { StringExpression, StartExpression, EndExpression };
            }
            else
            {
                return new ISparqlExpression[] { EndExpression, StartExpression };
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
            return StringExpression.CanParallelise && StartExpression.CanParallelise && (EndExpression == null || EndExpression.CanParallelise);
        }
    }

    /// <summary>
    /// Transforms the Expression using the given Transformer.
    /// </summary>
    /// <param name="transformer">Expression Transformer.</param>
    /// <returns></returns>
    public ISparqlExpression Transform(IExpressionTransformer transformer)
    {
        if (EndExpression != null)
        {
            return new SubstringFunction(transformer.Transform(StringExpression), transformer.Transform(StartExpression), transformer.Transform(EndExpression));
        }
        else
        {
            return new SubstringFunction(transformer.Transform(EndExpression), transformer.Transform(StartExpression));
        }
    }
}
