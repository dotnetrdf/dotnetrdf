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

using System;

namespace VDS.RDF.Query.Expressions.Functions.Leviathan.Numeric.Trigonometry;

/// <summary>
/// Represents the Leviathan lfn:cos() or lfn:cos-1 function.
/// </summary>
public class CosineFunction
    : BaseTrigonometricFunction
{
    /// <summary>
    /// Creates a new Leviathan Cosine Function.
    /// </summary>
    /// <param name="expr">Expression.</param>
    public CosineFunction(ISparqlExpression expr)
        : base(expr, Math.Cos) { }

    /// <summary>
    /// Creates a new Leviathan Cosine Function.
    /// </summary>
    /// <param name="expr">Expression.</param>
    /// <param name="inverse">Whether this should be the inverse function.</param>
    public CosineFunction(ISparqlExpression expr, bool inverse)
        : base(expr)
    {
        Inverse = inverse;
        if (Inverse)
        {
            _func = Math.Acos;
        }
        else
        {
            _func = Math.Cos;
        }
    }

    /// <summary>
    /// Gets whether this is the inverse function.
    /// </summary>
    public bool Inverse { get; }

    /// <summary>
    /// Gets the String representation of the function.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (Inverse)
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCosInv + ">(" + InnerExpression + ")";
        }
        else
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCos + ">(" + InnerExpression + ")";
        }
    }

    /// <inheritdoc />
    public override TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
    {
        return processor.ProcessCosineFunction(this, context, binding);
    }

    /// <inheritdoc />
    public override T Accept<T>(ISparqlExpressionVisitor<T> visitor)
    {
        return visitor.VisitCosineFunction(this);
    }

    /// <summary>
    /// Gets the Functor of the Expression.
    /// </summary>
    public override string Functor
    {
        get
        {
            if (Inverse)
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCosInv;
            }
            else
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCos;
            }
        }
    }

    /// <summary>
    /// Transforms the Expression using the given Transformer.
    /// </summary>
    /// <param name="transformer">Expression Transformer.</param>
    /// <returns></returns>
    public override ISparqlExpression Transform(IExpressionTransformer transformer)
    {
        return new CosineFunction(transformer.Transform(InnerExpression), Inverse);
    }
}
