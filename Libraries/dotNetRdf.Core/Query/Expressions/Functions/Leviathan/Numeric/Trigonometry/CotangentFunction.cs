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
/// Represents the Leviathan lfn:cot() or lfn:cot-1 function.
/// </summary>
public class CotangentFunction
    : BaseTrigonometricFunction
{
    private static readonly Func<double, double> _cotangent = (d => (Math.Cos(d) / Math.Sin(d)));
    private static readonly Func<double, double> _arccotangent = (d => Math.Atan(1 / d));

    /// <summary>
    /// Creates a new Leviathan Cotangent Function.
    /// </summary>
    /// <param name="expr">Expression.</param>
    public CotangentFunction(ISparqlExpression expr)
        : base(expr, _cotangent) { }

    /// <summary>
    /// Creates a new Leviathan Cotangent Function.
    /// </summary>
    /// <param name="expr">Expression.</param>
    /// <param name="inverse">Whether this should be the inverse function.</param>
    public CotangentFunction(ISparqlExpression expr, bool inverse)
        : base(expr)
    {
        Inverse = inverse;
        _func = Inverse ? _arccotangent : _cotangent;
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
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCotanInv + ">(" + InnerExpression + ")";
        }
        else
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCotan + ">(" + InnerExpression + ")";
        }
    }

    /// <inheritdoc />
    public override TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
    {
        return processor.ProcessCotangentFunction(this, context, binding);
    }

    /// <inheritdoc />
    public override T Accept<T>(ISparqlExpressionVisitor<T> visitor)
    {
        return visitor.VisitCotangentFunction(this);
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
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCotanInv;
            }
            else
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigCotan;
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
        return new CotangentFunction(transformer.Transform(InnerExpression), Inverse);
    }
}
