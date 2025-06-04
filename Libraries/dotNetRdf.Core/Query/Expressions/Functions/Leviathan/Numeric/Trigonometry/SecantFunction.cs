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
/// Represents the Leviathan lfn:sec() or lfn:sec-1 function.
/// </summary>
public class SecantFunction
    : BaseTrigonometricFunction
{
    private static Func<double, double> _secant = (d => (1 / Math.Cos(d)));
    private static Func<double, double> _arcsecant = (d => Math.Acos(1 / d));

    /// <summary>
    /// Creates a new Leviathan Secant Function.
    /// </summary>
    /// <param name="expr">Expression.</param>
    public SecantFunction(ISparqlExpression expr)
        : base(expr, _secant) { }

    /// <summary>
    /// Creates a new Leviathan Secant Function.
    /// </summary>
    /// <param name="expr">Expression.</param>
    /// <param name="inverse">Whether this should be the inverse function.</param>
    public SecantFunction(ISparqlExpression expr, bool inverse)
        : base(expr)
    {
        Inverse = inverse;
        if (Inverse)
        {
            _func = _arcsecant;
        }
        else
        {
            _func = _secant;
        }
    }

    /// <summary>
    /// Get whether this is the inverse function.
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
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigSecInv + ">(" + InnerExpression + ")";
        }
        else
        {
            return "<" + LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigSec + ">(" + InnerExpression + ")";
        }
    }

    /// <inheritdoc />
    public override TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
    {
        return processor.ProcessSecantFunction(this, context, binding);
    }

    /// <inheritdoc />
    public override T Accept<T>(ISparqlExpressionVisitor<T> visitor)
    {
        return visitor.VisitSecantFunction(this);
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
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigSecInv;
            }
            else
            {
                return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.TrigSec;
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
        return new SecantFunction(transformer.Transform(InnerExpression), Inverse);
    }
}
